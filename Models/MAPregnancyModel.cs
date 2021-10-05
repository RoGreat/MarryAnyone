using System;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone.Models
{
    internal class MAPregnancyModel : DefaultPregnancyModel
    {
        
		private bool IsHeroAgeSuitableForPregnancy(Hero hero)
		{   
            ISettingsProvider settings = new MASettings();
			return hero.Age >= settings.MinPregnancyAge && hero.Age <= 45f;
		}

		
		public override float GetDailyChanceOfPregnancyForHero(Hero hero)
		{
            ISettingsProvider settings = new MASettings();
			float result = 0f;
			if (hero.Spouse != null && this.IsHeroAgeSuitableForPregnancy(hero))
			{
				result = (1.2f - (hero.Age - settings.MinPregnancyAge) * 0.04f) / (float)Math.Pow((double)(hero.Children.Count + 1), 2.0) * 0.2f;
			}
			return result;
		}

    }
}