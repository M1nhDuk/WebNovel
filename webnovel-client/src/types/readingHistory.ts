export interface ReadingHistoryDto {
    historyId: string; 
    seriesId: number;
    lastAccessedAt: string; 
    seriesTitle: string | null;
    seriesCoverImage: string | null;
}