using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public interface IMyLogger
    {
        void LogInfo(string message);
    }

    public class NLogAdapter : IMyLogger
    {
        public NLogAdapter()
        {
        }

        public void LogInfo(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}