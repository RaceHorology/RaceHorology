import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// RaceHorology's DSVAlpin2HTTPServer listens on port 8081 (see MainWindow.xaml.cs).
// Override with RH_SERVER=http://host:port when the app runs elsewhere.
const rhServer = process.env.RH_SERVER ?? 'http://localhost:8081'

export default defineConfig({
  plugins: [react()],
  // The new client is served next to the old one under <AppDir>\webroot\next\
  base: '/next/',
  build: {
    // The MSBuild target in RaceHorology.csproj passes the output folder via RH_WEB_OUTDIR
    outDir: process.env.RH_WEB_OUTDIR ?? 'dist',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': {
        target: rhServer,
        changeOrigin: true,
        ws: true,
      },
    },
  },
})
