using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;

//система динамической мультиязычности
namespace RightVisionBot.Back
{
    public class Language
    {
        public static Dictionary<string, Dictionary<string, string>> Phrases = new();
        public static void Build(string[] lang)
        {
            Console.WriteLine("Начался процесс сборки языка...");
            foreach (var l in lang)
            {
                Console.WriteLine($"Сборка {l} языка...");
                var langDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine("lang", $"{l}.json")));
                Phrases.Add(l, langDictionary ?? throw new InvalidOperationException());
            }

            Console.WriteLine("Сборка завершена");
        }
        public static string GetPhrase(string phraseName, string language)
        {
            if (Phrases[language].ContainsKey(phraseName))
                return Phrases[language][phraseName];

            return Phrases[language].ContainsKey(phraseName) ? Phrases[language][phraseName] : "❌Phrase not found";
        }

        public static string GetUserStatusString(Status s, string lang) => GetPhrase($"Profile_{s}_Header", lang);
        public static string GetUserRoleString(Role r, string lang) => GetPhrase($"Profile_Role_{r}", lang);
    }
}