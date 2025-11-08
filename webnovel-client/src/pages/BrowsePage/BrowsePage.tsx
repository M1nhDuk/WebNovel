import React, { useState, useEffect } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { PagedResult, SeriesListDto } from '../../types/series';
import type { CategoryDto, NovelStatusDto, TagDto } from '../../types/filters';
import SeriesItem from '../../components/series/SeriesItem';
import './BrowsePage.css';
import { FaSortAlphaDown, FaSortAlphaUp } from 'react-icons/fa';


const sortOptions = ["Title", "Views", "WordCount", "UpdatedAt"];

const typeOptions = ["Series", "TRADITIONAL"];

const BrowsePage: React.FC = () => {

    const [seriesList, setSeriesList] = useState<SeriesListDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [page, setPage] = useState(1);
    const [totalRecords, setTotalRecords] = useState(0);


    const [sortBy, setSortBy] = useState<string>('Title'); // M?c ??nh
    const [isAscending, setIsAscending] = useState<boolean>(true);


    const [categories, setCategories] = useState<CategoryDto[]>([]);
    const [statuses, setStatuses] = useState<NovelStatusDto[]>([]);
    const [tags, setTags] = useState<TagDto[]>([]);


    const [selectedCategoryIds, setSelectedCategoryIds] = useState<number[]>([]);
    const [selectedStatusIds, setSelectedStatusIds] = useState<number[]>([]);
    const [selectedType, setSelectedType] = useState<string>(''); // 'Series' ho?c 'TRADITIONAL'
    const [selectedTagIds, setSelectedTagIds] = useState<number[]>([]);


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


    useEffect(() => {
        const fetchSeries = async () => {
            setLoading(true);
            setError(null);

            const params = new URLSearchParams();
            params.append('pageNumber', page.toString());
            params.append('pageSize', '18'); // 18 item m?i trang
            params.append('sortBy', sortBy);
            params.append('isAscending', String(isAscending));

            if (selectedType) {
                params.append('filter.Type', selectedType);
            }
            selectedCategoryIds.forEach(id => params.append('filter.CategoryId', id.toString()));
            selectedStatusIds.forEach(id => params.append('filter.StatusId', id.toString()));
            selectedTagIds.forEach(id => params.append('filter.TagId', id.toString()));

            try {
                const response = await apiClient.get<PagedResult<SeriesListDto>>(
                    API_ROUTES.SERIES.GET_ALL_SERIES,
                    { params } 
                );
                if (page === 1) {
                    setSeriesList(response.data.items);
                } else {
                    setSeriesList(prevList => [...prevList, ...response.data.items]);
                }
                setTotalRecords(response.data.totalRecords);
            } catch (err) {
                setError('Could not load series.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchSeries();
    }, [page, sortBy, isAscending, selectedCategoryIds, selectedStatusIds, selectedType, selectedTagIds]);

    const handleCheckboxChange = (
        id: number,
        setter: React.Dispatch<React.SetStateAction<number[]>>
    ) => {
        setter(prevIds =>
            prevIds.includes(id)
                ? prevIds.filter(prevId => prevId !== id) 
                : [...prevIds, id] 
        );
        setPage(1); 
    };


    const handleRadioChange = (
        value: string,
        setter: React.Dispatch<React.SetStateAction<string>>
    ) => {
        setter(value);
        setPage(1); 
    }

    const handleLoadMore = () => {
        if (seriesList.length < totalRecords) {
            setPage(prevPage => prevPage + 1);
        }
    };

    return (
        <div className="browse-page-container">

            <div className="browse-content">
                <div className="sorting-controls">
                    <select
                        id="sort-by"
                        value={sortBy}
                        onChange={(e) => {
                            setSortBy(e.target.value);
                            setPage(1);
                        }}
                    >
                        <option value="Title">SORTING FUNCTION</option> {/* Dòng m?c ??nh */}
                        {sortOptions.map(opt => (
                            <option key={opt} value={opt}>Sort by {opt}</option>
                        ))}
                    </select>
                    <button
                        className="sort-direction-btn"
                        title={isAscending ? "Sort Descending" : "Sort Ascending"}
                        onClick={() => {
                            setIsAscending(!isAscending);
                            setPage(1);
                        }}
                    >
                        {isAscending ? <FaSortAlphaDown /> : <FaSortAlphaUp />}
                    </button>
                </div>

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
                {seriesList.length < totalRecords && !loading && (
                    <div style={{ display: 'flex', justifyContent: 'center', padding: '20px' }}>
                        <button onClick={handleLoadMore}>
                            {loading ? 'Loading...' : 'Load More'}
                        </button>
                    </div>
                )}
            </div>



            {/* Filter (Sidebar) */}
            <aside className="browse-sidebar">
                {/* Filter: Type */}
                <div className="filter-box">
                    <div className="filter-box-header">Filter function : type</div>
                    <div className="filter-box-content">
                        <div className="filter-item" key="type-all">
                            <input
                                type="radio"
                                id="type-all"
                                name="filter-type"
                                value=""
                                checked={selectedType === ''}
                                onChange={(e) => handleRadioChange(e.target.value, setSelectedType)}
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
                                />
                                <label htmlFor={`type-${type}`}>{type}</label>
                            </div>
                        ))}
                    </div>
                </div>



                {/* Filter: Category */}
                <div className="filter-box">
                    <div className="filter-box-header">Filter function : category</div>
                    <div className="filter-box-content">
                        {categories.map(cat => (
                            <div className="filter-item" key={cat.category_id}>
                                <input
                                    type="checkbox"
                                    id={`cat-${cat.category_id}`}
                                    checked={selectedCategoryIds.includes(cat.category_id)}
                                    onChange={() => handleCheckboxChange(cat.category_id, setSelectedCategoryIds)}
                                />
                                <label htmlFor={`cat-${cat.category_id}`}>{cat.category_name}</label>
                            </div>
                        ))}
                    </div>
                </div>



                {/* Filter: Status */}
                <div className="filter-box">
                    <div className="filter-box-header">Filter function : status</div>
                    <div className="filter-box-content">
                        {statuses.map(status => (
                            <div className="filter-item" key={status.statusId}>
                                <input
                                    type="checkbox"
                                    id={`status-${status.statusId}`}
                                    checked={selectedStatusIds.includes(status.statusId)}
                                    onChange={() => handleCheckboxChange(status.statusId, setSelectedStatusIds)}
                                />
                                <label htmlFor={`status-${status.statusId}`}>{status.statusName}</label>
                            </div>
                        ))}
                    </div>
                </div>



                {/* Filter: Tag */}
                <div className="filter-box">
                    <div className="filter-box-header">Filter function : Tag</div>
                    <div className="filter-box-content">
                        {tags.map(tag => (
                            <div className="filter-item" key={tag.tagId}>
                                <input
                                    type="checkbox"
                                    id={`tag-${tag.tagId}`}
                                    checked={selectedTagIds.includes(tag.tagId)}
                                    onChange={() => handleCheckboxChange(tag.tagId, setSelectedTagIds)}
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