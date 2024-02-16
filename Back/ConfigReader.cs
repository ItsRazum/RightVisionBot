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
        static Dictionary<string, string>? _phrases;

        static ConfigReader()
        {
            _json = File.ReadAllText(_filePath);
            _phrases = JsonConvert.DeserializeObject<Dictionary<string, string>>(_json);
        }

        public static string Token => _phrases["BotToken"];
        public static string MySql => _phrases["MySQL"];
        public static string BuildDate => _phrases["buildDate"];
    }
}
