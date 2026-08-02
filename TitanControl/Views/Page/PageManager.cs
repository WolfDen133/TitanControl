using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Views.Page.Pages;

namespace TitanControl.Views.Page
{
    public class PageManager
    {
        public const string LoggingCategory = "PageManager";

        private Dictionary<PageId, BasePage> _pages = new();
        private Grid _root = null!;
        public PageId CurrentPage { get; private set; } = PageId.None;

        public event EventHandler<BasePage> OnPageChanged = null!;

        public void Initialize(Grid root)
        {
            _root = root;
            RegisterPages();
            LoadPages();
        }
        
        public void RegisterPages()
        {
            RegisterPage(new WorkspacePage());
            RegisterPage(new SessionPage());
        }

        private void RegisterPage(BasePage page)
        {
            if (_pages.ContainsKey(page.Id))
            {
                Log.Error($"Page {page.Id} has already been registered.", LoggingCategory);
                return;
            }

            _pages.Add(page.Id, page);
            page.OnRegister();
        }


        public void LoadPages()
        {
            if (_pages.Count <= 0)
            {
                Log.Warning("No pages registered, skipping loading.", LoggingCategory);
                return;
            }

            foreach (var page in _pages.Values)
            {
                _root.Children.Add(page);

                page.IsVisible = false;
            }
        }

        public void ShowPage(PageId pageId)
        {
            if (CurrentPage == pageId)
            {
                Log.Information($"{pageId} page is already being displayed.", LoggingCategory);
                return;
            }

            if (!_pages.TryGetValue(pageId, out var page))
            {
                Log.Error($"Could not find page {pageId}.", LoggingCategory);
                return;
            }

            foreach(var p in _pages)
            {
                if (p.Value.IsVisible && p.Value.Id != page.Id)
                {
                    p.Value.IsVisible = false;
                    p.Value.OnHide();
                }
            }

            CurrentPage = pageId;
            page.IsVisible = true;
            page.OnShow();

            OnPageChanged?.Invoke(null, page);
        }

        public bool TryGetPage(PageId id, out BasePage? page)
        {
            return _pages.TryGetValue(id, out page);
        }
    }
}
