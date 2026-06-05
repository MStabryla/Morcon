# ASP.NET Core + Vue.js Template

## Overview

This template combine ASP.NET Core WebApi application with Vue.js Single Page Application. In Development mode both applications are running separately (Vue.js app is hosted by Vite). WebApp is redirecting all default requests to Vue.js dev server and handle all other API calls on /api/* path to Controllers. In Production mode ASP.NET app and Vue.js are compiled to ./wwwroot/ folder and ASP.NET app is available to execute.

## How to Start

In Development Mode 
- Use `dotnet run` to start both ASP.NET app and Vue.js SPA by Vite. This script also starts `npm run dev` process in *Client* folder. 

In Production Mode 
- Use `./publish.ps1` to build and start the application.
- In order to only compiling app, use `build.ps1`.
- In order to only start a compiled app, use `start.ps1`.