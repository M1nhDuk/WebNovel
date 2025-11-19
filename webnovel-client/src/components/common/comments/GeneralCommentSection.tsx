import React, { useState, useEffect, useCallback, Fragment } from 'react';
import apiClient from '../../../api/apiClient';
import type { CommentDto, CommentSectionProps, UpdateCommentDto } from '../../../types/comments';
import type { PagedResult } from '../../../types/series';
import { useAuth } from '../../../hooks/useAuth';
import Pagination from '../../common/Pagination';
import '../CSS/CommentSection.css';
import { Link } from 'react-router-dom';
import CommentReplyInput from './CommentReplyInput';
import { API_ROUTES } from '../../../api/apiRoutes';

interface CommentContentProps {
    content: string;
}


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
                    {isExpanded ? "Rút gọn" : "Xem thêm"}
                </button>
            )}
        </div>
    );
};


const PAGE_SIZE = 8;
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

    const isSeriesComment = !!seriesId;
    const targetId = isSeriesComment ? seriesId : chapterId;
    const targetType = isSeriesComment ? 'series' : 'chapters';

    const [replyingToId, setReplyingToId] = useState<string | null>(null);
    const [editingComment, setEditingComment] = useState<{ id: string, content: string } | null>(null);

    const isAdmin = user && user.role === 'Admin';

    if (!targetId) {
        return <div className="comment-section">Error: Missing Series ID or Chapter ID.</div>;
    }

    // Hàm fetchComments
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


    const handleDeleteComment = async (commentId: string) => {
        if (!window.confirm("Are you sure you want to delete this comment?")) return;

        setError(null);
        try {
            await apiClient.delete(API_ROUTES.COMMENTS.DELETE(commentId));
            fetchComments(currentPage);
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

    const handleSaveEdit = async () => {
        if (!editingComment) return;

        setError(null);
        setIsLoading(true); 

        const payload: UpdateCommentDto = {
            content: editingComment.content
        };

        try {
            await apiClient.put(API_ROUTES.COMMENTS.UPDATE(editingComment.id), payload);
            setEditingComment(null);
            fetchComments(currentPage); 
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

    // Hàm helper lấy avatar
    const getAvatarUrl = (path: string | null | undefined) => {
        if (!path) return `${GATEWAY_URL}/uploads/default_avatar_thumb.png`;
        const formattedPath = path.startsWith('/') ? path : `/${path}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    // Hàm helper format thời gian
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

   
    const flattenReplies = (replies: CommentDto[]): CommentDto[] => {
        const flatList: CommentDto[] = [];
        const queue: CommentDto[] = [...replies].sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());

        while (queue.length > 0) {
            const current = queue.shift();
            if (current) {
                flatList.push(current);
                if (current.replies && current.replies.length > 0) {
                    queue.push(...current.replies.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()));
                }
            }
        }
        return flatList;
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
                        const allReplies = flattenReplies(comment.replies);
                        const isReplyingToRoot = replyingToId === comment.commentId;

                        //Logic check quyền và trạng thái edit
                        const isEditing = editingComment?.id === comment.commentId;
                        const isAuthor = user && user.userId === comment.userId;

                        return (
                            <Fragment key={comment.commentId}>

                                <div className="comment-item">
                                    <img
                                        src={getAvatarUrl(comment.userAvatarThumbnail)}
                                        alt={comment.userName || 'User'}
                                        className="comment-avatar"
                                    />
                                    <div className="comment-content-wrapper">
                                        <div className="comment-author">{comment.userName || 'Deleted User'}</div>

                                        {isEditing ? (
                                            <div className="comment-editor" style={{ marginTop: '5px' }}>
                                                <textarea
                                                    value={editingComment.content}
                                                    onChange={(e) => setEditingComment({ ...editingComment, content: e.target.value })}
                                                    rows={4}
                                                    autoFocus
                                                    disabled={isLoading}
                                                />
                                                <div className="comment-actions" style={{ justifyContent: 'flex-start', gap: '10px' }}>
                                                    <button onClick={handleSaveEdit} disabled={isLoading}>Save</button>
                                                    <button onClick={handleCancelEdit} className="comment-cancel-btn" disabled={isLoading}>Cancel</button>
                                                </div>
                                            </div>
                                        ) : (
                                                <CommentContent content={comment.content} />

                                        )}

                                        {!isEditing && (
                                            <div className="comment-meta">
                                                <span>{formatTimeAgo(comment.createdAt)}</span>

                                                {!isAuthor && (
                                                    <span
                                                        className="comment-action-link"
                                                        onClick={() => handleToggleReply(comment.commentId)}
                                                    >
                                                        {isReplyingToRoot ? 'Cancel' : 'Reply'}
                                                    </span>
                                                )}

                                                {/* NÚT EDIT/DELETE  */}
                                                {isAuthor && (
                                                    <span
                                                        className="comment-action-link"
                                                        onClick={() => handleStartEdit(comment)}
                                                    >
                                                        Edit
                                                    </span>
                                                )}
                                                {(isAuthor || isAdmin) && ( 
                                                    <span
                                                        className="comment-action-link"
                                                        style={{ color: '#e74c3c' }} 
                                                        onClick={() => handleDeleteComment(comment.commentId)}
                                                    >
                                                        Delete
                                                    </span>
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
                                            onReplySuccess={() => {
                                                setReplyingToId(null);
                                                fetchComments(currentPage);
                                            }}
                                        />
                                    </div>
                                )}


                                {allReplies.length > 0 && (
                                    <div className="comment-replies-container">
                                        {allReplies.map(reply => {
                                            const isReplyingThisReply = replyingToId === reply.commentId;

                                            //Logic check quyền và trạng thái edit
                                            const isEditingReply = editingComment?.id === reply.commentId;
                                            const isReplyAuthor = user && user.userId === reply.userId;

                                            return (
                                                <Fragment key={reply.commentId}>
                                                    <div className="comment-item is-reply">
                                                        <img
                                                            src={getAvatarUrl(reply.userAvatarThumbnail)}
                                                            alt={reply.userName || 'User'}
                                                            className="comment-avatar"
                                                        />
                                                        <div className="comment-content-wrapper">
                                                            <div className="comment-author">{reply.userName || 'Deleted User'}</div>


                                                            {isEditingReply ? (
                                                                <div className="comment-editor" style={{ marginTop: '5px' }}>
                                                                    <textarea
                                                                        value={editingComment.content}
                                                                        onChange={(e) => setEditingComment({ ...editingComment, content: e.target.value })}
                                                                        rows={4}
                                                                        autoFocus
                                                                        disabled={isLoading}
                                                                    />
                                                                    <div className="comment-actions" style={{ justifyContent: 'flex-start', gap: '10px' }}>
                                                                        <button onClick={handleSaveEdit} disabled={isLoading}>Save</button>
                                                                        <button onClick={handleCancelEdit} className="comment-cancel-btn" disabled={isLoading}>Cancel</button>
                                                                    </div>
                                                                </div>
                                                            ) : (
                                                                    <CommentContent content={reply.content} />
                                                            )}

                                                            {!isEditingReply && (
                                                                <div className="comment-meta">
                                                                    <span>{formatTimeAgo(reply.createdAt)}</span>

                                                                    {!isReplyAuthor && (
                                                                        <span
                                                                            className="comment-action-link"
                                                                            onClick={() => handleToggleReply(reply.commentId)}
                                                                        >
                                                                            {isReplyingThisReply ? 'Cancel' : 'Reply'}
                                                                        </span>
                                                                    )}

                                                                    {/* --- NÚT EDIT/DELETE --- */}
                                                                    {isReplyAuthor && (
                                                                        <span
                                                                            className="comment-action-link"
                                                                            onClick={() => handleStartEdit(reply)}
                                                                        >
                                                                            Edit
                                                                        </span>
                                                                    )}
                                                                    {(isReplyAuthor || isAdmin) && (
                                                                        <span
                                                                            className="comment-action-link"
                                                                            style={{ color: '#e74c3c' }}
                                                                            onClick={() => handleDeleteComment(reply.commentId)}
                                                                        >
                                                                            Delete
                                                                        </span>
                                                                    )}
                                                                </div>
                                                            )}
                                                        </div>
                                                    </div>


                                                    {isReplyingThisReply && (
                                                        <CommentReplyInput
                                                            targetId={targetId}
                                                            targetType={targetType}
                                                            parentCommentId={reply.commentId} 
                                                            authorUsername={reply.userName || 'User'}
                                                            onCancel={() => handleToggleReply(reply.commentId)}
                                                            onReplySuccess={() => {
                                                                setReplyingToId(null);
                                                                fetchComments(currentPage);
                                                            }}
                                                        />
                                                    )}
                                                </Fragment>
                                            );
                                        })}
                                    </div>
                                )}
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