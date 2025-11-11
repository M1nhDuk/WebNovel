import React from 'react';
import { Outlet } from 'react-router-dom';
import Header from './Header';
import LeftSidebar from './LeftSidebar';
import './CSS/MainLayout.css'; 

const MainLayout: React.FC = () => {
    return (
        <div className="app-layout">
            <Header />
            <LeftSidebar />

            <main className="main-content">
                <Outlet />
            </main>
        </div>
    );
};

export default MainLayout;