export interface CommentDto {
    commentId: string; 
    userId: string;    
    content: string;
    createdAt: string; 
    userName?: string;
    userAvatarThumbnail?: string | null; 
    replyCount: number;
    replies: CommentDto[];
}

export interface CommentSectionProps {
    seriesId?: number;
    chapterId?: number;
}

export interface CommentItemProps {
    comment: CommentDto;
    onToggleReply: (commentId: string) => void;
    replyingToId: string | null;
    targetId: number;
    targetType: 'series' | 'chapters';
    onReplySuccess: () => void;
    getAvatarUrl: (path: string | null | undefined) => string;
    formatTimeAgo: (dateString: string) => string;
    isReply: boolean;
}


export interface CommentReplyInputProps {
    targetId: number; 
    targetType: 'series' | 'chapters';
    parentCommentId: string; 
    authorUsername: string; 
    onReplySuccess: () => void; 
    onCancel: () => void;
}

export interface UpdateCommentDto {
    content: string;
}


export interface ReplyState {
    items: CommentDto[];
    nextPage: number;
    isLoading: boolean;
    isFullyLoaded: boolean;
}

export interface CommentContentProps {
    content: string;
}