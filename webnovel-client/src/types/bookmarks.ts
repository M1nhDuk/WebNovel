export interface BookmarkDto {
    bookmarkId: string;
    seriesId: number;
    chapterId: number;
    locationIdentifier: string;
    contextSnippet?: string | null;
    createdAt: string;


    seriesTitle?: string | null;
    seriesCoverImage?: string | null;
    chapterTitle?: string | null;
    chapterNumber: number;
}

export interface ToggleBookmarkDto {
    seriesId: number;
    chapterId: number;
    locationIdentifier: string;
    contextSnippet?: string | null;
}


export interface BookmarkToggleResultDto {
    isBookmarked: boolean;
    data: BookmarkDto | null;
}