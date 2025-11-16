export interface UserFavoriteDto {
    seriesId: number;
    addedAt: string;
    lastKnowChapter: number;

    seriesTitle?: string | null;
    seriesCoverImage?: string | null;
}

export interface AddFavoriteDto {
    seriesId: number;
    currentChapterCount: number;
}

export interface FavoriteToggleResult {
    message: string;
    isFavorited: boolean;
    data?: UserFavoriteDto;
}