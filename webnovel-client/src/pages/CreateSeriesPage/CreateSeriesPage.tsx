import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { CategoryDto, NovelStatusDto, TagDto } from '../../types/filters';
import type { NovelSeriesDetailDto, CreateSeriesDto, CreateTraditionalSeriesDto } from '../../types/series';
import './CreateSeriesPage.css';

type SeriesFormData = CreateTraditionalSeriesDto;

//Check ISBN
const isbn10Regex = /^[0-9]{9}[0-9xX]$/;

const isbn13Regex = /^(978|979)[0-9]{10}$/;


const CreateSeriesPage: React.FC = () => {
    const navigate = useNavigate();

    const [seriesType, setSeriesType] = useState<'Series' | 'TRADITIONAL'>('Series');

    const [formData, setFormData] = useState<Partial<SeriesFormData>>({
        series_title: '',
        author: '',
        artist: '',
        description: '',
        note: '',
        category_id: 0,
        status_id: 0,
        ISBN_10: '',
        ISBN_13: '',
        publisher: '',
        publish_date: '',
        edition: '',
    });

    const [selectedTags, setSelectedTags] = useState<number[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const tagContainerRef = useRef<HTMLDivElement>(null);

    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [statuses, setStatuses] = useState<NovelStatusDto[]>([]);
    const [allTags, setAllTags] = useState<TagDto[]>([]);

    const [isLoading, setIsLoading] = useState(false);
    const [loadError, setLoadError] = useState<string | null>(null);
    const [submitError, setSubmitError] = useState<string | null>(null);


    useEffect(() => {
        const fetchFiltersData = async () => {
            setIsLoading(true);
            setLoadError(null);
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
                setLoadError("Cannot load form data. Please try again.");
            } finally {
                setIsLoading(false);
            }
        };
        fetchFiltersData();
    }, []);

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

        //Chỉ nhập số ở ISBN
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

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitError(null);

        // Validation chung
        if (!formData.series_title?.trim()) {
            setSubmitError("Title cannot be empty.");
            return;
        }
        if (!formData.description?.trim()) {
            setSubmitError("Synopsis cannot be empty.");
            return;
        }
        if (!formData.category_id || formData.category_id === 0) {
            setSubmitError("Please select a category.");
            return;
        }
        if (!formData.status_id || formData.status_id === 0) {
            setSubmitError("Please select a status.");
            return;
        }
        if (selectedTags.length === 0) {
            setSubmitError("Please select at least one tag.");
            return;
        }

        if (seriesType === 'TRADITIONAL') {
            // ISBN-13 
            if (!formData.ISBN_13?.trim()) {
                setSubmitError("ISBN-13 is required for Classical Novel.");
                return;
            }
            if (!isbn13Regex.test(formData.ISBN_13.trim())) {
                setSubmitError("ISBN-13 must be 13 digits and start with 978 or 979.");
                return;
            }

            // ISBN-10
            if (formData.ISBN_10 && formData.ISBN_10.trim() !== '') {
                if (!isbn10Regex.test(formData.ISBN_10.trim())) {
                    setSubmitError("ISBN-10 must be 10 characters (9 digits + 1 digit or 'X').");
                    return;
                }
            }
        }


        setIsLoading(true);

        const basePayload: CreateSeriesDto = {
            series_title: formData.series_title!,
            author: formData.author,
            artist: formData.artist,
            description: formData.description!,
            note: formData.note,
            category_id: formData.category_id!,
            status_id: formData.status_id!,
            TagIds: selectedTags
        };

        try {
            let response;
            if (seriesType === 'TRADITIONAL') {
                const traditionalPayload: CreateTraditionalSeriesDto = {
                    ...basePayload,
                    ISBN_10: formData.ISBN_10,
                    ISBN_13: formData.ISBN_13!,
                    publisher: formData.publisher,
                    publish_date: formData.publish_date || null,
                    edition: formData.edition,
                };

                response = await apiClient.post<NovelSeriesDetailDto>(API_ROUTES.SERIES.CREATE_CLASSIC_SERIES, traditionalPayload);

            } else {
                response = await apiClient.post<NovelSeriesDetailDto>(API_ROUTES.SERIES.CREATE_SERIES, basePayload);
            }

            const newSeriesId = response.data.series_Id;

            navigate(`/manage/series/${newSeriesId}`);

        } catch (err: any) {
            console.error("Failed to create series:", err);
            setSubmitError(err.response?.data?.message || "An error occurred while creating the series.");
        } finally {
            setIsLoading(false);
        }
    };


    if (isLoading && !categories.length) {
        return <div className="create-series-container">Loading data...</div>;
    }

    if (loadError) {
        return <div className="create-series-container error-message">{loadError}</div>;
    }
  
    return (
        <div className="create-series-page-wrapper">
            <div className="create-series-container">
                <h1>Create Series</h1>

                <form className="create-series-form" onSubmit={handleSubmit}>
                    
                    <div className="form-group">
                        <label htmlFor="series_title">Title <span>*</span></label>
                        <input
                            type="text"
                            id="series_title"
                            name="series_title"
                            value={formData.series_title}
                            onChange={handleInputChange}
                            disabled={isLoading}
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="author">Author</label>
                        <input
                            type="text"
                            id="author"
                            name="author"
                            value={formData.author || ''}
                            onChange={handleInputChange}
                            disabled={isLoading}
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="artist">Artist</label>
                        <input
                            type="text"
                            id="artist"
                            name="artist"
                            value={formData.artist || ''}
                            onChange={handleInputChange}
                            disabled={isLoading}
                        />
                    </div>

                    <div className="form-group">
                        <label>Series Type <span>*</span></label>
                        <div className="form-group-radio">
                            <label>
                                <input
                                    type="radio"
                                    name="seriesType"
                                    value="Series"
                                    checked={seriesType === 'Series'}
                                    onChange={() => setSeriesType('Series')}
                                />
                                Web Novel
                            </label>
                            <label>
                                <input
                                    type="radio"
                                    name="seriesType"
                                    value="TRADITIONAL"
                                    checked={seriesType === 'TRADITIONAL'}
                                    onChange={() => setSeriesType('TRADITIONAL')}
                                />
                                Classical Novel
                            </label>
                        </div>
                    </div>

                    {seriesType === 'TRADITIONAL' && (
                        <>
                            <div className="form-group">
                                <label htmlFor="ISBN_13">ISBN-13 <span>*</span></label>
                                <input
                                    type="text"
                                    id="ISBN_13"
                                    name="ISBN_13"
                                    value={formData.ISBN_13 || ''}
                                    onChange={handleInputChange}
                                    disabled={isLoading}
                                    maxLength={13}
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="ISBN_10">ISBN-10</label>
                                <input
                                    type="text"
                                    id="ISBN_10"
                                    name="ISBN_10"
                                    value={formData.ISBN_10 || ''}
                                    onChange={handleInputChange}
                                    disabled={isLoading}
                                    maxLength={10}
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="publisher">Publisher</label>
                                <input
                                    type="text"
                                    id="publisher"
                                    name="publisher"
                                    value={formData.publisher || ''}
                                    onChange={handleInputChange}
                                    disabled={isLoading}
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="edition">Edition</label>
                                <input
                                    type="text"
                                    id="edition"
                                    name="edition"
                                    value={formData.edition || ''}
                                    onChange={handleInputChange}
                                    disabled={isLoading}
                                />
                            </div>
                            <div className="form-group">
                                <label htmlFor="publish_date">Publish Date</label>
                                <input
                                    type="date"
                                    id="publish_date"
                                    name="publish_date"
                                    value={formData.publish_date || ''}
                                    onChange={handleInputChange}
                                    disabled={isLoading}
                                />
                            </div>
                        </>
                    )}

                    <div className="form-group">
                        <label htmlFor="category_id">Category <span>*</span></label>
                        <select
                            id="category_id"
                            name="category_id"
                            value={formData.category_id || 0}
                            onChange={handleInputChange}
                            disabled={isLoading}
                        >
                            <option value={0} disabled>-- Select a category --</option>
                            {categories.map(cat => (
                                <option key={cat.category_id} value={cat.category_id}>
                                    {cat.category_name}
                                </option>
                            ))}
                        </select>
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
                                    disabled={isLoading}
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
                        <textarea
                            id="description"
                            name="description"
                            rows={10}
                            value={formData.description}
                            onChange={handleInputChange}
                            disabled={isLoading}
                        ></textarea>
                    </div>

                    <div className="form-group">
                        <label htmlFor="note">Additional Notes</label>
                        <textarea
                            id="note"
                            name="note"
                            rows={5}
                            value={formData.note || ''}
                            onChange={handleInputChange}
                            disabled={isLoading}
                        ></textarea>
                    </div>

                    <div className="form-group">
                        <label htmlFor="status_id">Translation Status <span>*</span></label>
                        <select
                            id="status_id"
                            name="status_id"
                            value={formData.status_id || 0}
                            onChange={handleInputChange}
                            disabled={isLoading}
                        >
                            <option value={0} disabled>-- Select a status --</option>
                            {statuses.map(status => (
                                <option key={status.statusId} value={status.statusId}>
                                    {status.statusName}
                                </option>
                            ))}
                        </select>
                    </div>

                    {submitError && (
                        <div className="form-error-message">{submitError}</div>
                    )}

                    <div className="form-actions">
                        <button type="submit" disabled={isLoading}>
                            {isLoading ? 'Creating...' : 'Create Series'}
                        </button>
                        <button type="button" className="cancel-btn" onClick={() => navigate(-1)} disabled={isLoading}>
                            Cancel
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};



export default CreateSeriesPage;