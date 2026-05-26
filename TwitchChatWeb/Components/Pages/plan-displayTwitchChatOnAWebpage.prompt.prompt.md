## Plan: Display Twitch Chat on a Webpage with Blazor Server (Local Only)

This plan outlines how to display Twitch chat messages from your bot on a webpage using Blazor Server, so you can easily monitor chat activity in real time. The solution will run locally, with no authentication required. Blazor Server allows you to build interactive web UIs with C#, and SignalR enables real-time updates.

### Steps
1. Add a Blazor Server project to your solution (local-only, no authentication).
2. Create a chat page/component in Blazor to display messages.
3. Integrate SignalR in the Blazor app for real-time chat updates.
4. Update your bot logic to push new chat messages to the SignalR hub.
5. Run the Blazor app in your browser to see live chat updates.
6. Test the integration to ensure chat messages appear instantly on the webpage.

### Further Considerations
1. The Blazor Server app will be accessible only on your local machine.
2. No authentication will be required.
 DOb