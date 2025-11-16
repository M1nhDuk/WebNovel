import React from 'react';
import GeneralCommentSection from './GeneralCommentSection';

interface ChapterCommentSectionProps {
    chapterId: number;
}


const ChapterCommentSection: React.FC<ChapterCommentSectionProps> = ({ chapterId }) => {
    return (
        <GeneralCommentSection
            chapterId={chapterId}
        />
    );
};

export default ChapterCommentSection;