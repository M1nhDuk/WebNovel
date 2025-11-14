import React from 'react';
import { Link } from 'react-router-dom';
import type { SeriesListDto } from '../../types/series';
import './SeriesItem.css'; 

const GATEWAY_URL = 'https://localhost:8000';

interface SeriesItemProps {
    series: SeriesListDto;
    type: 'slider' | 'grid';
}

const SeriesItem: React.FC<SeriesItemProps> = ({ series, type }) => {
    const getImageUrl = (coverPath: string | undefined) => {
        if (!coverPath) {
            return 'img/nocover.jpg';
        }
        const formattedPath = coverPath.startsWith('/') ? coverPath : `/${coverPath}`;
        return `${GATEWAY_URL}${formattedPath}`;
    };


    const itemClass = type === 'slider' ? 'popular-thumb-item' : 'thumb-item-flow';

    return (
        <div className={itemClass}>
            <div className="thumb-wrapper">
                <Link to={`/series/${series.series_Id}`} title={series.series_title}>
                    <div className="a6-ratio">
                        <div
                            className="content img-in-ratio"
                            style={{ backgroundImage: `url(${getImageUrl(series.cover_images)})` }}
                        ></div>
                    </div>
                </Link>
                {type === 'grid' && (
                    <div className="thumb-detail">
                        <div className="thumb_attr chapter-title">
                            <a href="#" title="Latest Chapter">Latest Chapter...</a>
                        </div>
                        <div className="thumb_attr volume-title">Volume 1</div>
                    </div>
                )}
            </div>
            <div className="series-title">
                <Link to={`/series/${series.series_Id}`} title={series.series_title}>
                    {series.series_title}
                </Link>
            </div>
        </div>
    );
};

export default SeriesItem;