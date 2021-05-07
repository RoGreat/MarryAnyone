using HarmonyLib;
using MarryAnyone.Behaviors;
using MarryAnyone.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using NoHarmony;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using MarryAnyone.Models;

namespace MarryAnyone
{
    internal class MASubModule : NoHarmonyLoader
    {
        public override void NoHarmonyInit()
        {
            Logging = false;
            LogFile = "MANoHarmony.txt";
            LogDateFormat = "MM/dd/yy HH:mm:ss.fff";
        }

        public override void NoHarmonyLoad()
        {
            ReplaceModel<MADefaultMarriageModel, DefaultMarriageModel>();
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
                campaignGameStarter.LoadGameTexts(BasePath.Name + "Modules/MarryAnyone/ModuleData/ma_module_strings.xml");
                AddBehaviors(campaignGameStarter);
            }
        }

        private void AddBehaviors(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddBehavior(new MARomanceCampaignBehavior());
            campaignGameStarter.AddBehavior(new MAAdoptionCampaignBehavior());
        }
    }
}