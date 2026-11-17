using TestConsole;
using Xunit;

namespace TwitchBot01.Tests;

public class ChatMessageNotificationPolicyTests
{
    [Theory]
    [InlineData("viewer", "MarcusVoiceCoder", true)]
    [InlineData("marcusvoicecoder", "MarcusVoiceCoder", false)]
    [InlineData("MARCUSVOICECODER", "marcusvoicecoder", false)]
    public void ShouldNotify_OnlyWhenMessageIsFromAnotherUser(string messageUsername, string botUsername, bool expected)
    {
        var shouldNotify = ChatMessageNotificationPolicy.ShouldNotify(messageUsername, botUsername);

        Assert.Equal(expected, shouldNotify);
    }
}
