import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { Suspense } from 'react'
import { Header } from './components/Header'
import { FeedPage } from './pages/FeedPage'
import { ArticleDetailPage } from './pages/ArticleDetailPage'
import { AdminSourcesPage } from './pages/AdminSourcesPage'
import './i18n'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 1000 * 60 * 2,
    },
  },
})

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <div className="min-h-screen bg-gray-50">
          <Suspense fallback={null}>
            <Header />
            <main>
              <Routes>
                <Route path="/" element={<FeedPage />} />
                <Route path="/articles/:id" element={<ArticleDetailPage />} />
                <Route path="/admin/sources" element={<AdminSourcesPage />} />
              </Routes>
            </main>
          </Suspense>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

export default App
