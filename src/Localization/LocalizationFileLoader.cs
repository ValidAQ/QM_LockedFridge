using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace QM_LockedFridge.LocalizationSupport
{
    public static class LocalizationFileLoader
    {
        private static string GetLangCode(MGSC.Localization.Lang lang)
        {
            switch (lang)
            {
                case MGSC.Localization.Lang.EnglishUS:         return "en";
                case MGSC.Localization.Lang.Russian:           return "ru";
                case MGSC.Localization.Lang.German:            return "de";
                case MGSC.Localization.Lang.French:            return "fr";
                case MGSC.Localization.Lang.Spanish:           return "es";
                case MGSC.Localization.Lang.Polish:            return "pl";
                case MGSC.Localization.Lang.Turkish:           return "tr";
                case MGSC.Localization.Lang.BrazilianPortugal: return "pt";
                case MGSC.Localization.Lang.Korean:            return "ko";
                case MGSC.Localization.Lang.Japanese:          return "ja";
                case MGSC.Localization.Lang.ChineseSimp:       return "zh";
                default:                                       return null;
            }
        }

        /// <summary>
        /// Loads localization entries from a JSON resource embedded in
        /// <paramref name="callingAssembly"/> (defaults to the calling assembly).
        /// Expected format:
        /// <code>
        /// {
        ///   "key": {
        ///     "en": "English text",
        ///     "ru": "Russian text"
        ///   }
        /// }
        /// </code>
        /// Language codes: en, ru, de, fr, es, pl, tr, pt-BR, ko, ja, zh-Hans.
        /// Missing languages fall back to the "en" value.
        /// </summary>
        public static void LoadFromEmbeddedJson(
            string resourceName,
            Assembly callingAssembly = null,
            Action<string> logError  = null)
        {
            var assembly = callingAssembly ?? Assembly.GetCallingAssembly();
            var db = MGSC.Localization.Instance.db;

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                logError?.Invoke($"Embedded localization resource not found: {resourceName}");
                return;
            }

            string json;
            using (stream)
            using (var reader = new StreamReader(stream))
                json = reader.ReadToEnd();

            var entries = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
            if (entries == null)
                return;

            foreach (var entry in entries)
            {
                string key = entry.Key;
                var translations = entry.Value;

                if (!translations.TryGetValue("en", out string englishFallback))
                    englishFallback = string.Empty;

                foreach (var kvp in db)
                {
                    string langCode = GetLangCode(kvp.Key);
                    string value = langCode != null
                        && translations.TryGetValue(langCode, out string v)
                        && !string.IsNullOrEmpty(v)
                            ? v
                            : englishFallback;

                    var langDict = kvp.Value;
                    if (langDict.ContainsKey(key))
                        langDict[key] = value;
                    else
                        langDict.Add(key, value);
                }
            }
        }
    }
}
