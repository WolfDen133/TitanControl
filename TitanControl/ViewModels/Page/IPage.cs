using System.Threading.Tasks;

namespace TitanControl.ViewModels.Page
{
    public interface IPage
    {
        public PageId Id { get; }

        Task InitializeAsync();
    }
}
