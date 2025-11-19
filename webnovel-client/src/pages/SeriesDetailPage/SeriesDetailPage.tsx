import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { NovelSeriesDetailDto } from '../../types/series';
import {
    FaHeart,
    FaInfoCircle,
    FaPencilAlt,
    FaSpinner
} from 'react-icons/fa';

import type { PagedResult } from '../../types/series';
import { useAuth } from '../../hooks/useAuth';
import { formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';
import './SeriesDetailPage.css';
import SeriesCommentSection from '../../components/common/comments/SeriesCommentSection';
import type { UserFavoriteDto, AddFavoriteDto, FavoriteToggleResult } from '../../types/userActions';

const GATEWAY_URL = 'https://localhost:8000';

const SeriesDetailPage: React.FC = () => {

    const { id } = useParams<{ id: string }>();

    const [notification, setNotification] = useState<string | null>(null);
    const [isFavorited, setIsFavorited] = useState(false);
    const [series, setSeries] = useState<NovelSeriesDetailDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [isDescriptionExpanded, setIsDescriptionExpanded] = useState(false);
    const DESCRIPTION_THRESHOLD = 50;

    const [isTogglingFavorite, setIsTogglingFavorite] = useState(false);
    const [isLoadingInitialFavorite, setIsLoadingInitialFavorite] = useState(true);

    const { user } = useAuth();

    // --- Lưu danh sách các chương đã đọc trong session này để làm mờ ---
    const [readChapters, setReadChapters] = useState<Set<number>>(new Set());

    const [isTagsExpanded, setIsTagsExpanded] = useState(false);
    const TAGS_THRESHOLD = 15;

    const [expandedVolumes, setExpandedVolumes] = useState<Set<number>>(new Set());

    const getImageUrl = (coverPath: string | undefined | null) => {
        if (!coverPath) {
            return `${GATEWAY_URL}/images/covers/default_cover.jpg`;
        }
        const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    // --- Xử lý khi click đọc chương ---
    const handleReadChapter = async (chapterId: number) => {
        //Cập nhật
        setReadChapters(prev => new Set(prev).add(chapterId));

        if (!user || !id) return;

        try {
            //Gọi API báo Backend cập nhật lịch sử & giảm Badge
            await apiClient.post(API_ROUTES.USER.READING_HISTORY, {
                seriesId: Number(id),
                chapterId: chapterId
            });
            console.log(`Saved history for Chapter ${chapterId}`);
        } catch (err) {
            console.warn("Failed to save reading history:", err);
        }
    };

    useEffect(() => {
        if (!id) return;

        const fetchSeriesDetail = async () => {
            setLoading(true);
            setError(null);
            try {
                const response = await apiClient.get<NovelSeriesDetailDto>(
                    API_ROUTES.SERIES.GET_BY_ID(id)
                );
                console.log("DỮ LIỆU SERIES NHẬN ĐƯỢC:", response.data);
                setSeries(response.data);
            } catch (err) {
                console.error("Failed to fetch series details:", err);
                setError("Could not load series details.");
            } finally {
                setLoading(false);
            }
        };

        fetchSeriesDetail();

    }, [id]);


    // Check Favorite Status
    useEffect(() => {
        const checkFavoriteStatus = async () => {
            if (user && id) {
                setIsLoadingInitialFavorite(true);
                try {
                    const response = await apiClient.get<PagedResult<UserFavoriteDto>>(
                        API_ROUTES.USER.GET_FAVORITES,
                        {
                            params: {
                                page: 1,
                                pageSize: 100
                            }
                        }
                    );
                    const isFav = response.data.items.some(fav => fav.seriesId === Number(id));
                    setIsFavorited(isFav);
                } catch (err) {
                    console.error("Failed to check favorite status:", err);
                } finally {
                    setIsLoadingInitialFavorite(false);
                }
            } else {
                setIsLoadingInitialFavorite(false);
            }
        };

        checkFavoriteStatus();
    }, [id, user]);


    useEffect(() => {
        if (notification) {
            const timer = setTimeout(() => {
                setNotification(null);
            }, 4000);

            return () => clearTimeout(timer);
        }
    }, [notification]);


    const handleFavoriteClick = async () => {
        if (!user) {
            return;
        }

        if (!id || isTogglingFavorite) return;

        setIsTogglingFavorite(true);
        setNotification(null);
        setError(null);

        const dto: AddFavoriteDto = {
            seriesId: Number(id),
            currentChapterCount: 0 
        };

        try {
            const response = await apiClient.post<FavoriteToggleResult>(
                API_ROUTES.USER.TOGGLE_FAVORITE,
                dto
            );

            const { isFavorited: newStatus, message } = response.data;

            setIsFavorited(newStatus);
            setNotification(message);

        } catch (err: any) {
            console.error("Failed to toggle favorite:", err);
            setError(err.response?.data?.message || "An error occurred.");
        } finally {
            setIsTogglingFavorite(false);
        }
    };

    if (loading || isLoadingInitialFavorite) {
        return <div className="detail-page-container">Loading...</div>;
    }

    if (error) { return <div className="detail-page-container" style={{ color: 'red' }}>{error}</div>; }
    if (!series) { return <div className="detail-page-container">Series not found.</div>; }


    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    };

    const formatTimeAgo = (dateString: string) => {
        const date = new Date(dateString);
        return formatDistanceToNow(date, { locale: vi });
    };

    const toggleVolumeExpand = (volumeId: number) => {
        setExpandedVolumes(prev => {
            const newSet = new Set(prev);
            if (newSet.has(volumeId)) {
                newSet.delete(volumeId);
            } else {
                newSet.add(volumeId);
            }
            return newSet;
        });
    };

    const isUploader = user && series.uploader_id === user.userId;
    const isAdmin = user && user.role === 'Admin';
    const canEdit = isUploader || isAdmin;


    return (
        <div className="detail-page-container">
            <h1 className="series-title-header">{series.series_title}</h1>

            <div className="series-content-layout">

                {/* === CỘT BÊN TRÁI === */}
                <aside className="series-left-col">
                    <img
                        src={getImageUrl(series.cover_images)}
                        alt={series.series_title}
                        className="series-cover-img"
                    />
                    <button
                        className={`series-action-btn ${isFavorited ? 'favorited' : ''}`}
                        onClick={handleFavoriteClick}
                        disabled={isTogglingFavorite}
                    >
                        {isTogglingFavorite ? (
                            <FaSpinner className="fa-spin" />
                        ) : (
                            <FaHeart style={{ marginRight: '8px' }} />
                        )}
                        {isFavorited ? 'Following' : 'Follow'}
                    </button>

                    {canEdit && (
                        <Link to={`/manage/series/${series.series_Id}`} style={{ display: 'block', textDecoration: 'none', marginTop: '10px' }}>
                            <button
                                className="series-action-btn"
                                style={{
                                    backgroundColor: 'var(--accent-color)',
                                    color: '#fff',
                                    border: '2px solid var(--accent-color)'
                                }}
                            >
                                <FaPencilAlt style={{ marginRight: '8px' }} />
                                Edit Series
                            </button>
                        </Link>
                    )}

                </aside>


                {/* === CỘT Ở GIỮA === */}
                <section className="series-right-col">

                    {/* (Phần Details) */}
                    <div className="synopsis-section">
                        <h3>Details</h3>
                        {(() => {
                            const areTagsLong = series.tags.length > TAGS_THRESHOLD;
                            const tagsToShow = areTagsLong && !isTagsExpanded
                                ? series.tags.slice(0, TAGS_THRESHOLD)
                                : series.tags;

                            return (
                                <>
                                    <div
                                        className={`series-tags-list ${areTagsLong && !isTagsExpanded ? 'collapsed' : ''}`}
                                    >
                                        <strong>Tags:</strong>
                                        {tagsToShow.map(tag => (
                                            <Link to={`/browse?tag=${tag}`} className="tag-pill" key={tag}>
                                                {tag}
                                            </Link>
                                        ))}
                                    </div>
                                    {areTagsLong && (
                                        <div className="tags-toggle-footer">
                                            <button
                                                className="toggle-description-btn"
                                                onClick={() => setIsTagsExpanded(prev => !prev)}
                                            >
                                                {isTagsExpanded ? 'Collapse' : '.............'}
                                            </button>
                                        </div>
                                    )}
                                </>
                            );
                        })()}


                        <div className="series-meta-info">
                            <div className="meta-item">
                                <strong>Author:</strong>
                                <span>{series.author || 'N/A'}</span>
                            </div>
                            <div className="meta-item">
                                <strong>Artist:</strong>
                                <span>{series.artist || 'N/A'}</span>
                            </div>
                            <div className="meta-item">
                                <strong>Status:</strong>
                                <span>{series.statusName || 'N/A'}</span>
                            </div>
                            <div className="meta-item">
                                <strong>Category:</strong>
                                <span>{series.categoryName || 'N/A'}</span>
                            </div>

                            {series.type === 'TRADITIONAL' && (
                                <>
                                    {series.iSBN_13 && (
                                        <div className="meta-item">
                                            <strong>ISBN-13:</strong>
                                            <span>{series.iSBN_13}</span>
                                        </div>
                                    )}
                                    {series.iSBN_10 && (
                                        <div className="meta-item">
                                            <strong>ISBN-10:</strong>
                                            <span>{series.iSBN_10}</span>
                                        </div>
                                    )}
                                    {series.publisher && (
                                        <div className="meta-item">
                                            <strong>Publisher:</strong>
                                            <span>{series.publisher}</span>
                                        </div>
                                    )}
                                    {series.publish_date && (
                                        <div className="meta-item">
                                            <strong>Published:</strong>
                                            <span>{formatDate(series.publish_date)}</span>
                                        </div>
                                    )}
                                    {series.edition && (
                                        <div className="meta-item">
                                            <strong>Edition:</strong>
                                            <span>{series.edition}</span>
                                        </div>
                                    )}
                                </>
                            )}
                        </div>

                        <div className="series-stats-bar">
                            <div className="stat-item">
                                <span className="stat-label">Last Update</span>
                                <span className="stat-value">{formatTimeAgo(series.updated_at)}</span>
                            </div>
                            <div className="stat-item">
                                <span className="stat-label">Word count</span>
                                <span className="stat-value">
                                    {series.word_count.toLocaleString('vi-VN')}
                                </span>
                            </div>
                            <div className="stat-item">
                                <span className="stat-label">View</span>
                                <span className="stat-value">
                                    {series.views.toLocaleString('vi-VN')}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* (Khối Tóm tắt) */}
                    <div className="sidebar-box">
                        <div className="sidebar-box-header">
                            Tóm tắt
                        </div>
                        {(() => {
                            const isLong = series.description.length > DESCRIPTION_THRESHOLD;
                            return (
                                <>
                                    <div
                                        className={`sidebar-box-content description-content ${isLong && !isDescriptionExpanded ? 'collapsed' : ''}`}
                                    >
                                        <p>{series.description}</p>
                                        {isLong && !isDescriptionExpanded && (
                                            <div
                                                className="description-overlay-toggle"
                                                onClick={() => setIsDescriptionExpanded(true)}
                                            >
                                                <span>More</span>
                                            </div>
                                        )}
                                    </div>
                                    {isLong && isDescriptionExpanded && (
                                        <div className="description-toggle-footer">
                                            <button
                                                className="toggle-description-btn"
                                                onClick={() => setIsDescriptionExpanded(false)}
                                            >
                                                Collapse
                                            </button>
                                        </div>
                                    )}
                                </>
                            );
                        })()}
                    </div>


                    {/* (Phần Volume & Chapter List) */}
                    <div className="chapter-list-section">
                        <h3>Volume & Chapter List</h3>

                        {/* === HIỂN THỊ NẾU LÀ WEB NOVEL (CÓ VOLUME) === */}
                        {series.type === 'Series' && series.novels.length > 0 && series.novels.map(volume => {
                            const isExpanded = expandedVolumes.has(volume.novel_Id);
                            const chaptersToShow = isExpanded
                                ? volume.chapters
                                : volume.chapters.slice(0, 5);

                            return (
                                <div key={volume.novel_Id} className="volume-item">
                                    <div className="volume-cover">
                                        <Link to={`/series/${series.series_Id}/novel/${volume.novel_Id}`}>
                                            <img
                                                src={getImageUrl(volume.cover_images)}
                                                alt={volume.title}
                                                className="volume-cover-img"
                                            />
                                        </Link>
                                    </div>
                                    <div className="volume-list-wrapper">
                                        <h4 className="volume-title">{volume.title}</h4>
                                        <ul className="chapter-list">
                                            {chaptersToShow.map(chapter => {
                                                // Kiểm tra xem đã đọc chưa
                                                const isRead = readChapters.has(chapter.chapter_id);

                                                return (
                                                    <li key={chapter.chapter_id} className={`chapter-item ${isRead ? 'is-read' : ''}`}>
                                                        <Link
                                                            to={`/series/${series.series_Id}/chapter/${chapter.chapter_id}`}
                                                            className="chapter-title"
                                                            onClick={() => handleReadChapter(chapter.chapter_id)}
                                                        >
                                                            {chapter.title}
                                                        </Link>
                                                        <span className="chapter-date">{formatDate(chapter.created_at)}</span>
                                                    </li>
                                                );
                                            })}

                                            {volume.chapters.length > 5 && (
                                                <li className="chapter-item chapter-see-more">
                                                    <button
                                                        className="toggle-chapters-btn"
                                                        onClick={() => toggleVolumeExpand(volume.novel_Id)}
                                                    >
                                                        {isExpanded
                                                            ? "Thu gọn"
                                                            : `Xem tiếp (${volume.chapters.length} chương)`
                                                        }
                                                    </button>
                                                </li>
                                            )}

                                            {volume.chapters.length === 0 && (
                                                <li className="chapter-item">
                                                    <span>No chapters in this volume yet.</span>
                                                </li>
                                            )}
                                        </ul>
                                    </div>
                                </div>
                            );
                        })}

                        {/* === HIỂN THỊ NẾU LÀ CLASSIC NOVEL (CHỈ CÓ CHAPTER) === */}
                        {series.type === 'TRADITIONAL' && series.chapters && series.chapters.length > 0 && (
                            <div className="volume-item">
                                <div className="volume-list-wrapper" style={{ width: "100%" }}>
                                    <h4 className="volume-title">Chapters</h4>
                                    <ul className="chapter-list">
                                        {series.chapters.map(chapter => {
                                            const isRead = readChapters.has(chapter.chapter_id);

                                            return (
                                                <li key={chapter.chapter_id} className={`chapter-item ${isRead ? 'is-read' : ''}`}>
                                                    <Link
                                                        to={`/series/${series.series_Id}/chapter/${chapter.chapter_id}`}
                                                        className="chapter-title"
                                                        onClick={() => handleReadChapter(chapter.chapter_id)} 
                                                    >
                                                        {chapter.title}
                                                    </Link>
                                                    <span className="chapter-date">{formatDate(chapter.created_at)}</span>
                                                </li>
                                            );
                                        })}
                                    </ul>
                                </div>
                            </div>
                        )}


                        {series.type === 'Series' && series.novels.length === 0 && (
                            <p>No volumes or chapters have been added yet.</p>
                        )}
                        {series.type === 'TRADITIONAL' && (!series.chapters || series.chapters.length === 0) && (
                            <p>No chapters have been added yet.</p>
                        )}
                    </div>


                    {/* ---COMMENT  --- */}
                    <SeriesCommentSection
                        seriesId={Number(id)}
                    />
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

            {notification && (
                <div className={`action-notification-bar ${isFavorited ? 'follow' : 'unfollow'}`}>
                    <FaInfoCircle className="notification-icon" />
                    <span className="notification-text">{notification}</span>
                </div>
            )}
        </div>
    );
};

export default SeriesDetailPage;