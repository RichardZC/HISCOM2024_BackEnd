dotnet ef dbcontext scaffold name=connectionDB  Microsoft.EntityFrameworkCore.SqlServer -o ../Domain/Models -c HISCOMContext -n Domain.Models -f --no-pluralize
