using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public interface IMyLogger
    {
        void Info(string message);
    }

    public class NLogAdapter : IMyLogger
    {
        public NLogAdapter()
        {
        }

        public void Info(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}