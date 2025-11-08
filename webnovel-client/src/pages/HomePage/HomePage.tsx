import { useState, useEffect } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { PagedResult, SeriesListDto } from '../../types/series';
import './HomePage.css';

const GATEWAY_URL = 'https://localhost:8000';

interface SeriesItemProps {
    series: SeriesListDto;
    type: 'slider' | 'grid';
}

const SeriesItem: React.FC<SeriesItemProps> = ({ series, type }) => {
    const getImageUrl = (coverPath: string | undefined) => {
        if (!coverPath) {
            return 'path/to/default/placeholder.png';
        }
        const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };

    const itemClass = type === 'slider' ? 'popular-thumb-item' : 'thumb-item-flow';

    return (
        <div className={itemClass}>
            <div className="thumb-wrapper">
                <a href={`/series/${series.series_Id}`} title={series.series_title}>
                    <div className="a6-ratio">
                        <div
                            className="content img-in-ratio"
                            style={{ backgroundImage: `url(${getImageUrl(series.cover_images)})` }}
                        ></div>
                    </div>
                </a>
                {type === 'grid' && (
                    <div className="thumb-detail">
                        <div className="thumb_attr chapter-title">
                            <a href="#" title="Latest Chapter">Latest Chapter...</a>
                        </div>
                        <div className="thumb_attr volume-title">Volume 1</div>
                    </div>
                )}
            </div>
            <div className="thumb_attr series-title">
                <a href={`/series/${series.series_Id}`} title={series.series_title}>
                    {series.series_title}
                </a>
            </div>
        </div>
    );
};

// --- Reusable Section Component ---
interface SeriesSectionProps {
    title: string;
    subTitle: string;
    seriesList: SeriesListDto[];
    type: 'slider' | 'grid';
    seeMoreLink: string;
}

const SeriesSection: React.FC<SeriesSectionProps> = ({ title, subTitle, seriesList, type, seeMoreLink }) => {
    return (
        <section className={`index-section ${type === 'slider' ? 'daily-recent_views' : 'thumb-section-flow'}`}>
            <header className="section-title">
                <span className="sts-bold">{title}</span>
                <span className="sts-empty">{subTitle}</span>
            </header>
            <main className={`row ${type === 'slider' ? 'slider' : ''}`}>
                {seriesList.map(series => (
                    <SeriesItem key={series.series_Id} series={series} type={type} />
                ))}

                <div className={`thumb-item-flow see-more ${type === 'grid' ? 'col-4 col-md-3 col-lg-2' : ''}`}>
                    <div className="thumb-wrapper">
                        <a href={seeMoreLink}>
                            <div className="a6-ratio">
                                <div className="content img-in-ratio" style={{ backgroundImage: "url('/img/nocover.jpg')" }}></div>
                            </div>
                            <div className="thumb-see-more">
                                <div className="see-more-inside">
                                    <div className="see-more-content">
                                        <div className="see-more-icon">&rarr;</div>
                                        <div className="see-more-text">See More</div>
                                    </div>
                                </div>
                            </div>
                        </a>
                    </div>
                </div>
            </main>
        </section>
    );
};

// --- MAIN HOME PAGE COMPONENT ---
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
                    { params: { pageNumber: 1, pageSize: 50 } } 
                );
                setSeriesList(response.data.items);
            } catch (err) {
                setError('Could not load series.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };
        fetchSeries();
    }, []);

    if (loading) {
        return <div>Loading...</div>;
    }

    if (error) {
        return <div>{error}</div>;
    }

    const featuredSeries = seriesList.slice(0, 8);
    const webNovels = seriesList.filter(s => s.categoryName === "Translated").slice(0, 6);
    const classicNovels = seriesList.filter(s => s.categoryName === "Original").slice(0, 6);
    const selfComposed = seriesList.filter(s => s.categoryName === "Self-Composed").slice(0, 6);


    const webNovelSeries = seriesList.filter(s => s.type === "Series").slice(0, 6);
    const classicNovelSeries = seriesList.filter(s => s.type === "TRADITIONAL").slice(0, 6);

    return (
        <main id="mainpart" className="at-index">
            <div className="container" style={{ paddingTop: '20px' }}>
                <div className="row">
                    <div className="col-12">

                        {/* Featured Section */}
                        <SeriesSection
                            title="Featured"
                            subTitle="Series"
                            seriesList={featuredSeries}
                            type="slider"
                            seeMoreLink="/list?sort=views"
                        />

                        {/* === (M?I) THÊM CÁC M?C L?C THEO TYPE === */}
                        <SeriesSection
                            title="Web"
                            subTitle="Novels"
                            seriesList={webNovelSeries}
                            type="grid"
                            seeMoreLink="/list?type=Series"
                        />
                        <SeriesSection
                            title="Classic"
                            subTitle="Novels"
                            seriesList={classicNovelSeries}
                            type="grid"
                            seeMoreLink="/list?type=TRADITIONAL"
                        />

                        {/* Translate Section */}
                        <SeriesSection
                            title="Translated"
                            subTitle="Publications"
                            seriesList={webNovels}                                                     
                            type="grid"
                            seeMoreLink="/list?category=Translated" 
                        />

                        {/* Classic Novels Section */}
                        <SeriesSection
                            title="Original"
                            subTitle="Publications"
                            seriesList={classicNovels} 
                            type="grid"
                            seeMoreLink="/list?category=Original" 
                        />

                        {/* Originals Section */}
                        <SeriesSection
                            title="Self-Composed"
                            subTitle="Creations"
                            seriesList={selfComposed} 
                            type="grid"
                            seeMoreLink="/list?category=Self-Composed" 
                        />

                    </div>
                </div>
            </div>
        </main>
    );
};

export default HomePage;