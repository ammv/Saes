-- ***********************************************************************
--    ������ ��� �������� ������ � ����� ���������������� ���� ������
-- 
-- ����� �������� ����������� ������-���������, �.� ���������� ������,
-- ��������, ����������...
-- ������� �������� ����������� ����� ���������� *** �� *** �� ������ ����� N
-- ��� ����� ����� ��������� ������� ������ ��� ��������� ������ ����������
-- ***********************************************************************

-- ***********************************�����*******************************
-- !!! ������ �������� ���� � ������� !!!
USE [SAES]

-- Sql Server �� ����� ��������� ���� ���� ����� �������� ���� ������
-- � ����� �������� ������ ��� ���������� �������� ���������� ��������
-- � ������ ��������� ��������� ���� �������, ����� ������ ����������� ���������
-- ***********************************************************************



-- ***********************************************************************
-- �1 �������� ������ ������������ � ��������� ����� ������

-- ��� ������ ��������: ��������� ����� ������

-- ������: 9437F0BB-982D-4AB6-8115-488D87F426A3
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

-- ******************************����� �1*********************************

GO

-- ***********************************************************************
-- �2 �������� ������ ������������, ��������� ����� ������ � ��������� ID ������� ������

-- ��� ������ ��������: ������������� ������� ������

-- ������: 1
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

-- ******************************����� �2*********************************

GO

-- ***********************************************************************
-- �3 �������� ������������ ��� ������������� 'admin'

-- ��� ������ ��������: �������������� ����� �������������� ������������ ������������ � ������� 'test'

-- ������:

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

-- ******************************����� �3*********************************

GO

-- ***********************************************************************
-- �4 �������� ������ ������������ � ����� ��������� ������ ���� ������

-- ��� ������ ��������: �������� ������

-- ������: 6	2024-04-05 03:00:25.947	NULL	50000	18	2	Authorization.uspCreateSession	25	������ ������� ������, ��� ����� ��������� ������ ��������:[CreatedAt] ���  5 2024  3:00AM > [ExpiredAt] ���  5 2024 12:00AM
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

print '������ �� ���������'

END TRY
begin catch
	IF @@TRANCOUNT > 0
	BEGIN
		ROLLBACK TRANSACTION
	END
	select * from [Audit].[ErrorLog] WHERE [ErrorLogID] = @@IDENTITY
END CATCH

-- ******************************����� �4*********************************

GO

-- ***********************************************************************
-- �5 ���������� ����� ���� � �������� ������� ���� ����������

-- ��� ������ ��������: ��� � �����������

-- ������: 1	2024-04-05 03:04:44.067	��������	UserRole	2	admin	admin
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

-- ******************************����� �5*********************************

GO

-- ***********************************************************************
-- �6 ���������� ����������� � ��������� �� ������

-- ��� ������ ��������: ��� �� ���������� ��������� � ������

-- ������: 
-- 5	Organization	DirectorFullName	������� ����� ����������	�����
-- 5	Organization	INN	123456789012	111777111000
-- 5	Organization	FullName	��� ���� � ������	��� ������� � ������
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
(1, '��� ���� � ������', '123456789012', '123456789', '������� ����� ����������')

UPDATE [HumanResources].[Organization]
SET [FullName] = '��� ������� � ������', [INN] = '111777111000', [DirectorFullName] = '�����'
WHERE [INN] = '123456789012'

SELECT * FROM [Audit].[viewLogChange] ORDER BY [LogID] DESC

IF @@TRANCOUNT > 0
BEGIN
	ROLLBACK TRANSACTION
END

DBCC CHECKIDENT('[HumanResources].[BusinessEntityType]', RESEED, 0)
DBCC CHECKIDENT('[HumanResources].[BusinessEntity]', RESEED, 0)
DBCC CHECKIDENT('[HumanResources].[Organization]', RESEED, 0)


-- ******************************����� �6*********************************

GO

-- ***********************************************************************
-- �7 �������� ����������� ���� ��������, ��������� �������� �������������� (SysIsDeleted >= 1)

-- ��� ������ ��������: ������ ���� - ��������, ���������, ��������, ��������������

-- ������: 
-- 38	2024-04-05 06:54:45.623	��������������	Organization	1	admin	admin
-- 37	2024-04-05 06:54:45.600	��������	Organization	1	admin	admin
-- 36	2024-04-05 06:54:45.573	����������	Organization	1	admin	admin
-- 35	2024-04-05 06:54:45.537	��������	Organization	1	admin	admin
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

-- ��� ��������
INSERT INTO [HumanResources].[Organization]([BusinessEntityID], [FullName], [INN], [KPP], [DirectorFullName]) VALUES
(1, '��� ���� � ������', '123456789012', '123456789', '������� ����� ����������')

-- ��� ����������
UPDATE [HumanResources].[Organization]
SET [FullName] = '��� ������� � ������', [INN] = '111777111000', [DirectorFullName] = '�����'
WHERE [INN] = '123456789012'

-- ��� �������
DELETE [HumanResources].[Organization] Where [INN] = '111777111000'

-- ��� ��������������
UPDATE [HumanResources].[Organization] SET [SysIsDeleted] = 0 WHERE [INN] = '111777111000'

SELECT * FROM [Audit].[viewLog] WHERE [Table] = 'Organization' ORDER BY [LogID] DESC, [Date] DESC 

IF @@TRANCOUNT > 0
BEGIN
	ROLLBACK TRANSACTION
END

DBCC CHECKIDENT('[HumanResources].[BusinessEntityType]', RESEED, 0)
DBCC CHECKIDENT('[HumanResources].[BusinessEntity]', RESEED, 0)
DBCC CHECKIDENT('[HumanResources].[Organization]', RESEED, 0)


-- ******************************����� �7*********************************