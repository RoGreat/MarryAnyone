using HarmonyLib;

using MarryAnyone.CampaignBehaviors;
using MarryAnyone.Models;

using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            Harmony harmony = new("mod.bannerlord.anyone.marry");
            harmony.PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;

                MarriageModel? currentMarriageModel = gameStarter.GetModel<MarriageModel>();
                gameStarter.AddModel(new MarryAnyoneMarriageModel(currentMarriageModel));

                if (Settings.Instance!.EnableNotLordRomance)
                {
                    gameStarter.RemoveBehaviors<NotLordConversationsCampaignBehavior>();
                    gameStarter.AddBehavior(new NotLordConversationsCampaignBehavior());
                    gameStarter.AddBehavior(new NotLordRomanceCampaignBehavior());
                }

                if (Settings.Instance!.EnableLeaderRomance)
                {
                    gameStarter.AddBehavior(new LeaderRomanceCampaignBehavior());
                }
            }
        }
    }
}
