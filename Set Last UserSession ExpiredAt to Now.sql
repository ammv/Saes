use [SAES]

declare @id int = (select top 1 [UserSessionID] from [Authorization].[UserSession]
WHERE [IsExpired] = 0 order by  [UserSessionID] desc)

UPDATE [Authorization].[UserSession] SET [ExpiredAt] = GETDATE() WHERE [UserSessionID] = @id

print 'Срок истекания сесси изменен'