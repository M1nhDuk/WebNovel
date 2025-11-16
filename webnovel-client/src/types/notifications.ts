export interface NotificationDto {
    notificationId: string;
    type: string;
    message: string;
    linkUrl: string | null;
    createdAt: string;
    isRead: boolean;
}

export interface RemoveNotificationsDto {
    notificationIds: string[];
}