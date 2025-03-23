# RedmineDocs

![GitHub repo size](https://img.shields.io/github/repo-size/amats6655/RedmineDocs) ![GitHub issues](https://img.shields.io/github/issues/amats6655/RedmineDocs)

`RedmineDocs` — это приложение для генерации документации из базы данных Redmine. Оно автоматически создает страницы в формате Markdown на основе данных о проектах, ролях, кнопках, трекерах и группах, с поддержкой многоязычности.

Данный проект работает только при условии использования представлений базы данных: [buttons_view](SQL_Views/buttons_view.sql) и [projects_view](SQL_Views/projects_view.sql)

## Функциональность

- Автоматическая генерация Markdown-документации для:
  - Проектов и их компонентов
  - Ролей и их прав доступа
  - Трекеров задач
  - Кнопок интерфейса и их действий
  - Групп пользователей
- Многоязычная поддержка через систему переводов
- Интеграция с базой данных MySQL
- Гибкая система логирования

## Структура проекта

- **Program.cs**: Точка входа приложения
  - Конфигурация сервисов и логирования
  - Управление жизненным циклом генерации документации
  - Обработка аргументов командной строки (например, `--debug-groups`)

- **Services/**
  - **Interfaces/**: Интерфейсы сервисов
    - `IDataService`: Базовый интерфейс для работы с данными
    - `IButtonService`: Работа с кнопками
    - `IGroupService`: Работа с группами
    - `IProjectService`: Работа с проектами
    - `IRoleService`: Работа с ролями
    - `ITrackerService`: Работа с трекерами
  - **Implementation/**: Реализации сервисов
  - `MarkdownGeneratorBase.cs`: Базовый класс для генерации Markdown-документов

- **Models/**
  - `Project.cs`: Модель проекта
  - `Role.cs`: Модель роли и прав доступа
  - `Button.cs`: Модель кнопки интерфейса
  - `ButtonAction.cs`: Действия кнопок
  - `ButtonOption.cs`: Опции кнопок
  - `ButtonValue.cs`: Значения кнопок
  - `Group.cs`: Модель группы пользователей
  - `Tracker.cs`: Модель трекера задач
  - `RoleSettings.cs`: Настройки ролей
  - `AssociationType.cs`: Типы связей между сущностями

- **Resources/**
  - `Translations.cs`: Система локализации
  - `README.md`: Документация по ресурсам

## Установка и запуск

1. Убедитесь, что у вас установлен .NET SDK версии 8.0 или выше.

2. Настройте файл `.env` с переменными окружения:
   ```env
   DB_HOST=your_host
   DB_PORT=your_port
   DB_NAME=your_database
   DB_USER=your_username
   DB_PASSWORD=your_password
   DB_CONNECTION_STRING=server=${DB_HOST};port=${DB_PORT};username=${DB_USERNAME};password=${DB_PASSWORD};database=${DB_DATABASE}
   ```

3. Настройте локализацию в файле `translations.json` при необходимости.

4. Скомпилируйте и запустите проект:
   ```bash
   dotnet build
   dotnet run
   ```

Дополнительные параметры запуска:
- `--debug-groups`: Режим отладки данных групп
- `--debug-translations`: Режим отладки переводов
- `--debug-role`: Режим отладки данных ролей

## Генерация документации

Приложение автоматически:
1. Подключается к базе данных MySQL
2. Загружает все необходимые данные
3. Генерирует Markdown-файлы для каждой сущности
4. Сохраняет результаты в директорию `docs`

Процесс логируется в:
- Консоль (уровень Information)
- Файлы логов (уровень Debug) с ротацией по дням и размеру

## Зависимости

- **Работа с данными**:
  - MySQL (MySql.Data 9.2.0)
  - MySqlConnector 2.4.0
  - Dapper 2.1.35

- **Конфигурация**:
  - Microsoft.Extensions.Configuration 9.0.3
  - Microsoft.Extensions.Configuration.Json 9.0.3
  - Microsoft.Extensions.DependencyInjection 9.0.3
  - DotEnv.Core 3.1.0

- **Логирование**:
  - Serilog 4.2.0
  - Serilog.Sinks.Console 6.0.0
  - Serilog.Sinks.File 6.0.0

- **Обработка данных**:
  - Markdig 0.38.0
  - Newtonsoft.Json 13.0.3
  - YamlDotNet 16.3.0

## Расширение функциональности

Проект построен на принципах SOLID с использованием dependency injection, что позволяет легко расширять функциональность:

1. Добавьте новую модель в директорию `Models`
2. Создайте интерфейс сервиса в `Services/Interfaces`
3. Реализуйте сервис в `Services/Implementation`
4. Зарегистрируйте сервис в `Program.cs`
5. Добавьте генерацию документации в `MarkdownGeneratorBase`
