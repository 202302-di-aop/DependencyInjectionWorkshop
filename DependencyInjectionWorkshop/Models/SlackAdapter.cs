using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void Notify(string message);
    }

    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        public void Notify(string message)
        {
            //notify user
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}