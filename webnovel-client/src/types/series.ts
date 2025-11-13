export interface SeriesListDto {
    series_Id: number;
    series_title: string;
    cover_images?: string;
    category_id: number;
    categoryName?: string;
    status_id: number;
    statusName?: string;
    tags: string[];
    type: string;
}

export interface ChapterDetailDto {
    chapter_id: number;
    title: string;
    created_at: string;
}


export interface NovelDetailDto {
    series_Id: number | null;
    novel_Id: number;
    title: string; 
    author: string;
    artist: string | null;
    cover_images: string | null;
    updated_at: string;
    novel_number: number;
    uploader_id: string;
    uploader_name: string;
    uploader_avatar: string | null;
    chapters: ChapterDetailDto[]; 
}


export interface NovelSeriesDetailDto {
    series_Id: number;
    series_title: string;
    author: string | null;
    artist: string | null;
    description: string;
    cover_images: string | null;
    word_count: number;
    views: number;
    note: string | null;
    created_at: string;
    updated_at: string;
    type: string;
    uploader_id: string;
    uploader_name: string;
    uploader_avatar: string | null;
    category_id: number;
    categoryName: string | null;
    status_id: number;
    statusName: string | null;
    tags: string[];
    novels: NovelDetailDto[]; 
}

export interface UserProfile {
    userId: string;
    username: string;
    avatar: string | null;
    avatarThumbnail: string | null;
    backgroundImage: string | null;
    role: string;
}

export interface CreateSeriesDto {
    series_title: string;
    artist?: string | null;
    author?: string | null;
    description: string;
    note?: string | null;
    status_id: number;
    category_id: number | null;
    TagIds?: number[] | null;
}

export interface PagedResult<T> {
    items: T[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}