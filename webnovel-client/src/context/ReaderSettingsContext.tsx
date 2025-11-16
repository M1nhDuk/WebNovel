import React, { createContext, useState, useEffect, useCallback, useMemo } from 'react';
import apiClient from '../api/apiClient';
import { useAuth } from '../hooks/useAuth';
import { useDebounce } from '../hooks/useDebounce';
import type { UserSettingDto, UpdateUserSettingDto } from '../types/series';
import { API_ROUTES } from '../api/apiRoutes';

const defaultSettings: UserSettingDto = {
    fontFamily: "Times New Roman",
    fontSize: 20,
    backgroundColor: "#FFFFFF",
    fontColor: "#000000",
    alignment: "left",
    paddingPx: 0
};

interface ReaderSettingsContextType {
    settings: UserSettingDto;
    updateSetting: (key: keyof UpdateUserSettingDto, value: any) => void;
    isLoading: boolean;
}

export const ReaderSettingsContext = createContext<ReaderSettingsContextType | undefined>(undefined);

export const ReaderSettingsProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const { user } = useAuth();
    const [settings, setSettings] = useState<UserSettingDto>(defaultSettings);
    const [isLoading, setIsLoading] = useState(true);

    const debouncedSettings = useDebounce(settings, 1000); 

    // [LOGIC TẢI CÀI ĐẶT]
    useEffect(() => {
        const fetchSettings = async () => {
            setIsLoading(true);
            if (user) {

                // 1. User đã đăng nhập: Tải từ API
                try {
                    const response = await apiClient.get<UserSettingDto>('/api/user/settings');
                    setSettings(response.data);
                } catch (err) {
                    console.error("Failed to fetch user settings:", err);
                    setSettings(defaultSettings);
                } finally {
                    setIsLoading(false);
                }
            } else {

                // 2. User là khách: Tải từ localStorage
                try {
                    const localSettings = localStorage.getItem('readerSettings');
                    if (localSettings) {
                        setSettings(JSON.parse(localSettings));
                    } else {
                        setSettings(defaultSettings);
                    }
                } catch (err) {
                    console.error("Failed to parse local settings:", err);
                    setSettings(defaultSettings);
                } finally {
                    setIsLoading(false);
                }
            }
        };
        fetchSettings();
    }, [user]); 

    // [LOGIC LƯU CÀI ĐẶT]
    useEffect(() => {
        if (isLoading) return;

        if (user) {

            // 1. User đã đăng nhập: Lưu vào API
            const updateRemoteSettings = async () => {
                const payload: UpdateUserSettingDto = { ...debouncedSettings };
                try {
                    await apiClient.put(API_ROUTES.USER.SETTINGS, payload);
                } catch (err) {
                    console.error("Failed to save user settings:", err);
                }
            };
            updateRemoteSettings();
        } else {

            // 2. User là khách: Lưu vào localStorage
            try {
                localStorage.setItem('readerSettings', JSON.stringify(debouncedSettings));
            } catch (err) {
                console.error("Failed to save local settings:", err);
            }
        }
    }, [debouncedSettings, user, isLoading]);

    // Hàm cập nhật state (cho UI)
    const updateSetting = useCallback((key: keyof UpdateUserSettingDto, value: any) => {
        setSettings(prev => ({
            ...prev,
            [key]: value
        }));
    }, []);

    const value = useMemo(() => ({
        settings,
        updateSetting,
        isLoading
    }), [settings, updateSetting, isLoading]);

    return (
        <ReaderSettingsContext.Provider value={value}>
            {children}
        </ReaderSettingsContext.Provider>
    );
};