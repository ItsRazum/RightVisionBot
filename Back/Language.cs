using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//система динамической мультиязычности
namespace RightVisionBot.Back
{
    public class Language
    {
        public static string GetPhrase(string phraseName, string language)
        {
            string langFilePath = Path.Combine("lang", $"{language}.json");
            string json = File.ReadAllText(langFilePath);
            var phrases = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (phrases.ContainsKey(phraseName))
                return phrases[phraseName];
            else
            {
                string _langFilePath = Path.Combine("lang", "ru.json");
                string _json = File.ReadAllText(_langFilePath);
                var _phrases = JsonConvert.DeserializeObject<Dictionary<string, string>>(_json);

                if (_phrases.ContainsKey(phraseName))
                    return _phrases[phraseName];
                else
                    return "❌Phrase not found";
            }
        }
    }
}