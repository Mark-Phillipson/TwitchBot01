using System;
using System.IO;
using System.Media;
using Microsoft.Extensions.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            Console.ReadLine();
        }
    }
    class Bot
    {
        TwitchClient client;

        public Bot()
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string username = configuration["TwitchBot:Username"] ?? throw new InvalidOperationException("Username not found in configuration");
            string accessToken = configuration["TwitchBot:AccessToken"] ?? throw new InvalidOperationException("AccessToken not found in configuration");
            string channel = configuration["TwitchBot:Channel"] ?? throw new InvalidOperationException("Channel not found in configuration");

            Console.WriteLine($"Connecting as: {username} to channel: {channel}");

            ConnectionCredentials credentials = new ConnectionCredentials(username, accessToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, channel);

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnConnected += Client_OnConnected;

            client.Connect();
        }
        private void Client_OnLog(object? sender, OnLogArgs e)
        {
            Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object? sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            client.SendMessage(e.Channel, "Hey guys! I am a bot connected via TwitchLib!");
        }
        private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            // Play a sound notification for new chat messages
            try
            {
                // Use Console.Beep for a simple sound notification
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(800, 200); // 800 Hz frequency for 200 milliseconds
                    Console.Beep(800, 200); // 800 Hz frequency for 200 milliseconds
                    Console.Beep(800, 200); // 800 Hz frequency for 200 milliseconds
                }
            }
            catch
            {
                // If beep doesn't work, just show a visual indicator
                Console.WriteLine("🔔 NEW MESSAGE!");
            }

            // Log the message to console so you can see it
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {e.ChatMessage.Username}: {e.ChatMessage.Message}");

            if (e.ChatMessage.Message.Contains("badword"))
                client.TimeoutUser(e.ChatMessage.Channel, e.ChatMessage.Username, TimeSpan.FromMinutes(30), "Bad word! 30 minute timeout!");
        }

        private void Client_OnWhisperReceived(object? sender, OnWhisperReceivedArgs e)
        {
            if (e.WhisperMessage.Username == "my_friend")
                client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private void Client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
            else
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
        }
    }
}