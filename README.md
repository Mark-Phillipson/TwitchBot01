# TwitchBot01 - C# Twitch Chat Bot

A simple Twitch chat bot built with C# using the TwitchLib library. This bot can monitor chat, respond to messages, handle subscriptions, and perform moderation actions.

## Features

- **Chat Monitoring**: Reads and responds to chat messages
- **Sound Notifications**: Plays audio alerts when new chat messages arrive
- **Moderation**: Automatically times out users for using bad words
- **Subscription Alerts**: Welcomes new subscribers with custom messages
- **Whisper Support**: Responds to private whisper messages
- **Logging**: Comprehensive logging of bot activities

## Prerequisites

- .NET 9.0 SDK
- Visual Studio or VS Code
- A Twitch account
- A registered Twitch application (for API access)

## Getting Started

### 1. Clone/Download the Project

```powershell
# If using git
git clone <your-repo-url>
cd TwitchBot01

# Or download and extract the project files
```

### 2. Install Dependencies

The project uses the following NuGet packages (already included in the .csproj):
- `TwitchLib.Client` - Main Twitch client library
- `TwitchLib.Communication` - Communication layer for Twitch
- `Microsoft.Extensions.Configuration` - Configuration framework
- `Microsoft.Extensions.Configuration.Json` - JSON configuration provider

```powershell
dotnet restore
```

### 3. Create a Twitch Application

#### Step 1: Register Your Application
1. Go to the [Twitch Developer Console](https://dev.twitch.tv/console)
2. Log in with your Twitch account
3. Click **"Register Your Application"**
4. Fill out the form:
   - **Name**: `MarcusVoiceCoder Bot` (or your preferred name)
   - **OAuth Redirect URLs**: `https://twitchtokengenerator.com/`
   - **Category**: `Chat Bot`
5. Complete the reCAPTCHA
6. Click **"Create"**

#### Step 2: Get Your Credentials
1. After creation, note down your **Client ID**
2. Click **"New Secret"** to generate a **Client Secret**
3. Save both credentials securely

### 4. Generate an Access Token

#### Option A: Using Twitch Token Generator (Recommended)
1. Go to [twitchtokengenerator.com](https://twitchtokengenerator.com/)
2. Enter your **Client ID** and **Client Secret**
3. Select the following scopes:
   - `chat:read` - Read chat messages
   - `chat:edit` - Send chat messages
   - `whispers:read` - Read whisper messages
   - `whispers:edit` - Send whisper messages
4. Click **"Generate Token"**
5. Save the generated access token

#### Option B: Manual OAuth Flow
1. Replace `YOUR_CLIENT_ID` with your actual Client ID in this URL:
```
https://id.twitch.tv/oauth2/authorize?client_id=YOUR_CLIENT_ID&redirect_uri=https://twitchtokengenerator.com/&response_type=token&scope=chat:read+chat:edit+whispers:read+whispers:edit
```
2. Navigate to the URL in your browser
3. Authorize the application
4. Copy the access token from the result

### 5. Configure Your Bot Credentials

#### Create Configuration File
1. Copy the template configuration file:
```powershell
Copy-Item "TwitchBot01/appsettings.template.json" "TwitchBot01/appsettings.json"
```

2. Edit `TwitchBot01/appsettings.json` with your actual credentials:
```json
{
  "TwitchBot": {
    "Username": "your_twitch_username",
    "AccessToken": "your_oauth_token_from_step_4",
    "Channel": "your_channel_name"
  }
}
```

3. **Security**: The `appsettings.json` file is already included in `.gitignore` to prevent accidentally committing your credentials to version control.

### 6. Build and Run the Bot

```powershell
# Build the project
dotnet build

# Run the bot
dotnet run
```

## Configuration

### Bot Behavior Settings

The bot is configured with the following settings in `Program.cs`:

```csharp
var clientOptions = new ClientOptions
{
    MessagesAllowedInPeriod = 750,
    ThrottlingPeriod = TimeSpan.FromSeconds(30)
};
```

### Customizable Features

#### Moderation
- **Bad Word Detection**: Currently set to timeout users for 30 minutes when they use "badword"
- **Modify in**: `Client_OnMessageReceived` method

#### Welcome Messages
- **New Subscribers**: Different messages for Prime vs paid subscriptions
- **Channel Join**: Bot announces itself when joining a channel

#### Whisper Responses
- **Friend Whispers**: Currently responds only to username "my_friend"
- **Modify in**: `Client_OnWhisperReceived` method

## Project Structure

```
TwitchBot01/
├── Program.cs              # Main bot logic
├── TwitchBot01.csproj     # Project configuration
├── README.md              # This file
├── .env                   # Environment variables (create this)
└── .gitignore            # Git ignore file (create this)
```

## Security Best Practices

### ⚠️ Important Security Notes

1. **Never commit access tokens to version control**
2. **Use environment variables or secure configuration files**
3. **Access tokens can expire** - you may need to refresh them periodically
4. **Limit token scopes** to only what your bot needs

### Recommended .gitignore

Create a `.gitignore` file with:
```
# Environment files
.env
*.env

# Build outputs
bin/
obj/

# User-specific files
*.user
*.suo
.vs/
```

## Troubleshooting

### Common Issues

#### 1. "TWITCH_ACCESS_TOKEN environment variable is required"
- Make sure you've set the environment variables correctly
- Verify the token is valid and hasn't expired

#### 2. Bot connects but doesn't respond
- Check that your bot account has the necessary permissions
- Verify the channel name is correct
- Ensure the access token has the required scopes

#### 3. Build errors with nullability warnings
- All event handlers use `object? sender` parameters (already fixed in current code)

#### 4. Connection timeout or authentication errors
- Verify your access token is still valid
- Check your Client ID and Client Secret
- Ensure your redirect URLs match exactly

### Debugging

Enable detailed logging by monitoring the console output. The bot logs:
- Connection status
- Chat messages received
- Actions taken (timeouts, welcome messages, etc.)

## Bot Commands and Responses

### Current Bot Features

1. **Auto-moderation**: Times out users for saying "badword"
2. **Subscription alerts**: Welcomes new subscribers
3. **Friend whispers**: Responds to whispers from "my_friend"
4. **Join announcement**: Announces when bot joins the channel

### Extending the Bot

To add new features, you can:

1. **Add new event handlers** in the `Bot` constructor
2. **Implement custom commands** in `Client_OnMessageReceived`
3. **Add new response patterns** for different user interactions

Example of adding a simple command:
```csharp
private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
{
    if (e.ChatMessage.Message.StartsWith("!hello"))
    {
        client.SendMessage(e.ChatMessage.Channel, $"Hello {e.ChatMessage.DisplayName}!");
    }
    
    // Existing moderation code...
}
```

## Resources

- [TwitchLib Documentation](https://github.com/TwitchLib/TwitchLib)
- [Twitch API Documentation](https://dev.twitch.tv/docs/api/)
- [Twitch Developer Console](https://dev.twitch.tv/console)
- [Twitch Token Generator](https://twitchtokengenerator.com/)

## License

This project is for educational purposes. Make sure to comply with Twitch's Terms of Service and API guidelines when using this bot.

---

**Happy Botting! 🤖**
