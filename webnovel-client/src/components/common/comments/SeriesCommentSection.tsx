import React from 'react';
import GeneralCommentSection from './GeneralCommentSection';

interface SeriesCommentSectionProps {
    seriesId: number;
    totalCommentCount: number;
}

// Dùng cho Series Page
const SeriesCommentSection: React.FC<SeriesCommentSectionProps> = ({ seriesId, totalCommentCount }) => {
    return (
        <GeneralCommentSection
            seriesId={seriesId}
            totalCommentCount={totalCommentCount}
        />
    );
};

export default SeriesCommentSection;