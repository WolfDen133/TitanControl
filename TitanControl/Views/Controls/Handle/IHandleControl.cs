using System.Drawing;
using System.Threading.Tasks;
using TitanControl.Models.Control;
using TitanControl.WebAPI.Data;

namespace TitanControl.Views.Controls.Handle
{
    public interface IHandleControl
    {
        ControlId ControlId { get; }
        Rectangle Location { get; set; }
        int TitanId { get; }
        HandleType HandleType { get; set; }
        KeyProfile KeyProfile { get; set; }

        bool IsSelected { get; set; }
        bool IsMoving { get; set; }

        Task ExecuteAsync();
    }

    public interface IHandleControl<TModel> : IHandleControl
        where TModel : ControlModel
    {
        TModel Model { get; }
    }
}
