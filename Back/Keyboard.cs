using RightVisionBot.Common;
using RightVisionBot.Tracks;
using RightVisionBot.UI;
using RightVisionBot.User;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBot.Back
{
    internal class Keyboard
    {
        public static InlineKeyboardMarkup Registration => new(new[]
        {
            InlineKeyboardButton.WithUrl("Зарегистрироваться", "https://t.me/RightVisionBot?start=start")
        });

        public static InlineKeyboardMarkup About(RvUser rvUser) => new(new[]
        {
            InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), "menu_main"),
            InlineKeyboardButton.WithCallbackData("Информация про бота", "menu_aboutBot")
        });

        public static InlineKeyboardMarkup Hub(RvUser rvUser) => new(new[]
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

        public static InlineKeyboardMarkup ProfileOptions(RvUser rvUser, Message message, string lang)
        {
            InlineKeyboardButton[] top = new[]
            {
                InlineKeyboardButton.WithCallbackData("🗒"+Language.GetPhrase("Keyboard_Choice_PermissionsList", lang), $"menu_permissions-{rvUser.UserId}"),
                InlineKeyboardButton.WithCallbackData("👨‍⚖️"+Language.GetPhrase("Keyboard_Choice_PunishmentsHistory", lang), $"menu_history-{rvUser.UserId}")
            };

            InlineKeyboardButton[] back = new[] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", lang), "menu_back") };

            InlineKeyboardButton[] criticMenu = new[]
                { InlineKeyboardButton.WithCallbackData("📱" + Language.GetPhrase("Keyboard_Choice_Critic_Menu_Open", lang), "c_openmenu"), };

            InlineKeyboardButton[] bottom = new[]
                { InlineKeyboardButton.WithCallbackData("📨" + Language.GetPhrase("Keyboard_Choice_Apply", lang), "menu_forms"), };

            InlineKeyboardButton[] memberButtons = new[]
            {
                InlineKeyboardButton.WithCallbackData("✏️" + Language.GetPhrase("Keyboard_Choice_EditTrack", lang), "m_edittrack"),
                InlineKeyboardButton.WithCallbackData("📇" + Language.GetPhrase("Keyboard_Choice_SendTrack", lang), "m_openmenu"),
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
            return message.Chat.Type != ChatType.Private ? top : custom;
        }

        //=======
        public static InlineKeyboardMarkup Forms(RvUser rvUser, RvLocation location) => new(new[]
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
        public static InlineKeyboardMarkup InlineBack(RvUser rvUser, RvLocation? type = null) => new (new[]
        { new [] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), 
            type switch
                {
                    RvLocation.Profile => "menu_profile", 
                    _ => "menu_main"
                }
            ) }
        });

        //=======
        public static InlineKeyboardMarkup BackToAbout(RvUser rvUser) => new(new[]
        { new [] { InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), "menu_about") } });

        //=======
        public static InlineKeyboardMarkup CancelForm(RvUser rvUser, Status? type = null) => new(new[]
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
        public static InlineKeyboardMarkup Minimize(RvUser rvUser) => new(new[]
        { new [] 
            { 
                InlineKeyboardButton.WithCallbackData("↑ " + "Свернуть", $"permissions_minimize-{rvUser.UserId}"),
                InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), $"permissions_back-{rvUser.UserId}")
            }
        });

        public static InlineKeyboardMarkup Maximize(RvUser rvUser) => new(new[]
        { new []
            {
                InlineKeyboardButton.WithCallbackData("↓ " + "Развернуть", $"permissions_maximize-{rvUser.UserId}"),
                InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang), $"permissions_back-{rvUser.UserId}")
            }
        });

        public static InlineKeyboardButton[] PermissionsBack(RvUser rvUser) => new[]
        {
            InlineKeyboardButton.WithCallbackData("« " + Language.GetPhrase("Keyboard_Choice_Back", rvUser.Lang),
                $"permissions_back-{rvUser.UserId}")
        };
        //=======
        public static ReplyKeyboardMarkup MainMenu(string lang) => new(new[] 
            { new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_MainMenu", lang)) } })
        { ResizeKeyboard = true };
        
        //=======
        public static ReplyKeyboardMarkup BackButton(string lang) => new(new[] { new KeyboardButton(Language.GetPhrase("Keyboard_Choice_Back", lang)) }) { ResizeKeyboard = true };
        
        //=======
        public static ReplyKeyboardMarkup ChooseRate(string lang) => new(new[]
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
        public static ReplyKeyboardMarkup СhooseLang = new(new[]
        {
                new[]
                {
                    new KeyboardButton("🇷🇺RU / CIS")
                },
                new[]
                {
                    new KeyboardButton("🇺🇦UA"),
                    new KeyboardButton("🇰🇿KZ")
                }
            })
        { ResizeKeyboard = true };
        
        //=======
        public static InlineKeyboardMarkup PreListeningCategories = new(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🥉Bronze", "c_changeTo_bronze"),
                    InlineKeyboardButton.WithCallbackData("🥈Silver", "c_changeTo_silver")
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
        public static InlineKeyboardMarkup Evaluation(long userId) => new(new[]
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
        public static InlineKeyboardMarkup CCategories(long userId) => new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("❌Отклонить", $"c_deny2-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥉Bronze", $"c_bronze-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥈Silver", $"c_silver-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥇Gold", $"c_gold-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("💎Brilliant", $"c_brilliant-{userId}") }
            }
        );

        public static InlineKeyboardMarkup NextTrack = new(
            new[] { new[] { InlineKeyboardButton.WithCallbackData("Следующий ремикс", "r_nexttrack") } }
        );
        
        //=======
        public static InlineKeyboardMarkup finalActions = new(
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
        public static InlineKeyboardMarkup MCategories(long userId) => new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("❌Отклонить", $"m_deny2-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥉Bronze", $"m_bronze-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥈Silver", $"m_silver-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("🥇Gold", $"m_gold-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("💎Brilliant", $"m_brilliant-{userId}") }
            }
        );
        //=======
        public static InlineKeyboardMarkup MemberAcceptOrDeny(long userId) => new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Взять кураторство над участником", $"m_accept-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("Отклонить кандидатуру", $"m_deny-{userId}") }
            }
        );
        //=======
        public static InlineKeyboardMarkup CriticAcceptOrDeny(long userId) => new(
            new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Взять кураторство над судьёй", $"c_accept-{userId}") },
                new[] { InlineKeyboardButton.WithCallbackData("Отклонить кандидатуру", $"c_deny-{userId}") }
            }
        );
        //=======
        public static InlineKeyboardMarkup KickHares = new(
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

        public static ReplyKeyboardRemove remove = new();
    }
}
