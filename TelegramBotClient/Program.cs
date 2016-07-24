

using System;

namespace TelegramBotClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new TelegramBot.Bot();
            bot.StartReceiving();
            Console.ReadLine();
            bot.StopReceiving();
        }
    }
}
