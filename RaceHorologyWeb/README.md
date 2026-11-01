# RaceHorologyWeb

New web client for Race Horology (React + TypeScript, built with Vite). It replaced the previous Vue client and is served by RaceHorology at `/` (port 8081).

- `npm ci` – install dependencies
- `npm run dev` – dev server with hot reload; `/api` and the `/api/LiveData` WebSocket are proxied to a running RaceHorology (default `http://localhost:8081`, override with `RH_SERVER`)
- `npm run build` – production build into `dist/`

Building `RaceHorology.csproj` runs `npm ci` / `npm run build` automatically and copies the result to `webroot\`. Pass `/p:SkipWebBuild=true` to skip this if Node.js is not installed.
