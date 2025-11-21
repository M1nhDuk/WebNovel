import React, { useState, useEffect, useCallback, Fragment } from 'react';
import apiClient from '../../../api/apiClient';
import type { CommentDto, CommentSectionProps, UpdateCommentDto, ReplyState, CommentContentProps } from '../../../types/comments';
import type { PagedResult } from '../../../types/series';
import { useAuth } from '../../../hooks/useAuth';
import Pagination from '../../common/Pagination';
import '../CSS/CommentSection.css';
import { Link } from 'react-router-dom';
import CommentReplyInput from './CommentReplyInput';
import { API_ROUTES } from '../../../api/apiRoutes';


const CommentContent: React.FC<CommentContentProps> = ({ content }) => {
    const [isExpanded, setIsExpanded] = useState(false);
    const MAX_LENGTH = 300;

    const renderText = (text: string) => {
        const mentionRegex = /^(@[^\s:]+:)/;
        const match = text.match(mentionRegex);

        if (match) {
            const mention = match[0];
            const restOfText = text.substring(mention.length);
            return (
                <>
                    <span className="comment-mention">{mention}</span>
                    {restOfText}
                </>
            );
        }
        return text;
    };

    const shouldTruncate = content.length > MAX_LENGTH;
    const displayedContent = isExpanded ? content : content.slice(0, MAX_LENGTH);

    return (
        <div className="comment-text-container">
            <p className="comment-text">
                {renderText(displayedContent)}
                {!isExpanded && shouldTruncate && '...'}
            </p>

            {shouldTruncate && (
                <button
                    className="comment-toggle-btn"
                    onClick={() => setIsExpanded(!isExpanded)}
                >
                    {isExpanded ? "Less" : "More"}
                </button>
            )}
        </div>
    );
};


const PAGE_SIZE = 8;
const REPLY_PAGE_SIZE = 5;
const GATEWAY_URL = 'https://localhost:8000';



const GeneralCommentSection: React.FC<CommentSectionProps> = ({
    seriesId,
    chapterId
}) => {
    const { user } = useAuth();
    const [comments, setComments] = useState<CommentDto[]>([]);
    const [newComment, setNewComment] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [internalTotalCount, setInternalTotalCount] = useState(0);


    // State lưu trữ các reply đã tải cho từng comment cha
    const [replyStates, setReplyStates] = useState<Record<string, ReplyState>>({});

    const isSeriesComment = !!seriesId;
    const targetId = isSeriesComment ? seriesId : chapterId;
    const targetType = isSeriesComment ? 'series' : 'chapters';

    const [replyingToId, setReplyingToId] = useState<string | null>(null);
    const [editingComment, setEditingComment] = useState<{ id: string, content: string } | null>(null);

    const isAdmin = user && user.role === 'Admin';



    if (!targetId) {
        return <div className="comment-section">Error: Missing Series ID or Chapter ID.</div>;
    }



    // Hàm fetchComments (Root comments)
    const fetchComments = useCallback(async (page: number) => {
        setIsLoading(true);
        setError(null);
        try {
            const endpoint = `/${targetType}/${targetId}/comments`;
            const response = await apiClient.get<PagedResult<CommentDto>>(
                endpoint,
                { params: { page, size: PAGE_SIZE } }
            );
            setComments(response.data.items);
            setCurrentPage(response.data.pageNumber);
            setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));
            setInternalTotalCount(response.data.totalRecords);

            // Reset trạng thái reply khi chuyển trang root comment
            setReplyStates({});
        } catch (err: any) {
            setError(err.response?.data?.message || `Failed to load comments for ${targetType}.`);
        } finally {
            setIsLoading(false);
        }
    }, [targetId, targetType]);



    useEffect(() => {
        fetchComments(currentPage);
    }, [fetchComments, currentPage]);



    // Hàm fetch replies từ server (Phân trang phía Server)
    const handleFetchReplies = async (commentId: string) => {

        // Lấy state hiện tại hoặc khởi tạo mặc định
        const currentState = replyStates[commentId] || {
            items: [], nextPage: 1, isLoading: false, isFullyLoaded: false
        };

        //Loading
        setReplyStates(prev => ({
            ...prev,
            [commentId]: { ...currentState, isLoading: true }
        }));

        try {
            // Gọi API lấy danh sách reply cho commentId này
            const response = await apiClient.get<PagedResult<CommentDto>>(`/comments/${commentId}/replies`, {
                params: { page: currentState.nextPage, size: REPLY_PAGE_SIZE }
            });

            const newItems = response.data.items;
            const totalRecords = response.data.totalRecords;

            setReplyStates(prev => {
                // Nếu page > 1 thì nối thêm vào danh sách cũ, ngược lại thay thế (trường hợp load lại từ đầu)
                const combinedItems = currentState.nextPage === 1
                    ? newItems
                    : [...currentState.items, ...newItems];

                return {
                    ...prev,
                    [commentId]: {
                        items: combinedItems,
                        nextPage: currentState.nextPage + 1,
                        isLoading: false,
                        // Đã load hết nếu số lượng item >= tổng số bản ghi trong DB
                        isFullyLoaded: combinedItems.length >= totalRecords
                    }
                };
            });
        } catch (err) {
            console.error("Failed to load replies", err);
            setReplyStates(prev => ({
                ...prev,
                [commentId]: { ...currentState, isLoading: false }
            }));
        }
    };



    // Hàm thu gọn danh sách reply
    const handleCollapseReplies = (commentId: string) => {
        setReplyStates(prev => {
            const newState = { ...prev };
            delete newState[commentId];
            return newState;
        });
    };

    // --- Xử lý khi reply thành công để cập nhật UI ---
    const handleReplySuccess = (rootCommentId: string) => {
        setReplyingToId(null);

        //Cập nhật số lượng reply trong state comments ngay lập tức
        setComments(prevComments => prevComments.map(c => {
            if (c.commentId === rootCommentId) {
                return { ...c, replyCount: (c.replyCount || 0) + 1 };
            }
            return c;
        }));

        //Reset và load lại danh sách reply của comment đó để hiện reply mới nhất
        handleCollapseReplies(rootCommentId);

        setTimeout(() => {
            handleFetchReplies(rootCommentId);
        }, 50);
    };

    const handleSubmitComment = async () => {
        if (!user || newComment.trim() === '') return;
        try {
            const endpoint = `/${targetType}/${targetId}/comments`;
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

    const handleDeleteComment = async (commentId: string, parentCommentId?: string | null) => {
        if (!window.confirm("Are you sure you want to delete this comment?")) return;

        setError(null);
        try {
            await apiClient.delete(API_ROUTES.COMMENTS.DELETE(commentId));

            if (parentCommentId) {
                // Nếu xóa reply -> cập nhật lại state reply của cha và giảm count
                setComments(prevComments => prevComments.map(c => {
                    if (c.commentId === parentCommentId) {
                        return { ...c, replyCount: Math.max((c.replyCount || 0) - 1, 0) };
                    }
                    return c;
                }));

                // Reload replies của comment cha
                handleCollapseReplies(parentCommentId);
                setTimeout(() => handleFetchReplies(parentCommentId), 50);
            } else {
                // Nếu xóa comment gốc -> reload cả trang
                fetchComments(currentPage);
            }
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to delete comment.");
        }
    };

    const handleStartEdit = (comment: CommentDto) => {
        setEditingComment({ id: comment.commentId, content: comment.content });
        setReplyingToId(null);
    };

    const handleCancelEdit = () => {
        setEditingComment(null);
    };

    const handleSaveEdit = async (parentCommentId?: string | null) => {
        if (!editingComment) return;
        setError(null);
        setIsLoading(true);

        const payload: UpdateCommentDto = {
            content: editingComment.content
        };

        try {
            await apiClient.put(API_ROUTES.COMMENTS.UPDATE(editingComment.id), payload);
            setEditingComment(null);

            if (parentCommentId) {
                // Nếu edit reply, reload lại list reply của comment cha
                handleCollapseReplies(parentCommentId);
                setTimeout(() => handleFetchReplies(parentCommentId), 50);
            } else {
                fetchComments(currentPage);
            }
        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to save comment.");
        } finally {
            setIsLoading(false);
        }
    };

    // Hàm bật/tắt box reply
    const handleToggleReply = (commentId: string) => {
        setEditingComment(null);
        if (replyingToId === commentId) {
            setReplyingToId(null);
        } else {
            setReplyingToId(commentId);
        }
    };

    const getAvatarUrl = (path: string | null | undefined) => {
        if (!path) return `${GATEWAY_URL}/uploads/default_avatar_thumb.png`;
        const formattedPath = path.startsWith('/') ? path : `/${path}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };


    const formatTimeAgo = (dateString: string) => {
        const diff = Date.now() - new Date(dateString).getTime();
        const seconds = Math.floor(diff / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);
        const months = Math.floor(days / 30);

        if (months > 0) return `${months} ${months > 1 ? 'months' : 'month'} ago`;
        if (days > 0) return `${days} ${days > 1 ? 'days' : 'day'} ago`;
        if (hours > 0) return `${hours} ${hours > 1 ? 'hours' : 'hour'} ago`;
        if (minutes > 0) return `${minutes} ${minutes > 1 ? 'minutes' : 'minute'} ago`;
        return "Just now";
    };

    return (
        <section className="comment-section">
            <h3>Comments ({internalTotalCount})</h3>

            <div className="comment-input-area">
                {user ? (
                    <div className="comment-editor">
                        <textarea
                            placeholder="Write your comment..."
                            value={newComment}
                            onChange={(e) => setNewComment(e.target.value)}
                            disabled={isLoading}
                        />
                        <div className="comment-actions">
                            <button
                                onClick={handleSubmitComment}
                                disabled={isLoading || newComment.trim() === ''}
                            >
                                Post Comment
                            </button>
                        </div>
                    </div>
                ) : (
                    <div className="comment-login-prompt">
                        You must <Link to="/login">Login</Link> or <Link to="/register">Register</Link> to comment.
                    </div>
                )}
            </div>

            <div className="comment-list-header">
                {isLoading ? 'Loading...' : `${internalTotalCount} ${internalTotalCount === 1 ? 'Comment' : 'Comments'}`}
            </div>

            {error && <div className="comment-error">{error}</div>}

            <div className="comment-list">
                {comments.length === 0 && !isLoading ? (
                    <p className="no-comments">There are no comments for this section yet.</p>
                ) : (
                    comments.map(comment => {

                        // Lấy thông tin state reply của comment này
                        const replyState = replyStates[comment.commentId];
                        const loadedReplies = replyState?.items || [];
                        const isLoadingReply = replyState?.isLoading;
                        const isFullyLoaded = replyState?.isFullyLoaded;

                        // Tổng số reply (Backend cần trả về field này trong CommentDto)
                        const totalRepliesInDB = comment.replyCount || 0;

                        const isReplyingToRoot = replyingToId === comment.commentId;
                        const isEditing = editingComment?.id === comment.commentId;
                        const isAuthor = user && user.userId === comment.userId;

                        return (
                            <Fragment key={comment.commentId}>

                                {/* --- ROOT COMMENT --- */}
                                <div className="comment-item">
                                    {/* 1. Header Row: Avatar + Name + Time */}
                                    <div className="comment-header-row">
                                        <img
                                            src={getAvatarUrl(comment.userAvatarThumbnail)}
                                            alt={comment.userName || 'User'}
                                            className="comment-header-avatar"
                                        />
                                        <div className="comment-header-info">
                                            <span className="comment-header-author-name">{comment.userName || 'Deleted User'}</span>
                                            <span className="comment-header-date">{formatTimeAgo(comment.createdAt)}</span>
                                        </div>
                                    </div>

                                    {/* 2. Body Wrapper: Content + Actions */}
                                    <div className="comment-body-wrapper">
                                        {isEditing ? (
                                            <div className="comment-editor">
                                                <textarea
                                                    value={editingComment.content}
                                                    onChange={(e) => setEditingComment({ ...editingComment, content: e.target.value })}
                                                    rows={4}
                                                    autoFocus
                                                    disabled={isLoading}
                                                />
                                                <div className="comment-actions" style={{ justifyContent: 'flex-start', gap: '10px' }}>
                                                    <button onClick={() => handleSaveEdit(null)} disabled={isLoading}>Save</button>
                                                    <button onClick={handleCancelEdit} className="comment-cancel-btn" disabled={isLoading}>Cancel</button>
                                                </div>
                                            </div>
                                        ) : (
                                            <CommentContent content={comment.content} />
                                        )}

                                        {!isEditing && (
                                            <div className="comment-meta">
                                                {/* Note: Date moved to header */}
                                                {!isAuthor && (
                                                    <span className="comment-action-link" onClick={() => handleToggleReply(comment.commentId)}>
                                                        {isReplyingToRoot ? 'Cancel' : 'Reply'}
                                                    </span>
                                                )}
                                                {isAuthor && (
                                                    <span className="comment-action-link" onClick={() => handleStartEdit(comment)}>Edit</span>
                                                )}
                                                {(isAuthor || isAdmin) && (
                                                    <span className="comment-action-link delete-action" onClick={() => handleDeleteComment(comment.commentId, null)}>Delete</span>
                                                )}
                                            </div>
                                        )}
                                    </div>
                                </div>

                                {isReplyingToRoot && (
                                    <div className="comment-replies-container">
                                        <CommentReplyInput
                                            targetId={targetId}
                                            targetType={targetType}
                                            parentCommentId={comment.commentId}
                                            authorUsername={comment.userName || 'User'}
                                            onCancel={() => handleToggleReply(comment.commentId)}
                                            onReplySuccess={() => handleReplySuccess(comment.commentId)}
                                        />
                                    </div>
                                )}

                                {/* --- LOADED REPLIES --- */}
                                {loadedReplies.length > 0 && (
                                    <div className="comment-replies-container">
                                        {loadedReplies.map(reply => {
                                            const isReplyingThisReply = replyingToId === reply.commentId;
                                            const isEditingReply = editingComment?.id === reply.commentId;
                                            const isReplyAuthor = user && user.userId === reply.userId;

                                            return (
                                                <Fragment key={reply.commentId}>
                                                    <div className="comment-item is-reply">
                                                        {/* Reply Header Row */}
                                                        <div className="comment-header-row">
                                                            <img
                                                                src={getAvatarUrl(reply.userAvatarThumbnail)}
                                                                alt={reply.userName || 'User'}
                                                                className="comment-header-avatar"
                                                            />
                                                            <div className="comment-header-info">
                                                                <span className="comment-header-author-name">{reply.userName || 'Deleted User'}</span>
                                                                <span className="comment-header-date">{formatTimeAgo(reply.createdAt)}</span>
                                                            </div>
                                                        </div>

                                                        {/* Reply Body Wrapper */}
                                                        <div className="comment-body-wrapper">
                                                            {isEditingReply ? (
                                                                <div className="comment-editor">
                                                                    <textarea
                                                                        value={editingComment.content}
                                                                        onChange={(e) => setEditingComment({ ...editingComment, content: e.target.value })}
                                                                        rows={4}
                                                                        autoFocus
                                                                        disabled={isLoading}
                                                                    />
                                                                    <div className="comment-actions" style={{ justifyContent: 'flex-start', gap: '10px' }}>
                                                                        <button onClick={() => handleSaveEdit(comment.commentId)} disabled={isLoading}>Save</button>
                                                                        <button onClick={handleCancelEdit} className="comment-cancel-btn" disabled={isLoading}>Cancel</button>
                                                                    </div>
                                                                </div>
                                                            ) : (
                                                                <CommentContent content={reply.content} />
                                                            )}

                                                            {!isEditingReply && (
                                                                <div className="comment-meta">
                                                                    {!isReplyAuthor && (
                                                                        <span className="comment-action-link" onClick={() => handleToggleReply(reply.commentId)}>
                                                                            {isReplyingThisReply ? 'Cancel' : 'Reply'}
                                                                        </span>
                                                                    )}
                                                                    {isReplyAuthor && (
                                                                        <span className="comment-action-link" onClick={() => handleStartEdit(reply)}>Edit</span>
                                                                    )}
                                                                    {(isReplyAuthor || isAdmin) && (
                                                                        <span className="comment-action-link delete-action" onClick={() => handleDeleteComment(reply.commentId, comment.commentId)}>Delete</span>
                                                                    )}
                                                                </div>
                                                            )}
                                                        </div>
                                                    </div>

                                                    {isReplyingThisReply && (
                                                        <div style={{ marginBottom: '15px' }}>
                                                            <CommentReplyInput
                                                                targetId={targetId}
                                                                targetType={targetType}
                                                                parentCommentId={reply.commentId}
                                                                authorUsername={reply.userName || 'User'}
                                                                onCancel={() => handleToggleReply(reply.commentId)}
                                                                onReplySuccess={() => handleReplySuccess(comment.commentId)}
                                                            />
                                                        </div>
                                                    )}
                                                </Fragment>
                                            );
                                        })}
                                    </div>
                                )}

                                {/* --- PAGINATION CONTROLS FOR REPLIES --- */}
                                <div className="comment-replies-container" style={{ marginTop: '5px', marginBottom: '15px' }}>
                                    {/* Nút Load More */}
                                    {totalRepliesInDB > 0 && !isFullyLoaded && (
                                        <span
                                            className="comment-action-link"
                                            style={{ fontWeight: 'bold', marginRight: '15px' }}
                                            onClick={() => handleFetchReplies(comment.commentId)}
                                        >
                                            {isLoadingReply
                                                ? "Loading..."
                                                : loadedReplies.length === 0
                                                    ? `View ${totalRepliesInDB} replies`
                                                    : `View more`
                                            }
                                        </span>
                                    )}

                                    {/* Nút Show Less */}
                                    {loadedReplies.length > 0 && (
                                        <span
                                            className="comment-action-link"
                                            style={{ fontWeight: 'bold', color: '#7f8c8d' }}
                                            onClick={() => handleCollapseReplies(comment.commentId)}
                                        >
                                            Show less
                                        </span>
                                    )}
                                </div>

                            </Fragment>
                        );
                    })
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