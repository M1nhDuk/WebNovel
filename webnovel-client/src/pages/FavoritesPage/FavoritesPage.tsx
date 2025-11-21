import React, { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import { useAuth } from '../../hooks/useAuth';
import type { PagedResult } from '../../types/series';
import type { UserFavoriteDto, AddFavoriteDto, FavoriteToggleResult } from '../../types/userActions';
import Pagination from '../../components/common/Pagination';
import { FaHeartBroken } from 'react-icons/fa';
import './FavoritesPage.css';

const GATEWAY_URL = 'https://localhost:8000';
const PAGE_SIZE = 12;

const FavoritesPage: React.FC = () => {
    const { user, isLoading: userLoading } = useAuth();

    const [favoriteList, setFavoriteList] = useState<UserFavoriteDto[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const getImageUrl = (coverPath: string | undefined | null) => {
        if (!coverPath) {
            return `${GATEWAY_URL}/images/covers/default_cover.jpg`;
        }
        const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    // Hàm fetchFavorites 
    const fetchFavorites = useCallback(async (pageToFetch: number) => {
        if (!user) {
            setIsLoading(false);
            return;
        }

        setIsLoading(true);
        setError(null);

        try {
            const response = await apiClient.get<PagedResult<UserFavoriteDto>>(
                API_ROUTES.USER.GET_FAVORITES,
                {
                    params: {
                        page: pageToFetch,
                        pageSize: PAGE_SIZE
                    }
                }
            );

            //PagedResult
            setFavoriteList(response.data.items);
            setCurrentPage(response.data.pageNumber);
            setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));

        } catch (err: any) {
            console.error("Failed to fetch favorites list:", err);
            setError(err.response?.data?.message || "Could not load your favorites.");
            setFavoriteList([]);
        } finally {
            setIsLoading(false);
        }
    }, [user]);

    useEffect(() => {
        if (!userLoading && !user) {
            setError("You must be logged in to view your favorites.");
            setIsLoading(false);
        }
    }, [user, userLoading]);


    useEffect(() => {
        if (user) {
            fetchFavorites(currentPage);
        }
    }, [user, fetchFavorites, currentPage]);


    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            setCurrentPage(page);
            window.scrollTo(0, 0);
        }
    };

    // Hàm xóa (Toggle)
    const handleDelete = async (seriesId: number) => {
        if (!window.confirm("Are you sure you want to unfollow this series?")) {
            return;
        }

        setIsDeleting(true);
        setError(null);

        const dto: AddFavoriteDto = {
            seriesId: seriesId,
            currentChapterCount: 0
        };

        try {
            await apiClient.post<FavoriteToggleResult>(
                API_ROUTES.USER.TOGGLE_FAVORITE,
                dto
            );

            if (favoriteList.length === 1 && currentPage > 1) {
                setCurrentPage(currentPage - 1);
            } else {
                fetchFavorites(currentPage);
            }

        } catch (err: any) {
            console.error("Failed to delete favorite item:", err);
            setError(err.response?.data?.message || "Failed to remove item.");
        } finally {
            setIsDeleting(false);
        }
    };

    if (userLoading || (isLoading && currentPage === 1)) {
        return <div className="favorites-page-container"><h1>My Favorites</h1>Loading...</div>;
    }

    if (!user) {
        return (
            <div className="favorites-page-container">
                <h1>My Favorites</h1>
                <div className="favorites-error">
                    {error || "Login to view your favorites."}
                </div>
                <div style={{ textAlign: 'center', marginTop: '20px' }}>
                    <Link to="/login">Login</Link>
                </div>
            </div>
        );
    }

    return (
        <div className="favorites-page-container">
            <h1>My Favorites</h1>

            {error && <div className="favorites-error">{error}</div>}

            <div className="favorites-list">
                {favoriteList.length === 0 && !isLoading ? (
                    <div className="favorites-empty">
                        You haven't favorited any series yet.
                    </div>
                ) : (
                    favoriteList.map(item => {
                        {/* --- [CHANGED Logic] --- */ }
                        // S? d?ng tr?c ti?p unreadCount t? Backend tr? v?
                        // Không c?n tính toán current - last n?a
                        const unreadCount = item.unreadCount || 0;
                        const hasNewChapter = unreadCount > 0;

                        return (
                            <div key={item.seriesId} className="favorite-item">
                                <Link to={`/series/${item.seriesId}`} className="favorite-cover-wrapper">
                                    <img
                                        src={getImageUrl(item.seriesCoverImage)}
                                        alt={item.seriesTitle || 'Series Cover'}
                                        className="favorite-cover"
                                    />
                                </Link>
                                <div className="favorite-details">
                                    <div className="favorite-header">
                                        <Link to={`/series/${item.seriesId}`} className="favorite-title">
                                            {item.seriesTitle || '[Series Deleted/Not Found]'}
                                        </Link>

                                        {/* Badge báo ch??ng m?i d?a trên unreadCount */}
                                        {hasNewChapter && (
                                            <span className="new-chapter-badge" title={`${unreadCount} new unread chapter(s)`}>
                                                New (+{unreadCount > 99 ? '99+' : unreadCount})
                                            </span>
                                        )}
                                    </div>

                                    <div className="favorite-progress-info">
                                        {/* Hi?n th? ti?n ?? ??c (ch? mang tính ch?t thông tin, ko ?nh h??ng logic ??m) */}
                                        {item.lastKnowChapter > 0 ? (
                                            <span className="reading-status text-primary">
                                                Reading: Chapter {item.lastKnowChapter}
                                            </span>
                                        ) : (
                                            <span className="reading-status text-muted">
                                                Not started
                                            </span>
                                        )}
                                        <span className="total-chapter-info">
                                            / {item.currentChapterCount} Chapters
                                        </span>
                                    </div>

                                    <span className="favorite-date">
                                        Added: {new Date(item.addedAt).toLocaleDateString()}
                                    </span>
                                </div>
                                <div className="favorite-actions">
                                    <button
                                        className="favorite-delete-btn"
                                        title="Unfollow this series"
                                        onClick={() => handleDelete(item.seriesId)}
                                        disabled={isDeleting}
                                    >
                                        <FaHeartBroken />
                                    </button>
                                </div>
                            </div>
                        );
                    })
                )}
            </div>

            {!isLoading && totalPages > 1 && (
                <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={handlePageChange}
                />
            )}
        </div>
    );
};

export default FavoritesPage;