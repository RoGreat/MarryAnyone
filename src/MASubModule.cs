using HarmonyLib;
using MarryAnyone.Behaviors;
using MarryAnyone.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone
{
    internal class MASubModule : MBSubModuleBase
    {
        public static void Debug(string message, bool important = false)
        {
            ISettingsProvider settings = new MASettings();
            Color color;
            if (important)
            {
                color = Colors.Red;
            }
            else
            {
                color = new Color(0.6f, 0.2f, 1f);
            }
            if (settings.Debug || important)
            {
                InformationManager.DisplayMessage(new InformationMessage(message, color));
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            MAConfig.Instance = new MAConfig();
            new Harmony("mod.bannerlord.anyone.marry").PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarter;
                campaignGameStarter.LoadGameTexts($"{BasePath.Name}Modules/MarryAnyone/ModuleData/ma_module_strings.xml");
                AddBehaviors(campaignGameStarter);
            }
            try
            {
                if (MAMCMSettings.Instance is null)
                {
                    MASettings.UsingMCM = true;
                }
            }
            catch 
            {
                Debug("Marry Anyone: Not using MCM", true);
                Debug("Using config settings", true);
            }
        }

        private void AddBehaviors(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddBehavior(new MARomanceCampaignBehavior());
            campaignGameStarter.AddBehavior(new MAAdoptionCampaignBehavior());
        }
    }
}