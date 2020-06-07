using HarmonyLib;
using NoHarmony;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MarryAnyone
{
    internal class MASubModule : NoHarmonyLoader
    {
        private static Harmony harmony;

        public static void MADebug(string message)
        {
            if (MASettings.Instance.Debug)
            {
                InformationManager.DisplayMessage(new InformationMessage(message, new Color(0.6f, 0.2f, 1f)));
            }
        }

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

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            harmony = new Harmony("mod.bannerlord.anyone.marry");
            harmony.PatchAll();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            harmony.UnpatchAll();
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            MARomanceCampaignBehavior.DeactivateEncounter();
        }

        public override void OnCampaignStart(Game game, object starterObject)
        {
            base.OnCampaignStart(game, starterObject);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameInitializer = (CampaignGameStarter)starterObject;
                AddBehaviors(gameInitializer);
                LoadXMLs(gameInitializer);
            }
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            base.OnGameLoaded(game, initializerObject);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameInitializer = (CampaignGameStarter)initializerObject;
                AddBehaviors(gameInitializer);
                LoadXMLs(gameInitializer);
            }
        }

        private void AddBehaviors(CampaignGameStarter gameInitializer)
        {
            gameInitializer.AddBehavior(new MALordConversationsCampaignBehavior());
            gameInitializer.AddBehavior(new MARomanceCampaignBehavior());
        }

        private void LoadXMLs(CampaignGameStarter campaignGameStarter)
        {
            string path = Path.Combine(BasePath.Name, "Modules", "MarryAnyone", "ModuleData", "ma_module_strings.xml");
            campaignGameStarter.LoadGameTexts(path);
        }
    }
}