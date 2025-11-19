import axios from 'axios';
import { API_ROUTES } from './apiRoutes';

const API_GATEWAY_URL = 'https://localhost:8000';

const apiClient = axios.create({
    baseURL: API_GATEWAY_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// --- Helper: Decoded JWT to take User Id ---
const getUserIdFromToken = (token: string): string | null => {
    try {
        const base64Url = token.split('.')[1];
        if (!base64Url) return null;

        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        const payload = JSON.parse(jsonPayload);

        return payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
            || payload["nameid"]
            || payload["sub"]
            || null;
    } catch (e) {
        console.error("Error parsing token:", e);
        return null;
    }
};

apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// --- Response Interceptor: Refresh Token ---
let isRefreshing = false;
let failedQueue: any[] = [];

const processQueue = (error: any, token: string | null = null) => {
    failedQueue.forEach(prom => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });
    failedQueue = [];
};

apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {

            // N?u ?ang có m?t ti?n trình refresh token khác ?ang ch?y
            if (isRefreshing) {
                return new Promise(function (resolve, reject) {
                    failedQueue.push({ resolve, reject });
                }).then(token => {
                    originalRequest.headers['Authorization'] = 'Bearer ' + token;
                    return apiClient(originalRequest);
                }).catch(err => {
                    return Promise.reject(err);
                });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            const refreshToken = localStorage.getItem('refreshToken');
            const accessToken = localStorage.getItem('accessToken');

            //logout if does not have refresh or access token 
            if (!refreshToken || !accessToken) {
                isRefreshing = false;
                window.location.href = '/login';
                return Promise.reject(error);
            }

          
            const userId = getUserIdFromToken(accessToken);

            if (!userId) {
                isRefreshing = false;
                localStorage.clear();
                window.location.href = '/login';
                return Promise.reject(new Error("Invalid access token structure"));
            }

            try {
                const response = await axios.post(`${API_GATEWAY_URL}${API_ROUTES.AUTH.REFRESH_TOKEN}`, {
                    userId: userId,
                    refreshToken: refreshToken
                });

                const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;

                //Saven new token
                localStorage.setItem('accessToken', newAccessToken);
                localStorage.setItem('refreshToken', newRefreshToken);

                //Update header for new request
                apiClient.defaults.headers.common['Authorization'] = `Bearer ${newAccessToken}`;
                originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;

                //Resolve refresh token in the queu
                processQueue(null, newAccessToken);
                isRefreshing = false;

                return apiClient(originalRequest);

            } catch (refreshError) {
                processQueue(refreshError, null);
                isRefreshing = false;

                
                console.error("Session expired. Please login again.");
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                window.location.href = '/login';

                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

export default apiClient;