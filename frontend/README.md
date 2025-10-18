# Frontend: environment & deployment notes

This file documents how the Angular frontend handles environment configuration for development and production, and how to override the API base URL at container runtime using Docker / docker-compose + a `.env` file.

Summary

- `ng serve` / development builds use `src/environments/environment.development.ts` (configured via `angular.json` file replacements).
- Production builds (used by the Dockerfile) use `src/environments/environment.ts`.
- At runtime, the production container generates `assets/env.js` from an environment variable (`API_BASE_URL`) so you can change the API URL without rebuilding the app.

Files of interest

- `src/environments/environment.development.ts` — development environment values used by `ng serve`.
- `src/environments/environment.ts` — production defaults used at build time.
- `docker-entrypoint.sh` — entrypoint in the frontend image that writes `/usr/share/nginx/html/assets/env.js` from `API_BASE_URL`.
- `frontend/Dockerfile` — copies the built files into nginx and uses the entrypoint.
- `docker-compose.yml` — configured to pass `API_BASE_URL` from `.env` into the frontend container.

How to set the API base URL for Docker deployments

1. In the repo root create/modify `.env` and set the variable:

```
API_BASE_URL=https://api.myproduction.site/
```

2. Start the stack with docker-compose (no frontend rebuild required):

```
docker-compose up -d --build
```

The frontend container's entrypoint will create `assets/env.js` with
`window.__env.API_BASE_URL` set to the value from `.env`. `index.html` includes
`assets/env.js` so the app will read the runtime value.

Verification steps

- Development (local):

  - Run the dev server and verify it uses the development environment:
    ```powershell
    cd frontend
    npm install
    npm start
    # open http://localhost:4200 and inspect behavior or values that differ in dev
    ```

- Production build (local):

  - Build production bundle and inspect output:
    ```powershell
    cd frontend
    npm run build -- --configuration=production
    # the bundle will include values from src/environments/environment.ts
    ```

- Runtime override (docker-compose):
  - Set `API_BASE_URL` in `.env` and start the stack:
    ```powershell
    docker-compose up -d --build
    ```
  - To test a different API URL without rebuilding, change `.env` and restart the frontend container:
    ```powershell
    docker-compose up -d --force-recreate --no-deps --build frontend
    ```

Notes & tips

- Do not store secrets in frontend env vars. Only public config like API endpoints should be put in `assets/env.js`.
- If you want to force normalization (e.g. ensure trailing slash) I can add a small normalization step to `docker-entrypoint.sh`.
- If you deploy static files (S3, CloudFront, etc.) instead of containers, upload a customized `assets/env.js` during deployment with the runtime API URL.

If you'd like I can also add a tiny CI/CD snippet (GitHub Actions, Azure Pipeline) to publish the built files and write `assets/env.js` during deploy.

# Frontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 19.2.0.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
