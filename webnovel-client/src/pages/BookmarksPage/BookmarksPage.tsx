import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import { useAuth } from '../../hooks/useAuth';
import type { PagedResult } from '../../types/series';
import type { BookmarkDto } from '../../types/bookmarks';
import Pagination from '../../components/common/Pagination';
import { FaTrash, FaChevronDown } from 'react-icons/fa';
import './BookmarksPage.css';

const GATEWAY_URL = 'https://localhost:8000';
const PAGE_SIZE = 10;

const getImageUrl = (coverPath: string | undefined | null) => {
    if (!coverPath) {
        return `${GATEWAY_URL}/images/covers/default_cover.jpg`;
    }
    const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
    return `${GATEWAY_URL}${formattedPath}`;
};


interface GroupedBookmarks {
    seriesId: number;
    seriesTitle: string | null;
    seriesCoverImage: string | null;
    bookmarks: BookmarkDto[]; 
}

const BookmarksPage: React.FC = () => {
    const { user, isLoading: userLoading } = useAuth();

    const [bookmarkList, setBookmarkList] = useState<BookmarkDto[]>([]); 
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [openSeriesId, setOpenSeriesId] = useState<number | null>(null);

    const fetchBookmarks = useCallback(async (pageToFetch: number) => {
        if (!user) {
            setIsLoading(false);
            return;
        }

        setIsLoading(true);
        setError(null);

        try {
            const response = await apiClient.get<PagedResult<BookmarkDto>>(
                API_ROUTES.USER.GET_BOOKMARKS,
                {
                    params: {
                        page: pageToFetch,
                        pageSize: 50 
                    }
                }
            );

            setBookmarkList(response.data.items);
            setCurrentPage(response.data.pageNumber);

        } catch (err: any) {
            console.error("Failed to fetch bookmarks list:", err);
            setError(err.response?.data?.message || "Could not load your bookmarks.");
            setBookmarkList([]);
        } finally {
            setIsLoading(false);
        }
    }, [user]);

    useEffect(() => {
        if (!userLoading && !user) {
            setError("You must be logged in to view your bookmarks.");
            setIsLoading(false);
        }
    }, [user, userLoading]);

    useEffect(() => {
        if (user) {
            fetchBookmarks(currentPage);
        }
    }, [user, fetchBookmarks, currentPage]);

    // --- LOGIC NHÓM BOOKMARK ---
    const groupedBookmarks = useMemo(() => {
        const groups = new Map<number, GroupedBookmarks>();

        for (const bookmark of bookmarkList) {
            if (!groups.has(bookmark.seriesId)) {
                groups.set(bookmark.seriesId, {
                    seriesId: bookmark.seriesId,


                    seriesTitle: bookmark.seriesTitle ?? null,
                    seriesCoverImage: bookmark.seriesCoverImage ?? null,


                    bookmarks: []
                });
            }

            groups.get(bookmark.seriesId)!.bookmarks.push(bookmark);
        }

        const result = Array.from(groups.values());


        setTotalPages(Math.ceil(result.length / PAGE_SIZE));


        const start = (currentPage - 1) * PAGE_SIZE;
        const end = start + PAGE_SIZE;

        return result.slice(start, end);

    }, [bookmarkList, currentPage]);


    // Hàm xử lý khi đổi trang
    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            setCurrentPage(page);
            window.scrollTo(0, 0);
        }
    };

    // Hàm xử lý khi nhấn vào mũi tên
    const handleToggleSeries = (seriesId: number) => {
        setOpenSeriesId(prevId => (prevId === seriesId ? null : seriesId));
    };

    // Hàm xóa bookmark
    const handleDelete = async (bookmarkId: string) => {
        if (!window.confirm("Are you sure you want to remove this bookmark?")) {
            return;
        }
        setError(null);
        try {
            await apiClient.delete(API_ROUTES.USER.DELETE_BOOKMARK(bookmarkId));
            fetchBookmarks(currentPage);
        } catch (err: any) {
            console.error("Failed to delete bookmark item:", err);
            setError(err.response?.data?.message || "Failed to remove item.");
        }
    };

    if (userLoading || (isLoading && currentPage === 1)) {
        return <div className="bookmarks-page-container"><h1>Bookmark list</h1>Loading...</div>;
    }

    if (!user) {
        return (
            <div className="bookmarks-page-container">
                <h1>Bookmark list</h1>
                <div className="bookmarks-error">
                    {error || "Login to view your bookmarks."}
                </div>
                <div style={{ textAlign: 'center', marginTop: '20px' }}>
                    <Link to="/login">Login</Link>
                </div>
            </div>
        );
    }

    return (
        <div className="bookmarks-page-container">
            <h1>Bookmark list</h1>

            {error && <div className="bookmarks-error">{error}</div>}

            <div className="bookmarks-list">
                {groupedBookmarks.length === 0 && !isLoading ? (
                    <div className="bookmarks-empty">
                        You haven't bookmarked any chapters yet.
                    </div>
                ) : (
                    groupedBookmarks.map(group => {
                        const isOpen = openSeriesId === group.seriesId;
                        return (
                            <div key={group.seriesId} className="bookmark-group-item">

                                {/* --- Header của Series --- */}
                                <div className="bookmark-series-header" onClick={() => handleToggleSeries(group.seriesId)}>
                                    <Link to={`/series/${group.seriesId}`} className="bookmark-series-info" onClick={(e) => e.stopPropagation()}>
                                        <img
                                            src={getImageUrl(group.seriesCoverImage)}
                                            alt={group.seriesTitle || 'Series Cover'}
                                            className="bookmark-cover"
                                        />
                                        <span className="bookmark-series-title">
                                            {group.seriesTitle || '[Series Not Found]'}
                                        </span>
                                    </Link>
                                    <button
                                        className={`bookmark-toggle-arrow ${isOpen ? 'toggled' : ''}`}
                                        title={isOpen ? "Collapse" : "Expand"}
                                    >
                                        <FaChevronDown />
                                    </button>
                                </div>

                                {/* --- Danh sách Chapter (Dropdown) --- */}
                                <div className={`bookmark-chapter-list ${isOpen ? 'open' : ''}`}>
                                    {group.bookmarks.map(item => (
                                        <div key={item.bookmarkId} className="bookmark-chapter-item">
                                            <div className="bookmark-chapter-details">
                                                <Link to={`/series/${item.seriesId}/chapter/${item.chapterId}`} className="bookmark-chapter-title">
                                                    {item.chapterTitle || '[Chapter Not Found]'}
                                                </Link>
                                                <span className="bookmark-chapter-context">
                                                    {item.contextSnippet ? `"...${item.contextSnippet}..."` : `Bookmarked at: ${item.locationIdentifier}`}
                                                </span>
                                            </div>
                                            <div className="bookmark-chapter-actions">
                                                <button
                                                    className="bookmark-delete-btn"
                                                    title="Remove bookmark"
                                                    onClick={() => handleDelete(item.bookmarkId)}
                                                    disabled={isLoading}
                                                >
                                                    <FaTrash />
                                                </button>
                                            </div>
                                        </div>
                                    ))}
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

export default BookmarksPage;