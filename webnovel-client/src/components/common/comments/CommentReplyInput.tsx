import React, { useState } from 'react';
import apiClient from '../../../api/apiClient';
import { useAuth } from '../../../hooks/useAuth';
import { Link } from 'react-router-dom';
import type { CommentReplyInputProps } from '../../../types/comments';


const CommentReplyInput: React.FC<CommentReplyInputProps> = ({
    targetId,
    targetType,
    parentCommentId,
    authorUsername,
    onReplySuccess,
    onCancel
}) => {
    const { user } = useAuth();
    // Pre-fill @mention
    const [replyContent, setReplyContent] = useState(`@${authorUsername} `);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmitReply = async () => {
        if (!user || replyContent.trim() === `@${authorUsername}` || replyContent.trim() === '') {
            setError("Reply cannot be empty.");
            return;
        }

        setIsLoading(true);
        setError(null);

        try {
            const endpoint = `/${targetType}/${targetId}/comments`;

            await apiClient.post(endpoint, {
                content: replyContent.trim(),
                parentCommentId: parentCommentId 
            });

            onReplySuccess(); 

        } catch (err: any) {
            setError(err.response?.data?.message || "Failed to post reply.");
        } finally {
            setIsLoading(false);
        }
    };

    if (!user) {
        return (
            <div className="comment-login-prompt" style={{ margin: '10px 0 0 55px' }}>
                Bạn cần <Link to="/login">Đăng nhập</Link> để trả lời.
            </div>
        );
    }

    return (
        <div className="comment-reply-box" style={{ marginLeft: '55px', marginTop: '10px' }}>
            {error && <div className="comment-error" style={{ fontSize: '0.9rem', marginBottom: '5px' }}>{error}</div>}
            <div className="comment-editor">
                <textarea
                    placeholder={`Replying to ${authorUsername}...`}
                    value={replyContent}
                    onChange={(e) => setReplyContent(e.target.value)}
                    disabled={isLoading}
                    autoFocus 
                    rows={4}
                />
                <div className="comment-actions" style={{ justifyContent: 'flex-start' }}>
                    <button
                        onClick={handleSubmitReply}
                        disabled={isLoading || replyContent.trim() === ''}
                    >
                        Reply
                    </button>
                    <button
                        type="button"
                        className="comment-cancel-btn" 
                        onClick={onCancel}
                        disabled={isLoading}
                    >
                        Cancel
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CommentReplyInput;