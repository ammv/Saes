-- ***********************************************************************
--    СКРИПТ ДЛЯ ПРОВЕРКИ АУДИТА И ОБЩЕЙ ФУНКЦИОНАЛЬНОСТИ БАЗЫ ДАННЫХ
-- 
-- Будет эмуляция последствий бизнес-процессов, т.е добавление данных,
-- удаление, обновление...
-- Следует выделять определённые блоки помеченные *** до *** со словом конец N
-- Все блоки можно выполнять сколько угодно раз благодаря откату транзакций
-- ***********************************************************************

-- ***********************************ВАЖНО*******************************
-- !!! ВЫДЕЛИ ОПЕРАЦИЮ НИЖЕ И ВЫПОЛНИ !!!
USE [SAES]

-- Sql Server не очень корректно себя ведёт после удаления базы данных
-- И может выдавать ошибки при выполнении большого количества операций
-- и иногда требуется выполнить одну простую, чтобы другие выполнились корректно
-- ***********************************************************************



-- ***********************************************************************
-- №1 Создание сессии пользователя и получения ключа сессии

-- Что должны получить: получение ключа сессии

-- Пример: 9437F0BB-982D-4AB6-8115-488D87F426A3
-- ***********************************************************************
USE [SAES]

BEGIN TRANSACTION

INSERT INTO [Audit].[LogAuthentication] ([EnteredLogin], [FirstFactorResult], [SecondFactorResult], [AuthServiceResponse]) VALUES
('admin', 1, 1, 'Successfully authenticated')

DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = 'admin')

DECLARE @SessionKey nvarchar(128)

DECLARE @ExpiredAt datetime = DATEADD(hour, 3, GETDATE())

EXEC [Authorization].[uspCreateSession] @UserID, @ExpiredAt, @SessionKey OUTPUT

PRINT @SessionKey

IF @@TRANCOUNT > 0
BEGIN
    ROLLBACK TRANSACTION
END

-- ******************************КОНЕЦ №1*********************************

GO

-- ***********************************************************************
-- №2 Создание сессии пользователя, получения ключа сессии и установка ID текущей сессии

-- Что должны получить: идентификатор текущей сессии

-- Пример: 1
-- ***********************************************************************
USE [SAES]

BEGIN TRANSACTION

INSERT INTO [Audit].[LogAuthentication] ([EnteredLogin], [FirstFactorResult], [SecondFactorResult], [AuthServiceResponse]) VALUES
('admin', 1, 1, 'Successfully authenticated')

DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = 'admin')

DECLARE @SessionKey nvarchar(128)

DECLARE @ExpiredAt datetime = DATEADD(hour, 3, GETDATE())

EXEC [Authorization].[uspCreateSession] @UserID, @ExpiredAt, @SessionKey OUTPUT

EXEC [Authorization].[uspSetCurrentUserSessionID] @SessionKey

DECLARE @CurrentUserSessionID INT

EXEC [Authorization].[uspGetCurrentUserSessionID] @CurrentUserSessionID OUTPUT

PRINT @CurrentUserSessionID

IF @@TRANCOUNT > 0
BEGIN
    ROLLBACK TRANSACTION
END

-- ******************************КОНЕЦ №2*********************************

GO

-- ***********************************************************************
-- №3 Создание пользователя под пользователем 'admin'

-- Что должны получить: результирующий набор представляющий добавленного пользователя с логином 'test'

-- Пример:

-- UserID      Login                            PasswordHash                                                       PasswordSalt                       UserRoleID  LastLoginDate           TwoFAEnabled SysIsDeleted SysModifiedDate
-- ----------- -------------------------------- ------------------------------------------------------------------ ---------------------------------- ----------- ----------------------- ------------ ------------ -----------------------
-- 5           test                             0x0E3BC3F4EF1B081E3AA0A510A5F1ACA22F947DBBA713C169A1220E38C7BBFFCF 0xEAC226918CF42258EF85BD353342D6DE 1           NULL                    1            0            2024-04-04 22:55:53.983

-- ***********************************************************************
USE [SAES]

BEGIN TRANSACTION

INSERT INTO [Audit].[LogAuthentication] ([EnteredLogin], [FirstFactorResult], [SecondFactorResult], [AuthServiceResponse]) VALUES
('admin', 1, 1, 'Successfully authenticated')
DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = 'admin')
DECLARE @SessionKey nvarchar(128)
DECLARE @ExpiredAt datetime = DATEADD(hour, 3, GETDATE())
EXEC [Authorization].[uspCreateSession] @UserID, @ExpiredAt, @SessionKey OUTPUT
EXEC [Authorization].[uspSetCurrentUserSessionID] @SessionKey
DECLARE @CurrentUserSessionID INT

EXEC [Authentication].[uspAddUser] 'test', 'password', 1

SELECT * FROM [Authentication].[User] WHERE [Login] = 'test'

IF @@TRANCOUNT > 0
BEGIN
    ROLLBACK TRANSACTION
END

-- ******************************КОНЕЦ №3*********************************

GO

-- ***********************************************************************
-- №4 Создание сессии пользователя с датой истекания меньше даты выдачи

-- Что должны получить: отловить ошибку

-- Пример: 6	2024-04-05 03:00:25.947	NULL	50000	18	2	Authorization.uspCreateSession	25	Нельзя создать сессию, чьё время истекания меньше текущего:[CreatedAt] апр  5 2024  3:00AM > [ExpiredAt] апр  5 2024 12:00AM
-- ***********************************************************************
USE [SAES]

BEGIN TRY
BEGIN TRANSACTION

INSERT INTO [Audit].[LogAuthentication] ([EnteredLogin], [FirstFactorResult], [SecondFactorResult], [AuthServiceResponse]) VALUES
('admin', 1, 1, 'Successfully authenticated')
DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = 'admin')
DECLARE @SessionKey nvarchar(128)
DECLARE @ExpiredAt datetime = DATEADD(hour, -3, GETDATE())

EXEC [Authorization].[uspCreateSession] @UserID, @ExpiredAt, @SessionKey OUTPUT

SELECT * FROM [Audit].[ErrorLog]

COMMIT TRANSACTION

print 'Ошибки не произошло'

END TRY
begin catch
	IF @@TRANCOUNT > 0
	BEGIN
		ROLLBACK TRANSACTION
	END
	select * from [Audit].[ErrorLog] WHERE [ErrorLogID] = @@IDENTITY
END CATCH

-- ******************************КОНЕЦ №4*********************************

GO

-- ***********************************************************************
-- №5 Добавление новой роли и проверка наличия лога добавления

-- Что должны получить: лог с добавлением

-- Пример: 1	2024-04-05 03:04:44.067	Создание	UserRole	2	admin	admin
-- ***********************************************************************
USE [SAES]

BEGIN TRANSACTION

INSERT INTO [Audit].[LogAuthentication] ([EnteredLogin], [FirstFactorResult], [SecondFactorResult], [AuthServiceResponse]) VALUES
('admin', 1, 1, 'Successfully authenticated')
DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = 'admin')
DECLARE @SessionKey nvarchar(128)
DECLARE @ExpiredAt datetime = DATEADD(hour, 3, GETDATE())
EXEC [Authorization].[uspCreateSession] @UserID, @ExpiredAt, @SessionKey OUTPUT
EXEC [Authorization].[uspSetCurrentUserSessionID] @SessionKey

INSERT INTO [Authorization].[UserRole]([Name]) VALUES
('Manager')

SELECT TOP 1 * FROM [Audit].[viewLog] ORDER BY [LogID] DESC

IF @@TRANCOUNT > 0
BEGIN
	ROLLBACK TRANSACTION
END

-- ******************************КОНЕЦ №5*********************************

GO

-- ***********************************************************************
-- №6 Добавление организации и изменение ее данных

-- Что должны получить: лог со внесенными изменения в запись

-- Пример: 
-- 5	Organization	DirectorFullName	Моматов Артем Дмитриевич	Иисус
-- 5	Organization	INN	123456789012	111777111000
-- 5	Organization	FullName	ООО Рога и копыта	ААА Господи и ангелы
-- ***********************************************************************
USE [SAES]

BEGIN TRANSACTION

INSERT INTO [Audit].[LogAuthentication] ([EnteredLogin], [FirstFactorResult], [SecondFactorResult], [AuthServiceResponse]) VALUES
('admin', 1, 1, 'Successfully authenticated')
DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = 'admin')
DECLARE @SessionKey nvarchar(128)
DECLARE @ExpiredAt datetime = DATEADD(hour, 3, GETDATE())
EXEC [Authorization].[uspCreateSession] @UserID, @ExpiredAt, @SessionKey OUTPUT
EXEC [Authorization].[uspSetCurrentUserSessionID] @SessionKey

INSERT INTO [HumanResources].[BusinessEntityType]([Name]) VALUES ('Organization')
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (1)

INSERT INTO [HumanResources].[Organization]([BusinessEntityID], [FullName], [INN], [KPP], [DirectorFullName]) VALUES
(1, 'ООО Рога и копыта', '123456789012', '123456789', 'Моматов Артем Дмитриевич')

UPDATE [HumanResources].[Organization]
SET [FullName] = 'ААА Господи и ангелы', [INN] = '111777111000', [DirectorFullName] = 'Иисус'
WHERE [INN] = '123456789012'

SELECT * FROM [Audit].[viewLogChange] ORDER BY [LogID] DESC

IF @@TRANCOUNT > 0
BEGIN
	ROLLBACK TRANSACTION
END

DBCC CHECKIDENT('[HumanResources].[BusinessEntityType]', RESEED, 0)
DBCC CHECKIDENT('[HumanResources].[BusinessEntity]', RESEED, 0)
DBCC CHECKIDENT('[HumanResources].[Organization]', RESEED, 0)


-- ******************************КОНЕЦ №6*********************************

GO

-- ***********************************************************************
-- №7 Проверка логирования всех действий, добавочно операция восстановления (SysIsDeleted >= 1)

-- Что должны получить: Четыре лога - создание, изменения, удаление, восстановление

-- Пример: 
-- 38	2024-04-05 06:54:45.623	Восстановление	Organization	1	admin	admin
-- 37	2024-04-05 06:54:45.600	Удаление	Organization	1	admin	admin
-- 36	2024-04-05 06:54:45.573	Обновление	Organization	1	admin	admin
-- 35	2024-04-05 06:54:45.537	Создание	Organization	1	admin	admin
-- ***********************************************************************
USE [SAES]

BEGIN TRANSACTION

INSERT INTO [Audit].[LogAuthentication] ([EnteredLogin], [FirstFactorResult], [SecondFactorResult], [AuthServiceResponse]) VALUES
('admin', 1, 1, 'Successfully authenticated')
DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = 'admin')
DECLARE @SessionKey nvarchar(128)
DECLARE @ExpiredAt datetime = DATEADD(hour, 3, GETDATE())
EXEC [Authorization].[uspCreateSession] @UserID, @ExpiredAt, @SessionKey OUTPUT
EXEC [Authorization].[uspSetCurrentUserSessionID] @SessionKey

INSERT INTO [HumanResources].[BusinessEntityType]([Name]) VALUES ('Organization')
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (1)

-- Лог создания
INSERT INTO [HumanResources].[Organization]([BusinessEntityID], [FullName], [INN], [KPP], [DirectorFullName]) VALUES
(1, 'ООО Рога и копыта', '123456789012', '123456789', 'Моматов Артем Дмитриевич')

-- Лог обновления
UPDATE [HumanResources].[Organization]
SET [FullName] = 'ААА Господи и ангелы', [INN] = '111777111000', [DirectorFullName] = 'Иисус'
WHERE [INN] = '123456789012'

-- Лог удалеия
DELETE [HumanResources].[Organization] Where [INN] = '111777111000'

-- Лог восстановления
UPDATE [HumanResources].[Organization] SET [SysIsDeleted] = 0 WHERE [INN] = '111777111000'

SELECT * FROM [Audit].[viewLog] WHERE [Table] = 'Organization' ORDER BY [LogID] DESC, [Date] DESC 

IF @@TRANCOUNT > 0
BEGIN
	ROLLBACK TRANSACTION
END

DBCC CHECKIDENT('[HumanResources].[BusinessEntityType]', RESEED, 0)
DBCC CHECKIDENT('[HumanResources].[BusinessEntity]', RESEED, 0)
DBCC CHECKIDENT('[HumanResources].[Organization]', RESEED, 0)


-- ******************************КОНЕЦ №7*********************************