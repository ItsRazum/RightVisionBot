using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RightVisionBot.Back.Commands.Admin
{
    class Restriction
    {
        private static List<long> Hares = new();

        private static string Contacts(string lang) => Language.GetPhrase("Punishments_Contacts", lang);

        public static async Task Mute(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            long mutedId = message.ReplyToMessage.From.Id == null ? long.Parse(message.Text.Replace("/mute", "")) : message.ReplyToMessage.From.Id;
            ChatMember mutedMember = await botClient.GetChatMemberAsync(message.Chat, mutedId);
            string groupType = message.Chat.Id == -1001968408177 ? "организаторов" : "участников";
            if (mutedMember.Status is ChatMemberStatus.Member or ChatMemberStatus.Restricted)
            {
                Telegram.Bot.Types.User mutedUser = mutedMember.User;
                DateTime time = DateTime.Now.AddHours(1);

                RvUser.Get(mutedId).AddPunishment(RvPunishment.PunishmentType.Mute, message.Chat.Id, "не указана", DateTime.Now,time);
                await botClient.SendTextMessageAsync(message.Chat, $"Пользователь {mutedUser.FirstName} получает мут в группе!");
                await botClient.RestrictChatMemberAsync(message.Chat, mutedId, new ChatPermissions()
                {
                    CanSendAudios = false,
                    CanSendDocuments = false,
                    CanSendMessages = false,
                    CanSendVideos = false,
                    CanSendOtherMessages = false,
                    CanSendPhotos = false,
                    CanSendPolls = false,
                    CanSendVideoNotes = false,
                    CanSendVoiceNotes = false
                }, untilDate: time);
                try
                {
                    await botClient.SendTextMessageAsync(mutedId,
                        string.Format(Language.GetPhrase("Punishments_Mute_Notification", RvUser.Get(mutedId).Lang), groupType) +
                        Contacts(RvUser.Get(mutedId).Lang));
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Не удалось выдать наказание! Возможно, введён неправильный Id или команда написана с ошибкой");
                }
            }
        }

        public static async Task Ban(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            if (rvUser.Has(Permission.Ban))
            {
                long bannedId = message.ReplyToMessage.From.Id == null ? long.Parse(message.Text.Replace("/ban", "")) : message.ReplyToMessage.From.Id;
                ChatMember bannedMember = await botClient.GetChatMemberAsync(message.Chat, bannedId);
                string groupType = message.Chat.Id == -1001968408177 ? "организаторов" : "участников";
                if (bannedMember.Status is ChatMemberStatus.Member)
                {
                    Telegram.Bot.Types.User bannedUser = bannedMember.User;

                    RvUser.Get(bannedUser.Id).AddPunishment(RvPunishment.PunishmentType.Ban, message.Chat.Id, "не указана", DateTime.Now, DateTime.Now.AddDays(7));
                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь {bannedUser.FirstName} получает бан в группе!");
                    await botClient.BanChatMemberAsync(message.Chat, bannedId);
                    try
                    {
                        await botClient.SendTextMessageAsync(bannedId,
                            string.Format(Language.GetPhrase("Punishments_Ban_Notification", RvUser.Get(bannedId).Lang), groupType) +
                            Contacts(RvUser.Get(bannedId).Lang));
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Не удалось выдать наказание! Возможно, введён неправильный Id или команда написана с ошибкой");
                    }
                }
            }
        }

        public static async Task Blacklist(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            if (rvUser.Has(Permission.Ban))
            {
                long bannedId = message.ReplyToMessage.From.Id == null ? long.Parse(message.Text.Replace("/blacklist ", "")) : message.ReplyToMessage.From.Id;
                ChatMember bannedMember = await botClient.GetChatMemberAsync(message.Chat, bannedId);
                var bannedUser = bannedMember.User;
                await botClient.SendTextMessageAsync(message.Chat, $"Пользователь {bannedUser.FirstName} вносится в чёрный список RightVision!");
                await botClient.BanChatMemberAsync(-1002074764678, bannedId);
                await botClient.BanChatMemberAsync(-1001968408177, bannedId);
                try
                {
                    await botClient.SendTextMessageAsync(bannedId,
                        Language.GetPhrase("Punishments_Blacklist_Notification", RvUser.Get(bannedId).Lang) +
                                                                   Contacts(RvUser.Get(bannedId).Lang), replyMarkup: Keyboard.remove);
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Не удалось выдать наказание! Возможно, введён неправильный Id или команда написана с ошибкой");
                }

                RvUser.Get(bannedId).RvLocation = RvLocation.Blacklist;
            }
        }
        public static async Task Cleaning(ITelegramBotClient botClient, Message message)
        {
            if (RvUser.Get(message.From.Id).Has(Permission.Ban))
            {
                await botClient.SendTextMessageAsync(message.Chat, "Начинается проверка прав доступа у всех участников группы, это займёт некоторое время...");
                foreach (var user in Program.users)
                    try
                    {
                        var chatMember = await botClient.GetChatMemberAsync(-1002074764678, user.UserId);
                        if (chatMember.Status is ChatMemberStatus.Member)
                        {
                            long userId = chatMember.User.Id;
                            if (!RvUser.Get(userId).Has(Permission.MemberChat))
                                Hares.Add(userId);
                        }
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(message.Chat, $"Произошла неизвестная ошибка, проверка прав доступа прервана!");
                    }

                if (Hares.Count > 0)
                    await botClient.SendTextMessageAsync(message.Chat, $"Обнаружено {Hares.Count} безбилетников. Кикать их?", replyMarkup: Keyboard.KickHares);
                else
                    await botClient.SendTextMessageAsync(message.Chat, $"Безбилетников в группе нет. Всё в порядке!");
            }
            else
                Permissions.NoPermission(message.Chat);
        }

        public static async Task KickHares(ITelegramBotClient botClient, long chatId)
        {
            foreach (var hare in Hares)
            {
                await botClient.BanChatMemberAsync(chatId, hare, DateTime.UtcNow.AddMinutes(1));
                try
                {
                    await botClient.SendTextMessageAsync(hare, Language.GetPhrase("Punishments_HareKick_Notification", RvUser.Get(hare).Lang));
                }
                catch { }
            }
        }
    }
}
