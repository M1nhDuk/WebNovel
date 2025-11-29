import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { NovelSeriesDetailDto, NovelDetailDto } from '../../types/series';
import '../SeriesDetailPage/SeriesDetailPage.css';
import './NovelDetailPage.css'

const GATEWAY_URL = 'https://localhost:8000';

const NovelDetailPage: React.FC = () => {

    const { seriesId, novelId } = useParams<{ seriesId: string, novelId: string }>();

    const [series, setSeries] = useState<NovelSeriesDetailDto | null>(null);
    const [novel, setNovel] = useState<NovelDetailDto | null>(null);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);




    const [isChapterListExpanded, setIsChapterListExpanded] = useState(false);
    const CHAPTER_LIMIT = 10;





    const getImageUrl = (coverPath: string | undefined | null) => {
        if (!coverPath) {
            return `${GATEWAY_URL}/images/covers/default_cover.jpg`;
        }
        const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    };


    useEffect(() => {
        if (!seriesId || !novelId) return;

        const fetchNovelDetails = async () => {
            setLoading(true);
            setError(null);
            try {
                const response = await apiClient.get<NovelSeriesDetailDto>(
                    API_ROUTES.SERIES.GET_BY_ID(seriesId)
                );
                setSeries(response.data);

                const numericNovelId = parseInt(novelId, 10);
                const foundNovel = response.data.novels.find(n => n.novel_Id === numericNovelId);

                if (foundNovel) {
                    setNovel(foundNovel);
                } else {
                    setError("Novel not found within this series.");
                }

            } catch (err) {
                console.error("Failed to fetch details:", err);
                setError("Could not load details.");
            } finally {
                setLoading(false);
            }
        };

        fetchNovelDetails();
    }, [seriesId, novelId]);

   

    if (loading) { return <div className="detail-page-container">Loading...</div>; }
    if (error) { return <div className="detail-page-container" style={{ color: 'red' }}>{error}</div>; }
    if (!series || !novel) { return <div className="detail-page-container">Details not found.</div>; }


    const isChapterListLong = novel.chapters.length > CHAPTER_LIMIT;
    const chaptersToShow = isChapterListExpanded
        ? novel.chapters
        : novel.chapters.slice(0, CHAPTER_LIMIT);


    return (
        <div className="detail-page-container">
            {/* Dùng novel.title thay vì series.series_title */}
            <h1 className="series-title-header">{novel.title}</h1>

            <div className="series-content-layout">

                {/* === CỘT BÊN TRÁI  === */}
                <aside className="series-left-col">
                    <img
                        src={getImageUrl(novel.cover_images)}
                        alt={novel.title}
                        className="series-cover-img"
                    />
                </aside>


                {/* === CỘT Ở GIỮA  === */}
                <section className="series-right-col">

                    <div className="synopsis-section">
                        <h3>Details</h3>
                        <div className="series-meta-info">
                            <div className="meta-item">
                                <strong>Publication:</strong>
                                <Link to={`/series/${series.series_Id}`}>{series.series_title}</Link>
                            </div>
                            <div className="meta-item">
                                <strong>Author:</strong>
                                <span>{series.author || 'N/A'}</span>
                            </div>
                            <div className="meta-item">
                                <strong>Artist:</strong>
                                <span>{series.artist || 'N/A'}</span>
                            </div>
                           
                        </div>
                    </div>


                    {/* (Phần Chapter List) */}
                    <div className="chapter-list-section novel-page-list">
                        <h3>Chapter List</h3>

                        <ul className="chapter-list">
                            {chaptersToShow.map(chapter => (
                                <li key={chapter.chapter_id} className="chapter-item">
                                    <Link
                                        to={`/series/${series.series_Id}/chapter/${chapter.chapter_id}`}
                                        className="chapter-title"
                                    >
                                        {chapter.title}
                                    </Link>
                                    <span className="chapter-date">{formatDate(chapter.created_at)}</span>
                                </li>
                            ))}

                            {/* Nút "Xem tiếp" / "Thu gọn" */}
                            {isChapterListLong && (
                                <li className="chapter-item chapter-see-more">
                                    <button
                                        className="toggle-chapters-btn"
                                        onClick={() => setIsChapterListExpanded(prev => !prev)}
                                    >
                                        {isChapterListExpanded
                                            ? "Less"
                                            : `More(${novel.chapters.length - CHAPTER_LIMIT} chương nữa)`
                                        }
                                    </button>
                                </li>
                            )}

                            {novel.chapters.length === 0 && (
                                <li className="chapter-item">
                                    <span>No chapters in this volume yet.</span>
                                </li>
                            )}
                        </ul>
                    </div>

                </section>


                {/* === CỘT BÊN PHẢI (SIDEBAR) === */}
                <aside className="series-sidebar-col">
                    <div className="sidebar-box">
                        <div className="sidebar-box-header uploader-header">
                            <img
                                src={getImageUrl(series.uploader_avatar)}
                                alt={series.uploader_name}
                                className="uploader-avatar"
                            />
                            <span className="uploader-name">Uploader</span>
                        </div>
                        <div className="sidebar-box-content uploader-name-value">
                            <Link to={`/user/${series.uploader_id}`} title={`View ${series.uploader_name}'s profile`}>
                                <strong>{series.uploader_name}</strong>
                            </Link>
                        </div>
                    </div>
                    {series.note && (
                        <div className="sidebar-box">
                            <div className="sidebar-box-header">
                                Note
                            </div>
                            <div className="sidebar-box-content">
                                <p>{series.note}</p>
                            </div>
                        </div>
                    )}
                </aside>
            </div>

        </div>
    );
};

export default NovelDetailPage;