import { useState, useEffect } from 'react';
import apiClient from './api/apiClient'; 
import { API_ROUTES } from './api/apiRoutes';
import type { SeriesListDto, PagedResult } from './types/series';

const PUBLICATION_SERVICE_URL = 'https://localhost:7263';

function App() {
    const [seriesList, setSeriesList] = useState<SeriesListDto[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchAllSeries = async () => {
            try {
                setIsLoading(true);
                //                                                         
                const response = await apiClient.get<PagedResult<SeriesListDto>>(API_ROUTES.SERIES.GET_ALL_SERIES);
                setSeriesList(response.data.items);
                setError(null);
            } catch (err) {
                setError('Không th? t?i danh sách truy?n. Hãy ??m b?o backend ?ang ch?y.');
                console.error(err);
            } finally {
                setIsLoading(false);
            }
        };

        fetchAllSeries();
    }, []);

    // *** B??C 2: Hàm tr? giúp ?? t?o URL ?nh ??y ?? ***
    const getImageUrl = (relativePath?: string) => {
        // N?u cover_images không null ho?c r?ng, hãy n?i nó v?i URL c?a service
        if (relativePath) {
            // Ví d?: "https://localhost:7263" + "/images/covers/abc.jpg"
            return `${PUBLICATION_SERVICE_URL}${relativePath}`;
        }
        
        return undefined;
    };

    return (
        <div>
            <h1>HomePage WebNovel</h1>

            {isLoading && <p>?ang t?i d? li?u...</p>}
            {error && <p style={{ color: 'red' }}>{error}</p>}

            {/* * *** B??C 3: C?p nh?t ph?n hi?n th? (Render) ***
       * Chúng ta s? thêm th? <img>
       */}
            {!isLoading && !error && (
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px' }}>
                    {seriesList.map((series) => (
                        <div key={series.series_Id} style={{ border: '1px solid #ccc', padding: '10px', borderRadius: '8px', width: '200px' }}>

                            {/* Th? Image (IMG) */}
                            <img
                                src={getImageUrl(series.cover_images)}
                                alt={`Bìa truy?n ${series.series_title}`}
                                style={{ width: '100%', height: 'auto', objectFit: 'cover' }}
                            />

                            {/* Tên truy?n */}
                            <h3 style={{ marginTop: '10px' }}>{series.series_title}</h3>

                            {/* Thông tin thêm (ví d?) */}
                            <p style={{ fontSize: '14px', color: '#555' }}>
                                {series.categoryName}
                            </p>

                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default App;