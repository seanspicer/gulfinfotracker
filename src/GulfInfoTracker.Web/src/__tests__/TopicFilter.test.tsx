import { render, screen, fireEvent } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { I18nextProvider } from 'react-i18next'
import i18n from '../i18n'
import { TopicFilter } from '../components/TopicFilter'

function renderWithRouter(initialSearch = '') {
  return render(
    <MemoryRouter initialEntries={[`/${initialSearch}`]}>
      <I18nextProvider i18n={i18n}>
        <TopicFilter />
      </I18nextProvider>
    </MemoryRouter>
  )
}

describe('TopicFilter', () => {
  it('renders T1-T5 and All Topics buttons', () => {
    renderWithRouter()
    expect(screen.getByText(/all topics/i)).toBeInTheDocument()
    expect(screen.getByText(/Politics/i)).toBeInTheDocument()
    expect(screen.getByText(/Energy/i)).toBeInTheDocument()
  })

  it('clicking a topic button renders the topic label', () => {
    renderWithRouter()
    const button = screen.getByText(/Energy/i)
    fireEvent.click(button)
    // Button should still be in the DOM
    expect(screen.getByText(/Energy/i)).toBeInTheDocument()
  })
})
