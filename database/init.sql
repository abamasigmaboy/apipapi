-- =============================================================================
-- База данных: trade_db (Проект ТОРГОВЛЯ / Задание 7)
-- Скрипт инициализации таблиц, связей, индексов, представлений и начальных данных
-- =============================================================================

-- Создание таблиц
DROP VIEW IF EXISTS v_task7_monthly_revenue CASCADE;
DROP VIEW IF EXISTS v_department_summary CASCADE;
DROP VIEW IF EXISTS v_product_ratings CASCADE;
DROP VIEW IF EXISTS v_daily_sales CASCADE;
DROP TABLE IF EXISTS sales CASCADE;
DROP TABLE IF EXISTS products CASCADE;
DROP TABLE IF EXISTS departments CASCADE;
DROP TABLE IF EXISTS clients CASCADE;

-- 1. Таблица отделов (departments)
CREATE TABLE departments (
    department_code INT PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    manager VARCHAR(150) NOT NULL,
    phone VARCHAR(50) NOT NULL,
    daily_plan NUMERIC(15, 2) NOT NULL CHECK (daily_plan >= 0)
);

COMMENT ON TABLE departments IS 'Отделы магазина';
COMMENT ON COLUMN departments.department_code IS 'Код отдела (первичный ключ)';
COMMENT ON COLUMN departments.name IS 'Наименование отдела';
COMMENT ON COLUMN departments.manager IS 'ФИО заведующего';
COMMENT ON COLUMN departments.phone IS 'Контактный телефон';
COMMENT ON COLUMN departments.daily_plan IS 'Дневной план товарооборота (руб)';

-- 2. Таблица товаров (products)
CREATE TABLE products (
    article INT PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    unit VARCHAR(50) NOT NULL,
    price NUMERIC(15, 2) NOT NULL CHECK (price >= 0),
    department_code INT NOT NULL,
    CONSTRAINT fk_products_department FOREIGN KEY (department_code)
        REFERENCES departments (department_code) ON DELETE CASCADE
);

COMMENT ON TABLE products IS 'Справочник товаров';
COMMENT ON COLUMN products.article IS 'Артикул товара (первичный ключ)';
COMMENT ON COLUMN products.name IS 'Наименование товара';
COMMENT ON COLUMN products.unit IS 'Единица измерения (шт, кг, набор и т.д.)';
COMMENT ON COLUMN products.price IS 'Розничная цена товара (руб)';
COMMENT ON COLUMN products.department_code IS 'Код отдела, к которому привязан товар';

-- 3. Таблица продаж (sales)
CREATE TABLE sales (
    sale_id SERIAL PRIMARY KEY,
    article INT NOT NULL,
    sale_date DATE NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    department_code INT NOT NULL,
    CONSTRAINT fk_sales_product FOREIGN KEY (article)
        REFERENCES products (article) ON DELETE CASCADE,
    CONSTRAINT fk_sales_department FOREIGN KEY (department_code)
        REFERENCES departments (department_code) ON DELETE CASCADE
);

COMMENT ON TABLE sales IS 'Журнал продаж товаров';
COMMENT ON COLUMN sales.sale_id IS 'Идентификатор продажи (автоинкремент)';
COMMENT ON COLUMN sales.article IS 'Артикул проданного товара';
COMMENT ON COLUMN sales.sale_date IS 'Дата совершения продажи';
COMMENT ON COLUMN sales.quantity IS 'Количество проданных единиц';
COMMENT ON COLUMN sales.department_code IS 'Код отдела, где произошла продажа';

-- 4. Таблица клиентов / пользователей для авторизации (clients)
CREATE TABLE clients (
    login VARCHAR(100) PRIMARY KEY,
    password VARCHAR(255) NOT NULL,
    firstname VARCHAR(100) NOT NULL,
    lastname VARCHAR(100) NOT NULL,
    middlename VARCHAR(100),
    email VARCHAR(150),
    phone VARCHAR(50),
    passport VARCHAR(100),
    type VARCHAR(50) DEFAULT 'Manager'
);

COMMENT ON TABLE clients IS 'Пользователи системы (для REST API авторизации)';

-- Индексы для оптимизации запросов и отчётов
CREATE INDEX idx_sales_sale_date ON sales(sale_date);
CREATE INDEX idx_sales_department_code ON sales(department_code);
CREATE INDEX idx_sales_article ON sales(article);
CREATE INDEX idx_products_department_code ON products(department_code);

-- =============================================================================
-- ЗАПОЛНЕНИЕ НАЧАЛЬНЫМИ ДАННЫМИ (SEED DATA)
-- =============================================================================

-- 1. Отделы
INSERT INTO departments (department_code, name, manager, phone, daily_plan) VALUES
(1, 'Электроника', 'Иванов Иван Иванович', '8-495-123-45-67', 150000.00),
(2, 'Бытовая техника', 'Петрова Анна Сергеевна', '8-812-456-78-90', 200000.00),
(3, 'Компьютерная техника', 'Сидоров Виктор Петрович', '8-383-222-33-44', 180000.00),
(4, 'Аксессуары', 'Козлова Елена Николаевна', '8-343-555-66-77', 80000.00),
(5, 'Аудио/Видео', 'Михайлов Дмитрий Алексеевич', '8-843-111-22-33', 120000.00);

-- 2. Товары
INSERT INTO products (article, name, unit, price, department_code) VALUES
(1001, 'Смартфон Galaxy A', 'шт', 25000.00, 1),
(1002, 'Смартфон X100', 'шт', 32000.00, 1),
(1003, 'Ноутбук Pro 15', 'шт', 65000.00, 3),
(1004, 'Ноутбук Air 13', 'шт', 48000.00, 3),
(1005, 'Холодильник двухкамерный', 'шт', 45000.00, 2),
(1006, 'Микроволновая печь', 'шт', 8000.00, 2),
(1007, 'Мышь беспроводная', 'шт', 1500.00, 4),
(1008, 'Клавиатура механическая', 'шт', 3500.00, 4),
(1009, 'Телевизор 4K 55"', 'шт', 55000.00, 1),
(1010, 'Наушники Bluetooth', 'шт', 5000.00, 5),
(1011, 'Планшет 10"', 'шт', 18000.00, 3),
(1012, 'Колонка портативная', 'шт', 3000.00, 5);

-- 3. Продажи (май 2025 года)
INSERT INTO sales (article, sale_date, quantity, department_code) VALUES
(1001, '2025-05-20', 2, 1),
(1001, '2025-05-20', 1, 1),
(1003, '2025-05-20', 1, 3),
(1005, '2025-05-20', 1, 2),
(1006, '2025-05-20', 3, 2),
(1007, '2025-05-20', 5, 4),
(1007, '2025-05-20', 3, 4),
(1009, '2025-05-20', 1, 1),
(1010, '2025-05-20', 4, 5),
(1011, '2025-05-20', 2, 3),
(1002, '2025-05-21', 2, 1),
(1004, '2025-05-21', 1, 3),
(1005, '2025-05-21', 1, 2),
(1007, '2025-05-21', 6, 4),
(1008, '2025-05-21', 3, 4),
(1009, '2025-05-21', 1, 1),
(1012, '2025-05-21', 5, 5),
(1001, '2025-05-22', 3, 1),
(1002, '2025-05-22', 1, 1),
(1003, '2025-05-22', 2, 3),
(1005, '2025-05-22', 2, 2),
(1006, '2025-05-22', 2, 2),
(1007, '2025-05-22', 4, 4),
(1010, '2025-05-22', 3, 5),
(1011, '2025-05-22', 1, 3),
(1001, '2025-05-23', 2, 1),
(1004, '2025-05-23', 1, 3),
(1005, '2025-05-23', 1, 2),
(1007, '2025-05-23', 5, 4),
(1009, '2025-05-23', 2, 1),
(1012, '2025-05-23', 4, 5),
(1002, '2025-05-24', 2, 1),
(1003, '2025-05-24', 1, 3),
(1006, '2025-05-24', 3, 2),
(1008, '2025-05-24', 4, 4),
(1009, '2025-05-24', 1, 1),
(1010, '2025-05-24', 5, 5),
(1011, '2025-05-24', 2, 3),
(1001, '2025-05-25', 2, 1),
(1005, '2025-05-25', 2, 2),
(1007, '2025-05-25', 4, 4),
(1009, '2025-05-25', 1, 1),
(1012, '2025-05-25', 3, 5),
(1002, '2025-05-26', 1, 1),
(1004, '2025-05-26', 2, 3),
(1005, '2025-05-26', 1, 2),
(1007, '2025-05-26', 5, 4),
(1008, '2025-05-26', 3, 4),
(1009, '2025-05-26', 1, 1),
(1011, '2025-05-26', 1, 3),
(1001, '2025-05-27', 3, 1),
(1003, '2025-05-27', 1, 3),
(1006, '2025-05-27', 2, 2),
(1007, '2025-05-27', 6, 4),
(1010, '2025-05-27', 4, 5),
(1009, '2025-05-28', 2, 1),
(1005, '2025-05-28', 1, 2),
(1007, '2025-05-28', 5, 4),
(1012, '2025-05-28', 4, 5),
(1004, '2025-05-29', 2, 3),
(1007, '2025-05-29', 4, 4),
(1009, '2025-05-29', 1, 1),
(1010, '2025-05-29', 3, 5);

-- 4. Пользователи для авторизации
INSERT INTO clients (login, password, firstname, lastname, middlename, email, phone, passport, type) VALUES
('admin', 'admin123', 'Алексей', 'Смирнов', 'Сергеевич', 'admin@trade.local', '8-999-000-00-01', '4510 123456', 'Administrator'),
('manager', 'manager123', 'Ольга', 'Кузнецова', 'Ивановна', 'manager@trade.local', '8-999-000-00-02', '4511 654321', 'Manager'),
('user', 'secret', 'Иван', 'Иванов', 'Иванович', 'user@trade.local', '8-999-000-00-03', 'secret', 'User');

-- =============================================================================
-- ПРЕДСТАВЛЕНИЯ (VIEWS) ДЛЯ ОТЧЁТОВ И ПРОСМОТРА В DBEAVER
-- =============================================================================

-- Представление для Задания 7: Выручка от продажи товаров за месяц года
-- Отображает: Наименование товара, Единица измерения, Цена розничная тыс. руб.,
-- Количество товара, Выручка тыс. руб.
CREATE OR REPLACE VIEW v_task7_monthly_revenue AS
SELECT
    EXTRACT(YEAR FROM s.sale_date)::INT AS sale_year,
    EXTRACT(MONTH FROM s.sale_date)::INT AS sale_month,
    TO_CHAR(s.sale_date, 'TMMonth YYYY') AS period_name,
    d.department_code,
    d.name AS department_name,
    p.article,
    p.name AS product_name,
    p.unit,
    ROUND(p.price / 1000.0, 3) AS price_thousand_rub,
    p.price AS price_rub,
    SUM(s.quantity)::INT AS total_quantity,
    ROUND(SUM(s.quantity * p.price) / 1000.0, 3) AS revenue_thousand_rub,
    SUM(s.quantity * p.price) AS revenue_rub
FROM sales s
JOIN products p ON s.article = p.article
JOIN departments d ON s.department_code = d.department_code
GROUP BY
    EXTRACT(YEAR FROM s.sale_date),
    EXTRACT(MONTH FROM s.sale_date),
    TO_CHAR(s.sale_date, 'TMMonth YYYY'),
    d.department_code,
    d.name,
    p.article,
    p.name,
    p.unit,
    p.price
ORDER BY
    sale_year DESC,
    sale_month DESC,
    d.department_code,
    p.name;

COMMENT ON VIEW v_task7_monthly_revenue IS 'Детальный отчёт по Заданию 7 (в тысячах рублей)';

-- Сводное представление по отделам (выполнение плана)
CREATE OR REPLACE VIEW v_department_summary AS
SELECT
    d.department_code,
    d.name AS department_name,
    d.manager,
    d.phone,
    d.daily_plan,
    COALESCE(SUM(s.quantity * p.price), 0) AS actual_volume_rub,
    ROUND(COALESCE(SUM(s.quantity * p.price), 0) / 1000.0, 3) AS actual_volume_thousand_rub,
    CASE
        WHEN d.daily_plan > 0 THEN ROUND((COALESCE(SUM(s.quantity * p.price), 0) / (d.daily_plan * 10.0)) * 100, 2)
        ELSE 0
    END AS plan_fulfillment_percent
FROM departments d
LEFT JOIN sales s ON d.department_code = s.department_code
LEFT JOIN products p ON s.article = p.article
GROUP BY d.department_code, d.name, d.manager, d.phone, d.daily_plan
ORDER BY d.department_code;

-- Рейтинг товаров по выручке
CREATE OR REPLACE VIEW v_product_ratings AS
SELECT
    p.article,
    p.name AS product_name,
    p.unit,
    p.price AS price_rub,
    d.name AS department_name,
    COALESCE(SUM(s.quantity), 0)::INT AS total_quantity_sold,
    COALESCE(SUM(s.quantity * p.price), 0) AS total_revenue_rub,
    ROUND(COALESCE(SUM(s.quantity * p.price), 0) / 1000.0, 3) AS total_revenue_thousand_rub
FROM products p
JOIN departments d ON p.department_code = d.department_code
LEFT JOIN sales s ON p.article = s.article
GROUP BY p.article, p.name, p.unit, p.price, d.name
ORDER BY total_revenue_rub DESC;

-- Динамика продаж по дням
CREATE OR REPLACE VIEW v_daily_sales AS
SELECT
    s.sale_date,
    COUNT(s.sale_id)::INT AS sales_count,
    SUM(s.quantity)::INT AS total_quantity,
    SUM(s.quantity * p.price) AS total_revenue_rub,
    ROUND(SUM(s.quantity * p.price) / 1000.0, 3) AS total_revenue_thousand_rub
FROM sales s
JOIN products p ON s.article = p.article
GROUP BY s.sale_date
ORDER BY s.sale_date;
