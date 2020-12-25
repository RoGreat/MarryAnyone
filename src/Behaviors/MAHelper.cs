using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace MarryAnyone.Behaviors
{
    internal static class MAHelper
    {
        public static void RemoveExSpouses(Hero hero, bool spouse = true)
        {
            FieldInfo _exSpouses = AccessTools.Field(typeof(Hero), "_exSpouses");
            List<Hero> _exSpousesList = (List<Hero>)_exSpouses.GetValue(hero);
            FieldInfo ExSpouses = AccessTools.Field(typeof(Hero), "ExSpouses");
            MBReadOnlyList<Hero> ExSpousesReadOnlyList;

            if (spouse)
            {
                _exSpousesList = _exSpousesList.Distinct().ToList();
                if (_exSpousesList.Contains(hero.Spouse))
                {
                    _exSpousesList.Remove(hero.Spouse);
                }
            }
            else
            {
                _exSpousesList = _exSpousesList.ToList();
                Hero exSpouse = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).FirstOrDefault();
                if (exSpouse != null)
                {
                    _exSpousesList.Remove(exSpouse);
                }
            }
            ExSpousesReadOnlyList = new MBReadOnlyList<Hero>(_exSpousesList);
            _exSpouses.SetValue(hero, _exSpousesList);
            ExSpouses.SetValue(hero, ExSpousesReadOnlyList);
        }

        public static void OccupationToLord(CharacterObject character, CharacterObject template)
        {
            if (character.Occupation != Occupation.Lord)
            {
                AccessTools.Property(typeof(CharacterObject), "Occupation").SetValue(character, Occupation.Lord);
                MASubModule.Debug("Occupation To Lord");
            }
            var _originCharacter = AccessTools.Field(typeof(CharacterObject), "_originCharacter");
            var _originCharacterStringId = AccessTools.Field(typeof(CharacterObject), "_originCharacterStringId");
            if (_originCharacter != null)
            {
                _originCharacter.SetValue(character, template);
            }
            else if (_originCharacterStringId != null)
            {
                _originCharacterStringId.SetValue(character, template.StringId);
            }
            // In ClanLordItemVM
            // this.IsFamilyMember = Hero.MainHero.Clan.Lords.Contains(this._hero);
            List<Hero> _lords = (List<Hero>)AccessTools.Field(typeof(Clan), "_lords").GetValue(Clan.PlayerClan);
            if (!_lords.Contains(character.HeroObject))
            {
                _lords.Add(character.HeroObject);
            }
        }
    }
}