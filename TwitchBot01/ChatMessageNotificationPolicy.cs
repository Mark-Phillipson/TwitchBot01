namespace TestConsole;

public static class ChatMessageNotificationPolicy
{
    public static bool ShouldNotify(string messageUsername, string botUsername) =>
        !string.Equals(messageUsername, botUsername, StringComparison.OrdinalIgnoreCase);
}
