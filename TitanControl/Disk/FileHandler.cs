using Json.Schema;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TitanControl.Disk.Model.Session;
using TitanControl.Disk.Model.Workspace;
using TitanControl.Helper;
using TitanControl.Logging;
using TitanControl.Session;
using TitanControl.Session.Interface;

namespace TitanControl.Disk
{
    public static class FileHandler
    {
        private const string LoggingCategory = "FileHandler";

        private const string SessionRecord = "sessions.json";
        private const string WorkspaceRecord = "workspacesRecord.json";
        private const string ConfigRecord = "config.json";

        private static string SavePath = PathHelper.DocumentsPath;
        private static string WorkspaceRecordPath = Path.Combine(PathHelper.AppDataPath, WorkspaceRecord);
        private static string SessionsPath = Path.Combine(PathHelper.AppDataPath, SessionRecord);
        private static string ConfigPath = Path.Combine(PathHelper.BasePath, ConfigRecord);

        private static JsonSchema WorkspaceSchema = null!;
        private static JsonSchema WorkspaceRecordSchema = null!;
        private static JsonSchema SessionsSchema = null!;

        public static void Initialize()
        {
            Log.Information("Initializing disk operations...", LoggingCategory);
            EnsureDirectoriesExist();
            EnsureFilesExist();


            Log.Information("Loading json schemas...", LoggingCategory);
            WorkspaceSchema = SchemaLoader.Load("workspace");
            WorkspaceRecordSchema = SchemaLoader.Load("workspaces");
            SessionsSchema = SchemaLoader.Load("sessions");
        }

        private static void EnsureDirectoriesExist()
        {
            Log.Information("Ensuring directories exist...", LoggingCategory);
            Directory.CreateDirectory(SavePath);
            Directory.CreateDirectory(PathHelper.AppDataPath);
        }

        private static void EnsureFilesExist()
        {
            if (!File.Exists(WorkspaceRecordPath))
            {
                _ = SaveWorkspaceRecord(new WorkspaceRecordModel());
                Log.Information($"Workspace record was not found therefore re-written to: {WorkspaceRecordPath}", LoggingCategory);
            }

            if (!File.Exists(SessionsPath))
            {
                _ = SaveSessions(new());
                Log.Information($"Sessions was not found therefore re-written to: {SessionsPath}", LoggingCategory);
            }

            if (!File.Exists(ConfigPath))
            {
                File.Create(ConfigPath);
                Log.Information($"Config was not found therefore re-written to: {ConfigPath}", LoggingCategory);
            }
        }

        public static async Task<List<TitanSession>> LoadSessions()
        {
            string raw = await File.ReadAllTextAsync(SessionsPath);
            return JsonModelLoader.ParseAndValidate<List<TitanSession>>(raw, SessionsSchema);
        }

        public static async Task SaveSessions(List<TitanSession> sessions)
        {
            string raw = JsonModelLoader.Serialize(sessions);
            await File.WriteAllTextAsync(SessionsPath, raw);
            Log.Information($"Sessions record saved.", LoggingCategory);
        }

        public static async Task<WorkspaceRecordModel> LoadWorkspaceRecord()
        {
            string raw = await File.ReadAllTextAsync(WorkspaceRecordPath);
            var recordModel = JsonModelLoader.ParseAndValidate<WorkspaceRecordModel>(raw, WorkspaceRecordSchema);
            Log.Information("Sucessfully loaded workspace record", LoggingCategory);
            Log.Debug($"\n{raw}", "JsonObject");
            return recordModel;
        }

        public static async Task<WorkspaceModel> LoadWorkspace(string path)
        {
            if (!File.Exists(path))
            {
                Log.Error($"Could not read {path}. File doesn't exist");
                throw new System.Exception("Invalid path");
            }

            string raw = await File.ReadAllTextAsync(path);
            return JsonModelLoader.ParseAndValidate<WorkspaceModel>(raw, WorkspaceSchema);
        }

        public static async Task SaveWorkspace(WorkspaceModel workspace, string path)
        {
            string raw = JsonModelLoader.Serialize(workspace);
            await File.WriteAllTextAsync(path, raw);
            Log.Information($"Workspace '{workspace.Name}' saved to: {path}", LoggingCategory);
        }

        public static async Task SaveWorkspaceRecord(WorkspaceRecordModel record)
        {
            string raw = JsonModelLoader.Serialize(record);
            await File.WriteAllTextAsync(WorkspaceRecordPath, raw);
            Log.Information($"Workspace record saved.", LoggingCategory);
        }
    }
}
