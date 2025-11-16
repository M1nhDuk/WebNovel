
import { API_ROUTES } from './apiRoutes';

export type ViewType = 'categories' | 'tags' | 'statuses';

type MetaPanelApiConfig = {
    [key in ViewType]: {
        GET: string;
        CREATE: string;
        UPDATE: (id: number | string) => string;
        DELETE: (id: number | string) => string;
        dtoKey: string;
        hasDescription: boolean;
    };
};

export const metaPanelApiConfig: MetaPanelApiConfig = {
    categories: {
        GET: API_ROUTES.ADMIN.GET_CATEGORIES,
        CREATE: API_ROUTES.ADMIN.CREATE_CATEGORY,
        UPDATE: API_ROUTES.ADMIN.UPDATE_CATEGORY,
        DELETE: API_ROUTES.ADMIN.DELETE_CATEGORY,
        dtoKey: 'category_name',
        hasDescription: false,
    },
    tags: {
        GET: API_ROUTES.ADMIN.GET_TAGS,
        CREATE: API_ROUTES.ADMIN.CREATE_TAG,
        UPDATE: API_ROUTES.ADMIN.UPDATE_TAG,
        DELETE: API_ROUTES.ADMIN.DELETE_TAG,
        dtoKey: 'tagName',
        hasDescription: true,
    },
    statuses: {
        GET: API_ROUTES.ADMIN.GET_STATUSES,
        CREATE: API_ROUTES.ADMIN.CREATE_STATUS,
        UPDATE: API_ROUTES.ADMIN.UPDATE_STATUS,
        DELETE: API_ROUTES.ADMIN.DELETE_STATUS,
        dtoKey: 'statusName',
        hasDescription: false,
    },
};