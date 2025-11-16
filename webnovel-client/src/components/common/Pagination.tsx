import React from 'react';
import './CSS/Pagination.css'; 

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
}

const Pagination: React.FC<PaginationProps> = ({ currentPage, totalPages, onPageChange }) => {

    const getPageNumbers = () => {
        const pageNumbers: (number | string)[] = [];
        const maxPagesToShow = 3; 
        const ellipsis = '...';


        // Nếu tổng số trang nhỏ, hiển thị tất cả
        if (totalPages <= maxPagesToShow + 4) { 
            for (let i = 1; i <= totalPages; i++) {
                pageNumbers.push(i);
            }
        } else {

            pageNumbers.push(1);

            let start = Math.max(2, currentPage - 1);
            let end = Math.min(totalPages - 1, currentPage + 1);

            if (currentPage > 3) {
                pageNumbers.push(ellipsis);
            }

            if (currentPage <= 2) {
                start = 2;
                end = 3;
            }
            if (currentPage >= totalPages - 1) {
                start = totalPages - 2;
                end = totalPages - 1;
            }

            for (let i = start; i <= end; i++) {
                pageNumbers.push(i);
            }

            if (currentPage < totalPages - 2) {
                pageNumbers.push(ellipsis);
            }


            pageNumbers.push(totalPages);
        }

        return pageNumbers;
    };

    const pages = getPageNumbers();

    return (
        <ul className="pagination-container">
            <li className="pagination-item">
                <button
                    className="pagination-button"
                    onClick={() => onPageChange(currentPage - 1)}
                    disabled={currentPage === 1}
                >
                    Previous
                </button>
            </li>
            {pages.map((page, index) => (
                <li key={index} className="pagination-item">
                    {page === '...' ? (
                        <span className="pagination-ellipsis">...</span>
                    ) : (
                        <button
                            className={`pagination-button ${page === currentPage ? 'active' : ''}`}
                            onClick={() => onPageChange(page as number)}
                        >
                            {page}
                        </button>
                    )}
                </li>
            ))}
            <li className="pagination-item">
                <button
                    className="pagination-button"
                    onClick={() => onPageChange(currentPage + 1)}
                    disabled={currentPage === totalPages}
                >
                    Next
                </button>
            </li>
        </ul>
    );
};

export default Pagination;