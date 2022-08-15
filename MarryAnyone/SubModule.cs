using HarmonyLib;
using MarryAnyone.Behaviors;
using MarryAnyone.Models;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone
{
    internal class SubModule : MBSubModuleBase
    {
        public static RomanceCampaignBehavior? RomanceCampaignBehaviorInstance;

        public static CompanionsCampaignBehavior? CompanionsCampaignBehaviorInstance;

        public static CharacterDevelopmentCampaignBehavior? CharacterDevelopmentCampaignBehaviorInstance;

        public static LordConversationsCampaignBehavior? LordConversationsCampaignBehaviorInstance;

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
                campaignGameStarter.AddBehavior(new MarryAnyoneCampaignBehavior());
                campaignGameStarter.AddModel(new MarryAnyoneMarriageModel());
            }
        }
    }
}