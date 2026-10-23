using System;
using System.Collections.Generic;
using CubeTower.Application.Localization;
using CubeTower.Core.Common;
using UnityEngine;

namespace CubeTower.Infrastructure.Localization
{
    /// <summary>
    /// Localization from a ScriptableObject table. Picks the device language,
    /// falls back to the table's fallback language, then to the key itself.
    /// </summary>
    public sealed class TableLocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, string> _texts = new();
        private readonly HashSet<string> _reportedMissingKeys = new();
        private readonly ILog _log;

        public TableLocalizationService(LocalizationTableAsset table, ILog log)
            : this(table, log, UnityEngine.Application.systemLanguage) { }

        public TableLocalizationService(LocalizationTableAsset table, ILog log, SystemLanguage language)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));

            if (table == null)
            {
                _log.Error("Localization table is not assigned. Keys will be shown instead of texts.");
                Language = language.ToString();
                return;
            }

            Language = language.ToString();
            Fill(table, table.FallbackLanguage);
            Fill(table, language);
        }

        public string Language { get; }

        public string Get(string key)
        {
            if (key != null && _texts.TryGetValue(key, out string text))
                return text;

            if (key != null && _reportedMissingKeys.Add(key))
                _log.Warning($"Missing localization for key '{key}'.");

            return key ?? string.Empty;
        }

        public string Get(string key, params object[] arguments)
        {
            string format = Get(key);
            
            try
            {
                return string.Format(format, arguments);
            }
            catch (FormatException exception)
            {
                _log.Error($"Localization format error for key '{key}'.", exception);
                return format;
            }
        }

        private void Fill(LocalizationTableAsset table, SystemLanguage language)
        {
            foreach (LocalizationEntry entry in table.Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Key))
                    continue;

                foreach (LocalizedText translation in entry.Translations)
                    if (translation != null && translation.Language == language && !string.IsNullOrEmpty(translation.Text))
                        _texts[entry.Key] = translation.Text;
            }
        }
    }
}
