using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
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