using HarmonyLib;
using NoHarmony;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace MarryAnyone
{
    class MASubModule : NoHarmonyLoader
    {
        public override void NoHarmonyInit()
        {
            LogDateFormat = "MM/dd/yy HH:mm:ss.fff";
        }

        public override void NoHarmonyLoad()
        {
            ReplaceModel<MADefaultMarriageModel, DefaultMarriageModel>();
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            var harmony = new Harmony("mod.bannerlord.anyone.marry");
            harmony.PatchAll();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            if (!_isLoaded)
            {
                InformationManager.DisplayMessage(new InformationMessage("Loaded Marry Anyone", Color.FromUint(4282569842U)));
                _isLoaded = true;
            }
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

        private bool _isLoaded;
    }
}
