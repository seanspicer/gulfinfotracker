import { render, screen, fireEvent } from '@testing-library/react'
import { describe, it, expect, beforeEach } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { I18nextProvider } from 'react-i18next'
import i18n from '../i18n'
import { Header } from '../components/Header'

describe('Header', () => {
  beforeEach(() => {
    i18n.changeLanguage('en')
    document.documentElement.dir = 'ltr'
    document.body.classList.remove('font-arabic')
  })

  it('renders app name', () => {
    render(
      <MemoryRouter>
        <I18nextProvider i18n={i18n}>
          <Header />
        </I18nextProvider>
      </MemoryRouter>
    )
    expect(screen.getByText(/Gulf Info Tracker/i)).toBeInTheDocument()
  })

  it('AR toggle sets document.dir to rtl', async () => {
    render(
      <MemoryRouter>
        <I18nextProvider i18n={i18n}>
          <Header />
        </I18nextProvider>
      </MemoryRouter>
    )
    const btn = screen.getByRole('button', { name: /toggle language/i })
    fireEvent.click(btn)
    expect(document.documentElement.dir).toBe('rtl')
  })

  it('AR toggle persists to localStorage', () => {
    render(
      <MemoryRouter>
        <I18nextProvider i18n={i18n}>
          <Header />
        </I18nextProvider>
      </MemoryRouter>
    )
    const btn = screen.getByRole('button', { name: /toggle language/i })
    fireEvent.click(btn)
    expect(localStorage.getItem('i18nextLng')).toBe('ar')
  })
})
