import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

const apiUrl = process.env.services__api__https__0 ?? process.env.services__api__http__0 ?? 'http://localhost:5000'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT || '5173'),
    proxy: {
      '/api': {
        target: apiUrl,
        changeOrigin: true,
        secure: false,
      }
    }
  },
  define: {
    'import.meta.env.VITE_API_URL': JSON.stringify(process.env.services__api__https__0 ?? ''),
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})
