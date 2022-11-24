using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using TaleWorlds.Library;
using RecruitEveryone.Settings;

namespace MarryAnyone.Settings
{
    internal sealed class MASettingsConfig : ISettingsProvider
    {
        public static MASettingsConfig? Instance { get; private set; }

        public MASettingsConfig()
        {
            Instance = this;
        }

        public string SexualOrientation 
        { 
            get { return MAConfig.SexualOrientation; }
            set { MAConfig.SexualOrientation = value; }
        }

        public string TemplateCharacter
        {
            get { return MAConfig.TemplateCharacter; }
            set { MAConfig.TemplateCharacter = value; }
        }

        public bool Polygamy
        {
            get { return MAConfig.Polygamy; }
            set { MAConfig.Polygamy = value; }
        }

        public bool Polyamory
        {
            get { return MAConfig.Polyamory; }
            set { MAConfig.Polyamory = value; }
        }

        public bool PregnancyPlus
        {
            get { return MAConfig.PregnancyPlus; }
            set { MAConfig.PregnancyPlus = value; }
        }

        public bool Cheating
        {
            get { return MAConfig.Cheating; }
            set { MAConfig.Cheating = value; }
        }

        public bool Incest
        {
            get { return MAConfig.Incest; }
            set { MAConfig.Incest = value; }
        }

        public bool SkipCourtship
        {
            get { return MAConfig.SkipCourtship; }
            set { MAConfig.SkipCourtship = value; }
        }

        public string FactionLeader
        {
            get { return MAConfig.FactionLeader; }
            set { MAConfig.FactionLeader = value; }
        }

        public bool RetryCourtship
        {
            get { return MAConfig.RetryCourtship; }
            set { MAConfig.RetryCourtship = value; }
        }

        public bool Debug
        {
            get { return MAConfig.Debug; }
            set { MAConfig.Debug = value; }
        }
    }

    internal static class MAConfig
    {
        private static bool _polyamory = false;

        private static bool _polygamy = false;

        private static bool _pregnancyPlus = false;

        private static bool _incest = false;

        private static bool _cheating = false;

        private static bool _skipCourtship = false;

        private static bool _retryCourtship = false;

        private static bool _debug = false;

        private static string _sexualOrientation = "Heterosexual";

        private static string _factionLeader = "Default";

        private static string _templateCharacter = "Default";


        [ConfigPropertyUnbounded]
        public static string TemplateCharacter
        {
            get
            {
                return _templateCharacter;
            }
            set
            {
                if (_templateCharacter != value)
                {
                    switch (_templateCharacter)
                    {
                        case "Default":
                            _templateCharacter = value;
                            break;
                        case "Wanderer":
                            _templateCharacter = value;
                            break;
                    }
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static string SexualOrientation
        {
            get
            {
                return _sexualOrientation;
            }
            set
            {
                if (_sexualOrientation != value)
                {
                    switch (_sexualOrientation)
                    {
                        case "Heterosexual":
                            _sexualOrientation = value;
                            break;
                        case "Homosexual":
                            _sexualOrientation = value;
                            break;
                        case "Bisexual":
                            _sexualOrientation = value;
                            break;
                    }
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static string FactionLeader
        {
            get
            {
                return _factionLeader;
            }
            set
            {
                if (_factionLeader != value)
                {
                    switch (_factionLeader)
                    {
                        case "Default":
                            _factionLeader = value;
                            break;
                        case "Player":
                            _factionLeader = value;
                            break;
                        case "Spouse":
                            _factionLeader = value;
                            break;
                    }
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool Polygamy
        {
            get
            {
                return _polygamy;
            }
            set
            {
                if (_polygamy != value)
                {
                    _polygamy = value;
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool Polyamory
        {
            get
            {
                return _polyamory;
            }
            set
            {
                if (_polyamory != value)
                {
                    _polyamory = value;
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool PregnancyPlus
        {
            get
            {
                return _pregnancyPlus;
            }
            set
            {
                if (_pregnancyPlus != value)
                {
                    _pregnancyPlus = value;
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool Incest
        {
            get
            {
                return _incest;
            }
            set
            {
                if (_incest != value)
                {
                    _incest = value;
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool Cheating
        {
            get
            {
                return _cheating;
            }
            set
            {
                if (_cheating != value)
                {
                    _cheating = value;
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool SkipCourtship
        {
            get
            {
                return _skipCourtship;
            }
            set
            {
                if (_skipCourtship != value)
                {
                    _skipCourtship = value;
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool RetryCourtship
        {
            get
            {
                return _retryCourtship;
            }
            set
            {
                if (_retryCourtship != value)
                {
                    _retryCourtship = value;
                    Save();
                }
            }
        }

        [ConfigPropertyUnbounded]
        public static bool Debug
        {
            get
            {
                return _debug;
            }
            set
            {
                if (_debug != value)
                {
                    _debug = value;
                    Save();
                }
            }
        }

        public static void Initialize()
        {
            string text = MAUtilities.LoadConfigFile();
            if (string.IsNullOrEmpty(text))
            {
                Save();
            }
            else
            {
                bool flag = false;
                string[] array = text.Split(new char[]
                {
                    '\n'
                });
                for (int i = 0; i < array.Length; i++)
                {
                    string[] array2 = array[i].Split(new char[]
                    {
                        '='
                    });
                    PropertyInfo property = typeof(MAConfig).GetProperty(array2[0]);
                    if (property is null)
                    {
                        flag = true;
                    }
                    else
                    {
                        string text2 = array2[1];
                        try
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                string value = Regex.Replace(text2, "\\r", "");
                                property.SetValue(null, value);
                            }
                            else if (property.PropertyType == typeof(float))
                            {
                                if (float.TryParse(text2, out float num))
                                {
                                    property.SetValue(null, num);
                                }
                                else
                                {
                                    flag = true;
                                }
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (int.TryParse(text2, out int num2))
                                {
                                    ConfigPropertyInt customAttribute = property.GetCustomAttribute<ConfigPropertyInt>();
                                    if (customAttribute is null || customAttribute.IsValidValue(num2))
                                    {
                                        property.SetValue(null, num2);
                                    }
                                    else
                                    {
                                        flag = true;
                                    }
                                }
                                else
                                {
                                    flag = true;
                                }
                            }
                            else if (property.PropertyType == typeof(bool))
                            {
                                if (bool.TryParse(text2, out bool flag2))
                                {
                                    property.SetValue(null, flag2);
                                }
                                else
                                {
                                    flag = true;
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        catch
                        {
                            flag = true;
                        }
                    }
                }
                if (flag)
                {
                    Save();
                }
            }
        }

        public static SaveResult Save()
        {
            Dictionary<PropertyInfo, object> dictionary = new();
            foreach (PropertyInfo propertyInfo in typeof(MAConfig).GetProperties())
            {
                if (propertyInfo.GetCustomAttribute<ConfigProperty>() is not null)
                {
                    dictionary.Add(propertyInfo, propertyInfo.GetValue(null, null));
                }
            }
            string text = "";
            foreach (KeyValuePair<PropertyInfo, object> keyValuePair in dictionary)
            {
                text = string.Concat(new string[]
                {
                    text,
                    keyValuePair.Key.Name,
                    "=",
                    keyValuePair.Value.ToString(),
                    "\n"
                });
            }
            SaveResult result = MAUtilities.SaveConfigFile(text);
            return result;
        }

        private interface IConfigPropertyBoundChecker<T>
        {
        }

        private abstract class ConfigProperty : Attribute
        {
        }

        private sealed class ConfigPropertyInt : ConfigProperty
        {
            public ConfigPropertyInt(int[] possibleValues, bool isRange = false)
            {
                _possibleValues = possibleValues;
                _isRange = isRange;
                bool isRange2 = _isRange;
            }

            public bool IsValidValue(int value)
            {
                if (_isRange)
                {
                    return value >= _possibleValues[0] && value <= _possibleValues[1];
                }
                int[] possibleValues = _possibleValues;
                for (int i = 0; i < possibleValues.Length; i++)
                {
                    if (possibleValues[i] == value)
                    {
                        return true;
                    }
                }
                return false;
            }

            private int[] _possibleValues;

            private bool _isRange;
        }

        private sealed class ConfigPropertyUnbounded : ConfigProperty
        {
        }
    }
}