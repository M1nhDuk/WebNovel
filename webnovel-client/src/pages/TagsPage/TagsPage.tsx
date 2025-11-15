import React, { useState, useEffect, useCallback } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { TagDto } from '../../types/filters';
import './TagsPage.css';
import { Link } from 'react-router-dom';
import Pagination from '../../components/common/Pagination';

const PAGE_SIZE = 30;

const TagsPage: React.FC = () => {

    const [allTags, setAllTags] = useState<TagDto[]>([]);

    const [pagedTags, setPagedTags] = useState<TagDto[]>([]);

    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);

    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);


    const fetchTags = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
       
            const response = await apiClient.get<TagDto[]>(API_ROUTES.TAG.GET_ALL); 
            const sortedTags = response.data.sort((a, b) => a.tagName.localeCompare(b.tagName));

            setAllTags(sortedTags);
            setTotalPages(Math.ceil(sortedTags.length / PAGE_SIZE));

        } catch (err: any) {
            console.error("Failed to fetch tags:", err);
            setError(err.response?.data?.message || "Could not load tags data.");
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchTags();
    }, [fetchTags]);

  
    useEffect(() => {
        const startIndex = (currentPage - 1) * PAGE_SIZE;
        const endIndex = startIndex + PAGE_SIZE;
        setPagedTags(allTags.slice(startIndex, endIndex));


        setTotalPages(Math.ceil(allTags.length / PAGE_SIZE));
    }, [allTags, currentPage]);

 
    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            setCurrentPage(page);
            window.scrollTo(0, 0); 
        }
    };


    if (isLoading) {
        return <div className="admin-tags-page-container">Loading tags...</div>;
    }

    if (error) {
        return <div className="admin-tags-page-container"><div className="auth-error">{error}</div></div>;
    }

    return (
        <div className="admin-tags-page-container">
            <div className="genres-header">
                Genres
            </div>
            <div className="genres-list">
                {pagedTags.length === 0 && allTags.length === 0 ? (
                    <div className="tag-list-item">
                        <div style={{ fontStyle: 'italic', color: 'var(--text-secondary)' }}>
                            No tags found.
                        </div>
                    </div>
                ) : (
                    pagedTags.map((tag, index) => (
                        <div
                            key={tag.tagId}
                            className={`tag-list-item ${index % 2 !== 0 ? 'odd-row' : ''}`}
                        >
                            {/* Tag Name */}
                            <Link to={`/browse?tag=${tag.tagName}`} className="tag-name">
                                {tag.tagName}
                            </Link>

                            {/* Description */}
                            <div className="tag-description">
                                {/* Đã sửa thành tag.description (camelCase) */}
                                {tag.description || 'No description provided for this tag.'}
                            </div>
                        </div>
                    ))
                )}
            </div>

            {/*  Pagination */}
            {totalPages > 1 && (
                <div style={{ display: 'flex', justifyContent: 'center', padding: '20px 0', width: '100%' }}>
                    <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        onPageChange={handlePageChange}
                    />
                </div>
            )}
        </div>
    );
};

export default TagsPage;