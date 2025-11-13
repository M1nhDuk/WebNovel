import './App.css'
import { Routes, Route } from 'react-router-dom'

import HomePage from './pages/HomePage/HomePage'
import BrowsePage from './pages/BrowsePage/BrowsePage'
import SeriesDetailPage from './pages/SeriesDetailPage/SeriesDetailPage'
import NovelDetailPage from './pages/NovelDetailPage/NovelDetailPage'
import LoginPage from './pages/LoginPage/LoginPage'
import RegisterPage from './pages/RegisterPage/RegisterPage'
import ForgotPasswordPage from './pages/ForgotPasswordPage/ForgotPasswordPage'
import AccountSettingsPage from './pages/AccountSettingsPage/AccountSettingsPage'
import ProfilePage from './pages/ProfilePage/ProfilePage'
import CreateSeriesPage from './pages/CreateSeriesPage/CreateSeriesPage'

import MainLayout from './components/layout/MainLayout'


function App() {
    return (
        <Routes>

            <Route path="/login" element={<LoginPage />} />

            <Route path="/register" element={<RegisterPage />} />

            <Route path="/forgot-password" element={<ForgotPasswordPage />} />

            <Route path="/" element={<MainLayout />}>

                <Route index element={<HomePage />} />

                <Route path="browse" element={<BrowsePage />} />

                <Route path="series/:id" element={<SeriesDetailPage />} />

                <Route path="create-series" element={<CreateSeriesPage />} />

                <Route path="series/:seriesId/novel/:novelId" element={<NovelDetailPage />} />

                <Route path="/profile" element={<ProfilePage />} />

                <Route path="/account-settings" element={<AccountSettingsPage />} />

            </Route>

        </Routes>
    )
}

export default App