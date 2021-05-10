using MarryAnyone.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MarryAnyone.Behaviors
{
    internal class MAPerSaveCampaignBehavior : CampaignBehaviorBase
    {
        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            new MASettings();
            if (MAConfig.Instance?.Warning ?? false && !MASettings.UsingMCM)
            {
                if (MASettings.NoMCMWarning)
                {
                    InformationManager.ShowInquiry(new InquiryData(GameTexts.FindText("str_warning").ToString(), GameTexts.FindText("str_no_mcm_info").ToString(), true, true, GameTexts.FindText("str_ok").ToString(), GameTexts.FindText("str_dontshowagain").ToString(), null, new Action(DontShowAgain)), false);
                }
                else if (MASettings.NoConfigWarning)
                {
                    InformationManager.ShowInquiry(new InquiryData(GameTexts.FindText("str_warning").ToString(), GameTexts.FindText("str_no_config_info").ToString(), true, true, GameTexts.FindText("str_ok").ToString(), GameTexts.FindText("str_dontshowagain").ToString(), null, new Action(DontShowAgain)), false);
                }
            }
        }

        private void DontShowAgain()
        {
            try
            {
                string json = File.ReadAllText(MASettings.ConfigPath);
                JObject? jObject = JsonConvert.DeserializeObject(json) as JObject;
                if (jObject is not null)
                {
                    JToken jToken = jObject.SelectToken("Warning");
                    jToken.Replace(false);
                    File.WriteAllText(MASettings.ConfigPath, jObject.ToString());
                }
            }
            catch (Exception exception)
            {
                MAHelper.Error(exception);
            }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}