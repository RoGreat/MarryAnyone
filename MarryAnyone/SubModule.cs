using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using MarryAnyone.Behaviors;
using MarryAnyone.Models;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone
{
    internal sealed class SubModule : MBSubModuleBase
    {
        public static CompanionsCampaignBehavior? CompanionsCampaignBehaviorInstance { get; private set; }

        public static CharacterDevelopmentCampaignBehavior? CharacterDevelopmentCampaignBehaviorInstance { get; private set; }

        public static HeroAgentSpawnCampaignBehavior? HeroAgentSpawnCampaignBehaviorInstance { get; private set; }

        /* HeroAgentSpawnCampaignBehavior -> AddNotablesAndWanderers */
        //public delegate bool AddWandererLocationCharacterDelegate(HeroAgentSpawnCampaignBehavior instance, Hero wanderer, Settlement settlement);
        //public static readonly AddWandererLocationCharacterDelegate AddWandererLocationCharacter = AccessTools2.GetDelegate<AddWandererLocationCharacterDelegate>(typeof(HeroAgentSpawnCampaignBehavior), "AddWandererLocationCharacter", new Type[] { typeof(Hero), typeof(Hero) });

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
                HeroAgentSpawnCampaignBehaviorInstance = new();
            }
        }
    }
}