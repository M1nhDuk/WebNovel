import { useState, useEffect, useRef, useLayoutEffect } from 'react';
import apiClient from '../../api/apiClient';
import { API_ROUTES } from '../../api/apiRoutes';
import type { PagedResult, SeriesListDto } from '../../types/series';
import './HomePage.css';
import { Link } from 'react-router-dom';
import SeriesItem from '../../components/series/SeriesItem';


interface SeriesSectionProps {
    title: string;
    subTitle: string;
    seriesList: SeriesListDto[];
    type: 'slider' | 'grid';
    seeMoreLink: string;
}

const SeriesSection: React.FC<SeriesSectionProps> = ({ title, subTitle, seriesList, type, seeMoreLink }) => {

    const [isOverflowing, setIsOverflowing] = useState(false);
    const mainRowRef = useRef<HTMLElement>(null);

    const [isDown, setIsDown] = useState(false);
    const [startX, setStartX] = useState(0);
    const [scrollLeft, setScrollLeft] = useState(0);

    useLayoutEffect(() => {
        const checkOverflow = () => {
            if (mainRowRef.current) {
                const hasOverflow = mainRowRef.current.scrollWidth > mainRowRef.current.clientWidth;

                if (hasOverflow !== isOverflowing) {
                    setIsOverflowing(hasOverflow);
                }
            }
        };

        const timer = setTimeout(checkOverflow, 100);
        window.addEventListener('resize', checkOverflow);

        return () => {
            clearTimeout(timer);
            window.removeEventListener('resize', checkOverflow);
        };
    }, [seriesList, isOverflowing]);

    //MOUSE INTERACTION
    const handleMouseDown = (e: React.MouseEvent) => {
        if (!mainRowRef.current) return;
        setIsDown(true);
        // L?u v? trí b?t ??u và v? trí thanh cu?n hi?n t?i
        setStartX(e.pageX - mainRowRef.current.offsetLeft);
        setScrollLeft(mainRowRef.current.scrollLeft);
    };

    const handleMouseLeave = () => {
        setIsDown(false);
    };

    const handleMouseUp = () => {
        setIsDown(false);
    };

    const handleMouseMove = (e: React.MouseEvent) => {
        if (!isDown || !mainRowRef.current) return;
        e.preventDefault(); 
        const x = e.pageX - mainRowRef.current.offsetLeft;
        const walk = (x - startX) * 2; 
        mainRowRef.current.scrollLeft = scrollLeft - walk;
    };


    return (
        <section className={`index-section thumb-section-flow`}>
            <header className="section-title">
                <span className="sts-bold">{title}</span>
                <span className="sts-empty">{subTitle}</span>
            </header>

            <main className="row"
                ref={mainRowRef}
                onMouseDown={handleMouseDown}
                onMouseLeave={handleMouseLeave}
                onMouseUp={handleMouseUp}
                onMouseMove={handleMouseMove}
            >
                
                {seriesList.map(series => (
                    <SeriesItem key={series.series_Id} series={series} type={type} />
                ))}

                {isOverflowing && (
                    <div className="thumb-item-flow see-more">
                        <div className="thumb-wrapper">
                            <Link to={seeMoreLink}>
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
                            </Link>
                        </div>
                    </div>
                )}
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

    // Logic slice
    const featuredSeries = seriesList.slice(0, 5);

    const webNovels = seriesList.filter(s => s.categoryName === "Translated").slice(0, 9);
    const classicNovels = seriesList.filter(s => s.categoryName === "Original").slice(0, 9);
    const selfComposed = seriesList.filter(s => s.categoryName === "Self-Composed").slice(0, 9);

    const webNovelSeries = seriesList.filter(s => s.type === "Series").slice(0, 9);
    const classicNovelSeries = seriesList.filter(s => s.type === "TRADITIONAL").slice(0, 9);

    return (
        <main id="mainpart" className="at-index">
            <div className="container" style={{ paddingTop: '20px' }}>
                <div className="row">
                    <div className="col-12">

                        <SeriesSection
                            title="Featured"
                            subTitle="Series"
                            seriesList={featuredSeries}
                            type="grid"
                            seeMoreLink="/browse"
                        />

                        {/*TYPE */}
                        <SeriesSection
                            title="Web"
                            subTitle="Novels"
                            seriesList={webNovelSeries}
                            type="grid"
                            seeMoreLink="/browse"
                        />
                        <SeriesSection
                            title="Classic"
                            subTitle="Novels"
                            seriesList={classicNovelSeries}
                            type="grid"
                            seeMoreLink="/browse"
                        />

                        {/* Translate Section */}
                        <SeriesSection
                            title="Translated"
                            subTitle="Publications"
                            seriesList={webNovels}
                            type="grid"
                            seeMoreLink="/browse"
                        />

                        {/* Classic Novels Section */}
                        <SeriesSection
                            title="Original"
                            subTitle="Publications"
                            seriesList={classicNovels}
                            type="grid"
                            seeMoreLink="/browse"
                        />

                        {/* Originals Section */}
                        <SeriesSection
                            title="Self-Composed"
                            subTitle="Creations"
                            seriesList={selfComposed}
                            type="grid"
                            seeMoreLink="/browse"
                        />

                    </div>
                </div>
            </div>
        </main>
    );
};

export default HomePage;