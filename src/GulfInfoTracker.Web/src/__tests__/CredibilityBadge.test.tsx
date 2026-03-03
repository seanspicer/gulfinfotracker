import { render, screen } from '@testing-library/react'
import { describe, it, expect } from 'vitest'
import { I18nextProvider } from 'react-i18next'
import i18n from '../i18n'
import { CredibilityBadge } from '../components/CredibilityBadge'

function renderBadge(score: number | null) {
  return render(
    <I18nextProvider i18n={i18n}>
      <CredibilityBadge score={score} />
    </I18nextProvider>
  )
}

describe('CredibilityBadge', () => {
  it('shows green Verified for score >= 80', () => {
    renderBadge(80)
    expect(screen.getByText(/verified/i)).toBeInTheDocument()
  })

  it('shows blue Credible for score 79', () => {
    renderBadge(79)
    expect(screen.getByText(/credible/i)).toBeInTheDocument()
  })

  it('shows amber Uncertain for score 40', () => {
    renderBadge(40)
    expect(screen.getByText(/uncertain/i)).toBeInTheDocument()
  })

  it('shows red Low for score 39', () => {
    renderBadge(39)
    expect(screen.getByText(/low/i)).toBeInTheDocument()
  })

  it('shows grey pending for null score', () => {
    renderBadge(null)
    expect(screen.getByText(/pending/i)).toBeInTheDocument()
  })
})
