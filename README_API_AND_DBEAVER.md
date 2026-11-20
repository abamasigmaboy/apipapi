# Проект «ТОРГОВЛЯ» (Задание 7) — REST API, База данных PostgreSQL и Docker Compose

Данный проект разработан в точном соответствии с архитектурой демонстрационного примера `RestAPI-main` с вашего рабочего стола и реализует бизнес-логику курсовой работы **«ТОРГОВЛЯ» (Задание 7: «Выручка от продажи товаров за месяц года»)**.

---

## 📁 Структура проекта

Проект размещен в двух удобных местах:
1. В репозитории курсовой: `c:\Users\kiryu\Desktop\дз\KURSACH-master\`
   - `docker-compose.yml` — запуск всей инфраструктуры (БД + API)
   - `database/init.sql` — полный SQL-скрипт создания таблиц, связей, индексов, представлений и данных
   - `database/dbeaver_queries.sql` — готовые запросы для демонстрации в DBeaver
   - `TradeRestAPI/` — исходный код REST API (.NET 10 Web API)
2. На рабочем столе (рядом с примером `RestAPI-main`): `c:\Users\kiryu\Desktop\TradeRestAPI\`

---

## 🗄️ 1. База данных (PostgreSQL) и DBeaver

### Схема базы данных (`trade_db`):
- **`departments` (Отделы)**:
  - `department_code` (PK) — код отдела (1..5)
  - `name` — наименование отдела («Электроника», «Бытовая техника», «Компьютерная техника», «Аксессуары», «Аудио/Видео»)
  - `manager` — ФИО заведующего
  - `phone` — телефон
  - `daily_plan` — дневной план товарооборота (руб)
- **`products` (Товары)**:
  - `article` (PK) — артикул товара (1001..1012)
  - `name` — наименование («Смартфон Galaxy A», «Ноутбук Pro 15» и др.)
  - `unit` — единица измерения («шт»)
  - `price` — розничная цена (руб)
  - `department_code` (FK) — привязка к отделу
- **`sales` (Продажи)**:
  - `sale_id` (PK, Serial) — номер чека/продажи
  - `article` (FK) — артикул проданного товара
  - `sale_date` — дата продажи (май 2025 года)
  - `quantity` — количество
  - `department_code` (FK) — отдел продажи
- **`clients` (Пользователи / Авторизация)**:
  - `login` (PK), `password`, `firstname`, `lastname`, `middlename`, `email`, `phone`, `passport`, `type`
  - Предсозданные пользователи:
    - `admin` / `admin123` (Administrator)
    - `manager` / `manager123` (Manager)
    - `user` / `secret` (User)

### Представления (Views) для отчётов:
1. **`v_task7_monthly_revenue`** — прямое отображение таблицы Задания 7 (выручка и цены в тыс. руб. и руб.).
2. **`v_department_summary`** — выполнение плана по отделам (план vs факт).
3. **`v_product_ratings`** — рейтинг популярности и доходности товаров.
4. **`v_daily_sales`** — аналитика продаж по дням.

---

## 🚀 2. Запуск через Docker Compose

### Шаг 1. Запустите Docker Desktop
Убедитесь, что Docker Desktop запущен (ярлык на рабочем столе).

### Шаг 2. Запуск контейнеров
Откройте PowerShell в папке проекта и выполните:
```powershell
docker compose up -d
```
Или из папки `c:\Users\kiryu\Desktop\TradeRestAPI`:
```powershell
cd C:\Users\kiryu\Desktop\TradeRestAPI
docker compose up -d
```

Команда автоматически:
1. Поднимет контейнер PostgreSQL 16 (`trade_postgres`) на порту **5432**.
2. Инициализирует базу данных `trade_db` со всеми таблицами, связями и данными из `init.sql`.
3. Соберёт и запустит контейнер REST API (`trade_api`) на порту **5000**.

---

## 🦫 3. Подключение в DBeaver

1. Запустите **DBeaver** (установлен на вашем компьютере).
2. Нажмите **Новое соединение** (значок розетки с плюсом) -> выберите **PostgreSQL**.
3. Заполните параметры подключения:
   - **Хост (Host):** `localhost`
   - **Порт (Port):** `5432`
   - **База данных (Database):** `trade_db`
   - **Пользователь (Username):** `postgres`
   - **Пароль (Password):** `postgres`
4. Нажмите **Тест соединения (Test Connection)** -> **Готово (Finish)**.

### Что посмотреть в DBeaver:
- Разверните `trade_db` -> `Схемы` -> `public` -> **Таблицы**:
  - Откройте любую таблицу (`departments`, `products`, `sales`, `clients`) и перейдите на вкладку **«Данные»**.
  - Нажмите вкладку **«ER-диаграмма»** для наглядной визуализации связей один-ко-многим между отделами, товарами и продажами!
- В DBeaver нажмите `Файл` -> `Открыть файл...` и выберите:
  `c:\Users\kiryu\Desktop\дз\KURSACH-master\database\dbeaver_queries.sql`
  Выполните запросы (нажав Ctrl+Enter) для демонстрации Задания 7 и других отчётов!

---

## 🌐 4. Использование и проверка REST API (Swagger)

После запуска перейдите в браузере по адресу:
👉 **[http://localhost:5000](http://localhost:5000)** (или **[http://localhost:5000/swagger](http://localhost:5000/swagger)**)

В открывшемся интерактивном интерфейсе **Swagger UI** доступны все контроллеры:

### 1. `Report` (Отчёты и Задание 7)
- **`GET /api/Report/task7-monthly-revenue?year=2025&month=5`** — **Главное Задание 7**:
  Возвращает отчёт:
  - Наименование товара
  - Единица измерения
  - Цена розничная, тыс. руб.
  - Количество товара
  - Выручка, тыс. руб.
  - **Итого по отделу (тыс. руб. и руб.)**
  - **Итого по магазину (тыс. руб. и руб.)**
- **`GET /api/Report/department-summary`** — Сводка по отделам и процент выполнения плана.
- **`GET /api/Report/product-rating`** — Рейтинг товаров по суммарной выручке и доле в % продаж.
- **`GET /api/Report/daily-sales`** — Динамика продаж по дням.
- **`GET /api/Report/underperformers`** — Товары без продаж.

### 2. `Autorize` (Авторизация и JWT, как в примере)
- **`POST /api/Autorize/login?login=admin&password=admin123`** — возвращает JWT токен и данные пользователя.
- **`POST /api/Autorize/register`** — регистрация нового сотрудника/клиента.

### 3. `Department` (Отделы)
- `GET /api/Department?page=1` — постраничная выборка с MemoryCache.
- `GET /api/Department/all` — список всех отделов.
- `GET /api/Department/{id}` — получение отдела по коду.
- `POST /api/Department` — создание отдела.
- `PUT /api/Department/{id}` — обновление отдела.
- `DELETE /api/Department/{id}` — удаление отдела.

### 4. `Product` (Товары)
- `GET /api/Product?page=1` — товары с постраничной выборкой и кэшированием.
- `GET /api/Product/all` — все товары.
- `GET /api/Product/by-department/{departmentCode}` — фильтр товаров по отделу.
- `POST`, `PUT`, `DELETE` — управление товарами.

### 5. `Sale` (Продажи)
- `GET /api/Sale?page=1` — журнал продаж.
- `GET /api/Sale/filter?from=2025-05-20&to=2025-05-25&departmentCode=1` — фильтрация по периоду и отделу.
- `POST`, `PUT`, `DELETE` — регистрация и редактирование продаж.

---

## 💻 5. Локальный запуск без Docker (через Visual Studio / CLI)

Если вы хотите запустить только базу в Docker, а сам API отлаживать в Visual Studio или через терминал:
1. Запустите базу:
   ```powershell
   docker compose up -d db
   ```
2. Запустите API:
   ```powershell
   cd C:\Users\kiryu\Desktop\TradeRestAPI
   dotnet run
   ```
3. Откройте `TradeRestAPI.slnx` в Visual Studio или используйте файл `TradeRestAPI.http` для быстрых запросов.
