import { Routes, Route } from "react-router-dom";
import Header from "./components/Header";
import FeedPage from "./pages/FeedPage";
import ArticleDetailPage from "./pages/ArticleDetailPage";
import AdminSourcesPage from "./pages/AdminSourcesPage";

export default function App() {
  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <main className="mx-auto max-w-7xl px-4 py-6">
        <Routes>
          <Route path="/" element={<FeedPage />} />
          <Route path="/article/:id" element={<ArticleDetailPage />} />
          <Route path="/admin/sources" element={<AdminSourcesPage />} />
        </Routes>
      </main>
    </div>
  );
}
