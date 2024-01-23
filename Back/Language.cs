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
                langFilePath = Path.Combine("lang", "ru.json");
                json = File.ReadAllText(langFilePath);
                phrases = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                if (phrases.ContainsKey(phraseName))
                    return phrases[phraseName];
                else
                    return "❌Phrase not found";
            }
        }
    }
}