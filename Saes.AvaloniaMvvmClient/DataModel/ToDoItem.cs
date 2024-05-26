using System;

namespace Saes.AvaloniaMvvmClient.DataModel
{
    public class ToDoItem
    {
        public string Description { get; set; } = String.Empty;
        public bool IsChecked { get; set; }
    }
}