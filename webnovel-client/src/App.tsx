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
import ReadingHistoryPage from './pages/ReadingHistoryPage/ReadingHistoryPage'
import TagsPage from './pages/TagsPage/TagsPage'
import AdminDashboardPage from './pages/AdminPage/AdminDashboardPage'
import ChapterDetailPage from './pages/ChapterDetailPage/ChapterDetailPage'
import { ReaderSettingsProvider } from './context/ReaderSettingsContext'
import FavoritesPage from './pages/FavoritesPage/FavoritesPage'
import BookmarksPage from './pages/BookmarksPage/BookmarksPage';
import NotificationsPage from './pages/NotificationsPage/NotificationsPage';

import ManageSeriesPage from './pages/ManageSeriesPage/ManageSeriesPage'


import MainLayout from './components/layout/MainLayout'


function App() {
    return (
        <ReaderSettingsProvider>
            <Routes>

                <Route path="/login" element={<LoginPage />} />

                <Route path="/register" element={<RegisterPage />} />

                <Route path="/forgot-password" element={<ForgotPasswordPage />} />

                <Route path="/" element={<MainLayout />}>

                    <Route index element={<HomePage />} />

                    <Route path="browse" element={<BrowsePage />} />

                    <Route path="tags" element={<TagsPage />} />

                    <Route path="series/:id" element={<SeriesDetailPage />} />

                    <Route path="create-series" element={<CreateSeriesPage />} />

                    <Route path="/manage/series/:id" element={<ManageSeriesPage />} />

                    <Route path="series/:seriesId/novel/:novelId" element={<NovelDetailPage />} />

                    <Route path="series/:seriesId/chapter/:chapterId" element={<ChapterDetailPage />} />

                    <Route path="/profile" element={<ProfilePage />} />

                    <Route path="/user/:userId" element={<ProfilePage />} />

                    <Route path="/account-settings" element={<AccountSettingsPage />} />

                    <Route path="history" element={<ReadingHistoryPage />} />

                    <Route path="/admin" element={<AdminDashboardPage />} />

                    <Route path="favorites" element={<FavoritesPage />} />

                    <Route path="notifications" element={<NotificationsPage />} />

                    <Route path="bookmarks" element={<BookmarksPage />} />

                </Route>

            </Routes>
        </ReaderSettingsProvider>
    )
}

export default App