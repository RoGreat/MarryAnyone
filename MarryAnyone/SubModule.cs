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

            Harmony harmony = new Harmony("mod.bannerlord.anyone.marry");
            harmony.PatchAll();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign)
            {
                var gameStarter = (CampaignGameStarter)gameStarterObject;

                var currentMarriageModel = GetGameModel<MarriageModel>(gameStarter);
                if (currentMarriageModel is null)
                {
                    Log.Warning("DefaultMarriageModel not found");
                }

                if (Settings.Instance!.EnableVillagerRomance)
                {
                    gameStarter.AddBehavior(new VillagerRomanceCampaignBehavior());
                    gameStarter.AddModel(new VillagerMarriageModel(currentMarriageModel));
                }

                if (Settings.Instance!.EnableLeaderRomance)
                {
                    gameStarter.AddBehavior(new LeaderRomanceCampaignBehavior());
                    gameStarter.AddModel(new LeaderMarriageModel(currentMarriageModel));
                }
            }
        }

        private T? GetGameModel<T>(IGameStarter gameStarterObject) where T : GameModel
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
