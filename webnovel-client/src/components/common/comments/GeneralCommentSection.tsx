import React, { useState, useEffect, useCallback } from 'react';
import apiClient from '../../../api/apiClient';
import type { CommentDto, CommentSectionProps } from '../../../types/comments';
import type { PagedResult } from '../../../types/series';
import { useAuth } from '../../../hooks/useAuth';
import Pagination from '../../common/Pagination';
import '../CSS/CommentSection.css';
import { Link } from 'react-router-dom';

const PAGE_SIZE = 8;
const GATEWAY_URL = 'https://localhost:8000';

const GeneralCommentSection: React.FC<CommentSectionProps> = ({
    seriesId,
    chapterId,
    totalCommentCount
}) => {
    const { user } = useAuth();
    const [comments, setComments] = useState<CommentDto[]>([]);
    const [newComment, setNewComment] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);

    // Xác định Endpoint và ID mục tiêu
    const isSeriesComment = !!seriesId;
    const targetId = isSeriesComment ? seriesId : chapterId;
    const targetType = isSeriesComment ? 'series' : 'chapters';

    if (!targetId) {
        return <div className="comment-section">Error: Missing Series ID or Chapter ID.</div>;
    }


    const fetchComments = useCallback(async (page: number) => {
        setIsLoading(true);
        setError(null);
        try {
            // Sử dụng API routes chung: /series/{id}/comments hoặc /chapters/{id}/comments
            const endpoint = `/${targetType}/${targetId}/comments`;

            const response = await apiClient.get<PagedResult<CommentDto>>(
                endpoint,
                { params: { page, size: PAGE_SIZE } }
            );

            setComments(response.data.items);
            setCurrentPage(response.data.pageNumber);
            setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));

        } catch (err: any) {
            setError(err.response?.data?.message || `Failed to load comments for ${targetType}.`);
        } finally {
            setIsLoading(false);
        }
    }, [targetId, targetType]);

    useEffect(() => {
        fetchComments(currentPage);
    }, [fetchComments, currentPage]);


    const handleSubmitComment = async () => {
        if (!user || newComment.trim() === '') return;

        try {
            const endpoint = `/${targetType}/${targetId}/comments`;

            // Route POST cho cả Series và Chapter đều có cấu trúc tương tự
            await apiClient.post(endpoint, {
                content: newComment.trim(),
                parentCommentId: null
            });

            setNewComment('');
            await fetchComments(1);

        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to post comment.");
        }
    };

    // Helper functions (getAvatarUrl, formatTimeAgo)
    const getAvatarUrl = (path: string | null | undefined) => {
        if (!path) return `${GATEWAY_URL}/uploads/default_avatar_thumb.png`;
        const formattedPath = path.startsWith('/') ? path : `/${path}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    const formatTimeAgo = (dateString: string) => {
        const diff = Date.now() - new Date(dateString).getTime();
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        if (days > 30) return `${Math.floor(days / 30)} tháng`;
        if (days > 0) return `${days} ngày`;
        return "hôm nay";
    };


    return (
        <section className="comment-section">
            <h3>Bình luận ({totalCommentCount})</h3>

            <div className="comment-input-area">
                <p className="report-link">
                    Báo cáo bình luận không phù hợp ở <a>đây</a>
                </p>

                {user ? (
                    <div className="comment-editor">
                        <textarea
                            placeholder="Viết bình luận của bạn..."
                            value={newComment}
                            onChange={(e) => setNewComment(e.target.value)}
                            disabled={isLoading}
                        />
                        <div className="comment-actions">
                            <button
                                onClick={handleSubmitComment}
                                disabled={isLoading || newComment.trim() === ''}
                            >
                                Đăng bình luận
                            </button>
                        </div>
                    </div>
                ) : (
                    <div className="comment-login-prompt">
                        Bạn cần <Link to="/login">Đăng nhập</Link> hoặc <Link to="/register">Đăng ký</Link> để bình luận.
                    </div>
                )}
            </div>

            <div className="comment-list-header">
                {isLoading ? 'Loading...' : `${comments.length} Bình luận`}
            </div>

            {error && <div className="comment-error">{error}</div>}

            <div className="comment-list">
                {comments.length === 0 && !isLoading ? (
                    <p className="no-comments">Chưa có bình luận nào cho mục này.</p>
                ) : (
                    comments.map(comment => (
                        <div key={comment.commentId} className="comment-item">
                            <img
                                src={getAvatarUrl(comment.userAvatarThumbnail)}
                                alt={comment.userName}
                                className="comment-avatar"
                            />
                            <div className="comment-content-wrapper">
                                <div className="comment-author">{comment.userName || 'Người dùng đã xóa'}</div>
                                <p className="comment-text">{comment.content}</p>
                                <div className="comment-meta">
                                    <span>{formatTimeAgo(comment.createdAt)}</span>
                                    <span>{comment.replyCount > 0 ? `(${comment.replyCount} trả lời)` : ''}</span>
                                    <span className="comment-action-link">Thích</span>
                                    <span className="comment-action-link">Trả lời</span>
                                </div>
                            </div>
                        </div>
                    ))
                )}
            </div>

            {totalPages > 1 && (
                <div style={{ display: 'flex', justifyContent: 'center', padding: '15px 0' }}>
                    <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        onPageChange={setCurrentPage}
                    />
                </div>
            )}
        </section>
    );
};

export default GeneralCommentSection;