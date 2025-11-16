import React from 'react';
import GeneralCommentSection from './GeneralCommentSection';

interface ChapterCommentSectionProps {
    chapterId: number;
}

// Component ChapterCommentSection chỉ đơn giản là gọi GeneralCommentSection với prop chapterId
const ChapterCommentSection: React.FC<ChapterCommentSectionProps> = ({ chapterId, totalCommentCount }) => {
    return (
        <GeneralCommentSection
            chapterId={chapterId}
        />
    );
};

export default ChapterCommentSection;