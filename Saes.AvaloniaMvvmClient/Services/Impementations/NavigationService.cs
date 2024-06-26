﻿using ReactiveUI;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Services.Impementations
{
    public class NavigationService : ReactiveObject, INavigationService
    {
        public event EventHandler<ViewModelBase> Navigated;
        public event EventHandler<ViewModelBase> Navigating;

        private ViewModelBase _content;
        public ViewModelBase Content => _content;

        public void NavigateTo(ViewModelBase content)
        {
            Navigating?.Invoke(this, _content);
            this.RaiseAndSetIfChanged(ref _content, content, nameof(Content));
            Navigated?.Invoke(this, _content);
        }
    }
}
