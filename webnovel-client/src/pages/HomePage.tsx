import { useState, useEffect } from 'react';
import apiClient from '../api/apiClient';
import { API_ROUTES } from '../api/apiRoutes';
import type { PagedResult, SeriesListDto } from '../types/series';
import './HomePage.css';


const GATEWAY_URL = 'https://localhost:8000';

const HomePage = () => {
    const [seriesList, setSeriesList] = useState<SeriesListDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchSeries = async () => {
            try {
                setLoading(true);
                setError(null);

                const response = await apiClient.get<PagedResult<SeriesListDto>>(
                    API_ROUTES.SERIES.GET_ALL_SERIES,
                    {
                        params: { pageNumber: 1, pageSize: 12 }
                    }
                );
                setSeriesList(response.data.items);
            } catch (err) {
                setError('Can not load series.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };
        fetchSeries();
    }, []);

    const getImageUrl = (coverPath: string | undefined) => {
        if (!coverPath) {
            return 'path/to/default/placeholder.png';
        }
        const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    if (loading) {
        return <div>?ang t?i...</div>;
    }

    if (error) {
        return <div>{error}</div>;
    }

    return (
        <div className="homepage-container">
            <div className="tabs">
                <span className="tab-item">Series</span>
                <span className="tab-item active">M?I NH?T</span>
            </div>
            <div className="series-grid">
                {seriesList.map((series) => (
                    <div key={series.series_Id} className="series-item">
                        <div className="series-cover-wrapper">
                            <img
                                src={getImageUrl(series.cover_images)}
                                alt={series.series_title}
                                className="series-cover-image"
                            />
                        </div>
                        <p className="series-title">{series.series_title}</p>
                    </div>
                ))}
                <div className="series-item see-more">
                    <div className="series-cover-wrapper">
                        <span>&rarr;</span>
                    </div>
                    <p className="series-title">Xem thêm</p>
                </div>
            </div>
        </div>
    );
};

export default HomePage;