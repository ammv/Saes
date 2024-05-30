using Saes.AvaloniaMvvmClient.Core;
using Saes.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Injections
{
    public static class ConfigureExcelExporterConfig
    {
        public static void Configure(ExcelExporterConfig excelExporterConfig)
        {
            if(excelExporterConfig == null)
            {
                throw new ArgumentNullException(nameof(excelExporterConfig));
            }
            excelExporterConfig.AddConfig<UserDto>(
                (x) =>
                {
                    var userDto = (UserDto)x;
                    return [userDto.UserId, userDto.Login, userDto.UserRoleDto.Name, userDto.LastLoginDate.ToDateTime().ToLocalTime()];
                },
                    ["№ п/п", "Логин", "Роль", "Последняя дата входа"]
                    ,
                    "Пользователи"
                );
        }
    }
}
