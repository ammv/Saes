using Avalonia.Data;
using Avalonia.Data.Converters;
using Saes.AvaloniaMvvmClient.Core.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Converters
{
    public class FormModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is FormMode mode)
            {
                switch(mode)
                {
                    case FormMode.See:
                        return "Просмотреть";
                    case FormMode.Add:
                        return "Добавить";
                    case FormMode.Edit:
                        return "Изменить";
                    default:
                        throw new ArgumentException($"Неизвестный режим - {mode}");
                }
            }
            if(value == null)
            {
                return BindingOperations.DoNothing;
            }
            return new BindingNotification(new InvalidCastException($"Cant casting {value.GetType().Name} to FormMode"), BindingErrorType.Error);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
