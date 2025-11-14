import React, { useState } from 'react';
import apiClient from '../../api/apiClient';
import type { ChapterDetailDto, AddChapterFormProps } from '../../types/series';
import '../CreateSeriesPage/CreateSeriesPage.css'; 
import { API_ROUTES } from '../../api/apiRoutes'; 


const AddChapterForm: React.FC<AddChapterFormProps> = ({
    seriesId,
    novelId,
    seriesType,
    onChapterCreated
}) => {

    const [title, setTitle] = useState('');
    const [content, setContent] = useState('');

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const getApiEndpoint = () => {
        if (seriesType === 'TRADITIONAL') {
            return API_ROUTES.SERIES.CREATE_CHAPTER_FOR_SERIES(seriesId);
        }
        if (novelId) {
            return API_ROUTES.SERIES.CREATE_CHAPTER_FOR_NOVEL(novelId);
        }
        return null;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setSuccess(null);

        if (!title.trim()) {
            setError("Chapter title cannot be empty.");
            return;
        }
        if (!content.trim()) {
            setError("Chapter content cannot be empty.");
            return;
        }

        const endpoint = getApiEndpoint();
        if (!endpoint) {
            setError("Configuration error: Cannot determine API endpoint.");
            return;
        }

        setLoading(true);

        // Payload dựa trên ChapterCreateDto
        const createPayload = {
            title: title.trim(),
            content: content.trim()
        };

        try {
            const response = await apiClient.post<ChapterDetailDto>(
                endpoint,
                createPayload
            );

            setSuccess(`Chapter "${title.trim()}" created successfully!`);
            setTitle('');
            setContent('');

            //ManageSeriesPage tải lại dữ liệu
            onChapterCreated(response.data);

        } catch (err: any) {
            console.error("Failed to create chapter:", err);
            setError(err.response?.data?.message || "An error occurred while creating the chapter.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="create-series-form">
            <h2>
                Add New Chapter
                {seriesType === 'Series' && novelId ? ` (Volume)` : ` (Series)`}
            </h2>

            {error && <div className="form-message error">{error}</div>}
            {success && <div className="form-message success">{success}</div>}

            <div className="form-group">
                <label htmlFor="chapterTitle">Chapter Title <span>*</span></label>
                <input
                    type="text"
                    id="chapterTitle"
                    name="title"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    disabled={loading}
                    autoFocus
                />
            </div>

            <div className="form-group">
                <label htmlFor="chapterContent">Content <span>*</span></label>
                <textarea
                    id="chapterContent"
                    name="content"
                    rows={20} // Tăng số hàng cho nội dung
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                    disabled={loading}
                ></textarea>
                <small style={{ color: 'var(--text-secondary)', marginTop: '5px' }}>
                    Nội dung chương (văn bản thuần túy hoặc markdown).
                </small>
            </div>

            <div className="form-actions">
                <button type="submit" disabled={loading}>
                    {loading ? 'Creating...' : 'Create Chapter'}
                </button>
            </div>
        </form>
    );
};

export default AddChapterForm;