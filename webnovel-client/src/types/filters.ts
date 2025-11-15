export interface CategoryDto {
    category_id: number;
    category_name: string;
}

export interface NovelStatusDto {
    statusId: number;
    statusName: string;
}

export interface TagDto {
    tagId: number;
    tagName: string;
    description?: string | null;
}