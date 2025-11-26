
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

        GET_PUBLIC_PROFILE: (id: string) => `/user/${id}/public`,
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

        GET_SERIES_BY_UPLOADER: (uploaderId: string) => `/series/uploader/${uploaderId}`,

    },

    USER: { 
        READING_HISTORY: '/user/reading-history',
        SETTINGS: '/api/user/settings',
        GET_FAVORITES: '/user/favorites',
        TOGGLE_FAVORITE: '/user/favorites/toggle',
        SYNC_COUNTS: '/user/favorites/sync-counts',

        GET_BOOKMARKS: '/user/bookmarks',
        GET_BOOKMARK_FOR_CHAPTER: (chapterId: number | string) => `/user/bookmarks/chapter/${chapterId}`,
        TOGGLE_BOOKMARK: '/user/bookmarks/toggle',
        DELETE_BOOKMARK_FOR_CHAPTER: (chapterId: number | string) => `/user/bookmarks/chapter/${chapterId}`,
        DELETE_BOOKMARK: (bookmarkId: string) => `/user/bookmarks/${bookmarkId}`,
        GET_BOOKMARKS_FOR_SERIES: (seriesId: number | string) => `/user/bookmarks/series/${seriesId}`,


        GET_NOTIFICATIONS: '/user/notifications',
        GET_UNREAD_COUNT: '/user/notifications/unread-count',
        MARK_ALL_AS_READ: '/user/notifications/mark-all-read',
        MARK_AS_READ: (id: string) => `/user/notifications/${id}/mark-as-read`,
        MARK_ALL_AS_READ_BY_TYPE: (type: number | string) => `/api/user/notifications/mark-all-read/type/${type}`,
        DELETE_NOTIFICATIONS: '/user/notifications/batch-delete',

        GET_READ_CHAPTERS: (seriesId: string | number) => `/api/user/reading-history/read-chapters/${seriesId}`,


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