using RightVisionBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using RightVisionBot.User;

namespace RightVisionBot.Back.Commands.Admin
{
    class Unbans
    {
        public static async Task Unban(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            if (rvUser.Has(Permission.Unban))
            {
                long bannedId = message.ReplyToMessage.From.Id == null ? long.Parse(message.Text.Replace("/mute ", "")) : message.ReplyToMessage.From.Id;
                string groupType = message.Chat.Id == -1001968408177 ? "организаторов" : "участников";
                string link = message.Chat.Id == -1001968408177 ? "https://t.me/+vUBCHXqsoP9lOTAy" : "https://t.me/+p-NYz5VLgYBjZTMy";

                try
                {
                    await botClient.UnbanChatMemberAsync(message.Chat, bannedId);
                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь разбанен!");
                    try
                    { await botClient.SendTextMessageAsync(bannedId, string.Format(Language.GetPhrase("Punishments_Unban_Notification", RvUser.Get(bannedId).Lang), groupType, link)); }
                    catch { /* :) */ }
                }
                catch
                { await botClient.SendTextMessageAsync(message.Chat, "Пользователь не забанен!"); }
            }
            else
                Permissions.NoPermission(message.Chat);
        }

        public static async Task Unmute(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            if (rvUser.Has(Permission.Unmute))
            {
                long mutedId = message.ReplyToMessage.From.Id == null
                    ? long.Parse(message.Text.Replace("/mute ", ""))
                    : message.ReplyToMessage.From.Id;
                try
                {
                    await botClient.RestrictChatMemberAsync(message.Chat, mutedId, new ChatPermissions()
                    {
                        CanSendAudios = true,
                        CanSendDocuments = true,
                        CanSendMessages = true,
                        CanSendVideos = true,
                        CanSendOtherMessages = true,
                        CanSendPhotos = true,
                        CanSendPolls = true,
                        CanSendVideoNotes = true,
                        CanSendVoiceNotes = true
                    });
                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь размучен!");
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь не замучен!");
                }
            }
            else
                Permissions.NoPermission(message.Chat);
        }

        public static async Task BlacklistOff(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            if (rvUser.Has(Permission.BlacklistOff))
            {
                long bannedId = message.ReplyToMessage.From.Id == null ? long.Parse(message.Text.Replace("/blacklist off ", "")) : message.ReplyToMessage.From.Id;
                ChatMember bannedMember = await botClient.GetChatMemberAsync(message.Chat, bannedId);
                var bannedUser = bannedMember.User;
                RvUser bannedRvUser = RvUser.Get(bannedId);
                try
                {
                    try { await botClient.UnbanChatMemberAsync(-1002074764678, bannedId); }
                    catch {  /* :) */  }

                    try { await botClient.UnbanChatMemberAsync(-1001968408177, bannedId); }
                    catch {  /* :) */  }

                    bannedRvUser.RvLocation = RvLocation.MainMenu;
                    bannedRvUser.Permissions.Add(Permission.Messaging);

                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь {bannedUser.FirstName} удалён из чёрного списка RightVision!");
                    await botClient.SendTextMessageAsync(bannedId, Language.GetPhrase("Punishments_BlacklistOff_Notification", bannedRvUser.Lang),
                        replyMarkup: Keyboard.MainMenu(bannedRvUser.Lang));
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь не вписан в чёрный список!");
                }

            }
            else Permissions.NoPermission(message.Chat);
        }
    }
}
