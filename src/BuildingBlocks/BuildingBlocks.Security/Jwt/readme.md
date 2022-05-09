# Create SSL Certificate

[document](https://docs.microsoft.com/en-us/aspnet/core/security/docker-compose-https)

## Windows using Linux containers

Generate certificate and configure local machine:

```bash
# in the current folder
dotnet dev-certs https -ep aspnetapp.pfx -p test123
dotnet dev-certs https --trust
```

```powershell
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\aspnetapp.pfx -p test123
```

```cmd
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p test123
```