Staff Management App

## Технологический стек
- Blazor Web App (.NET 10) — Интерактивный сервер-режим (SignalR)
- PostgreSQL 16 — основная БД
- EF Core 10 + Npgsql — ORM (через `IDbContextFactory<>` для Blazor Server)
- ClosedXML — экспорт отчётов в Excel
- Bootstrap 5 + Bootstrap Icons — UI

## Структура проекта

StaffManagementApp/
├── Data/
│   ├── AppDbContext.cs            ← EF Core контекст
│   ├── DbInitializer.cs           ← Применение миграций
│   ├── Entities/
│   │   ├── Department.cs          ← Иерархия, soft-delete
│   │   ├── Employee.cs            ← Gender enum, FullName computed
│   │   └── CareerRecord.cs        ← EventType: Hire/Transfer/Dismissal
│   └── Migrations/
│       └── 20260918185522_InitialCreate.cs
├── Services/
│   ├── DepartmentService.cs       ← GetTree(), LiquidateAsync() с валидацией
│   ├── EmployeeService.cs         ← Search, GetAllWithStatus
│   ├── CareerService.cs           ← HireAsync, TransferAsync, DismissAsync (транзакции)
│   ├── ReportService.cs           ← GetReportAsync, ExportToExcel
│   ├── LocalFileStorageService.cs ← Загрузка фото, gender-fallback
│   └── DateHelper.cs              ← CalculateFullYears, CalculateAge, CalculateTenure
├── Components/
│   ├── Pages/
│   │   ├── Departments/
│   │   │   ├── Index.razor        ← Дерево подразделений
│   │   │   └── DepartmentFormModal.razor
│   │   ├── Employees/
│   │   │   ├── Index.razor        ← Карточки сотрудников + поиск
│   │   │   ├── Details.razor      ← Детали: фото, возраст, стаж, история
│   │   │   ├── HireModal.razor    ← Приём + создание сотрудника
│   │   │   ├── TransferModal.razor
│   │   │   ├── DismissModal.razor
│   │   │   └── EmployeeEditModal.razor
│   │   └── Reports/
│   │       └── Index.razor        ← Фильтры, таблица, Excel-экспорт
│   └── Layout/
│       ├── MainLayout.razor
│       └── NavMenu.razor
├── wwwroot/images/
│   ├── male-avatar.svg
│   └── female-avatar.svg
├── Dockerfile                     ← Multi-stage .NET 10
└── .dockerignore
docker-compose.yml                 ← app + db (PostgreSQL 16)


## Запуск через Docker Compose:
docker-compose up --build

После запуска: **http://localhost:8080**



## Ключевые бизнес-правила

| Правило                  | Реализация                                                      |
|--------------------------|-----------------------------------------------------------------|
| Ликвидация подразделения | Запрет при активных сотрудниках или дочерних подразделениях     |
| Закрытые подразделения   | Не появляются в выпадающих списках при найме/переводе           |
| Возраст и стаж           | Вычисляются динамически формулой `CalculateFullYears`           |
| История карьеры          | Отдельная таблица `CareerRecords` — никогда не перезаписывается |
| Фото                     | Заглушки: `male-avatar.svg` / `female-avatar.svg` по полу       |
| EF Core в Blazor         | `IDbContextFactory<>` — потокобезопасно                         |

## Отчёты

Страница `/reports`:
- Фильтр по периоду (дата начала — дата конца)
- Фильтр по типу события (Приём / Перевод / Увольнение / Все)
- Цветовая кодировка строк (зелёный — приём, жёлтый — перевод, красный — увольнение)
- Кнопка **«Экспорт в Excel»** (ClosedXML, формат `.xlsx`)
