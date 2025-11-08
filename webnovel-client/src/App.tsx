import './App.css'
import HomePage from './pages/HomePage/HomePage'
import LeftSidebar from './components/layout/LeftSidebar'
import Header from './components/layout/Header'
import { Routes, Route } from 'react-router-dom'

function App() {
    return (
        <div className="app-layout">
            <Header />
            <LeftSidebar />

            <main className="main-content">
                <Routes>
                    <Route path="/" element={<HomePage />} />                 
                </Routes>
            </main>
        </div>
    )
}

export default App