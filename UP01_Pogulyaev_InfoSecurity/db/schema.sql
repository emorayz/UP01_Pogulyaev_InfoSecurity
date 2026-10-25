IF DB_ID(N'DB_Pogulyaev_InfoSecurity') IS NULL
BEGIN
    CREATE DATABASE DB_Pogulyaev_InfoSecurity;
END;
GO
USE DB_Pogulyaev_InfoSecurity;
GO

IF OBJECT_ID(N'dbo.AuditEvents', N'U') IS NOT NULL DROP TABLE dbo.AuditEvents;
IF OBJECT_ID(N'dbo.RiskAssessments', N'U') IS NOT NULL DROP TABLE dbo.RiskAssessments;
IF OBJECT_ID(N'dbo.SecurityIncidents', N'U') IS NOT NULL DROP TABLE dbo.SecurityIncidents;
IF OBJECT_ID(N'dbo.SecurityPolicies', N'U') IS NOT NULL DROP TABLE dbo.SecurityPolicies;
IF OBJECT_ID(N'dbo.SecurityAssets', N'U') IS NOT NULL DROP TABLE dbo.SecurityAssets;
IF OBJECT_ID(N'dbo.Employees', N'U') IS NOT NULL DROP TABLE dbo.Employees;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Departments', N'U') IS NOT NULL DROP TABLE dbo.Departments;
GO

CREATE TABLE dbo.Users (
    ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Login NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Login UNIQUE,
    PasswordHash NVARCHAR(64) NOT NULL,
    Role NVARCHAR(20) NOT NULL CONSTRAINT CK_Users_Role CHECK (Role IN (N'User', N'Admin')),
    FIO NVARCHAR(150) NOT NULL,
    Photo NVARCHAR(260) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1)
);

CREATE TABLE dbo.Departments (
    ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departments PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL CONSTRAINT UQ_Departments_Name UNIQUE,
    Description NVARCHAR(500) NULL
);

CREATE TABLE dbo.Employees (
    ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
    FIO NVARCHAR(150) NOT NULL,
    Position NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(25) NULL,
    DepartmentID INT NOT NULL,
    UserID INT NULL,
    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentID) REFERENCES dbo.Departments(ID) ON DELETE CASCADE,
    CONSTRAINT FK_Employees_Users FOREIGN KEY (UserID) REFERENCES dbo.Users(ID) ON DELETE NO ACTION
);

CREATE TABLE dbo.SecurityAssets (
    ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SecurityAssets PRIMARY KEY,
    Name NVARCHAR(120) NOT NULL,
    AssetType NVARCHAR(60) NOT NULL,
    InventoryNumber NVARCHAR(50) NOT NULL CONSTRAINT UQ_SecurityAssets_Inventory UNIQUE,
    Criticality INT NOT NULL CONSTRAINT CK_SecurityAssets_Criticality CHECK (Criticality BETWEEN 1 AND 5),
    DepartmentID INT NOT NULL,
    CONSTRAINT FK_SecurityAssets_Departments FOREIGN KEY (DepartmentID) REFERENCES dbo.Departments(ID) ON DELETE CASCADE
);

CREATE TABLE dbo.SecurityIncidents (
    ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SecurityIncidents PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    IncidentDate DATETIME2(0) NOT NULL CONSTRAINT DF_SecurityIncidents_Date DEFAULT (SYSDATETIME()),
    Severity INT NOT NULL CONSTRAINT CK_SecurityIncidents_Severity CHECK (Severity BETWEEN 1 AND 5),
    Status NVARCHAR(40) NOT NULL CONSTRAINT CK_SecurityIncidents_Status CHECK (Status IN (N'Новый', N'В работе', N'Решён', N'Закрыт')),
    AssetID INT NOT NULL,
    AssignedUserID INT NULL,
    CONSTRAINT FK_SecurityIncidents_Assets FOREIGN KEY (AssetID) REFERENCES dbo.SecurityAssets(ID) ON DELETE CASCADE,
    CONSTRAINT FK_SecurityIncidents_Users FOREIGN KEY (AssignedUserID) REFERENCES dbo.Users(ID) ON DELETE NO ACTION
);

CREATE TABLE dbo.RiskAssessments (
    ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RiskAssessments PRIMARY KEY,
    AssetID INT NOT NULL,
    Threat NVARCHAR(200) NOT NULL,
    Vulnerability NVARCHAR(300) NOT NULL,
    Probability INT NOT NULL CONSTRAINT CK_RiskAssessments_Probability CHECK (Probability BETWEEN 1 AND 5),
    Impact INT NOT NULL CONSTRAINT CK_RiskAssessments_Impact CHECK (Impact BETWEEN 1 AND 5),
    AssessmentDate DATETIME2(0) NOT NULL,
    Mitigation NVARCHAR(1000) NOT NULL,
    CONSTRAINT FK_RiskAssessments_Assets FOREIGN KEY (AssetID) REFERENCES dbo.SecurityAssets(ID) ON DELETE CASCADE
);

CREATE TABLE dbo.SecurityPolicies (
    ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SecurityPolicies PRIMARY KEY,
    Title NVARCHAR(180) NOT NULL,
    Version NVARCHAR(20) NOT NULL,
    ApprovalDate DATETIME2(0) NOT NULL,
    DepartmentID INT NOT NULL,
    Content NVARCHAR(2000) NOT NULL,
    CONSTRAINT FK_SecurityPolicies_Departments FOREIGN KEY (DepartmentID) REFERENCES dbo.Departments(ID) ON DELETE CASCADE
);

CREATE TABLE dbo.AuditEvents (
    ID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditEvents PRIMARY KEY,
    EventDate DATETIME2(0) NOT NULL CONSTRAINT DF_AuditEvents_Date DEFAULT (SYSDATETIME()),
    UserID INT NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    Details NVARCHAR(1000) NOT NULL,
    IpAddress NVARCHAR(45) NULL,
    CONSTRAINT FK_AuditEvents_Users FOREIGN KEY (UserID) REFERENCES dbo.Users(ID) ON DELETE CASCADE
);
GO

CREATE INDEX IX_Employees_DepartmentID ON dbo.Employees(DepartmentID);
CREATE INDEX IX_Employees_UserID ON dbo.Employees(UserID);
CREATE INDEX IX_SecurityAssets_DepartmentID ON dbo.SecurityAssets(DepartmentID);
CREATE INDEX IX_SecurityIncidents_AssetID ON dbo.SecurityIncidents(AssetID);
CREATE INDEX IX_SecurityIncidents_AssignedUserID ON dbo.SecurityIncidents(AssignedUserID);
CREATE INDEX IX_SecurityIncidents_Status ON dbo.SecurityIncidents(Status);
CREATE INDEX IX_RiskAssessments_AssetID ON dbo.RiskAssessments(AssetID);
CREATE INDEX IX_SecurityPolicies_DepartmentID ON dbo.SecurityPolicies(DepartmentID);
CREATE INDEX IX_AuditEvents_UserID ON dbo.AuditEvents(UserID);
GO

SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (ID, Login, PasswordHash, Role, FIO, Photo, IsActive) VALUES
(1, N'admin', N'059A50CE956B7EC61527C7ECC0C55B5A009DC54AB4ACDDCE8852B46BAA2ABA30', N'Admin', N'Погуляев Андрей Денисович', NULL, 1),
(2, N'user', N'25D66617B6C65C21AA983DCC58CE8057EF56036EC8328E81379CDD6A127F8222', N'User', N'Смирнова Анна Сергеевна', NULL, 1),
(3, N'auditor', N'1FEAD8958B2CF5E328119F3956B41BEAE1E5B8C2E3AA50C13074E7C70FE97EFE', N'User', N'Волков Игорь Алексеевич', NULL, 1);
SET IDENTITY_INSERT dbo.Users OFF;

SET IDENTITY_INSERT dbo.Departments ON;
INSERT INTO dbo.Departments (ID, Name, Description) VALUES
(1, N'Отдел информационной безопасности', N'Управление защитой информации, мониторинг и реагирование на инциденты'),
(2, N'ИТ-департамент', N'Эксплуатация инфраструктуры, серверов, сетей и рабочих станций'),
(3, N'Финансовый департамент', N'Обработка финансовой и бухгалтерской информации'),
(4, N'Отдел кадров', N'Хранение и обработка персональных данных сотрудников');
SET IDENTITY_INSERT dbo.Departments OFF;

SET IDENTITY_INSERT dbo.Employees ON;
INSERT INTO dbo.Employees (ID, FIO, Position, Email, Phone, DepartmentID, UserID) VALUES
(1, N'Погуляев Андрей Денисович', N'Администратор ИБ', N'a.pogulyaev@example.ru', N'+7-900-100-10-01', 1, 1),
(2, N'Смирнова Анна Сергеевна', N'Специалист по мониторингу', N'a.smirnova@example.ru', N'+7-900-100-10-02', 1, 2),
(3, N'Волков Игорь Алексеевич', N'Аудитор', N'i.volkov@example.ru', N'+7-900-100-10-03', 1, 3),
(4, N'Орлова Мария Ивановна', N'Системный администратор', N'm.orlova@example.ru', N'+7-900-100-10-04', 2, NULL);
SET IDENTITY_INSERT dbo.Employees OFF;

SET IDENTITY_INSERT dbo.SecurityAssets ON;
INSERT INTO dbo.SecurityAssets (ID, Name, AssetType, InventoryNumber, Criticality, DepartmentID) VALUES
(1, N'Сервер управления доступом', N'Сервер', N'SRV-AD-001', 5, 2),
(2, N'База персональных данных', N'База данных', N'DB-PD-001', 5, 4),
(3, N'Межсетевой экран', N'Сетевое оборудование', N'FW-EDGE-001', 5, 2),
(4, N'Система резервного копирования', N'Программное обеспечение', N'SW-BKP-001', 4, 2),
(5, N'Портал документооборота', N'Веб-приложение', N'APP-DOC-001', 4, 1),
(6, N'Бухгалтерская система', N'Программное обеспечение', N'APP-FIN-001', 5, 3),
(7, N'Архив журналов аудита', N'Хранилище данных', N'ST-LOG-001', 4, 1),
(8, N'Рабочие станции сотрудников', N'Оборудование', N'WS-GROUP-001', 3, 2);
SET IDENTITY_INSERT dbo.SecurityAssets OFF;

SET IDENTITY_INSERT dbo.SecurityIncidents ON;
INSERT INTO dbo.SecurityIncidents (ID, Title, Description, IncidentDate, Severity, Status, AssetID, AssignedUserID) VALUES
(1, N'Множественные неуспешные входы', N'Зафиксировано 25 неуспешных попыток входа под учётной записью администратора.', '2026-09-01T09:20:00', 4, N'Решён', 1, 1),
(2, N'Обнаружение вредоносного ПО', N'Антивирус обнаружил подозрительный исполняемый файл на рабочей станции.', '2026-09-03T11:45:00', 5, N'Закрыт', 8, 2),
(3, N'Недоступность портала', N'Пользователи сообщили о недоступности портала документооборота.', '2026-09-05T14:10:00', 3, N'В работе', 5, 2),
(4, N'Подозрительный сетевой трафик', N'Система мониторинга выявила рост исходящего трафика к внешнему узлу.', '2026-09-08T16:30:00', 4, N'В работе', 3, 1),
(5, N'Ошибка резервного копирования', N'Ночное задание резервного копирования завершилось с ошибкой.', '2026-09-10T08:15:00', 3, N'Новый', 4, 2),
(6, N'Попытка доступа к финансовым данным', N'Учётная запись без профильных полномочий попыталась открыть закрытый раздел.', '2026-09-12T12:40:00', 4, N'Решён', 6, 1),
(7, N'Потеря журнала событий', N'В архиве отсутствует часть записей за сутки.', '2026-09-15T10:05:00', 3, N'Новый', 7, 3),
(8, N'Устаревшее антивирусное ПО', N'На части рабочих станций не обновились антивирусные базы.', '2026-09-18T09:55:00', 2, N'Закрыт', 8, 2),
(9, N'Несанкционированная смена прав', N'Аудит выявил изменение группы доступа без заявки.', '2026-09-20T15:35:00', 5, N'В работе', 1, 1),
(10, N'Утечка конфиденциального файла', N'Файл с ограниченным доступом был отправлен на внешнюю почту.', '2026-09-22T13:25:00', 5, N'Новый', 2, 1);
SET IDENTITY_INSERT dbo.SecurityIncidents OFF;

SET IDENTITY_INSERT dbo.RiskAssessments ON;
INSERT INTO dbo.RiskAssessments (ID, AssetID, Threat, Vulnerability, Probability, Impact, AssessmentDate, Mitigation) VALUES
(1, 1, N'Компрометация учётной записи', N'Отсутствие многофакторной аутентификации', 4, 5, '2026-08-20T10:00:00', N'Внедрить MFA и пересмотреть политику блокировки учётных записей.'),
(2, 2, N'Утечка персональных данных', N'Избыточные права доступа', 3, 5, '2026-08-21T10:00:00', N'Провести пересмотр прав, включить шифрование и контроль выгрузок.'),
(3, 3, N'Проникновение через периметр', N'Устаревшие правила фильтрации', 3, 5, '2026-08-22T10:00:00', N'Обновить правила и провести внешнее тестирование на проникновение.'),
(4, 4, N'Потеря данных', N'Отсутствие проверки восстановления', 3, 4, '2026-08-23T10:00:00', N'Выполнять ежемесячное тестовое восстановление резервных копий.'),
(5, 5, N'Внедрение вредоносного кода', N'Недостаточная проверка загружаемых файлов', 3, 4, '2026-08-24T10:00:00', N'Ограничить типы файлов и подключить антивирусную проверку.'),
(6, 6, N'Мошеннические операции', N'Отсутствие разделения полномочий', 2, 5, '2026-08-25T10:00:00', N'Настроить двойное подтверждение критических операций.'),
(7, 7, N'Сокрытие следов атаки', N'Неполная централизация журналов', 3, 4, '2026-08-26T10:00:00', N'Настроить непрерывную передачу журналов и контроль целостности.'),
(8, 8, N'Заражение рабочих станций', N'Несвоевременная установка обновлений', 4, 3, '2026-08-27T10:00:00', N'Автоматизировать установку обновлений и контролировать состояние ПО.');
SET IDENTITY_INSERT dbo.RiskAssessments OFF;

SET IDENTITY_INSERT dbo.SecurityPolicies ON;
INSERT INTO dbo.SecurityPolicies (ID, Title, Version, ApprovalDate, DepartmentID, Content) VALUES
(1, N'Политика управления доступом', N'2.1', '2026-01-15T00:00:00', 1, N'Доступ предоставляется по принципу минимальных привилегий на основании согласованной заявки.'),
(2, N'Политика резервного копирования', N'1.4', '2026-02-01T00:00:00', 2, N'Критические данные копируются ежедневно. Восстановление проверяется не реже одного раза в месяц.'),
(3, N'Правила реагирования на инциденты', N'3.0', '2026-03-10T00:00:00', 1, N'Каждый инцидент регистрируется, классифицируется, назначается ответственный и фиксируется результат обработки.'),
(4, N'Политика защиты персональных данных', N'2.0', '2026-04-05T00:00:00', 4, N'Доступ к персональным данным ограничивается профильными сотрудниками, все операции подлежат аудиту.'),
(5, N'Правила обработки финансовой информации', N'1.2', '2026-05-12T00:00:00', 3, N'Критические операции выполняются с разделением полномочий и последующей проверкой журналов.'),
(6, N'Политика журналирования', N'1.0', '2026-06-01T00:00:00', 1, N'Журналы безопасности хранятся централизованно и защищаются от несанкционированного изменения.');
SET IDENTITY_INSERT dbo.SecurityPolicies OFF;

SET IDENTITY_INSERT dbo.AuditEvents ON;
INSERT INTO dbo.AuditEvents (ID, EventDate, UserID, Action, Details, IpAddress) VALUES
(1, '2026-09-01T09:25:00', 1, N'Авторизация', N'Успешный вход администратора', N'10.10.1.15'),
(2, '2026-09-03T11:50:00', 2, N'Редактирование', N'Обновлён статус инцидента №2', N'10.10.1.22'),
(3, '2026-09-05T14:20:00', 2, N'Добавление', N'Зарегистрирован инцидент недоступности портала', N'10.10.1.22'),
(4, '2026-09-08T16:35:00', 1, N'Редактирование', N'Назначен ответственный по инциденту №4', N'10.10.1.15'),
(5, '2026-09-12T12:50:00', 1, N'Экспорт Word', N'Сформирован отчёт по инцидентам', N'10.10.1.15'),
(6, '2026-09-15T10:10:00', 3, N'Просмотр', N'Проверен архив журналов аудита', N'10.10.1.30'),
(7, '2026-09-20T15:40:00', 1, N'Редактирование', N'Изменены права доступа по заявке №184', N'10.10.1.15'),
(8, '2026-09-22T13:30:00', 1, N'Добавление', N'Зарегистрирован инцидент утечки файла', N'10.10.1.15');
SET IDENTITY_INSERT dbo.AuditEvents OFF;
GO

-- Запросы для проверки наполнения и получения снимков таблиц в SSMS.
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Departments;
SELECT * FROM dbo.Employees;
SELECT * FROM dbo.SecurityAssets;
SELECT * FROM dbo.SecurityIncidents;
SELECT * FROM dbo.RiskAssessments;
SELECT * FROM dbo.SecurityPolicies;
SELECT * FROM dbo.AuditEvents;
GO
