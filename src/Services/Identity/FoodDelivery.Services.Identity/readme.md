#### Migration Scripts

```bash
dotnet ef migrations add InitialIdentityServerMigration -o Shared/Data/Migrations/Identity -c IdentityContext
dotnet ef database update -c IdentityContext
```
