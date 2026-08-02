using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Helper
{
    public class PathHelper
    {
        public static string BasePath = AppContext.BaseDirectory;

        public static string AppDataPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppConstants.AppName);

        public static string DocumentsPath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppConstants.AppName);
    }
}
