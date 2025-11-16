
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

        CREATE_CHAPTER_FOR_NOVEL: (novelId: number | string) => `/novels/${novelId}/chapters`,
        CREATE_CHAPTER_FOR_SERIES: (seriesId: number | string) => `/series/${seriesId}/chapters`,

        //Edit
        CHAPTER_FOR_NOVEL: (novelId: number | string, chapterId: number | string) => `/novels/${novelId}/chapters/${chapterId}`,
        CHAPTER_FOR_SERIES: (seriesId: number | string, chapterId: number | string) => `/series/${seriesId}/chapters/${chapterId}`,
    },

    USER: { 
        READING_HISTORY: '/user/reading-history',
    },

    COMMENTS: {
        UPDATE: (commentId: string) => `/comments/${commentId}`,
        DELETE: (commentId: string) => `/comments/${commentId}`,
    },

    ADMIN: {
        // Publication Metadata
        GET_CATEGORIES: '/api/admin/publication/categories',
        CREATE_CATEGORY: '/api/admin/publication/categories',
        UPDATE_CATEGORY: (id: number | string) => `/api/admin/publication/categories/${id}`,
        DELETE_CATEGORY: (id: number | string) => `/api/admin/publication/categories/${id}`,

        GET_TAGS: '/api/admin/publication/tags',
        CREATE_TAG: '/api/admin/publication/tags',
        UPDATE_TAG: (id: number | string) => `/api/admin/publication/tags/${id}`,
        DELETE_TAG: (id: number | string) => `/api/admin/publication/tags/${id}`,

        GET_STATUSES: '/api/admin/publication/statuses',
        CREATE_STATUS: '/api/admin/publication/statuses',
        UPDATE_STATUS: (id: number | string) => `/api/admin/publication/statuses/${id}`,
        DELETE_STATUS: (id: number | string) => `/api/admin/publication/statuses/${id}`,

        // Content Deletion
        DELETE_SERIES: (id: number | string) => `/api/admin/publication/series/${id}`,
        DELETE_NOVEL: (id: number | string) => `/api/admin/publication/novels/${id}`,
        DELETE_CHAPTER: (id: number | string) => `/api/admin/publication/chapters/${id}`,
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