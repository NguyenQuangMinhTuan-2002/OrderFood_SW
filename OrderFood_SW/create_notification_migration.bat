@echo off
echo Creating migration for Notification table...

dotnet ef migrations add AddNotificationTable --project OrderFood_SW --startup-project OrderFood_SW

echo.
echo Applying migration to database...
dotnet ef database update --project OrderFood_SW --startup-project OrderFood_SW

echo.
echo Migration completed!
pause
