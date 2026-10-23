using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CubeTower.Infrastructure.Localization
{
    /// <summary>
    /// Minimal localization table: key -> translations per language.
    /// Adding a language means adding translations, no code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "CubeTower/Localization Table", fileName = "LocalizationTable")]
    public sealed class LocalizationTableAsset : ScriptableObject
    {
        [SerializeField] private SystemLanguage _fallbackLanguage = SystemLanguage.English;
        [SerializeField] private List<LocalizationEntry> _entries = new();

        public SystemLanguage FallbackLanguage => _fallbackLanguage;
        public IReadOnlyList<LocalizationEntry> Entries => _entries;

#if UNITY_EDITOR
        public void EditorSetup(SystemLanguage fallbackLanguage, IEnumerable<LocalizationEntry> entries)
        {
            _fallbackLanguage = fallbackLanguage;
            _entries = entries.ToList();
        }

        /// <summary>Adds entries for keys that are missing; keeps texts edited by the user.</summary>
        public void EditorMerge(IEnumerable<LocalizationEntry> defaults)
        {
            foreach (LocalizationEntry entry in defaults)
                if (_entries.All(existing => existing.Key != entry.Key))
                    _entries.Add(entry);
        }
#endif
    }

    [Serializable]
    public sealed class LocalizationEntry
    {
        [SerializeField] private string _key;
        [SerializeField] private List<LocalizedText> _translations = new();

        public LocalizationEntry() { }

        public LocalizationEntry(string key, params LocalizedText[] translations)
        {
            _key = key;
            _translations = translations.ToList();
        }

        public string Key => _key;
        public IReadOnlyList<LocalizedText> Translations => _translations;
    }

    [Serializable]
    public sealed class LocalizedText
    {
        [SerializeField] private SystemLanguage _language;

        [SerializeField]
        [TextArea]
        private string _text;

        public LocalizedText() { }

        public LocalizedText(SystemLanguage language, string text)
        {
            _language = language;
            _text = text;
        }

        public SystemLanguage Language => _language;
        public string Text => _text;
    }
}
