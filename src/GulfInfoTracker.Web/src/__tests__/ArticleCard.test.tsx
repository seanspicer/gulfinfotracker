import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { I18nextProvider } from 'react-i18next'
import i18n from '../i18n'
import { ArticleCard } from '../components/ArticleCard'
import type { ArticleListItem } from '../lib/apiClient'

const mockArticle: ArticleListItem = {
  id: '123',
  pluginId: 'uae-gov',
  headlineEn: 'Test Headline',
  headlineAr: null,
  summaryEn: 'Test summary text',
  summaryAr: null,
  sourceUrl: 'https://example.com',
  publishedAt: new Date().toISOString(),
  country: 'UAE',
  credibilityScore: 85,
  fullText: false,
  translated: false,
  topicIds: ['T1'],
}

describe('ArticleCard', () => {
  it('renders headline', () => {
    render(
      <MemoryRouter>
        <I18nextProvider i18n={i18n}>
          <ArticleCard article={mockArticle} />
        </I18nextProvider>
      </MemoryRouter>
    )
    expect(screen.getByText('Test Headline')).toBeInTheDocument()
  })

  it('renders correct credibility tier label for score 85', () => {
    render(
      <MemoryRouter>
        <I18nextProvider i18n={i18n}>
          <ArticleCard article={mockArticle} />
        </I18nextProvider>
      </MemoryRouter>
    )
    expect(screen.getByText(/verified/i)).toBeInTheDocument()
    expect(screen.getByText('85')).toBeInTheDocument()
  })
})
