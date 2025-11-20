import React, { useState, useEffect, useCallback } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { PagedResult, SeriesListDto } from '../../types/series';
import type { CategoryDto, NovelStatusDto, TagDto } from '../../types/filters';
import SeriesItem from '../../components/series/SeriesItem';
import './BrowsePage.css';
import { FaSortAlphaDown, FaSortAlphaUp } from 'react-icons/fa';
import { useSearchParams } from 'react-router-dom';
import Pagination from '../../components/common/Pagination'


const sortOptions = ["Title", "Views", "WordCount", "UpdatedAt"];
const typeOptions = ["Series", "TRADITIONAL"];
const PAGE_SIZE = 18;


const BrowsePage: React.FC = () => {

    const [searchParams, setSearchParams] = useSearchParams();

    //SEARCH PARAMS 
    const keyword = searchParams.get('q');
    const tagNameFromQuery = searchParams.get('tag'); 

    const sortBy = searchParams.get('sortBy') || 'Title';
    const isAscending = searchParams.get('isAscending') !== 'false'; 
    const selectedType = searchParams.get('filter.Type') || '';

    
    const selectedCategoryId = searchParams.get('filter.CategoryId');
    const selectedStatusId = searchParams.get('filter.StatusId');
    const selectedTagId = searchParams.get('filter.TagId');
    const currentPage = parseInt(searchParams.get('pageNumber') || '1', 10);


    // ===LOCAL STATE CHO DATA VÀ METADATA ===
    const [seriesList, setSeriesList] = useState<SeriesListDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [totalPages, setTotalPages] = useState(0); // GI? L?I cho Pagination UI

    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [statuses, setStatuses] = useState<NovelStatusDto[]>([]);
    const [tags, setTags] = useState<TagDto[]>([]);

    // === HELPER UPDATE URL  ===
    const updateUrlParams = useCallback((key: string, value: string | number | boolean | null, resetPage: boolean = true) => {
        const newParams = new URLSearchParams(searchParams);

        // UPDATE VALUE
        if (value === null || value === '' || value === undefined) {
            newParams.delete(key);
        } else {
            newParams.set(key, value.toString());
        }

        // Reset pageNumber when switching option in sort and filter
        if (resetPage) {
            newParams.delete('pageNumber');
        }

        setSearchParams(newParams);
    }, [searchParams, setSearchParams]);


    // ===  LOAD METADATA ===
    useEffect(() => {
        const fetchFiltersData = async () => {
            try {
                const [catRes, statusRes, tagRes] = await Promise.all([
                    apiClient.get(API_ROUTES.CATEGORY.GET_ALL),
                    apiClient.get(API_ROUTES.STATUS.GET_ALL),
                    apiClient.get(API_ROUTES.TAG.GET_ALL)
                ]);
                setCategories(catRes.data);
                setStatuses(statusRes.data);
                setTags(tagRes.data);
            } catch (err) {
                console.error("Failed to fetch filter metadata:", err);
                setError("Could not load filter options.");
            }
        };
        fetchFiltersData();
    }, []);


    // === RESOLVE TAG NAME TO URL SANG TAG ID  ===
    useEffect(() => {
        if (tagNameFromQuery && tags.length > 0) {
            const foundTag = tags.find(t =>
                t.tagName.toLowerCase() === tagNameFromQuery.toLowerCase()
            );

            if (foundTag) {
                const newParams = new URLSearchParams(searchParams);
                newParams.set('filter.TagId', foundTag.tagId.toString());
                newParams.delete('tag');
                newParams.delete('pageNumber');

                setSearchParams(newParams, { replace: true });
            }
        }
    }, [tagNameFromQuery, tags, searchParams, setSearchParams]);


    // === FETCH LOGIC ===
    useEffect(() => {
        const fetchSeries = async () => {
            setLoading(true);
            setError(null);

            // Clone searchParams ?? thêm pagination
            const params = new URLSearchParams(searchParams);

            params.set('pageNumber', currentPage.toString());
            params.set('pageSize', PAGE_SIZE.toString());

            let apiUrl: string;

            if (keyword) {
                apiUrl = API_ROUTES.SERIES.SEARCH_SERIES;
            } else {
                apiUrl = API_ROUTES.SERIES.GET_ALL_SERIES;

               
                if (!params.has('sortBy')) params.set('sortBy', 'Title');
                if (!params.has('isAscending')) params.set('isAscending', 'true');
            }

            try {
                const response = await apiClient.get<PagedResult<SeriesListDto>>(
                    apiUrl,
                    { params }
                );

                setSeriesList(response.data.items);
                setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));
            } catch (err) {
                setError('Could not load series.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        // Not fetch when having  tagName in URL 
        if (!tagNameFromQuery) {
            fetchSeries();
        }

    }, [searchParams, keyword, tagNameFromQuery]);


    const handleSortChange = (value: string) => {
        updateUrlParams('sortBy', value, true);
    };

    const handleSortDirectionToggle = () => {
        updateUrlParams('isAscending', !isAscending, true);
    };

    const handleFilterChange = (key: string, value: string | number | null) => {
        updateUrlParams(key, value, true);
    };

    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            updateUrlParams('pageNumber', page, false);
        }
    };


    const isFilteringDisabled = !!keyword;

    return (
        <div className="browse-page-container">

            <div className="browse-content">
                {keyword ? (
                    <h2 style={{ textAlign: 'left', width: '100%' }}>Search results for: "{keyword}"</h2>
                ) : (
                    <div className="sorting-controls">
                        <select
                            id="sort-by"
                            value={sortBy} 
                            onChange={(e) => handleSortChange(e.target.value)}
                        >
                            {sortOptions.map(opt => (
                                <option key={opt} value={opt}>Sort by {opt}</option>
                            ))}
                        </select>
                        <button
                            className="sort-direction-btn"
                            title={isAscending ? "Sort Descending" : "Sort Ascending"}
                            onClick={handleSortDirectionToggle}
                        >
                            {isAscending ? <FaSortAlphaDown /> : <FaSortAlphaUp />}
                        </button>
                    </div>
                )}

                {/*Checking loading for chapter 1 */}
                {loading && currentPage === 1 && <div>Loading series...</div>}
                {error && <div style={{ color: 'red' }}>{error}</div>}
                {!loading && !error && seriesList.length === 0 && (
                    <div>No series found matching your criteria.</div>
                )}

                <div className="series-grid">
                    {seriesList.map(series => (
                        <SeriesItem key={series.series_Id} series={series} type="grid" />
                    ))}
                </div>

                {/* Pagination */}
                {!loading && totalPages > 1 && (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '20px', width: '100%' }}>
                        <Pagination
                            currentPage={currentPage}
                            totalPages={totalPages}
                            onPageChange={handlePageChange}
                        />
                    </div>
                )}
            </div>

            {/* Filter (Sidebar) */}
            <aside className="browse-sidebar">

                {/* Filter: Type */}
                <div className="filter-box">
                    <div className="filter-box-header">Type</div>
                    <div className="filter-box-content">
                        <div className="filter-item" key="type-all">
                            <input
                                type="radio" id="type-all" name="filter-type" value=""
                                checked={selectedType === ''}
                                onChange={(e) => handleFilterChange('filter.Type', e.target.value)}
                                disabled={isFilteringDisabled}
                            />
                            <label htmlFor="type-all">All Types</label>
                        </div>
                        {typeOptions.map(type => (
                            <div className="filter-item" key={type}>
                                <input
                                    type="radio" id={`type-${type}`} name="filter-type" value={type}
                                    checked={selectedType === type}
                                    onChange={(e) => handleFilterChange('filter.Type', e.target.value)}
                                    disabled={isFilteringDisabled}
                                />
                                <label htmlFor={`type-${type}`}>{type}</label>
                            </div>
                        ))}
                    </div>
                </div>


                {/* Filter: Category */}
                <div className="filter-box">
                    <div className="filter-box-header">CATEGORY</div>
                    <div className="filter-box-content">
                        <div className="filter-item" key="cat-all">
                            <input
                                type="radio" id="cat-all" name="filter-category"
                                checked={selectedCategoryId === null || selectedCategoryId === '0'}
                                onChange={() => handleFilterChange('filter.CategoryId', null)}
                                disabled={isFilteringDisabled}
                            />
                            <label htmlFor="cat-all">All Categories</label>
                        </div>

                        {/* Map qua các category */}
                        {categories.map(cat => (
                            <div className="filter-item" key={cat.category_id}>
                                <input
                                    type="radio" id={`cat-${cat.category_id}`} name="filter-category"
                                    checked={selectedCategoryId === cat.category_id.toString()}
                                    onChange={() => handleFilterChange('filter.CategoryId', cat.category_id)}
                                    disabled={isFilteringDisabled}
                                />
                                <label htmlFor={`cat-${cat.category_id}`}>{cat.category_name}</label>
                            </div>
                        ))}
                    </div>
                </div>


                {/* Filter: Status */}
                <div className="filter-box">
                    <div className="filter-box-header">STATUS</div>
                    <div className="filter-box-content">
                        <div className="filter-item" key="status-all">
                            <input
                                type="radio" id="status-all" name="filter-status"
                                checked={selectedStatusId === null || selectedStatusId === '0'}
                                onChange={() => handleFilterChange('filter.StatusId', null)}
                                disabled={isFilteringDisabled}
                            />
                            <label htmlFor="status-all">All Statuses</label>
                        </div>


                        {/* Map qua các status */}
                        {statuses.map(status => (
                            <div className="filter-item" key={status.statusId}>
                                <input
                                    type="radio" id={`status-${status.statusId}`} name="filter-status"
                                    checked={selectedStatusId === status.statusId.toString()}
                                    onChange={() => handleFilterChange('filter.StatusId', status.statusId)}
                                    disabled={isFilteringDisabled}
                                />
                                <label htmlFor={`status-${status.statusId}`}>{status.statusName}</label>
                            </div>
                        ))}
                    </div>
                </div>


                {/* Filter: Tag */}
                <div className="filter-box">
                    <div className="filter-box-header">TAG</div>
                    <div className="filter-box-content">

                        <div className="filter-item" key="tag-all">
                            <input
                                type="radio" id="tag-all" name="filter-tag"
                                checked={selectedTagId === null || selectedTagId === '0'}
                                onChange={() => handleFilterChange('filter.TagId', null)}
                                disabled={isFilteringDisabled}
                            />
                            <label htmlFor="tag-all">All Tags</label>
                        </div>

                        {tags.map(tag => (
                            <div className="filter-item" key={tag.tagId}>
                                <input
                                    type="radio" id={`tag-${tag.tagId}`} name="filter-tag"
                                    checked={selectedTagId === tag.tagId.toString()}
                                    onChange={() => handleFilterChange('filter.TagId', tag.tagId)}
                                    disabled={isFilteringDisabled}
                                />
                                <label htmlFor={`tag-${tag.tagId}`}>{tag.tagName}</label>
                            </div>
                        ))}
                    </div>
                </div>
            </aside>
        </div>
    );
};

export default BrowsePage;