import React, { useState, useEffect, useCallback } from 'react';
import {useParams, useNavigate } from 'react-router-dom'; // Thêm useNavigate
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { PagedResult, SeriesListDto } from '../../types/series';
import type { UserProfile } from '../../types/auth';
import SeriesItem from '../../components/series/SeriesItem';
import { useAuth } from '../../hooks/useAuth';
import './ProfilePage.css';
import ImageUploadButton from './ImageUploadButton';
import Pagination from '../../components/common/Pagination';

const PAGE_SIZE = 12;
const GATEWAY_URL = 'https://localhost:8000';

const ProfilePage: React.FC = () => {
    const { userId } = useParams<{ userId: string }>();
    const { user: currentUser, isLoading: authLoading } = useAuth();
    const navigate = useNavigate();

    const isOwnProfile = !userId || (currentUser && currentUser.userId === userId);

    const [seriesList, setSeriesList] = useState<SeriesListDto[]>([]);
    const [publicUser, setPublicUser] = useState<UserProfile | null>(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [isLoadingData, setIsLoadingData] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const getImageUrl = (imagePath: string | null | undefined) => {
        if (!imagePath) return `${GATEWAY_URL}/uploads/default_avatar_thumb.png`;
        if (imagePath.startsWith('http')) return imagePath;
        const formattedPath = imagePath.startsWith('/') ? imagePath : `/${imagePath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    const displayUser = isOwnProfile ? currentUser : publicUser;

    //Fetch thông tin Public Profile 
    useEffect(() => {
        const fetchPublicProfile = async () => {
            // Nếu là own profile hoặc chưa có userId trên URL thì bỏ qua
            if (isOwnProfile || !userId) return;

            try {
                const response = await apiClient.get<UserProfile>(`/api/User/${userId}/public`);
                const userData = response.data;
                setPublicUser({
                    ...userData,
                    avatar: getImageUrl(userData.avatar),
                    backgroundImage: getImageUrl(userData.backgroundImage)
                });
            } catch (err) {
                console.error("Failed to fetch public profile:", err);
                setError("User not found.");
            }
        };

        fetchPublicProfile();
    }, [userId, isOwnProfile]);

    //Fetch Series
    const fetchSeries = useCallback(async (pageToFetch: number) => {
        setIsLoadingData(true);
        setError(null);

        try {
            let response;
            if (isOwnProfile) {
                // Case 1: Xem của chính mình 
                if (!currentUser) return;
                response = await apiClient.get<PagedResult<SeriesListDto>>(
                    API_ROUTES.SERIES.GET_MY_SERIES,
                    { params: { pageNumber: pageToFetch, pageSize: PAGE_SIZE } }
                );
            } else {
                // Case 2: Xem của người khác (API public)
                if (!userId) return;
                response = await apiClient.get<PagedResult<SeriesListDto>>(
                    `/api/series/uploader/${userId}`,
                    { params: { pageNumber: pageToFetch, pageSize: PAGE_SIZE } }
                );
            }

            setSeriesList(response.data.items);
            setCurrentPage(response.data.pageNumber);
            setTotalPages(Math.ceil(response.data.totalRecords / PAGE_SIZE));

        } catch (err: any) {
            console.error("Failed to fetch series:", err);
            if (!isOwnProfile && err.response?.status === 404) {
                setSeriesList([]);
            } else {
                setError(err.response?.data?.message || "Could not load series.");
            }
        } finally {
            setIsLoadingData(false);
        }
    }, [isOwnProfile, currentUser, userId]);

    useEffect(() => {
        if (!authLoading) {
            if (!userId && !currentUser) {
                navigate('/login');
                return;
            }
            fetchSeries(currentPage);
        }
    }, [authLoading, userId, currentUser, fetchSeries, currentPage, navigate]);


    const handlePageChange = (page: number) => {
        if (page !== currentPage) {
            setCurrentPage(page);
            window.scrollTo(0, 0);
        }
    };

    if (authLoading) return <div>Loading...</div>;

    // Trường hợp xem profile người khác mà không tìm thấy user
    if (!isOwnProfile && error === "User not found.") {
        return <div style={{ padding: '40px', textAlign: 'center' }}>User not found.</div>;
    }

    if (!displayUser && !isOwnProfile) return <div>Loading profile info...</div>;


    if (isOwnProfile && !currentUser) return null;


    if (!displayUser) return <div>Something went wrong.</div>;

    return (
        <div className="profile-page-container">
            <header className={`profile-header ${isOwnProfile ? 'upload-hover-container' : ''}`}>
                <img
                    src={displayUser.backgroundImage || ''}
                    alt="User background"
                    className="profile-background-img"
                />
                {isOwnProfile && (
                    <>
                        <div className="profile-overlay"></div>
                        <div className="profile-upload-button">
                            <ImageUploadButton apiEndpoint={API_ROUTES.AUTH.UPLOAD_BACKGROUND} />
                        </div>
                    </>
                )}
                <div className="profile-white-backdrop"></div>
                <div className="profile-info">
                    <div className={`profile-avatar-container ${isOwnProfile ? 'upload-hover-container' : ''}`}>
                        <img
                            src={displayUser.avatar || ''}
                            alt="User avatar"
                            className="profile-avatar-img"
                        />
                        {isOwnProfile && (
                            <>
                                <div className="profile-overlay"></div>
                                <div className="profile-upload-button">
                                    <ImageUploadButton apiEndpoint={API_ROUTES.AUTH.UPLOAD_AVATAR} />
                                </div>
                            </>
                        )}
                    </div>
                    <h1 className="profile-username">{displayUser.username}</h1>
                </div>
            </header>

            <section className="profile-content">
                <div className="my-series-section">
                    <h2>{isOwnProfile ? "My Series" : `${displayUser.username}'s Series`}</h2>

                    {isLoadingData && <div>Loading series...</div>}

                  
                    {error && error !== "User not found." && <div style={{ color: 'red' }}>{error}</div>}

                    {!isLoadingData && seriesList.length === 0 && (
                        <div>
                            {isOwnProfile
                                ? "You have not uploaded any series yet."
                                : "This user hasn't uploaded any series yet."}
                        </div>
                    )}

                    {seriesList.length > 0 && (
                        <div className="series-grid">
                            {seriesList.map(series => (
                                <SeriesItem key={series.series_Id} series={series} type="grid" />
                            ))}
                        </div>
                    )}

                    {!isLoadingData && totalPages > 1 && (
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