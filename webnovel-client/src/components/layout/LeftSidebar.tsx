import React from 'react';
import './CSS/LeftSidebar.css';
import {
    FaHome,
    FaHistory,
    FaBookmark,
    FaThLarge,
    FaChevronDown
} from 'react-icons/fa'; 

const LeftSidebar: React.FC = () => {
    return (
        <nav className="left-sidebar">
            <ul>
                <li>
                    <a href="/" title="Home">
                        <FaHome className="sidebar-icon" />
                    </a>
                </li>
                <li>
                    <a href="/history" title="Reading History">
                        <FaHistory className="sidebar-icon" />
                    </a>
                </li>
                <li>
                    <a href="/bookmarks" title="Bookmarks">
                        <FaBookmark className="sidebar-icon" />
                    </a>
                </li>
                <li>
                    <a href="/categories" title="Categories">
                        <FaThLarge className="sidebar-icon" />
                    </a>
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