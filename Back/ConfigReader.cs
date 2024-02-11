using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightVisionBot.Back
{
    class ConfigReader
    {
        static string _filePath = "config.json";
        static string _json;
        static Dictionary<string, string> phrases;

        static ConfigReader()
        {
            _json = File.ReadAllText(_filePath);
            phrases = JsonConvert.DeserializeObject<Dictionary<string, string>>(_json);
        }

        public static string Token => phrases["BotToken"];

        public static string MySql => phrases["MySQL"];
    }
}
