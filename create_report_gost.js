import fs from "node:fs";
import path from "node:path";
import {
  AlignmentType,
  Document,
  Footer,
  Header,
  HeadingLevel,
  ImportedXmlComponent,
  Packer,
  PageNumber,
  Paragraph,
  TextRun,
  convertInchesToTwip,
  Table,
  TableCell,
  TableRow,
  WidthType,
  ShadingType,
  BorderStyle,
  SectionType,
} from "docx";

const outputPath = process.argv[2];
if (!outputPath) throw new Error("Usage: node create_report.js /absolute/path/output.docx");

const T = String.raw;
const font = { name: "Times New Roman", eastAsia: "Times New Roman" };
const fontSans = { name: "Courier New", eastAsia: "Courier New" };

const run = (text, options = {}) => new TextRun({ text, font, size: 28, ...options });
const runS = (text, options = {}) => new TextRun({ text, font: fontSans, size: 24, ...options });

const para = (children, options = {}) => new Paragraph({
  spacing: { after: 0, line: 360 },
  ...options,
  children: Array.isArray(children) ? children : [children],
});

const bodyPara = (text, options = {}) =>
  para(run(text), {
    indent: { firstLine: convertInchesToTwip(0.492) }, // 1.25 см
    alignment: AlignmentType.JUSTIFIED,
    ...options,
  });

const heading1 = (text) =>
  para(run(text, { bold: true, size: 28 }), {
    heading: HeadingLevel.HEADING_1,
    spacing: { before: 240, after: 120 },
    alignment: AlignmentType.CENTER,
  });

const heading2 = (text) =>
  para(run(text, { bold: true, size: 28 }), {
    heading: HeadingLevel.HEADING_2,
    spacing: { before: 200, after: 100 },
  });

const heading3 = (text) =>
  para(run(text, { bold: true, size: 28 }), {
    heading: HeadingLevel.HEADING_3,
    spacing: { before: 160, after: 80 },
  });

const cell = (text, options = {}) => new TableCell({
  children: [para(run(text, { size: 24 }), { spacing: { after: 60, line: 280 } })],
  margins: { top: 80, bottom: 80, left: 100, right: 100 },
  ...options,
});

const xmlEscape = (value) =>
  String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");

const toc = (entries) => {
  const cached = entries
    .map(({ title: entryTitle, level, page }) => {
      const indent = Math.max(0, level - 1) * 360;
      return `<w:p>
        <w:pPr>
          <w:pStyle w:val="TOC${level}"/>
          <w:tabs><w:tab w:val="right" w:leader="dot" w:pos="9000"/></w:tabs>
          <w:ind w:left="${indent}"/>
        </w:pPr>
        <w:r><w:t>${xmlEscape(entryTitle)}</w:t></w:r>
        <w:r><w:tab/></w:r>
        <w:r><w:t>${xmlEscape(page)}</w:t></w:r>
      </w:p>`;
    })
    .join("");

  return ImportedXmlComponent.fromXmlString(`<w:sdt xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
    <w:sdtPr><w:alias w:val="Оглавление"/></w:sdtPr>
    <w:sdtContent>
      <w:p><w:r><w:fldChar w:fldCharType="begin" w:dirty="true"/>
        <w:instrText xml:space="preserve"> TOC \\o &quot;1-3&quot; \\h \\z \\u </w:instrText>
        <w:fldChar w:fldCharType="separate"/></w:r></w:p>
      ${cached}
      <w:p><w:r><w:fldChar w:fldCharType="end"/></w:r></w:p>
    </w:sdtContent>
  </w:sdt>`).root[0];
};

const sections = [
  { title: T`Введение`, level: 1, page: 4 },
  { title: T`1. Анализ предметной области`, level: 1, page: 5 },
  { title: T`1.1. Описание сущностей`, level: 2, page: 5 },
  { title: T`1.2. Бизнес-правила`, level: 2, page: 6 },
  { title: T`2. Проектирование базы данных`, level: 1, page: 7 },
  { title: T`2.1. ER-диаграмма`, level: 2, page: 7 },
  { title: T`2.2. SQL-скрипты создания таблиц`, level: 2, page: 8 },
  { title: T`3. Реализация приложения`, level: 1, page: 9 },
  { title: T`3.1. Архитектура MVVM`, level: 2, page: 9 },
  { title: T`3.2. Импорт и экспорт данных`, level: 2, page: 10 },
  { title: T`3.3. Графики и визуализация`, level: 2, page: 11 },
  { title: T`3.4. Реализация отчётов`, level: 2, page: 12 },
  { title: T`4. Инструкция пользователя`, level: 1, page: 13 },
  { title: T`5. Заключение`, level: 1, page: 15 },
  { title: T`Список литературы`, level: 1, page: 16 },
  { title: T`Приложение А. Листинги кода`, level: 1, page: 17 },
];

const children = [];

// ===== ТИТУЛЬНЫЙ ЛИСТ (без нумерации) =====
const titlePageChildren = [
  para(run("МИНИСТЕРСТВО НАУКИ И ВЫСШЕГО ОБРАЗОВАНИЯ РОССИЙСКОЙ ФЕДЕРАЦИИ", { bold: true, size: 24 }), {
    alignment: AlignmentType.CENTER, spacing: { after: 120 },
  }),
  para(run("Федеральное государственное бюджетное образовательное учреждение", { size: 24 }), {
    alignment: AlignmentType.CENTER, spacing: { after: 60 },
  }),
  para(run("высшего образования", { size: 24 }), {
    alignment: AlignmentType.CENTER, spacing: { after: 60 },
  }),
  para(run("«НАЗВАНИЕ ВУЗА»", { bold: true, size: 24 }), {
    alignment: AlignmentType.CENTER, spacing: { after: 240 },
  }),
  para(run("Кафедра «Информационные системы и технологии»", { bold: true, size: 24 }), {
    alignment: AlignmentType.CENTER, spacing: { after: 360 },
  }),
  para(run("ПОЯСНИТЕЛЬНАЯ ЗАПИСКА", { bold: true, size: 32 }), {
    alignment: AlignmentType.CENTER, spacing: { before: 240, after: 120 },
  }),
  para(run("к курсовой работе", { size: 28 }), {
    alignment: AlignmentType.CENTER, spacing: { after: 60 },
  }),
  para(run("по дисциплине «Программирование»", { size: 28 }), {
    alignment: AlignmentType.CENTER, spacing: { after: 360 },
  }),
  para(run("Тема: «Система учёта отделов, товаров и продаж в магазине»", { bold: true, size: 28 }), {
    alignment: AlignmentType.CENTER, spacing: { after: 600 },
  }),
  para(run("Выполнил: студент группы ИС-xxx", { size: 24 }), {
    alignment: AlignmentType.RIGHT, spacing: { after: 60 },
  }),
  para(run("___________________", { size: 24 }), {
    alignment: AlignmentType.RIGHT, spacing: { after: 120 },
  }),
  para(run("Проверил: ___________________", { size: 24 }), {
    alignment: AlignmentType.RIGHT, spacing: { after: 60 },
  }),
  para(run("«____» _____________ 20____ г.", { size: 24 }), {
    alignment: AlignmentType.CENTER, spacing: { before: 360 },
  }),
];

// ===== ЗАДАНИЕ (курсовая работа) =====
const assignmentChildren = [
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("Задание на курсовую работу"),
  heading2("1. Цель работы"),
  bodyPara(T`Разработать приложение для учёта отделов, товаров и ежедневных продаж в магазине, обеспечивающее: импорт/экспорт данных в форматах Excel (XLSX), CSV, JSON, XML; построение интерактивных графиков и диаграмм для анализа продаж; формирование аналитических отчётов по эффективности отделов и товаров.`),
  heading2("2. Описание предметной области"),
  bodyPara(T`Система оперирует тремя сущностями: Отделы (код, наименование, заведующий, телефон, плановый объём реализации в день), Товары (артикул, наименование, единица измерения, розничная цена, код отдела) и Продажи (ID, артикул товара, дата продажи, количество, код отдела, сумма продажи).`),
  heading2("3. Бизнес-правила"),
  bodyPara(T`В таблице «Продажи» поля артикул_товара (FK на Товары) и код_отдела (FK на Отделы) должны быть NOT NULL. В таблице «Отделы» необходимо обеспечить наличие хотя бы одной продажи. Добавить ограничения: количество проданного товара > 0, розничная цена ≥ 0, объём реализации в день ≥ 0. Заполнить тестовыми данными (минимум 5 отделов, 10 товаров, 30 продаж).`),
  heading2("4. Обязательные требования к функционалу"),
  heading3("4.1. База данных (SQLite)"),
  bodyPara(T`Создать таблицы в соответствии с сущностями, задать первичные и внешние ключи. Учитывать бизнес-правила: NOT NULL для внешних ключей, контроль наличия продаж для отделов, ограничения на положительные значения.`),
  heading3("4.2. Импорт данных (минимум 3 формата)"),
  bodyPara(T`CSV — загрузка списка отделов и товаров. Excel (XLSX) — загрузка продаж с автоматическим определением отдела. JSON — загрузка полной структуры данных. Требования: проверка корректности данных, автоматический расчёт выручки, обработка ошибок с подробным отчётом.`),
  heading3("4.3. Экспорт данных (минимум 3 формата)"),
  bodyPara(T`Excel (XLSX) — экспорт любого отчёта с форматированием (заголовки, автоподбор ширины, денежный формат). CSV — экспорт для дальнейшей обработки. PDF — экспорт отчёта с таблицей и итоговыми суммами.`),
  heading3("4.4. Графики и визуализация"),
  bodyPara(T`Построить минимум 4 типа графиков: объём продаж по отделам (столбчатый), доля продаж по товарам (круговой), динамика продаж по дням (линейный), распределение товаров по ценовым категориям (гистограмма). Все графики интерактивные, с подписями осей, легендой, заголовком, возможностью сохранения в PNG.`),
  heading3("4.5. Расчётные функции"),
  bodyPara(T`Для каждой продажи: сумма = количество × розничная цена. Для каждого отдела за день: фактический объём = сумма продаж отдела за день, выполнение плана = факт / план × 100%.`),
  heading3("4.6. Отчёты (5 обязательных)"),
  bodyPara(T`Отчёт 1 — сводка по отделам (план, факт, % выполнения). Отчёт 2 — сводка по товарам (количество, выручка, среднее в день). Отчёт 3 — рейтинг товаров по продажам (топ-10, доля %). Отчёт 4 — дневная динамика продаж (дата, количество, выручка, средний чек, лидер дня). Отчёт 5 — товары-аутсайдеры (не продавались за период).`),
  heading3("4.7. Интерфейс пользователя"),
  bodyPara(T`Главное окно с вкладками: «Отделы», «Товары», «Продажи», «Отчёты», «Графики», «Импорт/Экспорт». Форма добавления/редактирования продажи с выпадающими списками отдела и товара, автоматическим расчётом суммы и проверкой данных.`),
];

// ===== ОГЛАВЛЕНИЕ =====
const tocChildren = [
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("Оглавление"),
  toc(sections.map(({ title: entryTitle, level, page }) => ({ title: entryTitle, level, page }))),
];

// ===== ОСНОВНАЯ ЧАСТЬ =====
const mainChildren = [];

// Введение
mainChildren.push(
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("Введение"),
  bodyPara(T`В современных условиях развития розничной торговли эффективность управления продажами напрямую влияет на конкурентоспособность магазина. Ручной учёт операций требует значительных временных затрат, сопряжён с риском ошибок и не позволяет оперативно получать аналитические данные для принятия управленческих решений.`),
  bodyPara(T`Целью данной курсовой работы является разработка десктоп-приложения для автоматизации учёта отделов, товаров и ежедневных продаж в магазине. Для достижения поставленной цели необходимо решить следующие задачи:`),
  para(run("• проанализировать предметную область и выделить ключевые сущности и бизнес-правила;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• спроектировать реляционную базу данных с учётом нормализации и связей;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• реализовать импорт и экспорт данных в форматах CSV, Excel, JSON, XML, PDF;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• разработать модуль визуализации с интерактивными графиками;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• создать систему аналитических отчётов с расчётными показателями;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• разработать удобный пользовательский интерфейс на базе паттерна MVVM."), { indent: { left: convertInchesToTwip(0.5) } }),
  bodyPara(T`В работе использованы технологии: язык программирования C# 12, платформа .NET 8, UI-фреймворк Avalonia UI, встроенная база данных SQLite, библиотеки LiveCharts + SkiaSharp для визуализации, EPPlus для работы с Excel.`),
);

// 1. Анализ предметной области
mainChildren.push(
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("1. Анализ предметной области"),
  heading2("1.1. Описание сущностей"),
  bodyPara(T`Система оперирует тремя основными сущностями, которые в совокупности описывают полный цикл торговой операции: от товарного запаса до факта продажи.`),
  bodyPara(T`Сущность «Отдел» описывает структурную единицу магазина. Атрибуты: код отдела (уникальный целочисленный идентификатор), наименование, ФИО заведующего, контактный телефон, плановый объём реализации в день (в рублях). Код отдела является первичным ключом.`),
  bodyPara(T`Сущность «Товар» описывает единицу товарного ассортимента. Атрибуты: артикул (уникальный целочисленный идентификатор), наименование, единица измерения, розничная цена (в рублях), код отдела (внешний ключ на Отдел). Артикул является первичным ключом.`),
  bodyPara(T`Сущность «Продажа» описывает факт совершения сделки. Атрибуты: ID продажи (автоинкремент), артикул товара (внешний ключ на Товар), дата продажи, количество проданных единиц, код отдела (внешний ключ на Отдел), сумма продажи (расчётное поле = количество × цена).`),
  heading2("1.2. Бизнес-правила"),
  bodyPara(T`При проектировании базы данных были учтены следующие бизнес-правила и ограничения предметной области:`),
  para(run("• поля артикул_товара и код_отдела в таблице Продажи обязательны для заполнения (NOT NULL);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• количество проданного товара должно быть строго положительным (> 0);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• розничная цена товара не может быть отрицательной (≥ 0);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• фактический объём реализации отдела за день не может быть отрицательным (≥ 0);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• каждый отдел должен иметь хотя бы одну запись о продаже (контроль на уровне приложения);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• товар не обязательно должен участвовать в продаже (допустимы товары без продаж)."), { indent: { left: convertInchesToTwip(0.5) } }),
);

// 2. Проектирование БД
mainChildren.push(
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("2. Проектирование базы данных"),
  heading2("2.1. ER-диаграмма"),
  bodyPara(T`База данных спроектирована в третьей нормальной форме (3NF). Связи между сущностями:`),
  para(run("• Отделы — Товары: один-ко-многим (1:N). Один отдел может содержать множество товаров, но каждый товар принадлежит ровно одному отделу."), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Отделы — Продажи: один-ко-многим (1:N). Один отдел совершает множество продаж."), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Товары — Продажи: один-ко-многим (1:N). Один товар может участвовать во множестве продаж."), { indent: { left: convertInchesToTwip(0.5) } }),
  bodyPara(T`Такая структура обеспечивает целостность данных через механизм внешних ключей и позволяет строить сложные аналитические запросы с JOIN-операциями.`),
  heading2("2.2. SQL-скрипты создания таблиц"),
  bodyPara(T`Для создания таблиц использованы следующие SQL-скрипты (SQLite):`),
  para(runS(T`CREATE TABLE Departments (`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 } }),
  para(runS(T`  DepartmentCode INTEGER PRIMARY KEY,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  Name TEXT NOT NULL,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  Manager TEXT,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  Phone TEXT,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  DailyPlan REAL DEFAULT 0`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`);`, { size: 22, font: fontSans }), { spacing: { after: 80, line: 280 } }),
  para(runS(T`CREATE TABLE Products (`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 } }),
  para(runS(T`  Article INTEGER PRIMARY KEY,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  Name TEXT NOT NULL,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  Unit TEXT,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  Price REAL NOT NULL CHECK (Price >= 0),`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  DepartmentCode INTEGER REFERENCES Departments(DepartmentCode)`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`);`, { size: 22, font: fontSans }), { spacing: { after: 80, line: 280 } }),
  para(runS(T`CREATE TABLE Sales (`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 } }),
  para(runS(T`  SaleId INTEGER PRIMARY KEY AUTOINCREMENT,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  Article INTEGER NOT NULL REFERENCES Products(Article),`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  SaleDate TEXT NOT NULL,`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  Quantity INTEGER NOT NULL CHECK (Quantity > 0),`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  DepartmentCode INTEGER NOT NULL REFERENCES Departments(DepartmentCode),`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`  TotalAmount REAL`, { size: 22, font: fontSans }), { spacing: { after: 40, line: 280 }, indent: { left: convertInchesToTwip(0.3) } }),
  para(runS(T`);`, { size: 22, font: fontSans }), { spacing: { after: 80, line: 280 } }),
  bodyPara(T`Ограничение CHECK (Price >= 0) гарантирует неотрицательность цены, CHECK (Quantity > 0) — положительность количества. Внешние ключи с NOT NULL обеспечивают ссылочную целостность.`),
);

// 3. Реализация приложения
mainChildren.push(
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("3. Реализация приложения"),
  heading2("3.1. Архитектура MVVM"),
  bodyPara(T`Приложение построено по паттерну MVVM (Model-View-ViewModel), который обеспечивает чистое разделение ответственности между слоями:`),
  para(run("• Model — классы сущностей (Department, Product, Sale) и сервис DatabaseService, отвечающий за доступ к SQLite;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• ViewModel — классы DepartmentsViewModel, ProductsViewModel, SalesViewModel и др., содержащие бизнес-логику, команды (ReactiveCommand) и привязку данных;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• View — XAML-разметка (Avalonia UI), описывающая визуальное представление каждого раздела."), { indent: { left: convertInchesToTwip(0.5) } }),
  bodyPara(T`Паттерн ViewLocator обеспечивает автоматическое сопоставление ViewModel и View по соглашению об именовании. ReactiveUI предоставляет механизм WhenAnyValue для реактивного обновления UI при изменении свойств.`),
  heading2("3.2. Импорт и экспорт данных"),
  bodyPara(T`Модуль импорта/экспорта реализован в классах ImportService и ExportService. Поддерживаемые форматы:`),
  para(run("• CSV — парсинг через разделитель запятых, загрузка отделов и товаров;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Excel (XLSX) — использование библиотеки EPPlus для чтения и записи с форматированием (заголовки, автоширина, денежный формат);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• JSON — сериализация/десериализация полной структуры данных через System.Text.Json;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• XML — ручной парсинг DOM для загрузки справочников;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• PDF — генерация отчётов через библиотеку PDF-генерации с таблицами и итогами."), { indent: { left: convertInchesToTwip(0.5) } }),
  bodyPara(T`Перед импортом выполняется валидация: проверка формата даты, положительность количества, существование артикула и кода отдела в справочниках. При ошибках выводится подробный отчёт с указанием строки и типа ошибки.`),
  heading2("3.3. Графики и визуализация"),
  bodyPara(T`Для построения графиков использована библиотека LiveChartsCore.SkiaSharpView.Avalonia. Реализованы четыре типа диаграмм:`),
  para(run("• Столбчатая диаграмма — объём продаж по отделам (CartesianChart + BarChartSeries);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Круговая диаграмма — доля продаж по товарам (PieChart);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Линейный график — динамика продаж по дням (CartesianChart + LineChartSeries);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Гистограмма — распределение товаров по ценовым категориям (CartesianChart + Histogram)."), { indent: { left: convertInchesToTwip(0.5) } }),
  bodyPara(T`Все графики интерактивны: при наведении отображается tooltip с точным значением. Реализована кнопка «Сохранить PNG», которая экспортирует текущий график в файл изображения через SkiaSharp.`),
  heading2("3.4. Реализация отчётов"),
  bodyPara(T`Отчёты формируются через SQL-запросы с агрегатными функциями (SUM, COUNT, AVG, MAX) и JOIN-соединениями. Примеры расчётных показателей:`),
  para(run("• Сумма продажи = Quantity × Products.Price (вычисляется при вставке или обновлении);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Фактический объём отдела = SUM(Sales.TotalAmount) WHERE DepartmentCode = ? AND SaleDate BETWEEN ? AND ?;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• % выполнения плана = (Факт / Departments.DailyPlan) × 100;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Средний чек = SUM(TotalAmount) / COUNT(DISTINCT SaleDate);"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• Товар-лидер дня = товар с MAX(TotalAmount) за конкретную дату."), { indent: { left: convertInchesToTwip(0.5) } }),
  bodyPara(T`Отчёты поддерживают фильтрацию по дате «с» и «по» через DatePicker. Результаты отображаются в DataGrid с сортировкой и форматированием числовых полей.`),
);

// 4. Инструкция пользователя
mainChildren.push(
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("4. Инструкция пользователя"),
  bodyPara(T`Главное окно приложения содержит меню навигации с шестью разделами: Отделы, Товары, Продажи, Отчёты, Графики, Импорт/Экспорт. Каждый раздел имеет единообразную структуру: таблица данных в верхней части экрана и панель редактирования в нижней.`),
  heading2("4.1. Раздел «Отделы»"),
  bodyPara(T`Отображает справочник отделов с колонками: код, наименование, заведующий, телефон, план (₽/день). Панель редактирования позволяет добавлять, обновлять и удалять записи. Кнопка «Новый» очищает форму для создания записи.`),
  heading2("4.2. Раздел «Товары»"),
  bodyPara(T`Отображает справочник товаров с колонками: артикул, наименование, ед. изм., цена, отдел. При выборе товара из таблицы его данные автоматически подставляются в форму. Для выбора отдела используется выпадающий список (ComboBox).`),
  heading2("4.3. Раздел «Продажи»"),
  bodyPara(T`Отображает журнал продаж. Форма содержит выпадающие списки для выбора товара и отдела, DatePicker для даты, NumericUpDown для количества. Сумма продажи рассчитывается автоматически. При добавлении продажи обновляется фактический объём отдела за день.`),
  heading2("4.4. Раздел «Отчёты»"),
  bodyPara(T`Содержит выпадающий список из 5 отчётов, фильтры по дате, кнопки экспорта в Excel, CSV, PDF. Отчёт отображается в DataGrid с сортировкой.`),
  heading2("4.5. Раздел «Графики»"),
  bodyPara(T`Отображает 4 диаграммы в режиме 2×2. Кнопка «Обновить» перестраивает графики по актуальным данным. Кнопка «Сохранить PNG» экспортирует выбранный график.`),
  heading2("4.6. Раздел «Импорт/Экспорт»"),
  bodyPara(T`Левая панель — импорт: выбор формата (CSV, Excel, JSON, XML), кнопка «Импортировать», поле результата. Правая панель — экспорт: выбор отчёта и формата (Excel, CSV, PDF), кнопка «Экспортировать». Дополнительно — кнопка «Очистить все данные» с подтверждением.`),
);

// 5. Заключение
mainChildren.push(
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("5. Заключение"),
  bodyPara(T`В ходе выполнения курсовой работы разработано десктоп-приложение «Система учёта продаж» на языке C# с использованием UI-фреймворка Avalonia и базы данных SQLite. Приложение обеспечивает полный цикл управления торговыми операциями: от ведения справочников отделов и товаров до анализа продаж и формирования отчётов.`),
  bodyPara(T`Достигнутые результаты:`),
  para(run("• реализованы CRUD-операции для всех справочников с валидацией данных;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• спроектирована нормализованная реляционная БД с внешними ключами и ограничениями;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• реализован импорт и экспорт в форматах CSV, Excel, JSON, XML, PDF;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• созданы 4 типа интерактивных графиков с возможностью сохранения в PNG;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• реализованы 5 аналитических отчётов с расчётными показателями;"), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("• применён паттерн MVVM, обеспечивающий чистоту кода и тестируемость."), { indent: { left: convertInchesToTwip(0.5) } }),
  bodyPara(T`Возможные направления дальнейшего развития: интеграция с онлайн-кассами для автоматического импорта продаж, реализация многопользовательского режима с разграничением прав доступа, разработка web-версии системы на базе ASP.NET Core, добавление push-уведомлений при невыполнении плана продаж.`),
);

// Список литературы
mainChildren.push(
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("Список литературы"),
  para(run("1. Рихтер, Д. CLR via C#. Программирование на платформе Microsoft .NET Framework 4.5 на языке C# / Д. Рихтер. — М. : Издательство «Русская Редакция», 2013. — 896 с."), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("2. Шилдт, Г. C# 4.0: полное руководство / Г. Шилдт. — М. : Издательский дом «Вильямс», 2011. — 1056 с."), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("3. Голубев, А.В. Технология ADO.NET / А.В. Голубев. — СПб. : БХВ-Петербург, 2015. — 432 с."), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("4. Официальная документация Avalonia UI [Электронный ресурс]. — Режим доступа: https://docs.avaloniaui.net/ (дата обращения: 15.06.2025)."), { indent: { left: convertInchesToTwip(0.5) } }),
  para(run("5. Официальная документация SQLite [Электронный ресурс]. — Режим доступа: https://www.sqlite.org/docs.html (дата обращения: 15.06.2025)."), { indent: { left: convertInchesToTwip(0.5) } }),
);

// Приложение А
mainChildren.push(
  new Paragraph({ children: [new TextRun({ text: "" })], pageBreakBefore: true }),
  heading1("Приложение А. Листинги кода"),
  bodyPara(T`Листинги ключевых модулей приложения приведены в электронном виде в составе исходного кода проекта. Основные классы: SupplyTrackingSystem.csproj (конфигурация), App.axaml (стили и тема), MainWindow.axaml (главное окно), DatabaseService.cs (доступ к БД), ImportService.cs / ExportService.cs (импорт/экспорт), соответствующие ViewModel (бизнес-логика).`),
);

const doc = new Document({
  features: { updateFields: true },
  sections: [
    // Титульный лист (без нумерации, без колонтитулов)
    {
      properties: {
        page: {
          margin: {
            top: 1134,    // 20 мм
            bottom: 1134, // 20 мм
            left: 1701,   // 30 мм
            right: 851,   // 15 мм
          },
        },
      },
      children: titlePageChildren,
    },
    // Задание (без нумерации)
    {
      properties: {
        type: SectionType.NEXT_PAGE,
        page: {
          margin: {
            top: 1134,
            bottom: 1134,
            left: 1701,
            right: 851,
          },
        },
      },
      children: assignmentChildren,
    },
    // Оглавление + основная часть (с нумерацией)
    {
      properties: {
        type: SectionType.NEXT_PAGE,
        page: {
          margin: {
            top: 1134,
            bottom: 1134,
            left: 1701,
            right: 851,
          },
        },
      },
      headers: {
        default: new Header({
          children: [
            para(run("Система учёта продаж | Пояснительная записка", { size: 20, color: "666666" }), {
              alignment: AlignmentType.RIGHT,
            }),
          ],
        }),
      },
      footers: {
        default: new Footer({
          children: [
            para(new TextRun({ children: [PageNumber.CURRENT], size: 20 }), {
              alignment: AlignmentType.CENTER,
            }),
          ],
        }),
      },
      children: [...tocChildren, ...mainChildren],
    },
  ],
});

const buffer = await Packer.toBuffer(doc);
fs.writeFileSync(outputPath, buffer);
