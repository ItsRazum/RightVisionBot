using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back
{
    internal class Keyboard
    {
        public static ReplyKeyboardMarkup ForMember(string lang) => new(new[] 
        {
            new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", lang)) },
            new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_EditTrack", lang)) },
            new[] { new KeyboardButton("Получить визуал ремикса") }
        })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup MainMenu(string lang) => new(new[] 
            { new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", lang)) } })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup backButton(string lang) => new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", lang)) }) { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup chooseRate(string lang) => new(new[]
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
                    new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", lang))
                }
            })
            { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup ForCritic(string lang) => new(new[] 
        {
            new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", lang)) },
            new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Critic_Menu_Open", lang)) }
        })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup ForOther(string lang) => new(new[]
        {
            new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", lang)) }
        })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup ForCriticAndMember(string lang) => new(new[] 
        {
            new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", lang)) },
            new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Critic_Menu_Open", lang)) },
            new[] { new KeyboardButton("Получить визуал ремикса") },
            new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_SendTrack", lang)) }
        })
        { ResizeKeyboard = true };

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
            new[] { new[] { InlineKeyboardButton.WithCallbackData("Следующий ремикс", "r_nexttrack") } }
        );

        public static InlineKeyboardMarkup finalActions = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Изм. 1", "r_change1"),
                    InlineKeyboardButton.WithCallbackData("Изм. 2", "r_change2"),
                    InlineKeyboardButton.WithCallbackData("Изм. 3", "r_change3"),
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

        public static InlineKeyboardMarkup KickHares = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Да", "h_kick"),
                    InlineKeyboardButton.WithCallbackData("Нет", "h_notkick"), 
                },
            }
        );

        public static ReplyKeyboardRemove remove = new ReplyKeyboardRemove();
    }
}
