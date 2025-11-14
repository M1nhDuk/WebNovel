
export const GATEWAY_URL = 'https://localhost:8000';

export const API_ROUTES = {
    // --- AuthService Routes ---
    AUTH: {
        REGISTER: '/auth/register',
        CONFIRM_EMAIL: '/auth/confirm-email',
        LOGIN: '/auth/login',
        REFRESH_TOKEN: '/auth/refresh-token',
        FORGOT_PASSWORD: '/auth/forgot-password',
        LOGOUT: '/auth/logout',
        UPLOAD_AVATAR: '/user/avatar',
        UPLOAD_BACKGROUND: '/user/background',
        CHANGE_PASSWORD: '/user/change-password',
        CHANGE_USERNAME: '/user/change-username',
        GET_MY_PROFILE: '/user/me',
    },

    // --- NovelService Routes ---
    SERIES: {
        GET_ALL_SERIES: '/series',
        SEARCH_SERIES: '/series/search',
        CREATE_SERIES: '/series',

    
        GET_BY_ID: (id: number | string) => `/series/${id}`,
        UPDATE: (id: number | string) => `/series/${id}`,
        DELETE: (id: number | string) => `/series/${id}`,
        UPLOAD_COVER: (id: number | string) => `/series/${id}/cover`,

        GET_MY_SERIES: '/user/series',

        CREATE_CLASSIC_SERIES: '/series/classic',
        UPDATE_CLASSIC_SERIES: (id: number | string) => `/series/${id}/classic`,

        CREATE_NOVEL: (seriesId: number | string) => `/series/${seriesId}/novels`,
        UPLOAD_NOVEL_COVER: (seriesId: number | string, novelId: number | string) => `/series/${seriesId}/novels/${novelId}/cover`,
    },

    CATEGORY: {
        GET_ALL: '/categories',
    },
    STATUS: {
        GET_ALL: '/statuses',
    },
    TAG: {
        GET_ALL: '/tags',
    },

};