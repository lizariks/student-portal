import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      '/api/discussionthread': {
        target: 'https://localhost:7106',
        changeOrigin: true,
        secure: false,
      },
      '/api/comment': {
        target: 'https://localhost:7106',
        changeOrigin: true,
        secure: false,
      },
      '/api': {
        target: 'https://localhost:7048',
        changeOrigin: true,
        secure: false,
      },
    },
  },
});