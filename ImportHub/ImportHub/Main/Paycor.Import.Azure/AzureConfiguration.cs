using System.Configuration;

namespace Paycor.Import.Azure
{
    public static class AzureConfiguration
    {
        private static string _storageConnectionString;
        private static string _serviceBusConnectionString;
        private static string _azureWebJobsDashboardConnection;

        public static string StorageConnectionString
        {
            get
            {
                if (null == _storageConnectionString)
                    _storageConnectionString = ConfigurationManager.AppSettings["TableStorageConnection"];

                return (_storageConnectionString);
            }

            set { _storageConnectionString = value; }
        }

        public static string ServiceBusConnectionString
        {
            get
            {
                if (null == _serviceBusConnectionString)
                    _serviceBusConnectionString = ConfigurationManager.AppSettings["ServiceBusConnection"];

                return (_serviceBusConnectionString);
            }

            set { _serviceBusConnectionString = value; }
        }

        public static string AzureWebJobsDashboardConnection
        {
            get
            {
                if (null == _azureWebJobsDashboardConnection)
                {
                    _azureWebJobsDashboardConnection =
                        ConfigurationManager.AppSettings["AzureWebJobsDashboardConnection"];

                    if (string.IsNullOrEmpty(_azureWebJobsDashboardConnection))
                        _azureWebJobsDashboardConnection = _storageConnectionString;
                }

                return (_azureWebJobsDashboardConnection);
            }

            set { _azureWebJobsDashboardConnection = value; }
        }
    }
}
