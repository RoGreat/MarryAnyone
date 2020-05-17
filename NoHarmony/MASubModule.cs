using HarmonyLib;
using NoHarmony;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;

namespace MarryAnyone
{
    class MASubModule : NoHarmonyLoader
    {
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
            new Harmony("mod.bannerlord.anyone.marry").PatchAll();
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
            gameInitializer.AddBehavior(new MAConversationsCampaignBehavior());
            gameInitializer.AddBehavior(new MARomanceCampaignBehavior());
        }
    }
}