### How to build application (local)
1. Make sure you have installed version of .Net SDK defined in `global.json`
2. Run `dotnet tool restore` to restore all necessary tools
3. Run `docker compose up -d db` to start a postgresql instance; or edit `envs/local.env` variables to use a existing postgresql instance
5. Run `dotnet fake build -t migrate` to execute all database migrations
6. Run `dotnet fake build -t run` to start application in watch mode (automatic recompilation and restart at file save)

### How to build application (docker-compose)
1. Make sure you have installed version of .Net SDK defined in `global.json`
2. Run `dotnet tool restore` to restore all necessary tools
3. Run `dotnet fake build -t build` 
4. Run `docker compose up`
