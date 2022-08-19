using HarmonyLib;
using MarryAnyone.Behaviors;
using MarryAnyone.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone
{
    internal sealed class SubModule : MBSubModuleBase
    {
        public static CompanionsCampaignBehavior? CompanionsCampaignBehaviorInstance { get; private set; }

        public static CharacterDevelopmentCampaignBehavior? CharacterDevelopmentCampaignBehaviorInstance { get; private set; }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("mod.bannerlord.anyone.marry").PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarterObject;
                campaignGameStarter.AddBehavior(new MARomanceCampaignBehavior());
                campaignGameStarter.AddModel(new MAMarriageModel());
                /* Used for calling private methods in these instances */
                CompanionsCampaignBehaviorInstance = new();
                CharacterDevelopmentCampaignBehaviorInstance = new();
            }
        }
    }
}