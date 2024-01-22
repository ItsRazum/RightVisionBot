using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Commands.Admin
{
    class Restriction
    {
        private static string contacts = $"\nСчитаешь это ошибкой? Обратись к главным организаторам!" +
                                  $"\n" +
                                  $"\n@NtRazum - Демид" +
                                  $"\n@Gachimaker - Глеб";

        public static async Task Mute(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            long mutedId = message.ReplyToMessage.From.Id;
            ChatMember mutedMember = await botClient.GetChatMemberAsync(message.Chat, mutedId);
            string groupType = message.Chat.Id == -1001968408177 ? "организаторов" : "участников";
            if (mutedMember != null)
            {
                Telegram.Bot.Types.User mutedUser = mutedMember.User;
                DateTime time = DateTime.Now.AddHours(1);
                await botClient.SendTextMessageAsync(message.Chat, $"Пользователь {mutedUser.FirstName} получает мут в группе!");
                await botClient.RestrictChatMemberAsync(message.Chat, mutedId, new ChatPermissions()
                {
                    CanChangeInfo = false,
                    CanSendAudios = false,
                    CanSendDocuments = false,
                    CanSendMessages = false,
                    CanSendVideos = false,
                    CanSendOtherMessages = false,
                    CanSendPhotos = false,
                    CanSendPolls = false,
                    CanSendVideoNotes = false,
                    CanPinMessages = false,
                    CanSendVoiceNotes = false
                }, untilDate: time);
                try
                {
                    await botClient.SendTextMessageAsync(mutedId, $"Уважаемый пользователь!\n\nТы получаешь мут в группе {groupType} на 1 час за нарушение правил поведения." + contacts);
                }
                catch { }
            }
        }

        public static async Task Ban(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            if (rvUser.Has(Permission.Ban))
            {
                long bannedId = message.ReplyToMessage.From.Id;
                ChatMember bannedMember = await botClient.GetChatMemberAsync(message.Chat, bannedId);
                string groupType = message.Chat.Id == -1001968408177 ? "организаторов" : "участников";
                if (bannedMember != null)
                {
                    Telegram.Bot.Types.User bannedUser = bannedMember.User;
                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь {bannedUser.FirstName} получает бан в группе!");
                    await botClient.BanChatMemberAsync(message.Chat, bannedId);
                    try
                    {
                        await botClient.SendTextMessageAsync(bannedId, $"Уважаемый пользователь!\n\nТы получаешь бан в группе {groupType} за нарушение правил поведения." + contacts);
                    }
                    catch { }
                }
            }
        }

        public static async Task Blacklist(ITelegramBotClient botClient, RvUser rvUser, Message message)
        {
            if (rvUser.Has(Permission.Ban))
            {
                long bannedId = message.ReplyToMessage.From.Id;
                ChatMember bannedMember = await botClient.GetChatMemberAsync(message.Chat, bannedId);
                if (bannedMember != null)
                {
                    Telegram.Bot.Types.User bannedUser = bannedMember.User;
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"Пользователь {bannedUser.FirstName} вносится в чёрный список RightVision!");
                    await botClient.BanChatMemberAsync(-1002074764678, bannedId);
                    await botClient.BanChatMemberAsync(-1001968408177, bannedId);
                    try
                    {
                        await botClient.SendTextMessageAsync(bannedId,
                            $"Уважаемый пользователь!\n\nПо решению организаторов ты вносишься в чёрный список RightVision. С этого момента тебе недоступно использование бота и нахождение в наших официальных группах." +
                            contacts, replyMarkup: Keyboard.remove);
                    }
                    catch
                    { }

                    RvUser.Get(bannedId).RvLocation = RvLocation.Blacklist;
                }
            }
        }
        public static async Task Cleaning(ITelegramBotClient botClient, Message message)
        {
            
        }
    }
}
