using System;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using Bot bot = new Bot();
            Console.ReadLine();
        }
    }
    class Bot : IDisposable
    {
        TwitchClient client;
        private HubConnection? hubConnection;

        // UI thread fields for tray icon
        private NotifyIcon? _notifyIcon;
        private Thread? _uiThread;
        private SynchronizationContext? _uiContext;
        private ManualResetEventSlim? _uiInitEvent;

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

            // Initialize tray UI on Windows
            if (OperatingSystem.IsWindows())
            {
                _uiInitEvent = new ManualResetEventSlim(false);
                _uiThread = new Thread(() =>
                {
                    SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                    _uiContext = SynchronizationContext.Current;
                    _notifyIcon = new NotifyIcon
                    {
                        Icon = SystemIcons.Application,
                        Visible = true,
                        Text = "TwitchBot01"
                    };
                        // Add a small context menu so the icon can be right-clicked (Exit)
                        var cms = new ContextMenuStrip();
                        var exitItem = new ToolStripMenuItem("Exit");
                        exitItem.Click += (s, ev) => Application.ExitThread();
                        cms.Items.Add(exitItem);
                        _notifyIcon.ContextMenuStrip = cms;
                        Console.WriteLine("Tray UI initialized");
                    _uiInitEvent.Set();
                    Application.Run();
                });
                _uiThread.SetApartmentState(ApartmentState.STA);
                _uiThread.IsBackground = true;
                _uiThread.Start();
                // Wait until the UI thread signals initialization (no short timeout)
                _uiInitEvent.Wait();
            }

            try
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5194/chathub")
                    .WithAutomaticReconnect()
                    .Build();

                hubConnection.StartAsync().Wait();
                Console.WriteLine("Connected to local chat relay at http://localhost:5194/chathub");
            }
            catch (Exception ex)
            {
                hubConnection = null;
                Console.WriteLine($"Local chat relay unavailable: {ex.GetBaseException().Message}");
                Console.WriteLine("Continuing without the web chat bridge. Start TwitchChatWeb if you want browser chat mirroring.");
            }

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
            // Forward to web relay when connected
            if (hubConnection?.State == HubConnectionState.Connected)
            {
                _ = hubConnection.SendAsync("SendMessage", $"{e.ChatMessage.Username}: {e.ChatMessage.Message}")
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine($"Failed to forward chat message to web relay: {task.Exception?.GetBaseException().Message}");
                        }
                    }, TaskScheduler.Default);
            }

            try
            {
                // Use Console.Beep for a simple sound notification
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(800, 200); // 800 Hz frequency for 200 milliseconds
                    Console.Beep(800, 200); // 800 Hz frequency for 200 milliseconds
                    Console.Beep(800, 200); // 800 Hz frequency for 200 milliseconds

                    // Show a tray balloon notification (marshal to UI thread)
                    string title = e.ChatMessage.Username;
                    string message = e.ChatMessage.Message;
                    if (_uiContext != null)
                    {
                        _uiContext.Post(_ =>
                        {
                            try
                            {
                                if (_notifyIcon != null)
                                {
                                    _notifyIcon.BalloonTipTitle = title;
                                    _notifyIcon.BalloonTipText = message;
                                    _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                                    _notifyIcon.ShowBalloonTip(3000);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to show balloon tip: {ex.Message}");
                            }
                        }, null);
                    }
                    else
                    {
                        // Fallback attempt (may be suppressed on some systems)
                        try
                        {
                            _notifyIcon?.BalloonTipTitle = title;
                            _notifyIcon?.BalloonTipText = message;
                            _notifyIcon?.BalloonTipIcon = ToolTipIcon.Info;
                            _notifyIcon?.ShowBalloonTip(3000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Fallback balloon failed: {ex.Message}");
                        }
                    }
                }
            }
            catch
            {
                // If beep/notification doesn't work, just show a visual indicator
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

        public void Dispose()
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    _uiContext?.Post(_ =>
                    {
                        try
                        {
                            if (_notifyIcon != null)
                            {
                                _notifyIcon.Visible = false;
                                _notifyIcon.Dispose();
                                _notifyIcon = null;
                            }
                        }
                        catch { }
                        Application.ExitThread();
                    }, null);

                    _uiThread?.Join(1000);
                }
                catch { }
                _uiInitEvent?.Dispose();
                _uiInitEvent = null;
            }

            try
            {
                client?.Disconnect();
            }
            catch { }

            try
            {
                hubConnection?.DisposeAsync().AsTask().Wait(500);
            }
            catch { }
        }
    }
}