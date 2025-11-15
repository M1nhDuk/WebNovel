import React, { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import { useAuth } from '../../hooks/useAuth';
import type { PagedResult } from '../../types/series';
import type { ReadingHistoryDto } from '../../types/readingHistory';
import Pagination from '../../components/common/Pagination';
import { FaTrash } from 'react-icons/fa';
import './ReadingHistoryPage.css';

const GATEWAY_URL = 'https://localhost:8000';
const PAGE_SIZE = 12;

const ReadingHistoryPage: React.FC = () => {
    const { user, isLoading: userLoading } = useAuth();

    const [historyList, setHistoryList] = useState<ReadingHistoryDto[]>([]);
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


    const fetchHistory = useCallback(async (pageToFetch: number) => {
        if (!user) {
            setIsLoading(false);
            return;
        }

        setIsLoading(true);
        setError(null);

        try {
            const response = await apiClient.get<PagedResult<ReadingHistoryDto>>(
                API_ROUTES.USER.READING_HISTORY,
                {
                    params: {
                        page: pageToFetch,
                        pageSize: PAGE_SIZE
                    }
                }
            );

            setHistoryList(response.data.items);
            setCurrentPage(response.data.pageNumber);
            setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));

        } catch (err: any) {
            console.error("Failed to fetch reading history:", err);
            setError(err.response?.data?.message || "Could not load reading history.");
            setHistoryList([]);
        } finally {
            setIsLoading(false);
        }
    }, [user]);

    useEffect(() => {
        if (!userLoading && !user) {
            setError("You must be logged in to view your reading history.");
            setIsLoading(false);
        }
    }, [user, userLoading]);

    useEffect(() => {
        if (user) {
            fetchHistory(currentPage);
        }
    }, [user, fetchHistory, currentPage]);

    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            setCurrentPage(page);
            window.scrollTo(0, 0);
        }
    };

    const handleDelete = async (historyId: string) => {
        if (!window.confirm("Are you sure you want to remove this item from your reading history?")) {
            return;
        }

        setIsDeleting(true);
        setError(null);

        try {
            await apiClient.delete(
                API_ROUTES.USER.READING_HISTORY,
                {
                    data: { historyIds: [historyId] }
                }
            );

           
            await fetchHistory(currentPage);

        } catch (err: any) {
            console.error("Failed to delete history item:", err);
            setError(err.response?.data?.message || "Failed to remove item.");
        } finally {
            setIsDeleting(false);
        }
    };

    if (userLoading || isLoading) {
        return <div className="history-page-container">Loading reading history...</div>;
    }

    if (!user) {
        return (
            <div className="history-page-container">
                <div className="history-error">
                    {error || "Login to view reaeding history."}
                </div>
                <div style={{ textAlign: 'center', marginTop: '20px' }}>
                    <Link to="/login">Login</Link>
                </div>
            </div>
        );
    }

    return (
        <div className="history-page-container">
            <h1>Reading History</h1>

            {error && <div className="history-error">{error}</div>}

            <div className="history-list">
                {historyList.length === 0 && !isLoading ? (
                    <div className="history-empty">
                        Your reading history is empty. Start reading a series to see it appear here!
                    </div>
                ) : (
                    historyList.map(item => (
                        <div key={item.historyId} className="history-item">
                            <Link to={`/series/${item.seriesId}`} className="history-cover-wrapper">
                                <img
                                    src={getImageUrl(item.seriesCoverImage)}
                                    alt={item.seriesTitle || 'Series Cover'}
                                    className="history-cover"
                                />
                            </Link>
                            <div className="history-details">
                                <Link to={`/series/${item.seriesId}`} className="history-title">
                                    {item.seriesTitle || '[Series Deleted/Not Found]'}
                                </Link>
                            </div>
                            <div className="history-actions">
                                <button
                                    className="history-delete-btn"
                                    title="Remove from history"
                                    onClick={() => handleDelete(item.historyId)}
                                    disabled={isDeleting}
                                >
                                    <FaTrash />
                                </button>
                            </div>
                        </div>
                    ))
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

export default ReadingHistoryPage;