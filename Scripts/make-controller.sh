dotnet-aspnet-codegenerator \
	-p ../Administrador/Administrador.csproj \
	controller -name PublicController \
	-api -dc HISCOMContext -outDir Controllers \
	-namespace Administrador.Controllers -f
	
	


dotnet-aspnet-codegenerator controller -name LevelController -api -m Domain.Models.Nivel -dc HISCOMContext -outDir Controllers -namespace Admin.Controllers -b ./Templates -f

