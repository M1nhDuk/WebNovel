import React from 'react';
import GeneralCommentSection from './GeneralCommentSection';

interface SeriesCommentSectionProps {
    seriesId: number;
}

// Dùng cho Series Page
const SeriesCommentSection: React.FC<SeriesCommentSectionProps> = ({ seriesId }) => {
    return (
        <GeneralCommentSection
            seriesId={seriesId}
        />
    );
};

export default SeriesCommentSection;