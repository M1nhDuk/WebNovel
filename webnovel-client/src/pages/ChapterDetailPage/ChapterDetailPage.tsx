import React, { useState, useEffect, useMemo, useRef } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { NovelSeriesDetailDto, FullChapterDto, NovelDetailDto, ChapterDetailDto } from '../../types/series';
import ChapterCommentSection from '../../components/common/comments/ChapterCommentSection.tsx';
import type { BookmarkDto, ToggleBookmarkDto, BookmarkToggleResultDto } from '../../types/bookmarks';
import { useAuth } from '../../hooks/useAuth';
import type { FavoriteReadUpdateDto } from '../../types/userActions';

import {
    FaHome, FaArrowLeft, FaArrowRight, FaCog, FaBookmark, FaListUl, FaArrowCircleLeft
} from 'react-icons/fa';

import './ChapterDetailPage.css';

import { useReaderSettings } from '../../hooks/useReaderSettings';
import ReaderSettingsPanel from '../../components/reader/ReaderSettingsPanel';

const GATEWAY_URL = 'https://localhost:8000';

const getImageUrl = (coverPath: string | undefined | null) => {
    if (!coverPath) {
        return `${GATEWAY_URL}/images/covers/default_cover.jpg`;
    }
    const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
    return `${GATEWAY_URL}${formattedPath}`;
};

const ChapterDetailPage: React.FC = () => {
    const { seriesId, chapterId } = useParams<{ seriesId: string, chapterId: string }>();
    const navigate = useNavigate();

    const [series, setSeries] = useState<NovelSeriesDetailDto | null>(null);
    const [chapter, setChapter] = useState<FullChapterDto | null>(null);
    const [parentNovel, setParentNovel] = useState<NovelDetailDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const { user } = useAuth();
    const [isSidebarOpen, setIsSidebarOpen] = useState(false);
    const [isSettingsOpen, setIsSettingsOpen] = useState(false);

    const { settings, isLoading: isSettingsLoading } = useReaderSettings();
    const numChapterId = chapterId ? parseInt(chapterId, 10) : 0;

    // State cho bookmark (chỉ dùng để hiển thị icon trên dòng)
    const [bookmarkedLocation, setBookmarkedLocation] = useState<string | null>(null);
    const [isLoadingBookmark, setIsLoadingBookmark] = useState(true);


    const chapterBodyRef = useRef<HTMLDivElement>(null);

    const logReadingHistory = async (seriesId: string) => {
        if (!user || !seriesId) return;

        try {
            // SỬA LẠI ĐOẠN NÀY:
            await apiClient.post(API_ROUTES.USER.READING_HISTORY, {
                seriesId: Number(seriesId),
                chapterId: numChapterId
            });
            console.log(`Reading history updated for Series ID: ${seriesId}`);
        } catch (err) {
            console.warn("Failed to log reading history:", err);
        }
    };

    const syncProgress = async (currentSeriesId: number, currentChapterNumber: number) => {
        if (!user) return;

        try {
            const payload: FavoriteReadUpdateDto[] = [{
                seriesId: currentSeriesId,
                latestChapterCount: currentChapterNumber
            }];

            // Gọi API sync-counts
            await apiClient.post(API_ROUTES.USER.SYNC_COUNTS, payload);
            console.log(`Synced progress for Series ${currentSeriesId}: Chapter ${currentChapterNumber}`);
        } catch (err) {
            console.warn("Failed to sync reading progress to favorites:", err);
        }
    };



    useEffect(() => {
        const fetchChapterData = async () => {
            if (!seriesId || !chapterId) return;
            setLoading(true);
            setError(null);
            try {
                const seriesResponse = await apiClient.get<NovelSeriesDetailDto>(
                    API_ROUTES.SERIES.GET_BY_ID(seriesId)
                );
                const seriesData = seriesResponse.data;
                setSeries(seriesData);

                let contentApiUrl: string | null = null;
                let foundChapterMeta: ChapterDetailDto | undefined;

                if (seriesData.type === 'TRADITIONAL') {
                    contentApiUrl = API_ROUTES.SERIES.CHAPTER_FOR_SERIES(seriesId, chapterId);
                    foundChapterMeta = seriesData.chapters?.find(c => c.chapter_id === numChapterId);
                } else {
                    const sortedNovels = seriesData.novels.sort((a, b) => a.novel_number - b.novel_number);
                    for (const novel of sortedNovels) {
                        const sortedChapters = novel.chapters.sort((a, b) => a.chapter_number - b.chapter_number);
                        foundChapterMeta = sortedChapters.find(c => c.chapter_id === numChapterId);
                        if (foundChapterMeta) {
                            setParentNovel(novel);
                            contentApiUrl = API_ROUTES.SERIES.CHAPTER_FOR_NOVEL(novel.novel_Id, chapterId);
                            break;
                        }
                    }
                }

                if (!contentApiUrl || !foundChapterMeta) {
                    throw new Error("Chapter not found within series.");
                }

                const chapterResponse = await apiClient.get<FullChapterDto>(contentApiUrl);
                const chapterData = chapterResponse.data;
                setChapter(chapterData);

                // === [BẮT ĐẦU SỬA ĐỔI: GỌI SYNC PROGRESS] ===
                if (user) {
                    // 1. Lưu lịch sử đọc (như cũ)
                    logReadingHistory(seriesId);

                    // 2. Đồng bộ tiến độ vào danh sách Favorite (Mới)
                    // Sử dụng series_Id thực tế và chapter_number vừa tải được
                    await syncProgress(seriesData.series_Id, chapterData.chapter_number);
                }
                // === [KẾT THÚC SỬA ĐỔI] ===

            } catch (err: any) {
                console.error("Failed to fetch chapter:", err);
                setError(err.response?.data?.message || "Cannot load chapter.");
            } finally {
                setLoading(false);
            }
        };

        fetchChapterData();
    }, [seriesId, chapterId, numChapterId, user]);


    useEffect(() => {
        const fetchBookmarkStatus = async () => {
            if (user && numChapterId > 0) {
                setIsLoadingBookmark(true);
                try {
                    const response = await apiClient.get<BookmarkDto>(
                        API_ROUTES.USER.GET_BOOKMARK_FOR_CHAPTER(numChapterId)
                    );
                    if (response.data) {
                        setBookmarkedLocation(response.data.locationIdentifier);
                    } else {
                        setBookmarkedLocation(null);
                    }
                } catch (err: any) {
                    if (err.response && err.response.status === 404) {
                        setBookmarkedLocation(null);
                    } else {
                        console.error("Failed to fetch bookmark status:", err);
                    }
                } finally {
                    setIsLoadingBookmark(false);
                }
            } else {
                setBookmarkedLocation(null);
                setIsLoadingBookmark(false);
            }
        };

        fetchBookmarkStatus();
    }, [user, numChapterId]);




    const flatChapterList: ChapterDetailDto[] = useMemo(() => {
        if (!series) return [];
        if (series.type === 'TRADITIONAL') {
            return series.chapters?.sort((a, b) => a.chapter_number - b.chapter_number) || [];
        }
        return series.novels
            .sort((a, b) => a.novel_number - b.novel_number)
            .flatMap(novel => novel.chapters?.sort((a, b) => a.chapter_number - b.chapter_number) || []);
    }, [series]);


    const navigationLinks = useMemo(() => {
        if (!flatChapterList.length || !numChapterId || !seriesId) {
            return { prev: null, next: null, isFirst: true, isLast: true };
        }
        const currentIndex = flatChapterList.findIndex(c => c.chapter_id === numChapterId);
        if (currentIndex === -1) {
            return { prev: null, next: null, isFirst: true, isLast: true };
        }
        const isFirst = currentIndex === 0;
        const isLast = currentIndex === flatChapterList.length - 1;
        const prevChapter = !isFirst ? flatChapterList[currentIndex - 1] : null;
        const nextChapter = !isLast ? flatChapterList[currentIndex + 1] : null;
        return {
            prev: prevChapter ? `/series/${seriesId}/chapter/${prevChapter.chapter_id}` : null,
            next: nextChapter ? `/series/${seriesId}/chapter/${nextChapter.chapter_id}` : null,
            isFirst: isFirst,
            isLast: isLast
        };
    }, [flatChapterList, numChapterId, seriesId]);


    const handleNavigate = (direction: 'prev' | 'next') => {
        if (direction === 'prev') {
            if (navigationLinks.prev) {
                navigate(navigationLinks.prev);
            } else if (navigationLinks.isFirst) {
                navigate(`/series/${seriesId}`);
            }
        } else if (direction === 'next') {
            if (navigationLinks.next) {
                navigate(navigationLinks.next);
            } else if (navigationLinks.isLast) {
                navigate(`/series/${seriesId}`);
            }
        }
    };

    useEffect(() => {
        const handleKeyDown = (event: KeyboardEvent) => {
            const activeElement = document.activeElement;
            if (activeElement && activeElement instanceof HTMLElement) {
                if (activeElement.tagName === 'INPUT' ||
                    activeElement.tagName === 'TEXTAREA' ||
                    activeElement.isContentEditable) {
                    return;
                }
            }
            if (event.key === 'ArrowLeft') {
                event.preventDefault();
                handleNavigate('prev');
            } else if (event.key === 'ArrowRight') {
                event.preventDefault();
                handleNavigate('next');
            }
        };
        window.addEventListener('keydown', handleKeyDown);
        return () => {
            window.removeEventListener('keydown', handleKeyDown);
        };
    }, [navigationLinks, navigate, seriesId]);


    const saveBookmark = async (locationIdentifier: string, contextSnippet: string) => {
        if (!user || !series || !chapter || isLoadingBookmark) return;
        setIsLoadingBookmark(true);
        const payload: ToggleBookmarkDto = {
            seriesId: series.series_Id,
            chapterId: chapter.chapter_id,
            locationIdentifier: locationIdentifier,
            contextSnippet: contextSnippet
        };
        try {
            const response = await apiClient.post<BookmarkToggleResultDto>(
                API_ROUTES.USER.TOGGLE_BOOKMARK,
                payload
            );
            if (response.data.isBookmarked && response.data.data) {
                setBookmarkedLocation(response.data.data.locationIdentifier);
            }
        } catch (err: any) {
            console.error("Failed to save bookmark:", err);
        } finally {
            setIsLoadingBookmark(false);
        }
    };

    const removeBookmark = async () => {
        if (!user || !numChapterId || isLoadingBookmark) return;
        setIsLoadingBookmark(true);
        try {
            await apiClient.delete(
                API_ROUTES.USER.DELETE_BOOKMARK_FOR_CHAPTER(numChapterId)
            );
            setBookmarkedLocation(null);
        } catch (err: any) {
            console.error("Failed to remove bookmark:", err);
        } finally {
            setIsLoadingBookmark(false);
        }
    };

    const handleParagraphDoubleClick = (e: React.MouseEvent<HTMLParagraphElement>) => {
        if (!user) return;
        const paragraph = e.currentTarget;
        const newLocation = paragraph.id;
        const snippet = paragraph.textContent?.slice(0, 100) + '...';

        if (bookmarkedLocation === newLocation) {
            removeBookmark();
        } else {
            saveBookmark(newLocation, snippet || '');
        }
    };

    const handleNavigateToBookmark = () => {
        if (bookmarkedLocation) {
            const element = document.getElementById(bookmarkedLocation);
            if (element) {
                element.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        } else {
            alert("No bookmark in this chapter.");
        }
    };



    const renderSidebarContent = () => {
        if (!series) return null;

        return (
            <div className="sidebar-content-wrapper">

                <div className="sidebar-list-container">
                    {series.type === 'TRADITIONAL' ? (
                        <ul className="sidebar-list">
                            {series.chapters?.map(ch => (
                                <li key={ch.chapter_id} className={ch.chapter_id === numChapterId ? 'active' : ''}>
                                    <Link
                                        to={`/series/${series.series_Id}/chapter/${ch.chapter_id}`}
                                        onClick={() => setIsSidebarOpen(false)}
                                    >
                                        {ch.title}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    ) : (
                        <div className="sidebar-volume-list">
                            {series.novels.map(novel => (
                                <div key={novel.novel_Id} className="sidebar-volume-group">
                                    <h4 className="sidebar-volume-title">{novel.title}</h4>
                                    <ul className="sidebar-list">
                                        {novel.chapters.map(ch => (
                                            <li key={ch.chapter_id} className={ch.chapter_id === numChapterId ? 'active' : ''}>
                                                <Link
                                                    to={`/series/${series.series_Id}/chapter/${ch.chapter_id}`}
                                                    onClick={() => setIsSidebarOpen(false)}
                                                >
                                                    {ch.title}
                                                </Link>
                                            </li>
                                        ))}
                                    </ul>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        );
    };

    const mainContentStyles: React.CSSProperties = {
        backgroundColor: settings.backgroundColor,
    };
    const wrapperStyles: React.CSSProperties = {
        paddingLeft: `${settings.paddingPx}px`,
        paddingRight: `${settings.paddingPx}px`,
    };
    const getFontFamilyStack = (fontName: string) => {
        switch (fontName) {
            case "Noto Sans": return "'Noto Sans', sans-serif";
            case "Times New Roman": return "'Times New Roman', Times, serif";
            case "Merriweather": return "'Merriweather', serif";
            case "Lora": return "'Lora', serif";
            case "Roboto": return "'Roboto', sans-serif";
            default: return "'Times New Roman', Times, serif";
        }
    };
    const bodyTextStyles: React.CSSProperties = {
        fontFamily: getFontFamilyStack(settings.fontFamily),
        color: settings.fontColor,
        fontSize: `${settings.fontSize}px`,
        textAlign: settings.alignment as any,
    };
    const headerTextStyles: React.CSSProperties = {
        color: settings.fontColor,
    };


    if (loading || isSettingsLoading) {
        return <div className="chapter-loading-screen">Loading...</div>;
    }
    if (error) {
        return <div className="chapter-loading-screen">{error}</div>;
    }
    if (!chapter || !series) {
        return <div className="chapter-loading-screen">Chapter not found.</div>;
    }

    const paragraphs = chapter.content.split('\n').filter(p => p.trim() !== '');

    return (
        <>
            <ReaderSettingsPanel
                isOpen={isSettingsOpen}
                onClose={() => setIsSettingsOpen(false)}
            />

            <div className={`chapter-page-layout ${isSidebarOpen ? 'sidebar-visible' : ''}`}>

                {/* Cột trái (Sidebar) */}
                <aside className="chapter-sidebar-left">
                    <div className="sidebar-left-header">
                        <Link to={`/series/${series.series_Id}`} className="sidebar-series-link">
                            <img
                                src={getImageUrl(series.cover_images)}
                                alt={series.series_title}
                                className="sidebar-series-cover"
                            />
                            <div className="sidebar-series-info">
                                <h4>{series.series_title}</h4>
                                <span>{series.author || 'Updating'}</span>
                            </div>
                        </Link>
                        <button
                            className="sidebar-close-button"
                            title="Close"
                            onClick={() => setIsSidebarOpen(false)}
                        >
                            <FaArrowCircleLeft />
                        </button>
                    </div>
                    {renderSidebarContent()}
                </aside>


                <main className="chapter-content-main" style={mainContentStyles}>
                    <div className="chapter-content-wrapper" style={wrapperStyles}>
                        <header className="chapter-header">
                            <div className="chapter-series-title">
                                <Link to={`/series/${series.series_Id}`}>
                                    {series.series_title}
                                </Link>
                                {parentNovel && (
                                    <>
                                        {' / '}
                                        <Link to={`/series/${series.series_Id}/novel/${parentNovel.novel_Id}`}>
                                            {parentNovel.title}
                                        </Link>
                                    </>
                                )}
                            </div>
                            <h1 className="chapter-main-title" style={headerTextStyles}>
                                {chapter.title}
                            </h1>
                            <div className="chapter-metadata" style={headerTextStyles}>
                                <span>Length: {chapter.word_count.toLocaleString('vi-VN')} words</span>
                                <span> • </span>
                            </div>
                        </header>

                        <div className="chapter-body" ref={chapterBodyRef}>
                            {paragraphs.map((para, index) => (
                                <div key={index} className="paragraph-container">
                                    <p
                                        id={`p-index-${index}`}
                                        className="chapter-paragraph"
                                        style={bodyTextStyles}
                                        onDoubleClick={handleParagraphDoubleClick}
                                    >
                                        {para}
                                    </p>
                                    {bookmarkedLocation === `p-index-${index}` && (
                                        <FaBookmark
                                            className="line-bookmark-icon"
                                            title="Bookmark location"
                                            style={headerTextStyles}
                                        />
                                    )}
                                </div>
                            ))}
                        </div>
                    </div>

                    <ChapterCommentSection
                        chapterId={Number(chapterId)}
                    />
                </main>

                {/* Cột phải (Toolbar) */}
                <aside className="chapter-toolbar-right">
                    <button
                        className="toolbar-button"
                        title={navigationLinks.isFirst ? "Back to Series" : "Previous Chapter"}
                        onClick={() => handleNavigate('prev')}
                    >
                        <FaArrowLeft />
                    </button>
                    <button className="toolbar-button" title="Series Page" onClick={() => navigate(`/series/${seriesId}`)}>
                        <FaHome />
                    </button>


                    {/* === SỬA ĐỔI: Toggle Sidebar === */}
                    <button
                        className={`toolbar-button ${isSidebarOpen ? 'active' : ''}`}
                        title="Chapter List"
                        onClick={() => setIsSidebarOpen(!isSidebarOpen)} // Logic toggle
                    >
                        <FaListUl />
                    </button>

                    <button
                        className="toolbar-button"
                        title="Setting"
                        onClick={() => setIsSettingsOpen(true)}
                    >
                        <FaCog />
                    </button>

                    <button
                        className={`toolbar-button ${bookmarkedLocation ? 'active' : ''}`}
                        title={bookmarkedLocation ? "Go to bookmark" : "No bookmark"}
                        onClick={handleNavigateToBookmark}
                        disabled={isLoadingBookmark}
                    >
                        <FaBookmark />
                    </button>

                    <button
                        className="toolbar-button"
                        title={navigationLinks.isLast ? "Back to Series" : "Next Chapter"}
                        onClick={() => handleNavigate('next')}
                    >
                        <FaArrowRight />
                    </button>
                </aside>
            </div>

            {isSidebarOpen && (
                <div
                    className="sidebar-overlay"
                    onClick={() => setIsSidebarOpen(false)}
                ></div>
            )}
        </>
    );
};

export default ChapterDetailPage;