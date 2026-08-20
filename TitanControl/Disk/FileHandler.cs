using Json.Schema;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TitanControl.Disk.Interface;
using TitanControl.Disk.Model.Session;
using TitanControl.Disk.Model.Workspace;
using TitanControl.Helper;
using TitanControl.Logging;

namespace TitanControl.Disk
{
    public static class FileHandler
    {
        private const string LoggingCategory = "FileHandler";

        private const string SessionRecord = "sessionRecord.json";
        private const string WorkspaceRecord = "workspaceRecord.json";
        private const string ConfigRecord = "config.json";

        private static readonly string SavePath = PathHelper.DocumentsPath;
        private static readonly string WorkspaceRecordPath = Path.Combine(PathHelper.AppDataPath, WorkspaceRecord);
        private static readonly string SessionsPath = Path.Combine(PathHelper.AppDataPath, SessionRecord);
        private static readonly string ConfigPath = Path.Combine(PathHelper.BasePath, ConfigRecord);

        private static readonly StringComparer PathComparer =
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        // Models are cached by normalized full path. A successful load/update only touches disk once;
        // subsequent Load* calls return the same in-memory model instance.
        private static readonly ConcurrentDictionary<string, ISaveModel> ModelCache =
            new(PathComparer);

        // Serializes file I/O for each individual path without blocking unrelated files.
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks =
            new(PathComparer);

        private static readonly object InitializationSync = new();
        private static Task? initializationTask;

        private static JsonSchema WorkspaceSchema = null!;
        private static JsonSchema WorkspaceOldSchema = null!;
        private static JsonSchema WorkspaceRecordSchema = null!;
        private static JsonSchema WorkspaceRecordOldSchema = null!;
        private static JsonSchema SessionsSchema = null!;
        private static JsonSchema SessionsOldSchema = null!;

        public enum FileUpdateResult
        {
            Failure = 0,
            UpToDate = 1,
            Updated = 2,
            Overwritten = 3,
        }

        /// <summary>
        /// Starts disk initialization in the background and returns immediately.
        /// Existing callers can continue to call FileHandler.Initialize() without blocking startup.
        /// Any Load*/Save* method automatically awaits initialization before accessing files.
        /// </summary>
        public static void Initialize()
        {
            _ = EnsureInitializationStarted();
        }

        /// <summary>
        /// Starts (or joins) the single initialization operation.
        /// Await this when a caller explicitly needs disk state to be ready.
        /// </summary>
        public static Task InitializeAsync()
        {
            return EnsureInitializationStarted();
        }

        public static bool IsInitialized =>
            initializationTask is { Status: TaskStatus.RanToCompletion };

        private static Task EnsureInitializationStarted()
        {
            lock (InitializationSync)
            {
                // Task.Run keeps schema loading, directory enumeration and migration work off the UI thread.
                return initializationTask ??= Task.Run(InitializeCoreAsync);
            }
        }

        private static async Task InitializeCoreAsync()
        {
            try
            {
                Log.Information("Disk initialization started.", LoggingCategory);

                EnsureDirectoriesExist();
                LoadSchemas();
                await EnsureFilesExistAsync().ConfigureAwait(false);
                await AttemptUpgradeAsync().ConfigureAwait(false);

                Log.Information(
                    $"Disk initialization completed. Cached {ModelCache.Count} model(s).",
                    LoggingCategory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Disk initialization failed.", LoggingCategory);
                throw;
            }
        }

        private static void LoadSchemas()
        {
            Log.Debug("Loading JSON schemas.", LoggingCategory);

            WorkspaceSchema = SchemaLoader.Load("workspace");
            WorkspaceOldSchema = SchemaLoader.Load("workspace-old");

            WorkspaceRecordSchema = SchemaLoader.Load("workspaces");
            WorkspaceRecordOldSchema = SchemaLoader.Load("workspaces-old");

            SessionsSchema = SchemaLoader.Load("sessions");
            SessionsOldSchema = SchemaLoader.Load("sessions-old");
        }

        private static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(SavePath);
            Directory.CreateDirectory(PathHelper.AppDataPath);
            Directory.CreateDirectory(PathHelper.BasePath);

            Log.Debug("Verified application data directories.", LoggingCategory);
        }

        private static async Task EnsureFilesExistAsync()
        {
            if (!File.Exists(WorkspaceRecordPath))
            {
                await File.WriteAllTextAsync(
                    WorkspaceRecordPath,
                    JsonModelHelper.Serialize(new WorkspaceRecordModel()))
                    .ConfigureAwait(false);

                Log.Information(
                    $"Created missing workspace record: {WorkspaceRecordPath}",
                    LoggingCategory);
            }

            if (!File.Exists(SessionsPath))
            {
                await File.WriteAllTextAsync(
                    SessionsPath,
                    JsonModelHelper.Serialize(new SessionRecordModel()))
                    .ConfigureAwait(false);

                Log.Information(
                    $"Created missing session record: {SessionsPath}",
                    LoggingCategory);
            }

            if (!File.Exists(ConfigPath))
            {
                // TODO: Implement config model.
                await File.WriteAllTextAsync(ConfigPath, string.Empty)
                    .ConfigureAwait(false);

                Log.Information(
                    $"Created missing config file: {ConfigPath}",
                    LoggingCategory);
            }
        }

        private static async Task AttemptUpgradeAsync()
        {
            Log.Debug("Validating and migrating persisted data.", LoggingCategory);

            await UpdateFileAsync<SessionRecordModel>(SessionsPath, typeof(SessionRecordModel))
                .ConfigureAwait(false);

            (_, WorkspaceRecordModel? recordModel) =
                await UpdateFileAsync<WorkspaceRecordModel>(
                    WorkspaceRecordPath,
                    typeof(WorkspaceRecordModel))
                .ConfigureAwait(false);

            if (recordModel is null)
            {
                throw new InvalidDataException(
                    "Workspace record could not be loaded during initialization.");
            }

            await UpdateWorkspaceRecordEntriesAsync(recordModel)
                .ConfigureAwait(false);
        }

        private static async Task UpdateWorkspaceRecordEntriesAsync(
            WorkspaceRecordModel existing)
        {
            var workspaceFiles = new HashSet<string>(PathComparer);

            foreach (var workspace in existing.Workspaces.Values)
            {
                string path = !string.IsNullOrWhiteSpace(workspace.Path)
                    ? workspace.Path
                    : Path.Combine(PathHelper.DocumentsPath, workspace.Name + ".tcw");

                workspaceFiles.Add(NormalizePath(path));
            }

            foreach (string path in Directory.EnumerateFiles(
                         PathHelper.DocumentsPath,
                         "*.tcw",
                         SearchOption.AllDirectories))
            {
                workspaceFiles.Add(NormalizePath(path));
            }

            int addedEntries = 0;
            int refreshedEntries = 0;

            foreach (string file in workspaceFiles)
            {
                (_, WorkspaceModel? workspace) =
                    await UpdateFileAsync<WorkspaceModel>(file, typeof(WorkspaceModel))
                        .ConfigureAwait(false);

                if (workspace is null)
                    continue;

                if (existing.Workspaces.TryGetValue(workspace.Id, out var entry))
                {
                    bool changed = false;

                    if (entry.Name != workspace.Name)
                    {
                        entry.Name = workspace.Name;
                        changed = true;
                    }

                    if (entry.Path != file)
                    {
                        entry.Path = file;
                        changed = true;
                    }

                    if (changed)
                        refreshedEntries++;

                    continue;
                }

                existing.Workspaces.Add(workspace.Id, new()
                {
                    Name = workspace.Name,
                    Path = file,
                });

                addedEntries++;
            }

            // The workspace record loaded during initialization is already cached. Persist the same
            // instance after reconciliation so future loads do not need to touch disk again.
            if (addedEntries > 0 || refreshedEntries > 0)
            {
                await SaveModelCoreAsync(WorkspaceRecordPath, existing)
                    .ConfigureAwait(false);

                Log.Information(
                    $"Workspace record reconciled: {addedEntries} added, {refreshedEntries} refreshed.",
                    LoggingCategory);
            }
            else
            {
                Log.Debug("Workspace record is already reconciled.", LoggingCategory);
            }
        }

        private static async Task<(FileUpdateResult Result, T? Model)> UpdateFileAsync<T>(
            string path,
            Type modelType)
            where T : class, ISaveModel
        {
            string normalizedPath = NormalizePath(path);
            SemaphoreSlim fileLock = GetFileLock(normalizedPath);

            await fileLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!File.Exists(normalizedPath))
                {
                    Log.Warning(
                        $"Skipped {GetModelName(modelType)}; file was not found: {normalizedPath}",
                        LoggingCategory);

                    ModelCache.TryRemove(normalizedPath, out _);
                    return (FileUpdateResult.Failure, null);
                }

                (JsonSchema currentSchema, JsonSchema oldSchema) = GetSchemas(modelType);
                string raw = await File.ReadAllTextAsync(normalizedPath).ConfigureAwait(false);

                if (JsonModelHelper.IsValid(raw, currentSchema))
                {
                    T model = (T)JsonModelHelper.ParseAndValidate(
                        modelType,
                        raw,
                        currentSchema);

                    ModelCache[normalizedPath] = model;

                    Log.Debug(
                        $"Loaded current {GetModelName(modelType)}: {normalizedPath}",
                        LoggingCategory);

                    return (FileUpdateResult.UpToDate, model);
                }

                if (JsonModelHelper.IsValid(raw, oldSchema))
                {
                    string migratedJson = JsonModelHelper.MigrateJson(modelType, raw);
                    T model = (T)JsonModelHelper.ParseAndValidate(
                        modelType,
                        migratedJson,
                        currentSchema);

                    await WriteModelUnlockedAsync(normalizedPath, model)
                        .ConfigureAwait(false);

                    ModelCache[normalizedPath] = model;

                    Log.Information(
                        $"Migrated {GetModelName(modelType)} to the current format: {normalizedPath}",
                        LoggingCategory);

                    return (FileUpdateResult.Updated, model);
                }

                string backupPath = normalizedPath + ".old";

                if (modelType == typeof(WorkspaceModel))
                {
                    File.Move(normalizedPath, backupPath, overwrite: true);
                    ModelCache.TryRemove(normalizedPath, out _);

                    Log.Warning(
                        $"Workspace failed both current and legacy validation and was preserved as: {backupPath}",
                        LoggingCategory);

                    return (FileUpdateResult.Updated, null);
                }

                await File.WriteAllTextAsync(backupPath, raw).ConfigureAwait(false);

                T defaultModel = (T)(Activator.CreateInstance(modelType)
                    ?? throw new InvalidOperationException(
                        $"Could not create default instance of {modelType.Name}."));

                await WriteModelUnlockedAsync(normalizedPath, defaultModel)
                    .ConfigureAwait(false);

                ModelCache[normalizedPath] = defaultModel;

                Log.Warning(
                    $"{GetModelName(modelType)} failed validation. A default file was created and the original was backed up to: {backupPath}",
                    LoggingCategory);

                return (FileUpdateResult.Overwritten, defaultModel);
            }
            finally
            {
                fileLock.Release();
            }
        }

        //
        // Cache helpers
        //

        private static async Task<T> LoadCachedAsync<T>(
            string path,
            JsonSchema schema)
            where T : class, ISaveModel
        {
            await EnsureInitializationStarted().ConfigureAwait(false);

            string normalizedPath = NormalizePath(path);

            if (ModelCache.TryGetValue(normalizedPath, out ISaveModel? cached))
            {
                if (cached is T typed)
                    return typed;

                throw new InvalidCastException(
                    $"Cached model for '{normalizedPath}' is {cached.GetType().Name}, not {typeof(T).Name}.");
            }

            SemaphoreSlim fileLock = GetFileLock(normalizedPath);
            await fileLock.WaitAsync().ConfigureAwait(false);

            try
            {
                // Double-check after acquiring the lock in case another caller loaded it first.
                if (ModelCache.TryGetValue(normalizedPath, out cached))
                {
                    if (cached is T typed)
                        return typed;

                    throw new InvalidCastException(
                        $"Cached model for '{normalizedPath}' is {cached.GetType().Name}, not {typeof(T).Name}.");
                }

                if (!File.Exists(normalizedPath))
                {
                    var ex = new FileNotFoundException(
                        $"Could not load {typeof(T).Name}; file does not exist.",
                        normalizedPath);

                    Log.Error(ex, $"File load failed: {normalizedPath}", LoggingCategory);
                    throw ex;
                }

                string raw = await File.ReadAllTextAsync(normalizedPath).ConfigureAwait(false);
                T model = JsonModelHelper.ParseAndValidate<T>(raw, schema);

                ModelCache[normalizedPath] = model;

                Log.Debug(
                    $"Loaded and cached {typeof(T).Name}: {normalizedPath}",
                    LoggingCategory);

                return model;
            }
            finally
            {
                fileLock.Release();
            }
        }

        private static async Task SaveCachedAsync<T>(string path, T model)
            where T : class, ISaveModel
        {
            await EnsureInitializationStarted().ConfigureAwait(false);
            await SaveModelCoreAsync(path, model).ConfigureAwait(false);
        }

        private static async Task SaveModelCoreAsync<T>(string path, T model)
            where T : class, ISaveModel
        {
            string normalizedPath = NormalizePath(path);
            SemaphoreSlim fileLock = GetFileLock(normalizedPath);

            await fileLock.WaitAsync().ConfigureAwait(false);

            try
            {
                await WriteModelUnlockedAsync(normalizedPath, model)
                    .ConfigureAwait(false);

                // Replace or seed the cached instance only after the write succeeds.
                ModelCache[normalizedPath] = model;
            }
            finally
            {
                fileLock.Release();
            }
        }

        private static Task WriteModelUnlockedAsync<T>(string path, T model)
            where T : class, ISaveModel
        {
            string raw = JsonModelHelper.Serialize(model);
            return File.WriteAllTextAsync(path, raw);
        }

        private static SemaphoreSlim GetFileLock(string path)
        {
            return FileLocks.GetOrAdd(path, static _ => new SemaphoreSlim(1, 1));
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path);
        }

        private static (JsonSchema Current, JsonSchema Legacy) GetSchemas(Type modelType)
        {
            return modelType switch
            {
                Type t when t == typeof(SessionRecordModel)
                    => (SessionsSchema, SessionsOldSchema),

                Type t when t == typeof(WorkspaceModel)
                    => (WorkspaceSchema, WorkspaceOldSchema),

                Type t when t == typeof(WorkspaceRecordModel)
                    => (WorkspaceRecordSchema, WorkspaceRecordOldSchema),

                _ => throw new InvalidOperationException(
                    $"No schemas are registered for {modelType.Name}.")
            };
        }

        private static string GetModelName(Type modelType)
        {
            string name = modelType.Name;

            if (name.EndsWith("Model", StringComparison.Ordinal))
                name = name[..^"Model".Length];

            return string.IsNullOrEmpty(name)
                ? modelType.Name
                : char.ToLowerInvariant(name[0]) + name[1..];
        }

        /// <summary>
        /// Removes a cached model so the next Load* call reads it from disk again.
        /// Useful if a file may have been changed externally.
        /// </summary>
        public static bool InvalidateCache(string path)
        {
            return ModelCache.TryRemove(NormalizePath(path), out _);
        }

        public static void ClearCache()
        {
            ModelCache.Clear();
            Log.Debug("Cleared persisted model cache.", LoggingCategory);
        }

        //
        // Callable tasks for application function
        //

        public static Task<SessionRecordModel> LoadSessions()
        {
            return LoadCachedAsync<SessionRecordModel>(SessionsPath, SessionsSchema);
        }

        public static async Task SaveSessions(SessionRecordModel sessions)
        {
            await SaveCachedAsync(SessionsPath, sessions).ConfigureAwait(false);
            Log.Debug("Saved session record.", LoggingCategory);
        }

        public static Task<WorkspaceRecordModel> LoadWorkspaceRecord()
        {
            return LoadCachedAsync<WorkspaceRecordModel>(
                WorkspaceRecordPath,
                WorkspaceRecordSchema);
        }

        public static Task<WorkspaceModel> LoadWorkspace(string path)
        {
            return LoadCachedAsync<WorkspaceModel>(path, WorkspaceSchema);
        }

        public static async Task SaveWorkspace(WorkspaceModel workspace, string path)
        {
            await SaveCachedAsync(path, workspace).ConfigureAwait(false);

            Log.Debug(
                $"Saved workspace '{workspace.Name}': {NormalizePath(path)}",
                LoggingCategory);
        }

        public static async Task SaveWorkspaceRecord(WorkspaceRecordModel record)
        {
            await SaveCachedAsync(WorkspaceRecordPath, record).ConfigureAwait(false);
            Log.Debug("Saved workspace record.", LoggingCategory);
        }
    }
}
