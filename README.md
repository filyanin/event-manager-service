# event-manager-service
Web Api для сервиса управления мероприятиями

Для сборки решения:
1. Перейдите в корневую папку решения
2. Запустите dotnet build

Для Запуска тестов:
1. Из корневой папки перейдите в папку тестового проекта  cd .\EventService.Tests\
2. Выполните dotnet test

Для запуска проекта 
1. Из корневой папки решения перейдите в папку проекта  cd .\EventManagerService\
2. Используйте dotnet run -lp http (по умолчанию используется порт 5214) или dotnet run
3. После успешного запуска откройте http://localhost:port/swagger/index.html для доступа к swagger, вместо port подставьте порт из вывода dotnet run