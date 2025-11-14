import React, { useState, useEffect, useRef } from 'react';
import type {
    NovelSeriesDetailDto,
    UpdateClassicSeriesDto,
    CreateSeriesDto,
    CreateTraditionalSeriesDto,
    UpdateNovelServiceDto
} from '../../types/series';
import type { CategoryDto, NovelStatusDto, TagDto } from '../../types/filters';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import '../CreateSeriesPage/CreateSeriesPage.css';
import { FaUpload } from 'react-icons/fa';

const GATEWAY_URL = 'https://localhost:8000';

const isbn10Regex = /^[0-9]{9}[0-9xX]$/;
const isbn13Regex = /^(978|979)[0-9]{10}$/;

interface EditSeriesFormProps {
    series: NovelSeriesDetailDto;
    onSeriesUpdate: () => void;
}

type FormDataType = Partial<CreateTraditionalSeriesDto>;

const EditSeriesForm: React.FC<EditSeriesFormProps> = ({ series, onSeriesUpdate }) => {

    const [formData, setFormData] = useState<FormDataType>({});
    const [selectedTags, setSelectedTags] = useState<number[]>([]);


    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [statuses, setStatuses] = useState<NovelStatusDto[]>([]);
    const [allTags, setAllTags] = useState<TagDto[]>([]);

    const [searchTerm, setSearchTerm] = useState('');
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const tagContainerRef = useRef<HTMLDivElement>(null);

    const [loading, setLoading] = useState(false);
    const [submitError, setSubmitError] = useState<string | null>(null);
    const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);

    const [selectedFile, setSelectedFile] = useState<File | null>(null);

    const [coverPreview, setCoverPreview] = useState<string | null>(null);


    useEffect(() => {
        const fetchFiltersData = async () => {
            try {
                const [catRes, statusRes, tagRes] = await Promise.all([
                    apiClient.get<CategoryDto[]>(API_ROUTES.CATEGORY.GET_ALL),
                    apiClient.get<NovelStatusDto[]>(API_ROUTES.STATUS.GET_ALL),
                    apiClient.get<TagDto[]>(API_ROUTES.TAG.GET_ALL)
                ]);
                setCategories(catRes.data);
                setStatuses(statusRes.data);
                setAllTags(tagRes.data);
            } catch (err) {
                console.error("Failed to fetch filter metadata:", err);
                setSubmitError("Cannot load filter data.");
            }
        };
        fetchFiltersData();
    }, []);

    
    useEffect(() => {
        if (series && allTags.length > 0) {
            const seriesTagIds = series.tags
                .map(tagName => allTags.find(t => t.tagName === tagName)?.tagId)
                .filter((id): id is number => id !== undefined);

            let publishDate = (series as any).publish_date || '';
            if (publishDate) {
                publishDate = new Date(publishDate).toISOString().split('T')[0];
            }

            setFormData({
                series_title: series.series_title,
                author: series.author || '',
                artist: series.artist || '',
                description: series.description,
                note: series.note || '',
                category_id: series.category_id,
                status_id: series.status_id,
                ISBN_10: (series as any).ISBN_10 || '',
                ISBN_13: (series as any).ISBN_13 || '',
                publisher: (series as any).publisher || '',
                publish_date: publishDate,
                edition: (series as any).edition || '',
            });

            setSelectedTags(seriesTagIds);
        }
    }, [series, allTags]);

    
    useEffect(() => {
        const formattedPath = series.cover_images?.startsWith('/') ? series.cover_images : `/${series.cover_images}`;
        setCoverPreview(`${GATEWAY_URL}${formattedPath || '/images/covers/default_cover.jpg'}`);

       
        setSelectedFile(null);
        setSubmitSuccess(null);
        setSubmitError(null);

    }, [series.series_Id]); 




    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (tagContainerRef.current && !tagContainerRef.current.contains(event.target as Node)) {
                setIsDropdownOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [tagContainerRef]);


    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;

        if (name === 'ISBN_13' || name === 'ISBN_10') {
            const allowedChars = (name === 'ISBN_10') ? /[^0-9xX]/g : /[^0-9]/g;
            const cleanedValue = value.replace(allowedChars, '');

            setFormData(prev => ({
                ...prev,
                [name]: cleanedValue
            }));
        } else {
            setFormData(prev => ({
                ...prev,
                [name]: name === 'category_id' || name === 'status_id' ? parseInt(value, 10) : value
            }));
        }
    };


    const getTagById = (id: number): TagDto | undefined => allTags.find(t => t.tagId === id);

    const handleTagSelect = (tag: TagDto) => {
        if (!selectedTags.includes(tag.tagId)) {
            setSelectedTags(prev => [...prev, tag.tagId]);
        }
        setSearchTerm('');
        setIsDropdownOpen(true);
    };

    const handleTagRemove = (tagId: number) => {
        setSelectedTags(prev => prev.filter(id => id !== tagId));
    };

    const filteredTags = allTags.filter(tag =>
        tag.tagName.toLowerCase().includes(searchTerm.toLowerCase())
    );


    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            if (file.type === "image/jpeg" || file.type === "image/png") {
                setSelectedFile(file);
                setCoverPreview(URL.createObjectURL(file));
                setSubmitError(null);
            } else {
                setSubmitError("Invalid file type. Please select a JPG or PNG image.");
                setSelectedFile(null);
            }
        }
    };


    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!formData) return;

        setLoading(true);
        setSubmitError(null);
        setSubmitSuccess(null);

        //VALIDATION 
        if (!formData.series_title?.trim()) {
            setSubmitError("Title cannot be empty.");
            setLoading(false);
            return;
        }
        if (!formData.description?.trim()) {
            setSubmitError("Synopsis cannot be empty.");
            setLoading(false);
            return;
        }
        if (!formData.category_id || formData.category_id === 0) {
            setSubmitError("Please select a category.");
            setLoading(false);
            return;
        }
        if (!formData.status_id || formData.status_id === 0) {
            setSubmitError("Please select a status.");
            setLoading(false);
            return;
        }
        if (selectedTags.length === 0) {
            setSubmitError("Please select at least one tag.");
            setLoading(false);
            return;
        }
        if (series.type === 'TRADITIONAL') {
            if (!formData.ISBN_13?.trim()) {
                setSubmitError("ISBN-13 is required for Classical Novel.");
                setLoading(false);
                return;
            }
            if (!isbn13Regex.test(formData.ISBN_13.trim())) {
                setSubmitError("ISBN-13 must be 13 digits and start with 978 or 979.");
                setLoading(false);
                return;
            }
            if (formData.ISBN_10 && formData.ISBN_10.trim() !== '') {
                if (!isbn10Regex.test(formData.ISBN_10.trim())) {
                    setSubmitError("ISBN-10 must be 10 characters (9 digits + 1 digit or 'X').");
                    setLoading(false);
                    return;
                }
            }
        }

       
        if (selectedFile) {
            const uploadData = new FormData();
            uploadData.append('file', selectedFile);

            try {
                setSubmitSuccess("Uploading cover image..."); 
                const response = await apiClient.post(
                    API_ROUTES.SERIES.UPLOAD_COVER(series.series_Id),
                    uploadData,
                    { headers: { 'Content-Type': 'multipart/form-data' } }
                );

                const newCoverPath = response.data.coverUrl.startsWith('http')
                    ? new URL(response.data.coverUrl).pathname
                    : response.data.coverUrl;

               
                setCoverPreview(`${GATEWAY_URL}${newCoverPath}`);
                setSelectedFile(null); 

                setSubmitSuccess("Cover image updated! Saving details...");

            } catch (err: any) {
                setSubmitError(err.response?.data?.message || "Cover upload failed. Aborting save.");
                setLoading(false);
                return; 
            }
        }

        
        try {
            const basePayload: Omit<CreateSeriesDto, 'cover_images'> = {
                series_title: formData.series_title!,
                author: formData.author || null,
                artist: formData.artist || null,
                description: formData.description!,
                note: formData.note || null,
                category_id: formData.category_id!,
                status_id: formData.status_id!,
                TagIds: selectedTags
            };

            let finalPayload: UpdateClassicSeriesDto | UpdateNovelServiceDto;

            if (series.type === 'TRADITIONAL') {
                finalPayload = {
                    ...basePayload,
                    series_Id: series.series_Id,
                    ISBN_10: formData.ISBN_10 || null,
                    ISBN_13: formData.ISBN_13!,
                    publisher: formData.publisher || null,
                    publish_date: formData.publish_date || null,
                    edition: formData.edition || null,
                } as UpdateClassicSeriesDto;

                await apiClient.put(API_ROUTES.SERIES.UPDATE_CLASSIC_SERIES(series.series_Id), finalPayload);
            } else {
                finalPayload = {
                    ...basePayload,
                    series_Id: series.series_Id,
                } as UpdateNovelServiceDto;

                await apiClient.put(API_ROUTES.SERIES.UPDATE(series.series_Id), finalPayload);
            }

            setSubmitSuccess("Series details updated successfully!");

           
            onSeriesUpdate();

        } catch (err: any) {
            setSubmitError(err.response?.data?.message || "Failed to update series details.");
        } finally {
            setLoading(false);
        }
    };


    if (allTags.length === 0 || !formData.series_title) {
        return <div>Loading form data...</div>;
    }

    // JSX
    return (
        <form onSubmit={handleSubmit} className="create-series-form">
            <h2>Edit Series Details</h2>

            {submitError && <div className="form-message error">{submitError}</div>}
            {submitSuccess && <div className="form-message success">{submitSuccess}</div>}

            <div className="form-group">
                <label>Cover Image</label>
                <div className="cover-upload-wrapper">
                    {coverPreview && <img src={coverPreview} alt="Cover preview" className="cover-preview" />}
                    <label htmlFor="cover-upload-input" className="cover-upload-button">
                        <FaUpload /> {selectedFile ? 'Change Image' : (coverPreview && !coverPreview.includes('default_cover')) ? 'Change Image' : 'Choose Image'}
                    </label>
                    <input
                        id="cover-upload-input"
                        type="file"
                        accept="image/png, image/jpeg"
                        style={{ display: 'none' }}
                        onChange={handleFileChange}
                        disabled={loading}
                    />
                </div>
            </div>

            <div className="form-group">
                <label htmlFor="series_title">Title <span>*</span></label>
                <input
                    type="text" id="series_title" name="series_title"
                    value={formData.series_title || ''} onChange={handleInputChange} disabled={loading}
                />
            </div>

            <div className="form-row">
                <div className="form-group">
                    <label htmlFor="author">Author</label>
                    <input
                        type="text" id="author" name="author"
                        value={formData.author || ''} onChange={handleInputChange} disabled={loading}
                    />
                </div>
                <div className="form-group">
                    <label htmlFor="artist">Artist</label>
                    <input
                        type="text" id="artist" name="artist"
                        value={formData.artist || ''} onChange={handleInputChange} disabled={loading}
                    />
                </div>
            </div>

            {series.type === 'TRADITIONAL' && (
                <>
                    <div className="form-row">
                        <div className="form-group">
                            <label htmlFor="ISBN_13">ISBN-13 <span>*</span></label>
                            <input
                                type="text" id="ISBN_13" name="ISBN_13"
                                value={formData.ISBN_13 || ''} onChange={handleInputChange}
                                disabled={loading} maxLength={13}
                            />
                        </div>
                        <div className="form-group">
                            <label htmlFor="ISBN_10">ISBN-10</label>
                            <input
                                type="text" id="ISBN_10" name="ISBN_10"
                                value={formData.ISBN_10 || ''} onChange={handleInputChange}
                                disabled={loading} maxLength={10}
                            />
                        </div>
                    </div>
                    <div className="form-row">
                        <div className="form-group">
                            <label htmlFor="publisher">Publisher</label>
                            <input type="text" id="publisher" name="publisher" value={formData.publisher || ''} onChange={handleInputChange} disabled={loading} />
                        </div>
                        <div className="form-group">
                            <label htmlFor="edition">Edition</label>
                            <input type="text" id="edition" name="edition" value={formData.edition || ''} onChange={handleInputChange} disabled={loading} />
                        </div>
                    </div>
                    <div className="form-group">
                        <label htmlFor="publish_date">Publish Date</label>
                        <input
                            type="date" id="publish_date" name="publish_date"
                            value={formData.publish_date || ''} onChange={handleInputChange} disabled={loading}
                        />
                    </div>
                </>
            )}

            <div className="form-row">
                <div className="form-group">
                    <label htmlFor="category_id">Category <span>*</span></label>
                    <select id="category_id" name="category_id" value={formData.category_id || 0} onChange={handleInputChange} disabled={loading}>
                        <option value={0} disabled>-- Select a category --</option>
                        {categories.map(cat => (
                            <option key={cat.category_id} value={cat.category_id}>{cat.category_name}</option>
                        ))}
                    </select>
                </div>
                <div className="form-group">
                    <label htmlFor="status_id">Status <span>*</span></label>
                    <select id="status_id" name="status_id" value={formData.status_id || 0} onChange={handleInputChange} disabled={loading}>
                        <option value={0} disabled>-- Select a status --</option>
                        {statuses.map(status => (
                            <option key={status.statusId} value={status.statusId}>{status.statusName}</option>
                        ))}
                    </select>
                </div>
            </div>

            <div className="form-group">
                <label htmlFor="TagIds">Tags <span>*</span></label>
                <div className="tag-input-container" ref={tagContainerRef}>
                    <div className="tag-input-wrapper" onClick={() => setIsDropdownOpen(true)}>
                        {selectedTags.map(tagId => {
                            const tag = getTagById(tagId);
                            return tag ? (
                                <div key={tag.tagId} className="tag-pill">
                                    {tag.tagName}
                                    <button
                                        type="button"
                                        className="tag-remove-btn"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handleTagRemove(tag.tagId);
                                        }}
                                    >
                                        &times;
                                    </button>
                                </div>
                            ) : null;
                        })}
                        <input
                            type="text"
                            className="tag-search-input"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            onFocus={() => setIsDropdownOpen(true)}
                            placeholder={selectedTags.length ? '' : 'Search and select tags...'}
                            disabled={loading}
                        />
                    </div>
                    {isDropdownOpen && (
                        <div className="tag-dropdown-list">
                            {filteredTags.length > 0 ? (
                                filteredTags.map(tag => {
                                    const isSelected = selectedTags.includes(tag.tagId);
                                    return (
                                        <div
                                            key={tag.tagId}
                                            className={`tag-dropdown-item ${isSelected ? 'disabled' : ''}`}
                                            onMouseDown={(e) => e.preventDefault()}
                                            onClick={() => {
                                                if (isSelected) {
                                                    handleTagRemove(tag.tagId);
                                                } else {
                                                    handleTagSelect(tag);
                                                }
                                            }}
                                        >
                                            {tag.tagName}
                                        </div>
                                    );
                                })
                            ) : (
                                <div className="tag-dropdown-item disabled">No tags found</div>
                            )}
                        </div>
                    )}
                </div>
            </div>

            <div className="form-group">
                <label htmlFor="description">Synopsis <span>*</span></label>
                <textarea id="description" name="description" rows={10} value={formData.description || ''} onChange={handleInputChange} disabled={loading}></textarea>
            </div>
            <div className="form-group">
                <label htmlFor="note">Additional Notes</label>
                <textarea id="note" name="note" rows={5} value={formData.note || ''} onChange={handleInputChange} disabled={loading}></textarea>
            </div>

            <div className="form-actions">
                <button type="submit" disabled={loading}>
                    {loading ? 'Saving...' : 'Save Changes'}
                </button>
            </div>

        </form>
    );
};

export default EditSeriesForm;