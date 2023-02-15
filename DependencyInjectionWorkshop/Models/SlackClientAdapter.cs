using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void NotifyUser(string message);
    }

    public class SlackClientAdapter : INotification
    {
        public SlackClientAdapter()
        {
        }

        public void NotifyUser(string message)
        {
            // slack notify user
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}