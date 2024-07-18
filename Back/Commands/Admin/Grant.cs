using RightVisionBot.Common;
using RightVisionBot.User;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBot.Back.Commands.Admin
{
    class Grant
    {
        public static async Task Role(ITelegramBotClient botClient, Message message, RvUser _rvUser)
        {
            if (_rvUser.Has(Permission.Grant))
            {
                RvUser rvUser;
                Role newRole;
                try
                {
                    if (message.ReplyToMessage != null)
                    {
                        rvUser = RvUser.Get(message.ReplyToMessage.From.Id);
                        var sb = new StringBuilder(message.Text.ToLower().Replace("назначить ", ""));
                        sb[0] = char.ToUpper(sb[0]);
                        
                        newRole = Enum.Parse<Role>(sb.ToString());
                    }
                    else
                    {
                        string[] args = message.Text.ToLower().Replace("назначить ", "").Split(' ');
                        rvUser = RvUser.Get(long.Parse(args[0]));
                        var sb = new StringBuilder(args[1]);
                        sb[0] = char.ToUpper(sb[0]);
                        
                        newRole = Enum.Parse<Role>(sb.ToString());
                    }

                    rvUser.Role = newRole;
                    rvUser.Permissions += Permissions.Layouts[newRole];
                    await botClient.SendTextMessageAsync(message.Chat, "Пользователь назначен!");
                    await botClient.SendTextMessageAsync(rvUser.UserId,
                        $"Уважаемый пользователь!\nПоздравляю с назначением на должность: {rvUser.Role}\nГордись своим положением, и приноси пользу RightVision и всему гачи в целом!");

                }
                catch (Exception e) when(e.Message.Contains("Requested value"))
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Запрашиваемая должность не найдена!");
                }
            }
            else
                Permissions.NoPermission(message.Chat);
        }

        public static async Task Perm(ITelegramBotClient botClient, Message message, RvUser executor)
        {
            if (executor.Has(Permission.Grant))
                try
                {
                    Permission newPermission;
                    RvUser rvUser;
                    if (message.ReplyToMessage != null)
                    {
                        newPermission = Enum.Parse<Permission>(message.Text.Replace("+permission ", ""));
                        rvUser = RvUser.Get(message.ReplyToMessage.From.Id);
                    }
                    else
                    {
                        string[] args = message.Text.Replace("+permission ", "").Split(' ');
                        newPermission = Enum.Parse<Permission>(args[1]);
                        rvUser = RvUser.Get(long.Parse(args[0]));
                    }

                    try { rvUser.Permissions += newPermission; }
                    catch { await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!"); }

                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь получает право: Permission.{newPermission}");
                    if (newPermission is Permission.ChattingInCriticChat or Permission.ChattingInMemberChat)
                    {
                        ChatPermissions chatPermissions = new()
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
                        };

                        switch (newPermission)
                        {
                            case Permission.ChattingInCriticChat:
                                await botClient.RestrictChatMemberAsync(Program.CriticGroupId, rvUser.UserId, chatPermissions); break;
                            case Permission.ChattingInMemberChat:
                                await botClient.RestrictChatMemberAsync(Program.MemberGroupId, rvUser.UserId, chatPermissions); break;
                        }
                    }
                    
                }
                catch (Exception ex) when(ex.Message.Contains("Requested value"))
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Запрашиваемое право не найдено!");
                }
            else
                Permissions.NoPermission(message.Chat);
        }
    }
}
