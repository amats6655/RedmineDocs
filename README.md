# RedmineDocs

`RedmineDocs` — это приложение для генерации документации из базы данных Redmine. Оно предоставляет возможность автоматического создания страниц в формате Markdown и HTML на основе данных о проектах, ролях, кнопках, трекерах и группах.

## Функциональность

- Генерация страниц для:
  - Проектов
  - Ролей
  - Трекеров
  - Кнопок
  - Групп
- Использование шаблонов Scriban для гибкой настройки структуры документации.
- Интеграция с базой данных MySQL.
- Поддержка Obsidian для организации документации.

## Структура проекта

- **Program.cs**: Точка входа приложения, отвечает за настройку и запуск процесса генерации документации.
- **Services**:
  - `ButtonService`: Обработка кнопок, их опций и действий.
  - `GroupService`: Работа с группами, их проектами и ролями.
  - `RoleService`: Обработка ролей, их прав и кнопок.
  - `TrackerService`: Управление трекерами, их ролями и кнопками.
  - `ProjectService`: Генерация страниц для проектов и их компонентов.
  - `PageGeneratorService`: Генерация страниц на основе шаблонов Scriban.
- **Models**:
  - Базовые сущности: `BaseModel`, `Group`, `Button`, `Role`, `Project`, `Tracker`, `Option`, `Action`.
  - Вспомогательный класс `FieldNames` для определения соответствий полей и таблиц.
- **Helpers**: Вспомогательные утилиты для обработки полей и их значений.

## Установка и запуск

1. Убедитесь, что у вас установлен .NET SDK версии 6.0 или выше.
2. Настройте файл `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "RedmineDatabase": "Your_MySQL_Connection_String"
     }
   }
   ```
3. Скомпилируйте проект:
   ```bash
   dotnet build
   ```
4. Запустите приложение:
   ```bash
   dotnet run
   ```

## Использование

Приложение автоматически подключается к базе данных MySQL, получает необходимые данные и создает файлы документации в папках `Output` и `Obsidian`.

### Генерируемые файлы
- Markdown (`.md`): Для каждого элемента (проекты, роли, группы и т.д.).
- HTML (по шаблону): Для более удобного просмотра документации в браузере.

## Конфигурация шаблонов

Шаблоны Scriban находятся в папке `Templates`.

- **Примеры шаблонов:**
  - `ProjectTemplate.sbn`
  - `RoleTemplate.sbn`
  - `GroupTemplate.sbn`
  - `TrackerTemplate.sbn`
  - `ButtonTemplate.sbn`

Вы можете изменить их структуру и стиль под свои нужды.

## Зависимости

- **Базы данных**:
  - MySQL
  - Dapper для выполнения запросов.
- **Логирование**:
  - Serilog
- **Парсинг данных**:
  - Markdig для работы с Markdown.
  - Scriban для шаблонов.
  - YamlDotNet для обработки YAML в настройках ролей.

## Расширение

Вы можете добавлять новые типы сущностей и шаблоны, расширяя существующие сервисы или создавая новые.
