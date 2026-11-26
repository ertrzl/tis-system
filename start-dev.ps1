# Layihəni lokalda işə salır: API (5190) və React (5173) ayrı pəncərələrdə açılır, sonra brauzer açılır.
# İstifadə:  .\start-dev.ps1
# Dayandırmaq üçün açılan iki pəncərəni bağlayın (və ya içində Ctrl+C basın).
# Ön şərt: SQL Server işləyir və src\Tis.Api\appsettings.Development.json mövcuddur (README-yə bax).

$root = $PSScriptRoot

Start-Process powershell -ArgumentList '-NoExit', '-Command', "cd '$root'; dotnet run --project src/Tis.Api --urls http://localhost:5190"
Start-Process powershell -ArgumentList '-NoExit', '-Command', "cd '$root\frontend'; npm install; npm run dev"

Start-Sleep -Seconds 10
Start-Process 'http://localhost:5173'
