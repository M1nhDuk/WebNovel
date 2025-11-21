export interface UserFavoriteDto {
    seriesId: number;
    addedAt: string;
    lastKnowChapter: number;

    seriesTitle?: string;
    seriesCoverImage?: string;

    currentChapterCount: number;

    unreadCount: number;
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

export interface FavoriteReadUpdateDto {
    seriesId: number;
    latestChapterCount: number; 
}