using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using RightVisionBot.Back;
using RightVisionBot.Tracks;
using RightVisionBot.Tracks;
using RightVisionBot.UI;
using RightVisionBot.User;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;
// ReSharper disable All

//центральный файл проекта, обработка всех обновлений и адресация запросов
namespace RightVisionBot.Back
{
    class RV_User
    {
        public string userName { get; set; }
        public long userId { get; set; }
        public string lang { get; set; }
        public string location { get; set; }
    }

    class Program
    {
        public static ReplyKeyboardMarkup chooseLang = new(new[]
            {
                new[]
                {
                    new KeyboardButton("🇷🇺RU / CIS"),
                    new KeyboardButton("🇺🇦UA")
                },
                new[]
                {
                    new KeyboardButton("🇰🇿KZ"),
                    new KeyboardButton("🇬🇧EN")
                }
            })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup categories = new(new[]
            {
                new[]
                {
                    new KeyboardButton("🥉Bronze"),
                    new KeyboardButton("🥈Steel")
                },
                new[]
                {
                    new KeyboardButton("🥇Gold"),
                    new KeyboardButton("💎Brilliant")
                },
                new[] { new KeyboardButton("Отмена") }
            })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup criticMenu = new(new[]
            {
                new[] { new KeyboardButton("Оценивание ремиксов") },
                new[] { new KeyboardButton("Предварительное прослушивание") },
                new[] { new KeyboardButton("Назад") }
            })
            { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup actions = new(new[]
            {
                new[] { new KeyboardButton("Завершить прослушивание") },
                new[] { new KeyboardButton("Сменить категорию") },
                new[] { new KeyboardButton("Заблокировать ремикс") },
                new[] { new KeyboardButton("Одобрить ремикс") }
            })
            { ResizeKeyboard = true };

        public static InlineKeyboardMarkup cCategories = new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("❌Отклонить", "c_deny2") },
                new[] { InlineKeyboardButton.WithCallbackData("🥉Bronze", "c_bronze") },
                new[] { InlineKeyboardButton.WithCallbackData("🥈Steel", "c_steel") },
                new[] { InlineKeyboardButton.WithCallbackData("🥇Gold", "c_gold") },
                new[] { InlineKeyboardButton.WithCallbackData("💎Brilliant", "c_brilliant") }
            }
        );

        public static InlineKeyboardMarkup NextTrack = new(
            new[] { new[] { InlineKeyboardButton.WithCallbackData("Следующий ремикс", "nexttrack") } }
        );

        public static InlineKeyboardMarkup finalActions = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Изм. 1", "change1"),
                    InlineKeyboardButton.WithCallbackData("Изм. 2", "change2"),
                    InlineKeyboardButton.WithCallbackData("Изм. 3", "change3"),
                    InlineKeyboardButton.WithCallbackData("Изм. 4", "change4")
                },
                new[] { InlineKeyboardButton.WithCallbackData("Завершить", "r_send") }
            }
        );

        public static InlineKeyboardMarkup mCategories = new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("❌Отклонить", "m_deny2") },
                new[] { InlineKeyboardButton.WithCallbackData("🥉Bronze", "m_bronze") },
                new[] { InlineKeyboardButton.WithCallbackData("🥈Steel", "m_steel") },
                new[] { InlineKeyboardButton.WithCallbackData("🥇Gold", "m_gold") },
                new[] { InlineKeyboardButton.WithCallbackData("💎Brilliant", "m_brilliant") }
            }
        );

        public static InlineKeyboardMarkup memberAcceptOrDeny = new InlineKeyboardMarkup(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Взять кураторство над участником", "m_accept") },
                new[] { InlineKeyboardButton.WithCallbackData("Отклонить кандидатуру", "m_deny") }
            }
        );

        public static InlineKeyboardMarkup criticAcceptOrDeny = new InlineKeyboardMarkup(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Взять кураторство над судьёй", "c_accept") },
                new[] { InlineKeyboardButton.WithCallbackData("Отклонить кандидатуру", "c_deny") }
            }
        );

        public static ReplyKeyboardRemove remove = new ReplyKeyboardRemove();

        public static volatile List<RV_User> users = new();
        static readonly ITelegramBotClient botClient = new TelegramBotClient("token");
        static sql database = new("server=127.0.0.1;uid=phpmyadmin;pwd=12345;database=phpmyadmin");

        static async Task Main(string[] args)
        {
            if (args is null)
            { throw new ArgumentNullException(nameof(args)); }

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
                string command = Console.ReadLine();
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
            var message = update.Message;
            try
            {
                Console.WriteLine(JsonConvert.SerializeObject(update));
                if (message != null)
                {
                    if ((message is { Audio: not null, Chat.Type: ChatType.Private }) && GetUser(message.From.Id).location == "trackcard")
                    {
                        var fileId = message.Audio.FileId;
                        long userId = message.From.Id;
                        database.Read($"UPDATE `RV_Tracks` SET `track` = '{fileId}' WHERE `userId` = '{userId}';", "");
                        Track.GetTrack(userId).Track = fileId;
                        await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendTrack_Success", Program.GetUser(userId).lang));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сдал свой ремикс\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                        Track.Send(botClient, message);
                    }

                    if (message is { Photo: not null, Chat.Type: ChatType.Private })
                    {
                        var fileId = message.Photo.LastOrDefault()?.FileId;
                        long userId = message.From.Id;
                        database.Read($"UPDATE `RV_Tracks` SET `image` = '{fileId}' WHERE `userId` = '{userId}';", "");
                        Track.GetTrack(userId).Image = fileId;
                        if (Track.GetTrack(userId).Track != null)
                            database.Read($"UPDATE `RV_C{MemberRoot.GetMember(userId).Status}` SET `status` = 'waiting' WHERE `userId` = {userId};", "");
                        await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendImage_Success", Program.GetUser(userId).lang));
                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сдал обложку своего ремикса\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                        Track.Send(botClient, message);
                    }

                    if ((message is { Document: not null, Chat.Type: ChatType.Private }) && GetUser(message.From.Id).location == "trackcard")
                    {
                        var fileName = message.Document.FileName;
                        if (fileName.EndsWith(".txt"))
                        {
                            var fileId = message.Document.FileId;
                            long userId = message.From.Id;
                            database.Read($"UPDATE `RV_Tracks` SET `text` = '{fileId}' WHERE `userId` = '{userId}';", "");
                            Track.GetTrack(userId).Text = fileId;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendText_Success", Program.GetUser(userId).lang));
                            await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сдал текст своего ремикса\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                            Track.Send(botClient, message);

                        }
                        else if ((fileName.EndsWith(".wav") || fileName.EndsWith(".mp3") || fileName.EndsWith(".flac")) && GetUser(message.From.Id).location == "trackcard")
                        {
                            var fileId = message.Document.FileId;
                            long userId = message.From.Id;
                            database.Read($"UPDATE `RV_Tracks` SET `track` = '{fileId}' WHERE `userId` = '{userId}';", "");
                            Track.GetTrack(userId).Track = fileId;
                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendTrack_Success", Program.GetUser(userId).lang));
                            if (Track.GetTrack(userId).Image != null)
                                database.Read($"UPDATE `RV_C{MemberRoot.GetMember(userId).Status}` SET `status` = 'waiting' WHERE `userId` = {userId};", "");
                            await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сдал свой ремикс\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                            Track.Send(botClient, message);
                        }
                    }

                    if (message.NewChatMembers != null)
                    {
                        var userId = message.NewChatMembers[0].Id;
                        var chatId = message.Chat.Id;
                        switch (chatId)
                        {
                            case -1002074764678:
                                {
                                    var Id = database.Read($"SELECT `userId` FROM RV_Members WHERE `userId` = {userId} AND `status` != 'denied';", "userId");
                                    var NewMemberId = Id.FirstOrDefault();

                                    if (string.IsNullOrEmpty(NewMemberId))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "An unauthorized attempt to enter a group with restricted access was detected! Deleting a user from the group...");
                                        await botClient.BanChatMemberAsync(message.Chat, userId);
                                    }
                                }
                                break;
                            case -1001968408177:
                                {
                                    var Id = database.Read($"SELECT `userId` FROM RV_Critics WHERE `userId` = {userId} AND `status` != 'denied';", "userId");
                                    var NewCriticId = Id.FirstOrDefault();

                                    if (string.IsNullOrEmpty(NewCriticId))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "An unauthorized attempt to enter a group with restricted access was detected! Deleting a user from the group...");
                                        await botClient.BanChatMemberAsync(message.Chat, userId);
                                    }
                                }
                                break;
                        }
                    }
                    if (message.Text != null)
                    {
                        long userId = message.From.Id;
                        var lowercaseText = message.Text.ToLower();

                        switch (lowercaseText)
                        {
                            case "/start":
                                if (message.Chat.Type == ChatType.Private)
                                    await botClient.SendTextMessageAsync(message.Chat, "Choose lang:", replyMarkup: chooseLang);
                                break;
                            case "/sendtracks":
                                if (message.Chat.Id == -1001968408177) 
                                    Track.SendAllFiles(botClient);
                                break;
                            case "/profile":
                                if (message.ReplyToMessage != null && message.ReplyToMessage.From.IsBot == true)
                                    await botClient.SendTextMessageAsync(message.Chat, "🧾 Мой профиль RightVision:\n———\n🪪Статус: БОТ!!!!!\n🎖Категория участия: 🤓Душнила\n📍Место проживания: Хостинг за 150р\n💿Трек: Never Gonna Give You Up");
                                else
                                    UserProfile.Profile(botClient, update);
                                break;
                            case "получить купон на скидку на курс по гачимейкингу✅":
                                var couponCheck = database.Read($"SELECT `owner` FROM `RV_Coupons` WHERE `owner` = '@{message.From.Username}';", "owner");
                                if (couponCheck.FirstOrDefault() != $"@{message.From.Username}")
                                {
                                    var couponList = database.Read($"SELECT `coupon` FROM `RV_Coupons` WHERE `owner` = 'none' LIMIT 1", "coupon");
                                    string coupon = couponList.FirstOrDefault();
                                    ReplyKeyboardMarkup MainMenu = new(new[] { new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", GetUser(userId).lang)) } })
                                    { ResizeKeyboard = true };
                                    await botClient.SendTextMessageAsync(message.Chat,
                                        $"Твой персональный скидочный купон: `{coupon}`\nКак только ты захочешь приобрести курс у Таболича - отправь ему этот купон, и он выдаст тебе скидку! Поверь мне, курс стоит своих небольших денег, т.к. в нём просто море полезного контента :) Теперь ты можешь перейти в главное меню!", parseMode: ParseMode.Markdown, replyMarkup: MainMenu);
                                    database.Read($"UPDATE `RV_Coupons` SET `owner` = '@{message.From.Username}' WHERE `coupon` = '{coupon}';", "");
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} получил скидочный купон на курс от таболича\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                                }
                                break;
                            case "авторизовать":
                                if (message.From.Id == 901152811 && message.ReplyToMessage != null)
                                {
                                    var curatorQuery = $"SELECT * FROM `RV_Curators` WHERE `id` = '{message.ReplyToMessage.From.Id}';";
                                    List<string> CuratorId = database.Read(curatorQuery, "id");
                                    if (CuratorId != null)
                                    {
                                        var query = $"INSERT INTO `RV_Curators` (`id`) VALUES ('{message.ReplyToMessage.From.Id}');";
                                        database.Read(query, "");
                                        await botClient.SendTextMessageAsync(message.Chat, "Пользователь авторизован. Теперь он может брать кураторство над кандидатами (судьи и участники)");
                                    }
                                    else
                                        await botClient.SendTextMessageAsync(message.Chat, "Пользователь уже авторизован!");
                                }
                                break;
                            case "🇷🇺ru / cis":
                                if (message.Chat.Type == ChatType.Private)
                                    HubClass.Hub(botClient, update, "ru");
                                break;
                            case "🇺🇦ua":
                                if (message.Chat.Type == ChatType.Private)
                                    HubClass.Hub(botClient, update, "ua");
                                break;
                            case "🇰🇿kz":
                                if (message.Chat.Type == ChatType.Private)
                                    HubClass.Hub(botClient, update, "kz");
                                break;
                            case "🇬🇧en":
                                if (message.Chat.Type == ChatType.Private)
                                    HubClass.Hub(botClient, update, "en");
                                break;

                            case "открыть судейское меню":
                                updateLocation(userId, "criticmenu");
                                await botClient.SendTextMessageAsync(message.Chat, $"Добро пожаловать в судейское меню, коллега! Если ты являешься куратором - для тебя доступно предварительное прослушивание. В любом случае тебе доступно оценивание ремиксов твоей категории: {CriticRoot.GetCritic(userId).Status}", replyMarkup: criticMenu);
                                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} открыл судейское меню \n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
                                break;
                            case "оценивание ремиксов":
                                TrackEvaluation.Start(botClient, message);
                                break;
                            case "начать оценивание":
                                TrackEvaluation.First(botClient, message);
                                break;
                            case "предварительное прослушивание":
                                PreListening.Start(botClient, message);
                                break;
                            case "начать предварительное прослушивание":
                                PreListening.PreListenTrack(botClient, message);
                                break;
                            case "сменить категорию":
                                if (GetUser(userId).location == "prelistening")
                                    await botClient.SendTextMessageAsync(message.Chat, "Выбери категорию", replyMarkup: categories);
                                break;
                            case "назад":
                                switch (GetUser(userId).location)
                                {
                                    case "criticmenu":
                                        UserProfile.Profile(botClient, update);
                                        break;
                                    case "prelistening":
                                    case "evaluation":
                                        await botClient.SendTextMessageAsync(message.Chat, "Возвращаемся в судейское меню", replyMarkup: criticMenu); updateLocation(userId, "criticmenu");
                                        break;
                                }
                                break;
                            case "🥉bronze":
                                if (GetUser(userId).location == "prelistening")
                                {
                                    MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status = "bronze";
                                    MemberRoot.ChangeMemberCategory(PreListening.Get(userId).ArtistId, MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status);
                                    await botClient.SendTextMessageAsync(message.Chat, "Смена категории прошла успешно!", replyMarkup: actions);
                                    await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", GetUser(userId).lang), "🥉Bronze"));
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил категорию ремикса {MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Track} на Bronze \n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
                                }
                                break;
                            case "🥈steel":
                                if (GetUser(userId).location == "prelistening")
                                {
                                    MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status = "steel";
                                    MemberRoot.ChangeMemberCategory(PreListening.Get(userId).ArtistId, MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status);
                                    await botClient.SendTextMessageAsync(message.Chat, "Смена категории прошла успешно!", replyMarkup: actions);
                                    await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", GetUser(userId).lang), "🥈Steel"));
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил категорию ремикса {MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Track} на Steel \n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
                                }
                                break;
                            case "🥇gold":
                                if (GetUser(userId).location == "prelistening")
                                {
                                    MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status = "gold";
                                    MemberRoot.ChangeMemberCategory(PreListening.Get(userId).ArtistId, MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status);
                                    await botClient.SendTextMessageAsync(message.Chat, "Смена категории прошла успешно!", replyMarkup: actions);
                                    await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", GetUser(userId).lang), "🥇Gold"));
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил категорию ремикса {MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Track} на Gold \n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
                                }
                                break;
                            case "💎brilliant":
                                if (GetUser(userId).location == "prelistening")
                                {
                                    MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status = "brilliant";
                                    MemberRoot.ChangeMemberCategory(PreListening.Get(userId).ArtistId, MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status);
                                    await botClient.SendTextMessageAsync(message.Chat, "Смена категории прошла успешно!", replyMarkup: actions);
                                    await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, string.Format(Language.GetPhrase("Member_Messages_PreListening_CategoryChanged", GetUser(userId).lang), "💎Brilliant"));
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил категорию ремикса {MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Track} на Brilliant \n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
                                }
                                break;
                            case "заблокировать ремикс":
                                if (GetUser(userId).location == "prelistening")
                                {
                                    ReplyKeyboardMarkup yesno = new(new[]
                                        {
                                            new[] { new KeyboardButton("Да") },
                                            new[] { new KeyboardButton("Нет") }
                                        })
                                        { ResizeKeyboard = true };
                                    await botClient.SendTextMessageAsync(message.Chat,
                                        "Ты уверен, что хочешь заблокировать этот ремикс?", replyMarkup: yesno);
                                }
                                break;
                            case "да":
                                if (GetUser(userId).location == "prelistening")
                                {
                                    MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status = "denied";
                                    database.Read($"UPDATE `RV_Tracks` SET `status` = 'denied' WHERE `userId` = '{PreListening.Get(userId).ArtistId}'", "");
                                    await botClient.SendTextMessageAsync(message.Chat, "Ремикс заблокирован!");
                                    await botClient.SendTextMessageAsync(PreListening.Get(userId).ArtistId, Language.GetPhrase("Member_Messages_PreListening_Blocked", GetUser(userId).lang));
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} заблокировал ремикс {MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Track} \n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
                                    PreListening.NextTrack(botClient, message);
                                }
                                break;
                            case "нет":
                                if (GetUser(userId).location == "prelistening")
                                { botClient.SendTextMessageAsync(message.Chat, "Выбери действие", replyMarkup: actions); }
                                break;
                            case "одобрить ремикс":
                                if (GetUser(userId).location == "prelistening")
                                {
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} одобрил ремикс {MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Track} \n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
                                    await botClient.SendTextMessageAsync(message.Chat, "Ремикс допущен к дальнейшему оцениванию!");
                                    database.Read($"UPDATE `RV_Tracks` SET `status` = 'ok' WHERE `userId` = '{PreListening.Get(userId).ArtistId}'", "");
                                    database.Read($"UPDATE `RV_C{MemberRoot.GetMember(PreListening.Get(userId).ArtistId).Status}` SET `status` = 'ok' WHERE `userId` = {PreListening.Get(userId).ArtistId}", "");
                                    PreListening.NextTrack(botClient, message);
                                }
                                break;

                            case "завершить прослушивание":
                                if (GetUser(userId).location == "prelistening")
                                {
                                    await botClient.SendTextMessageAsync(message.Chat,
                                        "Ты вышел из режима прослушивания!");
                                    UserProfile.Profile(botClient, update);
                                    database.Read(
                                        $"UPDATE `RV_Tracks` SET `status` = 'waiting' WHERE `userId` = {PreListening.Get(userId).ArtistId}",
                                        "");
                                    database.Read($"DELETE FROM `RV_PreListening` WHERE `listenerId` = '{PreListening.Get(userId).ListenerId}';", "");
                                    PreListening.preListeners.Remove(PreListening.Get(userId));
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} закрыл предварительное прослушивание\n=====\nId:{message.From.Id}\nЯзык: {Program.GetUser(userId).lang}\nЛокация: {Program.GetUser(userId).location}", disableNotification: true);
                                }
                                break;
                            case "//rmkboard":
                                if (message.From.Id == 901152811)
                                {
                                    ReplyKeyboardRemove remove = new();
                                    await botClient.SendTextMessageAsync(message.Chat, "отключено", replyMarkup: remove);
                                }
                                break;
                            case "c":
                                foreach (RV_User user in users)
                                { Console.WriteLine($"{user.userName}\n{user.userId}\n{user.lang}\n"); }
                                break;
                            case "mc":
                                foreach (Member member in MemberRoot.newMembers)
                                { Console.WriteLine($"{member.Name}\n{member.Telegram}\n{member.UserId}\n{member.Country}\n{member.City}\n{member.Link}\n{member.Rate}\n{member.Curator}"); }
                                break;
                            case "cc":
                                foreach (Critic critic in CriticRoot.newCritics)
                                { Console.WriteLine($"{critic.Name}\n{critic.Telegram}\n{critic.UserId}\n{critic.Link}\n{critic.Rate}\n{critic.About}\n{critic.WhyYou}"); }
                                break;
                            case "1":
                            case "2":
                            case "3":
                            case "4":
                            case "5":
                            case "6":
                            case "7":
                            case "8":
                            case "9":
                            case "10":
                                if (message.ReplyToMessage != null)
                                {
                                    int Rate = int.Parse(message.Text);
                                    TrackEvaluation.Get(userId).General = Rate;
                                    await botClient.EditMessageReplyMarkupAsync(message.Chat, message.ReplyToMessage.MessageId,
                                        TrackEvaluation.RatingSystem(message.From.Id));
                                }
                                break;
                            default:
                                if (message.Chat.Type == ChatType.Private)
                                {
                                    if (MemberRoot.GetMember(userId) != null && MemberRoot.GetMember(userId).Track.Contains("_waiting+>"))
                                    {
                                        if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                                        {
                                            MemberRoot.GetMember(userId).Track = MemberRoot.GetMember(userId).Track.Substring(10);
                                            UserProfile.Profile(botClient, update);
                                        }
                                        else
                                        {
                                            MemberRoot.GetMember(userId).Track = message.Text;
                                            database.Read($"UPDATE `RV_C{MemberRoot.GetMember(userId).Status}` SET `track` = '{message.Text}' WHERE `userId` = {userId};", "");
                                            await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Member_Track_Updated", GetUser(userId).lang));
                                            await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} сменил свой трек\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                                            UserProfile.Profile(botClient, update);
                                        }
                                    }
                                }
                                break;
                        }

                        if (GetUser(userId) != null)
                        {
                            if (message.Text == Language.GetPhrase("Keyboard_Choice_Apply", GetUser(userId).lang) +
                                "📨" && message.Chat.Type == ChatType.Private)
                                MemberRoot.EnterName(botClient, update);
                            else if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang) && GetUser(userId).location == "trackcard")
                                Track.Send(botClient, message);
                            else if (message.Text == Language.GetPhrase("Keyboard_Choice_About", GetUser(userId).lang) + "❓" && message.Chat.Type == ChatType.Private)
                                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_About", GetUser(userId).lang));

                            else if (message.Text == Language.GetPhrase("Keyboard_Choice_Critic", GetUser(userId).lang) && message.Chat.Type == ChatType.Private)
                                CriticRoot.EnterName(botClient, update);

                            else if (message.Text == Language.GetPhrase("Keyboard_Choice_MainMenu", GetUser(userId).lang) && message.Chat.Type == ChatType.Private)
                                HubClass.Hub(botClient, update, GetUser(userId).lang);

                            else if (message.Text == Language.GetPhrase("Keyboard_Choice_Sending_Subscribe", GetUser(userId).lang) + "📬" && message.Chat.Type == ChatType.Private)
                                HubClass.SubscribeSending(botClient, update);

                            else if (message.Text == Language.GetPhrase("Keyboard_Choice_Sending_Unsubscribe", GetUser(userId).lang) + "📬" && message.Chat.Type == ChatType.Private)
                                HubClass.UnsubscribeSending(botClient, update);

                            else if (message.Text == Language.GetPhrase("Keyboard_Choice_MyProfile", GetUser(userId).lang) + "👤" && message.Chat.Type == ChatType.Private)
                                UserProfile.Profile(botClient, update);

                            else if (message.Text == Language.GetPhrase("Keyboard_Choice_EditTrack", GetUser(userId).lang) && message.Chat.Type == ChatType.Private)
                            {
                                ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang)) }) { ResizeKeyboard = true };
                                MemberRoot.GetMember(userId).Track = "_waiting+>" + MemberRoot.GetMember(userId).Track;
                                await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Member_Track_EnterNewTrack", GetUser(userId).lang), replyMarkup: backButton);
                            }

                            else if (message.Text.StartsWith("/membernews"))
                            {
                                if (message.From.Id is 901152811 or 5151585471)
                                {

                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал новостную рассылку для гачимейкеров\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}", disableNotification: true);
                                    string newMessage = message.Text.Replace("/membernews", "");
                                    int i = 0;
                                    int b = 0;
                                    foreach (var member in MemberRoot.newMembers)
                                    {
                                        try
                                        { await botClient.SendTextMessageAsync(member.UserId, newMessage); i++; }
                                        catch
                                        { b++; }
                                    }

                                    Console.WriteLine($"Рассылка завершена. {i} получили сообщение, {b} не получили");
                                }
                            }
                            else if (message.Text.StartsWith("/get "))
                            {
                                string newMessage = message.Text.Replace("/get ", "");
                                int value = int.Parse(newMessage);
                                Track.SendFilesByOne(botClient, value);
                            }
                            else if (message.Text.StartsWith("/news"))
                            {
                                if (message.From.Id is 901152811 or 5151585471)
                                {
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал новостную рассылку\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}", disableNotification: true);
                                    string newMessage = message.Text.Replace("/news", "");
                                    int i;
                                    for (i = 0; i < Data.Subscribers.Count; i++)
                                    { await botClient.SendTextMessageAsync(Data.Subscribers[i], newMessage); }

                                    Console.WriteLine($"Рассылка завершена. {i} получили сообщение");
                                }
                            }

                            else if (message.Text.StartsWith("/tech"))
                            {
                                if (message.From.Id is 901152811)
                                {
                                    await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} начал техническую рассылку\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}", disableNotification: true);
                                    string newMessage = message.Text.Replace("/tech", "");
                                    var idList = database.Read("SELECT `id` FROM `RV_Users`;", "id");
                                    int i = 0;
                                    int b = 0;
                                    foreach (var id in idList)
                                    {
                                        try
                                        {
                                            await botClient.SendTextMessageAsync(long.Parse(id), newMessage);
                                            i++;
                                        }
                                        catch { b++; }
                                    }

                                    Console.WriteLine($"Рассылка завершена. {i} получили сообщение, {b} не получили");
                                }
                            }

                            else if (message.Text.StartsWith("/check") && message.From.Id is 703169649 or 901152811)
                            {
                                string newMessage = message.Text.Replace("/check ", "");
                                await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} проверил купон {newMessage}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                                var couponList = database.Read($"SELECT `coupon` FROM `RV_Coupons` WHERE `coupon` = '{newMessage}';", "coupon");
                                if (couponList.FirstOrDefault() == null)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Купон не найден!");
                                }
                                else
                                {
                                    var ownerList = database.Read($"SELECT `owner` FROM `RV_Coupons` WHERE `coupon` = '{newMessage}';", "owner");
                                    switch (ownerList.FirstOrDefault())
                                    {
                                        case "none":
                                        case "-1":
                                            await botClient.SendTextMessageAsync(message.Chat,
                                                "Данный купон был обнаружен, однако он никому не принадлежит!");
                                            break;
                                        default:
                                            await botClient.SendTextMessageAsync(message.Chat, $"Данный купон обнаружен! его владелец: {ownerList.FirstOrDefault()}");
                                            break;
                                    }
                                }
                            }

                            else if (message.Text.ToLower().StartsWith("заблокировать ") && message.From.Id is 703169649 or 901152811)
                            {
                                if (message.Text.ToLower().StartsWith("заблокировать участие "))
                                {
                                    string newMessage = message.Text.ToLower().Replace("заблокировать участие ", "");
                                    var memberAsList = database.Read($"SELECT `userId` FROM `RV_Members` WHERE `userId` = '{newMessage}';", "userId");
                                    if (memberAsList.FirstOrDefault() == null)
                                        await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
                                    else
                                    {
                                        database.Read($"UPDATE `RV_Members` SET `status` = 'denied' WHERE `userId` = '{newMessage}';", "");
                                        await botClient.SendTextMessageAsync(message.Chat, $"Участие пользователя Id:{newMessage} заблокировано");
                                        await botClient.SendTextMessageAsync(long.Parse(newMessage),
                                            string.Format(Language.GetPhrase("Member_Messages_FormBlocked",
                                                GetUser(long.Parse(newMessage)).lang), message.From.FirstName + " " + message.From.LastName));
                                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} заблокировал участие Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}", disableNotification: true);
                                        try
                                        {
                                            await botClient.SendTextMessageAsync(-1002074764678, $"Пользователь Id:{newMessage} получает бан на участие в RightVision за нарушение правил ивента.");
                                            await botClient.BanChatMemberAsync(-1002074764678, long.Parse(newMessage));
                                        }
                                        catch { }
                                    }
                                }
                                else if (message.Text.ToLower().StartsWith("заблокировать судейство "))
                                {
                                    string newMessage = message.Text.ToLower().Replace("заблокировать судейство ", "");
                                    var criticAsList = database.Read($"SELECT `userId` FROM `RV_Critics` WHERE `userId` = '{newMessage}';", "userId");
                                    if (criticAsList.FirstOrDefault() == null)
                                        await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
                                    else
                                    {
                                        database.Read($"UPDATE `RV_Critics` SET `status` = 'denied' WHERE `userId` = '{newMessage}';", "");

                                        await botClient.SendTextMessageAsync(message.Chat, $"Судейство пользователя Id:{newMessage} заблокировано");
                                        await botClient.SendTextMessageAsync(long.Parse(newMessage),
                                            string.Format(Language.GetPhrase("Critic_Messages_FormBlocked",
                                                GetUser(long.Parse(newMessage)).lang), message.From.FirstName + " " + message.From.LastName));
                                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} заблокировал судейство Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}", disableNotification: true);
                                    }
                                }
                            }

                            else if (message.Text.ToLower().StartsWith("аннулировать ") && message.From.Id is 703169649 or 901152811)
                            {
                                if (message.Text.ToLower().StartsWith("аннулировать участие "))
                                {
                                    string newMessage = message.Text.ToLower().Replace("аннулировать участие ", "");
                                    var memberAsList = database.Read($"SELECT `userId` FROM `RV_Members` WHERE `userId` = '{newMessage}';", "userId");
                                    if (memberAsList.FirstOrDefault() == null)
                                        await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
                                    else
                                    {
                                        database.Read($"DELETE FROM `RV_Members` WHERE `userId` = '{newMessage}';", "");
                                        MemberRoot.newMembers.Remove(MemberRoot.GetMember(long.Parse(newMessage)));

                                        await botClient.SendTextMessageAsync(message.Chat, $"Участие пользователя Id:{newMessage} аннулировано");
                                        await botClient.SendTextMessageAsync(long.Parse(newMessage),
                                            string.Format(Language.GetPhrase("Member_Messages_FormCanceled",
                                                GetUser(long.Parse(newMessage)).lang), message.From.FirstName + " " + message.From.LastName));
                                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} аннулировал участие Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}", disableNotification: true);
                                    }
                                }
                                else if (message.Text.ToLower().StartsWith("аннулировать судейство "))
                                {
                                    string newMessage = message.Text.ToLower().ToLower().Replace("аннулировать судейство ", "");
                                    CriticRoot.newCritics.Remove(CriticRoot.GetCritic(long.Parse(newMessage)));

                                    var criticAsList = database.Read($"SELECT `userId` FROM `RV_Critics` WHERE `userId` = '{newMessage}';", "userId");
                                    if (criticAsList.FirstOrDefault() == null)
                                        await botClient.SendTextMessageAsync(message.Chat, "Пользователь не найден!");
                                    else
                                    {
                                        database.Read($"DELETE FROM `RV_Critics` WHERE `userId` = '{newMessage}';", "");
                                        await botClient.SendTextMessageAsync(message.Chat, $"Судейство пользователя Id:{newMessage} аннулировано");
                                        await botClient.SendTextMessageAsync(long.Parse(newMessage),
                                            string.Format(Language.GetPhrase("Critic_Messages_FormCanceled",
                                                GetUser(long.Parse(newMessage)).lang), message.From.FirstName + " " + message.From.LastName));
                                        await botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} аннулировал судейство Id:{newMessage}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}", disableNotification: true);
                                    }
                                }
                            }

                            else if (MemberRoot.GetMember(userId) != null && MemberRoot.GetMember(userId).Status is not "denied" or "waiting" or "unfinished")
                            {
                                if (message.Text == Language.GetPhrase("Keyboard_Choice_SendTrack", GetUser(userId).lang) && message.Chat.Type == ChatType.Private && MemberRoot.GetMember(userId).Status is not "denied" or "waiting" or "unfinished")
                                {
                                    if (MemberRoot.GetMember(userId) != null)
                                        Track.Send(botClient, message);
                                    else
                                    {
                                        InlineKeyboardMarkup onlyM = new InlineKeyboardMarkup(
                                            new[] { new[] { InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Profile_Form_Send_Member", Program.GetUser(userId).lang), "m_send") } }
                                        );
                                        await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_NotAMember", GetUser(userId).lang), replyMarkup: onlyM);
                                    }
                                }

                                else if (message.Text == Language.GetPhrase("Profile_Track_SendTrack", Program.GetUser(userId).lang) + "♂" && message.Chat.Type == ChatType.Private)
                                {
                                    ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang)) }) { ResizeKeyboard = true };
                                    await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendTrack_Instruction", Program.GetUser(userId).lang), replyMarkup: backButton);
                                }

                                else if (message.Text == Language.GetPhrase("Profile_Track_SendImage", Program.GetUser(userId).lang) + "🖼" && message.Chat.Type == ChatType.Private)
                                {
                                    ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang)) }) { ResizeKeyboard = true };
                                    await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendImage_Instruction", Program.GetUser(userId).lang), replyMarkup: backButton);
                                }

                                else if (message.Text == Language.GetPhrase("Profile_Track_SendText", Program.GetUser(userId).lang) + "📝" && message.Chat.Type == ChatType.Private)
                                {
                                    ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang)) }) { ResizeKeyboard = true };
                                    await botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Profile_Track_SendText_Instruction", Program.GetUser(userId).lang), replyMarkup: backButton);
                                }
                            }
                        }
                    }
                }

                if (update.CallbackQuery != null)
                {
                    var callback = update.CallbackQuery;
                    long userId = callback.From.Id;
                    var callbackQuery = update.CallbackQuery.Data;
                    string fullname = callback.From.FirstName + callback.From.LastName;
                        /*
                    string m_Message = callback.Message.Text.Length > 191
                        ? callback.Message.Text.Substring(0, callback.Message.Text.Length - 191)
                        : callback.Message.Text;
                        */
                    List<string> CuratorId = database.Read($"SELECT * FROM `RV_Curators` WHERE `id` = '{callback.From.Id}';", "id");
                    string curatorId = CuratorId.FirstOrDefault();
                    switch (callbackQuery)
                    {
                        case "t_CheckTrack":
                            try
                            {
                                var track = new InputFileId(Track.GetTrack(userId).Track);
                                await botClient.SendDocumentAsync(userId, track,
                                    caption: "Это файл твоего ремикса, который ты скидывал!");
                            }
                            catch
                            {
                                await botClient.SendTextMessageAsync(userId, "Ты ещё не скидывал ремикс!");
                            }

                            break;
                        case "t_CheckImage":
                            try
                            {
                                var image = new InputFileId(Track.GetTrack(userId).Image);
                                await botClient.SendPhotoAsync(userId, image,
                                    caption: "Это обложка ремикса, которую ты скидывал!");
                            }
                            catch
                            {
                                await botClient.SendTextMessageAsync(userId, "Ты ещё не скидывал обложку!");
                            }

                            break;
                        case "t_CheckText":
                            try
                            {
                                var text = new InputFileId(Track.GetTrack(userId).Text);
                                await botClient.SendDocumentAsync(userId, text,
                                    caption: "Это файл текста твоего ремикса, который ты скидывал!");
                            }
                            catch
                            {
                                await botClient.SendTextMessageAsync(userId, "Ты ещё не скидывал текст!");
                            }

                            break;

                        case "c_accept":
                        {
                            if (curatorId == null)
                                return;
                            else
                            {
                                Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                                long criticId = long.Parse(match.Groups[1].Value);

                                CriticRoot.GetCritic(criticId).Curator = callback.From.Id;
                                var query =
                                    $"UPDATE `RV_Critics` SET `curator` = '{callback.From.Id}' WHERE `userId` =  {criticId};";
                                database.Read(query, "");
                                await botClient.EditMessageTextAsync(callback.Message.Chat,
                                    update.CallbackQuery.Message.MessageId,
                                    $"{callback.Message.Text}\n\nОтветственный за судью: {update.CallbackQuery.From.FirstName}",
                                    replyMarkup: cCategories);
                                await botClient.SendTextMessageAsync(-4074101060,
                                    $"Пользователь @{update.CallbackQuery.From.Username} взял кураторство над судьёй Id:{criticId}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}",
                                    disableNotification: true);
                            }
                        }
                            break;
                        case "c_deny":
                        {
                            if (curatorId == null)
                                return;
                            else
                            {
                                Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                                long criticId = long.Parse(match.Groups[1].Value);

                                CriticRoot.GetCritic(criticId).Curator = callback.From.Id;
                                var query =
                                    $"UPDATE `RV_Critics` SET `curator` = '{callback.From.Id}' WHERE `userId` =  {criticId};";
                                database.Read(query, "");
                                await botClient.EditMessageTextAsync(callback.Message.Chat, callback.Message.MessageId,
                                    $"{callback.Message.Text}\n\nОтветственный за судью: {update.CallbackQuery.From.FirstName}\n❌Заявка отклонена!");
                                await botClient.SendTextMessageAsync(criticId,
                                    string.Format(
                                        Language.GetPhrase("Critic_Messages_FormDenied", GetUser(criticId).lang),
                                        fullname));
                                var updateCriticStatus =
                                    $"UPDATE `RV_Critics` SET `status` = 'denied' WHERE `userId` = {criticId};";
                                database.Read(updateCriticStatus, "");
                                await botClient.SendTextMessageAsync(-4074101060,
                                    $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру судьи Id:{criticId}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}",
                                    disableNotification: true);
                            }
                        }
                            break;


                        case "m_accept":
                            if (curatorId == null)
                                return;
                            else
                            {
                                Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                                long memberId = long.Parse(match.Groups[1].Value);

                                MemberRoot.GetMember(memberId).Curator = callback.From.Id;
                                var query =
                                    $"UPDATE `RV_Members` SET `curator` = '{callback.From.Id}' WHERE `userId` =  {memberId};";
                                database.Read(query, "");
                                await botClient.EditMessageTextAsync(callback.Message.Chat,
                                    update.CallbackQuery.Message.MessageId,
                                    $"{{m_Message}}\n\nОтветственный за участника: {update.CallbackQuery.From.FirstName}",
                                    replyMarkup: mCategories);
                                await botClient.SendTextMessageAsync(-4074101060,
                                    $"Пользователь @{update.CallbackQuery.From.Username} взял кураторством над участником Id:{memberId}\\n=====\\nId:{{message.From.Id}}\\nЯзык: {{GetUser(userId).lang}}",
                                    disableNotification: true);
                            }

                            break;
                        case "m_deny":
                            if (curatorId == null)
                                return;
                            else
                            {
                                Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                                long memberId = long.Parse(match.Groups[1].Value);

                                MemberRoot.GetMember(memberId).Curator = callback.From.Id;
                                var query =
                                    $"UPDATE `RV_Members` SET `curator` = '{callback.From.Id}' WHERE `userId` =  {memberId};";
                                database.Read(query, "");
                                await botClient.EditMessageTextAsync(callback.Message.Chat,
                                    update.CallbackQuery.Message.MessageId,
                                    $"{{m_Message}}\n\nОтветственный за участника: {update.CallbackQuery.From.FirstName}\n❌Заявка отклонена!");
                                await botClient.SendTextMessageAsync(memberId,
                                    string.Format(
                                        Language.GetPhrase("Member_Messages_FormDenied", GetUser(memberId).lang),
                                        fullname));
                                var updateMemberStatus =
                                    $"UPDATE `RV_Members` SET `status` = 'denied' WHERE `userId` = {memberId};";
                                database.Read(updateMemberStatus, "");
                                await botClient.SendTextMessageAsync(-4074101060,
                                    $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру участника Id:{memberId}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}",
                                    disableNotification: true);
                            }

                            break;


                        case "m_deny2":
                        {
                            Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                            long memberId = long.Parse(match.Groups[1].Value);
                            if (callback.From.Id == MemberRoot.GetMember(memberId).Curator)
                            {
                                await botClient.EditMessageTextAsync(callback.Message.Chat,
                                    update.CallbackQuery.Message.MessageId,
                                    $"{callback.Message.Text}\n❌Заявка отклонена!");
                                await botClient.SendTextMessageAsync(memberId,
                                    string.Format(
                                        Language.GetPhrase("Member_Messages_FormDenied", GetUser(memberId).lang),
                                        fullname));
                                var updateMemberStatus =
                                    $"UPDATE `RV_Members` SET `status` = 'denied' WHERE `userId` = {memberId};";
                                database.Read(updateMemberStatus, "");
                                await botClient.SendTextMessageAsync(-4074101060,
                                    $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру участника Id:{memberId}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}",
                                    disableNotification: true);
                            }
                        }
                            break;
                        case "m_bronze":
                            MemberRoot.SetMemberCategory(botClient, update, "🥉Bronze");
                            break;
                        case "m_steel":
                            MemberRoot.SetMemberCategory(botClient, update, "🥈Steel");
                            break;
                        case "m_gold":
                            MemberRoot.SetMemberCategory(botClient, update, "🥇Gold");
                            break;
                        case "m_brilliant":
                            MemberRoot.SetMemberCategory(botClient, update, "💎Brilliant");
                            break;

                        case "c_deny2":
                        {
                            Match match = Regex.Match(callback.Message.Text, @"Id:\s*(\d+)");
                            long criticId = long.Parse(match.Groups[1].Value);
                            if (callback.From.Id == CriticRoot.GetCritic(criticId).Curator)
                            {
                                await botClient.EditMessageTextAsync(callback.Message.Chat,
                                    update.CallbackQuery.Message.MessageId,
                                    $"{callback.Message.Text}\n❌Заявка отклонена!");
                                await botClient.SendTextMessageAsync(criticId,
                                    string.Format(
                                        Language.GetPhrase("Critic_Messages_FormDenied", GetUser(criticId).lang),
                                        fullname));
                                var updateCriticStatus =
                                    $"UPDATE `RV_Critics` SET `status` = 'denied' WHERE `userId` = {criticId};";
                                database.Read(updateCriticStatus, "");
                                await botClient.SendTextMessageAsync(-4074101060,
                                    $"Пользователь @{update.CallbackQuery.From.Username} отклонил кандидатуру критика Id:{criticId}\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}",
                                    disableNotification: true);
                            }
                        }
                            break;
                        case "c_bronze":
                            CriticRoot.SetCriticCategory(botClient, update, "🥉Bronze");
                            break;
                        case "c_steel":
                            CriticRoot.SetCriticCategory(botClient, update, "🥈Steel");
                            break;
                        case "c_gold":
                            CriticRoot.SetCriticCategory(botClient, update, "🥇Gold");
                            break;
                        case "c_brilliant":
                            CriticRoot.SetCriticCategory(botClient, update, "💎Brilliant");
                            break;

                        case "m_send":
                            MemberRoot.EnterName(botClient, update);
                            break;
                        case "c_send":
                            await botClient.SendTextMessageAsync(update.Message.Chat,
                                Language.GetPhrase("Critic_Messages_EnrollmentClosed",
                                    Program.GetUser(update.CallbackQuery.Message.From.Id).lang));
                            break;

                        case "r_lower":
                            if (TrackEvaluation.Get(callback.From.Id).General is 0 or 1)
                                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Поставить оценку меньше 1 нельзя!", showAlert: true);
                            else
                            {
                                TrackEvaluation.Get(callback.From.Id).General--;
                                await botClient.EditMessageTextAsync(chatId:callback.Message.Chat, messageId:update.CallbackQuery.Message.MessageId, text:callback.Message.Text,  replyMarkup: TrackEvaluation.RatingSystem(callback.From.Id));
                            }
                            break;
                        case "r_higher":
                            if (TrackEvaluation.Get(callback.From.Id).General == 10)
                                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Поставить оценку больше 10 нельзя!", showAlert: true);
                            else
                            {
                                TrackEvaluation.Get(callback.From.Id).General++;
                                await botClient.EditMessageTextAsync(chatId: callback.Message.Chat, messageId: update.CallbackQuery.Message.MessageId, text: callback.Message.Text, replyMarkup: TrackEvaluation.RatingSystem(callback.From.Id));
                            }
                            break;
                        case "r_enter":
                        {
                            string property = null;
                            var voter = TrackEvaluation.Get(userId);

                            if (voter.Rate1 == 0)
                            { property = "Rate1"; voter.Rate1 = voter.General; }
                            else if (voter.Rate2 == 0)
                            { property = "Rate2"; voter.Rate2 = voter.General; }
                            else if (voter.Rate3 == 0)
                            { property = "Rate3"; voter.Rate3 = voter.General; }
                            else if (voter.Rate4 == 0)
                            { property = "Rate4"; voter.Rate4 = voter.General; }

                            await botClient.EditMessageTextAsync(
                                chatId: callback.Message.Chat,
                                messageId: update.CallbackQuery.Message.MessageId,
                                text: TrackEvaluation.RatesNot0(userId) ? $"Твоя оценка инструментала: {voter.Rate1}\nТвоя оценка гачивокала: {voter.Rate2}\nТвоя оценка технического исполнения: {voter.Rate3}\nТвоя оценка творческого исполнения: {voter.Rate4}" : TrackEvaluation.EnterVote(callback.From.Id, property),
                                replyMarkup: TrackEvaluation.RatesNot0(userId) ? finalActions : TrackEvaluation.RatingSystem(callback.From.Id));
                            }
                            break;
                        case "back":
                            await botClient.EditMessageTextAsync(
                                chatId: callback.Message.Chat,
                                messageId: update.CallbackQuery.Message.MessageId,
                                text: TrackEvaluation.RollBackVote(callback.From.Id),
                                replyMarkup: TrackEvaluation.RatesNot0(userId) ? finalActions : TrackEvaluation.RatingSystem(callback.From.Id));
                            break;
                        case "count":
                            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Если ты хочешь вручную вписать оценку - напиши цифру в чат от 1 до 10 самостоятельно!", showAlert: true);
                            break;
                        case "change1":
                        case "change2":
                        case "change3":
                        case "change4":
                            TrackEvaluation.ChangeRate(botClient, update);
                            break;
                        case "r_send":
                        {
                            var voter = TrackEvaluation.Get(userId); 
                            double finalRate = (voter.Rate1 + voter.Rate2 + voter.Rate3 + voter.Rate4) / 4.0;

                            await botClient.EditMessageTextAsync(
                                chatId: callback.Message.Chat,
                                messageId: update.CallbackQuery.Message.MessageId,
                                text: update.CallbackQuery.Message.Text + $"\n\n{voter.Rate1}+{voter.Rate2}+{voter.Rate3}+{voter.Rate4}={voter.Rate1 + voter.Rate2 + voter.Rate3 + voter.Rate4} / 4\nИтоговая оценка: {finalRate}",
                                replyMarkup: NextTrack);
                            database.Read($"UPDATE `RV_C{CriticRoot.GetCritic(callback.From.Id).Status}` SET `{callback.From.Id}` = {finalRate} WHERE `userId` = {TrackEvaluation.Get(callback.From.Id).ArtistId};", "");
                        }
                            break;
                        case "nexttrack":
                            TrackEvaluation.NextTrack(botClient, callback);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                await botClient.SendTextMessageAsync(-4074101060, $"Произошла ошибка: {e.Message}\n\nСтек вызовов:\n{e.StackTrace}");
                Console.WriteLine(e.Message + JsonConvert.SerializeObject(e));
            }
        }

        public static RV_User GetUser(long userId)
        {
            foreach (RV_User user in users)
            { if (user.userId == userId) return user; }
            return null;
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


        public static void Form(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            var userId = message.From.Id;

            ReplyKeyboardMarkup chooseRate = new(new[]
            {
                new[]
                {
                    new KeyboardButton("1"),
                    new KeyboardButton("2"),
                    new KeyboardButton("3"),
                    new KeyboardButton("4")
                },
                new[]
                {
                    new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                }
            })
            { ResizeKeyboard = true };


            ReplyKeyboardMarkup MainMenu = new(new[]
                { new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", GetUser(userId).lang)) } })
            { ResizeKeyboard = true };

            ReplyKeyboardMarkup backButton = new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang)) }) { ResizeKeyboard = true };
            if (CriticRoot.GetCritic(userId) != null && CriticRoot.GetCritic(userId).UserId == userId)
            {
                if (CriticRoot.GetCritic(userId).Name == "0") //Вводится имя
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        var query = $"DELETE FROM `RV_Critics` WHERE `userId` = '{userId}';";
                        database.Read(query, "");
                        CriticRoot.newCritics.Remove(CriticRoot.GetCritic(userId));
                        botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} отменил заполнение заявки на судейство\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                        HubClass.SelectRole(botClient, update);
                    }




                    else if (message.Text == "0" || message.Text.Contains("'"))
                    {
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    }
                    else
                    {
                        CriticRoot.GetCritic(userId).Name = message.Text;
                        {
                            var query = $"UPDATE `RV_Critics` SET `name` = '{message.Text}' WHERE `userId` = {userId};";
                            database.Read(query, "");
                            botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterLink", GetUser(userId).lang), message.Text), replyMarkup: backButton);
                        }
                    }
                }
                else if (CriticRoot.GetCritic(userId).Name != "0" && CriticRoot.GetCritic(userId).Link == "0") //Вводится ссылка
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        { var query = $"UPDATE `RV_Critics` SET `name` = '0' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterName", GetUser(userId).lang), replyMarkup: backButton);
                    }




                    else if (!message.Text.StartsWith("https://"))
                    {
                        botClient.SendTextMessageAsync(message.Chat,
                            Language.GetPhrase("Critic_Messages_IncorrectFormat", GetUser(userId).lang));
                    }
                    else if (message.Text == "0" || message.Text.Contains("'"))
                    {
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    }
                    else
                    {
                        CriticRoot.GetCritic(userId).Link = message.Text;
                        { var query = $"UPDATE `RV_Critics` SET `Link` = '{message.Text}' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_EnterRate", GetUser(userId).lang), replyMarkup: chooseRate);
                    }
                }
                else if (CriticRoot.GetCritic(userId).Name != "0" &&
                         CriticRoot.GetCritic(userId).Link != "0" &&
                         CriticRoot.GetCritic(userId).Rate == "0") //Вводится оценка
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        { var query = $"UPDATE `RV_Members` SET `link` = '0' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        CriticRoot.GetCritic(userId).Link = "0";
                        botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterLink", GetUser(userId).lang), MemberRoot.GetMember(userId).Name), replyMarkup: backButton);
                    }




                    else if (message.Text == "0" || message.Text.Contains("'"))
                    {
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    }
                    else
                    {
                        CriticRoot.GetCritic(userId).Rate = message.Text;
                        {
                            var query = $"UPDATE `RV_Critics` SET `rate` = '{message.Text}' WHERE `userId` =  {userId};";
                            database.Read(query, "");
                        }
                        botClient.SendTextMessageAsync(message.Chat,
                            Language.GetPhrase("Critic_Messages_EnterAboutYou",
                                GetUser(userId).lang), replyMarkup: backButton);
                    }
                }
                else if (CriticRoot.GetCritic(userId).Name != "0" &&
                         CriticRoot.GetCritic(userId).Link != "0" &&
                         CriticRoot.GetCritic(userId).Rate != "0" &&
                         CriticRoot.GetCritic(userId).About == "0") //Вводится о себе
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        { var query = $"UPDATE `RV_Members` SET `rate` = '0' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        CriticRoot.GetCritic(userId).Link = "0";
                        botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterRate", GetUser(userId).lang), CriticRoot.GetCritic(userId).Name), replyMarkup: chooseRate);
                    }




                    else if (message.Text == "0" || message.Text.Contains("'"))
                    {
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    }
                    else
                    {
                        CriticRoot.GetCritic(userId).About = message.Text;
                        {
                            var query =
                                $"UPDATE `RV_Critics` SET `about` = '{message.Text}' WHERE `userId` =  {userId} ;";
                            database.Read(query, "");
                        }
                        botClient.SendTextMessageAsync(message.Chat,
                            Language.GetPhrase("Critic_Messages_EnterWhyYou",
                                GetUser(userId).lang), replyMarkup: backButton);
                    }
                }
                else if (CriticRoot.GetCritic(userId).Name != "0" &&
                         CriticRoot.GetCritic(userId).Link != "0" &&
                         CriticRoot.GetCritic(userId).Rate != "0" &&
                         CriticRoot.GetCritic(userId).About != "0" &&
                         CriticRoot.GetCritic(userId).WhyYou == "0") //Вводится почему ты
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        { var query = $"UPDATE `RV_Members` SET `about` = '0' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        CriticRoot.GetCritic(userId).About = "0";
                        botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Critic_Messages_EnterAboutYou", GetUser(userId).lang), CriticRoot.GetCritic(userId).Name), replyMarkup: backButton);
                    }




                    else if (message.Text == "0" || message.Text.Contains("'"))
                    {
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    }
                    else
                    {
                        CriticRoot.GetCritic(userId).WhyYou = message.Text;
                        { var query = $"UPDATE `RV_Critics` SET `whyyou` = '{message.Text}' WHERE `userId` =  {userId} ;"; database.Read(query, ""); }
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Critic_Messages_FormSubmitted", GetUser(userId).lang), replyMarkup: MainMenu);
                        botClient.SendTextMessageAsync(-1001968408177,
                            $"Пришла новая заявка на должность судьи!\n\n" +
                            $"Id: {CriticRoot.GetCritic(userId).UserId}\n" +
                            $"Имя: {CriticRoot.GetCritic(userId).Name}\n" +
                            $"Тег: {CriticRoot.GetCritic(userId).Telegram}\n" +
                            $"Ссылка на канал: {CriticRoot.GetCritic(userId).Link}\n" +
                            $"Субъективная оценка навыков: {CriticRoot.GetCritic(userId).Rate}\n" +
                            $"Что написал о себе: {CriticRoot.GetCritic(userId).About}\n" +
                            $"Почему мы должны его принять: {CriticRoot.GetCritic(userId).WhyYou}\n",
                            replyMarkup: criticAcceptOrDeny);
                    }
                }
            }

            if (MemberRoot.GetMember(userId) != null &&
                MemberRoot.GetMember(userId).UserId == userId)
            {
                if (MemberRoot.GetMember(userId).Name == "0")
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        var query = $"DELETE FROM `RV_Members` WHERE `userId` = '{userId}';";
                        database.Read(query, "");
                        MemberRoot.newMembers.Remove(MemberRoot.GetMember(userId));
                        botClient.SendTextMessageAsync(-4074101060, $"Пользователь @{message.From.Username} отменил заполнение заявки на участие\n=====\nId:{message.From.Id}\nЯзык: {GetUser(userId).lang}\nЛокация: {GetUser(userId).location}", disableNotification: true);
                        HubClass.SelectRole(botClient, update);
                    }
                    else if (message.Text == "0" || message.Text.Contains("'"))
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    else
                    {
                        { var query = $"UPDATE `RV_Members` SET `name` = '{message.Text}' WHERE `userId` = {userId}"; database.Read(query, ""); }
                        MemberRoot.GetMember(userId).Name = message.Text;
                        botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterCountry", GetUser(userId).lang), message.Text), replyMarkup: backButton);
                    }

                }
                else if (MemberRoot.GetMember(userId).Name != "0" &&
                         MemberRoot.GetMember(userId).Country == "0")
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        MemberRoot.GetMember(userId).Name = "0";
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterName", GetUser(userId).lang));
                    }
                    else if (message.Text == "0" || message.Text.Contains("'"))
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    else
                    {
                        { var query = $"UPDATE `RV_Members` SET `country` = '{message.Text}' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        MemberRoot.GetMember(userId).Country = message.Text;
                        botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterCity", GetUser(userId).lang), MemberRoot.GetMember(userId).Name), replyMarkup: backButton);
                    }
                }
                else if (MemberRoot.GetMember(userId).Name != "0" &&
                         MemberRoot.GetMember(userId).Country != "0" &&
                         MemberRoot.GetMember(userId).City == "0")
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        { var query = $"UPDATE `RV_Members` SET `country` = '0' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        MemberRoot.GetMember(userId).Country = "0";
                        botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterCountry", GetUser(userId).lang), MemberRoot.GetMember(userId).Name), replyMarkup: backButton);
                    }
                    else if (message.Text == "0" || message.Text.Contains("'"))
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    else
                    {
                        { var query = $"UPDATE `RV_Members` SET `city` = '{message.Text}' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        MemberRoot.GetMember(userId).City = message.Text;
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterLink", GetUser(userId).lang), replyMarkup: backButton);
                    }
                else if (MemberRoot.GetMember(userId).Name != "0" &&
                         MemberRoot.GetMember(userId).Country != "0" &&
                         MemberRoot.GetMember(userId).City != "0" &&
                         MemberRoot.GetMember(userId).Link == "0")
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        { var query = $"UPDATE `RV_Members` SET `city` = '0' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        MemberRoot.GetMember(userId).City = "0";
                        botClient.SendTextMessageAsync(message.Chat, string.Format(Language.GetPhrase("Member_Messages_EnterCity", GetUser(userId).lang), MemberRoot.GetMember(userId).Name), replyMarkup: backButton);
                    }
                    else if (message.Text == "0" || message.Text.Contains("'"))
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    else
                    {
                        { var query = $"UPDATE `RV_Members` SET `link` = '{message.Text}' WHERE `userId` = {userId};"; database.Read(query, ""); }
                        MemberRoot.GetMember(userId).Link = message.Text;
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterRate", GetUser(userId).lang), replyMarkup: chooseRate);
                    }

                }
                else if (MemberRoot.GetMember(userId).Name != "0" &&
                         MemberRoot.GetMember(userId).Country != "0" &&
                         MemberRoot.GetMember(userId).City != "0" &&
                         MemberRoot.GetMember(userId).Link != "0" &&
                         MemberRoot.GetMember(userId).Rate == "0")
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        MemberRoot.GetMember(userId).Link = "0";
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterLink", GetUser(userId).lang), replyMarkup: backButton);
                    }
                    else if (message.Text == "0" || message.Text.Contains("'"))
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    else
                    {
                        { var query = $"UPDATE `RV_Members` SET `rate` = '{message.Text}' WHERE `userId` = {userId}"; database.Read(query, ""); }
                        MemberRoot.GetMember(userId).Rate = message.Text;
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterTrack", GetUser(userId).lang), replyMarkup: backButton);

                    }
                }
                else if (MemberRoot.GetMember(userId).Name != "0" &&
                         MemberRoot.GetMember(userId).Country != "0" &&
                         MemberRoot.GetMember(userId).City != "0" &&
                         MemberRoot.GetMember(userId).Link != "0" &&
                         MemberRoot.GetMember(userId).Rate != "0" &&
                         MemberRoot.GetMember(userId).Track == "0")
                {
                    if (message.Text == Language.GetPhrase("Keyboard_Choice_Back", GetUser(userId).lang))
                    {
                        MemberRoot.GetMember(userId).Rate = "0";
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_EnterRate", GetUser(userId).lang), replyMarkup: chooseRate);
                    }
                    else if (message.Text == "0" || message.Text.Contains("'"))
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Messages_DontEnterZero", GetUser(userId).lang));
                    else
                    {
                        { var query = $"UPDATE `RV_Members` SET `track` = '{message.Text}' WHERE `userId` = {userId}"; database.Read(query, ""); }
                        ReplyKeyboardMarkup keyboard1 = new(new[] { new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", GetUser(userId).lang)) } })
                        { ResizeKeyboard = true };
                        ReplyKeyboardMarkup keyboard2 = new(new[] { new[] { new KeyboardButton("Получить купон на скидку на курс по гачимейкингу✅") } })
                        { ResizeKeyboard = true };
                        ReplyKeyboardMarkup keyboard = keyboard1;

                        if (GetUser(userId).lang == "ru")
                            keyboard = keyboard2;
                        MemberRoot.GetMember(userId).Track = message.Text;
                        botClient.SendTextMessageAsync(message.Chat, Language.GetPhrase("Member_Messages_FormSubmitted", GetUser(userId).lang), replyMarkup: keyboard);
                        if (GetUser(userId).lang == "ru")
                            botClient.SendTextMessageAsync(message.Chat, "❗️Не уверен в своих навыках гачимейкинга? Хочешь увеличить свой кабачок для выдачи более качественной икры?? Тогда советую тебе получить специальный курс по гачимейкингу от Tabolich Gachi!\nМелодия, тюн, баланс звука и многое другое! Специально для всех участников RightVision действует купон на скидку в 25%! \n\nПодробности в официальном ТГ-канале Tabolich Gachi - \nhttps://t.me/TabolichGachi/115\nЧтобы получить свой персональный купон на скидку, жми на кнопку ниже🔽", replyMarkup: keyboard);
                        botClient.SendTextMessageAsync(-1001968408177,
                            $"Пришла новая заявка на участие!\n\n" +
                            $"Id: {MemberRoot.GetMember(userId).UserId}\n" +
                            $"Имя: {MemberRoot.GetMember(userId).Name}\n" +
                            $"Тег: {MemberRoot.GetMember(userId).Telegram}\n" +
                            $"Страна проживания: {MemberRoot.GetMember(userId).Country}\n" +
                            $"Город: {MemberRoot.GetMember(userId).City}\n" +
                            $"Ссылка на канал: {MemberRoot.GetMember(userId).Link}\n" +
                            $"Субъективная оценка навыков: {MemberRoot.GetMember(userId).Rate}\n" +
                            $"Его трек внесён в базу и пока что держится в тайне!\n" +
                            $"\n" +
                            $"Тот, кто возьмёт кураторство над участником, обязан будет проверить канал и выдать категорию! В случае, если ссылки нету - провести лично с ним проверку мастерства и также выдать категорию!",
                            replyMarkup: memberAcceptOrDeny);
                    }
                }
            }
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
                    $"UPDATE `RV_Users` SET `status` = 'member' WHERE `id` = {id};",
                    "");
            else if (fromMembers.FirstOrDefault() == null && fromCritics.FirstOrDefault() != null)
                database.Read(
                    $"UPDATE `RV_Users` SET `status` = 'critic' WHERE `id` = {id};",
                    "");
            else if (fromMembers.FirstOrDefault() != null && fromCritics.FirstOrDefault() != null)
                database.Read(
                    $"UPDATE `RV_Users` SET `status` = 'criticAndMember' WHERE `id` = {id};",
                    "");
        }

        public static void updateLocation(long userId, string location)
        {
            GetUser(userId).location = location;
            database.Read($"UPDATE `RV_Users` SET `location` = '{location}' WHERE `id` = '{userId}';", "");
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(exception));
        }
    }
}