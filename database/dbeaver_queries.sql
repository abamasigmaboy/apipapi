-- =============================================================================
-- ПРИМЕРЫ ЗАПРОСОВ ДЛЯ ВЫПОЛНЕНИЯ В DBEAVER
-- База данных: trade_db (Проект ТОРГОВЛЯ)
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. ЗАДАНИЕ 7: ВЫРУЧКА ОТ ПРОДАЖИ ТОВАРОВ ЗА МЕСЯЦ ГОДА
-- -----------------------------------------------------------------------------
-- Точный формат таблицы из задания:
-- Наименование товара | Единица измерения | Цена розничная, тыс. руб. | Количество товара | Выручка, тыс. руб.

SELECT 
    d.name AS "Отдел",
    p.name AS "Наименование товара",
    p.unit AS "Единица измерения",
    ROUND(p.price / 1000.0, 3) AS "Цена розничная, тыс. руб.",
    SUM(s.quantity) AS "Количество товара",
    ROUND(SUM(s.quantity * p.price) / 1000.0, 3) AS "Выручка, тыс. руб."
FROM sales s
JOIN products p ON s.article = p.article
JOIN departments d ON s.department_code = d.department_code
WHERE EXTRACT(YEAR FROM s.sale_date) = 2025
  AND EXTRACT(MONTH FROM s.sale_date) = 5
GROUP BY d.department_code, d.name, p.article, p.name, p.unit, p.price
ORDER BY d.department_code, p.name;

-- Итоги по отделам за месяц (тыс. руб.):
SELECT 
    d.department_code AS "Код",
    d.name AS "Отдел",
    ROUND(SUM(s.quantity * p.price) / 1000.0, 3) AS "Итого по отделу, тыс. руб.",
    SUM(s.quantity * p.price) AS "Итого по отделу, руб."
FROM sales s
JOIN products p ON s.article = p.article
JOIN departments d ON s.department_code = d.department_code
WHERE EXTRACT(YEAR FROM s.sale_date) = 2025
  AND EXTRACT(MONTH FROM s.sale_date) = 5
GROUP BY d.department_code, d.name
ORDER BY d.department_code;

-- Итого по магазину за месяц:
SELECT 
    'ИТОГО ПО МАГАЗИНУ' AS "Показатель",
    ROUND(SUM(s.quantity * p.price) / 1000.0, 3) AS "Выручка, тыс. руб.",
    SUM(s.quantity * p.price) AS "Выручка, руб.",
    SUM(s.quantity) AS "Всего продано товаров, шт"
FROM sales s
JOIN products p ON s.article = p.article
WHERE EXTRACT(YEAR FROM s.sale_date) = 2025
  AND EXTRACT(MONTH FROM s.sale_date) = 5;

-- -----------------------------------------------------------------------------
-- 2. ПРОСМОТР ВСЕХ ТАБЛИЦ
-- -----------------------------------------------------------------------------
SELECT * FROM departments ORDER BY department_code;
SELECT * FROM products ORDER BY article;
SELECT * FROM sales ORDER BY sale_date DESC, sale_id DESC;
SELECT login, firstname, lastname, email, phone, type FROM clients;

-- -----------------------------------------------------------------------------
-- 3. ПРЕДСТАВЛЕНИЯ (VIEWS)
-- -----------------------------------------------------------------------------
-- Готовый отчёт из представления v_task7_monthly_revenue:
SELECT * FROM v_task7_monthly_revenue WHERE sale_year = 2025 AND sale_month = 5;

-- Сводка по отделам и выполнению плана:
SELECT * FROM v_department_summary;

-- Рейтинг самых прибыльных товаров:
SELECT * FROM v_product_ratings;

-- Продажи по дням:
SELECT * FROM v_daily_sales;

-- -----------------------------------------------------------------------------
-- 4. ТОВАРЫ, НЕ ИМЕЮЩИЕ ПРОДАЖ (ОТСТАЮЩИЕ)
-- -----------------------------------------------------------------------------
SELECT 
    p.article,
    p.name,
    p.unit,
    p.price,
    d.name AS department_name
FROM products p
JOIN departments d ON p.department_code = d.department_code
WHERE NOT EXISTS (
    SELECT 1 FROM sales s WHERE s.article = p.article
);
