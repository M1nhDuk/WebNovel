import React, { useState } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES, GATEWAY_URL } from '../../api/apiRoutes'; 
import type { CreateNovelDto, NovelDetailDto } from '../../types/series';
import '../CreateSeriesPage/CreateSeriesPage.css'; // Tái sử dụng CSS
import { FaUpload } from 'react-icons/fa';

interface AddNovelFormProps {
    seriesId: number;
    onNovelCreated: () => void; 
}

const AddNovelForm: React.FC<AddNovelFormProps> = ({ seriesId, onNovelCreated }) => {

    const [title, setTitle] = useState('');
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [coverPreview, setCoverPreview] = useState<string | null>(null);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

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
                setCoverPreview(null);
            }
        }
    };

    // Hàm reset form
    const resetForm = () => {
        setTitle('');
        setSelectedFile(null);
        setCoverPreview(null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setSuccess(null);

        if (!title.trim()) {
            setError("Volume title cannot be empty.");
            return;
        }

        setLoading(true);

        const createPayload: CreateNovelDto = {
            title: title.trim(),
            series_Id: seriesId,
            cover_images: null 
        };

        let newNovelId: number;

        try {
            const response = await apiClient.post<NovelDetailDto>(
                API_ROUTES.SERIES.CREATE_NOVEL(seriesId),
                createPayload
            );
            newNovelId = response.data.novel_Id; 

        } catch (err: any) {
            console.error("Failed to create volume:", err);
            setError(err.response?.data?.message || "An error occurred while creating the volume.");
            setLoading(false);
            return;
        }

       
        if (selectedFile) {
            setSuccess(`Volume "${title.trim()}" created. Now uploading cover...`);
            const uploadData = new FormData();
            uploadData.append('file', selectedFile);

            try {
                
                await apiClient.post(
                    API_ROUTES.SERIES.UPLOAD_NOVEL_COVER(seriesId, newNovelId),
                    uploadData,
                    { headers: { 'Content-Type': 'multipart/form-data' } }
                );
            } catch (err: any) {
                console.error("Failed to upload cover:", err);              
                setError(`Volume created, but cover upload failed: ${err.response?.data?.message}`);
                setLoading(false);
               
            }
        }

       
        setLoading(false);
        setSuccess(`Volume "${title.trim()}" created successfully!`);
        resetForm(); // Xóa form
        
    };

   
    const previewSrc = coverPreview || `${GATEWAY_URL}/images/covers/default_cover.jpg`;

    return (
        <form onSubmit={handleSubmit} className="create-series-form">
            <h2>Add New Volume</h2>

            {error && <div className="form-message error">{error}</div>}
            {success && <div className="form-message success">{success}</div>}

            <div className="form-group">
                <label htmlFor="volumeTitle">Volume Title <span>*</span></label>
                <input
                    type="text"
                    id="volumeTitle"
                    name="title"
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    disabled={loading}
                    autoFocus
                />
            </div>

            {/* --- Trường Cover Image  --- */}
            <div className="form-group">
                <label>Cover Image</label>
                <div className="cover-upload-wrapper">
                    <img
                        src={previewSrc}
                        alt="Cover preview"
                        className="cover-preview"
                    />
                    <label htmlFor="cover-upload-input" className="cover-upload-button">
                        <FaUpload /> {selectedFile ? 'Change Image' : 'Choose Image'}
                    </label>
                    <input
                        id="cover-upload-input"
                        type="file"
                        accept="image/png, image/jpeg"
                        style={{ display: 'none' }}
                        onChange={handleFileChange} 
                        disabled={loading} // Bị khóa khi đang submit
                    />
                </div>
                <small style={{ color: 'var(--text-secondary)', marginTop: '5px' }}>
                    You can add a cover image now, or leave it for default.
                </small>
            </div>
           

            <div className="form-actions">
                <button type="submit" disabled={loading}>
                    {loading ? 'Creating...' : 'Create Volume'}
                </button>
            </div>
        </form>
    );
};

export default AddNovelForm;