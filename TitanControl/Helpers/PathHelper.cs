using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TitanControl.Helper
{
    public class PathHelper
    {
        public static string BasePath = AppContext.BaseDirectory;

        public static string AppDataPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.AppName);

        public static string DocumentsPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppConstants.AppName);

        public static string GetNextFileName(string baseName, IEnumerable<string> nameList)
        {
            var names = nameList.ToHashSet();

            // Remove an existing trailing number.
            // "Titan control 1" -> "Titan control"
            var match = Regex.Match(baseName, @"^(.*?)(?:\s+(\d+))?$");

            var rootName = match.Groups[1].Value.TrimEnd();

            // If the exact name doesn't exist, return it unchanged.
            if (!names.Contains(baseName))
                return baseName;

            int i = 1;

            while (names.Contains($"{rootName} {i}"))
                i++;

            return $"{rootName} {i}";
        }
    }
}
