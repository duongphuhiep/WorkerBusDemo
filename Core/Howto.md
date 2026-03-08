# How to scaffold the database context and entities

```sh
dotnet tool install --global dotnet-ef

dotnet ef dbcontext scaffold 'Server=127.0.0.1,14333;User ID=sa;Password=Password123!;TrustServerCertificate=true;Initial Catalog=northwind' \
    Microsoft.EntityFrameworkCore.SqlServer \
    -o ./Db \
    -c NorthwindDbContext \
    -n Core.Db \
    --no-onconfiguring \
    --no-build
```