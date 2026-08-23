namespace TitanControl.Models.Workspace
{
    public class WorkspaceEntryModel : ISaveModel
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
