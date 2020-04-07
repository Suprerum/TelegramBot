using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InlineQueryResults;
using ApiAiSDK;
using ApiAiSDK.Model;


namespace TelegramBot
{
    class Program
    {
        static TelegramBotClient Bot;
        static ApiAi apiAi;
        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("1142605519:AAHGUPytp88bqo1u0un7ds0hZuM6jiqBRHU");

            AIConfiguration aIConfiguration = new AIConfiguration("c44eb63c93fb4b46a29f2262e3d98738", SupportedLanguage.English);
            apiAi = new ApiAi(aIConfiguration);

            Bot.OnMessage += BotOnMessageRecived;
            Bot.OnCallbackQuery += BotOnCallbackQueryRecived;
            Bot.StartReceiving();
            var me = Bot.GetMeAsync().Result;
            Console.WriteLine(me.FirstName);
            
            Console.ReadLine();
            Bot.StartReceiving();
        }

        private static async void BotOnCallbackQueryRecived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            await Bot.AnswerCallbackQueryAsync(
                callbackQueryId: e.CallbackQuery.Id, 
                text: $"Button {e.CallbackQuery.Data} has been pressed");

            await Bot.SendTextMessageAsync(
                chatId: e.CallbackQuery.Message.Chat.Id,
                text: $"Button {e.CallbackQuery.Data} has been pressed");
        }

        private static async void BotOnMessageRecived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != MessageType.Text)
            {
                return; 
            }
            else
            {
                Console.WriteLine(message.Text);
            }

            switch (message.Text)
            {
                case "/start":
                    string text =
@"Список команд:
/start - запуск бота
/inline - вывод меню
/keyboard - вывод клавиатуры";
                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;
                case "/inline":
                    await SendInlineKeyboard(message);
                    break;
                case "/keyboard":
                    await SendReplyKeyboard(message);
                    break;
                default:
                    await SendApiAiResponse(message);
                    break;
            }
        }
        static async Task SendInlineKeyboard (Message message)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            await Task.Delay(500);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("1.1","1.2"),
                },
                new []
                {
                    InlineKeyboardButton.WithUrl("Залететь на Хабр", "https://habr.com/ru/"),
                }
            });
            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "choose your destiny",
                replyMarkup: inlineKeyboard
                );
        }

        static async Task SendReplyKeyboard(Message message)
        {
            var relpyKeyboardMarkup = new ReplyKeyboardMarkup(
                new KeyboardButton[][]
                {
                    new KeyboardButton[] {"1.1", "1.2"},
                    new KeyboardButton[] {"2.1", "2.2"},
                },
                resizeKeyboard: true
                );
            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: relpyKeyboardMarkup
                );
        }

        static async Task SendApiAiResponse(Message message)
        {
            var response = apiAi.TextRequest(message.Text);
            string answer = response.Result.Fulfillment.Speech;
            if (answer == "")
            {
                answer = "Cant understand";
            }
            await Bot.SendTextMessageAsync(message.From.Id, answer);
        }
    }
}
