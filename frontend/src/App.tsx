import { lazy, Suspense } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { CatalogProvider } from "./context/CatalogProvider";
import NavBar from "./components/NavBar";
import RunListPage from "./pages/RunListPage";
import RunDetailPage from "./pages/RunDetailPage";
import UploadPage from "./pages/UploadPage";
import ChoiceStatsPage from "./pages/ChoiceStatsPage";

// Loaded on demand so the charting library isn't in the initial bundle.
const StatsPage = lazy(() => import("./pages/StatsPage"));

function App() {
    return (
        <BrowserRouter>
            <CatalogProvider>
                <NavBar />
                <main className="container">
                    <Suspense fallback={<div>Loading...</div>}>
                        <Routes>
                            <Route path="/" element={<RunListPage />} />
                            <Route path="/runs/:id" element={<RunDetailPage />} />
                            <Route path="/upload" element={<UploadPage />} />
                            <Route path="/stats" element={<StatsPage />} />
                            {/* key forces a fresh page (and fresh state) when switching between the two */}
                            <Route path="/cards" element={<ChoiceStatsPage key="card" kind="card" />} />
                            <Route path="/relics" element={<ChoiceStatsPage key="relic" kind="relic" />} />
                        </Routes>
                    </Suspense>
                </main>
            </CatalogProvider>
        </BrowserRouter>
    );
}

export default App;
