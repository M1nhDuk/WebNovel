import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { NovelSeriesDetailDto, FullChapterDto, NovelDetailDto, ChapterDetailDto } from '../../types/series';
import ChapterCommentSection from '../../components/common/comments/ChapterCommentSection.tsx';

import {
    FaHome,
    FaArrowLeft,
    FaArrowRight,
    FaCog,
    FaInfoCircle,
    FaBookmark,
    FaListUl, 
    FaArrowCircleLeft
} from 'react-icons/fa';
import './ChapterDetailPage.css'; 
import { formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';


// Helper lấy URL ảnh, đảm bảo hoạt động
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

    // State quản lý việc ẩn/hiện sidebar trái
    const [isSidebarOpen, setIsSidebarOpen] = useState(false);

    // Biến chapterId thành số để so sánh
    const numChapterId = chapterId ? parseInt(chapterId, 10) : 0;

    useEffect(() => {
        const fetchChapterData = async () => {
            if (!seriesId || !chapterId) return;

            setLoading(true);
            setError(null);

            try {
                // 1. Lấy thông tin Series (để biết tên, loại, và cấu trúc)
                const seriesResponse = await apiClient.get<NovelSeriesDetailDto>(
                    API_ROUTES.SERIES.GET_BY_ID(seriesId)
                );
                const seriesData = seriesResponse.data;
                setSeries(seriesData);

                let contentApiUrl: string | null = null;
                let foundChapterMeta: ChapterDetailDto | undefined;

                // 2. Quyết định API nào để gọi dựa trên loại Series
                if (seriesData.type === 'TRADITIONAL') {
                    contentApiUrl = API_ROUTES.SERIES.CHAPTER_FOR_SERIES(seriesId, chapterId);
                    foundChapterMeta = seriesData.chapters?.find(c => c.chapter_id === numChapterId);
                } else {
                    for (const novel of seriesData.novels) {
                        foundChapterMeta = novel.chapters.find(c => c.chapter_id === numChapterId);
                        if (foundChapterMeta) {
                            setParentNovel(novel);
                            contentApiUrl = API_ROUTES.SERIES.CHAPTER_FOR_NOVEL(novel.novel_Id, chapterId);
                            break;
                        }
                    }
                }

                if (!contentApiUrl || !foundChapterMeta) {
                    throw new Error("Không tìm thấy chương này trong series.");
                }

                // 3. Lấy nội dung chi tiết của chương
                const chapterResponse = await apiClient.get<FullChapterDto>(contentApiUrl);
                setChapter(chapterResponse.data);

            } catch (err: any) {
                console.error("Failed to fetch chapter:", err);
                setError(err.response?.data?.message || "Không thể tải dữ liệu chương.");
            } finally {
                setLoading(false);
            }
        };

        fetchChapterData();
    }, [seriesId, chapterId, numChapterId]);

    const formatTimeAgo = (dateString: string) => {
        const date = new Date(dateString);
        return formatDistanceToNow(date, { addSuffix: true, locale: vi });
    };

    if (loading) {
        return <div className="chapter-loading-screen">Đang tải...</div>;
    }

    if (error) {
        return <div className="chapter-loading-screen">{error}</div>;
    }

    if (!chapter || !series) {
        return <div className="chapter-loading-screen">Không tìm thấy chương.</div>;
    }

    // Tách nội dung thành các đoạn văn (split bằng dấu xuống dòng)
    const paragraphs = chapter.content.split('\n').filter(p => p.trim() !== '');

    // Hàm render danh sách chương cho cột trái
    const renderChapterList = () => {
        // Flow 2: TRADITIONAL (Series -> Chapter)
        if (series.type === 'TRADITIONAL') {
            return (
                <ul className="sidebar-list">
                    {series.chapters?.map(ch => (
                        <li key={ch.chapter_id} className={ch.chapter_id === numChapterId ? 'active' : ''}>
                            <Link
                                to={`/series/${series.series_Id}/chapter/${ch.chapter_id}`}
                                // Thêm onClick để tự đóng sidebar khi chọn chương (trên mobile)
                                onClick={() => setIsSidebarOpen(false)}
                            >
                                {ch.title}
                            </Link>
                        </li>
                    ))}
                </ul>
            );
        }

        // Flow 1: Series (Series -> Novel -> Chapter)
        return (
            <div className="sidebar-volume-list">
                {series.novels.map(novel => (
                    <div key={novel.novel_Id} className="sidebar-volume-group">
                        {/* Đây là tên Volume (ví dụ: Tập 01 Đại Khởi) */}
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

    return (
        <>
            {/* Class 'sidebar-visible' sẽ được thêm/bớt bởi React */}
            <div className={`chapter-page-layout ${isSidebarOpen ? 'sidebar-visible' : ''}`}>

                {/* === CỘT TRÁI: DANH SÁCH CHƯƠNG === */}
                <aside className="chapter-sidebar-left">
                    <div className="sidebar-left-header">
                        <Link to={`/series/${series.series_Id}`} className="sidebar-series-link">
                            <img
                                // Lấy ảnh bìa của series
                                src={getImageUrl(series.cover_images)}
                                alt={series.series_title}
                                className="sidebar-series-cover"
                            />
                            <div className="sidebar-series-info">
                                <h4>{series.series_title}</h4>
                                <span>{series.author || 'Đang cập nhật'}</span>
                            </div>
                        </Link>
                        {/* Nút này sẽ đóng sidebar trên mobile */}
                        <button
                            className="sidebar-close-button"
                            title="Đóng"
                            onClick={() => setIsSidebarOpen(false)}
                        >
                            <FaArrowCircleLeft />
                        </button>
                    </div>
                    {renderChapterList()}
                </aside>

                {/* === CỘT GIỮA: NỘI DUNG CHÍNH === */}
                <main className="chapter-content-main">
                    <div className="chapter-content-wrapper">
                        <header className="chapter-header">
                            <div className="chapter-series-title">
                                <Link to={`/series/${series.series_Id}`}>
                                    {series.series_title}
                                </Link>
                                {/* Hiển thị tên Volume (nếu có) */}
                                {parentNovel && (
                                    <>
                                        {' / '}
                                        <Link to={`/series/${series.series_Id}/novel/${parentNovel.novel_Id}`}>
                                            {parentNovel.title}
                                        </Link>
                                    </>
                                )}
                            </div>
                            <h1 className="chapter-main-title">{chapter.title}</h1>
                            <div className="chapter-metadata">
                                <span>Độ dài: {chapter.word_count.toLocaleString('vi-VN')} từ</span>
                                <span> • </span>
                                <span>Cập nhật: {formatTimeAgo(chapter.created_at)}</span>
                            </div>
                        </header>

                        <div className="chapter-body">
                            {paragraphs.map((para, index) => (
                                <p key={index}>{para}</p>
                            ))}
                        </div>
                    </div>

                    <ChapterCommentSection
                        chapterId={Number(chapterId)}
                        totalCommentCount={0}
                    />

                </main>           

                {/* === CỘT PHẢI: THANH CÔNG CỤ === */}
                <aside className="chapter-toolbar-right">
                    <button className="toolbar-button" title="Chương trước" onClick={() => alert('Chuyển chương trước')}>
                        <FaArrowLeft />
                    </button>
                    <button className="toolbar-button" title="Trang chủ Series" onClick={() => navigate(`/series/${seriesId}`)}>
                        <FaHome />
                    </button>
                    {/* Nút này sẽ toggle sidebar trái */}
                    <button
                        className="toolbar-button"
                        title="Danh sách chương"
                        onClick={() => setIsSidebarOpen(!isSidebarOpen)}
                    >
                        <FaListUl />
                    </button>
                    <button className="toolbar-button" title="Cài đặt" onClick={() => alert('Mở cài đặt')}>
                        <FaCog />
                    </button>
                    <button className="toolbar-button" title="Thông tin" onClick={() => alert('Hiển thị thông tin')}>
                        <FaInfoCircle />
                    </button>
                    <button className="toolbar-button" title="Đánh dấu" onClick={() => alert('Đánh dấu chương')}>
                        <FaBookmark />
                    </button>
                    <button className="toolbar-button" title="Chương sau" onClick={() => alert('Chuyển chương sau')}>
                        <FaArrowRight />
                    </button>
                </aside>
            </div>

            {/* Lớp phủ màu đen, bấm vào sẽ đóng sidebar */}
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