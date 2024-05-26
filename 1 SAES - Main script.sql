-- �� ������ --
-- ������ �� ������� ���������� ����� �152: https://ca.kontur.ru/Files/userfiles/file/news/������%20���������������%20�����%20����%2C%20����������������%20�%20�����������%20������������%20�%20���%2C%20��������.rtf

-- ***********************************************************************
--							������
-- ����� ������������ ������� �������� ������� ������� ������� � ����� (***)
-- ��� ��� ���� ����.
--
-- ����� ������������ �� ���������� ������� �� ������� � �����
-- ***********************************************************************



-- ***********************************************************************
--                    � ���� ������ � ��������� �� ������
-- 
-- � ���� ��� � ����� ��������������� ���� �������� �� �� ���������� ������, � �������� ����� ���� ������,
-- �������� � ������������, ���������.
--
-- ��� ���������� � ��������� � ���� ������ ��������:
-- 1. <����� ���� ����������� � ��������>:
--		+ ������ �� ����������, ��� � ����
-- 2. <������� ������ � �������������>:
--		+ ��������� ����������: ������ ������ ����������� �� ������ ������� � �������������� �������� ������, � �� �������������� (FK_User_UserRole_ABC234235)
--		+ ���������� ��������������� ������: ������� ��������� UNIQUE, CHECK, ������� �������� ������ ������, � ��� � ����
-- 3. <��������� ����>:
--		+ ������ ��������� ����� ��� ���������� ������, ��������, ������� � �.�.
-- 4. <��������� ���������� �������� SQL Server>:
--		+ ������ ����� �������� (���������, �������, ...) ��� �������� ������������ ��������� � ��������� �� ����������
-- 5. <���������� � ���������� ���������>:
--		+ ������ ������ �������� ���� ���������� � try..catch � ���������� ���������� ������ �� ������ ������ ���
-- 6. <�����>:
--		+ ����� ��������� � �������� ������ �������������, � ��������� ������������, ������� �� ������
-- 7. <������ �� ��������>:
--		+ ������ � ������� �������� ������ ������ �������, ��� ����� ������ ����� ������� ������ SysIsDeleted BIT
--
-- �� ���� ��� �������� ������� ���� ���� ������ ���������
-- ***********************************************************************



-- ***********************************************************************
--							�����
--					��� �������� �����
-- ���� �������, ��������� � ������� ���� �����������
-- � ������ ������ �� ������� � DML �����: UPDATE, INSERT, DELETE
-- ��� ��� �������� ����� ����������� � ������� ���������.
--
-- ��� ������ ������������� ������� �� ����������� ��������� ������ (�������������
-- ������ Audit � dbo � ������� Session) ��������� �������� �� ��� ��������
-- ��� ��������� ���������� ����� �� �������� DML, ��� �������� ������� ������������ ���������
-- � ������� �������� ������, �.�. ���������, � ������ ����� ��������� ���� ��������.
-- 
-- � ����� ������ ������������ ��������� ����������: �������, �������, ��������, �����, ID ������, ������������, ID ������, ���� ������������
-- ����� �����, ����� ������������ ��� ���������, ��� ����� ������������ ����������� ���������, ������� ������������� ID ������� ������
-- � �������� ������ (������� ����-�������) sp_set_session_context key(sql_variant), value(sql_variant).

-- ��� ���� ��������� ������� ���������-������, ����� ��������� ������������� � ���������� ���������� �������� ������
-- ��� ���� ���������: 
--		1. Authorization.uspSetCurrentUserSessionID @SessionKey UNIQUEIDENTIFIER - ������������� ID ������� ������
--		2. Authorization.uspGetCurrentUserSessionID @UserSessionID INT OUTPUT - ���������� ID ������� ������
-- 
-- ���� ������� ������������ �� ��� ����� ����� ��� ���������, �� ���� UserID ����� NULL, � ���������, ������� ��� �����������
-- ������ ��������������. ��� ������� ������, ��� �� ����� �������������� ������ ����� SSMS ��������� ��������� �����������, � ������ �������� ������ ��������� ���� T-SQL ���
-- �.�. ������� ���������, ����� �������� ������ ��������� ���������
--
-- ������� ���������� ������ ������ ����������� ������������ ����������� �� �������������� ���� ������
-- �������� � ������ ������� ������� ���� ���� �������������, ���� � ��������� �������� ��� �� �����
-- �� ��� ���� �������� ��� ������ �� ����� �������
--
-- ����������:
--	1. ����� ������ �������� �� ���������
--	2. �������� �������� ��������� ������, � ����� ������ ���������
--	3. ����� �������� ������ ���������, �������� ���������: EXEC Authentication.uspSetCurrentUserSessionKey @SessionKey
-- ***********************************************************************



-- ***********************************************************************
-- �������� ������������� FILESTREAM
-- ��������� ������� � ������� �� ���� �������� ������ (�����, ��������), � ���� ��������� �� ���, � ���� �������� ������
-- ����������� �� �����
-- ������� ����������� �������, ������ SQL �� ���������� ��������� ���������� � ��� ��������� �������
-- ***********************************************************************
EXEC sp_configure filestream_access_level, 2  
RECONFIGURE

-- ***********************************************************************
-- ������� ���� ������, ���� ��� ���������� � 100% �������
-- ���� ��� ������������, ������ ��������� � �������������������� ����� � ��������� ��� ����������
-- � ����� � 100% ������ ��������� �������
-- ***********************************************************************
use master
IF EXISTS(select * from sys.databases where name='SAES')
begin
	print '���� ������ ����� �������'
	alter database [SAES] set single_user with rollback immediate
	DROP DATABASE IF EXISTS [SAES];
end

CREATE DATABASE [SAES] COLLATE Cyrillic_General_CI_AS;
GO



-- ***********************************************************************
-- ������� ������� ��� ������ ��������� � Filestream
-- ***********************************************************************
ALTER DATABASE [SAES] ADD FILEGROUP [SAESFileGroup] CONTAINS FILESTREAM
GO
ALTER DATABASE [SAES] ADD FILE (Name =N'C', FILENAME ='C:\SAESFileGroup') TO FILEGROUP [SAESFileGroup]
GO



USE [SAES];

-- ***********************************************************************
-- ������� ������� ����������, ���� sql ���������� ������� ������ ������
-- ��� �� ������ ���������� �� ������ �� ������� ����������
-- ***********************************************************************
SET XACT_ABORT ON



BEGIN TRANSACTION CreateTables

-- ***********************************************************************
-- �������� ���� ������������� ������ ��� ���������� ���������������� ������
-- ���� ��� ���������� ��������� ����� �������� ����������� ������� SHA512('SAES')
-- ���� ��������� �� ������ ���� ������
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
-- ����� ��� ������ ������: ����, ���������� ������ � �� ��������
CREATE SCHEMA [Audit];
GO
-- ����� ��� ������ ��������� � ������������:����, �� �����
CREATE SCHEMA [Authorization];
GO
-- ����� ��� ������ ��������� � �������������: ������������
CREATE SCHEMA [Authentication];
GO

-- ����� ��� ������ � ����������, ������������
CREATE SCHEMA [HumanResources];
GO
-- ����� ��� ������ � ������������ �����������
CREATE SCHEMA [Person];
GO
-- ����� ��� ������ ��������� � ������ ��
CREATE SCHEMA [AccountingCPI];
GO
-- ����� ��� ������ ��������� � �������� ������
CREATE SCHEMA [Office];
GO

-- ������ ������������ NEWID � CRYPT_GEN_RANDOM � ��������
--CREATE FUNCTION [Authentication].[udfGenerateSecretString]
--(
--    @Length INT
--)
--RETURNS NVARCHAR(MAX)
--AS
--BEGIN
--    -- ������� ��� ��������� ��������� �����
--    DECLARE @SecretString NVARCHAR(MAX) = '';
--    DECLARE @IndexEnd INT = CEILING(@Length / 40.0); -- �������� ��� ������������� � CRYPT_GEN_RANDOM
--    DECLARE @IndexStart INT = 0;

--    WHILE @IndexStart < @IndexEnd
--    BEGIN
--        -- ���������� CRYPT_GEN_RANDOM ������ NEWID()
--        SET @SecretString = CONCAT(@SecretString,CONVERT(VARCHAR(40), CRYPT_GEN_RANDOM(32), 2)); 
--        SET @IndexStart = @IndexStart + 1;
--    END;

--    RETURN SUBSTRING(@SecretString, 1, @Length);
--END;

GO
-- ��������: ����
-- ��������: ������������ ����� ����, ���� �� �������� ��� PDF ��������
CREATE TABLE [File]
(
	[FileID] INT IDENTITY,
	-- ����������� ���� ��� FILESTREAM
	[GUID] [uniqueidentifier] ROWGUIDCOL NOT NULL CONSTRAINT DF_File_GUID DEFAULT NEWID(),
	-- ����������
	[Data] VARBINARY(MAX) FILESTREAM NULL,
	-- �������� �����
	[Name] NVARCHAR(128),
	-- ��� �������� � ���
	[ContentType] NVARCHAR(64),

	CONSTRAINT PK_File_FileID PRIMARY KEY([FileID]),
	CONSTRAINT UQ_File_GUID UNIQUE([GUID])
)

-- ��������: ���� ������������
CREATE TABLE [Authorization].[UserRole]
(
	[UserRoleID] INT IDENTITY,
	[Name] nvarchar(64) NOT NULL,
	
	CONSTRAINT PK_UserRole_UserRoleId PRIMARY KEY([UserRoleID]),
	CONSTRAINT UQ_UserRole_Name UNIQUE([Name])
)

-- ��������: ������ ����
-- ��������: ������������ ��� ����������� ���� ��� �������� ���������� � ������������� � ����������� ���������� ����
CREATE TABLE [Authorization].[RightGroup]
(
	[RightGroupID] INT IDENTITY,
	[Code] NVARCHAR(256),
	[Name] NVARCHAR(256),
	
	CONSTRAINT PK_RightGroup_RightGroupId PRIMARY KEY([RightGroupID]),
	CONSTRAINT UQ_RightGroup_Code UNIQUE([Code]),
)

-- ��������: �����
-- ��������: �������� ���� ����� UserListView, ����� ��������� � ���������� UserListView_CanView � ����� ����� ��������� 0 ��� 1
-- �.�. ������������ ��� ����� ��������� ���� � �������: ����� �� ������, ��������, ��������������
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

-- ��������: ����� ���� ������������
-- ��������: ����� ���� ������������ ���� ������������
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
-- ������� �� �� ������������
-- � �������� ���������� � ���������� ���� �������������
-- ***********************************************************************
---- ��������: �����
---- ��������: ������������� ������ ������������ � �������
--CREATE TABLE [Authorization].[Login]
--(
--	[LoginID] INT IDENTITY,
--	[Name] NVARCHAR(32) NOT NULL,
--	[Password] NVARCHAR(32) NOT NULL,

--	CONSTRAINT PK_Login_UserID PRIMARY KEY([LoginID]),
--	CONSTRAINT UQ_Login_Name UNIQUE([Name])
--)

-- ��������: ������������
CREATE TABLE [Authentication].[User]
(
	[UserID] INT IDENTITY,
	-- ����� ������������
	[Login] NVARCHAR(32) NOT NULL,
	-- ��� ������������
	[PasswordHash] VARBINARY(32) NOT NULL,
	-- ���� ������
	[PasswordSalt] VARBINARY(16) NOT NULL,
	-- ���� ������������
	[UserRoleID] INT,
	-- ��������� ���� �����������
	[LastLoginDate] DATETIME NULL,
	-- ������������� ��������� ���� ��� ��������� TOTP ������
	--[TotpSecretKey] VARBINARY(max) NOT NULL CONSTRAINT DF_User_TotpSecretKey
	--						DEFAULT ENCRYPTBYASYMKEY(ASYMKEY_ID('Main_AsymmetricKeys_Saes'),
	--												CRYPT_GEN_RANDOM(128)),
	--[TotpSecretKey] VARBINARY(max) NOT NULL CONSTRAINT DF_User_TotpSecretKey
	--						DEFAULT CRYPT_GEN_RANDOM(128),
	[TotpSecretKey] NVARCHAR(64),
	-- �������� �� ������������� ������������
	[TwoFactorEnabled] BIT NOT NULL CONSTRAINT DF_User_TwoFactorEnabled DEFAULT 1

	CONSTRAINT PK_User_UserID PRIMARY KEY([UserID]),
	CONSTRAINT UQ_User_Login UNIQUE([Login]),
	CONSTRAINT FK_User_UserRole_UserRoleID FOREIGN KEY([UserRoleID]) REFERENCES [Authorization].[UserRole]([UserRoleID])
)

-- ������� ������������ ������ �� ������ ��� �������� ������
-- �������: ������������ ������ - ����������, � ������� ���� ��������� �� ������ �������, ������� ������������� �� ��������� ��������� ��������
-- (��� ���������� �����)
CREATE NONCLUSTERED INDEX IX_User_Login
ON [Authentication].[User]([Login])

-- ������� �� �� ������������
-- � �������� ���������� ���� ������ ���� ������, ��� ��� ������ ��� � ������ ������ �������
-- ��������: ��� ���������� ������������
-- ��������: ������������ ��� ������ ������� ������������: ������������ ������, ������������ ����� � �.�.
--CREATE TABLE [Authentication].[ResultType]
--(
--	[ResultTypeID] INT IDENTITY,
--	-- �������� ���� ���������� ������������ (�������, ��������)
--	[Name] NVARCHAR(256) NOT NULL,

--	CONSTRAINT PK_ResultType_ResultTypeID PRIMARY KEY([ResultTypeID]),
--	CONSTRAINT UQ_ResultType_Name UNIQUE([Name])
--)

--CREATE NONCLUSTERED INDEX IX_ResultType_Name
--ON [Authentication].[ResultType]([Name])



-- ***********************************************************************
-- ��������� ������� ��� �����, ��������, ������� �� ���� �������
-- �� ������� ����, ��� ��� ���������� � �������
-- �� ��� �� ����� ������������� ����� ��� ����������
-- ***********************************************************************

-- ��������: �����
CREATE TABLE [Person].[Address]
(
	[AddressID] INT IDENTITY,
	-- ������
	[Country] NVARCHAR(128),
	-- ������
	[Region] NVARCHAR(128),
	-- �����
	[City] NVARCHAR(128),
	-- �����
	[District] NVARCHAR(128),
	-- �����
	[Street] NVARCHAR(128),
	-- ����� ������
	[BuildingNumber] NVARCHAR(16),
	-- ������
	[Corpus] NVARCHAR(16),
	-- ����
	[Floor] INT,
	-- ��������/����
	[Flat] NVARCHAR(16),
	-- �������� ������
	[PostalIndex] NVARCHAR(10),

	CONSTRAINT PK_Address_AddressID PRIMARY KEY([AddressID])
)

-- ��������: ��� ������-��������
-- ��������: ������������ ��� ������-��������, �������� �����������, ���������, ����������
CREATE TABLE [HumanResources].[BusinessEntityType]
(
	[BusinessEntityTypeID] INT IDENTITY,
	[Name] NVARCHAR(128) NOT NULL,

	CONSTRAINT PK_BusinessEntityType_BusinessEntityTypeID PRIMARY KEY([BusinessEntityTypeID]),
	CONSTRAINT UQ_BusinessEntityType_Name UNIQUE ([Name])
)
-- ��������: ������-��������
-- ��������: ���������� �������� ��������� ��� ����������� ��������� ���������: �����������, ��������� � ������
CREATE TABLE [HumanResources].[BusinessEntity]
(
	[BusinessEntityID] INT IDENTITY,
	-- ��� �����������
	-- ���� ������� ���� 1 - ������ ������� ������ ������������ �����������
	-- ���� �������� ���� 0 - ������ ������� ������ ������������ ����������
	[BusinessEntityTypeID] INT NOT NULL

	CONSTRAINT PK_BusinessEntity_BusinessEntityID PRIMARY KEY([BusinessEntityID]),
	CONSTRAINT FK_BusinessEntity_BusinessEntityType_BusinessEntityTypeID FOREIGN KEY([BusinessEntityTypeID]) REFERENCES [HumanResources].[BusinessEntityType]([BusinessEntityTypeID]),
)

-- ��������: �����������
CREATE TABLE [HumanResources].[Organization]
(
	-- �������� ��������� � ��������� ������
	[BusinessEntityID] INT NOT NULL,

	-- �� �������� ��������� ������
	[OrganizationID] INT IDENTITY NOT NULL,
	-- ������ ������������ �����������
	[FullName] NVARCHAR(256),
	-- ����������� ������������ �����������
	[ShortName] NVARCHAR(256),
	-- ����������� �����/����� ������������
	[BusinessAddressID] INT,
	-- ���
	[INN] NVARCHAR(12),
	-- ���
	[KPP] NVARCHAR(9),
	-- ����
	[OKPO] NVARCHAR(8),
	-- ����
	[OGRN] NVARCHAR(13),
	-- ���� ���������� ����
	[DateOfAssignmentOGRN] DATE,
	-- ��� ���������
	[DirectorFullName] NVARCHAR(128),
	-- ��� ��.���������
	[ChiefAccountantFullName] NVARCHAR(128),
	-- �������� ��� �����
	[OKVED] NVARCHAR(32),
	-- �������� ���������� ������� ����� ����
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

-- ��������: ��� ��������
CREATE TABLE [Person].[ContactType]
(
	[ContactTypeID] INT IDENTITY, 
	[Name] NVARCHAR(64),
	
	CONSTRAINT PK_ContactType_ContactTypeID PRIMARY KEY([ContactTypeID]),
	CONSTRAINT UQ_ContactType_Name UNIQUE([Name]),
)

-- ��������: �������� �����������
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

-- ��������: ��������� ����������
CREATE TABLE [HumanResources].[EmployeePosition]
(
	[EmployeePositionID] INT IDENTITY,
	[Name] nvarchar(64) NOT NULL,
	[Note] nvarchar(512) NULL,

	CONSTRAINT PK_EmployeePosition_EmployeePositionID PRIMARY KEY([EmployeePositionID]),
	CONSTRAINT UQ_EmployeePosition_Name UNIQUE([Name])
)

-- ��������: ���������
-- ��������: ������������ ��� ����� ����������� �����������
CREATE TABLE [HumanResources].[Employee]
(
	-- �������� ��������� � ������� ������
	[BusinessEntityID] INT NOT NULL,

	-- �� �������� ��������� ������
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

-- ��������: ������ ������
-- ��������: ������������ ��� �������� ��������� ������, ������������ ��� ������� ������
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

-- �������� ������� ��� ��������� ������ �� �����
CREATE INDEX [IX_TableData_Name] ON [Audit].[TableData] ([Name], [SchemaName])

-- ��������: ������ �������� ������
-- ��������: ������������ ��� �������� ��������� �������� ������, ������������ ��� ������� ������
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

-- �������� ������� ��� ��������� ������ �� �����
CREATE INDEX [IX_TableColumnData_Name] ON [Audit].[TableColumnData] ([Name])

GO

CREATE FUNCTION [Authentication].[udfGetExistingUserIDByLogin]
(@UserLogin nvarchar(32))
RETURNS INT
AS
BEGIN
	-- ������� ��� ��������� ID ������������ �� ��� ������
	DECLARE @UserID INT = (SELECT TOP 1 [UserID] FROM [Authentication].[User] WHERE [Login] = @UserLogin)
	RETURN @UserID
END

GO

-- ��������: ���� ������������ �������������
-- ��������: ����� �������� ��� �������� ���� �������������
CREATE TABLE [Audit].[LogAuthentication]
(
	[LogAuthenticationID] INT IDENTITY,
	-- �������� �����
	[EnteredLogin] NVARCHAR(32) NULL,
	-- ��� ���������� ������������ �� ������� ������ (�����, ������)
	[FirstFactorResult] BIT NOT NULL,
	-- ��� ���������� ������������ �� ������� �������� (�������, �����)
	[SecondFactorResult] BIT NOT NULL,
	-- ����� ������� ������������
	[AuthServiceResponse] NVARCHAR(1024) NOT NULL,
	-- �� ������������ ���������� �� ��������� ������
	[UserIDFoundByEnteredLogin] AS [Authentication].[udfGetExistingUserIDByLogin]([EnteredLogin]),
	-- MAC-����� ����������
	[MAC] NVARCHAR(12) NULL,
	--IP �����-����������
	[IP] NVARCHAR(12) NULL,
	-- ��� ����������
	[MashineName] NVARCHAR(128) NULL,
	-- ��� ������������ ��,
	[MashineUserName] NVARCHAR(128) NULL,

	[Date] DATETIME NOT NULL CONSTRAINT DF_LogAuthentication_Date DEFAULT GETDATE(),

	CONSTRAINT PK_LogAuthentication_LogAuthenticationID PRIMARY KEY ([LogAuthenticationID]),
	--CONSTRAINT FK_LogAuthentication_User_UserID FOREIGN KEY ([UserIDFoundByEnteredLogin]) REFERENCES [Authentication].[User]([UserID]),
	--CONSTRAINT FK_LogAuthentication_FirstFactorAuthResultTypeID_ResultType_ResultTypeID FOREIGN KEY ([FirstFactorAuthResultTypeID]) REFERENCES [Authentication].[ResultType]([ResultTypeID]),
	--CONSTRAINT FK_LogAuthentication_SecondFactorAuthResultTypeID_ResultType_ResultTypeID FOREIGN KEY ([SecondFactorAuthResultTypeID]) REFERENCES [Authentication].[ResultType]([ResultTypeID]),
)

-- ��������: ������ ������������
-- ��������: ������������ ��� �������������� ������������ ������ � �������
CREATE TABLE [Authorization].[UserSession]
(
	-- ������������� ������
	[UserSessionID] INT IDENTITY,
	-- ������������� ���� ������
	-- ������������ ��� ����������� � ���������� �������� � �������
	--[SessionKey] varbinary(max) NOT NULL CONSTRAINT DF_UserSession_SessionKey
	--						DEFAULT ENCRYPTBYASYMKEY(ASYMKEY_ID('Main_AsymmetricKeys_Saes'),
	--												[Authentication].[udfGenerateSecretString](128)),
	[SessionKey] nvarchar(128) NOT NULL,
	-- ������������� ������������
	[UserID] INT NOT NULL,
	-- ���� � ����� �������� ������
	[CreatedAt] DATETIME NOT NULL,
	-- ���� � ����� ��������� �������� ������
	[ExpiredAt] DATETIME NOT NULL,
	-- ������� �� ������
	[IsExpired] AS CONVERT(BIT, CASE WHEN (GETDATE() > [ExpiredAt]) then 1 else 0 end, 0),
	-- ��������� ��� ������������
	[LogAuthenticationID] INT NOT NULL,

	CONSTRAINT PK_UserSession_UserSessionID PRIMARY KEY([UserSessionID]),
	CONSTRAINT UQ_UserSession_SessionKey UNIQUE([SessionKey]),
	CONSTRAINT FK_UserSession_User_UserID FOREIGN KEY([UserID]) REFERENCES [Authentication].[User]([UserID]),
	CONSTRAINT FK_UserSession_LogAuthentication_LogAuthenticationID FOREIGN KEY(LogAuthenticationID) REFERENCES [Audit].[LogAuthentication]([LogAuthenticationID]),
)

-- ��� �������������� � �������� ����� ����� ������������� ����� �� ����� �����
CREATE NONCLUSTERED INDEX IX_UserSession_SessionKey
ON [Authorization].[UserSession]([SessionKey])


-- ��������: ���� ������
-- ��������: ������������ ��� ������ ������ ���� ������
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

-- ��������: �����
-- ��������: ����� �������� ��� �������� ���� �������������
CREATE TABLE [Audit].[Log]
(
	[LogID] INT IDENTITY,
	[TableDataID] INT NULL,
	[TableRowID] INT NOT NULL,
	-- �������� (���. �����):
	-- 'C'(Create) - ������ ������
	-- 'U'(Update) - ������� ������
	-- 'D'(Delete) - ������ ������
	-- 'R'(Recover) - ����������� ������ (SysIsDeleted => 1) 
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

-- ��������: ������ ������
-- ��������: ����� �������� ��� �������� ���� �������������
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
-- ��������: ���� (��������-��������)
-- ��������: ������������� ��� �����, ����� ����� ���� �� ���������
CREATE VIEW [Audit].[viewLog]
AS 
	SELECT
	[lg].[LogID] AS [LogID],
	[lg].[Date] as [Date],
	CASE [lg].[Action]
		WHEN 'C' THEN '��������'
		WHEN 'U' THEN '����������'
		WHEN 'D' THEN '��������'
		WHEN 'R' THEN '��������������'
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
-- ��������: ���� �������� (��������-��������)
-- ��������: ������������� ��� ����� ���������, ����� ����� ���� �� ���������
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
-- ������� �� �� ������������
-- � �������� ���������� � ���������� ���� ������������� � ����� �������� �� HumanResources.BusinessEntity
-- ***********************************************************************
-- ��������: ������� ������� ����� ����
-- ��������: �������� �������� ��� ���� � ������ ��� ��������� ������ ���� ������
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

-- ��������: ������ ��������������� ����� ���� ��� ������ ����������������� ������
CREATE TABLE [AccountingCPI].[JournalInstanceForCPARecord]
(
	-- ����� ����������� ����������� ������ ������
	[OrganizationID] INT,
	-- �1
	[JournalInstanceForCPARecordID] INT IDENTITY,
	-- �2 ������������ ����, ���������������� � ����������� ������������ � ���, �������� ����������
	[NameCPI] nvarchar(512) null,
	-- �3 �������� ������ ����, ���������������� � ����������� ������������ � ���, ������ ����� �������� ���������� 
	[SerialCPI] nvarchar(256) null,
	-- �4 ������ ����������� (����������������� ������) �������� ���������� 
	[InstanceNumber] int null,
	
	-- ����: ������� � ��������� 
	--�5 �� ���� �������� ��� �.�.�. ���������� ������ ����������������� ������, 
	[ReceivedFromID] int null, 
	--�6 ���� � ����� ����������������� ������ ��� ���� ������������ �������� ���������� � �������� � ������������ 
	[DateAndNumberCoverLetterReceive] nvarchar(256) null,

	-- ����: ������� � �������� (��������) 
	-- �7 [���� ��������� (��������) �������: JournalInstanceAccountingCPIForCPAReceiver]
	-- �8 ���� � ����� ����������������� ������ 
	[DateAndNumberCoverLetterSend] nvarchar(256) null,
	-- �9 ���� � ����� ������������� ��� �������� � ��������� 
	[DateAndNumberConfirmationSend] nvarchar(256) null,

	-- ����: ������� � �������� 
	-- �10 ���� � ����� ����������������� ������ 
	[DateAndNumberCoverLetterReturn] nvarchar(256) null,
	-- �11 ���� � ����� ������������� 
	[DateAndNumberConfirmationReturn] nvarchar(256) null,

	-- �12 ���� ����� � �������� 
	[CommissioningDate] datetime null,
	-- �13 ���� ������ �� �������� 
	[DecommissioningDate] datetime null,

	-- ����: ������� �� ����������� ����, �������� ���������� 
	-- �14 ���� ����������� 
	[DestructionDate] datetime null,
	-- �15 ����� ���� ��� �������� �� ����������� 
	[DestructionActNumber] nvarchar(256) null,
	
	-- �16 ����������
	[Note] nvarchar(512) null,

	-- ���� �������
	[SignFileID] int

	CONSTRAINT PK_JournalInstanceForCPARecord_JournalInstanceForCPARecordID PRIMARY KEY([JournalInstanceForCPARecordID]),
	CONSTRAINT FK_JournalInstanceForCPARecord_BusinessEntity_BusinessEntityID FOREIGN KEY ([ReceivedFromID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_JournalInstanceForCPARecord_File_FileID FOREIGN KEY ([SignFileID]) REFERENCES [File]([FileID]),
	CONSTRAINT FK_JournalInstanceForCPARecord_Organization_OrganizationID FOREIGN KEY ([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID]),
)

-- ��������:  �������� ������� ��������������� ����� ���� ��� ������ ����������������� ������ ������� ���� ��������� ����������
-- ��������: ������������ ��� �������� ��������� (��.����, ���.����) � �������� ����:
-- 1. ���� ��������� (��������) 
CREATE TABLE [AccountingCPI].[JournalInstanceCPAReceiver]
(
	[JournalInstanceCPAReceiverID] INT IDENTITY,
	-- ������ �� ������ � �������
	[RecordID] INT,
	-- �������� (�������)
	[ReceiverID] INT,

	CONSTRAINT PK_JournalInstanceCPAReceiver_JournalInstanceCPAReceiverID PRIMARY KEY ([JournalInstanceCPAReceiverID]),
	CONSTRAINT FK_JournalInstanceCPAReceiver_JournalInstanceForCPARecord_JournalInstanceForCPARecordID FOREIGN KEY ([JournalInstanceCPAReceiverID]) REFERENCES [AccountingCPI].[JournalInstanceForCPARecord]([JournalInstanceForCPARecordID]),
	CONSTRAINT FK_JournalInstanceCPAReceiver_BusinessEntity_BusinessEntityID FOREIGN KEY ([ReceiverID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT UQ_JournalInstanceCPAReceiver_RecordID_ReceiverID UNIQUE([RecordID], [ReceiverID])
)

-- ��������: ����������
-- ��������: ������������ � ��������
-- ��� ����� ������� ����(������� �������) � �����������
CREATE TABLE [Office].[Hardware]
(
	[HardwareID] INT IDENTITY,
	-- ����� ����������� ����������� ����������
	[OrganizationID] int,
	-- �������� ����������
	[Name] nvarchar(256) null,
	-- �������� ����� (���. ����������� ����� ��� ������ (�� ������� ��))
	[SerialNumber] nvarchar(32),
	-- �������
	[Note] nvarchar(512) null,

	CONSTRAINT PK_Hardware_HardwareID PRIMARY KEY ([HardwareID]),
	CONSTRAINT UQ_Hardware_SerialNumber UNIQUE ([SerialNumber], [OrganizationID])
)

-- ��������: ������ ��������������� ����� ����, 
-- ���������������� � ����������� ������������ � ���,
-- �������� ���������� (��� ���������� ���������������� ����������) 
CREATE TABLE [AccountingCPI].[JournalInstanceForCIHRecord]
(
	-- ����� ����������� ����������� ����������
	[OrganizationID] int,
	-- �1
	[JournalInstanceForCIHRecordID] INT IDENTITY,
	-- �2 ������������ ����, ���������������� � ����������� ������������ � ���, �������� ����������
	[NameCPI] nvarchar(512) null,
	-- �3 �������� ������ ����, ���������������� � ����������� ������������ � ���, ������ ����� �������� ���������� 
	[SerialCPI] nvarchar(256) null,
	-- �4 ������ ����������� (����������������� ������) �������� ���������� 
	[InstanceNumber] int null,

	-- ����: ������� � ��������� 
	-- �5 �� ���� �������� 
	[ReceivedFromID] int null, 
	-- �6 ���� � ����� ����������������� ������ 
	[DateAndNumberCoverLetterReceive] nvarchar(256) null,

	-- ����: ������� � ������ 
	-- �7 �.�.�. ������������ ���� 
	[CPIUserID] int null, 
	-- �8 ���� � �������� � ��������� 
	[DateAndNumberConfirmationIssue] nvarchar(256) null,

	-- ����: ������� � ����������� (��������� ����) 
	-- �9 [�.�.�. ����������� ������ ����������������� ������, ������������ ����,
	-- ����������� ����������� (���������) �������]: JournalInstanceAccountingCPIForCIHInstallers
	-- �10 ���� ����������� (���������) � ������� ���, ����������� ����������� (���������) 
	[InstallationDateAndConfirmation] nvarchar(256) null,
	-- �11 [������ ���������� �������, � ������� ����������� ��� � ������� ���������� ���� 
	-- �������]: JournalInstanceAccountingCPIForCIHConnectedHardwares
	
	-- ����: ������� �� ������� ���� �� ���������� �������, ����������� �������� ���������� 
	-- �12 ���� ������� (�����������) 
	[DestructionDate] datetime null,
	-- �13 [�.�.�. ����������� ������ ����������������� ������, ������������ ����,
	-- ������������� ������� (�����������) �������]: JournalInstanceAccountingCPIForCIHDestructors
	-- �14 ����� ���� ��� �������� �� ����������� 
	[DestructionActNumber] nvarchar(256) null,
	
	-- �15 ����������
	[Note] nvarchar(512) null,

	-- ���� �������
	[SignFileID] int

	CONSTRAINT PK_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID PRIMARY KEY([JournalInstanceForCIHRecordID]),
	--CONSTRAINT FK_JournalInstanceForCIHRecord_Subject_SubjectID_0 FOREIGN KEY ([ReceivedFromID]) REFERENCES [AccountingCPI].[Subject]([SubjectID]),
	--CONSTRAINT FK_JournalInstanceForCIHRecord_Subject_SubjectID_1 FOREIGN KEY ([CPIUserID]) REFERENCES [AccountingCPI].[Subject]([SubjectID]),
	CONSTRAINT FK_JournalInstanceForCIHRecord_ReceivedFromID_BusinessEntity_BusinessEntityID FOREIGN KEY ([ReceivedFromID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_JournalInstanceForCIHRecord_CPIUserID_BusinessEntity_BusinessEntityID FOREIGN KEY ([CPIUserID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_JournalInstanceForCIHRecord_File_FileID FOREIGN KEY ([SignFileID]) REFERENCES [File]([FileID]),
	CONSTRAINT FK_JournalInstanceForCIHRecord_Organization_OrganizationID FOREIGN KEY ([OrganizationID]) REFERENCES [HumanResources].[Organization]([OrganizationID])
)

-- ��������: �.�.�. ����������� ������ ����������������� ������, ������������ ����,
-- ����������� ����������� (���������)
-- ��������: ������������ � ������� ��������������� ����� ����,
-- ���������������� � ����������� ������������ � ���,
-- �������� ���������� (��� ���������� ���������������� ����������) 
CREATE TABLE [AccountingCPI].[JournalInstanceForCIHInstaller]
(
	[JournalInstanceForCIHInstallerID] INT IDENTITY,
	-- ������ �� ������ � �������
	[RecordID] INT NOT NULL ,
	-- ������� ������������� ���������
	[InstallerID] INT NOT NULL,

	CONSTRAINT PK_JournalInstanceForCIHInstaller_JournalInstanceForCIHInstallerID PRIMARY KEY ([JournalInstanceForCIHInstallerID]),
	CONSTRAINT UQ_JournalInstanceForCIHInstaller_RecordID_InstallerID UNIQUE([RecordID], [InstallerID]),
	CONSTRAINT FK_JournalInstanceForCIHInstaller_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID FOREIGN KEY([RecordID]) REFERENCES [AccountingCPI].[JournalInstanceForCIHRecord]([JournalInstanceForCIHRecordID]),
	CONSTRAINT FK_JournalInstanceForCIHInstaller_BusinessEntity_BusinessEntityID FOREIGN KEY([InstallerID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID])
)

-- ��������: ������ ���������� �������, � ������� ����������� ��� � ������� ���������� ����
-- ��������: ������������ � ������� ��������������� ����� ����,
-- ���������������� � ����������� ������������ � ���,
-- �������� ���������� (��� ���������� ���������������� ����������) ��� ��������
-- ���������� �������, � ������� ����������� ����
CREATE TABLE [AccountingCPI].[JournalInstanceForCIHConnectedHardware]
(
	[JournalInstanceForCIHConnectedHardwareID] INT IDENTITY,
	-- ������ � �������
	[RecordID] int not null,
	-- �������
	[HardwareID] int not null,

	CONSTRAINT PK_JournalInstanceForCIHConnectedHardware_JournalInstanceForCIHConnectedHardwareID PRIMARY KEY([JournalInstanceForCIHConnectedHardwareID]),
	CONSTRAINT FK_JournalInstanceForCIHConnectedHardware_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID FOREIGN KEY([RecordID]) REFERENCES [AccountingCPI].[JournalInstanceForCIHRecord]([JournalInstanceForCIHRecordID]),
	CONSTRAINT FK_JournalInstanceForCIHConnectedHardware_Hardware_HardwareID FOREIGN KEY([HardwareID]) REFERENCES [Office].[Hardware]([HardwareID]),
	CONSTRAINT UQ_RecordCPIForCIH_RecordID_HardwareID UNIQUE([RecordID], [HardwareID])
)

-- ��������: �.�.�. ����������� ������ ����������������� ������, ������������ ����,
-- ������������� ������� (�����������)
-- ��������: ������������ � ������� ��������������� ����� ����,
-- ���������������� � ����������� ������������ � ���,
-- �������� ���������� (��� ���������� ���������������� ����������) ��� �������� 
-- ����������� ������������� ������� ����
CREATE TABLE [AccountingCPI].[JournalInstanceForCIHDestructor]
(
	[JournalInstanceForCIHDestructorID] INT IDENTITY,
	-- ������ � �������
	[RecordID] int not null,
	-- ������� ������������� ������� ����
	[DestructorID] int not null,

	CONSTRAINT PK_JournalInstanceForCIHDestructor_JournalInstanceForCIHDestructorID PRIMARY KEY ([JournalInstanceForCIHDestructorID]),
	CONSTRAINT FK_JournalInstanceForCIHDestructor_BusinessEntity_BusinessEntityID FOREIGN KEY([DestructorID]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_JournalInstanceForCIHDestructor_JournalInstanceForCIHRecord_JournalInstanceForCIHRecordID FOREIGN KEY([RecordID]) REFERENCES [AccountingCPI].[JournalInstanceForCIHRecord]([JournalInstanceForCIHRecordID]),
	CONSTRAINT UQ_JournalInstanceForCIHDestructor_RecordID_DestructorID UNIQUE([RecordID], [DestructorID])
)

-- ��������: ��� ��������� ���������
-- ��������: ������������ � ������ ����������� (����������)
-- ���� �������� ���������� ����� �������� �����, �����������,
-- ��������, ����� ����������� ������� � ������ ��������. 
CREATE TABLE [AccountingCPI].[KeyDocumentType]
(
	[KeyDocumentTypeID] INT IDENTITY,
	[Name] nvarchar(256) NOT NULL,
	[Note] nvarchar(256) NULL,

	CONSTRAINT PK_KeyDocumentType_KeyDocumentTypeID PRIMARY KEY([KeyDocumentTypeID]),
	CONSTRAINT UQ_KeyDocumentType_Name UNIQUE ([Name])
)

-- ��������: ��� ��������� ��������
CREATE TABLE [AccountingCPI].[KeyHolderType]
(
	[KeyHolderTypeID] INT IDENTITY,
	[Name] nvarchar(256) NOT NULL,

	CONSTRAINT PK_KeyHolderType_KeyHolderTypeID PRIMARY KEY([KeyHolderTypeID]),
	CONSTRAINT UQ_KeyHolderType_Name UNIQUE ([Name])
)

-- ��������: �������� ��������
CREATE TABLE [AccountingCPI].[KeyHolder]
(
	[KeyHolderID] INT IDENTITY,
	-- �������� �����
	[SerialNumber] nvarchar(128) not null,
	-- ��� ��������� ��������
	[TypeID] INT NOT NULL,
	-- ������������ ���� �������� ����������� ������ ��������
	[UserCPI] INT NULL,
	-- ���� �������
	[SignFileID] INT NULL,

	CONSTRAINT PK_KeyHolder_KeyHolderID PRIMARY KEY([KeyHolderID]),
	CONSTRAINT UQ_KeyHolder_SerialNumber UNIQUE([SerialNumber]),
	CONSTRAINT FK_KeyHolder_KeyHolderType_KeyHolderTypeID FOREIGN KEY([TypeID]) REFERENCES [AccountingCPI].[KeyHolderType]([KeyHolderTypeID]),
	CONSTRAINT FK_KeyHolder_BusinessEntity_BusinessEntityID FOREIGN KEY([UserCPI]) REFERENCES [HumanResources].[BusinessEntity]([BusinessEntityID]),
	CONSTRAINT FK_KeyHolder_File_FileID FOREIGN KEY([SignFileID]) REFERENCES [dbo].[File]([FileID]),
)

-- ��������: ������ ����������� (����������) 
CREATE TABLE [AccountingCPI].[JournalTechnicalRecord]
(
	-- ����� ����������� ����������� ����������
	[OrganizationID] int,
	-- �1
	[JournalTechnicalRecordID] INT IDENTITY,
	-- �2 ����
	[Date] DATE NULL,
	-- �3 ��� � �������� ������ ������������ ����
	[TypeAndSerialUsedCPI] nvarchar(256) null,
	-- �4 ������ �� ������������ ���� 
	[RecordOnMaintenanceCPI] nvarchar(256) null,
	
	-- ����: ������������ ����������� 
	-- �5 [��� ��������� ���������]
	[KeyDocumentTypeID] INT null ,
	-- �6 ��������, ����������������� ����� � ����� ���������� ��������� ��������� 
	[SerialCPIAndKeyDocumentInstanceNumber] nvarchar(256) null,
	-- �7 ����� �������� ��������� �������� ��� ���� ����, � ������� ������� ����������� 
	[NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted] NVARCHAR(256) NULL,
	
	-- ����: ������� �� ����������� (��������) 
	-- �8 ����
	[DestructionDate] datetime null,
	-- �9 ������� ������������ ���� (���� ����� ����, ���� ��� ���)
	[ActNumber] nvarchar(256) null,
	-- �10 ����������
	[Note] nvarchar(512) null,

	-- ���� �������
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
	-- ������� ��� ����������� ���������� �������� � ����� �� 16 ���� ���������� SHA2_256
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
	-- ��������� ��� ���������� ������������� sql �������, � ������� ������������� ����� ������ � ����
	-- ������������ ��� �������� ��������� ��� ���� ������, � �������
	-- ����� ���� �� ����� ���������� �������
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
				PRINT '[������] �� ������� ��������� T-SQL ������: '+ERROR_MESSAGE()
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
-- ������ ������� ���������� �������� [SysIsDeleted] BIT NOT NULL � [SysModifiedDate] DATETIME NOT NULL
-- �� ���� ��������, ����� �������� � ����� [Audit], [dbo]
-- ***********************************************************************
DECLARE @Template nvarchar(max) = 'ALTER TABLE [#SCHEMA].[#TABLE] ADD [SysIsDeleted] BIT NOT NULL; \
ALTER TABLE [#SCHEMA].[#TABLE] ADD CONSTRAINT DF_#TABLE_SysIsDeleted DEFAULT 0 FOR [SysIsDeleted]; \
ALTER TABLE [#SCHEMA].[#TABLE] ADD [SysModifiedDate] DATETIME NOT NULL; \
ALTER TABLE [#SCHEMA].[#TABLE] ADD CONSTRAINT DF_#TABLE_SysModifiedDate DEFAULT GETDATE() FOR [SysModifiedDate];'

DECLARE @PrintTemplate nvarchar(max) = 'PRINT ''������� ������� SysModifiedDate � SysIsDeleted ��� ������� #SCHEMA.#TABLE'''

EXEC [Audit].[uspDeleteMeAfterUsing] @Template, @PrintTemplate

-- ***********************************************************************
-- ��������� ������������ 'admin' ��� �������� ������ ������, ����� ����� ��������� ������
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
	-- ��������� ��� ���������� ������������, ������� ������������� �������� ������ ������������
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
	-- ��������� ��� ���������� ������ ������������
	SET NOCOUNT ON;

	BEGIN TRY

		IF NOT EXISTS(SELECT 1 FROM [Authorization].[User] WHERE [Login] = @UserLogin)
		BEGIN
			DECLARE @Error nvarchar(256) =  '������������ � ������� '+@UserLogin+' �����������!'
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
	-- ��������� ��� �������� ������ ������������
	SET NOCOUNT ON;

	BEGIN TRY
		DECLARE @Error nvarchar(256)

		IF NOT EXISTS(SELECT TOP 1 1 FROM [Authentication].[User] WHERE [UserID] = 1 AND [SysIsDeleted] = 0)
		BEGIN
			SET @Error =  '������������ � ��������������� '+@UserID+' �� ���������� ��� ������!'
			RAISERROR(@Error,19,1)
		END

		DECLARE @CreatedAt DATETIME = GETDATE()
		IF (@CreatedAt > @ExpiredAt)
		BEGIN
			SET @Error = CONCAT('������ ������� ������, ��� ����� ��������� ������ ��������:[CreatedAt] ',@CreatedAt,' > [ExpiredAt] ',@ExpiredAt)
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
	-- ��������� ��� ���������� ����, ����� �������� � ��������
	SET NOCOUNT ON

	BEGIN TRY

		DECLARE @TableDataID INT

		SELECT @TableDataID = TableDataID FROM [Audit].[TableData]
		WHERE [Name] = @TableName AND [SchemaName] = @SchemaName

		IF(@TableDataID IS NULL)
		BEGIN
			RAISERROR('� ��������� ���� �������� ��� ������� � ��� ����� �������������� � Audit.TableData',19,1)
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
	-- ��������� ��� ���������� ��������� � ������ � ������� Audit.LogChanges
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
		-- ������� � ��������� ����� ��� ������������ �� ��������� � ��������
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

-- ������� ���������������: ������ �� �������� ������� Login
-- ������� ��� �������� ���������� ������������
--CREATE FUNCTION [Authorization].[sp_IsValidUser]
--(@LoginID INT)
--RETURNS BIT
--AS
--BEGIN
--	-- ������� ����������� �������� �� ������� ������������ ��������
--	-- ����������� ������� ���� � ����� ������������� ������� ���� � ��� �� ����� � ������ �������� 0
--	DECLARE @UsersCountWithSearchLogin INT

--	SELECT @UsersCountWithSearchLogin = COUNT(*) FROM [Authorization].[User]
--	WHERE [LoginID] = @LoginID AND [SysIsDeleted] = 0
	
--	IF(@UsersCountWithSearchLogin > 1)
--		RETURN 0
--	RETURN 1
--END
--GO

---- ��������� ������������ �������� 
--ALTER TABLE [Authorization].[User] ADD CONSTRAINT CK_User_LoginID CHECK ([Authorization].[sp_IsValidUser]([LoginID]) = 1)

-- ***********************************************************************
-- ������ ��� �������� �������� ������� ����������
-- ��������� ��������� ������ � ������� Audit.LogChanges
-- ��������� ��� ������ ���� ����, ����� Audit � dbo
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

DECLARE @PrintTemplate NVARCHAR(MAX) = 'PRINT ''������ ������� [#SCHEMA].[#TABLE_InsteadOfUpdate]'''

EXEC [Audit].uspDeleteMeAfterUsing @Template, @PrintTemplate

GO

-- ***********************************************************************
-- ������ ��� �������� �������� ����� �������
-- ��������� ��� � ����� ������ � ������� Audit.Log
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

DECLARE @PrintTemplate NVARCHAR(MAX) = 'PRINT ''������ ������� [#SCHEMA].[#TABLE_AfterInsert]'''

EXEC [Audit].uspDeleteMeAfterUsing @Template, @PrintTemplate

GO

-- ***********************************************************************
-- ������ ��� �������� �������� ������ ��������
-- ��������� ��� �� ��������
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
DECLARE @PrintTemplate NVARCHAR(MAX) = 'PRINT '' ������ ������� [#SCHEMA].[#TABLE_AfterInsert]'''

EXEC [Audit].uspDeleteMeAfterUsing @Template, @PrintTemplate

GO

CREATE PROCEDURE [Audit].[uspAddTableColumnData]
(@SchemaName NVARCHAR(256),
@TableName NVARCHAR(256),
@ColumnName NVARCHAR(256),
@ColumnRusName NVARCHAR(256))
AS
BEGIN
	-- ��������� ��� �������� ���������� ������ � ������� TableColumnData �� ����� ������� � �����
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
		SET @Error = CONCAT(@SchemaName,'.',@TableName, ' �� ���������� � ������� [Audit].[TableData]')
		RAISERROR(@Error, 19, 1)
	END

	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @SchemaName AND COLUMN_NAME = @ColumnName)
	BEGIN
		SET @Error = CONCAT(@ColumnName, ' �� ���������� � ������� ', @SchemaName, '.', @TableName)
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
-- ��� �������� ��������� ���������� � �������� � ��������
-- ��� ��������� ������ � ������
-- ***********************************************************************

-- Current Table Name
DECLARE @ctn NVARCHAR(256) 
-- Current Schema Name
DECLARE @csn NVARCHAR(256)

SET @csn = 'dbo'
SET @ctn = 'File'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '����')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'FileID', '������������� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'GUID', 'GUID'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Data', '����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ContentType', '��� ��������'

SET @csn = 'Person'
SET @ctn = 'Address'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�����')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'AddressID', '������������� ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Country', '������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Region', '������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'City', '�����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'District', '�����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Street', '�����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BuildingNumber', '����� ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Corpus', '������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Floor', '����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Flat', '��������/����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'PostalIndex', '�������� ������'

SET @csn = 'HumanResources'
SET @ctn = 'BusinessEntityType'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '��� ������-��������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityTypeID', '������������� ���� ������-��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� ����'

SET @csn = 'HumanResources'
SET @ctn = 'BusinessEntity'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '������-��������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityID', '������������� ������-��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityTypeID', '������������� ���� ������-��������'

SET @csn = 'HumanResources'
SET @ctn = 'Organization'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�����������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityID', '������������� ������-��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', '������������� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'FullName', '������ ������������ �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ShortName', '����������� ������������ �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessAddressID', '����������� �����/����� ������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'INN', '���'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KPP', '���'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OKPO', '����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OGRN', '����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateOfAssignmentOGRN', '���� ���������� ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DirectorFullName', '��� ���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ChiefAccountantFullName', '��� ��.���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OKVED', '�������� ��� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'IsOwnerJournalAccountingCPI', '������ ��������� ������� ����� ����'

SET @csn = 'Person'
SET @ctn = 'ContactType'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '��� ��������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ContactTypeID', '������������� ���� ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� ���� ��������'

SET @csn = 'HumanResources'
SET @ctn = 'OrganizationContact'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�������� �����������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationContactID', '������������� �������� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', '������������� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ContactTypeID', '������������� ���� ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Value', '�������� ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', '�������'

SET @csn = 'Authorization'
SET @ctn = 'UserRole'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '���� ������������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserRoleID', '������������� ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� ����'

SET @csn = 'Authorization'
SET @ctn = 'RightGroup'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '������ ����')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RightGroupID', '������������� ������ ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Code', '��� ������'

SET @csn = 'Authorization'
SET @ctn = 'Right'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�����')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RightID', '������������� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RightGroupID', '������������� ������ ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Code', '��� �����'

SET @csn = 'Authorization'
SET @ctn = 'UserRoleRight'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '����� ���� ������������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserRoleRightID', '������������� ����� ���� ������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserRoleID', '������������� ���� ������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RightID', '������������� �����'

--SET @csn = 'Authorization'
--SET @ctn = 'Login'
--INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�����')
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'LoginID', '������������� ������'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� ������'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Password', '������ ������'

SET @csn = 'Authentication'
SET @ctn = 'User'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '������������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserID', '������������� ������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Login', '�����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserRoleID', '������������� ���� ������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'PasswordHash', '��� ������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'PasswordSalt', '���� ���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'LastLoginDate', '��������� ���� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'TotpSecretKey', '������������� ��������� ���� ��� TOTP ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'TwoFactorEnabled', '������ ������������� ������������'

--SET @csn = 'Authentication'
--SET @ctn = 'ResultType'
--INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '��� ���������� ������������')
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ResultTypeID', '������������� ���� ���������� ������������'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� ����'

SET @csn = 'HumanResources'
SET @ctn = 'EmployeePosition'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '��������� ����������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'EmployeePositionID', '������������� ��������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '�������� ���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', '�������'

SET @csn = 'HumanResources'
SET @ctn = 'Employee'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '���������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'BusinessEntityID', '������������� ������-��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'EmployeeID', '������������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'FirstName', '������ ����� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'MiddleName', '������ ����� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'LastName', '��������� ����� �����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'EmployeePositionID', '������������� ��������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', '������������� �����������'

--SET @csn = 'AccountingCPI'
--SET @ctn = 'Subject'
--INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '������� ������� ����� ����')
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SubjectID', '������������� ��������'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', '������������� �����������'
--EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'EmployeeID', '������������� ����������'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCPARecord'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '������ ������ ��������������� ����� ���� ��� ������ ����������������� ������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', '������������� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCPARecordID', '������������� ������ � ������� ��������������� ����� ���� ��� ������ ����������������� ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'NameCPI', '������������ ����, ���������������� � ����������� ������������ � ���, �������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialCPI', '�������� ����� ����, ���������������� � ����������� ������������ � ���, ������ ����� �������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstanceNumber', '����� ���������� (����������������� �����) �������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ReceivedFromID', '������� �� �������� �������� ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberCoverLetterReceive', '���� � ����� ����������������� ������ ��� ���� ������������ �������� ���������� � �������� � ������������ � ������� � ���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberCoverLetterSend', '���� � ����� ����������������� ������ � ������� � ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberConfirmationSend', '���� � ����� ������������� ��� �������� � ��������� � ������� � ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberCoverLetterReturn', '���� � ����� ������������� ��� �������� � ��������� � ������� � ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberConfirmationReturn', '���� � ����� ������������� � ������� � ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'CommissioningDate', '���� ����� � ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DecommissioningDate', '���� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionDate', '���� ������ �� ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionActNumber', '����� ���� ��� �������� �� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', '����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', '������������� ����� �������'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceCPAReceiver'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�������� ������� ��������������� ����� ���� ��� ������ ����������������� ������ ������� ���� ��������� ����������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceCPAReceiverID', '������������� c������� ������� ��������������� ����� ���� ��� ������ ����������������� ������ �������� ���� ��������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordID', '������������� ������ ������� ��������������� ����� ���� ��� ������ ����������������� ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ReceiverID', '������������� �������� ������� ����� ����'

SET @csn = 'Office'
SET @ctn = 'Hardware'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '����������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', '������������� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'HardwareID', '������������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialNumber', '���������� ����������� ����� ��� ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', '�������'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHRecord'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '������ ������� ��������������� ����� ���� ��� ���������� ���������������� ����������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', '������������� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCIHRecordID', '������������� ������ ������� ��������������� ����� ���� ��� ���������� ���������������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'NameCPI', '������������ ����, ���������������� � ����������� ������������ � ���, �������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialCPI', '�������� ����� ����, ���������������� � ����������� ������������ � ���, ������ ����� �������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstanceNumber', '����� ���������� (����������������� �����) �������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ReceivedFromID', '������� �� �������� �������� ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberCoverLetterReceive', '���� � ����� ����������������� ������ ��� ���� ������������ �������� ���������� � �������� � ������������ � ������� � ���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'CPIUserID', '������������� �������� (������������) ����������� ���� � ������� � ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DateAndNumberConfirmationIssue', '���� � �������� � ��������� � ������� � ������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstallationDateAndConfirmation', '���� ����������� (���������) � ������� ���, ����������� ����������� (���������) � ������� � ����������� (��������� ����)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionDate', '���� ������� (�����������) � ������� �� ������� ���� �� ���������� �������, ����������� �������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionActNumber', '����� ���� ��� �������� �� ����������� � ������� �� ������� ���� �� ���������� �������, ����������� �������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', '����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', '������������� ����� �������'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHInstaller'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�.�.�. ����������� ������ ����������������� ������, ������������ ����, ����������� ����������� (���������)')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCIHInstallerID', '�������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordID', '������������� ������ ������� ��������������� ����� ���� ��� ���������� ���������������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'InstallerID', '������������� �������� ������� ����� ���� �������������� ��������� ����'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHConnectedHardware'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '������ ���������� �������, � ������� ����������� ��� � ������� ���������� ����')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCIHConnectedHardwareID', '�������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordID', '������������� ������ ������� ��������������� ����� ���� ��� ���������� ���������������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'HardwareID', '������������� �������� � ������� ���� ����������� ��� ���������� ����'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalInstanceForCIHDestructor'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�.�.�. ����������� ������ ����������������� ������, ������������ ����, ������������� ������� (�����������)')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalInstanceForCIHDestructorID', '�������������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordID', '������������� ������ ������� ��������������� ����� ���� ��� ���������� ���������������� ����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructorID', '������������� �������� ������� ����� ���� ������������� ������� (�����������) ����'

SET @csn = 'AccountingCPI'
SET @ctn = 'KeyDocumentType'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '��� ��������� ���������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KeyDocumentTypeID', '������������� ���� ��������� ���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', '�������'

SET @csn = 'AccountingCPI'
SET @ctn = 'KeyHolderType'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '��� ��������� ��������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KeyHolderTypeID', '������������� ���� ��������� ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Name', '��������'

SET @csn = 'AccountingCPI'
SET @ctn = 'KeyHolder'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '�������� ��������')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KeyHolderID', '������������� ��������� ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialNumber', '�������� ����� ��������� ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'TypeID', '������������� ���� ��������� ��������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'UserCPI', '������������ ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', '������������� ����� �������'

SET @csn = 'AccountingCPI'
SET @ctn = 'JournalTechnicalRecord'
INSERT INTO [Audit].[TableData] VALUES (@csn, @ctn, '������ ������� ����������� (����������)')
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'OrganizationID', '������������� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'JournalTechnicalRecordID', '������������� ������ ������� ����������� (����������)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted', '����� �������� ��������� �������� ��� ���� ����, � ������� ������� �����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Date', '����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'TypeAndSerialUsedCPI', '��� � �������� ������ ������������ ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'RecordOnMaintenanceCPI', '������ �� ������������ ����'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'KeyDocumentTypeID', '������������� ���� ��������� ���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SerialCPIAndKeyDocumentInstanceNumber', '��������, ����������������� ����� � ����� ���������� ��������� ���������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'DestructionDate', '���� ����������� (��������)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'ActNumber', '����� ���� �� ����������� (��������)'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'Note', '����������'
EXEC [Audit].[uspAddTableColumnData] @csn, @ctn, 'SignFileID', '������������� ����� �������'

GO

-- ***********************************************************************
-- �������� ����, ��� �� �������� ��� �������
-- ���� ���-�� ������, �� �� ���������� ���������� � ������� ���������� � �������
-- ������� ���� ��� ��������
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
	PRINT CONCAT('[������] ������� ', @SchemaName, '.', @TableName, ' ����������� � ������� [Audit].[TableData]')

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
-- �������� ����, ��� �� �������� ��� ������� ��� ��������� ������
-- ���� ���-�� ������, �� �� ���������� ���������� � ������� ���������� � �������
-- ������� ���� ��� ��������
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
	PRINT CONCAT('[������] ��� ������� ', @SchemaName, '.', @TableName, ' ����������� ������� ', @ColumnName, ' � ������� [Audit].[TableColumnData]')

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