import React, { useState, useEffect } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES, GATEWAY_URL } from '../../api/apiRoutes';
import type { NovelDetailDto, NovelUpdateDto } from '../../types/series';
import '../CreateSeriesPage/CreateSeriesPage.css';
import { FaUpload } from 'react-icons/fa';

interface EditNovelFormProps {
    seriesId: number;
    novel: NovelDetailDto;
    onNovelUpdated: () => void;
    onCancel: () => void;
}

const EditNovelForm: React.FC<EditNovelFormProps> = ({ seriesId, novel, onNovelUpdated, onCancel }) => {

    // State cho tiêu đề
    const [title, setTitle] = useState(novel.title);

    // State cho file và ảnh preview
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [coverPreview, setCoverPreview] = useState<string | null>(null);

    // State cho loading và thông báo
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    // Helper lấy URL ảnh 
    const getImageUrl = (coverPath: string | undefined | null) => {
        if (!coverPath) {
            return `${GATEWAY_URL}/images/covers/default_cover.jpg`;
        }
        const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    // useEffect để cập nhật form khi prop `novel` thay đổi 
    useEffect(() => {
        setTitle(novel.title);
        setCoverPreview(getImageUrl(novel.cover_images));

        // Reset trạng thái khi đổi novel
        setSelectedFile(null);
        setError(null);
        setSuccess(null);
    }, [novel]);


    // Xử lý khi người dùng chọn file
    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            if (file.type === "image/jpeg" || file.type === "image/png") {
                setSelectedFile(file);
                setCoverPreview(URL.createObjectURL(file));
                setError(null);
            } else {
                setError("Invalid file type. Please select a JPG or PNG image.");
                setSelectedFile(null);
            }
        }
    };

    // Xử lý khi người dùng thay đổi tiêu đề
    const handleTitleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setTitle(e.target.value);
    };

    // Xử lý khi submit form
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setSuccess(null);

        if (!title.trim()) {
            setError("Volume title cannot be empty.");
            return;
        }

        setLoading(true);

        const updatePayload: NovelUpdateDto = {
            title: title.trim(),
            cover_images: novel.cover_images
        };

        try {
            await apiClient.put(
                `/series/${seriesId}/novels/${novel.novel_Id}`,
                updatePayload
            );
        } catch (err: any) {
            console.error("Failed to update volume details:", err);
            setError(err.response?.data?.message || "An error occurred while saving details.");
            setLoading(false);
            return;
        }

        // Nếu có file mới, upload file đó
        if (selectedFile) {
            setSuccess(`Details saved. Now uploading new cover...`);
            const uploadData = new FormData();
            uploadData.append('file', selectedFile);

            try {
                await apiClient.post(
                    API_ROUTES.SERIES.UPLOAD_NOVEL_COVER(seriesId, novel.novel_Id),
                    uploadData
                );
            } catch (err: any) {
                console.error("Failed to upload cover:", err);
                setError(`Volume details saved, but cover upload failed: ${err.response?.data?.message || err.message}`);
                setLoading(false);
                onNovelUpdated();
                return;
            }
        }

        setLoading(false);
        setSuccess(`Volume "${title.trim()}" updated successfully!`);
        setSelectedFile(null);
        onNovelUpdated();
    };

    // Đảm bảo cover images không bao giờ null
    const previewSrc = coverPreview || `${GATEWAY_URL}/images/covers/default_cover.jpg`;

    return (
        <form onSubmit={handleSubmit} className="create-series-form">
            <h2>Edit Volume</h2>

            {error && <div className="form-message error">{error}</div>}
            {success && <div className="form-message success">{success}</div>}

            <div className="form-group">
                <label htmlFor="volumeTitle">Volume Title <span>*</span></label>
                <input
                    type="text"
                    id="volumeTitle"
                    name="title"
                    value={title}
                    onChange={handleTitleChange}
                    disabled={loading}
                    autoFocus
                />
            </div>

            {/* --- Cover Image --- */}
            <div className="form-group">
                <label>Cover Image</label>
                <div className="cover-upload-wrapper">
                    <img
                        src={previewSrc}
                        alt="Cover preview"
                        className="cover-preview"
                    />
                    <label htmlFor="cover-upload-input-edit" className="cover-upload-button">
                        <FaUpload /> {selectedFile ? 'Change Image' : 'Choose Image'}
                    </label>
                    <input
                        id="cover-upload-input-edit"
                        type="file"
                        accept="image/png, image/jpeg"
                        style={{ display: 'none' }}
                        onChange={handleFileChange}
                        disabled={loading}
                    />
                </div>
            </div>

            <div className="form-actions">
                <button type="submit" disabled={loading}>
                    {loading ? 'Saving...' : 'Save Changes'}
                </button>
                <button type="button" className="cancel-btn" onClick={onCancel} disabled={loading}>
                    Cancel
                </button>
            </div>
        </form>
    );
};

export default EditNovelForm;