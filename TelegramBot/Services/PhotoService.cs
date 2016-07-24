using System;
using System.IO;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Services
{
    public class PhotoService
    {
    
        private readonly TelegramBotClient _botClient;

        public PhotoService(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async void SendRandomPhoto(long chatId)
        {
            await _botClient.SendChatActionAsync(chatId, ChatAction.UploadPhoto);

            var url = Config.WebApiBaseUrl + "photo";

            using (var client = new WebClient())
            {
                var data = await client.DownloadDataTaskAsync(new Uri(url));

                var fts = new FileToSend("randomFile.jpg", new MemoryStream(data));
                await _botClient.SendPhotoAsync(chatId, fts, "Nice Picture");
            }
        }
    }
}
