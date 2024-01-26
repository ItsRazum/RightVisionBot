using RightVisionBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace RightVisionBot.Back.Commands.Admin
{
    class Unbans
    {
        public static async Task Unban(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            long bannedId = message.ReplyToMessage.From.Id == null ? long.Parse(message.Text.Replace("/mute ", "")) : message.ReplyToMessage.From.Id;
            ChatMember bannedMember = await botClient.GetChatMemberAsync(message.Chat, bannedId);
            string groupType = message.Chat.Id == -1001968408177 ? "организаторов" : "участников";
            string link  =     message.Chat.Id == -1001968408177 ? "https://t.me/+vUBCHXqsoP9lOTAy" : "https://t.me/+p-NYz5VLgYBjZTMy";

            try
            {
                await botClient.UnbanChatMemberAsync(message.Chat, bannedId);
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь разбанен!");
                try
                { await botClient.SendTextMessageAsync(bannedId, $"Уважаемый пользователь!\nС тебя был снят бан в группе {groupType}. Можешь заходить обратно по ссылке:\n{link}"); } 
                catch{}
            }
            catch
            {
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь не забанен!");
            }
        }

        public static async Task Unmute(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            if (rvUser.Has(Permission.Unmute))
            {
                long mutedId = message.ReplyToMessage.From.Id == null
                    ? long.Parse(message.Text.Replace("/mute ", ""))
                    : message.ReplyToMessage.From.Id;
                ChatMember mutedMember = await botClient.GetChatMemberAsync(message.Chat, mutedId);
                string groupType = message.Chat.Id == -1001968408177 ? "организаторов" : "участников";
                string link = message.Chat.Id == -1001968408177 ? "https://t.me/+vUBCHXqsoP9lOTAy" : "https://t.me/+p-NYz5VLgYBjZTMy";

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
                    }, untilDate: DateTime.Now);
                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь размучен!");
                }
                catch
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь не замучен!");
                }
            }
            else
                Permissions.NoPermission(message);
        }

        public static async Task BlacklistOff(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            long bannedId = message.ReplyToMessage.From.Id == null ? long.Parse(message.Text.Replace("/blacklist off ", "")) : message.ReplyToMessage.From.Id;
            ChatMember bannedMember = await botClient.GetChatMemberAsync(message.Chat, bannedId);
            var bannedUser = bannedMember.User;
            RvUser bannedRvUser = RvUser.Get(bannedId);
            try
            {
                try { await botClient.UnbanChatMemberAsync(-1002074764678, bannedId); } catch { }
                try { await botClient.UnbanChatMemberAsync(-1001968408177, bannedId); } catch { }

                bannedRvUser.RvLocation = RvLocation.MainMenu;
                bannedRvUser.AddPermissions(array: new[] { Permission.Messaging });

                await botClient.SendTextMessageAsync(message.Chat, $"Пользователь {bannedUser.FirstName} удалён из чёрного списка RightVision!");
                await botClient.SendTextMessageAsync(bannedId, "Уважаемый пользователь!\nКоманда RightVision приняла решение удалить тебя из чёрного списка. Очень надеюсь, что у неё больше не будет поводов вносить тебя обратно.",
                    replyMarkup: Keyboard.MainMenu(bannedRvUser.Lang));
            }
            catch
            {
                await botClient.SendTextMessageAsync(message.Chat, "Пользователь не вписан в чёрный список!");
            }
        }
    }
}
