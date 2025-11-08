import './App.css'
import HomePage from './pages/HomePage/HomePage'
import LeftSidebar from './components/layout/LeftSidebar'
import Header from './components/layout/Header' 

function App() {
    return (
        <div className="app-layout">
            <Header /> 
            <LeftSidebar />

            <main className="main-content">
                <HomePage />
            </main>
        </div>
    )
}

export default App