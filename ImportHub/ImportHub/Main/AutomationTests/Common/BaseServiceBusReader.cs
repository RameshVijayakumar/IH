using System.Runtime.ConstrainedExecution;

namespace Paycor.Import.ImportHubTest.Common
{
    public abstract class BaseServiceBusReader<T> : IServiceBusReader<T>
    {
        protected string ConnectionString { get; private set; }
        protected string Path { get; private set; }


        protected BaseServiceBusReader(string connectionString, string path)
        {
            ConnectionString = connectionString;
            Path = path;
        }

        public abstract T ReceiveMessage();
    }
}
