import { createContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import apiClient from '../api/apiClient';
import { API_ROUTES } from '../api/apiRoutes';
import type { UserProfile } from '../types/auth';
import type { LoginDto } from '../types/login';
import type { UnreadSummaryDto } from '../types/notifications';
import { NotificationType } from '../types/notifications';

interface AuthContextType {
    user: UserProfile | null;
    isLoading: boolean;
    login: (credentials: LoginDto) => Promise<void>;
    logout: () => void;
    refreshUserProfile: () => Promise<void>;
    unreadGeneralCount: number;
    unreadChapterCount: number;
    refreshUnreadCount: () => Promise<void>;

    // Hàm mới để reset thông báo chương mới
    clearChapterNotifications: () => void;
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

    // --- KHAI BÁO STATE MỚI  ---
    const [unreadGeneralCount, setUnreadGeneralCount] = useState(0);
    const [unreadChapterCount, setUnreadChapterCount] = useState(0);

    // Hàm lấy số lượng thông báo từ server
    const refreshUnreadCount = useCallback(async () => {
        const token = localStorage.getItem('accessToken');
        if (!token) {
            setUnreadGeneralCount(0);
            setUnreadChapterCount(0);
            return;
        }

        try {
            // Gọi API
            const response = await apiClient.get<UnreadSummaryDto>('/api/user/notifications/unread-summary');
            setUnreadGeneralCount(response.data.generalCount);
            setUnreadChapterCount(response.data.chapterCount);
        } catch (error) {
            if ((error as any).response?.status !== 401) {
                console.error("Failed to fetch unread count:", error);
            }
            setUnreadGeneralCount(0);
            setUnreadChapterCount(0);
        }
    }, []);

    const clearChapterNotifications = useCallback(async () => {
        setUnreadChapterCount(0);

        try {
            await apiClient.post(
                API_ROUTES.USER.MARK_ALL_AS_READ_BY_TYPE(NotificationType.NewChapter));
        } catch (error) {
            console.error("Failed to mark chapters as read", error);
        }
    }, []);

    const refreshUserProfile = useCallback(async () => {
        const token = localStorage.getItem('accessToken');
        if (!token) {
            setIsLoading(false);
            setUnreadGeneralCount(0);
            setUnreadChapterCount(0);
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
            setUnreadGeneralCount(0);
            setUnreadChapterCount(0);
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

        setUnreadGeneralCount(0);
        setUnreadChapterCount(0);
    };

    return (
        <AuthContext.Provider value={{
            user,
            isLoading,
            login,
            logout,
            refreshUserProfile,
            unreadGeneralCount,      
            unreadChapterCount,     
            refreshUnreadCount,
            clearChapterNotifications
        }}>
            {children}
        </AuthContext.Provider>
    );
};