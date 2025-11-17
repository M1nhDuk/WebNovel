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
    refreshUserProfile: () => Promise<void>;

    unreadCount: number;
    refreshUnreadCount: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

const GATEWAY_URL = 'https://localhost:8000';


const getImageUrl = (imagePath: string | null | undefined, type: 'avatar' | 'background') => {
    if (!imagePath) {
        return type === 'avatar'
            ? `${GATEWAY_URL}/uploads/default_avatar_thumb.png`
            : `${GATEWAY_URL}/uploads/default_background.png`;
    }


    if (imagePath.startsWith('http')) {
        try {
            const url = new URL(imagePath);
            if (url.port === '7154') {
                return `${GATEWAY_URL}${url.pathname}`;
            }
        } catch (e) { }
        return imagePath;
    }

    const formattedPath = imagePath.startsWith('/') ? imagePath : `/${imagePath}`;
    return `${GATEWAY_URL}${formattedPath}`;
};


const formatUserProfile = (user: UserProfile): UserProfile => ({
    ...user,
    avatar: getImageUrl(user.avatar, 'avatar'),
    avatarThumbnail: getImageUrl(user.avatarThumbnail, 'avatar'),
    backgroundImage: getImageUrl(user.backgroundImage, 'background'),
});


// Component Provider
export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<UserProfile | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [unreadCount, setUnreadCount] = useState(0);

    const refreshUnreadCount = useCallback(async () => {
        const token = localStorage.getItem('accessToken');
        if (!token) {
            setUnreadCount(0);
            return;
        }

        try {
            const response = await apiClient.get<number>(API_ROUTES.USER.GET_UNREAD_COUNT);
            setUnreadCount(response.data);
        } catch (error) {
            if ((error as any).response?.status !== 401) {
                console.error("Failed to fetch unread count:", error);
            }
            setUnreadCount(0);
        }
    }, []); 

    const refreshUserProfile = useCallback(async () => {
        const token = localStorage.getItem('accessToken');
        if (!token) {
            setIsLoading(false);
            setUnreadCount(0);
            return;
        }
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        try {
            const response = await apiClient.get<UserProfile>(API_ROUTES.AUTH.GET_MY_PROFILE);
            setUser(formatUserProfile(response.data));
            await refreshUnreadCount();
        } catch (error) {
            console.error("Failed to fetch user:", error);
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            setUnreadCount(0);
        } finally {
            setIsLoading(false);
        }
    }, [refreshUnreadCount]);


    useEffect(() => {
        refreshUserProfile();
    }, [refreshUserProfile]);

    useEffect(() => {
        if (user) {
            const intervalId = setInterval(() => {
                console.log("Polling for new notifications..."); 
                refreshUnreadCount();
            }, 60000); 

  
            return () => {
                clearInterval(intervalId); 
            };
        }
    }, [user, refreshUnreadCount]);



    // Hàm Login
    const login = async (credentials: LoginDto) => {
        const response = await apiClient.post(API_ROUTES.AUTH.LOGIN, credentials);
        const { accessToken, refreshToken } = response.data;
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
        await refreshUserProfile();
    };

    // Hàm Logout
    const logout = () => {
        apiClient.post(API_ROUTES.AUTH.LOGOUT);
        setUser(null);
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        delete apiClient.defaults.headers.common['Authorization'];
        setUnreadCount(0);
    };

    return (
        <AuthContext.Provider value={{
            user,
            isLoading,
            login,
            logout,
            refreshUserProfile,
            unreadCount,
            refreshUnreadCount
        }}>
            {children}
        </AuthContext.Provider>
    );
};