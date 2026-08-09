using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TitanControl.Services.Data;
using TitanControl.Services.Interface;
using TitanControl.Views;

namespace TitanControl.Services
{
    public class DialogService : IDialogService
    {
        private Window _parent;

        public DialogService(Window parent) 
        {
            _parent = parent;
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var dialog = new ModalDialogWindow
            {
                Title = $"{AppConstants.AppName} - {title}",
                Heading = title,
                Text = message
            };

            return await dialog.ShowDialog<bool>(_parent);
        }
    }
}
