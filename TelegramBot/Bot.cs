using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public class Bot
    {
        private const string ApiKey = "221390342:AAHEbJxB1PoUUxdnkPKb5SjUxIKj-Cffib8";

        private static readonly TelegramBotClient BotClient = new TelegramBotClient(ApiKey);

        public class WeatherIcons
        {
            private static string Thunderstorm = "\U0001F4A8"; // Code: 200's, 900, 901, 902, 905
            private static string Drizzle = "\U0001F4A7"; // Code: 300's
            private static string Rain = "\U00002614"; // Code: 500's
            private static string Snowflake = "\U00002744"; // Code: 600's snowflake
            private static string Snowman = "\U000026C4"; // Code: 600's snowman, 903, 906
            private static string Atmosphere = "\U0001F301"; // Code: 700's foogy
            private static string ClearSky = "\U00002600"; // Code: 800 clear sky
            private static string FewClouds = "\U000026C5"; // Code: 801 sun behind clouds
            private static string Clouds = "\U00002601"; // Code: 802-803-804 clouds general
            private static string Hot = "\U0001F525"; // Code: 904
            private static string DefaultEmoji = "\U0001F300"; // default emojis

            public static string GetEmoji(int weatherId)
            {

                if (weatherId.ToString().StartsWith("2") || weatherId == 900 || weatherId == 901 || weatherId == 902 || weatherId == 905)
                {
                    return Thunderstorm;
                }

                if (weatherId.ToString().StartsWith("3"))
                {
                    return Drizzle;
                }

                if (weatherId.ToString().StartsWith("5"))
                {
                    return Rain;
                }

                if (weatherId.ToString().StartsWith("6") || weatherId == 903 || weatherId == 906)
                {
                    return Snowflake + ' ' + Snowman;
                }

                if (weatherId.ToString().StartsWith("7"))
                {
                    return Atmosphere;
                }

                if (weatherId == 800)
                {
                    return ClearSky;
                }
                if (weatherId == 801)
                {
                    return FewClouds;
                }
                if (weatherId == 802 || weatherId == 803 || weatherId == 803)
                {
                    return Clouds;
                }

                if (weatherId == 904)
                {
                    return Hot;
                }


                return DefaultEmoji;
            }
        }





        public Bot()
        {
            BotClient.OnMessage += BotClientOnMessageReceived;
            BotClient.OnMessageEdited += BotClientOnMessageReceived;
            BotClient.OnInlineQuery += BotClientOnInlineQueryReceived;
            BotClient.OnInlineResultChosen += BotClientOnChosenInlineResultReceived;
            BotClient.OnReceiveError += BotClientOnReceiveError;


        }

        public void StartReceiving()
        {
            BotClient.StartReceiving();
        }

        public void StopReceiving()
        {
            BotClient.StopReceiving();
        }

        private static void BotClientOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static void BotClientOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {

        }

        private static async void BotClientOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            InlineQueryResult[] results = {
                new InlineQueryResultLocation
                {
                    Id = "1",
                    Latitude = 40.7058316f, // displayed result
                    Longitude = -74.2581888f,
                    Title = "New York",
                    InputMessageContent = new InputLocationMessageContent // message if result is selected
                    {
                        Latitude = 40.7058316f,
                        Longitude = -74.2581888f,
                    }
                },

                new InlineQueryResultLocation
                {
                    Id = "2",
                    Longitude = 52.507629f, // displayed result
                    Latitude = 13.1449577f,
                    Title = "Berlin",
                    InputMessageContent = new InputLocationMessageContent // message if result is selected
                    {
                        Longitude = 52.507629f,
                        Latitude = 13.1449577f
                    }
                }
            };

            await BotClient.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results, isPersonal: true, cacheTime: 0);
        }

        private static async void BotClientOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage) return;

            var text = message.Text.ToLower();

            if (text.StartsWith("/photo")) // send a photo
            {
                await BotClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                var url = "http://localhost//botwebapi/api/photo";

                using (var client = new WebClient())
                {
                    var data = await client.DownloadDataTaskAsync(new Uri(url));

                    var fts = new FileToSend("randomFile.jpg", new MemoryStream(data));
                    await BotClient.SendPhotoAsync(message.Chat.Id, fts, "Nice Picture");
                }


            }
            else if (text.StartsWith("/weather")) // request location or contact
            {

                var r = new Regex(@"(\/weather)\s([a-zA-Z]+)");

                var tokens = r.Match(text).Groups;
                var url = "http://localhost//botwebapi/api/weather/";

                if (tokens.Count > 1)
                {
                    url += tokens[2];
                }


                using (var client = new WebClient())
                {
                    var data = await client.DownloadStringTaskAsync(new Uri(url));


                    var obj = JsonConvert.DeserializeObject<dynamic>(data);

                    var strMessage = string.Format("температура: {0}\u00B0C\n{1} {2} ", (decimal)obj.Temp, obj.Description, WeatherIcons.GetEmoji((int)obj.WeatherId));
                    await BotClient.SendTextMessageAsync(message.Chat.Id, strMessage);
                }
            }
            else
            {
                var usage = @"Usage:
/photo    - send a photo
/weather  - show wheather
";

                await BotClient.SendTextMessageAsync(message.Chat.Id, usage,
                    replyMarkup: new ReplyKeyboardHide());
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await BotClient.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
