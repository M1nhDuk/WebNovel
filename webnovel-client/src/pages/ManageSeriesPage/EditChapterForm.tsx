import React, { useState, useEffect } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { EditChapterFormProps, FullChapterDto } from '../../types/series';
import '../CreateSeriesPage/CreateSeriesPage.css';


const EditChapterForm: React.FC<EditChapterFormProps> = ({
    seriesId,
    novelId,
    chapterId,
    seriesType,
    onChapterUpdated,
    onCancel
}) => {
    const [title, setTitle] = useState('');
    const [content, setContent] = useState('');

    const [loading, setLoading] = useState(true); 
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    // Xác định API endpoint (GET và PUT)
    const getApiEndpoints = () => {
        if (seriesType === 'TRADITIONAL') {
            // Flow 2: ClassicSeries -> Chapter
            const route = API_ROUTES.SERIES.CHAPTER_FOR_SERIES(seriesId, chapterId);
            return { get: route, put: route };
        }
        if (novelId) {
            // Flow 1: Series -> Novel -> Chapter
            const route = API_ROUTES.SERIES.CHAPTER_FOR_NOVEL(novelId, chapterId);
            return { get: route, put: route };
        }
        return null;
    };

    //Fetch dữ liệu đầy đủ của Chapter
    useEffect(() => {
        const fetchChapterContent = async () => {
            setError(null);
            setLoading(true);

            const endpoints = getApiEndpoints();
            if (!endpoints) {
                setError("Configuration error: Cannot determine API endpoint.");
                setLoading(false);
                return;
            }

            try {
                const response = await apiClient.get<FullChapterDto>(endpoints.get);
                setTitle(response.data.title);
                setContent(response.data.content);
            } catch (err: any) {
                console.error("Failed to fetch chapter details:", err);
                setError(err.response?.data?.message || "Could not load chapter content.");
            } finally {
                setLoading(false);
            }
        };

        fetchChapterContent();
    }, [seriesId, novelId, chapterId, seriesType]);

    //PUT dữ liệu đã cập nhật
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

        const endpoints = getApiEndpoints();
        if (!endpoints) {
            setError("Configuration error: Cannot determine API endpoint.");
            return;
        }

        setLoading(true);

        const updatePayload = {
            title: title.trim(),
            content: content.trim()
        };

        try {
            await apiClient.put(endpoints.put, updatePayload);
            setSuccess(`Chapter "${title.trim()}" updated successfully!`);

            onChapterUpdated();

        } catch (err: any) {
            console.error("Failed to update chapter:", err);
            setError(err.response?.data?.message || "An error occurred while saving the chapter.");
        } finally {
            setLoading(false);
        }
    };

    // Giao diện
    return (
        <form onSubmit={handleSubmit} className="create-series-form">
            <h2>Edit Chapter</h2>

            {loading && !title && <p>Loading chapter content...</p>}

            {!loading && (
                <>
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
                            rows={20}
                            value={content}
                            onChange={(e) => setContent(e.target.value)}
                            disabled={loading}
                        ></textarea>
                    </div>

                    <div className="form-actions">
                        <button type="submit" disabled={loading}>
                            {loading ? 'Saving...' : 'Save Changes'}
                        </button>
                        <button type="button" className="cancel-btn" onClick={onCancel} disabled={loading}>
                            Cancel
                        </button>
                    </div>
                </>
            )}
                {error && <div className="form-message error">{error}</div>}
                {success && <div className="form-message success">{success}</div>}
        </form>
    );
};

export default EditChapterForm;