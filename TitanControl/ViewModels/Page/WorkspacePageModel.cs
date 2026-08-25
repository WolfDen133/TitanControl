using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Models.Control;
using TitanControl.Models.Workspace;
using TitanControl.Services.Session;
using TitanControl.Services.Workspace;
using TitanControl.ViewModel;
using TitanControl.ViewModels.Controls.Handle;
using TitanControl.Views.Controls.Handle;

namespace TitanControl.ViewModels.Page
{
    public class WorkspacePageModel : BaseViewModel, IPage
    {
        private const string LoggingCategory = "Workspace ViewModel";

        private readonly IWorkspaceService _workspaceService;
        private readonly ISessionService _sessionService;

        public PageId Id => PageId.Workspace;

        public ObservableCollection<IHandleControl> Controls { get; } = [];

        public WorkspaceModel CurrentWorkspace => _workspaceService.CurrentWorkspace;


        public WorkspacePageModel(IWorkspaceService workspaceService, ISessionService sessionService)
        {
            _workspaceService = workspaceService;
            _sessionService = sessionService;
        }

        public async Task InitializeAsync()
        {

            // 1 — Large green GO button
            // 1
            var buttonModel1 = new ButtonControlModel()
            {
                Location = new(0, 0, 4, 3),
                KeyProfile = KeyProfile.Go,
            };

            var button1 = new HandleButtonModel(buttonModel1, _sessionService)
            {
                Color = "#22C55E",
                Legend = "Main Go"
            };


            // 2
            var buttonModel2 = new ButtonControlModel()
            {
                Location = new(4, 0, 3, 3),
                KeyProfile = KeyProfile.Flash,
            };

            var button2 = new HandleButtonModel(buttonModel2, _sessionService)
            {
                Color = "#EF4444",
                Legend = "Bump In"
            };


            // 3
            var buttonModel3 = new ButtonControlModel()
            {
                Location = new(7, 0, 5, 2),
                KeyProfile = KeyProfile.Go,
            };

            var button3 = new HandleButtonModel(buttonModel3, _sessionService)
            {
                Color = "#3B82F6",
                Legend = "Stage Wash"
            };


            // 4
            var buttonModel4 = new ButtonControlModel()
            {
                Location = new(12, 0, 3, 2),
                KeyProfile = KeyProfile.Flash,
            };

            var button4 = new HandleButtonModel(buttonModel4, _sessionService)
            {
                Color = "#F59E0B",
                Legend = "Strobe"
            };


            // 5
            var buttonModel5 = new ButtonControlModel()
            {
                Location = new(15, 0, 3, 4),
                KeyProfile = KeyProfile.Go,
            };

            var button5 = new HandleButtonModel(buttonModel5, _sessionService)
            {
                Color = "#A855F7",
                Legend = "Colour Chase"
            };


            // 6
            var buttonModel6 = new ButtonControlModel()
            {
                Location = new(7, 2, 3, 3),
                KeyProfile = KeyProfile.Flash,
            };

            var button6 = new HandleButtonModel(buttonModel6, _sessionService)
            {
                Color = "#06B6D4",
                Legend = "Audience Hit"
            };


            // 7
            var buttonModel7 = new ButtonControlModel()
            {
                Location = new(10, 2, 5, 2),
                KeyProfile = KeyProfile.Go,
            };

            var button7 = new HandleButtonModel(buttonModel7, _sessionService)
            {
                Color = "#EC4899",
                Legend = "Pink Look"
            };


            // 8
            var buttonModel8 = new ButtonControlModel()
            {
                Location = new(0, 3, 4, 3),
                KeyProfile = KeyProfile.Go,
            };

            var button8 = new HandleButtonModel(buttonModel8, _sessionService)
            {
                Color = "#14B8A6",
                Legend = "Step CW"
            };


            // 9
            var buttonModel9 = new ButtonControlModel()
            {
                Location = new(4, 3, 3, 2),
                KeyProfile = KeyProfile.Flash,
            };

            var button9 = new HandleButtonModel(buttonModel9, _sessionService)
            {
                Color = "#F97316",
                Legend = "Warm Bump"
            };


            // 10
            var buttonModel10 = new ButtonControlModel()
            {
                Location = new(10, 4, 4, 4),
                KeyProfile = KeyProfile.Go,
            };

            var button10 = new HandleButtonModel(buttonModel10, _sessionService)
            {
                Color = "#6366F1",
                Legend = "Finale Build"
            };


            // 11
            var buttonModel11 = new ButtonControlModel()
            {
                Location = new(14, 4, 4, 3),
                KeyProfile = KeyProfile.Flash,
            };

            var button11 = new HandleButtonModel(buttonModel11, _sessionService)
            {
                Color = "#EAB308",
                Legend = "Blinders"
            };


            // 12
            var buttonModel12 = new ButtonControlModel()
            {
                Location = new(4, 5, 3, 4),
                KeyProfile = KeyProfile.Go,
            };

            var button12 = new HandleButtonModel(buttonModel12, _sessionService)
            {
                Color = "#84CC16",
                Legend = "Green Chase"
            };


            // 13
            var buttonModel13 = new ButtonControlModel()
            {
                Location = new(7, 5, 3, 3),
                KeyProfile = KeyProfile.Flash,
            };

            var button13 = new HandleButtonModel(buttonModel13, _sessionService)
            {
                Color = "#DC2626",
                Legend = "Blackout"
            };


            // 14
            var buttonModel14 = new ButtonControlModel()
            {
                Location = new(0, 6, 4, 3),
                KeyProfile = KeyProfile.Go,
            };

            var button14 = new HandleButtonModel(buttonModel14, _sessionService)
            {
                Color = "#0EA5E9",
                Legend = "Full Stage"
            };


            // 15
            var buttonModel15 = new ButtonControlModel()
            {
                Location = new(14, 7, 4, 4),
                KeyProfile = KeyProfile.Go,
            };

            var button15 = new HandleButtonModel(buttonModel15, _sessionService)
            {
                Color = "#8B5CF6",
                Legend = "Finale"
            };


            // 16
            var buttonModel16 = new ButtonControlModel()
            {
                Location = new(7, 8, 4, 2),
                KeyProfile = KeyProfile.Flash,
            };

            var button16 = new HandleButtonModel(buttonModel16, _sessionService)
            {
                Color = "#94A3B8",
                Legend = "White Hit"
            };


            // 17
            var buttonModel17 = new ButtonControlModel()
            {
                Location = new(11, 8, 3, 3),
                KeyProfile = KeyProfile.Go,
            };

            var button17 = new HandleButtonModel(buttonModel17, _sessionService)
            {
                Color = "#10B981",
                Legend = "House Look"
            };


            // 18
            var buttonModel18 = new ButtonControlModel()
            {
                Location = new(0, 9, 5, 4),
                KeyProfile = KeyProfile.Go,
            };

            var button18 = new HandleButtonModel(buttonModel18, _sessionService)
            {
                Color = "#2563EB",
                Legend = "Opening Look"
            };


            // 19
            var buttonModel19 = new ButtonControlModel()
            {
                Location = new(5, 9, 2, 3),
                KeyProfile = KeyProfile.Flash,
            };

            var button19 = new HandleButtonModel(buttonModel19, _sessionService)
            {
                Color = "#F43F5E",
                Legend = "Red Hit"
            };


            // 20
            var buttonModel20 = new ButtonControlModel()
            {
                Location = new(8, 10, 3, 3),
                KeyProfile = KeyProfile.Go,
            };

            var button20 = new HandleButtonModel(buttonModel20, _sessionService)
            {
                Color = "#D946EF",
                Legend = "Magenta Look"
            };


            // 21
            var buttonModel21 = new ButtonControlModel()
            {
                Location = new(11, 11, 4, 3),
                KeyProfile = KeyProfile.Go,
            };

            var button21 = new HandleButtonModel(buttonModel21, _sessionService)
            {
                Color = "#0891B2",
                Legend = "Cool Chase"
            };


            // 22
            var buttonModel22 = new ButtonControlModel()
            {
                Location = new(15, 11, 3, 4),
                KeyProfile = KeyProfile.Flash,
            };

            var button22 = new HandleButtonModel(buttonModel22, _sessionService)
            {
                Color = "#FACC15",
                Legend = "Audience Blind"
            };


            // 23
            var buttonModel23 = new ButtonControlModel()
            {
                Location = new(0, 13, 4, 5),
                KeyProfile = KeyProfile.Go,
            };

            var button23 = new HandleButtonModel(buttonModel23, _sessionService)
            {
                Color = "#16A34A",
                Legend = "Encore"
            };


            // 24
            var buttonModel24 = new ButtonControlModel()
            {
                Location = new(4, 12, 4, 3),
                KeyProfile = KeyProfile.Go,
            };

            var button24 = new HandleButtonModel(buttonModel24, _sessionService)
            {
                Color = "#7C3AED",
                Legend = "UV Look"
            };


            // 25
            var buttonModel25 = new ButtonControlModel()
            {
                Location = new(8, 13, 3, 5),
                KeyProfile = KeyProfile.Flash,
            };

            var button25 = new HandleButtonModel(buttonModel25, _sessionService)
            {
                Color = "#EA580C",
                Legend = "Fire Hit"
            };


            // 26
            var buttonModel26 = new ButtonControlModel()
            {
                Location = new(11, 14, 4, 4),
                KeyProfile = KeyProfile.Go,
            };

            var button26 = new HandleButtonModel(buttonModel26, _sessionService)
            {
                Color = "#0284C7",
                Legend = "Blue Finale"
            };


            // 27
            var buttonModel27 = new ButtonControlModel()
            {
                Location = new(15, 15, 3, 3),
                KeyProfile = KeyProfile.Flash,
            };

            var button27 = new HandleButtonModel(buttonModel27, _sessionService)
            {
                Color = "#E11D48",
                Legend = "Final Hit"
            };


            // 28
            var buttonModel28 = new ButtonControlModel()
            {
                Location = new(4, 15, 4, 3),
                KeyProfile = KeyProfile.Go,
            };

            var button28 = new HandleButtonModel(buttonModel28, _sessionService)
            {
                Color = "#0F766E",
                Legend = "Exit Look"
            };

            Controls.Add(button1);
            Controls.Add(button2);
            Controls.Add(button3);
            Controls.Add(button4);
            Controls.Add(button5);
            Controls.Add(button6);
            Controls.Add(button7);
            Controls.Add(button8);
            Controls.Add(button9);
            Controls.Add(button10);
            Controls.Add(button11);
            Controls.Add(button12);
            Controls.Add(button13);
            Controls.Add(button14);
            Controls.Add(button15);
            Controls.Add(button16);
            Controls.Add(button17);
            Controls.Add(button18);
            Controls.Add(button19);
            Controls.Add(button20);
            Controls.Add(button21);
            Controls.Add(button22);
            Controls.Add(button23);
            Controls.Add(button24);
            Controls.Add(button25);
            Controls.Add(button26);
            Controls.Add(button27);
            Controls.Add(button28);

            foreach (var model in CurrentWorkspace.Controls)
                Controls.Add((IHandleControl)model.ToInstance(_sessionService));

            Log.Information($"Loaded {Controls.Count} controls into workspace", LoggingCategory);
        }
    }
}
