using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using TaleWorlds.Library;

namespace MarryAnyone
{
    internal static class MASettings
    {
        public static MAConfig Config { get; private set; }
        public static void Settings()
        {
            string path = Path.Combine(BasePath.Name, "Modules", "MarryAnyone", "ModuleData", "config.xml");
            if (File.Exists(path))
            {
                using (StreamReader streamReader = File.OpenText(path))
                {
                    using (XmlReader xmlReader = XmlReader.Create(streamReader))
                    {
                        MAConfig config = new DataContractSerializer(typeof(MAConfig)).ReadObject(xmlReader) as MAConfig;
                        if (config != null)
                        {
                            Config = config;
                        }
                    }
                }
            }
        }
    }
}