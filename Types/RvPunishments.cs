using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightVisionBot.Types
{
    public class RvPunishment
    {
        public PunishmentType Type;
        public long GroupId;
        public string? Reason;
        public DateTime From;
        public DateTime To;

        public RvPunishment(RvPunishment.PunishmentType type, long groupId, string? reason, DateTime from, DateTime to)
        {
            Type = type;
            GroupId = groupId;
            Reason = reason;
            From = from;
            To = to;
        }

        public enum PunishmentType
        {
            Ban,
            Mute
        }
    }

    public class RvPunishments
    {
        public List<RvPunishment> Collection = new();
        private readonly long? _userId;

        public RvPunishments(long? userId = null)
        {
            _userId = userId;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var punishment in Collection)
            {
                StringBuilder one = new();
                one.Append($"{punishment.Type};");
                one.Append(punishment.GroupId + ";");
                one.Append(punishment.Reason + ";");
                one.Append(punishment.From.ToString(CultureInfo.GetCultureInfo("en-US").DateTimeFormat) + ";");
                one.Append(punishment.To.ToString(CultureInfo.GetCultureInfo("en-US").DateTimeFormat) + ",");
                sb.Append(one);
            }
            return sb.ToString();
        }

        public void Add(RvPunishment punishment)
        {
            Collection.Add(punishment);
            Collection.Reverse();
            if(_userId != null)
                Program.database.Read($"UPDATE RV_Users SET punishments = '{this}' WHERE userId = {_userId};", "");
        }
    }
}
