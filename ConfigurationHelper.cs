using System.Configuration;

namespace EventsProducer
{
    public static class ConfigurationHelper
    {
        public static string EventHubConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings["EventHubConnectionString"];
            }
        }

        public static string EventHubName
        {
            get
            {
                return ConfigurationManager.AppSettings["EventHubName"];
            }
        }
    }
}
