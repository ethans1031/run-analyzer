import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import UploadPage from './Pages/UploadPage.tsx'
import RunListPage from './Pages/RunListPage.tsx'
import RunDetailPage from './Pages/RunDetailPage.tsx'
import NavBar from './Components/NavBar.tsx'
import StatsPage from './Pages/StatsPage.tsx'
import "./Styles/globals.css"
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { CatalogProvider } from './Context/context.tsx'

createRoot(document.getElementById('root')!).render(
  <BrowserRouter>
    <CatalogProvider>
      <NavBar />
      <div className="container">
        <Routes>
          <Route path="/" element={<RunListPage />} />
          <Route path="/runs/:id" element={<RunDetailPage />} />
          <Route path="/upload" element={<UploadPage />} />
          <Route path="/stats" element={<StatsPage />} />
        </Routes>
      </div>
    </CatalogProvider>
  </BrowserRouter>

)
