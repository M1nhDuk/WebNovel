import React, { useState, useRef } from 'react';
import apiClient from '../../api/apiClient';
import { useAuth } from '../../hooks/useAuth';
import { FaCamera } from 'react-icons/fa';
import './ImageUploadButton.css';

interface ImageUploadButtonProps {
    apiEndpoint: string; 
}

const ImageUploadButton: React.FC<ImageUploadButtonProps> = ({ apiEndpoint }) => {
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const { refreshUserProfile } = useAuth(); 

    //Xử lý khi người dùng chọn file
    const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) return;

        setIsLoading(true);
        setError(null);

        const formData = new FormData();
        formData.append('file', file);

        try {
            //Gọi API upload
            await apiClient.post(apiEndpoint, formData, {
                headers: { 'Content-Type': 'multipart/form-data' },
            });
            
            //Yêu cầu AuthContext tải lại ảnh vừa upload
            await refreshUserProfile(); 
        } catch (err: any) {
            console.error("Upload failed:", err);
            setError(err.response?.data?.message || "Upload failed.");
        } finally {
            setIsLoading(false);
            // Reset input upload ảnh
            if (fileInputRef.current) {
                fileInputRef.current.value = "";
            }
        }
    };

    //Kích hoạt input file ẩn khi bấm nút
    const handleClick = () => {
        fileInputRef.current?.click();
    };

    return (
        <div className="image-upload-container">
            <button 
                onClick={handleClick} 
                disabled={isLoading} 
                className="upload-btn-icon"
                title={isLoading ? "Uploading..." : "Change image"}
            >
                {isLoading ? '...' : <FaCamera />}
            </button>
            <input
                type="file"
                ref={fileInputRef}
                onChange={handleFileChange}
                style={{ display: 'none' }}
                accept="image/png, image/jpeg, image/gif" 
            />
            {error && <span className="upload-error">{error}</span>}
        </div>
    );
};

export default ImageUploadButton;