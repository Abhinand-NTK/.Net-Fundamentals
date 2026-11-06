import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// The ASP.NET Core API's "http" launch profile (PersonalDetailsApi/Properties/launchSettings.json).
const API_URL = 'http://localhost:5056';

// The dev server forwards API calls to ASP.NET Core. The browser only ever talks to
// http://localhost:5173, so there is no cross-origin request and no CORS set-up is needed.
const proxy = {
  '/api': API_URL,
  '/health': API_URL,
};

export default defineConfig({
  plugins: [react()],
  server: { port: 5173, strictPort: true, proxy },
  preview: { port: 4173, proxy },
});
