-- ОТ АВТОРА --
-- Ссылка на шаблоны документов ФАПСИ №152: https://ca.kontur.ru/Files/userfiles/file/news/Журнал%20поэкземплярного%20учета%20СКЗИ%2C%20эксплуатационной%20и%20технической%20документации%20к%20ним%2C%20ключевых.rtf

-- ***********************************************************************
--							ПОМОЩЬ
-- Блоки комментариев которые являются важными имеются обёрнуты в звёзды (***)
-- как сам этот блок.
--
-- Блоки комментариев не являющиеся важными не обёрнуты в звёзды
-- ***********************************************************************



-- ***********************************************************************
--                    О БАЗЕ ДАННЫХ В СРАВНЕНИИ СО СТАРОЙ
-- 
-- В этот раз я решил сконцетрировать свое внимание не на количестве таблиц, а качестве самой базы данных,
-- удобства её обслуживания, доработки.
--
-- Что изменилось в сравнении с базы данных курсовой:
-- 1. <Везде есть комментарии и описание>:
--		+ теперь не запутаться, что к чему
-- 2. <Улучшил работу с ограничениями>:
--		+ Грамотные именование: теперь создаю ограничения на уровне таблицы с самостоятельно заданным именем, а не автоматическим (FK_User_UserRole_ABC234235)
--		+ Повышенная согласованность данных: добавил ограчения UNIQUE, CHECK, которые добавлял раньше меньше, а щас с умом
-- 3. <Внедрение схем>:
--		+ теперь использую схемы для компановки таблиц, процедур, функций и т.п.
-- 4. <Улучшение именование объектов SQL Server>:
--		+ теперь имена объектов (процедуры, таблицы, ...) все согласно общепринятым практикам и конвенцию по именованию
-- 5. <Безопасные и отзывчивые процедуры>:
--		+ теперь внутри процедур есть транзакции и try..catch и клиентское приложение узнает об ошибке внутри них
-- 6. <Аудит>:
--		+ Любые изменения в таблицах теперь отслеживаются, с указанием пользователя, который их сделал
-- 7. <Данные не пропадут>:
--		+ записи в основых таблицах теперь нельзя удалить, все имеют вместо этого столбец статус SysIsDeleted BIT
--
-- На этом все ключевые аспекты этой базы данных закончены
-- ***********************************************************************



-- ***********************************************************************
--							АУДИТ
--					КАК РАБОТАЕТ АУДИТ
-- Есть таблицы, изменения в которых надо отслеживать
-- В данном случае мы говорим о DML языке: UPDATE, INSERT, DELETE
-- Все эти операции можно отслеживать с помощью триггеров.
--
-- Для каждой отслеживаемой таблицы за исключением некоторых таблиц (принадлежащих
-- схемам Audit и dbo и таблицы Session) создаются триггеры на все операции
-- При изменении выполнении одной из операций DML, все триггеры сначала регистрируют изменения
-- с помощью процедур аудита, т.е. логгируют, а только потом выполняют саму операцию.
-- 
-- В логах аудита записывается следующая информация: Таблица, Столбец, Действие, Время, ID Сессии, Пользователь, ID Записи, Роль пользователя
-- Чтобы знать, какой пользователь внёс изменения, для этого используется специальная процедура, которая устанавливает ID текущей сессии
-- в контекст сеанса (словарь ключ-значние) sp_set_session_context key(sql_variant), value(sql_variant).

-- Для этой процедуры создана процедура-обёртка, чтобы упростить использование и обеспечить гарантирую качества данных
-- Для есть процедуры: 
--		1. Authorization.uspSetCurrentUserSessionID @SessionKey UNIQUEIDENTIFIER - устанавливает ID текущей сессии
--		2. Authorization.uspGetCurrentUserSessionID @UserSessionID INT OUTPUT - возвращает ID текущей сессии
-- 
-- Если текущий пользователь не был задан через эту процедуру, то поле UserID будет NULL, а процедура, которая его запрашивает
-- выдаст предупреждение. Так сделано потому, что во время редактирования данных через SSMS создается отдельное подключение, в рамках которого нельзя выполнять свой T-SQL код
-- т.е. вызвать процедуру, чтобы отразить автора внесенных изменений
--
-- Текущая реализация такого аудита накладывает определенные ограничения на проектирование базы данных
-- Например в каждой таблице обязано быть поле идентификатор, хотя в некоторых таблицах оно не нужно
-- Но без него триггеры для аудита не будут созданы
--
-- Резюмируем:
--	1. Аудит данных работает на триггерах
--	2. Триггеры вызывают процедуры аудита, а потом вносят изменения
--	3. Чтобы отразить автора изменений, вызываем процедуру: EXEC Authentication.uspSetCurrentUserSessionKey @SessionKey
-- ***********************************************************************



-- ***********************************************************************
-- Включает использование FILESTREAM
-- Позволяет хранить в столбце не сами двоичные данные (файлы, картинки), а лишь указатели на них, а сами двоичные данные
-- расположены на диске
-- Запросы выполняются быстрей, журнал SQL не засирается огромными значениями и все счастливы здоровы
-- ***********************************************************************
EXEC sp_configure filestream_access_level, 2  
RECONFIGURE

-- ***********************************************************************
-- Удаляет базу данных, если она существует в 100% случаях
-- Если она используется, просто переводит в однопользовательский режим и завершает все транзакции
-- И тогда в 100% случае получится удалить
-- ***********************************************************************
use master
IF EXISTS(select * from sys.databases where name='SAES')
begin
	print 'База данных будет удалена'
	alter database [SAES] set single_user with rollback immediate
	DROP DATABASE IF EXISTS [SAES];
end

CREATE DATABASE [SAES] COLLATE Cyrillic_General_CI_AS;
GO



-- ***********************************************************************
-- Создает каталог для файлов связанных с Filestream
-- ***********************************************************************
ALTER DATABASE [SAES] ADD FILEGROUP [SAESFileGroup] CONTAINS FILESTREAM
GO
ALTER DATABASE [SAES] ADD FILE (Name =N'C', FILENAME ='C:\SAESFileGroup') TO FILEGROUP [SAESFileGroup]
GO



USE [SAES];

-- ***********************************************************************
-- Откатит текущую транзакцию, если sql инструкция вызовет мягкую ошибку
-- без неё скрипт выполнялся бы вплоть до коммита транзакции
-- ***********************************************************************
SET XACT_ABORT ON



BEGIN TRANSACTION CreateTables

-- ***********************************************************************
-- Создание пары ассиметричных ключей для шифрования конфиденциальных данных
-- Ключ для шифрования закрытого ключа является результатом функции SHA512('SAES')
-- Ключ создается на уровне базы данных
-- ***********************************************************************
GO
IF NOT EXISTS (SELECT * 
                FROM sys.asymmetric_keys 
                WHERE name = 'Main_AsymmetricKeys_Saes')
BEGIN
    CREATE ASYMMETRIC KEY Main_AsymmetricKeys_Saes
    WITH ALGORITHM = RSA_2048
    ENCRYPTION BY PASSWORD = '7c204454035eeac7a16572af2da0a239e4f2e459eb4a7c5759603594b8f0dc377cca439cd3f79c60327c21fb8b960d17551d0901800234a41a346f04be9aacb7'
    ;
END
GO
-- Схема для таблиц аудита: логи, метаданные таблиц и их столбцов
CREATE SCHEMA [Audit];
GO
-- Схема для таблиц связанных с авторизацией:роли, их права
CREATE SCHEMA [Authorization];
GO
-- Схема для таблиц связанных с аутенфикацией: пользователи
CREATE SCHEMA [Authentication];
GO

-- Схема для таблиц с персоналом, сотрудниками
CREATE SCHEMA [HumanResources];
GO
-- Схема для таблиц с персональной информацией
CREATE SCHEMA [Person];
GO
-- Схема для таблиц связанных с учетом ЭП
CREATE SCHEMA [AccountingCPI];
GO
-- Схема для таблиц связанных с офисными делами
CREATE SCHEMA [Office];
GO

-- НЕЛЬЗЯ ИСПОЛЬЗОВАТЬ NEWID И CRYPT_GEN_RANDOM В ФУНКЦИЯХ
--CREATE FUNCTION [Authentication].[udfGenerateSecretString]
--(
--    @Length INT
--)
--RETURNS NVARCHAR(MAX)
--AS
--BEGIN
--    -- Функция для генерации секретных строк
--    DECLARE @SecretString NVARCHAR(MAX) = '';
--    DECLARE @IndexEnd INT = CEILING(@Length / 40.0); -- Изменено для совместимости с CRYPT_GEN_RANDOM
--    DECLARE @IndexStart INT = 0;

--    WHILE @IndexStart < @IndexEnd
--    BEGIN
--        -- Используем CRYPT_GEN_RANDOM вместо NEWID()
--        SET @SecretString = CONCAT(@SecretString,CONVERT(VARCHAR(40), CRYPT_GEN_RANDOM(32), 2)); 
--        SET @IndexStart = @IndexStart + 1;
--    END;

--    RETURN SUBSTRING(@SecretString, 1, @Length);
--END;

GO
-- Название: Файл
-- Описание: представляет любой файл, будь то картинка или PDF документ
CREATE TABLE [File]
(
	[FileID] INT IDENTITY,
	-- Специальное поле для FILESTREAM
	[GUID] [uniqueidentifier] ROWGUIDCOL NOT NULL CONSTRAINT DF_File_GUID DEFAULT NEWID(),
	-- Содержимое
	[Data] VARBINARY(MAX) FILESTREAM NULL,
	-- Название файла
	[Name] NVARCHAR(128),
	-- Тип контента в нем
	[ContentType] NVARCHAR(64),

	CONSTRAINT PK_File_FileID PRIMARY KEY([FileID]),
	CONSTRAINT UQ_File_GUID UNIQUE([GUID])
)

-- Название: Роль пользователя
CREATE TABLE [Authorization].[UserRole]
(
	[UserRoleID] INT IDENTITY,
	[Name] nvarchar(64) NOT NULL,
	
	CONSTRAINT PK_UserRole_UserRoleId PRIMARY KEY([UserRoleID]),
	CONSTRAINT UQ_UserRole_Name UNIQUE([Name])
)

-- Название: Группа прав
-- Описание: Используется для группировки прав для удобного управления и представления в интерфейсах редакторов прав
CREATE TABLE [Authorization].[RightGroup]
(
	[RightGroupID] INT IDENTITY,
	[Code] NVARCHAR(256),
	[Name] NVARCHAR(256),
	
	CONSTRAINT PK_RightGroup_RightGroupId PRIMARY KEY([RightGroupID]),
	CONSTRAINT UQ_RightGroup_Code UNIQUE([Code]),
)

-- Название: Право
-- Описание: например есть форма UserListView, право просмотра её называется UserListView_CanView и может иметь состояние 0 или 1
-- т.е. используется для учёта различных прав в системе: право на печать, просмотр, редактирование
CREATE TABLE [Authorization].[Right]
(
	[RightID] INT IDENTITY,
	[RightGroupID] INT,
	[Code] NVARCHAR(256),
	[Name] NVARCHAR(256),
	
	CONSTRAINT PK_Right_RightId PRIMARY KEY([RightID]),
	CONSTRAINT FK_Right_RightGroup_RightGroupId FOREIGN KEY([RightGroupID]) REFERENCES [Authorization].[RightGroup]([RightGroupID]),
	CONSTRAINT UQ_Right_GroupID_Code UNIQUE([RightGroupID], [Code])
)

-- Название: Права роли пользователя
-- Описание: Набор прав определенной роли пользователя
CREATE TABLE [Authorization].[UserRoleRight]
(
	[UserRoleRightID] INT IDENTITY,
	[UserRoleID] INT,
	[RightID] INT,
	
	CONSTRAINT PK_UserRoleRight_UserRoleRightId PRIMARY KEY([UserRoleRightID]),
	CONSTRAINT FK_UserRoleRight_UserRole_UserRoleID FOREIGN KEY([UserRoleID]) REFERENCES [Authorization].[UserRole]([UserRoleID]),
	CONSTRAINT FK_UserRoleRight_Right_RightID FOREIGN KEY([RightID]) REFERENCES [Authorization].[Right]([RightID]),
	CONSTRAINT UQ_UserRole_UserRoleID_RightID UNIQUE([UserRoleID], [RightID]) 
)

-- ***********************************************************************
-- Удалена из за ненадобности
-- В процессе разработки её надобность была переосмыслена
-- ***********************************************************************
---- Название: Логин
---- Описание: Предоставляет доступ пользователя к системе
--CREATE TABLE [Authorization].[Login]
--(
--	[LoginID] INT IDENTITY,
--	[Name] NVARCHAR(32) NOT NULL,
--	[Password] NVARCHAR(32) NOT NULL,

--	CONSTRAINT PK_Login_UserID PRIMARY KEY([LoginID]),
--	CONSTRAINT UQ_Login_Name UNIQUE([Name])
--)

-- Название: Пользователь
CREATE TABLE [Authentication].[User]
(
	[UserID] INT IDENTITY,
	-- Логин пользователя
	[Login] NVARCHAR(32) NOT NULL,
	-- Хэш пользователя
	[PasswordHash] VARBINARY(32) NOT NULL,
	-- Соль пароля
	[PasswordSalt] VARBINARY(16) NOT NULL,
	-- Роль пользователя
	[UserRoleID] INT,
	-- Последняя дата авторизации
	[LastLoginDate] DATETIME NULL,
	-- Зашифрованный секретный ключ для генерации TOTP пароля
	--[TotpSecretKey] VARBINARY(max) NOT NULL CONSTRAINT DF_User_TotpSecretKey
	--						DEFAULT ENCRYPTBYASYMKEY(ASYMKEY_ID('Main_AsymmetricKeys_Saes'),
	--												CRYPT_GEN_RANDOM(128)),
	--[TotpSecretKey] VARBINARY(max) NOT NULL CONSTRAINT DF_User_TotpSecretKey
	--						DEFAULT CRYPT_GEN_RANDOM(128),
	[TotpSecretKey] NVARCHAR(64),
	-- Включена ли двухфакторная аутенфикация
	[TwoFactorEnabled] BIT NOT NULL CONSTRAINT DF_User_TwoFactorEnabled DEFAULT 1

	CONSTRAINT PK_User_UserID PRIMARY KEY([UserID]),
	CONSTRAINT UQ_User_Login UNIQUE([Login]),
	CONSTRAINT FK_User_UserRole_UserRoleID FOREIGN KEY([UserRoleID]) REFERENCES [Authorization].[UserRole]([UserRoleID])
)

-- Создаем некластерный индекс по логину для быстрого поиска
-- Заметка: Некластерный индекс - справочник, в котором есть указатели на записи таблицы, которые отсортированы по значениям указанных столбцов
-- (Как телефонная книга)
CREATE NONCLUSTERED INDEX IX_User_Login
ON [Authentication].[User]([Login])

-- Удалена из за ненадобности
-- В процессе разработки базы данных было решено, что бог послал мне в голову плохую таблицу
-- Название: Тип результата аутенфикации
-- Описание: используется для аудита попыток аутенфикации: Неправильный пароль, неправильный логин и т.д.
--CREATE TABLE [Authentication].[ResultType]
--(
--	[ResultTypeID] INT IDENTITY,
--	-- Названия типа результата аутенфикации (Успешно, неудачно)
--	[Name] NVARCHAR(256) NOT NULL,

--	CONSTRAINT PK_ResultType_ResultTypeID PRIMARY KEY([ResultTypeID]),
--	CONSTRAINT UQ_ResultType_Name UNIQUE([Name])
--)

--CREATE NONCLUSTERED INDEX IX_ResultType_Name
--ON [Authentication].[ResultType]([Name])



-- ***********************************************************************
-- Отдельные таблицы для стран, регионов, городов не были созданы
-- по причине того, что нет требований к адресам
-- по ним не будет производиться поиск или фильтрация
-- ***********************************************************************

-- Название: Адрес
CREATE TABLE [Person].[Address]
(
	[AddressID] INT IDENTITY,
	-- Страна
	[Country] NVARCHAR(128),
	-- Регион
	[Region] NVARCHAR(128),
	-- Город
	[City] NVARCHAR(128),
	-- Район
	[District] NVARCHAR(128),
	-- Улица
	[Street] NVARCHAR(128),
	-- Номер здания
	[BuildingNumber] NVARCHAR(16),
	-- Корпус
	[Corpus] NVARCHAR(16),
	-- Этаж
	[Floor] INT,
	-- Квартира/Офис
	[Flat] NVARCHAR(16),
	-- Почтовый индекс
	[PostalIndex] NVARCHAR(10),

	CONSTRAINT PK_Address_AddressID PRIMARY KEY([AddressID])
)

-- Название: Тип бизнес-сущности
-- Описание: Представляет тип бизнес-сущности, например организация, сотрудник, контрагент
CREATE TABLE [HumanResources].[BusinessEntityType]
(
	[BusinessEntityTypeID] INT IDENTITY,
	[Name] NVARCHAR(128) NOT NULL,

	CONSTRAINT PK_BusinessEntityType_BusinessEntityTypeID PRIMARY KEY([BusinessEntityTypeID]),
	CONSTRAINT UQ_BusinessEntityType_Name UNIQUE ([Name])
)
-- Название: Бизнес-сущность
-- Описание: Абстракная сущность созданная для объединения связанных сущностей: организация, сотрудник и прочее
CREATE TABLE [HumanResources].[BusinessEntity]
(
	[BusinessEntityID] INT IDENTITY,
	-- Это организация
	-- Если значния поля 1 - значит текущая запись представляет организацию
	-- Если значение поля 0 - значит текущая запись представляет сотрудника
	[BusinessEntityTypeID] INT NOT NULL

	CONSTRAINT PK_BusinessEntity_BusinessEntityID PRIMARY KEY([BusinessEntityID]),
	CONSTRAINT FK_BusinessEntity_BusinessEntityType_BusinessEntityTypeID FOREIGN KEY([BusinessEntityTypeID]) REFERENCES [HumanResources].[BusinessEntityType]([BusinessEntityTypeID]),
)

-- Название: Организация
CREATE TABLE [HumanResources].[Organization]
(
	-- ЯВЛЯЕТСЯ ПЕРВИЧНЫМ И ВТОРИЧНЫМ КЛЮЧОМ
	[BusinessEntityID] INT NOT NULL,

	-- НЕ ЯВЛЯЕТСЯ ПЕРВИЧНЫМ КЛЮЧОМ
	[OrganizationID] INT IDENTITY NOT NULL,
	-- Полное наименование организации
	[FullName] NVARCHAR(256),
	-- Сокращенное наименование организации
	[ShortName] NVARCHAR(256),
	-- Юридический адрес/Адрес деятельности
	[BusinessAddressID] INT,
	-- ИНН
	[INN] NVARCHAR(12),
	-- КПП
	[KPP] NVARCHAR(9),
	-- ОКПО
	[OKPO] NVARCHAR(8),
	-- ОГРН
	[OGRN] NVARCHAR(13),
	-- Дата присовения ОГРН
	[DateOfAssignmentOGRN] DATE,
	-- ФИО директора
	[DirectorFullName] NVARCHAR(128),
	-- ФИО Гл.Бухгалтер
	[ChiefAccountantFullName] NVARCHAR(128),
	-- Основной код ОКВЭД
	[OKVED] NVARCHAR(32),
	-- Является владельцем журнала учёта СКЗИ
	[IsOwnerJournalAccountingCPI] BIT NOT NULL CONSTRAINT DF_Organization_ForAccountingCPI DEFAULT 0

	CONSTRAINT PK_Organization_BusinessEntityID PRIMARY KEY([BusinessEntityID]),
	CONSTRAINT FK_Organization_BusinessEntity_BusinessEntityID FOREIGN KEY([BusinessEntityID]) REFERENCES [HumanResources].[BusinessEntity](BusinessEntityID),


	CONSTRAINT UQ_Organization_OrganizationID UNIQUE([OrganizationID]),

	CONSTRAINT CK_Organization_INN CHECK(LEN([INN]) = 10 OR LEN([INN]) = 12),
	CONSTRAINT CK_Organization_KPP CHECK(LEN([KPP]) = 9),
	CONSTRAINT CK_Organization_OKPO CHECK(LEN([OKPO]) = 8),
	CONSTRAINT CK_Organization_OGRN CHECK(LEN([OGRN]) = 13 OR LEN([OGRN]) = 15),
	CONSTRAINT FK_Organization_Address_AddressID FOREIGN KEY([BusinessAddressID]) REFERENCES [Person].[Address]([AddressID]),
)

-- Название: Тип контакта
CREATE TABLE [Person].[ContactType]
(
	[ContactTypeID] INT IDENTITY, 
	[Name] NVARCHAR(64),
	
	CONSTRAINT PK_ContactType_ContactTypeID PRIMARY KEY([ContactTypeID]),
	CONSTRAINT UQ_ContactType_Name UNIQUE([Name]),
)

-- Название: Контакты организации
CREATE TABLE [HumanResources].[OrganizationContact]
(
	[OrganizationContactID] INT IDENTITY,
	[OrganizationID] INT NOT NULL,
	[ContactTypeID] INT NOT NULL,
	[Value] NVARCHAR(128),
	[Note] NVARCHAR(512) null

	CONSTRAINT PK_OrganizationContact_OrganizationContactID PRIMARY KEY([OrganizationContactID]),
	CONSTRAINT FK_OrganizationContact_Organization_OrganizationID FOREIGN KEY([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID]),
	CONSTRAINT FK_OrganizationContact_ContactType_ContactTypeID FOREIGN KEY([ContactTypeID]) REFERENCES [Person].[ContactType]([ContactTypeID]),
)

-- Название: Должность сотрудника
CREATE TABLE [HumanResources].[EmployeePosition]
(
	[EmployeePositionID] INT IDENTITY,
	[Name] nvarchar(64) NOT NULL,
	[Note] nvarchar(512) NULL,

	CONSTRAINT PK_EmployeePosition_EmployeePositionID PRIMARY KEY([EmployeePositionID]),
	CONSTRAINT UQ_EmployeePosition_Name UNIQUE([Name])
)

-- Название: Сотрудник
-- Описание: Используется для учёта сотрудников организаций
CREATE TABLE [HumanResources].[Employee]
(
	-- ЯВЛЯЕТСЯ ПЕРВИЧНЫМ И ВНЕШНИМ КЛЮЧОМ
	[BusinessEntityID] INT NOT NULL,

	-- НЕ ЯВЛЯЕТСЯ ПЕРВИЧНЫМ КЛЮЧОМ
	[EmployeeID] INT IDENTITY,
	[OrganizationID] int NOT NULL,
	[FirstName] nvarchar(64) NOT NULL,
	[MiddleName] nvarchar(64) NOT NULL,
	[LastName] nvarchar(64) NULL,
	[EmployeePositionID] INT NOT NULL,

	CONSTRAINT PK_Employee_BusinessEntityID PRIMARY KEY([BusinessEntityID]),
	CONSTRAINT FK_Employee_BusinessEntity_BusinessEntityID FOREIGN KEY([BusinessEntityID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT UQ_Employee_EmployeeID UNIQUE([EmployeeID]),
	CONSTRAINT FK_Employee_EmployeePosition_EmployeePositionID FOREIGN KEY([EmployeePositionID]) REFERENCES [HumanResources].[EmployeePosition]([EmployeePositionID]),
	CONSTRAINT FK_Employee_Organization_OrganizationID FOREIGN KEY([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID]),
	--CONSTRAINT FK_Employee_User_UserID FOREIGN KEY ([UserID]) REFERENCES [Authorization].[User]([UserID]),
	--CONSTRAINT UQ_Employee_UserID UNIQUE([UserID]),
)

-- Название: Данные таблиц
-- Описание: Используется для хранения имеющихся таблиц, используется для журнала аудита
CREATE TABLE [Audit].[TableData]
(
	[TableDataID] INT IDENTITY,
	[SchemaName] NVARCHAR(256) NOT NULL,
	[Name] NVARCHAR(256) NOT NULL,
	[RusName] NVARCHAR(256) NULL,

	CONSTRAINT PK_TableData_TableDataID PRIMARY KEY([TableDataID]),
	CONSTRAINT UQ_TableData_Name UNIQUE([Name]),
	CONSTRAINT UQ_TableData_RusName UNIQUE([RusName]),
	CONSTRAINT UQ_TableData_SchemaNameAndName UNIQUE([Name], [SchemaName])
)

-- Создание индекса для ускорения поиска по имени
CREATE INDEX [IX_TableData_Name] ON [Audit].[TableData] ([Name], [SchemaName])

-- Название: Данные столбцов таблиц
-- Описание: Используется для хранения имеющихся столбцов таблиц, используется для журнала аудита
CREATE TABLE [Audit].[TableColumnData]
(
	[TableColumnDataID] INT IDENTITY,
	[TableDataID] INT,
	[Name] NVARCHAR(256) NOT NULL,
	[RusName] NVARCHAR(256) NULL,

	CONSTRAINT PK_TableColumnData_TableColumnDataID PRIMARY KEY([TableColumnDataID]),
	CONSTRAINT FK_TableColumnData_TableData_TableID FOREIGN KEY ([TableDataID]) REFERENCES [Audit].[TableData]([TableDataID]),
	CONSTRAINT UQ_TableColumnData_Name UNIQUE([TableDataID], [Name]) 
)

-- Создание индекса для ускорения поиска по имени
CREATE INDEX [IX_TableColumnData_Name] ON [Audit].[TableColumnData] ([Name])

GO

CREATE FUNCTION [Authentication].[udfGetExistingUserIDByLogin]
(@UserLogin nvarchar(32))
RETURNS INT
AS
BEGIN
	-- Функция для получения ID пользователя по его логину
	DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = @UserLogin)
	RETURN @UserID
END

GO

-- Название: Логи аутенфикаций пользователей
-- Описание: Здесь хранятся все действия всех пользователей
CREATE TABLE [Audit].[LogAuthentication]
(
	[LogAuthenticationID] INT IDENTITY,
	-- Введённый логин
	[EnteredLogin] NVARCHAR(32) NULL,
	-- Тип результата аутенфикации по фактору знания (логин, пароль)
	[FirstFactorResult] BIT NOT NULL,
	-- Тип результата аутенфикации по фактору владения (телефон, токен)
	[SecondFactorResult] BIT NOT NULL,
	-- Ответ сервиса аутенфикации
	[AuthServiceResponse] NVARCHAR(1024) NOT NULL,
	-- Ид пользователя найденного по введнному логину
	[UserIDFoundByEnteredLogin] AS [Authentication].[udfGetExistingUserIDByLogin]([EnteredLogin]),
	-- MAC-адрес устройства
	[MAC] NVARCHAR(12) NULL,
	--IP адрес-устройства
	[IP] NVARCHAR(12) NULL,
	-- Имя компьютера
	[MashineName] NVARCHAR(128) NULL,
	-- Имя пользователя ОС,
	[MashineUserName] NVARCHAR(128) NULL,

	[Date] DATETIME NOT NULL CONSTRAINT DF_LogAuthentication_Date DEFAULT GETDATE(),

	CONSTRAINT PK_LogAuthentication_LogAuthenticationID PRIMARY KEY ([LogAuthenticationID]),
	--CONSTRAINT FK_LogAuthentication_User_UserID FOREIGN KEY ([UserIDFoundByEnteredLogin]) REFERENCES [Authentication].[User]([UserID]),
	--CONSTRAINT FK_LogAuthentication_FirstFactorAuthResultTypeID_ResultType_ResultTypeID FOREIGN KEY ([FirstFactorAuthResultTypeID]) REFERENCES [Authentication].[ResultType]([ResultTypeID]),
	--CONSTRAINT FK_LogAuthentication_SecondFactorAuthResultTypeID_ResultType_ResultTypeID FOREIGN KEY ([SecondFactorAuthResultTypeID]) REFERENCES [Authentication].[ResultType]([ResultTypeID]),
)

-- Название: Сессия пользователя
-- Описание: Используется для предоставления пользователю доступ к системе
CREATE TABLE [Authorization].[UserSession]
(
	-- Идентификатор сессии
	[UserSessionID] INT IDENTITY,
	-- Зашифрованный ключ сессии
	-- Используется для авторизации и дальнейших действий в системе
	--[SessionKey] varbinary(max) NOT NULL CONSTRAINT DF_UserSession_SessionKey
	--						DEFAULT ENCRYPTBYASYMKEY(ASYMKEY_ID('Main_AsymmetricKeys_Saes'),
	--												[Authentication].[udfGenerateSecretString](128)),
	[SessionKey] nvarchar(128) NOT NULL,
	-- Идентификатор пользователя
	[UserID] INT NOT NULL,
	-- Дата и время создание сессии
	[CreatedAt] DATETIME NOT NULL,
	-- Дата и время окончания действия сессии
	[ExpiredAt] DATETIME NOT NULL,
	-- Истекла ли сессия
	[IsExpired] AS CONVERT(BIT, CASE WHEN (GETDATE() > [ExpiredAt]) then 1 else 0 end, 0),
	-- Связанный лог аутенфикации
	[LogAuthenticationID] INT NOT NULL,

	CONSTRAINT PK_UserSession_UserSessionID PRIMARY KEY([UserSessionID]),
	CONSTRAINT UQ_UserSession_SessionKey UNIQUE([SessionKey]),
	CONSTRAINT FK_UserSession_User_UserID FOREIGN KEY([UserID]) REFERENCES [Authentication].[User]([UserID]),
	CONSTRAINT FK_UserSession_LogAuthentication_LogAuthenticationID FOREIGN KEY(LogAuthenticationID) REFERENCES [Audit].[LogAuthentication]([LogAuthenticationID]),
)

-- При взаимодействии с системой будет часто производиться поиск по ключу сесси
CREATE NONCLUSTERED INDEX IX_UserSession_SessionKey
ON [Authorization].[UserSession]([SessionKey])


-- Название: Логи ошибок
-- Описание: используется для аудита ошибок базы данных
CREATE TABLE [Audit].[ErrorLog]
(
	[ErrorLogID] INT IDENTITY,
	[ErrorTime] DATETIME NOT NULL CONSTRAINT DF_ErrorLog_ErrorTime DEFAULT GETDATE(), 
	[UserSessionID] INT NULL,
	[ErrorNumber] INT NOT NULL,
	[ErrorSeverity] INT NULL,
	[ErrorState] INT NULL,
	[ErrorProcedure] NVARCHAR(126) NULL,
	[ErrorLine] INT NULL,
	[ErrorMessage] NVARCHAR(4000) NOT NULL,

	CONSTRAINT PK_ErrorLog_ErrorLogID PRIMARY KEY ([ErrorLogID]),
	CONSTRAINT FK_ErrorLog_UserSession_UserSessionID FOREIGN KEY ([UserSessionID]) REFERENCES [Authorization].[UserSession]([UserSessionID])

)

-- Название: Аудит
-- Описание: Здесь хранятся все действия всех пользователей
CREATE TABLE [Audit].[Log]
(
	[LogID] INT IDENTITY,
	[TableDataID] INT NULL,
	[TableRowID] INT NOT NULL,
	-- Действия (лат. буквы):
	-- 'C'(Create) - Создал запись
	-- 'U'(Update) - Обновил запись
	-- 'D'(Delete) - удалил запись
	-- 'R'(Recover) - восстановил запись (SysIsDeleted => 1) 
	[Action] CHAR(1) NOT NULL,
	--[UserID] INT NULL,
	[UserSessionID] INT NULL,
	[Date] DATETIME NOT NULL CONSTRAINT DF_Log_Date DEFAULT GETDATE(),
	[GUID] uniqueidentifier ROWGUIDCOL NOT NULL

	CONSTRAINT PK_Log_LogID PRIMARY KEY([LogID]),
	CONSTRAINT FK_Log_TableData_TableDataID FOREIGN KEY([TableDataID]) REFERENCES [Audit].[TableData]([TableDataID]),
	CONSTRAINT CK_Log_Action CHECK(CHARINDEX([Action], 'CUDR') <> 0),
	--CONSTRAINT FK_Log_Employee_UserID FOREIGN KEY([UserID]) REFERENCES [Authentication].[User]([UserID]),
	CONSTRAINT FK_Log_UserSession_UserSessionID FOREIGN KEY([UserSessionID]) REFERENCES [Authorization].[UserSession]([UserSessionID]),
)

-- Название: Журнал аудита
-- Описание: Здесь хранятся все действия всех пользователей
CREATE TABLE [Audit].[LogChange]
(
	[AuditLogID] INT,
	[TableColumnDataID] INT,
	[OldValue] NVARCHAR(512) NULL,
	[NewValue] NVARCHAR(512) NULL,

	CONSTRAINT FK_LogChange_Log_LogID FOREIGN KEY ([AuditLogID]) REFERENCES [Audit].[Log]([LogID]),
	CONSTRAINT FK_LogChange_TableColumnData_TableColumnDataID FOREIGN KEY ([TableColumnDataID]) REFERENCES [Audit].[TableColumnData]([TableColumnDataID]),
	CONSTRAINT UQ_LogChange_AuditLogID_TableColumnDataID UNIQUE([AuditLogID], [TableColumnDataID])
)

GO
-- Название: Логи (Человеко-читаемые)
-- Описание: Представления для логов, чтобы можно было их прочитать
CREATE VIEW [Audit].[viewLog]
AS 
	SELECT
	[lg].[LogID] AS [LogID],
	[lg].[Date] as [Date],
	CASE [lg].[Action]
		WHEN 'C' THEN 'Создание'
		WHEN 'U' THEN 'Обновление'
		WHEN 'D' THEN 'Удаление'
		WHEN 'R' THEN 'Восстановление'
	END AS [Action],
	[td].[Name] as [Table],
	[lg].[TableRowID] as [TableRecordID],
	[us].[Login] as [User],
	[ur].[Name] as [UserRole]
	FROM [Audit].[Log] as [lg]
	INNER JOIN [Audit].[TableData] as [td] on [td].[TableDataID] = [lg].[TableDataID]
	INNER JOIN [Authorization].[UserSession] as [uss] on [uss].[UserSessionID] = [lg].[UserSessionID]
	INNER JOIN [Authentication].[User] as [us] on [us].[UserID] = [uss].[UserID]
	INNER JOIN [Authorization].[UserRole] as [ur] on [ur].[UserRoleID] = [us].[UserRoleID]

GO
-- Название: Логи изменени (Человеко-читаемые)
-- Описание: Представления для логов изменений, чтобы можно было их прочитать
CREATE VIEW [Audit].[viewLogChange]
AS 
	SELECT
	[lgc].[AuditLogID] AS [LogID],
	[td].[Name] as [Table],
	[tcd].[Name] as [Column],
	[lgc].[OldValue] as [OldValue],
	[lgc].[NewValue] as [NewValue]
	FROM [Audit].[LogChange] as [lgc]
	INNER JOIN [Audit].[TableColumnData] as [tcd] on [tcd].[TableColumnDataID] = [lgc].[TableColumnDataID]
	INNER JOIN [Audit].[TableData] as [td] on [td].[TableDataID] = [tcd].[TableDataID]
GO

-- ***********************************************************************
-- Удалена из за ненадобности
-- В процессе разработки её надобность была переосмыслена и будет заменана на HumanResources.BusinessEntity
-- ***********************************************************************
-- Название: Субъект журнала учета СКЗИ
-- Описание: Например компания ООО Рога и Копыта или сотрудник Иванов Иван Иваныч
--CREATE TABLE [AccountingCPI].[Subject]
--(
--	[SubjectID] INT IDENTITY,
--	[OrganizationID] INT,
--	[EmployeeID] INT,

--	CONSTRAINT PK_Subject_SubjectID PRIMARY KEY([SubjectID]),
--	CONSTRAINT FK_Subject_Organization_OrganizationID FOREIGN KEY([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID]),
--	CONSTRAINT FK_Subject_Employee_EmployeeID FOREIGN KEY([EmployeeID]) REFERENCES [HumanResources].[Employee]([EmployeeID]),
--	CONSTRAINT CK_Subject_OrganizationOrEmployee CHECK(
--		([OrganizationID] IS NOT NULL AND [EmployeeID] IS NULL) OR
--		([OrganizationID] IS NULL AND [EmployeeID] IS NOT NULL)
--	)
--)

-- Название: Журнал поэкземплярного учета СКЗИ для органа криптографической защиты
CREATE TABLE [AccountingCPI].[JournalInstanceForCPARecord]
(
	-- Какой организации принадлежит данная запись
	[OrganizationID] INT,
	-- №1
	[JournalInstanceForCPARecordID] INT IDENTITY,
	-- №2 Наименование СКЗИ, эксплуатационной и технической документации к ним, ключевых документов
	[NameCPI] nvarchar(512) null,
	-- №3 Серийные номера СКЗИ, эксплуатационной и технической документации к ним, номера серий ключевых документов 
	[SerialCPI] nvarchar(256) null,
	-- №4 Номера экземпляров (криптографические номера) ключевых документов 
	[InstanceNumber] int null,
	
	-- Блок: Отметка о получении 
	--№5 От кого получены или Ф.И.О. сотрудника органа криптографической защиты, 
	[ReceivedFromID] int null, 
	--№6 Дата и номер сопроводительного письма или дата изготовления ключевых документов и расписка в изготовлении 
	[DateAndNumberCoverLetterReceive] nvarchar(256) null,

	-- Блок: Отметка о рассылке (передаче) 
	-- №7 [Кому разосланы (переданы) таблица: JournalInstanceAccountingCPIForCPAReceiver]
	-- №8 Дата и номер сопроводительного письма 
	[DateAndNumberCoverLetterSend] nvarchar(256) null,
	-- №9 Дата и номер подтверждения или расписка в получении 
	[DateAndNumberConfirmationSend] nvarchar(256) null,

	-- Блок: Отметка о возврате 
	-- №10 Дата и номер сопроводительного письма 
	[DateAndNumberCoverLetterReturn] nvarchar(256) null,
	-- №11 Дата и номер подтверждения 
	[DateAndNumberConfirmationReturn] nvarchar(256) null,

	-- №12 Дата ввода в действие 
	[CommissioningDate] datetime null,
	-- №13 Дата вывода из действия 
	[DecommissioningDate] datetime null,

	-- Блок: Отметка об уничтожении СКЗИ, ключевых документов 
	-- №14 Дата уничтожения 
	[DestructionDate] datetime null,
	-- №15 Номер акта или расписка об уничтожении 
	[DestructionActNumber] nvarchar(256) null,
	
	-- №16 Примечание
	[Note] nvarchar(512) null,

	-- Файл подписи
	[SignFileID] int

	CONSTRAINT PK_JournalInstanceForCPARecord_JournalInstanceForCPARecordID PRIMARY KEY([JournalInstanceForCPARecordID]),
	CONSTRAINT FK_JournalInstanceForCPARecord_BusinessEntity_BusinessEntityID FOREIGN KEY ([ReceivedFromID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_JournalInstanceForCPARecord_File_FileID FOREIGN KEY ([SignFileID]) REFERENCES [File]([FileID]),
	CONSTRAINT FK_JournalInstanceForCPARecord_Organization_OrganizationID FOREIGN KEY ([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID]),
)

-- Название:  Субъекты журнала поэкземплярного учета СКЗИ для органа криптографической защиты которым была разослана информация
-- Описание: Используется для указания субъектов (Юр.лица, Физ.лица) в столбцах типа:
-- 1. Кому разосланы (переданы) 
CREATE TABLE [AccountingCPI].[JournalInstanceCPAReceiver]
(
	[JournalInstanceCPAReceiverID] INT IDENTITY,
	-- Ссылка на запись в журнале
	[RecordID] INT,
	-- Приемщик (Субъект)
	[ReceiverID] INT,

	CONSTRAINT PK_JournalInstanceCPAReceiver_JournalInstanceCPAReceiverID PRIMARY KEY ([JournalInstanceCPAReceiverID]),
	CONSTRAINT FK_JournalInstanceCPAReceiver_JournalInstanceForCPARecord_JournalInstanceForCPARecordID FOREIGN KEY ([JournalInstanceCPAReceiverID]) REFERENCES [AccountingCPI].[JournalInstanceForCPARecord]([JournalInstanceForCPARecordID]),
	CONSTRAINT FK_JournalInstanceCPAReceiver_BusinessEntity_BusinessEntityID FOREIGN KEY ([ReceiverID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT UQ_JournalInstanceCPAReceiver_RecordID_ReceiverID UNIQUE([RecordID], [ReceiverID])
)

-- Название: Аппаратура
-- Описание: Используется в журналах
-- Для учета рабочие мест(рабочих станций) в организации
CREATE TABLE [Office].[Hardware]
(
	[HardwareID] INT IDENTITY,
	-- Какой организации принадлежит аппаратура
	[OrganizationID] int,
	-- Название аппаратуры
	[Name] nvarchar(256) null,
	-- Серийный номер (Сер. материнской платы или пломбы (На корпусе ПК))
	[SerialNumber] nvarchar(32),
	-- Заметка
	[Note] nvarchar(512) null,

	CONSTRAINT PK_Hardware_HardwareID PRIMARY KEY ([HardwareID]),
	CONSTRAINT UQ_Hardware_SerialNumber UNIQUE ([SerialNumber], [OrganizationID])
)

-- Название: Журнал поэкземплярного учета скзи, 
-- эксплуатационной и технической документации к ним,
-- ключевых документов (для обладателя конфиденциальной информации) 
CREATE TABLE [AccountingCPI].[JournalInstanceForCIHRecord]
(
	-- Какой организации принадлежит аппаратура
	[OrganizationID] int,
	-- №1
	[JournalInstanceForCIHRecordID] INT IDENTITY,
	-- №2 Наименование СКЗИ, эксплуатационной и технической документации к ним, ключевых документов
	[NameCPI] nvarchar(512) null,
	-- №3 Серийные номера СКЗИ, эксплуатационной и технической документации к ним, номера серий ключевых документов 
	[SerialCPI] nvarchar(256) null,
	-- №4 Номера экземпляров (криптографические номера) ключевых документов 
	[InstanceNumber] int null,

	-- Блок: Отметка о получении 
	-- №5 От кого получены 
	[ReceivedFromID] int null, 
	-- №6 Дата и номер сопроводительного письма 
	[DateAndNumberCoverLetterReceive] nvarchar(256) null,

	-- Блок: Отметка о выдаче 
	-- №7 Ф.И.О. пользователя СКЗИ 
	[CPIUserID] int null, 
	-- №8 Дата и расписка в получении 
	[DateAndNumberConfirmationIssue] nvarchar(256) null,

	-- Блок: Отметка о подключении (установке СКЗИ) 
	-- №9 [Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ,
	-- произведших подключение (установку) таблица]: JournalInstanceAccountingCPIForCIHInstallers
	-- №10 Дата подключения (установки) и подписи лиц, произведших подключение (установку) 
	[InstallationDateAndConfirmation] nvarchar(256) null,
	-- №11 [Номера аппаратных средств, в которые установлены или к которым подключены СКЗИ 
	-- таблица]: JournalInstanceAccountingCPIForCIHConnectedHardwares
	
	-- Блок: Отметка об изъятии СКЗИ из аппаратных средств, уничтожении ключевых документов 
	-- №12 Дата изъятия (уничтожения) 
	[DestructionDate] datetime null,
	-- №13 [Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ,
	-- производивших изъятие (уничтожение) таблица]: JournalInstanceAccountingCPIForCIHDestructors
	-- №14 Номер акта или расписка об уничтожении 
	[DestructionActNumber] nvarchar(256) null,
	
	-- №15 Примечание
	[Note] nvarchar(512) null,

	-- Файл подписи
	[SignFileID] int

	CONSTRAINT PK_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID PRIMARY KEY([JournalInstanceForCIHRecordID]),
	--CONSTRAINT FK_JournalInstanceForCIHRecord_Subject_SubjectID_0 FOREIGN KEY ([ReceivedFromID]) REFERENCES [AccountingCPI].[Subject]([SubjectID]),
	--CONSTRAINT FK_JournalInstanceForCIHRecord_Subject_SubjectID_1 FOREIGN KEY ([CPIUserID]) REFERENCES [AccountingCPI].[Subject]([SubjectID]),
	CONSTRAINT FK_JournalInstanceForCIHRecord_ReceivedFromID_BusinessEntity_BusinessEntityID FOREIGN KEY ([ReceivedFromID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_JournalInstanceForCIHRecord_CPIUserID_BusinessEntity_BusinessEntityID FOREIGN KEY ([CPIUserID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_JournalInstanceForCIHRecord_File_FileID FOREIGN KEY ([SignFileID]) REFERENCES [File]([FileID]),
	CONSTRAINT FK_JournalInstanceForCIHRecord_Organization_OrganizationID FOREIGN KEY ([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID])
)

-- Название: Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ,
-- произведших подключение (установку)
-- Описание: Используется в журнале поэкземплярного учета скзи,
-- эксплуатационной и технической документации к ним,
-- ключевых документов (для обладателя конфиденциальной информации) 
CREATE TABLE [AccountingCPI].[JournalInstanceForCIHInstaller]
(
	[JournalInstanceForCIHInstallerID] INT IDENTITY,
	-- Ссылка на запись в журнале
	[RecordID] INT NOT NULL ,
	-- Субъект производивший установку
	[InstallerID] INT NOT NULL,

	CONSTRAINT PK_JournalInstanceForCIHInstaller_JournalInstanceForCIHInstallerID PRIMARY KEY ([JournalInstanceForCIHInstallerID]),
	CONSTRAINT UQ_JournalInstanceForCIHInstaller_RecordID_InstallerID UNIQUE([RecordID], [InstallerID]),
	CONSTRAINT FK_JournalInstanceForCIHInstaller_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID FOREIGN KEY([RecordID]) REFERENCES [AccountingCPI].[JournalInstanceForCIHRecord]([JournalInstanceForCIHRecordID]),
	CONSTRAINT FK_JournalInstanceForCIHInstaller_BusinessEntity_BusinessEntityID FOREIGN KEY([InstallerID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID])
)

-- Название: Номера аппаратных средств, в которые установлены или к которым подключены СКЗИ
-- Описание: Используется в журнале поэкземплярного учета скзи,
-- эксплуатационной и технической документации к ним,
-- ключевых документов (для обладателя конфиденциальной информации) для указания
-- аппаратных средств, в которые установлены СКЗИ
CREATE TABLE [AccountingCPI].[JournalInstanceForCIHConnectedHardware]
(
	[JournalInstanceForCIHConnectedHardwareID] INT IDENTITY,
	-- Запись в журнале
	[RecordID] int not null,
	-- Аппарат
	[HardwareID] int not null,

	CONSTRAINT PK_JournalInstanceForCIHConnectedHardware_JournalInstanceForCIHConnectedHardwareID PRIMARY KEY([JournalInstanceForCIHConnectedHardwareID]),
	CONSTRAINT FK_JournalInstanceForCIHConnectedHardware_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID FOREIGN KEY([RecordID]) REFERENCES [AccountingCPI].[JournalInstanceForCIHRecord]([JournalInstanceForCIHRecordID]),
	CONSTRAINT FK_JournalInstanceForCIHConnectedHardware_Hardware_HardwareID FOREIGN KEY([HardwareID]) REFERENCES [Office].[Hardware]([HardwareID]),
	CONSTRAINT UQ_RecordCPIForCIH_RecordID_HardwareID UNIQUE([RecordID], [HardwareID])
)

-- Название: Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ,
-- производивших изъятие (уничтожение)
-- Описание: Используется в журнале поэкземплярного учета скзи,
-- эксплуатационной и технической документации к ним,
-- ключевых документов (для обладателя конфиденциальной информации) для указания 
-- сотрудников производивших изъятие СКЗИ
CREATE TABLE [AccountingCPI].[JournalInstanceForCIHDestructor]
(
	[JournalInstanceForCIHDestructorID] INT IDENTITY,
	-- Запись в журнале
	[RecordID] int not null,
	-- Субъект производивший изъятие СКЗИ
	[DestructorID] int not null,

	CONSTRAINT PK_JournalInstanceForCIHDestructor_JournalInstanceForCIHDestructorID PRIMARY KEY ([JournalInstanceForCIHDestructorID]),
	CONSTRAINT FK_JournalInstanceForCIHDestructor_BusinessEntity_BusinessEntityID FOREIGN KEY([DestructorID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_JournalInstanceForCIHDestructor_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID FOREIGN KEY([RecordID]) REFERENCES [AccountingCPI].[JournalInstanceForCIHRecord]([JournalInstanceForCIHRecordID]),
	CONSTRAINT UQ_JournalInstanceForCIHDestructor_RecordID_DestructorID UNIQUE([RecordID], [DestructorID])
)

-- Название: Тип ключевого документа
-- Описание: Используется в журнал технический (аппаратный)
-- Типы ключевых документов могут включать ключи, сертификаты,
-- лицензии, ключи электронной подписи и другие элементы. 
CREATE TABLE [AccountingCPI].[KeyDocumentType]
(
	[KeyDocumentTypeID] INT IDENTITY,
	[Name] nvarchar(256) NOT NULL,
	[Note] nvarchar(256) NULL,

	CONSTRAINT PK_KeyDocumentType_KeyDocumentTypeID PRIMARY KEY([KeyDocumentTypeID]),
	CONSTRAINT UQ_KeyDocumentType_Name UNIQUE ([Name])
)

-- Название: Тип ключевого носителя
CREATE TABLE [AccountingCPI].[KeyHolderType]
(
	[KeyHolderTypeID] INT IDENTITY,
	[Name] nvarchar(256) NOT NULL,

	CONSTRAINT PK_KeyHolderType_KeyHolderTypeID PRIMARY KEY([KeyHolderTypeID]),
	CONSTRAINT UQ_KeyHolderType_Name UNIQUE ([Name])
)

-- Название: Ключевой носитель
CREATE TABLE [AccountingCPI].[KeyHolder]
(
	[KeyHolderID] INT IDENTITY,
	-- Серийный номер
	[SerialNumber] nvarchar(128) not null,
	-- Тип ключевого носителя
	[TypeID] INT NOT NULL,
	-- Пользователь СКЗИ которому принадлежит данный носитель
	[UserCPI] INT NULL,
	-- Файл подписи
	[SignFileID] INT NULL,

	CONSTRAINT PK_KeyHolder_KeyHolderID PRIMARY KEY([KeyHolderID]),
	CONSTRAINT UQ_KeyHolder_SerialNumber UNIQUE([SerialNumber]),
	CONSTRAINT FK_KeyHolder_KeyHolderType_KeyHolderTypeID FOREIGN KEY([TypeID]) REFERENCES [AccountingCPI].[KeyHolderType]([KeyHolderTypeID]),
	CONSTRAINT FK_KeyHolder_BusinessEntity_BusinessEntityID FOREIGN KEY([UserCPI]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_KeyHolder_File_FileID FOREIGN KEY([SignFileID]) REFERENCES [dbo].[File]([FileID]),
)

-- Название: Журнал технический (аппаратный) 
CREATE TABLE [AccountingCPI].[JournalTechnicalRecord]
(
	-- Какой организации принадлежит аппаратура
	[OrganizationID] int,
	-- №1
	[JournalTechnicalRecordID] INT IDENTITY,
	-- №2 Дата
	[Date] DATE NULL,
	-- №3 Тип и серийные номера используемых СКЗИ
	[TypeAndSerialUsedCPI] nvarchar(256) null,
	-- №4 Записи по обслуживанию СКЗИ 
	[RecordOnMaintenanceCPI] nvarchar(256) null,
	
	-- Блок: Используемые криптоключи 
	-- №5 [Тип ключевого документа]
	[KeyDocumentTypeID] INT null ,
	-- №6 Серийный, криптографический номер и номер экземпляра ключевого документа 
	[SerialCPIAndKeyDocumentInstanceNumber] nvarchar(256) null,
	-- №7 Номер разового ключевого носителя или зоны СКЗИ, в которую введены криптоключи 
	[NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted] NVARCHAR(256) NULL,
	
	-- Блок: Отметка об уничтожении (стирании) 
	-- №8 Дата
	[DestructionDate] datetime null,
	-- №9 Подпись пользователя СКЗИ (Пока номер акта, пока нет КЭП)
	[ActNumber] nvarchar(256) null,
	-- №10 Примечание
	[Note] nvarchar(512) null,

	-- Файл подписи
	[SignFileID] int

	CONSTRAINT PK_JournalTechnicalRecord_JournalTechnicalRecordID PRIMARY KEY ([JournalTechnicalRecordID]),
	CONSTRAINT FK_JournalTechnicalRecord_KeyDocumentType_KeyDocumentTypeID FOREIGN KEY ([KeyDocumentTypeID]) REFERENCES [AccountingCPI].[KeyDocumentType]([KeyDocumentTypeID]),
	CONSTRAINT FK_JournalTechnicalRecord_Organization_OrganizationID FOREIGN KEY ([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID]),
	CONSTRAINT FK_JournalTechnicalRecord_File_FileID FOREIGN KEY ([SignFileID]) REFERENCES [File]([FileID])
)

GO

CREATE FUNCTION [dbo].[udfHashSalt]
	(@Value NVARCHAR(256),
	@Salt VARBINARY(16))
RETURNS VARBINARY(32)
BEGIN
	-- Функция для хеширования строкового значения с солью из 16 байт алгоритмом SHA2_256
	DECLARE @Result VARBINARY(32)
	SET @Result = HASHBYTES('SHA2_256', CONCAT(@Value, CONVERT(NVARCHAR(16), @Salt, 2)))
    RETURN @Result
END;
GO

GO
CREATE FUNCTION [Authorization].[udfVerifyUser]
(
	@UserLogin nvarchar(32),
	@UserPassword nvarchar(32)
)
RETURNS BIT
AS
BEGIN
	DECLARE @PasswordHash VARBINARY(32)
	DECLARE @PasswordSalt VARBINARY(16)

	SELECT TOP 1 @PasswordHash = [PasswordHash], @PasswordSalt = [PasswordSalt] FROM [Authorization].[User] WHERE [Login] = @UserLogin

	IF @@ROWCOUNT = 0
	RETURN 0

	DECLARE @Result BIT

	SET @Result = IIF([dbo].[udfHashSalt](@UserPassword, @PasswordSalt) = @PasswordHash, 1, 0)

	RETURN @Result
END

GO
-- uspPrintError prints error information about the error that caused 
-- execution to jump to the CATCH block of a TRY...CATCH construct. 
-- Should be executed from within the scope of a CATCH block otherwise 
-- it will return without printing any error information.
CREATE PROCEDURE [Audit].[uspPrintError] 
AS
BEGIN
    SET NOCOUNT ON;

    -- Print error information. 
    PRINT 'Error ' + CONVERT(varchar(50), ERROR_NUMBER()) +
          ', Severity ' + CONVERT(varchar(5), ERROR_SEVERITY()) +
          ', State ' + CONVERT(varchar(5), ERROR_STATE()) + 
          ', Procedure ' + ISNULL(ERROR_PROCEDURE(), '-') + 
          ', Line ' + CONVERT(varchar(5), ERROR_LINE());
    PRINT ERROR_MESSAGE();
END;

GO

CREATE PROCEDURE [Audit].[uspDeleteMeAfterUsing]
(@Template nvarchar(MAX),
@PrintTemplate nvarchar(max)) AS
BEGIN
	-- Процедура для выполнения динамического sql шаблона, в который подставляется имена таблиц и схем
	-- Используется для создания триггеров для всех таблиц, и прочего
	-- Нужно лишь на этапе выполнения скрипта
	SET NOCOUNT ON;

	DECLARE @trancount int = 0

	BEGIN TRY
		IF @@TRANCOUNT = 0
		BEGIN
			BEGIN TRANSACTION
			SET @trancount = 1
		END
		

		DECLARE @MyCursor CURSOR;
		DECLARE @TableName NVARCHAR(MAX);
		DECLARE @SchemaName NVARCHAR(MAX);
		DECLARE @SqlMain NVARCHAR(MAX);
		DECLARE @SqlPrint NVARCHAR(MAX);

		DECLARE @SqlUpdateForExecTemplate nvarchar(MAX) = 'UPDATE [#SCHEMA].[#TABLE] SET #SQLUPDATE FROM (SELECT TOP 1 * FROM [inserted] as [ins] WHERE [ins].[#TABLEID] = @RowID) AS source WHERE [#SCHEMA].[#TABLE].[#TABLEID] = @RowID'
		DECLARE @SqlUpdateColumnTemplate nvarchar(max) = ',[#SCHEMA].[#TABLE].[#NAME] = [source].[#NAME]'
		DECLARE @SqlUpdateColumnIdTemplate nvarchar(max) = '[#SCHEMA].[#TABLE].[#TABLEID] = [source].[#TABLEID]'
		DECLARE @SqlUpdate nvarchar(MAX) = ''

		SET @Template = REPLACE(@Template, '#CRLF', CHAR(13)+CHAR(10))
		SET @PrintTemplate = REPLACE(@PrintTemplate, '#CRLF', CHAR(13)+CHAR(10))

		BEGIN
			SET @MyCursor = CURSOR FOR
			select TABLE_SCHEMA, TABLE_NAME from INFORMATION_SCHEMA.TABLES
				WHERE 
			TABLE_TYPE = 'BASE TABLE' AND
			TABLE_SCHEMA NOT IN ('Audit', 'dbo') AND
			TABLE_NAME NOT IN ('UserSession')

			OPEN @MyCursor 
			FETCH NEXT FROM @MyCursor 
			INTO @SchemaName, @TableName

			WHILE @@FETCH_STATUS = 0
			BEGIN
		  
			  SET @SqlMain = REPLACE(REPLACE(@Template, '#SCHEMA', @SchemaName),  '#TABLE', @TableName)
			  SET @SqlPrint = REPLACE(REPLACE(@PrintTemplate, '#SCHEMA', @SchemaName),  '#TABLE', @TableName)

			  IF (charindex('#UPDATE',@SqlMain) > 0)
			  BEGIN
				SELECT @SqlUpdate = @SqlUpdate + REPLACE(@SqlUpdateColumnTemplate, '#NAME', [sc].[name])
				FROM sys.columns as sc
				WHERE object_id = OBJECT_ID(REPLACE(REPLACE('[#SCHEMA].[#TABLE]', '#SCHEMA', @SchemaName),  '#TABLE', @TableName))
				AND ([sc].[is_identity] = 0 AND [sc].[is_computed] = 0)

				SET @SqlUpdate = SUBSTRING(@SqlUpdate, 2, len(@SqlUpdate)-1)
				SET @SqlUpdate = REPLACE(REPLACE(@SqlUpdate, '#SCHEMA', @SchemaName),  '#TABLE', @TableName)

				SET @SqlMain = REPLACE(@SqlMain, '#UPDATE', REPLACE(REPLACE(REPLACE(@SqlUpdateForExecTemplate, '#SCHEMA', @SchemaName),'#TABLE', @TableName),'#SQLUPDATE', @SqlUpdate))
				SET @SqlUpdate = ''
				
			  END

			  BEGIN TRY
				EXEC sp_executesql @SqlMain
				EXEC sp_executesql @SqlPrint
			  END TRY
			  BEGIN CATCH
				PRINT '[ОШИБКА] Не удалось выполнить T-SQL скрипт: '+ERROR_MESSAGE()
				PRINT @SqlMain
			  END CATCH

			  FETCH NEXT FROM @MyCursor 
			  INTO @SchemaName, @TableName
			END; 

			CLOSE @MyCursor ;
			DEALLOCATE @MyCursor;
		END;

		IF @trancount = 1
		COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        EXECUTE [Audit].[uspPrintError]
	END CATCH
END

GO
-- ***********************************************************************
-- Шаблон скрипта добавления столбцов [SysIsDeleted] BIT NOT NULL и [SysModifiedDate] DATETIME NOT NULL
-- ко всем таблицам, кроме входящих в схемы [Audit], [dbo]
-- ***********************************************************************
DECLARE @Template nvarchar(max) = 'ALTER TABLE [#SCHEMA].[#TABLE] ADD [SysIsDeleted] BIT NOT NULL; \
ALTER TABLE [#SCHEMA].[#TABLE] ADD CONSTRAINT DF_#TABLE_SysIsDeleted DEFAULT 0 FOR [SysIsDeleted]; \
ALTER TABLE [#SCHEMA].[#TABLE] ADD [SysModifiedDate] DATETIME NOT NULL; \
ALTER TABLE [#SCHEMA].[#TABLE] ADD CONSTRAINT DF_#TABLE_SysModifiedDate DEFAULT GETDATE() FOR [SysModifiedDate];'

DECLARE @PrintTemplate nvarchar(max) = 'PRINT ''Сделаны столбцы SysModifiedDate и SysIsDeleted для таблицы #SCHEMA.#TABLE'''

EXEC [Audit].[uspDeleteMeAfterUsing] @Template, @PrintTemplate

-- ***********************************************************************
-- Создается пользователь 'admin' для проверки работы аудита, потом будут добавлены другие
-- ***********************************************************************

INSERT INTO [Authorization].[UserRole] ([Name]) VALUES ('admin')

DECLARE @Salt VARBINARY(16) = CRYPT_GEN_RANDOM(16);
DECLARE @Password NVARCHAR(16) = '12345'

INSERT INTO [Authentication].[User](Login, PasswordHash, PasswordSalt, UserRoleID, LastLoginDate, TotpSecretKey) VALUES
('admin', [dbo].[udfHashSalt](@Password, @Salt), @Salt, (SELECT [UserRoleID] from [Authorization].[UserRole] where [Name] = 'admin'), GETDATE(), 'HVR4CFHAFOWFGGFAGSA5JVTIMMPG6GMT')

GO
CREATE PROCEDURE [Authorization].[uspSetCurrentUserSessionID]
(@SessionKey nvarchar(128))
AS 
BEGIN
	-- Procedure set current user id in sessino context dictionary
	SET NOCOUNT ON

	IF (@SessionKey IS NULL)
		RAISERROR('@SessionKey parameter has NULL value', 19, 1)

	DECLARE @UserSessionID INT
	DECLARE @IsExpired BIT

	SELECT TOP 1 @UserSessionID = [UserSessionID], @IsExpired = [IsExpired]
	FROM [Authorization].[UserSession]
	WHERE [SessionKey] = @SessionKey

	IF @UserSessionID IS NULL
	BEGIN
		RAISERROR('@SessionKey parameter has incorrect value', 19, 2)
	END

	IF @IsExpired = 1
	BEGIN
		RAISERROR('You cannot make this session current because it has expired.', 19, 3)
	END

	EXEC sp_set_session_context N'UserSessionID', @UserSessionID
END

GO

CREATE PROCEDURE [Authorization].[uspGetCurrentUserSessionID]
(@UserSessionID int OUTPUT)
AS 
BEGIN
	SET NOCOUNT ON;
	DECLARE @TempUserSessionID int = CONVERT(int, SESSION_CONTEXT(N'UserSessionID'))

	IF @TempUserSessionID IS NULL
		GOTO SET_NULL_USERID

	IF NOT EXISTS(SELECT TOP 1 1 FROM [Authorization].[UserSession] WHERE [UserSessionID] = @TempUserSessionID)
		GOTO SET_NULL_USERID

	SET @UserSessionID = @TempUserSessionID
	RETURN

	SET_NULL_USERID:
		SET @UserSessionID = NULL
END

GO
CREATE PROCEDURE [Authorization].[uspPrintWarningIfCurrentUserSessionNotSet]
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @UserSessionID INT

	EXEC [Authorization].[uspGetCurrentUserSessionID] @UserSessionID OUTPUT

	IF @UserSessionID IS NULL
	BEGIN
		PRINT '[WARNING] The current user session was not specified through the procedure Authorization.uspSetCurrentUserSessionID'
	END
END

GO
-- uspLogError logs error information in the ErrorLog table about the 
-- error that caused execution to jump to the CATCH block of a 
-- TRY...CATCH construct. This should be executed from within the scope 
-- of a CATCH block otherwise it will return without inserting error 
-- information. 
CREATE PROCEDURE [Audit].[uspLogError] 
    @ErrorLogID [int] = 0 OUTPUT -- contains the ErrorLogID of the row inserted
AS                               -- by uspLogError in the ErrorLog table
BEGIN
    SET NOCOUNT ON;

    -- Output parameter value of 0 indicates that error 
    -- information was not logged
    SET @ErrorLogID = 0;

    BEGIN TRY
        -- Return if there is no error information to log
        IF ERROR_NUMBER() IS NULL
            RETURN;

        -- Return if inside an uncommittable transaction.
        -- Data insertion/modification is not allowed when 
        -- a transaction is in an uncommittable state.
        IF XACT_STATE() = -1
        BEGIN
            PRINT 'Cannot log error since the current transaction is in an uncommittable state. ' 
                + 'Rollback the transaction before executing uspLogError in order to successfully log error information.';
            RETURN;
        END

		DECLARE @UserSessionID INT

		EXEC [Authorization].[uspGetCurrentUserSessionID] @UserSessionID OUTPUT

		EXEC [Authorization].[uspPrintWarningIfCurrentUserSessionNotSet]

        INSERT [Audit].[ErrorLog] 
            (
            [UserSessionID], 
            [ErrorNumber], 
            [ErrorSeverity], 
            [ErrorState], 
            [ErrorProcedure], 
            [ErrorLine], 
            [ErrorMessage]
            ) 
        VALUES 
            (
            @UserSessionID, 
            ERROR_NUMBER(),
            ERROR_SEVERITY(),
            ERROR_STATE(),
            ERROR_PROCEDURE(),
            ERROR_LINE(),
            ERROR_MESSAGE()
            );

        -- Pass back the ErrorLogID of the row inserted
        SET @ErrorLogID = @@IDENTITY;

		EXECUTE [Audit].[uspPrintError];
    END TRY
    BEGIN CATCH
        PRINT 'An error occurred in stored procedure uspLogError: ';
        EXECUTE [Audit].[uspPrintError];
        RETURN -1;
    END CATCH
END;

GO
CREATE PROCEDURE [Authentication].[uspAddUser]
(
	@UserLogin nvarchar(32),
	@UserPassword nvarchar(32),
	@UserRoleID INT
)
AS
BEGIN
	-- Процедура для добавления пользователя, которая автоматически хеширует пароль пользователя
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @PasswordSalt VARBINARY(16) = CRYPT_GEN_RANDOM(16)
		DECLARE @PasswordHash VARBINARY(32) = [dbo].[udfHashSalt](@UserPassword, @PasswordSalt)

		INSERT INTO [Authentication].[User] (Login, PasswordHash, PasswordSalt, UserRoleID) VALUES
		(@UserLogin, @PasswordHash, @PasswordSalt, @UserRoleID)
	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK TRANSACTION
		END
        EXECUTE [Audit].[uspLogError];
	END CATCH
END

GO
CREATE PROCEDURE [Authentication].[uspUpdatePasswordUser]
(
	@UserLogin nvarchar(32),
	@UserPassword nvarchar(32)
)
AS
BEGIN
	-- Процедура для обновления пароля пользователя
	SET NOCOUNT ON;

	BEGIN TRY

		IF NOT EXISTS(SELECT 1 FROM [Authorization].[User] WHERE [Login] = @UserLogin)
		BEGIN
			DECLARE @Error nvarchar(256) =  'Пользователь с логином '+@UserLogin+' отсутствует!'
			RAISERROR(@Error,19,1)
		END

		DECLARE @PasswordSalt VARBINARY(16) = CRYPT_GEN_RANDOM(16)
		DECLARE @PasswordHash VARBINARY(32) = [dbo].[udfHashSalt](@UserPassword, @PasswordSalt)

		UPDATE [Authorization].[User] SET PasswordSalt = @PasswordSalt, PasswordHash = @PasswordHash
		WHERE [Login] = @UserLogin

	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK TRANSACTION
		END
        EXECUTE [Audit].[uspLogError];
	END CATCH
END

GO
CREATE PROCEDURE [Authorization].[uspCreateSession]
(
	@UserID int,
	@ExpiredAt datetime,
	@SessionKey nvarchar(128) OUTPUT
)
AS
BEGIN
	-- Процедура для создания сессии пользователя
	SET NOCOUNT ON;

	BEGIN TRY
		DECLARE @Error nvarchar(256)

		IF NOT EXISTS(SELECT TOP 1 1 FROM [Authentication].[User] WHERE [UserID] = 1 AND [SysIsDeleted] = 0)
		BEGIN
			SET @Error =  'Пользователя с идентификатором '+@UserID+' не существует или удален!'
			RAISERROR(@Error,19,1)
		END

		DECLARE @CreatedAt DATETIME = GETDATE()
		IF (@CreatedAt > @ExpiredAt)
		BEGIN
			SET @Error = CONCAT('Нельзя создать сессию, чьё время истекания меньше текущего:[CreatedAt] ',@CreatedAt,' > [ExpiredAt] ',@ExpiredAt)
			RAISERROR(@Error,18,2)
		END

		DECLARE @LogAuthenticationID INT = (
			SELECT TOP 1 [LogAuthenticationID]
			FROM [Audit].[LogAuthentication]
			WHERE [UserIDFoundByEnteredLogin] = @UserID AND
			([FirstFactorResult] = 1 AND [SecondFactorResult] = 1)
			ORDER BY [LogAuthenticationID] DESC )

		INSERT INTO [Authorization].[UserSession]([UserID], [CreatedAt], [ExpiredAt], [LogAuthenticationID]) VALUES
		(@UserID, @CreatedAt, @ExpiredAt, @LogAuthenticationID)

		SELECT @SessionKey = [SessionKey] FROM [Authorization].[UserSession] WHERE [UserSessionID] = @@IDENTITY
	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK TRANSACTION
		END
        EXECUTE [Audit].[uspLogError];
	END CATCH
END

GO
CREATE PROCEDURE [Audit].[uspAddLog]
(@TableName nvarchar(256),
@SchemaName nvarchar(256),
@TableRowID INT,
@Action CHAR(1),
@Date DATETIME,
@GUID UNIQUEIDENTIFIER)
AS
BEGIN
	-- Процедура для добавления лога, будет внедрена в триггеры
	SET NOCOUNT ON

	BEGIN TRY

		DECLARE @TableDataID INT

		SELECT @TableDataID = TableDataID FROM [Audit].[TableData]
		WHERE [Name] = @TableName AND [SchemaName] = @SchemaName

		IF(@TableDataID IS NULL)
		BEGIN
			RAISERROR('В процедуру были переданы имя таблицы и имя схемы несуществующие в Audit.TableData',19,1)
		END

		DECLARE @UserSessionID INT

		EXEC [Authorization].[uspGetCurrentUserSessionID] @UserSessionID OUTPUT

		EXEC [Authorization].[uspPrintWarningIfCurrentUserSessionNotSet]

		INSERT INTO [Audit].[Log] VALUES
		(@TableDataID, @TableRowID, @Action, @UserSessionID, @Date, @GUID)
	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK TRANSACTION
		END
        EXECUTE [Audit].[uspLogError];
	END CATCH
END

GO
CREATE PROCEDURE [Audit].[uspAddLogChanges]
(@GUID UNIQUEIDENTIFIER,
@JsonOld NVARCHAR(MAX),
@JsonNew NVARCHAR(MAX))
AS
BEGIN
	-- Процедура для добавления изменений в записи в таблице Audit.LogChanges
	SET nocount ON

	BEGIN TRY
		DECLARE @LogID INT
		DECLARE @TableDataID INT

		SELECT @LogID = [LogID], @TableDataID = [TableDataID] FROM [Audit].[Log]
		WHERE [GUID] = @GUID

		IF(@LogID IS NULL)
			RETURN

		SELECT @JsonOld = value
		FROM OPENJSON(@JsonOld)

		SELECT @JsonNew = value
		FROM OPENJSON(@JsonNew)
	uspAddTableColumnData
		-- Вставка в изменения логов все несовпадения по значением в столбцах
		INSERT INTO [Audit].[LogChange]
			SELECT @LogID, [tcd].[TableColumnDataID], [js_old].[value], [js_new].[value]
			FROM OPENJSON(@JsonOld) as [js_old] INNER JOIN
				 OPENJSON(@JsonNew) as [js_new] ON [js_new].[key] = [js_old].[key] LEFT JOIN
				 [Audit].[TableColumnData] as [tcd] ON [tcd].[Name] = [js_old].[key] COLLATE Cyrillic_General_CI_AS AND
				 [tcd].[TableDataID] = @TableDataID 
			WHERE ISNULL([js_new].[value],'') <> ISNULL([js_old].[value], '')
	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK TRANSACTION
		END
        EXECUTE [Audit].[uspLogError];
	END CATCH
END

GO

-- ПРИЧИНА КОММЕНТИРОВАНИЯ: УБРАНА ИЗ УДАЛЕНИЯ ТАБЛИЦЫ Login
-- Функиця для проверки валидности пользователя
--CREATE FUNCTION [Authorization].[sp_IsValidUser]
--(@LoginID INT)
--RETURNS BIT
--AS
--BEGIN
--	-- Функция проверяющая является ли текущий пользователь валидным
--	-- Проверяется наличие двух и более пользователей имеющих один и тот же логин и статус удаления 0
--	DECLARE @UsersCountWithSearchLogin INT

--	SELECT @UsersCountWithSearchLogin = COUNT(*) FROM [Authorization].[User]
--	WHERE [LoginID] = @LoginID AND [SysIsDeleted] = 0
	
--	IF(@UsersCountWithSearchLogin > 1)
--		RETURN 0
--	RETURN 1
--END
--GO

---- Добавляем пользователю проверку 
--ALTER TABLE [Authorization].[User] ADD CONSTRAINT CK_User_LoginID CHECK ([Authorization].[sp_IsValidUser]([LoginID]) = 1)

-- ***********************************************************************
-- Шаблон для создания триггера заместо обновления
-- Добавляет изменения записи в таблицу Audit.LogChanges
-- Создается для таблиц всех схем, кроме Audit и dbo
-- ***********************************************************************
DECLARE @Template NVARCHAR(MAX) = 'CREATE TRIGGER [#SCHEMA].[#TABLE_InsteadOfUpdate]#CRLF\
ON [#SCHEMA].[#TABLE] INSTEAD OF UPDATE AS #CRLF\
BEGIN #CRLF\
	SET NOCOUNT ON#CRLF\
	DECLARE @RowID INT = (SELECT TOP 1 #TABLEID from inserted ORDER BY #TABLEID); #CRLF\
	DECLARE @GUID UNIQUEIDENTIFIER = NEWID();#CRLF\
	DECLARE @Date DATETIME = GETDATE();#CRLF\
	DECLARE @JsonOld NVARCHAR(MAX), @JsonNew NVARCHAR(MAX);#CRLF\
	DECLARE @trancount int = 0;#CRLF\
	DECLARE @SysIsDeletedOld BIT, @SysIsDeletedNew BIT;#CRLF\
#CRLF\
	BEGIN TRY#CRLF\
	IF @@TRANCOUNT = 0#CRLF\
	BEGIN#CRLF\
		SET @trancount = 1#CRLF\
		BEGIN TRANSACTION#CRLF\
	END#CRLF\
	SET IDENTITY_INSERT [#SCHEMA].[#TABLE] ON#CRLF\
#CRLF\
	WHILE @RowID IS NOT NULL#CRLF\
	BEGIN#CRLF\
#CRLF\
		SELECT @SysIsDeletedOld = [SysIsDeleted] FROM [#SCHEMA].[#TABLE] WHERE [#TABLEID] = @RowID#CRLF\
		SELECT @SysIsDeletedNew = [SysIsDeleted] FROM inserted WHERE [#TABLEID] = @RowID#CRLF\
		#CRLF\
		IF @SysIsDeletedNew = 0 AND @SysIsDeletedOld = 1#CRLF\
		BEGIN#CRLF\
			EXEC [Audit].[uspAddLog] ''#TABLE'', ''#SCHEMA'', @RowID, ''R'', @Date, @GUID;#CRLF\
			UPDATE [#SCHEMA].[#TABLE] SET [SysIsDeleted] = 0 WHERE [#TABLEID] = @RowID#CRLF\
			GOTO NEXT_ROWID#CRLF\
		END#CRLF\
		EXEC [Audit].[uspAddLog] ''#TABLE'', ''#SCHEMA'', @RowID, ''U'', @Date, @GUID;#CRLF\
		#CRLF\
		SET @JsonOld = (SELECT * FROM [#SCHEMA].[#TABLE] WHERE [#TABLEID] = @RowID FOR JSON PATH, INCLUDE_NULL_VALUES);#CRLF\
		SET @JsonNew = (SELECT * FROM inserted WHERE [#TABLEID] = @RowID ORDER BY #TABLEID FOR JSON PATH, INCLUDE_NULL_VALUES);#CRLF\
		#CRLF\
		EXEC [Audit].uspAddLogChanges @GUID,  @JsonOld, @JsonNew;#CRLF\
#CRLF\
		#UPDATE#CRLF\
#CRLF\
		NEXT_ROWID:#CRLF\
			UPDATE [#SCHEMA].[#TABLE] SET [SysModifiedDate] = @Date WHERE [#TABLEID] = @RowID;#CRLF\
			SELECT TOP 1 @RowID = [#TABLEID] from inserted WHERE [#TABLEID] > @RowID ORDER BY [#TABLEID]#CRLF\
			IF @@ROWCOUNT = 0#CRLF\
			BREAK#CRLF\
	END#CRLF\
#CRLF\
	SET IDENTITY_INSERT [#SCHEMA].[#TABLE] OFF#CRLF\
	IF @trancount = 1#CRLF\
	COMMIT TRANSACTION#CRLF\
	END TRY#CRLF\
	BEGIN CATCH#CRLF\
		IF @@TRANCOUNT > 0#CRLF\
        BEGIN#CRLF\
            ROLLBACK TRANSACTION;#CRLF\
        END#CRLF\
		#CRLF\
        EXECUTE [Audit].[uspLogError];#CRLF\
	END CATCH#CRLF\
END'

DECLARE @PrintTemplate NVARCHAR(MAX) = 'PRINT ''Сделан триггер [#SCHEMA].[#TABLE_InsteadOfUpdate]'''

EXEC [Audit].uspDeleteMeAfterUsing @Template, @PrintTemplate

GO

-- ***********************************************************************
-- Шаблон для создания триггера после вставки
-- Добавляет лог о новой записи в таблицу Audit.Log
-- ***********************************************************************
DECLARE @Template NVARCHAR(MAX) = 'CREATE TRIGGER [#SCHEMA].[#TABLE_AfterInsert]#CRLF\
ON [#SCHEMA].[#TABLE] AFTER INSERT AS #CRLF\
BEGIN#CRLF\
	SET NOCOUNT ON#CRLF\
	DECLARE @trancount int = 0#CRLF\
	BEGIN TRY#CRLF\
	IF @@TRANCOUNT = 0
	BEGIN#CRLF\
		SET @trancount = 1;#CRLF\
		BEGIN TRANSACTION;#CRLF\
	END;#CRLF\
	DISABLE TRIGGER [#SCHEMA].[#TABLE_InsteadOfUpdate] ON [#SCHEMA].[#TABLE];#CRLF\
	DECLARE @RowID INT = (SELECT TOP 1 [#TABLEID] from inserted ORDER BY [#TABLEID]);#CRLF\
	DECLARE @GUID UNIQUEIDENTIFIER = NEWID();#CRLF\
	DECLARE @Date DATETIME = GETDATE();#CRLF\
#CRLF\
	WHILE @RowID IS NOT NULL#CRLF\
	BEGIN#CRLF\
		UPDATE [#SCHEMA].[#TABLE] SET [SysModifiedDate] = @Date WHERE [#TABLEID] = @RowID;#CRLF\
		EXEC [Audit].[uspAddLog] ''#TABLE'', ''#SCHEMA'', @RowID, ''C'', @Date, @GUID;#CRLF\
#CRLF\
		SELECT TOP 1 @RowID = [#TABLEID] from inserted WHERE [#TABLEID] > @RowID ORDER BY [#TABLEID]#CRLF\
		IF @@ROWCOUNT = 0#CRLF\
		BREAK#CRLF\
#CRLF\
	END;#CRLF\
	ENABLE TRIGGER [#SCHEMA].[#TABLE_InsteadOfUpdate] ON [#SCHEMA].[#TABLE];#CRLF\
	IF @trancount = 1#CRLF\
	COMMIT TRANSACTION#CRLF\
	END TRY#CRLF\
	BEGIN CATCH#CRLF\
	IF @@TRANCOUNT > 0#CRLF\
        BEGIN#CRLF\
            ROLLBACK TRANSACTION;#CRLF\
        END#CRLF\
		#CRLF\
        EXECUTE [Audit].[uspLogError];#CRLF\
	END CATCH#CRLF\
END'

DECLARE @PrintTemplate NVARCHAR(MAX) = 'PRINT ''Сделан триггер [#SCHEMA].[#TABLE_AfterInsert]'''

EXEC [Audit].uspDeleteMeAfterUsing @Template, @PrintTemplate

GO

-- ***********************************************************************
-- Шаблон для создания триггера вместо удаления
-- Добавляет лог об удалении
-- ***********************************************************************
DECLARE @Template NVARCHAR(MAX) = 'CREATE TRIGGER [#SCHEMA].[#TABLE_InsteadOfDelete]#CRLF\
ON [#SCHEMA].[#TABLE]#CRLF\
INSTEAD OF DELETE AS #CRLF\
BEGIN#CRLF\
	SET NOCOUNT ON;#CRLF\
	DECLARE @trancount int = 0#CRLF\
	BEGIN TRY#CRLF\
	IF @@TRANCOUNT = 0#CRLF\
	BEGIN#CRLF\
		SET @trancount = 1
		BEGIN TRANSACTION;#CRLF\
	END;#CRLF\
#CRLF\
	DISABLE TRIGGER [#SCHEMA].[#TABLE_InsteadOfUpdate] ON [#SCHEMA].[#TABLE];#CRLF\
#CRLF\
	DECLARE @RowID INT = (SELECT TOP 1 [#TABLEID] from deleted ORDER BY [#TABLEID]);#CRLF\
	DECLARE @GUID UNIQUEIDENTIFIER = NEWID();#CRLF\
	DECLARE @Date DATETIME = GETDATE();#CRLF\
#CRLF\
	WHILE @RowID IS NOT NULL#CRLF\
	BEGIN#CRLF\
		IF EXISTS(SELECT TOP 1 1 FROM [#SCHEMA].[#TABLE] WHERE [#TABLEID] = @RowID and [SysIsDeleted] = 1)#CRLF\
		BEGIN#CRLF\
			RAISERROR(''Record #TABLEID %d in [#SCHEMA].[#TABLE] already deleted'',19,1,@RowID)#CRLF\
		END#CRLF\
		ELSE#CRLF\
		BEGIN#CRLF\
			UPDATE [#SCHEMA].[#TABLE] SET [SysModifiedDate] = @Date, [SysIsDeleted] = 1 WHERE [#TABLEID] = @RowID;#CRLF\
			EXEC [Audit].[uspAddLog] ''#TABLE'', ''#SCHEMA'', @RowID, ''D'', @Date, @GUID;#CRLF\
		END#CRLF\
#CRLF\
		SELECT TOP 1 @RowID = [#TABLEID] from deleted WHERE [#TABLEID] > @RowID ORDER BY [#TABLEID]#CRLF\
		IF @@ROWCOUNT = 0#CRLF\
			BREAK#CRLF\
	END;#CRLF\
#CRLF\
	ENABLE TRIGGER [#SCHEMA].[#TABLE_InsteadOfUpdate] ON [#SCHEMA].[#TABLE];#CRLF\
#CRLF\
	IF @trancount = 1#CRLF\
	COMMIT TRANSACTION#CRLF\
	END TRY#CRLF\
	BEGIN CATCH#CRLF\
		IF @@TRANCOUNT > 0#CRLF\
        BEGIN#CRLF\
            ROLLBACK TRANSACTION;#CRLF\
        END#CRLF\
		#CRLF\
        EXECUTE [Audit].[uspLogError];#CRLF\
	END CATCH#CRLF\
END'
DECLARE @PrintTemplate NVARCHAR(MAX) = 'PRINT '' Сделан триггер [#SCHEMA].[#TABLE_AfterInsert]'''

EXEC [Audit].uspDeleteMeAfterUsing @Template, @PrintTemplate

GO

CREATE PROCEDURE [Audit].[uspAddTableColumnData]
(@SchemaName NVARCHAR(256),
@TableName NVARCHAR(256),
@ColumnName NVARCHAR(256),
@ColumnRusName NVARCHAR(256))
AS
BEGIN
	-- Процедура для удобного добавления записи в таблицу TableColumnData по имени таблицы и схемы
	SET NOCOUNT ON
	BEGIN TRY

	DECLARE @TableDataID INT
	DECLARE @Error NVARCHAR(MAX)

	SELECT @TableDataID = [TableDataID]
	FROM [Audit].[TableData]
	WHERE 
		[Name] = @TableName AND
		[SchemaName] = @SchemaName

	IF(@TableDataID IS NULL)
	BEGIN
		SET @Error = CONCAT(@SchemaName,'.',@TableName, ' не существует в таблице [Audit].[TableData]')
		RAISERROR(@Error, 19, 1)
	END

	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @SchemaName AND COLUMN_NAME = @ColumnName)
	BEGIN
		SET @Error = CONCAT(@ColumnName, ' не существует в таблице ', @SchemaName, '.', @TableName)
		RAISERROR(@Error, 19, 2)
	END

	INSERT INTO [Audit].[TableColumnData] VALUES (@TableDataID, @ColumnName, @ColumnRusName)
	END TRY

	BEGIN CATCH
		IF @@TRANCOUNT > 0
		BEGIN
			ROLLBACK TRANSACTION
		END
        EXECUTE [Audit].[uspLogError];
	END CATCH
END

GO

-- ***********************************************************************
-- Тут начинаем добавлять информацию о столбцах и таблицах
-- для красивого вывода в аудите
-- ***********************************************************************

-- Current Table Name
DECLARE @ctn NVARCHAR(256) 
-- Current Schema Name
DECLARE @csn NVARCHAR(256)

SET @csn = 'dbo'
SET @ctn = 'File'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Файл')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'FileID', 'Идентификатор файла'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'GUID', 'GUID'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Data', 'Содержимое'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название файла'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ContentType', 'Тип контента'

SET @csn = 'Person'
SET @ctn = 'Address'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Адрес')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'AddressID', 'Идентификатор адреса'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Country', 'Страна'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Region', 'Регион'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'City', 'Город'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'District', 'Район'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Street', 'Улица'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BuildingNumber', 'Номер здания'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Corpus', 'Корпус'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Floor', 'Этаж'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Flat', 'Квартира/Офис'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'PostalIndex', 'Почтовый индекс'

SET @csn = 'HumanResources'
SET @ctn = 'BusinessEntityType'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Тип бизнес-сущности')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityTypeID', 'Идентификатор типа бизнес-сущности'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название типа'

SET @csn = 'HumanResources'
SET @ctn = 'BusinessEntity'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Бизнес-сущность')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityID', 'Идентификатор бизнес-сущности'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityTypeID', 'Идентификатор типа бизнес-сущности'

SET @csn = 'HumanResources'
SET @ctn = 'Organization'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Организация')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityID', 'Идентификатор бизнес-сущности'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', 'Идентификатор организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'FullName', 'Полное наименование организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ShortName', 'Сокращенное наименование организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessAddressID', 'Юридический адрес/Адрес деятельности'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'INN', 'ИНН'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KPP', 'КПП'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OKPO', 'ОКПО'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OGRN', 'ОГРН'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateOfAssignmentOGRN', 'Дата присовения ОГРН'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DirectorFullName', 'ФИО директора'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ChiefAccountantFullName', 'ФИО Гл.Бухгалтер'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OKVED', 'Основной код ОКВЭД'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'IsOwnerJournalAccountingCPI', 'Статус владельца журнала учёта СКЗИ'

SET @csn = 'Person'
SET @ctn = 'ContactType'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Тип контакта')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ContactTypeID', 'Идентификатор типа контакта'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название типа контакта'

SET @csn = 'HumanResources'
SET @ctn = 'OrganizationContact'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Контакты организации')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationContactID', 'Идентификатор контакта организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', 'Идентификатор организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ContactTypeID', 'Идентификатор типа контакта'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Value', 'Значение контакта'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', 'Заметка'

SET @csn = 'Authorization'
SET @ctn = 'UserRole'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Роль пользователя')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserRoleID', 'Идентификатор роли'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название роли'

SET @csn = 'Authorization'
SET @ctn = 'RightGroup'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Группа прав')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RightGroupID', 'Идентификатор группы прав'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название группы'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Code', 'Код группы'

SET @csn = 'Authorization'
SET @ctn = 'Right'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Право')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RightID', 'Идентификатор права'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RightGroupID', 'Идентификатор группы прав'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название права'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Code', 'Код права'

SET @csn = 'Authorization'
SET @ctn = 'UserRoleRight'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Право роли пользователя')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserRoleRightID', 'Идентификатор права роли пользователя'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserRoleID', 'Идентификатор роли пользователи'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RightID', 'Идентификатор права'

--SET @csn = 'Authorization'
--SET @ctn = 'Login'
--INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Логин')
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'LoginID', 'Идентификатор логина'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название логина'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Password', 'Пароль логина'

SET @csn = 'Authentication'
SET @ctn = 'User'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Пользователь')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserID', 'Идентификатор пользователя'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Login', 'Логин'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserRoleID', 'Идентификатор роли пользователя'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'PasswordHash', 'Хэш пользователя'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'PasswordSalt', 'Соль питерская'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'LastLoginDate', 'Последняя дата входа'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'TotpSecretKey', 'Зашифрованный секретный ключ для TOTP пароля'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'TwoFactorEnabled', 'Статус двухфакторной аутенфикации'

--SET @csn = 'Authentication'
--SET @ctn = 'ResultType'
--INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Тип результата аутенфикации')
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ResultTypeID', 'Идентификатор типа результата аутенфикации'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название типа'

SET @csn = 'HumanResources'
SET @ctn = 'EmployeePosition'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Должность сотрудника')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'EmployeePositionID', 'Идентификатор должности сотрудника'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название должности'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', 'Заметка'

SET @csn = 'HumanResources'
SET @ctn = 'Employee'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Сотрудник')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityID', 'Идентификатор бизнес-сущности'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'EmployeeID', 'Идентификатор сотрудника'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'FirstName', 'Первая часть имени'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'MiddleName', 'Вторая часть имени'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'LastName', 'Последняя часть имени'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'EmployeePositionID', 'Идентификатор должности сотрудника'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', 'Идентификатор организации'

--SET @csn = 'AccountingCPI'
--SET @ctn = 'Subject'
--INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Субъект журнала учета СКЗИ')
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SubjectID', 'Идентификатор субъекта'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', 'Идентификатор организации'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'EmployeeID', 'Идентификатор сотрудника'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCPARecord'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Запись журнал поэкземплярного учета СКЗИ для органа криптографической защиты')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', 'Идентификатор организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCPARecordID', 'Идентификатор записи в журнале поэкземплярного учета СКЗИ для органа криптографической защиты'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'NameCPI', 'Наименование СКЗИ, эксплуатационной и технической документации к ним, ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialCPI', 'Серийный номер СКЗИ, эксплуатационной и технической документации к ним, номера серий ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstanceNumber', 'Номер экземпляра (криптографический номер) ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ReceivedFromID', 'Субъект от которого получены СКЗИ'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberCoverLetterReceive', 'Дата и номер сопроводительного письма или дата изготовления ключевых документов и расписка в изготовлении в отметке о получении'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberCoverLetterSend', 'Дата и номер сопроводительного письма в отметке о рассылке'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberConfirmationSend', 'Дата и номер подтверждения или расписка в получении в отметке о рассылке'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberCoverLetterReturn', 'Дата и номер подтверждения или расписка в получении в отметке о возврате'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberConfirmationReturn', 'Дата и номер подтверждения в отметке о возврате'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'CommissioningDate', 'Дата ввода в действие'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DecommissioningDate', 'Дата уничтожения'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionDate', 'Дата вывода из действия'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionActNumber', 'Номер акта или расписка об уничтожении'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', 'Примечание'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', 'Идентификатор файла подписи'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceCPAReceiver'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Субъекты журнала поэкземплярного учета СКЗИ для органа криптографической защиты которым была разослана информация')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceCPAReceiverID', 'Идентификатор cубъекты журнала поэкземплярного учета СКЗИ для органа криптографической защиты которому была разослана информация'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordID', 'Идентификатор записи журнала поэкземплярного учета СКЗИ для органа криптографической защиты'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ReceiverID', 'Идентификатор субъекта журнала учета СКЗИ'

SET @csn = 'Office'
SET @ctn = 'Hardware'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Аппаратура')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', 'Идентификатор организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'HardwareID', 'Идентификатор аппаратуры'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialNumber', 'Серийномер материнской платы или пломбы'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', 'Заметка'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHRecord'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Запись журнала поэкземплярного учета СКЗИ для обладателя конфиденциальной информации')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', 'Идентификатор организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCIHRecordID', 'Идентификатор записи журнала поэкземплярного учета СКЗИ для обладателя конфиденциальной информации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'NameCPI', 'Наименование СКЗИ, эксплуатационной и технической документации к ним, ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialCPI', 'Серийный номер СКЗИ, эксплуатационной и технической документации к ним, номера серий ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstanceNumber', 'Номер экземпляра (криптографический номер) ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ReceivedFromID', 'Субъект от которого получены СКЗИ'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberCoverLetterReceive', 'Дата и номер сопроводительного письма или дата изготовления ключевых документов и расписка в изготовлении в отметке о получении'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'CPIUserID', 'Идентификатор субъекта (пользователя) получившего СКЗИ в отметке о выдаче'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberConfirmationIssue', 'Дата и расписка в получении в отметке о выдаче'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstallationDateAndConfirmation', 'Дата подключения (установки) и подписи лиц, произведших подключение (установку) в отметке о подключении (установке СКЗИ)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionDate', 'Дата изъятия (уничтожения) в отметка об изъятии СКЗИ из аппаратных средств, уничтожении ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionActNumber', 'Номер акта или расписка об уничтожении в отметка об изъятии СКЗИ из аппаратных средств, уничтожении ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', 'Примечание'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', 'Идентификатор файла подписи'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHInstaller'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ, произведших подключение (установку)')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCIHInstallerID', 'Идентификатор'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordID', 'Идентификатор записи журнала поэкземплярного учета СКЗИ для обладателя конфиденциальной информации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstallerID', 'Идентификатор субъекта журнала учета СКЗИ производившего установку СКЗИ'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHConnectedHardware'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Номера аппаратных средств, в которые установлены или к которым подключены СКЗИ')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCIHConnectedHardwareID', 'Идентификатор'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordID', 'Идентификатор записи журнала поэкземплярного учета СКЗИ для обладателя конфиденциальной информации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'HardwareID', 'Идентификатор аппарата в который было установлено или подключено СКЗИ'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHDestructor'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ, производивших изъятие (уничтожение)')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCIHDestructorID', 'Идентификатор'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordID', 'Идентификатор записи журнала поэкземплярного учета СКЗИ для обладателя конфиденциальной информации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructorID', 'Идентификатор субъекта журнала учета СКЗИ производивший изъятие (уничтожение) СКЗИ'

SET @csn = 'AccountingCPI'
SET @ctn = 'KeyDocumentType'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Тип ключевого документа')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KeyDocumentTypeID', 'Идентификатор типа ключевого документа'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', 'Заметка'

SET @csn = 'AccountingCPI'
SET @ctn = 'KeyHolderType'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Тип ключевого носителя')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KeyHolderTypeID', 'Идентификатор типа ключевого носителя'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', 'Название'

SET @csn = 'AccountingCPI'
SET @ctn = 'KeyHolder'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Ключевой носитель')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KeyHolderID', 'Идентификатор ключевого носителя'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialNumber', 'Серийный номер ключевого носителя'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'TypeID', 'Идентификатор типа ключевого носителя'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserCPI', 'Пользователь СКЗИ'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', 'Идентификатор файла подписи'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalTechnicalRecord'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Запись журнала технический (аппаратный)')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', 'Идентификатор организации'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalTechnicalRecordID', 'Идентификатор записи журнала технический (аппаратный)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted', 'Номер разового ключевого носителя или зоны СКЗИ, в которую введены криптоключи'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Date', 'Дата'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'TypeAndSerialUsedCPI', 'Тип и серийные номера используемых СКЗИ'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordOnMaintenanceCPI', 'Записи по обслуживанию СКЗИ'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KeyDocumentTypeID', 'Идентификатор типа ключевого документа'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialCPIAndKeyDocumentInstanceNumber', 'Серийный, криптографический номер и номер экземпляра ключевого документа'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionDate', 'Дата уничтожения (стирании)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ActNumber', 'Номер акта об уничтожении (стирании)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', 'Примечание'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', 'Идентификатор файла подписи'

GO

-- ***********************************************************************
-- Проверка того, что мы добавили все таблицы
-- Если что-то забыли, то мы откатываем транзакцию и выводим информацию о таблице
-- которые надо ещё добавить
-- ***********************************************************************

DECLARE @MyCursor CURSOR
DECLARE @TableName NVARCHAR(MAX)
DECLARE @SchemaName NVARCHAR(MAX)

DECLARE @Error BIT = 0

SET @MyCursor = CURSOR FOR
	SELECT isc.TABLE_SCHEMA, isc.TABLE_NAME FROM INFORMATION_SCHEMA.TABLES as isc
	WHERE isc.TABLE_SCHEMA NOT IN ('Audit', 'dbo') AND
	isc.TABLE_NAME NOT IN ('UserSession') AND
		NOT EXISTS (SELECT TOP 1 1 FROM [Audit].[TableData] as tcd
						WHERE tcd.Name = isc.TABLE_NAME)

OPEN @MyCursor
FETCH NEXT FROM @MyCursor 
		INTO @SchemaName, @TableName

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @Error = 1 
	PRINT CONCAT('[ОШИБКА] Таблица ', @SchemaName, '.', @TableName, ' отсутствует в таблице [Audit].[TableData]')

	FETCH NEXT FROM @MyCursor 
		INTO @SchemaName, @TableName
END

IF @Error = 1 AND @@TRANCOUNT > 0
BEGIN
	ROLLBACK TRANSACTION CreateTables
END

CLOSE @MyCursor
DEALLOCATE @MyCursor

IF @Error = 1 AND @@TRANCOUNT > 0
RETURN

GO

-- ***********************************************************************
-- Проверка того, что мы добавили все столбцы для имеющихся таблиц
-- Если что-то забыли, то мы откатываем транзакцию и выводим информацию о столбце
-- который надо ещё добавить
-- ***********************************************************************

DECLARE @MyCursor CURSOR
DECLARE @TableName NVARCHAR(MAX)
DECLARE @SchemaName NVARCHAR(MAX)
DECLARE @ColumnName NVARCHAR(MAX)

DECLARE @Error BIT = 0

SET @MyCursor = CURSOR FOR
	SELECT isc.TABLE_SCHEMA, isc.TABLE_NAME, isc.COLUMN_NAME FROM [Audit].[TableData] as td
	INNER JOIN INFORMATION_SCHEMA.COLUMNS as isc ON isc.TABLE_SCHEMA = td.SchemaName AND isc.TABLE_NAME = td.NAME
	WHERE isc.COLUMN_NAME NOT IN ('SysIsDeleted', 'SysModifiedDate') AND
		isc.TABLE_NAME NOT IN ('UserSession') AND
		NOT EXISTS (SELECT 1 FROM [Audit].[TableColumnData] as tcd
						WHERE tcd.TableDataID = td.TableDataID AND
						tcd.Name = isc.COLUMN_NAME)

OPEN @MyCursor
FETCH NEXT FROM @MyCursor 
		INTO @SchemaName, @TableName, @ColumnName

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @Error = 1 
	PRINT CONCAT('[ОШИБКА] Для таблицы ', @SchemaName, '.', @TableName, ' отсутствует столбец ', @ColumnName, ' в таблице [Audit].[TableColumnData]')

	FETCH NEXT FROM @MyCursor 
		INTO @SchemaName, @TableName, @ColumnName
END

IF @Error = 1 AND @@TRANCOUNT > 0
BEGIN
	ROLLBACK TRANSACTION CreateTables
END

CLOSE @MyCursor
DEALLOCATE @MyCursor

DROP PROCEDURE [Audit].[uspDeleteMeAfterUsing]

IF @Error = 1 AND @@TRANCOUNT > 0
RETURN


COMMIT TRANSACTION CreateTables