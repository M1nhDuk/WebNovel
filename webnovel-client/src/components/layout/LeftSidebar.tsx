import React from 'react';
import './CSS/LeftSidebar.css';
import {
    FaHome,
    FaHistory,
    FaBookmark,
    FaThLarge,
    FaChevronDown
} from 'react-icons/fa';
import { Link } from 'react-router-dom';

const LeftSidebar: React.FC = () => {
    return (
        <nav className="left-sidebar">
            <ul>
                <li>
                    <Link to="/" title="Home">
                        <FaHome className="sidebar-icon" />
                    </Link>
                </li>
                <li>
                    <Link to="/history" title="Reading History">
                        <FaHistory className="sidebar-icon" />
                    </Link>
                </li>
                <li>
                    <Link to="/bookmarks" title="Bookmarks">
                        <FaBookmark className="sidebar-icon" />
                    </Link>
                </li>
                <li>
                    <Link to="/browse" title="Browse All">
                        <FaThLarge className="sidebar-icon" />
                    </Link>
                </li>
                <li>
                    <a href="#" title="More">
                        <FaChevronDown className="sidebar-icon" />
                    </a>
                </li>
            </ul>
        </nav>
    );
};

export default LeftSidebar;