using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.UI;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back
{
    internal class Keyboard
    {
        public static InlineKeyboardMarkup Hub(RvUser rvUser) => new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Keyboard_Choice_About", rvUser.Lang) + "❓", "menu_about"),
                InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Keyboard_Choice_Apply", rvUser.Lang) + "📨", "menu_forms")
            },
            new[] { InlineKeyboardButton.WithCallbackData(HubClass.SendingStatus(rvUser) + "📬", "menu_sending") },
            new[] { InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Keyboard_Choice_MyProfile", rvUser.Lang) + "👤", "menu_profile") }
        });
        //=======

        public static InlineKeyboardMarkup ProfileOptions(RvUser rvUser)
        {
            InlineKeyboardButton[] top = new[]
            {
                InlineKeyboardButton.WithCallbackData("🗒"+"Список прав", "menu_permissions"),
                InlineKeyboardButton.WithCallbackData("👨‍⚖️"+"История наказаний", "menu_history")
            };

            InlineKeyboardButton[] back = new[] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), "menu_back") };

            InlineKeyboardButton[] criticMenu = new[]
                { InlineKeyboardButton.WithCallbackData("📱" + Language.GetPhrase("Keyboard_Choice_Critic_Menu_Open", rvUser.Lang), "c_openmenu"), };

            InlineKeyboardButton[] bottom = new[]
                { InlineKeyboardButton.WithCallbackData("📨" + Language.GetPhrase("Keyboard_Choice_Apply", rvUser.Lang), "menu_forms"), };

            InlineKeyboardButton[] memberButtons = new[]
            {
                InlineKeyboardButton.WithCallbackData("✏️" + Language.GetPhrase("Keyboard_Choice_EditTrack", rvUser.Lang), "m_edittrack"),
                InlineKeyboardButton.WithCallbackData("📇" + Language.GetPhrase("Keyboard_Choice_SendTrack", rvUser.Lang), "m_openmenu"),
            };
            
            InlineKeyboardButton[] memberOptions = memberButtons;
            InlineKeyboardButton[] criticOptions = criticMenu;
            InlineKeyboardButton[] customOptions = criticOptions;
            InlineKeyboardMarkup criticAndMember = new(new[] { top, back, criticOptions, memberOptions, bottom });

            if (rvUser.Has(Permission.TrackCard) && !rvUser.Has(Permission.CriticMenu))
                customOptions = memberOptions;

            InlineKeyboardMarkup custom = new(new[]
            {
                top, back, customOptions, bottom
            });

            InlineKeyboardMarkup common = new(new[] { top, back, bottom });

            if (!rvUser.Has(Permission.TrackCard) && !rvUser.Has(Permission.CriticMenu))
                custom = common;
            else if (rvUser.Has(Permission.TrackCard) && rvUser.Has(Permission.CriticMenu))
                custom = criticAndMember;

            return custom;
        }

        //=======
        public static InlineKeyboardMarkup Forms(RvUser rvUser, RvLocation location) => new InlineKeyboardMarkup(new[]
        { 
            new [] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), location switch
            {
                RvLocation.Profile => "menu_profile",
                _ => "menu_main"
            }) },
            new []
            {
                InlineKeyboardButton.WithCallbackData("🙋‍♂️" + Language.GetPhrase("Keyboard_Choice_Member", rvUser.Lang), "m_send"),
                InlineKeyboardButton.WithCallbackData("👨‍⚖️"  + Language.GetPhrase("Keyboard_Choice_Critic", rvUser.Lang), "c_send"),
            }
        });

        //=======
        public static InlineKeyboardMarkup InlineBack(RvUser rvUser, RvLocation? type = null) => new InlineKeyboardMarkup(new[]
        { new [] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), 
            type switch
                {
                    RvLocation.Profile => "menu_profile", 
                    _ => "menu_main"
                }
            )
            }
        });

        //=======
        public static InlineKeyboardMarkup CancelForm(RvUser rvUser, Status? type = null) => new InlineKeyboardMarkup(new[]
        { new [] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang),
                    type switch
                    {
                        Status.Critic => "menu_cancelCritic",
                        _ => "menu_cancelMember"
                    }
                )
            }
        });

        //=======
        public static InlineKeyboardMarkup Minimize(RvUser rvUser) => new InlineKeyboardMarkup(new[]
        { new [] 
            { 
                InlineKeyboardButton.WithCallbackData("↑ " + "Свернуть", "permissions_minimize"),
                InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), "menu_profile")
            }
        });

        public static InlineKeyboardMarkup Maximize(RvUser rvUser) => new InlineKeyboardMarkup(new[]
        { new []
            {
                InlineKeyboardButton.WithCallbackData("↓ " + "Развернуть", "permissions_maximize"),
                InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), "menu_profile")
            }
        });
        
        //=======
        public static ReplyKeyboardMarkup MainMenu(string lang) => new(new[] 
            { new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", lang)) } })
        { ResizeKeyboard = true };
        
        //=======
        public static ReplyKeyboardMarkup backButton(string lang) => new ReplyKeyboardMarkup(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", lang)) }) { ResizeKeyboard = true };
        
        //=======
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
        
        //=======
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
        
        //=======
        public static InlineKeyboardMarkup PreListeningCategories = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🥉Bronze", "c_changeTo_bronze"),
                    InlineKeyboardButton.WithCallbackData("🥈Steel", "c_changeTo_steel")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🥇Gold", "c_changeTo_gold"),
                    InlineKeyboardButton.WithCallbackData("💎Brilliant", "c_changeTo_brilliant")
                },
                new[] { InlineKeyboardButton.WithCallbackData("Отмена", "c_preListeningBack"),  }
            });
        
        //=======
        public static InlineKeyboardMarkup criticMenu = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Оценивание ремиксов", "c_evaluation"),
                InlineKeyboardButton.WithCallbackData("Пред. прослушивание", "c_prelistening")
            },
            new[] { InlineKeyboardButton.WithCallbackData("« Назад", "menu_profile") }
        });

        //=======
        public static InlineKeyboardMarkup Evaluation(long userId) => new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("<", "r_lower"),
                InlineKeyboardButton.WithCallbackData("0", "count"),
                InlineKeyboardButton.WithCallbackData(">", "r_higher")
            },
            TrackEvaluation.secondaryActions(userId),
            new[] { InlineKeyboardButton.WithCallbackData("Завершить прослушивание", "r_exit") }
        });

        //=======
        public static InlineKeyboardMarkup YesNo = new(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("✅Да", "c_blockremix_yes") },
            new[] { InlineKeyboardButton.WithCallbackData("❌Нет", "c_blockremix_no"), }
        });

        //=======
        public static InlineKeyboardMarkup actions = new(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Завершить прослушивание", "c_openmenu") },
                new[] { InlineKeyboardButton.WithCallbackData("Сменить категорию", "c_editcategory") },
                new[] { InlineKeyboardButton.WithCallbackData("Заблокировать ремикс", "c_blockremix") },
                new[] { InlineKeyboardButton.WithCallbackData("Одобрить ремикс", "c_acceptremix") }
            });
        
        //=======
        public static InlineKeyboardMarkup cCategories(long userId) => new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("❌Отклонить", $"c_deny2-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥉Bronze", $"c_bronze-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥈Steel", $"c_steel-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥇Gold", $"c_gold-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("💎Brilliant", $"c_brilliant-{userId}") }
            }
        );

        public static InlineKeyboardMarkup NextTrack = new(
            new[] { new[] { InlineKeyboardButton.WithCallbackData("Следующий ремикс", "r_nexttrack") } }
        );
        
        //=======
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
        //=======
        public static InlineKeyboardMarkup mCategories(long userId) => new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("❌Отклонить", $"m_deny2-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥉Bronze", $"m_bronze-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥈Steel", $"m_steel-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥇Gold", $"m_gold-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("💎Brilliant", $"m_brilliant-{userId}") }
            }
        );
        //=======
        public static InlineKeyboardMarkup memberAcceptOrDeny(long userId) => new InlineKeyboardMarkup(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Взять кураторство над участником", $"m_accept-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("Отклонить кандидатуру", $"m_deny-{userId}") }
            }
        );
        //=======
        public static InlineKeyboardMarkup criticAcceptOrDeny(long userId) => new InlineKeyboardMarkup(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Взять кураторство над судьёй", $"c_accept-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("Отклонить кандидатуру", $"c_deny-{userId}") }
            }
        );
        //=======
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

        /* Починить и использовать код при следующем RightVision
         * 
        public static InlineKeyboardMarkup Inline(RvUser rvUser)
        {
            Language language = new();
            InlineKeyboardMarkup cAndM = new InlineKeyboardMarkup(
                new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Profile_Form_Send_Member", rvUser.Lang), "m_send") },
                    new[] { InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Profile_Form_Send_Critic", rvUser.Lang), "c_send") }
                }
            );
            InlineKeyboardMarkup onlyC = new InlineKeyboardMarkup(
                new[] { new[] { InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Profile_Form_Send_Critic", rvUser.Lang), "c_send") } }
            );

            InlineKeyboardMarkup onlyM = new InlineKeyboardMarkup(
                new[] { new[] { InlineKeyboardButton.WithCallbackData(Language.GetPhrase("Profile_Form_Send_Member", rvUser.Lang), "m_send") } }
            );
            if (GetCandidateStatus(rvUser.UserId, "Member") == Language.GetPhrase("Profile_Form_Status_Allowed", rvUser.Lang) &&
                GetCandidateStatus(rvUser.UserId, "Critic") == Language.GetPhrase("Profile_Form_Status_Allowed", rvUser.Lang))
                return cAndM;
            else if (GetCandidateStatus(rvUser.UserId, "Critic") == Language.GetPhrase("Profile_Form_Status_Allowed", rvUser.Lang))
                return onlyM;
            else if (GetCandidateStatus(rvUser.UserId, "Member") == Language.GetPhrase("Profile_Form_Status_Allowed", rvUser.Lang))
                return onlyC;
            else
                return InlineKeyboardMarkup.Empty();
        }
        */

        public static ReplyKeyboardRemove remove = new ReplyKeyboardRemove();
    }
}
