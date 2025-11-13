import React, { useState, useEffect } from 'react';
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
    const keyword = searchParams.get('q');

    const tagNameFromQuery = searchParams.get('tag');

    const [seriesList, setSeriesList] = useState<SeriesListDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [page, setPage] = useState(1);


    const [sortBy, setSortBy] = useState<string>('Title'); 
    const [isAscending, setIsAscending] = useState<boolean>(true);


    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [statuses, setStatuses] = useState<NovelStatusDto[]>([]);
    const [tags, setTags] = useState<TagDto[]>([]);


    const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);
    const [selectedStatusId, setSelectedStatusId] = useState<number | null>(null);
    const [selectedType, setSelectedType] = useState<string>('');
    const [selectedTagId, setSelectedTagId] = useState<number | null>(null);

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);


    //Load Filter
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


    //useEffect to find the tagId from the tagName in the URL
    useEffect(() => {
        if (tagNameFromQuery && tags.length > 0) {

            // Find the corresponding tagId
            const foundTag = tags.find(t =>
                t.tagName.toLowerCase() === tagNameFromQuery.toLowerCase()
            );

            if (foundTag) {
                //Set the radio filter to this ID
                setSelectedTagId(foundTag.tagId);

                // Clean up the URL parameter
                const newParams = new URLSearchParams(searchParams);
                newParams.delete('tag');
                setSearchParams(newParams, { replace: true });
            }
        }
      
    }, [tagNameFromQuery, tags, searchParams, setSearchParams]);



    //Filtered
    useEffect(() => {
        setPage(1);
    }, [sortBy, isAscending, selectedCategoryId, selectedStatusId, selectedType, selectedTagId, keyword]);


    useEffect(() => {
        const fetchSeries = async () => {
            setLoading(true);
            setError(null);

            const params = new URLSearchParams();
            params.append('pageNumber', currentPage.toString());
            params.append('pageSize', PAGE_SIZE.toString());

            let apiUrl: string;

            if (keyword) {
                apiUrl = API_ROUTES.SERIES.SEARCH_SERIES;
                params.append('keyword', keyword);
            } else {
                apiUrl = API_ROUTES.SERIES.GET_ALL_SERIES;
                params.append('sortBy', sortBy);
                params.append('isAscending', String(isAscending));

                if (selectedType) {
                    params.append('filter.Type', selectedType);
                }
                if (selectedCategoryId !== null) {
                    params.append('filter.CategoryId', selectedCategoryId.toString());
                }
                if (selectedStatusId !== null) {
                    params.append('filter.StatusId', selectedStatusId.toString());
                }
                if (selectedTagId !== null) {
                    params.append('filter.TagId', selectedTagId.toString());
                }
            }

            try {
                const response = await apiClient.get<PagedResult<SeriesListDto>>(
                    apiUrl,
                    { params }
                );

                setSeriesList(response.data.items);


                setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));
                setCurrentPage(response.data.pageNumber);

            } catch (err) {
                setError('Could not load series.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchSeries();
    }, [currentPage, sortBy, isAscending, selectedCategoryId, selectedStatusId, selectedType, selectedTagId, keyword]);




    //Type(string)
    const handleRadioChange = (
        value: string,
        setter: React.Dispatch<React.SetStateAction<string>>
    ) => {
        setter(value);
    }

    //Rest(number)
    const handleRadioIdChange = (
        id: number | null,
        setter: React.Dispatch<React.SetStateAction<number | null>>
    ) => {
        setter(id);
    };

    //Pagination
    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            setCurrentPage(page);
            window.scrollTo(0, 0);
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
                            onChange={(e) => {
                                setSortBy(e.target.value);
                                // setPage(1); 
                            }}
                        >
                            <option value="Title">SORTING FUNCTION</option>
                            {sortOptions.map(opt => (
                                <option key={opt} value={opt}>Sort by {opt}</option>
                            ))}
                        </select>
                        <button
                            className="sort-direction-btn"
                            title={isAscending ? "Sort Descending" : "Sort Ascending"}
                            onClick={() => {
                                setIsAscending(!isAscending);
                                // setPage(1); 
                            }}
                        >
                            {isAscending ? <FaSortAlphaDown /> : <FaSortAlphaUp />}
                        </button>
                    </div>
                )}

                {loading && page === 1 && <div>Loading series...</div>}
                {error && <div style={{ color: 'red' }}>{error}</div>}
                {!loading && !error && seriesList.length === 0 && (
                    <div>No series found matching your criteria.</div>
                )}

                <div className="series-grid">
                    {seriesList.map(series => (
                        <SeriesItem key={series.series_Id} series={series} type="grid" />
                    ))}
                </div>

                {/*Load More Button */}
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
                                type="radio"
                                id="type-all"
                                name="filter-type"
                                value=""
                                checked={selectedType === ''}
                                onChange={(e) => handleRadioChange(e.target.value, setSelectedType)}
                                disabled={isFilteringDisabled}
                            />
                            <label htmlFor="type-all">All Types</label>
                        </div>
                        {typeOptions.map(type => (
                            <div className="filter-item" key={type}>
                                <input
                                    type="radio"
                                    id={`type-${type}`}
                                    name="filter-type"
                                    value={type}
                                    checked={selectedType === type}
                                    onChange={(e) => handleRadioChange(e.target.value, setSelectedType)}
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
                                type="radio"
                                id="cat-all"
                                name="filter-category" 
                                checked={selectedCategoryId === null} 
                                onChange={() => handleRadioIdChange(null, setSelectedCategoryId)}
                                disabled={isFilteringDisabled}
                            />
                            <label htmlFor="cat-all">All Categories</label>
                        </div>
                        {/* Map qua các category */}
                        {categories.map(cat => (
                            <div className="filter-item" key={cat.category_id}>
                                <input
                                    type="radio"
                                    id={`cat-${cat.category_id}`}
                                    name="filter-category"
                                    checked={selectedCategoryId === cat.category_id}
                                    onChange={() => handleRadioIdChange(cat.category_id, setSelectedCategoryId)}
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
                                type="radio"
                                id="status-all"
                                name="filter-status"
                                checked={selectedStatusId === null}
                                onChange={() => handleRadioIdChange(null, setSelectedStatusId)}
                                disabled={isFilteringDisabled}
                            />
                            <label htmlFor="status-all">All Statuses</label>
                        </div>
                        {/* Map qua các status */}
                        {statuses.map(status => (
                            <div className="filter-item" key={status.statusId}>
                                <input
                                    type="radio"
                                    id={`status-${status.statusId}`}
                                    name="filter-status"
                                    checked={selectedStatusId === status.statusId}
                                    onChange={() => handleRadioIdChange(status.statusId, setSelectedStatusId)}
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
                                type="radio"
                                id="tag-all"
                                name="filter-tag"
                                checked={selectedTagId === null}
                                onChange={() => handleRadioIdChange(null, setSelectedTagId)}
                                disabled={isFilteringDisabled}
                            />
                            <label htmlFor="tag-all">All Tags</label>
                        </div>

                        {tags.map(tag => (
                            <div className="filter-item" key={tag.tagId}>
                                <input
                                    type="radio"
                                    id={`tag-${tag.tagId}`}
                                    name="filter-tag"
                                    checked={selectedTagId === tag.tagId}
                                    onChange={() => handleRadioIdChange(tag.tagId, setSelectedTagId)}
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