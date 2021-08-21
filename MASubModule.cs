using HarmonyLib;
using MarryAnyone.Behaviors;
using MarryAnyone.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.MountAndBlade;
using System.IO;
using System;
using System.Collections.Generic;
using TournamentsXPanded.Common.Patches;
using System.Reflection;
using System.Linq;

namespace MarryAnyone
{
    public class MASubModule : MBSubModuleBase // NoHarmonyLoader
    {

        public static string ModuleFolderName { get; } = "MarryAnyone";

        public static IDictionary<Type, IPatch> ActivePatches = new Dictionary<Type, IPatch>();
        public static readonly Harmony Harmony = new Harmony(ModuleFolderName);

        //public override void NoHarmonyInit()
        //{
        //    Logging = false;
        //    LogFile = "MANoHarmony.txt";
        //    LogDateFormat = "MM/dd/yy HH:mm:ss.fff";
        //}

        //public override void NoHarmonyLoad()
        //{
        //    ReplaceModel<MADefaultMarriageModel, DefaultMarriageModel>();
        //    ReplaceModel<MARomanceModel, DefaultRomanceModel>();
        //}

        protected override void OnSubModuleLoad()
        {
            var dirpath = System.IO.Path.Combine(TaleWorlds.Engine.Utilities.GetLocalOutputPath(), ModuleFolderName);
            try
            {
                if (!Directory.Exists(dirpath))
                {
                    Directory.CreateDirectory(dirpath);
                }
                MAHelper.Print("Output directory : " + dirpath, true);
            }
            catch
            {
                MAHelper.Print("Failed to create config directory.  Please manually create this directory: " + dirpath, true);
            }

            MAHelper.LogPath = dirpath;
            base.OnSubModuleLoad();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);

            MAHelper.Print(String.Format("Chemin output : '{0}'", MAHelper.LogPath), true);

            if (game.GameType is Campaign)
            {

                MAHelper.Print("Campaign", true);

                CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarter;
                campaignGameStarter.LoadGameTexts(BasePath.Name + "Modules/MarryAnyone/ModuleData/ma_module_strings.xml");
                AddBehaviors(campaignGameStarter);

                //gameStarter.AddModel(new MADefaultMarriageModel());
                //gameStarter.AddModel(new MARomanceModel());

                //MarriageModel oldMarriageModel = campaignGameStarter.Models.OfType<MarriageModel>().FirstOrDefault();
                //if (oldMarriageModel != null)
                //    campaignGameStarter.Models[oldMarriageModel] = new MADefaultMarriageModel();
                //else
                //    campaignGameStarter.Models.AddItem(new MADefaultMarriageModel());

                //RomanceModel romanceModel = campaignGameStarter.Models.OfType<RomanceModel>().FirstOrDefault();
                //if (romanceModel != null)
                //    campaignGameStarter.Models.[romanceModel] = new MADefaultMarriageModel();
                //else
                //    campaignGameStarter.Models.AddItem(new MARomanceModel());


                bool hadSpouse = Hero.MainHero.Spouse != null;
                bool mainHeroIsFemale = Hero.MainHero.IsFemale;

                foreach (Hero hero in Hero.MainHero.Children)
                {
                    if (hadSpouse && hero.Father == Hero.MainHero && hero.Mother == Hero.MainHero)
                    {
                        if (mainHeroIsFemale)
                            hero.Father = Hero.MainHero.Spouse;
                        else
                            hero.Mother = Hero.MainHero.Father;
                        MAHelper.Print(string.Format("Patch Parent of {0}", hero.Name), true);
                    }
                    if (hero.Father == null)
                    {
                        hero.Father = mainHeroIsFemale && hadSpouse ? Hero.MainHero.Spouse : Hero.MainHero;
                        MAHelper.Print(string.Format("Patch Father of {0}", hero.Name), true);
                    }
                    if (hero.Mother == null)
                    {
                        hero.Mother = !mainHeroIsFemale && hadSpouse ? Hero.MainHero.Spouse : Hero.MainHero;
                        MAHelper.Print(string.Format("Patch Mother of {0}", hero.Name), true);
                    }
                }
            }
        }

        #region HarmoryPatches

        protected void ApplyPatches(Game game, Type moduletype, bool debugmode = false)
        {
            //ActivePatches.Clear();

            foreach (var patch in GetPatches(moduletype))
            {
                try
                {
                    patch.Reset();
                }
                catch (Exception ex)
                {
                    //Error(ex, $"Error while resetting patch: {patch.GetType().Name}");
                    //MessageBox.Show("TournamentXP Patch Error", $"Error while applying patch: {patch.GetType().Name}\n" + ex.ToStringFull());
                    MAHelper.Log($"Error while resetting patch: {patch.GetType().Name}", "ERROR");
                    MAHelper.Log(ex.ToString(), "ERROR");
                }

                try
                {
                    if (patch.IsApplicable(game))
                    {
                        try
                        {
                            patch.Apply(game);
                        }
                        catch (Exception ex)
                        {
                            //  Error(ex, $"Error while applying patch: {patch.GetType().Name}");
                            MAHelper.Log($"Error while applying patch: {patch.GetType().Name}", "ERROR");
                            MAHelper.Log(ex.ToString(), "ERROR");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MAHelper.Log($"Error while checking if patch is applicable: {patch.GetType().Name}", "ERROR");
                    MAHelper.Log(ex.ToString(), "ERROR");
                }

                var patchApplied = patch.Applied;
                if (patchApplied)
                {
                    ActivePatches[patch.GetType()] = patch;
                }

                if (debugmode)
                {
                    MAHelper.PrintWithColor($"{(patchApplied ? "Applied" : "Skipped")} Patch: {patch.GetType().Name}", (patchApplied ? Colors.Cyan : Colors.Red));
                }
            }
        }

        private LinkedList<IPatch>? _patches;

        public LinkedList<IPatch> GetPatches(Type moduletype)
        {


            if (_patches != null)
            {
                return _patches;
            }

            var patchInterfaceType = typeof(IPatch);
            _patches = new LinkedList<IPatch>();


            foreach (var type in moduletype.Assembly.GetTypes())
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                if (!patchInterfaceType.IsAssignableFrom(type))
                {
                    continue;
                }

                try
                {
                    var patch = (IPatch)Activator.CreateInstance(type, true);
                    //var patch = (IPatch) FormatterServices.GetUninitializedObject(type);
                    _patches.AddLast(patch);
                }
                catch (TargetInvocationException tie)
                {
                    //     Error(tie.InnerException, $"Failed to create instance of patch: {type.FullName}");
                    MAHelper.Log($"Failed to create instance of patch: {type.FullName}", "INFO");
                    MAHelper.Log(tie.ToString(), "INFO");
                }
                catch (Exception ex)
                {
                    // Error(ex, $"Failed to create instance of patch: {type.FullName}");
                    MAHelper.Log($"Failed to create instance of patch: {type.FullName}", "INFO");
                    MAHelper.Log(ex.ToString(), "INFO");
                }

            }
            return _patches;

        }

        #endregion HarmoryPatches


        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);

            if (game.GameType is Campaign)
            {
                Harmony.PatchAll();
            }
        }

        private void AddBehaviors(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddBehavior(new MAPerSaveCampaignBehavior());
            campaignGameStarter.AddBehavior(new MARomanceCampaignBehavior());
            campaignGameStarter.AddBehavior(new MAAdoptionCampaignBehavior());
        }
    }
}