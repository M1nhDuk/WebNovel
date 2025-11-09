import './App.css'
import HomePage from './pages/HomePage/HomePage'
import LeftSidebar from './components/layout/LeftSidebar'
import Header from './components/layout/Header'
import { Routes, Route } from 'react-router-dom'
import BrowsePage from './pages/BrowsePage/BrowsePage'
import SeriesDetailPage from './pages/SeriesDetailPage/SeriesDetailPage'

function App() {
    return (
        <div className="app-layout">
            <Header />
            <LeftSidebar />

            <main className="main-content">
                <Routes>
                    <Route path="/" element={<HomePage />} />

                    <Route path="/browse" element={<BrowsePage />} />

                    <Route path="/series/:id" element={<SeriesDetailPage />} />

                </Routes>
            </main>
        </div>
    )
}

export default App