using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data;
using TitanControl.WebAPI.Data.Model;

namespace TitanControl.ViewModels.Page.HandleBrowser
{
    public class HandleEntry : ISelectable
    {
        public required Handle Handle { get; init; }

        public bool IsSelected { get; set; } = false;
    }
}
