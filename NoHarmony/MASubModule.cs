using HarmonyLib;
using NoHarmony;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;

namespace MarryAnyone
{
    internal class MASubModule : NoHarmonyLoader
    {
        public static bool IsPolyamorous = true;

        public static bool IsIncestual = true;

        public enum SexualOrientation
        {
            Heterosexual,
            Homosexual,
            Bisexual
        }

        public static int SetSexualOrientation = 2;

        public static SexualOrientation GetSexualOrientation()
        {
            switch(SetSexualOrientation)
            {
                case 1:
                    return SexualOrientation.Homosexual;
                case 2:
                    return SexualOrientation.Bisexual;
                default:
                    return SexualOrientation.Heterosexual;
            }
        }

        public static bool IsHeterosexual = GetSexualOrientation() == SexualOrientation.Heterosexual;

        public static bool IsHomosexual = GetSexualOrientation() == SexualOrientation.Homosexual;

        public static bool IsBisexual = GetSexualOrientation() == SexualOrientation.Bisexual;

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
            gameInitializer.AddBehavior(new MALordConversationsCampaignBehavior());
            gameInitializer.AddBehavior(new MARomanceCampaignBehavior());
        }
    }
}