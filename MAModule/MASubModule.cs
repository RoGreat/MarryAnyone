using HarmonyLib;
using MarryAnyone.Behaviors;
using MarryAnyone.Settings;
using MCM.Abstractions.Settings.Base.PerSave;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone
{
    internal class MASubModule : MBSubModuleBase
    {
        private static Harmony _harmony;

        public static void Debug(string message)
        {
            if (MASettings.Instance?.Debug ?? false)
            {
                InformationManager.DisplayMessage(new InformationMessage(message, new Color(0.6f, 0.2f, 1f)));
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            _harmony = new Harmony("mod.bannerlord.anyone.marry");
            _harmony.PatchAll();
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            _harmony.UnpatchAll();
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
            gameInitializer.AddBehavior(new MARomanceCampaignBehavior());
        }
    }
}