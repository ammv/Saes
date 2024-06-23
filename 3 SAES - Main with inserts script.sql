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
--
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

INSERT INTO [File]([Name], [ContentType], [Data]) VALUES
('Тест', 'txt', NULL),
('Годовой отчёт', 'docx', NULL),
('Среднесуточная стоимость моего завтра', 'xlsx', NULL),
('Изображение нашего сотрудника', 'png', NULL)

-- Название: Роль пользователя
CREATE TABLE [Authorization].[UserRole]
(
	[UserRoleID] INT IDENTITY,
	[Name] nvarchar(64) NOT NULL,
	
	CONSTRAINT PK_UserRole_UserRoleId PRIMARY KEY([UserRoleID]),
	CONSTRAINT UQ_UserRole_Name UNIQUE([Name])
)

INSERT INTO [Authorization].[UserRole] (Name) VALUES ('admin');
INSERT INTO [Authorization].[UserRole] (Name) VALUES ('director');
INSERT INTO [Authorization].[UserRole] (Name) VALUES ('head_employee');
INSERT INTO [Authorization].[UserRole] (Name) VALUES ('employee');

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

DECLARE @RightGroupId INT

-- /////////////// AUDIT ///////////////////
INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Логи', 'log');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр логов','log_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт логов','log_export');

SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Логи ошибок', 'log_error');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр логов ошибок','log_error_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт логов ошибок','log_error_export');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Логи аутенфикаций', 'log_auth');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр логов аутенфикации','log_auth_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт логов аутенфикации','log_auth_export');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Логи изменений', 'log_change');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр логов аутенфикации','log_change_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт логов аутенфикации','log_change_export');


INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Данные таблиц', 'table_data');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр данных таблиц','table_data_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт данных таблиц','table_data_export');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Данные столбцов таблиц', 'table_column_data');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр столбцов данных таблиц','table_column_data_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт столбцов данных таблиц','table_column_data_export');

-- /////////////// Аутенфикация ///////////////////

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Пользователи', 'user');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр пользователей','user_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт пользователей','user_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление пользователей','user_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение пользователей','user_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление пользователей','user_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление пользователей','user_recover');

-- /////////////// Авторизация ///////////////////

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Права', 'right');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр прав','right_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт прав','right_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление прав','right_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление прав','right_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Группы прав', 'right_group');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр группы прав','right_group_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт группы прав','right_group_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление группы прав','right_group_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение группы прав','right_group_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление группы прав','right_group_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление группы прав','right_group_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Роли пользователей', 'user_role');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр ролей пользователей','user_role_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт ролей пользователей','user_role_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление ролей пользователей','user_role_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение ролей пользователей','user_role_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление ролей пользователей','user_role_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление ролей пользователей','user_role_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Права ролей пользователей', 'user_role_right');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр прав ролей пользователей','user_role_right_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт прав ролей пользователей','user_role_right_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление прав ролей пользователей','user_role_right_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение прав ролей пользователей','user_role_right_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление прав ролей пользователей','user_role_right_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление прав ролей пользователей','user_role_right_recover');


INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Сессии пользователей', 'user_session');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр сессий пользователей','user_session_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт сессий пользователей','user_session_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение сессий пользователей','user_session_edit');

-- /////////////// Человеческие ресурсы ///////////////////

--INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Бизнес-сущности', 'business_entity');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (13, 'Просмотр бизнес-сущностей','business_entity_see');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (13, 'Экспорт бизнес-сущностей','business_entity_export');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (13, 'Удаление бизнес-сущностей','business_entity_delete');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (13, 'Восстановление бизнес-сущностей','business_entity_recover');

--INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Тип бизнес-сущности', 'business_entity_type');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (14, 'Просмотр типов бизнес-сущностей','business_entity_type_see');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (14, 'Экспорт типов бизнес-сущностей','business_entity_type_export');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (14, 'Удаление типов бизнес-сущностей','business_entity_type_delete');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (14, 'Восстановление типов бизнес-сущностей','business_entity_type_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Сотрудники', 'employee');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр сотрудников','employee_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт сотрудников','employee_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление сотрудников','employee_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение сотрудников','employee_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление сотрудников','employee_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление сотрудников','employee_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Должности сотрудников', 'employee_position');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр должностей сотрудников','employee_position_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт должностей сотрудников','employee_position_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление должностей сотрудников','employee_position_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение должностей сотрудников','employee_position_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление должностей сотрудников','employee_position_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление должностей сотрудников','employee_position_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Организации', 'organization');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр организаций','organization_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт организаций','organization_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление организаций','organization_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение организаций','organization_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление организаций','organization_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление организаций','organization_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Контакты организаций', 'orgnanization_contact');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр контактов организаций','orgnanization_contact_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт контактов организаций','orgnanization_contact_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление контактов организаций','orgnanization_contact_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение контактов организаций','orgnanization_contact_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление контактов организаций','orgnanization_contact_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление контактов организаций','orgnanization_contact_recover');

-- /////////////// Офис ///////////////////

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Аппаратура', 'hardware');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр аппаратуры','hardware_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт аппаратуры','hardware_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление аппаратуры','hardware_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение аппаратуры','hardware_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление аппаратуры','hardware_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление аппаратуры','hardware_recover');

-- /////////////// Личная информация ///////////////////

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Типы контактов', 'contact_type');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр типов контактов','contact_type_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт типов контактов','contact_type_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление типов контактов','contact_type_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение типов контактов','contact_type_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление типов контактов','contact_type_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление типов контактов','contact_type_recover');

-- /////////////// Прочее ///////////////////

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Адреса', 'address');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр адресов','address_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт адресов','address_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление адресов','address_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение адресов','address_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление адресов','address_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление адресов','address_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Файлы', 'file');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр файлов','file_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт файлов','file_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление файлов','file_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение файлов','file_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление файлов','file_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление файлов','file_recover');

-- /////////////// Электронные подписи ///////////////////

-- TEMPLATE
--INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('title', 'code');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (23, 'Просмотр сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','code_see');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (23, 'Экспорт сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','code_export');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (23, 'Добавление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','code_add');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (23, 'Изменение сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','code_edit');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (23, 'Удаление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','code_delete');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (23, 'Восстановление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','code_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Типы ключевых носителей', 'key_holder_type');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр типов ключевых носителей','key_holder_type_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт типов ключевых носителей','key_holder_type_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление типов ключевых носителей','key_holder_type_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение типов ключевых носителей','key_holder_type_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление типов ключевых носителей','key_holder_type_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление типов ключевых носителей','key_holder_type_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Типы ключевых документов', 'key_document_type');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр типов ключевых документов','key_document_type_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт типов ключевых документов','key_document_type_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление типов ключевых документов','key_document_type_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение типов ключевых документов','key_document_type_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление типов ключевых документов','key_document_type_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление типов ключевых документов','key_document_type_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Ключевые носители', 'key_holder');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр ключевых носителей','key_holder_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт ключевых носителей','key_holder_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление ключевых носителей','key_holder_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение ключевых носителей','key_holder_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление ключевых носителей','key_holder_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление ключевых носителей','key_holder_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Записи журнала технического (аппаратного)', 'journal_technical_record');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр записей журнала технического (аппаратного)','journal_technical_record_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт записей журнала технического (аппаратного)','journal_technical_record_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление записей журнала технического (аппаратного)','journal_technical_record_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение записей журнала технического (аппаратного)','journal_technical_record_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление записей журнала технического (аппаратного)','journal_technical_record_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление записей журнала технического (аппаратного)','journal_technical_record_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Записи журнала поэкземплярного учёта СКЗИ для ОКЗ', 'journal_instance_for_cpa_record');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр записей журнала поэкземплярного учёта СКЗИ для ОКЗ','journal_instance_for_cpa_record_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт записей журнала поэкземплярного учёта СКЗИ для ОКЗ','journal_instance_for_cpa_record_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление записей журнала поэкземплярного учёта СКЗИ для ОКЗ','journal_instance_for_cpa_record_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение записей журнала поэкземплярного учёта СКЗИ для ОКЗ','journal_instance_for_cpa_record_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление записей журнала поэкземплярного учёта СКЗИ для ОКЗ','journal_instance_for_cpa_record_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление записей журнала поэкземплярного учёта СКЗИ для ОКЗ','journal_instance_for_cpa_record_recover');

INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Записи журнала поэкземплярного учета СКЗИ для ОКИ', 'journal_instance_for_cih_record');
SET @RightGroupId = SCOPE_IDENTITY()
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Просмотр записей журнала поэкземплярного учета СКЗИ для ОКИ','journal_instance_for_cih_record_see');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Экспорт записей журнала поэкземплярного учета СКЗИ для ОКИ','journal_instance_for_cih_record_export');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Добавление записей журнала поэкземплярного учета СКЗИ для ОКИ','journal_instance_for_cih_record_add');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Изменение записей журнала поэкземплярного учета СКЗИ для ОКИ','journal_instance_for_cih_record_edit');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Удаление записей журнала поэкземплярного учета СКЗИ для ОКИ','journal_instance_for_cih_record_delete');
INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (@RightGroupId, 'Восстановление записей журнала поэкземплярного учета СКЗИ для ОКИ','journal_instance_for_cih_record_recover');

--INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Субъекты журнала поэкземплярного учета СКЗИ для органа криптографической защиты которым была разослана информация', 'journal_instance_cpa_receiver');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (29, 'Просмотр субъектов журнала поэкземплярного учета СКЗИ для ОКЗ которым была разослана информация','journal_instance_cpa_receiver_see');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (29, 'Экспорт субъектов журнала поэкземплярного учета СКЗИ для ОКЗ которым была разослана информация','journal_instance_cpa_receiver_export');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (29, 'Добавление субъектов журнала поэкземплярного учета СКЗИ для ОКЗ которым была разослана информация','journal_instance_cpa_receiver_add');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (29, 'Изменение субъектов журнала поэкземплярного учета СКЗИ для ОКЗ которым была разослана информация','journal_instance_cpa_receiver_edit');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (29, 'Удаление субъектов журнала поэкземплярного учета СКЗИ для ОКЗ которым была разослана информация','journal_instance_cpa_receiver_delete');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (29, 'Восстановление субъектов журнала поэкземплярного учета СКЗИ для ОКЗ которым была разослана информация','journal_instance_cpa_receiver_recover');

--INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Сотрудники ОКЗ, пользователи СКЗИ, производившие подключение (установку)', 'journal_instance_for_cih_installer');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (30, 'Просмотр сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_installer_see');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (30, 'Экспорт сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_installer_export');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (30, 'Добавление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_installer_add');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (30, 'Изменение сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_installer_edit');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (30, 'Удаление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_installer_delete');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (30, 'Восстановление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_installer_recover');

--INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Сотрудники ОКЗ, пользователи СКЗИ, производивших изъятие (уничтожение)', 'journal_instance_for_cih_destructor');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (31, 'Просмотр сотрудников ОКЗ, пользователей СКЗИ, производивших изъятие (уничтожение)','journal_instance_for_cih_destructor_see');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (31, 'Экспорт сотрудников ОКЗ, пользователей СКЗИ, производивших изъятие (уничтожение)','journal_instance_for_cih_destructor_export');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (31, 'Добавление сотрудников ОКЗ, пользователей СКЗИ, производивших изъятие (уничтожение)','journal_instance_for_cih_destructor_add');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (31, 'Изменение сотрудников ОКЗ, пользователей СКЗИ, производивших изъятие (уничтожение)','journal_instance_for_cih_destructor_edit');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (31, 'Удаление сотрудников ОКЗ, пользователей СКЗИ, производивших изъятие (уничтожение)','journal_instance_for_cih_destructor_delete');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (31, 'Восстановление сотрудников ОКЗ, пользователей СКЗИ, производивших изъятие (уничтожение)','journal_instance_for_cih_destructor_recover');

--INSERT INTO [Authorization].[RightGroup] (Name, Code) VALUES ('Cотрудники ОКЗ, пользователи СКЗИ, производивших подключение (установку)', 'journal_instance_for_cih_connected_hardware');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (32, 'Просмотр сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_connected_hardware_see');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (32, 'Экспорт сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_connected_hardware_export');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (32, 'Добавление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_connected_hardware_add');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (32, 'Изменение сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_connected_hardware_edit');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (32, 'Удаление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_connected_hardware_delete');
--INSERT INTO [Authorization].[Right] ([RightGroupID], Name, Code) VALUES (32, 'Восстановление сотрудников ОКЗ, пользователей СКЗИ, производивших подключение (установку)','journal_instance_for_cih_connected_hardware_recover');

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

-- Admin
--INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID) VALUES (1, 1);
--INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID) VALUES (1, 2);
--INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID) VALUES (1, 3);
--INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID) VALUES (1, 4);
--INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID) VALUES (1, 5);

INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID)
SELECT 
    (SELECT UserRoleID FROM [Authorization].[UserRole] WHERE Name = 'admin'), 
    r.RightID
FROM [Authorization].[Right] r
JOIN [Authorization].[RightGroup] rg ON r.RightGroupID = rg.RightGroupID
WHERE rg.Code IN ('log', 'log_error', 'log_auth', 'log_change', 'table_data', 'table_column_data', 'user', 'right', 'right_group', 'user_role', 'user_role_right', 'user_session')
AND NOT EXISTS (
    SELECT 1
    FROM [Authorization].[UserRoleRight] urr
    WHERE urr.UserRoleID = (SELECT UserRoleID FROM [Authorization].[UserRole] WHERE Name = 'admin')
    AND urr.RightID = r.RightID
);

INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID)
SELECT 
    (SELECT UserRoleID FROM [Authorization].[UserRole] WHERE Name = 'director'), 
    r.RightID
FROM [Authorization].[Right] r
JOIN [Authorization].[RightGroup] rg ON r.RightGroupID = rg.RightGroupID
WHERE rg.Code NOT IN ('134')
AND NOT EXISTS (
    SELECT 1
    FROM [Authorization].[UserRoleRight] urr
    WHERE urr.UserRoleID = (SELECT UserRoleID FROM [Authorization].[UserRole] WHERE Name = 'director')
    AND urr.RightID = r.RightID
);

INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID)
SELECT 
    (SELECT UserRoleID FROM [Authorization].[UserRole] WHERE Name = 'employee'), 
    r.RightID
FROM [Authorization].[Right] r
JOIN [Authorization].[RightGroup] rg ON r.RightGroupID = rg.RightGroupID
WHERE rg.Code IN ('hardware', 'contact_type', 'address', 'file', 'key_holder_type', 'key_document_type', 'key_holder', 'journal_technical_record', 'journal_instance_for_cpa_record', 'journal_instance_for_cih_record')
AND NOT EXISTS (
    SELECT 1
    FROM [Authorization].[UserRoleRight] urr
    WHERE urr.UserRoleID = (SELECT UserRoleID FROM [Authorization].[UserRole] WHERE Name = 'employee')
    AND urr.RightID = r.RightID
);

INSERT INTO [Authorization].[UserRoleRight] (UserRoleID, RightID)
SELECT 
    (SELECT UserRoleID FROM [Authorization].[UserRole] WHERE Name = 'head_employee'), 
    r.RightID
FROM [Authorization].[Right] r
JOIN [Authorization].[RightGroup] rg ON r.RightGroupID = rg.RightGroupID
WHERE rg.Code IN ('hardware', 'contact_type', 'address', 'file', 'key_holder_type', 'key_document_type', 'key_holder', 'journal_technical_record', 'journal_instance_for_cpa_record', 'journal_instance_for_cih_record', 'employee',
	'employee_position', 'organization', 'orgnanization_contact')
AND NOT EXISTS (
    SELECT 1
    FROM [Authorization].[UserRoleRight] urr
    WHERE urr.UserRoleID = (SELECT UserRoleID FROM [Authorization].[UserRole] WHERE Name = 'head_employee')
    AND urr.RightID = r.RightID
);

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

-- Вставка 1
INSERT INTO [Person].[Address] ([Country], [Region], [City], [District], [Street], [BuildingNumber], [Corpus], [Floor], [Flat], [PostalIndex])
VALUES ('Россия', 'Московская область', 'Москва', 'Центральный', 'Тверская', '12', '1', '5', '10', '125009');

-- Вставка 2
INSERT INTO [Person].[Address] ([Country], [Region], [City], [District], [Street], [BuildingNumber], [Corpus], [Floor], [Flat], [PostalIndex])
VALUES ('Украина', 'Киевская область', 'Киев', 'Шевченковский', 'Крещатик', '14', null, '3', '5', '01001');

-- Вставка 3
INSERT INTO [Person].[Address] ([Country], [Region], [City], [District], [Street], [BuildingNumber], [Corpus], [Floor], [Flat], [PostalIndex])
VALUES ('Беларусь', 'Минская область', 'Минск', 'Центральный', 'Независимости проспект', '45', '2', '10', '152', '220004');

-- Вставка 4
INSERT INTO [Person].[Address] ([Country], [Region], [City], [District], [Street], [BuildingNumber], [Corpus], [Floor], [Flat], [PostalIndex])
VALUES ('Казахстан', 'город республиканского значения Алматы', 'Алматы', 'Медеуский', 'Абая проспект', '10', null, '7', '85', '050000');

-- Вставка 5
INSERT INTO [Person].[Address] ([Country], [Region], [City], [District], [Street], [BuildingNumber], [Corpus], [Floor], [Flat], [PostalIndex])
VALUES ('США', 'Калифорния', 'Лос-Анджелес', 'Голливуд', 'Бульвар Голливуд', '7021', null, '12', '210', '90028');


-- Название: Тип бизнес-сущности
-- Описание: Представляет тип бизнес-сущности, например организация, сотрудник, контрагент
CREATE TABLE [HumanResources].[BusinessEntityType]
(
	[BusinessEntityTypeID] INT IDENTITY,
	[Name] NVARCHAR(128) NOT NULL,

	CONSTRAINT PK_BusinessEntityType_BusinessEntityTypeID PRIMARY KEY([BusinessEntityTypeID]),
	CONSTRAINT UQ_BusinessEntityType_Name UNIQUE ([Name])
)

INSERT INTO [HumanResources].[BusinessEntityType] ([Name]) VALUES ('Организация');
INSERT INTO [HumanResources].[BusinessEntityType] ([Name]) VALUES ('Сотрудник');
INSERT INTO [HumanResources].[BusinessEntityType] ([Name]) VALUES ('Прочее');

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

INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (1);
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (1);
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (1);
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (1);
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (1);

INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (2);
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (2);
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (2);
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (2);
INSERT INTO [HumanResources].[BusinessEntity] ([BusinessEntityTypeID]) VALUES (2);

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

INSERT INTO [HumanResources].[Organization] ([BusinessEntityID], [FullName], [ShortName], [BusinessAddressID], [INN], [KPP], [OKPO], [OGRN], [DateOfAssignmentOGRN], [DirectorFullName], [ChiefAccountantFullName], [OKVED], [IsOwnerJournalAccountingCPI])
VALUES 
(1, N'ООО "КриптоПро"', N'ООО "КриптоПро"', NULL, N'1234567890', N'123456789', N'12345678', N'1234567890123', '2023-01-15', N'Иванов Иван Иванович', N'Петрова Ольга Петровна', N'12.34.56', 1),
(2, N'ИП "Сидоров"', N'ИП "Сидоров"', NULL, N'0987654321', N'987654321', N'87654321', N'3210987654321', '2022-05-10', N'Сидоров Петр Сергеевич', NULL, N'65.43.21', 1),
(3, N'ООО "КиберПодпись"', N'ООО "КиберПодпись"', NULL, N'5678901234', N'567890123', N'78901234', N'4321098765432', '2021-09-22', N'Кузнецова Анна Ивановна', N'Смирнов Николай Николаевич', N'98.76.54', 0),
(4, N'ПАО "Подсолнух"', N'ПАО "Подсолнух"', NULL, N'9012345678', N'901234567', N'01234567', N'5432109876543', '2020-04-05', N'Васильев Андрей Петрович', N'Морозова Екатерина Сергеевна', N'32.10.98', 1),
(5, N'НКО "Фиалка"', N'НКО "Фиалка"', NULL, N'3456789012', N'345678901', N'45678901', N'6543210987654', '2019-11-18', N'Новикова Мария Александровна', N'Козлов Дмитрий Олегович', N'76.54.32', 0);


-- Название: Тип контакта
CREATE TABLE [Person].[ContactType]
(
	[ContactTypeID] INT IDENTITY, 
	[Name] NVARCHAR(64),
	
	CONSTRAINT PK_ContactType_ContactTypeID PRIMARY KEY([ContactTypeID]),
	CONSTRAINT UQ_ContactType_Name UNIQUE([Name]),
)

-- Insert 1
INSERT INTO [Person].[ContactType] ([Name])
VALUES ('Электронная почта');

-- Insert 2
INSERT INTO [Person].[ContactType] ([Name])
VALUES ('Телефон');

-- Insert 3
INSERT INTO [Person].[ContactType] ([Name])
VALUES ('Мобильный телефон');

-- Insert 4
INSERT INTO [Person].[ContactType] ([Name])
VALUES ('Домашний адрес');

-- Insert 5
INSERT INTO [Person].[ContactType] ([Name])
VALUES ('Рабочий адрес');


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

-- Insert 1
INSERT INTO [HumanResources].[OrganizationContact] ([OrganizationID], [ContactTypeID], [Value], [Note])
VALUES (1, 1, 'info@company1.com', 'Основной адрес электронной почты');

-- Insert 2
INSERT INTO [HumanResources].[OrganizationContact] ([OrganizationID], [ContactTypeID], [Value], [Note])
VALUES (1, 2, '+1-555-123-4567', 'Номер телефона приемной');

-- Insert 3
INSERT INTO [HumanResources].[OrganizationContact] ([OrganizationID], [ContactTypeID], [Value], [Note])
VALUES (2, 3, '+1-555-987-6543', 'Мобильный номер отдела продаж');

-- Insert 4
INSERT INTO [HumanResources].[OrganizationContact] ([OrganizationID], [ContactTypeID], [Value], [Note])
VALUES (3, 4, '123 Main Street, Anytown, CA 91234', 'Физический адрес главного офиса');

-- Insert 5
INSERT INTO [HumanResources].[OrganizationContact] ([OrganizationID], [ContactTypeID], [Value], [Note])
VALUES (4, 5, '456 Oak Avenue, Suite 200, Springfield, IL 62701', 'Адрес филиала');

-- Insert 6
INSERT INTO [HumanResources].[OrganizationContact] ([OrganizationID], [ContactTypeID], [Value], [Note])
VALUES (5, 1, 'support@company5.com', 'Электронная почта службы поддержки');

-- Insert 7
INSERT INTO [HumanResources].[OrganizationContact] ([OrganizationID], [ContactTypeID], [Value], [Note])
VALUES (2, 2, '+1-555-555-5555', 'Дополнительный номер телефона');


-- Название: Должность сотрудника
CREATE TABLE [HumanResources].[EmployeePosition]
(
	[EmployeePositionID] INT IDENTITY,
	[Name] nvarchar(64) NOT NULL,
	[Note] nvarchar(512) NULL,

	CONSTRAINT PK_EmployeePosition_EmployeePositionID PRIMARY KEY([EmployeePositionID]),
	CONSTRAINT UQ_EmployeePosition_Name UNIQUE([Name])
)

-- Вставка 1
INSERT INTO [HumanResources].[EmployeePosition] ([Name], [Note])
VALUES ('Разработчик программного обеспечения', 'Создает и поддерживает программное обеспечение');

-- Вставка 2
INSERT INTO [HumanResources].[EmployeePosition] ([Name], [Note])
VALUES ('Менеджер проекта', 'Управляет проектами и командами');

-- Вставка 3
INSERT INTO [HumanResources].[EmployeePosition] ([Name], [Note])
VALUES ('Бизнес-аналитик', 'Анализирует бизнес-процессы и предлагает решения');

-- Вставка 4
INSERT INTO [HumanResources].[EmployeePosition] ([Name], [Note])
VALUES ('Специалист по тестированию', 'Тестирует программное обеспечение на наличие ошибок');

-- Вставка 5
INSERT INTO [HumanResources].[EmployeePosition] ([Name], [Note])
VALUES ('Дизайнер', 'Создает дизайн интерфейсов и других визуальных элементов');

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

-- Вставка 1
INSERT INTO [HumanResources].[Employee] ([BusinessEntityID], [OrganizationID], [FirstName], [MiddleName], [LastName], [EmployeePositionID])
VALUES (6, 1, 'Иван', 'Петрович', 'Сидоров', 1);

-- Вставка 2
INSERT INTO [HumanResources].[Employee] ([BusinessEntityID], [OrganizationID], [FirstName], [MiddleName], [LastName], [EmployeePositionID])
VALUES (7, 2, 'Мария', 'Ивановна', 'Петрова', 2);

-- Вставка 3
INSERT INTO [HumanResources].[Employee] ([BusinessEntityID], [OrganizationID], [FirstName], [MiddleName], [LastName], [EmployeePositionID])
VALUES (8, 2, 'Алексей', 'Сергеевич', 'Иванов', 3);

-- Вставка 4
INSERT INTO [HumanResources].[Employee] ([BusinessEntityID], [OrganizationID], [FirstName], [MiddleName], [LastName], [EmployeePositionID])
VALUES (9, 4, 'Ольга', 'Владимировна', 'Сидорова', 4);

-- Вставка 5
INSERT INTO [HumanResources].[Employee] ([BusinessEntityID], [OrganizationID], [FirstName], [MiddleName], [LastName], [EmployeePositionID])
VALUES (10, 5, 'Дмитрий', 'Андреевич', 'Петров', 5);


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
	-- Впадлу тратить время на то, чтобы EF Core работал с ним
	-- [UserIDFoundByEnteredLogin] AS [Authentication].[udfGetExistingUserIDByLogin]([EnteredLogin]),
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

INSERT INTO [AccountingCPI].[JournalInstanceForCPARecord]([OrganizationID], [NameCPI], [SerialCPI], [InstanceNumber],
[ReceivedFromID],[DateAndNumberCoverLetterReceive],
[DateAndNumberCoverLetterSend], [DateAndNumberConfirmationSend], 
[DateAndNumberCoverLetterReturn], [DateAndNumberConfirmationReturn],
[CommissioningDate], [DecommissioningDate],
[DestructionDate],[DestructionActNumber]) VALUES
(
	2,
	'Диск CD-ROM с дистрибутивом СКЗИ',
	'Инв. № 5421',
	1,

	1, -- CryptoPro
	'т/н №54 от 10.01.2019',

	'т/н №44313 от 22.02.2020',
	null,

	NULL,
	NULL,

	'22.02.2020',
	NULL,

	NULL,
	NULL

),
(
	2,
	'Серийный номер лицезнии КриптоПро CSP',
	'RT4G-R45G-NU85-RT4G-DW3B',
	1,

	1, -- CryptoPro
	'т/н №54 от 10.01.2019',


	'т/н №44313 от 22.02.2020',
	null,

	NULL,
	NULL,

	'22.02.2020',
	NULL,

	NULL,
	NULL

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
	CONSTRAINT FK_JournalInstanceCPAReceiver_JournalInstanceForCPARecord_JournalInstanceForCPARecordID FOREIGN KEY ([RecordID]) REFERENCES [AccountingCPI].[JournalInstanceForCPARecord]([JournalInstanceForCPARecordID]),
	CONSTRAINT FK_JournalInstanceCPAReceiver_BusinessEntity_BusinessEntityID FOREIGN KEY ([ReceiverID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT UQ_JournalInstanceCPAReceiver_RecordID_ReceiverID UNIQUE([RecordID], [ReceiverID])
)

INSERT INTO [AccountingCPI].[JournalInstanceCPAReceiver] VALUES
(1, 2)

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
	CONSTRAINT FK_Hardware_Organization_OrganizationID FOREIGN KEY ([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID]),
	CONSTRAINT UQ_Hardware_SerialNumber UNIQUE ([SerialNumber], [OrganizationID])
)

INSERT INTO [Office].[Hardware]([OrganizationID], [Name], [SerialNumber]) VALUES
(2, 'АРМ AstraLinux x64', '534G7D3H')

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
	-- производивших подключение (установку) таблица]: JournalInstanceAccountingCPIForCIHInstallers
	-- №10 Дата подключения (установки) и подписи лиц, производивших подключение (установку) 
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

INSERT INTO [AccountingCPI].[JournalInstanceForCIHRecord]([OrganizationID], [NameCPI], [SerialCPI], [InstanceNumber],
[ReceivedFromID], [DateAndNumberCoverLetterReceive],
[CPIUserID],[DateAndNumberConfirmationIssue],
[InstallationDateAndConfirmation], 
[DestructionDate], [DestructionActNumber]
) VALUES
(
	2,
	'Диск CD-ROM с дистрибутивом СКЗИ',
	'Инв. № 5421',
	1,

	3, -- КиберПодпись
	'т/н №44313 от 22.02.2020',

	8, 
	'Акт приема передачи №22 от 13.02.2020',

	NULL,

	NULL,
	NULL
	
),
(
	2,
	'Серийный номер лицензии КриптоПро CSP',
	'RT4G-R45G-NU85-RT4G-DW3B',
	1,

	3,
	'т/н №44313 от 22.02.2020',

	7, -- пользователь СКЗИ конечный потребитель
	'Акт приема передачи №55 т 30.07.2020',

	'30.07.2020', -- ДАТА ПОДКЛЮЧЕНИЯ

	'20.09.2020', -- дата уничтожения,
	'Акт об изъятии СКЗИ №5 от 20.09.2020'

)

-- Название: Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ,
-- производивших подключение (установку)
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

INSERT INTO [AccountingCPI].[JournalInstanceForCIHInstaller] VALUES
(2, 7)

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

INSERT INTO [AccountingCPI].[JournalInstanceForCIHConnectedHardware] VALUES
(2, 1)

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

INSERT INTO [AccountingCPI].[JournalInstanceForCIHDestructor] VALUES
(2, 8)

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

INSERT INTO [AccountingCPI].[KeyDocumentType]([Name]) VALUES
('Ключевой носитель'),
('Сертификат открытого ключа'),
('Список отозванных сертифкатов'),
('Запрос на сертификат'),
('Ключевые соглашения'),
('Резервная копия ключей'),
('Журнал ключевых операций')

-- Название: Тип ключевого носителя
CREATE TABLE [AccountingCPI].[KeyHolderType]
(
	[KeyHolderTypeID] INT IDENTITY,
	[Name] nvarchar(256) NOT NULL,

	CONSTRAINT PK_KeyHolderType_KeyHolderTypeID PRIMARY KEY([KeyHolderTypeID]),
	CONSTRAINT UQ_KeyHolderType_Name UNIQUE ([Name])
)

INSERT INTO [AccountingCPI].[KeyHolderType] VALUES
('Токен USB eToken'),
('Токен USB Rutoken'),
('Токен USB JaCarta'),
('Токен Hardware Security Module'),
('Токен Trusted Platform Module'),

('Файл ключей'),
('База данных ключей'),
('Система управления ключами (KMS)')


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

INSERT INTO [AccountingCPI].[KeyHolder]([SerialNumber], [TypeID], [UserCPI]) VALUES 
('ABCD-EFGH-IJKL', 1, 7),
('ABCD-EFGH-IJKL5', 2, NULL),
('ABCD-EFGH-IJKL7', 3, NULL),
('ABCD-EFGH-IJKL9', 4, NULL),
('ABCD-EFGH-IJKL10', 5, NULL),
('ABCD-EFGH-IJKL12', 6, NULL),
('ABCD-EFGH-IJKL13e', 7, NULL),
('ABCD-EFGH-IJKL15', 8, NULL)

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

DECLARE @Salt VARBINARY(16);
DECLARE @Password NVARCHAR(16);

SET @Salt = CRYPT_GEN_RANDOM(16);
SET @Password = '12345';

INSERT INTO [Authentication].[User](Login, PasswordHash, PasswordSalt, UserRoleID, LastLoginDate, TotpSecretKey) VALUES
('admin', [dbo].[udfHashSalt](@Password, @Salt), @Salt, (SELECT [UserRoleID] from [Authorization].[UserRole] where [Name] = 'admin'), GETDATE(), 'HVR4CFHAFOWFGGFAGSA5JVTIMMPG6GMT')

SET @Salt = CRYPT_GEN_RANDOM(16);

INSERT INTO [Authentication].[User](Login, PasswordHash, PasswordSalt, UserRoleID, LastLoginDate, TotpSecretKey) VALUES
('director', [dbo].[udfHashSalt](@Password, @Salt), @Salt, (SELECT [UserRoleID] from [Authorization].[UserRole] where [Name] = 'director'), GETDATE(), 'HV54CFHAFOWFGGFAGSA5JVTIMMPG6GMT')

SET @Salt = CRYPT_GEN_RANDOM(16);

INSERT INTO [Authentication].[User](Login, PasswordHash, PasswordSalt, UserRoleID, LastLoginDate, TotpSecretKey) VALUES
('head_employee', [dbo].[udfHashSalt](@Password, @Salt), @Salt, (SELECT [UserRoleID] from [Authorization].[UserRole] where [Name] = 'head_employee'), GETDATE(), 'BV54CFHAFOWFGGFAGSA5JVTIMMPG6GMT')

SET @Salt = CRYPT_GEN_RANDOM(16);

INSERT INTO [Authentication].[User](Login, PasswordHash, PasswordSalt, UserRoleID, LastLoginDate, TotpSecretKey) VALUES
('employee', [dbo].[udfHashSalt](@Password, @Salt), @Salt, (SELECT [UserRoleID] from [Authorization].[UserRole] where [Name] = 'employee'), GETDATE(), 'BV64CFHAFOWFGGFAG225JVTIMMPG6GMT')

GO
CREATE FUNCTION [Authentication].[udfVerifyUser]
(
	@UserLogin nvarchar(32),
	@UserPassword nvarchar(32)
)
RETURNS BIT
AS
BEGIN
	DECLARE @PasswordHash VARBINARY(32)
	DECLARE @PasswordSalt VARBINARY(16)
	DECLARE @Find BIT = 0

	SELECT TOP 1 @Find = 1, @PasswordHash = [PasswordHash], @PasswordSalt = [PasswordSalt] FROM [Authentication].[User] WHERE [Login] = @UserLogin

	IF @Find = 0
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
				SELECT @SqlUpdate = @SqlUpdate + REPLACE(@SqlUpdateColumnTemplate, '#NAME', [sc].[Name])
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
--DECLARE @Template nvarchar(max) = 'ALTER TABLE [#SCHEMA].[#TABLE] ADD [SysIsDeleted] BIT NOT NULL; \
--ALTER TABLE [#SCHEMA].[#TABLE] ADD CONSTRAINT DF_#TABLE_SysIsDeleted DEFAULT 0 FOR [SysIsDeleted]; \
--ALTER TABLE [#SCHEMA].[#TABLE] ADD [SysModifiedDate] DATETIME NOT NULL; \
--ALTER TABLE [#SCHEMA].[#TABLE] ADD CONSTRAINT DF_#TABLE_SysModifiedDate DEFAULT GETDATE() FOR [SysModifiedDate];'

DECLARE @Template nvarchar(max) = 'ALTER TABLE [#SCHEMA].[#TABLE] ADD [SysIsDeleted] BIT NOT NULL CONSTRAINT DF_#TABLE_SysIsDeleted DEFAULT 0; \
ALTER TABLE [#SCHEMA].[#TABLE] ADD [SysModifiedDate] DATETIME NOT NULL CONSTRAINT DF_#TABLE_SysModifiedDate DEFAULT GETDATE();'

DECLARE @PrintTemplate nvarchar(max) = 'PRINT ''Сделаны столбцы SysModifiedDate и SysIsDeleted для таблицы #SCHEMA.#TABLE'''

EXEC [Audit].[uspDeleteMeAfterUsing] @Template, @PrintTemplate

-- ***********************************************************************
-- Создается пользователь 'admin' для проверки работы аудита, потом будут добавлены другие
-- ***********************************************************************

--INSERT INTO [Authorization].[UserRole] ([Name]) VALUES ('admin')

GO
CREATE PROCEDURE [Authorization].[uspSetCurrentUserSessionID]
(@SessionKey nvarchar(128))
AS 
BEGIN
	-- Procedure set current user id in sessino context dictionary
	SET NOCOUNT ON

	IF (@SessionKey IS NULL)
		RAISERROR('@SessionKey parameter has NULL value', 18, 1)

	DECLARE @UserSessionID INT
	DECLARE @IsExpired BIT

	SELECT TOP 1 @UserSessionID = [UserSessionID], @IsExpired = [IsExpired]
	FROM [Authorization].[UserSession]
	WHERE [SessionKey] = @SessionKey

	IF @UserSessionID IS NULL
	BEGIN
		RAISERROR('@SessionKey parameter has incorrect value', 18, 2)
	END

	IF @IsExpired = 1
	BEGIN
		RAISERROR('You cannot make this session current because it has expired.', 18, 3)
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

		IF NOT EXISTS(SELECT 1 FROM [Authentication].[User] WHERE [Login] = @UserLogin)
		BEGIN
			DECLARE @Error nvarchar(256) =  'Пользователь с логином '+@UserLogin+' отсутствует!'
			RAISERROR(@Error,18,1)
		END

		DECLARE @PasswordSalt VARBINARY(16) = CRYPT_GEN_RANDOM(16)
		DECLARE @PasswordHash VARBINARY(32) = [dbo].[udfHashSalt](@UserPassword, @PasswordSalt)

		UPDATE [Authentication].[User] SET PasswordSalt = @PasswordSalt, PasswordHash = @PasswordHash
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
			RAISERROR(@Error,18,1)
		END

		DECLARE @CreatedAt DATETIME = GETDATE()
		IF (@CreatedAt > @ExpiredAt)
		BEGIN
			SET @Error = CONCAT('Нельзя создать сессию, чьё время истекания меньше текущего:[CreatedAt] ',@CreatedAt,' > [ExpiredAt] ',@ExpiredAt)
			RAISERROR(@Error,18,2)
		END

		DECLARE @UsedSessionKey nvarchar(128) = (
			SELECT TOP 1 [SessionKey] FROM [UserSession]
			WHERE [IsExpired] = 0
			AND DATEDIFF(minute,[ExpiredAt], GETDATE()) > 10)
		
		IF @UsedSessionKey IS NOT NULL
		BEGIN
			SELECT @SessionKey = @UsedSessionKey
			RETURN
		END

		DECLARE @LogAuthenticationID INT = (
			SELECT TOP 1 [LogAuthenticationID]
			FROM [Audit].[LogAuthentication]
			WHERE [Authentication].[udfGetExistingUserIDByLogin]([EnteredLogin]) = @UserID AND
			([FirstFactorResult] = 1 AND [SecondFactorResult] = 1)
			ORDER BY [LogAuthenticationID] DESC )

		INSERT INTO [Authorization].[UserSession]([UserID], [CreatedAt], [ExpiredAt], [LogAuthenticationID], [SessionKey]) VALUES
		(@UserID, @CreatedAt, @ExpiredAt, @LogAuthenticationID, CONVERT(NVARCHAR(128), CRYPT_GEN_RANDOM(64), 2))

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
			RAISERROR('В процедуру были переданы имя таблицы и имя схемы несуществующие в Audit.TableData',18,1)
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
			RAISERROR(''Record #TABLEID %d in [#SCHEMA].[#TABLE] already deleted'',18,1,@RowID)#CRLF\
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
		RAISERROR(@Error, 18, 1)
	END

	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @SchemaName AND COLUMN_NAME = @ColumnName)
	BEGIN
		SET @Error = CONCAT(@ColumnName, ' не существует в таблице ', @SchemaName, '.', @TableName)
		RAISERROR(@Error, 18, 2)
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
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstallationDateAndConfirmation', 'Дата подключения (установки) и подписи лиц, производивших подключение (установку) в отметке о подключении (установке СКЗИ)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionDate', 'Дата изъятия (уничтожения) в отметка об изъятии СКЗИ из аппаратных средств, уничтожении ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionActNumber', 'Номер акта или расписка об уничтожении в отметка об изъятии СКЗИ из аппаратных средств, уничтожении ключевых документов'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', 'Примечание'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', 'Идентификатор файла подписи'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHInstaller'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, 'Ф.И.О. сотрудников органа криптографической защиты, пользователя СКЗИ, производивших подключение (установку)')
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

USE [SAES]

COMMIT TRANSACTION CreateTables