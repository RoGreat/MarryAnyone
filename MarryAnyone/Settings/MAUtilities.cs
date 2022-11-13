using System.IO;
using TaleWorlds.Engine;
using TaleWorlds.Library;

// Adapted from Taleworlds.Engine.Utilities
namespace RecruitEveryone.Settings
{
    internal static class MAUtilities
    {

        private static string ConfigDirectory
        {
            get
            {
                return System.IO.Path.Combine(EngineFilePaths.ConfigsPath.Path, "ModSettings", "MarryAnyone");
            }
        }

        private static PlatformFilePath ConfigFullPath
        {
            get
            {
                return new PlatformFilePath(new PlatformDirectoryPath(PlatformFileType.User, ConfigDirectory), "MarryAnyoneConfig.txt");
            }
        }

        public static string LoadConfigFile()
        {
            PlatformFilePath configFullPath = ConfigFullPath;
            if (!FileHelper.FileExists(configFullPath))
            {
                return "";
            }
            return FileHelper.GetFileContentString(configFullPath);
        }

        public static SaveResult SaveConfigFile(string configProperties)
        {
            // Create mod config directory if not already created
            string configDirectoryPath = ConfigDirectory;
            if (!Directory.Exists(configDirectoryPath))
            {
                Directory.CreateDirectory(configDirectoryPath); 
            }

            PlatformFilePath configFullPath = ConfigFullPath;
            SaveResult result;
            try
            {
                string data = configProperties.Substring(0, configProperties.Length - 1);
                FileHelper.SaveFileString(configFullPath, data);
                result = SaveResult.Success;
            }
            catch
            {
                Debug.Print("Could not create Marry Anyone Config file", 0, Debug.DebugColor.White);
                result = SaveResult.ConfigFileFailure;
            }
            return result;
        }
    }
}