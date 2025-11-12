import { createContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import apiClient from '../api/apiClient';
import { API_ROUTES } from '../api/apiRoutes';
import type { UserProfile } from '../types/auth';
import type { LoginDto } from '../types/login';

interface AuthContextType {
    user: UserProfile | null;
    isLoading: boolean;
    login: (credentials: LoginDto) => Promise<void>;
    logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

const GATEWAY_URL = 'https://localhost:8000';

// Hàm helper ?? chu?n hóa URL hình ?nh
const getImageUrl = (imagePath: string | null | undefined) => {
    if (!imagePath) {
        return `${GATEWAY_URL}/images/default_avatar_thumb.png`;
    }
    if (imagePath.startsWith('http')) {
        return imagePath; 
    }
    const formattedPath = imagePath.startsWith('/') ? imagePath : `/${imagePath}`;
    return `${GATEWAY_URL}${formattedPath}`; 
};

// Helper ?? ??nh d?ng ??i t??ng user v?i URL ??y ??
const formatUserProfile = (user: UserProfile): UserProfile => ({
    ...user,
    avatar: getImageUrl(user.avatar),
    avatarThumbnail: getImageUrl(user.avatarThumbnail),
    backgroundImage: getImageUrl(user.backgroundImage),
});

// Component Provider
export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<UserProfile | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    // G?i API /user/me ?? l?y data
    const fetchUser = useCallback(async () => {
        const token = localStorage.getItem('accessToken');
        if (!token) {
            setIsLoading(false);
            return;
        }
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        try {
            const response = await apiClient.get<UserProfile>(API_ROUTES.AUTH.GET_MY_PROFILE);
            setUser(formatUserProfile(response.data)); 
        } catch (error) {
            console.error("Failed to fetch user:", error);
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
        } finally {
            setIsLoading(false);
        }
    }, []);


    useEffect(() => {
        fetchUser();
    }, [fetchUser]);

    // Hàm Login
    const login = async (credentials: LoginDto) => {
        const response = await apiClient.post(API_ROUTES.AUTH.LOGIN, credentials);
        const { accessToken, refreshToken } = response.data;
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
        await fetchUser(); 
    };

    // Hàm Logout
    const logout = () => {
        apiClient.post(API_ROUTES.AUTH.LOGOUT);
        setUser(null);
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        delete apiClient.defaults.headers.common['Authorization'];
    };

    return (
        <AuthContext.Provider value={{ user, isLoading, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};