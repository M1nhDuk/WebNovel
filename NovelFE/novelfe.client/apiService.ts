import axios from 'axios';

// URL API Gateway
const API_BASE_URL = 'https://localhost:7000';

const apiService = axios.create({
    baseURL: API_BASE_URL,
});


apiService.interceptors.request.use(
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

export default apiService;