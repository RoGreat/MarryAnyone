using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone.Models
{
    internal class MARomanceModel : DefaultRomanceModel
    {
        public override bool CourtshipPossibleBetweenNPCs(Hero person1, Hero person2)
        {
            return Romance.GetRomanticLevel(person1, person2) == Romance.RomanceLevelEnum.Untested 
                && (person2.Clan == null || Romance.GetCourtedHeroInOtherClan(person1, person2) == null) 
                && (person1.Clan == null || Romance.GetCourtedHeroInOtherClan(person2, person1) == null) 
                && Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(person1, person2);
        }
    }
}
