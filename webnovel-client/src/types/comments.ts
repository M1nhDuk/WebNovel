export interface CommentDto {
    commentId: string; 
    userId: string;    
    content: string;
    createdAt: string; 
    userName?: string;
    userAvatarThumbnail?: string | null; 

    replyCount: number;
}

export interface CommentSectionProps {
    seriesId?: number;
    chapterId?: number;
    totalCommentCount: number;
}
