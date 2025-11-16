import React from 'react';
import GeneralCommentSection from './GeneralCommentSection';

interface ChapterCommentSectionProps {
    chapterId: number;
    totalCommentCount: number;
}

// Component ChapterCommentSection chỉ đơn giản là gọi GeneralCommentSection với prop chapterId
const ChapterCommentSection: React.FC<ChapterCommentSectionProps> = ({ chapterId, totalCommentCount }) => {
    return (
        <GeneralCommentSection
            chapterId={chapterId}
            totalCommentCount={totalCommentCount}
        />
    );
};

export default ChapterCommentSection;