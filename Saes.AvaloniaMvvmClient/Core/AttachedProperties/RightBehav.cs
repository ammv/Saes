using Avalonia;
using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Saes.AvaloniaMvvmClient.Core.AttachedProperties
{
    public class RightBehav: AvaloniaObject
    {
        public static readonly AttachedProperty<string> RightCodeProperty = AvaloniaProperty.RegisterAttached<RightBehav, string>(
            "RightCode", typeof(string), default);

        /// <summary>
        /// Accessor for Attached property <see cref="CommandProperty"/>.
        /// </summary>
        public static void SetRightCode(AvaloniaObject element, string rightCode)
        {
            element.SetValue(RightCodeProperty, rightCode);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="CommandProperty"/>.
        /// </summary>
        public static string GetRightCode(AvaloniaObject element)
        {
            return element.GetValue(RightCodeProperty);
        }

    }
}
