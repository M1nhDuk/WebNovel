import React, { useState, useEffect } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { PagedResult, SeriesListDto } from '../../types/series';
import SeriesItem from '../../components/series/SeriesItem';
import { useAuth } from '../../hooks/useAuth';
import './ProfilePage.css';
import { Link } from 'react-router-dom';
import ImageUploadButton from './ImageUploadButton';
import Pagination from '../../components/common/Pagination';



const PAGE_SIZE = 12; 

const ProfilePage: React.FC = () => {
    const { user, isLoading: userLoading } = useAuth();
    const [seriesList, setSeriesList] = useState<SeriesListDto[]>([]);


    const [currentPage, setCurrentPage] = useState(1); 
    const [totalPages, setTotalPages] = useState(0);   


    const [isLoadingSeries, setIsLoadingSeries] = useState(true);
    const [error, setError] = useState<string | null>(null);

    //fetch data 
    const fetchMySeries = async (pageToFetch: number) => {
        if (!user) {
            setError("You must be logged in to view this page.");
            setIsLoadingSeries(false);
            return;
        }

        setIsLoadingSeries(true);
        setError(null);

        try {
            const response = await apiClient.get<PagedResult<SeriesListDto>>(
                API_ROUTES.SERIES.GET_MY_SERIES,
                {
                    params: {
                        pageNumber: pageToFetch,
                        pageSize: PAGE_SIZE
                    }
                }
            );


            setSeriesList(response.data.items);


            setCurrentPage(response.data.pageNumber);
            setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));

        } catch (err: any) {
            console.error("Failed to fetch user series:", err);
            setError(err.response?.data?.message || "Could not load your series.");
        } finally {
            setIsLoadingSeries(false);
        }
    };


    useEffect(() => {
        if (user) {

            fetchMySeries(currentPage);
        } else if (!userLoading) {
            setIsLoadingSeries(false);
            setError("You must be logged in to view this page.");
        }
    }, [user, userLoading, currentPage]); 


    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            setCurrentPage(page);

            window.scrollTo(0, 0);
        }
    };

    if (userLoading) {
        return <div>Loading profile...</div>;
    }

    if (!user) {
        return (
            <div style={{ padding: '40px', textAlign: 'center' }}>
                <h2>{error || 'User not found.'}</h2>
                <Link to="/login">Please log in</Link>
            </div>
        );
    }

    return (
        <div className="profile-page-container">

            <header className="profile-header upload-hover-container">
                <img
                    src={user.backgroundImage || ''}
                    alt="User background"
                    className="profile-background-img"
                />

                <div className="profile-overlay"></div>
                <div className="profile-upload-button">
                    <ImageUploadButton
                        apiEndpoint={API_ROUTES.AUTH.UPLOAD_BACKGROUND}
                    />
                </div>

                <div className="profile-white-backdrop"></div>

                <div className="profile-info">
                    <div className="profile-avatar-container upload-hover-container">
                        <img
                            src={user.avatar || ''}
                            alt="User avatar"
                            className="profile-avatar-img"
                        />
                        <div className="profile-overlay"></div>
                        <div className="profile-upload-button">
                            <ImageUploadButton
                                apiEndpoint={API_ROUTES.AUTH.UPLOAD_AVATAR}
                            />
                        </div>
                    </div>
                    <h1 className="profile-username">{user.username}</h1>
                </div>

            </header>

            <section className="profile-content">

                <div className="my-series-section">
                    <h2>My Series</h2>

                    {isLoadingSeries && <div>Loading series...</div>}

                    {error && <div style={{ color: 'red' }}>{error}</div>}

                    {!isLoadingSeries && !error && seriesList.length === 0 && (
                        <div>You have not uploaded any series yet.</div>
                    )}

                    {seriesList.length > 0 && (
                        <div className="series-grid">
                            {seriesList.map(series => (
                                <SeriesItem key={series.series_Id} series={series} type="grid" />
                            ))}
                        </div>
                    )}

                    {!isLoadingSeries && totalPages > 1 && (
                        <Pagination
                            currentPage={currentPage}
                            totalPages={totalPages}
                            onPageChange={handlePageChange}
                        />
                    )}
                </div>

            </section>
        </div>
    );
};

export default ProfilePage;