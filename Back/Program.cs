using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RightVisionBot.Back.Commands;
using RightVisionBot.Back.Commands.Admin;
using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.UI;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
// ReSharper disable All

//центральный файл проекта, обработка всех обновлений и адресация запросов
namespace RightVisionBot.Back
{
    class Program
    {
        public static volatile List<RvUser> users = new();
        public static readonly ITelegramBotClient botClient = new TelegramBotClient("6450629882:AAFLdecqtb0qvrLFGE2JHxX8HVMB07fvxZQ");
        public static readonly sql database = new("server=127.0.0.1;uid=demid;pwd=Z2r757vnGK9J;database=phpmyadmin");

        static async Task Main(string[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            Console.WriteLine("Начался процесс запуска бота");
            Console.WriteLine("Восстановление данных пользователей...");
            DataRestorer.RestoreUsers();
            Console.WriteLine("Запуск бота...");
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
            Console.WriteLine("Завершено. RightVision Bot запущен и готов к работе!");

            while (true)
            {
                string? command = Console.ReadLine();
                if (command.StartsWith("send"))
                {
                    string[] commandWithArgs = command.Split(" ");
                    string msg = string.Join(" ", commandWithArgs.Skip(2));
                    await botClient.SendTextMessageAsync(long.Parse(commandWithArgs[1]), msg);
                }
                else if (command == "stop")
                {
                    cts.Cancel();
                    break;
                }
                else if (command == "restart")
                {
                    Console.WriteLine("Начался процесс перезапуска...\n");
                    cts.Cancel();
                    await Main(args);
                    break;
                }
            }
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine(JsonConvert.SerializeObject(update));
                if (update.CallbackQuery != null)
                {
                    var callback = update.CallbackQuery;
                    RvUser rvUser = RvUser.Get(callback.From.Id);

                    if (!rvUser.Cooldown.Enabled && rvUser.Has(Permission.Messaging) && rvUser.RvLocation != RvLocation.Blacklist)
                    {
                        if (callback.Data.StartsWith("c_"))
                            await Callbacks.Critic.Callbacks(botClient, update, rvUser);
                        else if (callback.Data.StartsWith("m_"))
                            await Callbacks.Member.Callbacks(botClient, update, rvUser);
                        else if (callback.Data.StartsWith("t_"))
                            await Callbacks.TrackCard.Callbacks(botClient, update);
                        else if (callback.Data.StartsWith("r_") || callback.Data.StartsWith("change"))
                            await Callbacks.Evaluation.Callbacks(botClient, update);
                        else if (callback.Data.StartsWith("h_"))
                            await Callbacks.Admin.Callbacks(botClient, update,
                                RvUser.Get(update.CallbackQuery.From.Id));
                        else if (callback.Data.StartsWith("menu_") || callback.Data.StartsWith("permissions_"))
                            await Callbacks.MainMenu.Callbacks(botClient, update, rvUser);
                    }
                    else
                        await botClient.AnswerCallbackQueryAsync(callback.Id, "Пожалуйста, не надо спамить! Если будешь продолжать - для тебя включится медленный режим, который может увеличиться вплоть до 10 секунд!", showAlert: true);
                }
                Message message = update.Message;
                if (message != null)
                {
                    if (message.NewChatMembers != null)
                    {
                        string kickMessage = "Обнаружена попытка неавторизованного пользователя зайти в группу с ограниченным доступом! Удаляю его...";
                        var newUserId = message.NewChatMembers[0].Id;
                        var chatId = message.Chat.Id;
                        switch (chatId)
                        {
                            case -1002074764678:
                                if ((RvMember.Get(newUserId) == null || RvUser.Get(newUserId).Has(Permission.MemberChat)) && RvUser.Get(message.From.Id).Role != Role.Admin)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, kickMessage);
                                    await botClient.BanChatMemberAsync(message.Chat, newUserId);
                                }
                                break;
                            case -1001968408177:
                                if ((RvCritic.Get(newUserId) == null || RvUser.Get(newUserId).Has(Permission.CriticChat)) && RvUser.Get(message.From.Id).Role != Role.Admin)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, kickMessage);
                                    await botClient.BanChatMemberAsync(message.Chat, newUserId);
                                }
                                break;
                        }
                    }

                    if (message.From != null)
                    {
                        long userId = message.From.Id;
                        var rvUser = RvUser.Get(userId);

                        if (message.Document != null || message.Audio != null || message.Photo != null)
                            await Document.Handling(botClient, message, rvUser);
                        if (RvUser.Get(userId) == null || (RvUser.Get(userId) != null && RvUser.Get(userId).RvLocation != RvLocation.Blacklist))
                            await General.Registration(botClient, message);

                        if (message.Text != null && rvUser != null && rvUser.Has(Permission.Messaging) && rvUser.RvLocation != RvLocation.Blacklist)
                        {
                            string lowercaseText = message.Text.ToLower();
                            await Document.Handling(botClient, message, rvUser);
                            await General.Commands(botClient, rvUser, update);
                            switch (rvUser.Status)
                            {
                                case Status.Critic:
                                    await Critic.Commands(botClient, rvUser, message);
                                    break;
                                case Status.Member:
                                    await MemberRoot.Commands(botClient, rvUser, message);
                                    break;
                                case Status.CriticAndMember:
                                    await Critic.Commands(botClient, rvUser, message);
                                    await MemberRoot.Commands(botClient, rvUser, message);
                                    break;
                            }

                            switch (rvUser.Role)
                            {
                                case Role.Moderator:
                                case Role.TechAdmin:
                                case Role.Developer:
                                case Role.Admin:
                                    await Admin.Commands(botClient, rvUser, message);
                                    break;
                                case Role.Curator:
                                    break;
                                case Role.Designer:
                                    break;
                                case Role.Translator:
                                    break;
                            }

                            switch (lowercaseText)
                            {
                                case "назад":
                                    switch (rvUser.RvLocation)
                                    {
                                        case RvLocation.CriticMenu:
                                            await UserProfile.Profile(message);
                                            break;
                                        case RvLocation.PreListening:
                                        case RvLocation.Evaluation:
                                            await botClient.SendTextMessageAsync(message.Chat, "Возвращаемся в судейское меню", replyMarkup: Keyboard.criticMenu); updateRvLocation(userId, RvLocation.CriticMenu);
                                            break;
                                    }
                                    break;
                                case "//rmkboard":
                                    if (message.From.Id == 901152811)
                                    {
                                        ReplyKeyboardRemove remove = new();
                                        await botClient.SendTextMessageAsync(message.Chat, "отключено", replyMarkup: remove);
                                    }
                                    break;
                                default:
                                    if (message.Chat.Type == ChatType.Private)
                                    {
                                        if (RvUser.Get(userId).RvLocation == RvLocation.EditTrack)
                                        {
                                            RvMember.Get(userId).TrackStr = message.Text;
                                            database.Read($"UPDATE `RV_C{RvMember.Get(userId).Status}` SET `track` = '{message.Text}' WHERE `userId` = {userId};", "");
                                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Member_Track_Updated", rvUser.Lang));
                                            await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил свой трек\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                                            await UserProfile.Profile(message);
                                        }
                                    }
                                    break;
                            }

                            if (rvUser != null)
                            {
                                if (message.Text == Language.GetPhrase("Keyboard_Choice_EditTrack", rvUser.Lang) && message.Chat.Type == ChatType.Private)
                                {
                                    ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang)) }) { ResizeKeyboard = true };
                                    RvMember.Get(userId).TrackStr = "_waiting+>" + RvMember.Get(userId).TrackStr;
                                    await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Member_Track_EnterNewTrack", rvUser.Lang), replyMarkup: backButton);
                                }

                                else if (message.Text.StartsWith("link ") && message.From.Id == 901152811)
                                {
                                    var args = message.Text.Split(" ");
                                    
                                    var idAsList = database.Read($"SELECT `userId` FROM `RV_Tracks` LIMIT 1 OFFSET {int.Parse(args[1]) - 1}", "userId");
                                    database.Read($"UPDATE `RV_Tracks` SET `link` = '{args[2]}' WHERE `userId` = {idAsList.FirstOrDefault()}", "");
                                    await botClient.SendTextMessageAsync(message.Chat, "Ссылка успешно привязана!");
                                    await botClient.SendTextMessageAsync(long.Parse(idAsList.FirstOrDefault()),
                                            "Уважаемый участник!" +
                                                  "\nОрганизаторы добавили ссылку на твой ремикс в облаке! Можешь качать и заливать к себе на канал :)" +
                                                    "\nЧтобы её получить - перейди в свой профиль и нажми \"Получить визуал ремикса\".");
                                }

                                else if (message.Text.StartsWith("/get "))
                                {
                                    string newMessage = message.Text.Replace("/get ", "");
                                    int value = int.Parse(newMessage);
                                    Track.SendFilesByOne(botClient, value);
                                }

                                else if (RvMember.Get(userId) != null && RvMember.Get(userId).Status is not "denied" or "waiting" or "unfinished")
                                {
                                    if (message.Text == Language.GetPhrase("Keyboard_Choice_SendTrack", rvUser.Lang) && message.Chat.Type == ChatType.Private && RvMember.Get(userId).Status is not "denied" or "waiting" or "unfinished")
                                    {
                                        if (RvMember.Get(userId) != null) 
                                            Track.Send(botClient, message);
                                        else 
                                        {
                                            InlineKeyboardMarkup onlyM = new InlineKeyboardMarkup(
                                                new[] { new[] { InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Profile_Form_Send_Member", rvUser.Lang), "m_send") } });
                                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_NotAMember", rvUser.Lang), replyMarkup: onlyM);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await botClient.SendTextMessageAsync(-4074101060, $"Произошла ошибка: {e.Message}\n\nСтек вызовов:\n{e.StackTrace}");
                Console.WriteLine(e.Message + JsonConvert.SerializeObject(e));
            }
        }


        public static bool StringExists(string TableName, long userId)
        {
            var query = $"select * FROM `{TableName}` WHERE `userId` = {userId}";
            var results = database.Read(query, "userId");
            string userIdFromDb = results.FirstOrDefault();
            if (userIdFromDb != null)
                return true;
            else
                return false;
        }

        public static void UpdateStatus(long id)
        {
            var fromCritics = database.Read(
                $"SELECT `userId` FROM `RV_Critics` WHERE `userId` = {id};",
                "userId");
            var fromMembers = database.Read(
                $"SELECT `userId` FROM `RV_Members` WHERE `userId` = {id};",
                "userId");
            if (fromMembers.FirstOrDefault() != null && fromCritics.FirstOrDefault() == null)
                database.Read(
                    $"UPDATE `RV_Users` SET `status` = 'member' WHERE `userId` = {id};",
                    "");
            else if (fromMembers.FirstOrDefault() == null && fromCritics.FirstOrDefault() != null)
                database.Read(
                    $"UPDATE `RV_Users` SET `status` = 'critic' WHERE `userId` = {id};",
                    "");
            else if (fromMembers.FirstOrDefault() != null && fromCritics.FirstOrDefault() != null)
                database.Read(
                    $"UPDATE `RV_Users` SET `status` = 'criticAndMember' WHERE `userId` = {id};",
                    "");
        }

        public static void updateRvLocation(long userId, RvLocation location) => RvUser.Get(userId).RvLocation = location;

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Console.WriteLine(JsonConvert.SerializeObject(exception));
    }
}
