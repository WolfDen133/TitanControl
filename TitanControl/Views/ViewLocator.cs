using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TitanControl.Controls.Handle;
using TitanControl.Controls.Models.Handle;
using TitanControl.ViewModels;
using TitanControl.ViewModels.Page;
using TitanControl.Views.Page.Pages;
using TitanControl.Views.Pages;

namespace TitanControl.Views
{
    public class ViewLocator : IDataTemplate
    {
        public Dictionary<Type, Type> PageMap = new();

        public ViewLocator()
        {
            PageMap.Add(typeof(WorkspacePageModel), typeof(WorkspacePage));
            PageMap.Add(typeof(SessionPageModel), typeof(SessionPage));
        }

        public Control? Build(object? data)
        {
            if (data is not object) return new TextBlock { Text = "No view data provided" };

            if (!PageMap.TryGetValue(data.GetType(), out Type? type))
            {
                return new TextBlock { Text = $"View not found: {data.GetType()}" };
            }

            return (Control)Activator.CreateInstance(type)!;
        }

        public bool Match(object? data)
        {
            return data is ObservableObject;
        }
    }
}
