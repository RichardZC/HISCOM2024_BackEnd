FROM mcr.microsoft.com/dotnet/sdk:5.0
COPY . src/
WORKDIR /src
RUN dotnet restore
RUN dotnet clean
RUN dotnet build -c Release
RUN dotnet publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY --from=0 /src/Admin/bin/Release/net5.0/publish/ Hiscom/
RUN apt update
RUN apt install libgdiplus -y
WORKDIR /Hiscom
ENTRYPOINT ["dotnet", "Admin.dll"]