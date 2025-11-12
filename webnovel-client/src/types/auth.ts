export interface UserProfile {
    userId: string;
    username: string;
    avatar: string | null;
    avatarThumbnail: string | null;
    backgroundImage: string | null;
    role: string;
}

export interface AuthUser {
    username: string;
    avatar: string | null;
    background: string | null;
}