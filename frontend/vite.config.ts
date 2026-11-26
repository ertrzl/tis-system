import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // İnkişaf zamanı /api sorğuları .NET API-yə yönləndirilir (CORS problemi olmasın).
    // Docker-də API və React eyni ünvandan verildiyi üçün buna ehtiyac yoxdur.
    proxy: {
      '/api': 'http://localhost:5190',
    },
  },
})
