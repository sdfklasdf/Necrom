using System;
using System.Collections.Generic;
using System.Globalization;
namespace Necrom.Core.Domain
{
    [Serializable] public sealed class LocalizationTableData
    {
        public int schemaVersion=1;
        public string defaultLanguage;
        public string[] languages;
        public LocalizationEntry[] entries;
    }
    [Serializable] public sealed class LocalizationEntry
    {
        public string key;
        public LocalizationValue[] values;
    }
    [Serializable] public sealed class LocalizationValue
    {
        public string language;
        public string text;
    }
    // Engine-independent. External JSON parsing belongs to platform adapters.
    public sealed class LocalizationManager
    {
        private readonly Dictionary<string,Dictionary<string,string>> entries=new Dictionary<string,Dictionary<string,string>>(StringComparer.Ordinal);
        private readonly HashSet<string> languages=new HashSet<string>(StringComparer.Ordinal);
        private readonly string defaultLanguage;
        public string Language { get; private set; }
        public event Action LanguageChanged;
        public LocalizationManager(LocalizationTableData table)
        {
            if(table==null || table.schemaVersion!=1 || table.languages==null || table.entries==null || table.entries.Length==0)
                throw new ArgumentException("Invalid localization table/version.");
            foreach(var language in table.languages)
            {
                if(string.IsNullOrWhiteSpace(language) || Normalize(language)!=language || !languages.Add(language))
                    throw new ArgumentException("Missing/duplicate/noncanonical language.");
                try { _=CultureInfo.GetCultureInfo(language); }
                catch(CultureNotFoundException) { throw new ArgumentException("Unknown language culture."); }
            }
            if(table.defaultLanguage==null || !languages.Contains(table.defaultLanguage))
                throw new ArgumentException("Default language must be supported.");
            defaultLanguage=table.defaultLanguage; Language=defaultLanguage;
            foreach(var row in table.entries)
            {
                if(row==null || string.IsNullOrWhiteSpace(row.key) || entries.ContainsKey(row.key) || row.values==null)
                    throw new ArgumentException("Missing/duplicate localization key.");
                var values=new Dictionary<string,string>(StringComparer.Ordinal);
                foreach(var value in row.values)
                {
                    if(value==null || value.language==null || !languages.Contains(value.language) || value.text==null ||
                        values.ContainsKey(value.language))throw new ArgumentException("Invalid/duplicate localization value.");
                    values.Add(value.language,value.text);
                }
                if(!values.ContainsKey(defaultLanguage))throw new ArgumentException("Default translation required: "+row.key);
                entries.Add(row.key,values);
            }
        }
        private static string Normalize(string value)=>value?.Trim().Replace('_','-').ToLowerInvariant();
        public bool TrySetLanguage(string language)
        {
            var candidate=Normalize(language);
            if(candidate==null)return false;
            if(!languages.Contains(candidate))
            {
                int separator=candidate.IndexOf('-');
                if(separator<0 || !languages.Contains(candidate.Substring(0,separator)))return false;
                candidate=candidate.Substring(0,separator);
            }
            if(Language==candidate)return true;
            Language=candidate;
            LanguageChanged?.Invoke();
            return true;
        }
        public bool HasKey(string key)=>key!=null && entries.ContainsKey(key);
        public string Get(string key)
        {
            if(key==null || !entries.TryGetValue(key,out var values))return "["+(key??"null")+"]";
            return values.TryGetValue(Language,out var translated)?translated:values[defaultLanguage];
        }
        public string Format(string key,params object[] args)
            => string.Format(CultureInfo.GetCultureInfo(Language),Get(key),args??Array.Empty<object>());
    }
}
