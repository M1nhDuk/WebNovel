import React, { useState, useEffect, useMemo } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { NovelSeriesDetailDto, FullChapterDto, NovelDetailDto, ChapterDetailDto } from '../../types/series';
import ChapterCommentSection from '../../components/common/comments/ChapterCommentSection.tsx';

import {
    FaHome, FaArrowLeft, FaArrowRight, FaCog, FaBookmark, FaListUl, FaArrowCircleLeft
} from 'react-icons/fa';
import './ChapterDetailPage.css';
import { formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';

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

    const [isSidebarOpen, setIsSidebarOpen] = useState(false);
    const [isSettingsOpen, setIsSettingsOpen] = useState(false);

    const { settings, isLoading: isSettingsLoading } = useReaderSettings();
    const numChapterId = chapterId ? parseInt(chapterId, 10) : 0;

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
                    // Sắp xếp novel trước khi tìm
                    const sortedNovels = seriesData.novels.sort((a, b) => a.novel_number - b.novel_number);
                    for (const novel of sortedNovels) {
                        // Sắp xếp chapter trong novel
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
                setChapter(chapterResponse.data);
            } catch (err: any) {
                console.error("Failed to fetch chapter:", err);
                setError(err.response?.data?.message || "Cannot load chapter.");
            } finally {
                setLoading(false);
            }
        };
        fetchChapterData();
    }, [seriesId, chapterId, numChapterId]);



    // --- LOGIC CHUYỂN CHƯƠNG ---

    // 1. Tạo danh sách chương "phẳng" (đã được sắp xếp)
    const flatChapterList: ChapterDetailDto[] = useMemo(() => {
        if (!series) return [];
        if (series.type === 'TRADITIONAL') {
            // Đảm bảo đã sắp xếp
            return series.chapters?.sort((a, b) => a.chapter_number - b.chapter_number) || [];
        }
        // Gộp chương từ tất cả các volume (đã được sắp xếp)
        return series.novels
            .sort((a, b) => a.novel_number - b.novel_number)
            .flatMap(novel => novel.chapters?.sort((a, b) => a.chapter_number - b.chapter_number) || []);
    }, [series]);

    // 2. Tìm chương trước/sau
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

    // 3. Hàm xử lý sự kiện click nút
    const handleNavigate = (direction: 'prev' | 'next') => {
        if (direction === 'prev') {
            if (navigationLinks.prev) {
                navigate(navigationLinks.prev);
            } else if (navigationLinks.isFirst) {
                // Nếu là chương đầu, quay về trang series
                navigate(`/series/${seriesId}`);
            }
        } else if (direction === 'next') {
            if (navigationLinks.next) {
                navigate(navigationLinks.next);
            } else if (navigationLinks.isLast) {
                // Nếu là chương cuối, quay về trang series
                navigate(`/series/${seriesId}`);
            }
        }
    };

    // 4. Lắng nghe sự kiện bàn phím
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
                // Gọi hàm handleNavigate thay vì kiểm tra link
                handleNavigate('prev');
            } else if (event.key === 'ArrowRight') {
                event.preventDefault();
                // Gọi hàm handleNavigate thay vì kiểm tra link
                handleNavigate('next');
            }
        };

        window.addEventListener('keydown', handleKeyDown);

        return () => {
            window.removeEventListener('keydown', handleKeyDown);
        };
    }, [navigationLinks, navigate, seriesId]); // Thêm seriesId vào dependency

    // --- KẾT THÚC LOGIC CHUYỂN CHƯƠNG ---




    const formatTimeAgo = (dateString: string) => {
        const date = new Date(dateString);
        return formatDistanceToNow(date, { addSuffix: true, locale: vi });
    };

    const renderChapterList = () => {
        if (!series) return null;
        if (series.type === 'TRADITIONAL') {
            return (
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
            );
        }
        return (
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
        );
    };

    // Style cho toàn bộ nền của cột giữa
    const mainContentStyles: React.CSSProperties = {
        backgroundColor: settings.backgroundColor,
    };

    // Style cho wrapper nội dung (chỉ lề)
    const wrapperStyles: React.CSSProperties = {
        paddingLeft: `${settings.paddingPx}px`,
        paddingRight: `${settings.paddingPx}px`,
    };

    const getFontFamilyStack = (fontName: string) => {
        switch (fontName) {
            case "Noto Sans":
                return "'Noto Sans', sans-serif";
            case "Times New Roman":
                return "'Times New Roman', Times, serif";
            case "Merriweather":
                return "'Merriweather', serif";
            case "Lora":
                return "'Lora', serif";
            case "Roboto":
                return "'Roboto', sans-serif";
            default:
                return "'Times New Roman', Times, serif";
        }
    };

    // Style chỉ áp dụng cho VĂN BẢN NỘI DUNG
    const bodyTextStyles: React.CSSProperties = {
        fontFamily: getFontFamilyStack(settings.fontFamily),
        color: settings.fontColor,
        fontSize: `${settings.fontSize}px`,
        textAlign: settings.alignment as any,
    };

    // Style cho phần text của header (Title và Metadata)
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
                    {renderChapterList()}
                </aside>

                {/* Áp dụng style đã tách */}
                <main
                    className="chapter-content-main"
                    style={mainContentStyles}
                >
                    <div
                        className="chapter-content-wrapper"
                        style={wrapperStyles}
                    >

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
                                <span>Length: {chapter.word_count.toLocaleString('vi-VN')} từ</span>
                                <span> • </span>
                                <span>Updated at: {formatTimeAgo(chapter.created_at)}</span>
                            </div>
                        </header>


                        <div className="chapter-body">
                            {paragraphs.map((para, index) => (
                                <p key={index} style={bodyTextStyles}>{para}</p>
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
                    <button
                        className="toolbar-button"
                        title="Chapter List"
                        onClick={() => setIsSidebarOpen(!isSidebarOpen)}
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
                    <button className="toolbar-button" title="BookMark" onClick={() => alert('Đánh dấu chương')}>
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