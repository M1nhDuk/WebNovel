export interface SeriesListDto {
    series_Id: number;
    series_title: string;
    cover_images?: string;
    category_id: number;
    categoryName?: string;
    status_id: number;
    statusName?: string;
    tags: string[];
    type: string;
}

export interface ChapterDetailDto {
    chapter_id: number;
    title: string;
    created_at: string;
    chapter_number: number;
    word_count: number;
}


export interface NovelDetailDto {
    series_Id: number | null;
    novel_Id: number;
    title: string; 
    author: string;
    artist: string | null;
    cover_images: string | null;
    updated_at: string;
    novel_number: number;
    uploader_id: string;
    uploader_name: string;
    uploader_avatar: string | null;
    chapters: ChapterDetailDto[]; 
}



export interface NovelSeriesDetailDto {
    series_Id: number;
    series_title: string;
    author: string | null;
    artist: string | null;
    description: string;
    cover_images: string | null;
    word_count: number;
    views: number;
    note: string | null;
    created_at: string;
    updated_at: string;
    type: 'Series' | 'TRADITIONAL';
    uploader_id: string;
    uploader_name: string;
    uploader_avatar: string | null;
    category_id: number;
    categoryName: string | null;
    status_id: number;
    statusName: string | null;
    tags: string[];
    novels: NovelDetailDto[]; 
    chapters?: ChapterDetailDto[];

    //Classic attri
    iSBN_10?: string | null; 
    iSBN_13?: string | null; 
    publisher?: string | null;
    publish_date?: string | null;
    edition?: string | null;
}

export interface UserProfile {
    userId: string;
    username: string;
    avatar: string | null;
    avatarThumbnail: string | null;
    backgroundImage: string | null;
    role: string;
}

export interface CreateSeriesDto {
    series_title: string;
    artist?: string | null;
    author?: string | null;
    description: string;
    note?: string | null;
    status_id: number;
    category_id: number | null;
    TagIds?: number[] | null;
}



export interface CreateTraditionalSeriesDto extends CreateSeriesDto {
    iSBN_10?: string | null; 
    iSBN_13: string; 
    publisher?: string | null;
    publish_date?: string | null; 
    edition?: string | null;
}


export interface UpdateNovelServiceDto {
    series_Id: number;
    series_title: string;
    artist: string | null;
    author: string | null;
    description: string;
    cover_images: string | null;
    note: string | null;
    category_id: number | null;
    status_id: number | null;
    TagIds: number[] | null;
}

export interface UpdateClassicSeriesDto extends UpdateNovelServiceDto {
    iSBN_10: string | null; 
    iSBN_13: string; 
    publisher: string | null;
    publish_date: string | null; 
    edition: string | null;
}

export interface AddNovelFormProps {
    seriesId: number;
    onNovelCreated: () => void;
}

export interface CreateNovelDto {
    series_Id: number;
    title: string;
    cover_images: string | null;
}


export interface NovelUpdateDto {
    title?: string | null;
    cover_images?: string | null;
}


export interface AddChapterFormProps {
    seriesId: number;
    novelId?: number; 
    seriesType: 'Series' | 'TRADITIONAL';
    onChapterCreated: (newChapter: ChapterDetailDto) => void;

}

export interface FullChapterDto extends ChapterDetailDto {
    content: string;
}

export interface EditChapterFormProps {
    seriesId: number;
    novelId?: number;
    chapterId: number;
    seriesType: 'Series' | 'TRADITIONAL';
    onChapterUpdated: () => void;
    onCancel: () => void;
}

export interface PagedResult<T> {
    items: T[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
}

export type EditingItem = {
    type: 'series' | 'novel' | 'chapter' | 'add-novel' | 'add-chapter' | 'reorder-novels' | 'reorder-chapters';
    id: number; // seriesId (cho reorder-novels) hoặc novelId (cho reorder-chapters) hoặc seriesId (cho reorder-chapters TRADITIONAL)
    parentId?: number; 
};

export interface SeriesContextMenuProps {
    visible: boolean; x: number; y: number; seriesType: 'Series' | 'TRADITIONAL';
    onEdit: () => void; onAddVolume: () => void; onReorder: () => void; onAddChapter: () => void
    onDeleteSeries: () => void;
}

export interface NovelContextMenuProps {
    visible: boolean; x: number; y: number;
    onEdit: () => void;
    onAddChapter: () => void;
    onReorderChapters: () => void;
    onDelete: () => void;
}

export interface ReorderableItem {
    id: number;
    title: string;
}

export interface ReorderableListProps {
    items: ReorderableItem[];
    listTitle: string;
    onSave: (orderedIds: number[]) => Promise<void>;
    onCancel: () => void;
}

export interface SeriesHierarchyProps {
    series: NovelSeriesDetailDto;
    setEditingItem: (item: EditingItem) => void;
    onRefresh: () => void;
    onDeleteSeries: () => void;
    onReorderTrigger: () => void;
    onEditNovel: (novelId: number) => void;
    onAddChapterToNovel: (novelId: number) => void;
    onReorderChapters: (novelId: number) => void;
    onDeleteNovel: (novelId: number) => void;
    onDeleteChapter: (chapterId: number, parentId: number) => void;
}

export interface AdminUserDto {
    userId: string;
    username: string | null;
    email: string | null;
    role: string | null;
    isEmailConfirmed: boolean;
    isLocked: boolean;
    createdAt: string | null;
    avatarThumbnail: string | null;
}