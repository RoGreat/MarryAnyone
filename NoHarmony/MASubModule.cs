using HarmonyLib;
using NoHarmony;
using System.Diagnostics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using TaleWorlds.Library;
using System;

namespace MarryAnyone
{
    internal class MASubModule : NoHarmonyLoader
    {
        private static Harmony harmony = null;

        public static MAConfig Config { get; private set; }

        public override void NoHarmonyInit()
        {
            LogFile = "MANoHarmony";
            LogDateFormat = "MM/dd/yy HH:mm:ss.fff";
            Logging = false;
        }

        public override void NoHarmonyLoad()
        {
            ReplaceModel<MADefaultMarriageModel, DefaultMarriageModel>();
        }

        private void MADebug(String message)
        {
            MAConfig config = MASubModule.Config;
            if(config.Debug)
            {
                InformationManager.DisplayMessage(new InformationMessage(message));
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            string path = Path.Combine(BasePath.Name, "Modules", "MarryAnyone", "ModuleData", "config.xml");
            if (File.Exists(path))
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                using (var xmlReader = XmlReader.Create(fileStream))
                {
                    if (new DataContractSerializer(typeof(MAConfig)).ReadObject(xmlReader) is MAConfig config)
                    {
                        MASubModule.Config = config;
                    }
                }
            }
            harmony = new Harmony("mod.bannerlord.anyone.marry");
            harmony.PatchAll();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            MAConfig config = MASubModule.Config;
            MADebug("MA Difficulty: " + config.Difficulty);
            MADebug("MA Orientation: " + config.SexualOrientation);
            MADebug("MA Polygamy: " + config.IsPolygamous.ToString());
            MADebug("MA Incest: " + config.IsIncestual.ToString());
        }

        public override void OnCampaignStart(Game game, object starterObject)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameInitializer = (CampaignGameStarter)starterObject;
                AddBehaviors(gameInitializer);
            }
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameInitializer = (CampaignGameStarter)initializerObject;
                AddBehaviors(gameInitializer);
            }
        }

        private void AddBehaviors(CampaignGameStarter gameInitializer)
        {
            gameInitializer.AddBehavior(new MALordConversationsCampaignBehavior());
            gameInitializer.AddBehavior(new MARomanceCampaignBehavior());
        }
    }
}