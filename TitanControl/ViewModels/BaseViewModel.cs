using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using TitanControl.ViewModels;
using TitanControl.ViewModels.Page;

namespace TitanControl.ViewModel
{
    public abstract class BaseViewModel : ObservableObject, IViewModel
    {
        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
