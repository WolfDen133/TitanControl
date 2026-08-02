using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.WebAPI.Data.Model;

namespace TitanControl.WebAPI.Data
{
    public static class RateMasterTypes
    {
        public const string Rate = "RateMaster";
        public const string Size = "SizeMaster";
        public const string BPM = "BPMMaster";
        public const string None = "None";

        private const string MatchString = "Master";

        public static MasterTypes GetType (Handle info)
        {
            string? typeString = null;

            foreach (var property in info.Properties)
            {
                if (property.Key == MatchString)
                {
                    typeString = property.Value;
                    break;
                }
            }

            if (typeString == null) return MasterTypes.None;
            
            return StringToType(typeString);
        }

        public static bool IsType (Handle info, MasterTypes type) {

            return GetType(info).Equals(type);
        }

        public static bool IsMaster (Handle info)
        {
            return !IsType(info, MasterTypes.None);
        }

        public static string TypeToString (MasterTypes type)
        {
            return type switch
            {
                MasterTypes.None => None,
                MasterTypes.Rate => Rate,
                MasterTypes.Size => Size,
                MasterTypes.BPM => BPM,
                _ => None
            };
        }

        public static MasterTypes StringToType (string type)
        {
            int colonIndex = type.LastIndexOf(':');
            string formatted = type.Substring(0, colonIndex);

            return formatted switch
            {
                Rate => MasterTypes.Rate,
                Size => MasterTypes.Size,
                BPM => MasterTypes.BPM,
                _ => MasterTypes.None
            };
        }
                
    }

    public enum MasterTypes
    {
        None = 0,
        Rate = 1,
        Size = 2,
        BPM = 3
    }
}
