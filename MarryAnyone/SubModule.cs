using HarmonyLib;

using MarryAnyone.CampaignBehaviors;
using MarryAnyone.Models;

using System.Linq;

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

                MarriageModel? currentMarriageModel = GetGameModel<MarriageModel>(gameStarter);
                gameStarter.AddModel(new MarryAnyoneMarriageModel(currentMarriageModel));

                HeroAgentLocationModel? currentHeroAgentLocationModel = GetGameModel<HeroAgentLocationModel>(gameStarter);
                gameStarter.AddModel(new MarryAnyoneHeroAgentLocationModel(currentHeroAgentLocationModel));

                if (Settings.Instance!.EnableCommonerRomance)
                {
                    gameStarter.AddBehavior(new CommonerRomanceCampaignBehavior());
                }

                if (Settings.Instance!.EnableLeaderRomance)
                {
                    gameStarter.AddBehavior(new LeaderRomanceCampaignBehavior());
                }
            }
        }

        private static T? GetGameModel<T>(IGameStarter gameStarterObject) where T : GameModel
        {
            var models = gameStarterObject.Models.ToArray();

            for (int index = models.Length - 1; index >= 0; --index)
            {
                if (models[index] is T gameModel1)
                    return gameModel1;
            }
            return default;
        }
    }
}
