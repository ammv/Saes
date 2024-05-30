Scaffold-DbContext `
-Connection "Data Source=KOMPUTER\SQLEXPRESS;Initial Catalog=SAES;Integrated Security=True;Connect Timeout=400;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False" `
-Provider Microsoft.EntityFrameworkCore.SqlServer `
-Context SaesContext `
-Project Saes.Models `
-StartupProject Saes.Models `
-DataAnnotations -Force
