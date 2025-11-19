export interface NotificationDto {
    notificationId: string;
    type: string;
    message: string;
    linkUrl: string | null;
    createdAt: string;
    isRead: boolean;
}

export enum NotificationType {
    SeriesUpdate = 0,
    SeriesDeleted = 1,
    NewComment = 2,
    NewChapter = 3 
}


export interface RemoveNotificationsDto {
    notificationIds: string[];
}

export interface UnreadSummaryDto {
    generalCount: number;
    chapterCount: number;
}