using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Views.Controls.Toolbar.Button;

namespace TitanControl.ViewModels.Controls.Toolbar
{
    public partial class ToolbarButtonModel : ObservableObject
    {
        public bool _toggled = false;
        public bool _isAvailable = false;

        public ToolbarButtonModel(ButtonId id, FlowDirection flow = FlowDirection.LeftToRight)
        {
            Id = id;
            Flow = flow;
        }

        public bool Toggled
        {
            get => _toggled;
            set
            {
                SetProperty(ref _toggled, value);
                OnPropertyChanged(nameof(Icon));
                OnPropertyChanged(nameof(Text));
            }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set => SetProperty(ref _isAvailable, value);
        }

        public string Text => Id switch
        {
            ButtonId.Back => "Back",
            ButtonId.Add => "Add",
            ButtonId.AddButton => "Button",
            ButtonId.AddFader => "Fader",
            ButtonId.AddColorPicker => "Color P.",
            ButtonId.Copy => "Copy",
            ButtonId.Move => "Move",
            ButtonId.Assign => "Assign",
            ButtonId.Options => "Options",
            ButtonId.Remove => "Remove",
            ButtonId.Config => "Config",
            ButtonId.Fullscreen => Toggled ? "Windowed" : "Fullscreen",
            ButtonId.Sessions => "Sessions",
            ButtonId.Windows => "Windows",
            ButtonId.CircleBuilder => "Circles",
            ButtonId.Disk => "Disk",
            ButtonId.Save => "Save",
            ButtonId.SaveAs => "Save As",
            ButtonId.Load => "Load",
            ButtonId.Rename => "Rename",
            ButtonId.New => "New",
            ButtonId.Latch => Toggled ? "Latched" : "Latch",
            _ => "Toolbutton"
        };

        public string Icon => Id switch {
            ButtonId.Back => "/Assets/Icons/arrow.svg",
            ButtonId.Add => "/Assets/Icons/add-control.svg",
            ButtonId.AddButton => "button.png",
            ButtonId.AddFader => "fader.png",
            ButtonId.AddColorPicker => "colorpicker.png",
            ButtonId.Copy => "/Assets/Icons/copy.svg",
            ButtonId.Move => "/Assets/Icons/move.svg",
            ButtonId.Assign => "/Assets/Icons/trigger.svg",
            ButtonId.Options => "/Assets/Icons/options.svg",
            ButtonId.Remove => "/Assets/Icons/remove.svg",
            ButtonId.Config => "/Assets/Icons/cog.svg",
            ButtonId.Fullscreen => Toggled ? "/Assets/Icons/contract.svg" : "/Assets/Icons/expand.svg",
            ButtonId.Sessions => "/Assets/Icons/session.svg",
            ButtonId.Windows => "/Assets/Icons/window.svg",
            ButtonId.CircleBuilder => "/Assets/Icons/circle.svg",
            ButtonId.Disk => "/Assets/Icons/disk.svg",
            ButtonId.Save => "/Assets/Icons/save.svg",
            ButtonId.SaveAs => "/Assets/Icons/save-as.svg",
            ButtonId.Load => "/Assets/Icons/load.svg",
            ButtonId.Rename => "/Assets/Icons/rename.svg",
            ButtonId.New => "/Assets/Icons/new.svg",
            ButtonId.Latch => Toggled ? "/Assets/Icons/lock.svg" : "/Assets/Icons/unlock.svg",
            _ => "/Assets/Icons/question-circle.svg"
        };

        // TODO Tooltip
        public string Tooltip => string.Empty;

        public bool IsToggle { get; init; } = false;
        public ObservableCollection<ButtonId> Children { get; set; } = new();
        public ButtonId Id { get; }
        public FlowDirection Flow { get; }
    }
}
