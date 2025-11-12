import React, { useState, useEffect } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { PagedResult, SeriesListDto } from '../../types/series';
import SeriesItem from '../../components/series/SeriesItem';
import { useAuth } from '../../hooks/useAuth';
import './ProfilePage.css';
import { Link } from 'react-router-dom';

const ProfilePage: React.FC = () => {
    const { user, isLoading: userLoading } = useAuth(); 
    const [seriesList, setSeriesList] = useState<SeriesListDto[]>([]);
    const [loadingSeries, setLoadingSeries] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
     
        if (user) {
            const fetchMySeries = async () => {
                setLoadingSeries(true);
                setError(null);
                try {
                    const response = await apiClient.get<PagedResult<SeriesListDto>>(
                        API_ROUTES.SERIES.GET_MY_SERIES
                    );
                    setSeriesList(response.data.items);
                } catch (err: any) {
                    console.error("Failed to fetch user series:", err);
                    setError(err.response?.data?.message || "Could not load your series.");
                } finally {
                    setLoadingSeries(false);
                }
            };

            fetchMySeries();
        } else if (!userLoading) {
            
            setLoadingSeries(false);
            setError("You must be logged in to view this page.");
        }
    }, [user, userLoading]); 

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
            <header className="profile-header">
                <img
                    src={user.backgroundImage || ''} 
                    alt="User background"
                    className="profile-background-img"
                />
                <div className="profile-info">
                    <img
                        src={user.avatar || ''} 
                        alt="User avatar"
                        className="profile-avatar-img"
                    />
                    <h1 className="profile-username">{user.username}</h1>
                </div>
            </header>

            <section className="profile-content">
                <div className="my-series-section">
                    <h2>My Series</h2>
                    {loadingSeries && <div>Loading series...</div>}
                    {error && <div style={{ color: 'red' }}>{error}</div>}
                    {!loadingSeries && !error && (
                        <div className="series-grid">
                            {seriesList.length > 0 ? (
                                seriesList.map(series => (
                                    <SeriesItem key={series.series_Id} series={series} type="grid" />
                                ))
                            ) : (
                                <div>You have not uploaded any series yet.</div>
                            )}
                        </div>
                    )}
                </div>
            </section>
        </div>
    );
};

export default ProfilePage;