
export interface SeriesListDto {
    series_Id: number;
    series_title: string;
    cover_images?: string; 
    category_id: number;
    categoryName?: string;
    status_id: number;
    statusName?: string;
    tags: string[];
}

export interface PagedResult<T> {
    items: T[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}