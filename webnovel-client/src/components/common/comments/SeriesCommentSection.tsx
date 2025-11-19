import React from 'react';
import GeneralCommentSection from './GeneralCommentSection';

interface SeriesCommentSectionProps {
    seriesId: number;
}

const SeriesCommentSection: React.FC<SeriesCommentSectionProps> = ({ seriesId }) => {
    return (
        <GeneralCommentSection
            seriesId={seriesId}
        />
    );
};

export default SeriesCommentSection;