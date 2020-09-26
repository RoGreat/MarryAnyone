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

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameLoaded(game, gameStarter);

            if (game.GameType is Campaign && gameStarter is CampaignGameStarter campaignGameStarter)
            {
                CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarter;
                campaignGameStarter.LoadGameTexts($"{BasePath.Name}Modules/MarryAnyone/ModuleData/ma_module_strings.xml");
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