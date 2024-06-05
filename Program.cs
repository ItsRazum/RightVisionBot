using Newtonsoft.Json;
using RightVisionBot.Back;
using Callbacks = RightVisionBot.Back.Callbacks;
using RightVisionBot.Back.Commands;
using RightVisionBot.Common;
using RightVisionBot.UI;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using RightVisionBot.Rating;
using Admin = RightVisionBot.Back.Commands.Admin.Admin;
using Critic = RightVisionBot.Back.Commands.Critic;
using Document = RightVisionBot.Back.Document;
// ReSharper disable All

//центральный файл проекта, обработка всех обновлений и адресация запросов
namespace RightVisionBot
{
    class Program
    {
        public static readonly ITelegramBotClient botClient = new TelegramBotClient(ConfigReader.Token);
        public static readonly sql database = new(ConfigReader.MySql);
        public static readonly long MemberGroupId = -1002074764678;
        public static readonly long CriticGroupId = -1001968408177;

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
            }
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine(JsonConvert.SerializeObject(update));
                if (update.CallbackQuery != null)
                {
                    CallbackQuery? callback = update.CallbackQuery;
                    RvUser rvUser = RvUser.Get(callback.From.Id);

                    // Обработка входящих Callback'ов от сервера
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
                            await Callbacks.Admin.Callbacks(botClient, update, RvUser.Get(update.CallbackQuery.From.Id));
                        else if (callback.Data.StartsWith("menu_") || callback.Data.StartsWith("permissions_"))
                            await Callbacks.MainMenu.Callbacks(botClient, update, rvUser);
                    }
                    else // Антиспам система
                        await botClient.AnswerCallbackQueryAsync(callback.Id, "Пожалуйста, не надо спамить! Если будешь продолжать - для тебя включится медленный режим, который может увеличиться вплоть до 10 секунд!", showAlert: true);
                }
                Message? message = update.Message;
                if (message != null)
                {
                    // Система защиты от несанкционированного доступа в группу
                    // Пример: если пользователь подал заявку на участие, и её приняли - он получает право Permission.MemberChat
                    // Permission.MemberChat позволяет находиться в группе для участников. Если этого права нету - бот кикает пользователя из группы
                    if (message.NewChatMembers != null)
                    {
                        string kickMessage = "Обнаружена попытка неавторизованного пользователя зайти в группу с ограниченным доступом! Удаляю его...";
                        var newUserId = message.NewChatMembers[0].Id;
                        var chatId = message.Chat.Id;
                        switch (chatId)
                        {
                            case -1002074764678: //Группа участников 
                                if (!RvUser.Get(newUserId).Has(Permission.MemberChat))
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, kickMessage);
                                    await botClient.BanChatMemberAsync(message.Chat, newUserId, DateTime.Now.AddMinutes(1));
                                }
                                break;
                            case -1001968408177: //Группа судей
                                if (!RvUser.Get(newUserId).Has(Permission.CriticChat))
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, kickMessage);
                                    await botClient.BanChatMemberAsync(message.Chat, newUserId, DateTime.Now.AddMinutes(1));
                                }
                                break;
                        }
                    }

                    // Обработка текстовых сообщений
                    if (message.From != null)
                    {
                        long userId = message.From.Id;
                        var rvUser = RvUser.Get(userId);

                        if (message.Document != null || message.Audio != null || message.Photo != null)
                            await Document.Handling(botClient, message, rvUser);
                        if (RvUser.Get(userId) == null || RvUser.Get(userId) != null && RvUser.Get(userId).RvLocation != RvLocation.Blacklist)
                            await General.Registration(botClient, message);

                        if (message.Text != null && rvUser != null && rvUser.Has(Permission.Messaging) && rvUser.RvLocation != RvLocation.Blacklist)
                        {
                            string lowercaseText = message.Text.ToLower();
                            await General.Commands(botClient, rvUser, message);
                            await Critic.Commands(botClient, rvUser, message);
                            await Admin.Commands(botClient, rvUser, message);

                            if (message.Chat.Type == ChatType.Private && RvUser.Get(userId).RvLocation == RvLocation.EditTrack)
                            {
                                RvMember.Get(userId).TrackStr = message.Text;
                                database.Read($"UPDATE `RV_C{RvMember.Get(userId).Status}` SET `track` = '{message.Text}' WHERE `userId` = {userId};", "");
                                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Member_Track_Updated", rvUser.Lang));
                                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил свой трек\n=====\nId:{message.From.Id}\nЯзык: {rvUser.Lang}\nЛокация: {rvUser.RvLocation}", disableNotification: true);
                                await botClient.SendTextMessageAsync(message.Chat, UserProfile.Profile(message), replyMarkup: Keyboard.ProfileOptions(rvUser, message));
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


        public static bool StringExists(string TableName, long userId) =>
            database.Read($"select * FROM `{TableName}` WHERE `userId` = {userId}", "userId").FirstOrDefault() != null;

        // Вспомогательный процедурный метод. Вызывается, когда нужно обновить статус пользователя
        public static void UpdateStatus(long id)
        {
            if (RvMember.Get(id) != null && RvCritic.Get(id) != null)
                RvUser.Get(id).Status = Status.CriticAndMember;
            else if (RvMember.Get(id) != null)
                RvUser.Get(id).Status = Status.Member;
            else if (RvCritic.Get(id) != null)
                RvUser.Get(id).Status = Status.Critic;
        }

        public static void UpdateRvLocation(long userId, RvLocation location) => RvUser.Get(userId).RvLocation = location;
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Console.WriteLine(JsonConvert.SerializeObject(exception));

        public static string About = "RightVision Bot\n" +
                               $"Дата сборки: {ConfigReader.BuildDate}\n" +
                               "Разработчик: @NtRazum\n\n" +
                               "Переводчики:\n" +
                               "UA: @crink1337 & @elec7reify\n" +
                               "KZ: @chrkovsky\n" +
                               "\nПрограммный стек:\n" +
                               "- Язык программирования: C#\n" +
                               "- GitHub: https://github.com/ItsRazum/RightVisionBot\n" +
                               "Использованные библиотеки:\n" +
                               "- Telegram.Bot\n" +
                               "- MySQL.Data\n" +
                               "Прочее ПО:\n" +
                               "- PhpMyAdmin\n" +
                               "- .NET 8.0\n\n" +
                               "Особые благодарности:\n" +
                               "- @Viktor290906 - за перевод старой версии английского языка\n" +
                               "- @elec7reify - за минимальную, но всё же помощь в разработке\n" +
                               "- @banan41ck - за рисование аватарки для бота";
    }
}
