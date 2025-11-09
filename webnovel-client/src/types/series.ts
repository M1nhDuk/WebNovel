
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

//import type { TagDto } from './filters';

export interface ChapterSummaryDto {
    chapter_id: number;
    title: string;
    created_at: string; 
}

export interface NovelSummary {
    novel_id: number;
    novel_title: string;
    chapters: ChapterSummaryDto[];
    cover_images: string;
}


export interface NovelSeriesDetailDto {
    series_Id: number;
    series_title: string;
    author: string;
    artist: string;
    description: string;
    cover_images: string;
    statusName: string;
    categoryName: string;
    type: string; 
    tags: string[];
    novels: NovelSummary[];
    word_count: number;
    updated_at: string;
    views: number;
    uploader_name: string;
    uploader_avatar: string;
    note: string;
}

export interface PagedResult<T> {
    items: T[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}

