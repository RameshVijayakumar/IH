using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


namespace Paycor.Import.Azure
{
    public class KeyVaultReader : IKeyVaultReader
    {
        private readonly ILog _log;
        private ThreadSafeCachingKeyResolver CachingKeyResolver { get; set; }
        private ClientCredential ClientCredential { get; set; }
        private KeyVaultClient KeyVaultClient { get; set; }
        private KeyVaultKeyResolver KeyVaultKeyResolver { get; set; }

        public KeyVaultReader(ILog log)
        {
            Ensure.ThatArgumentIsNotNull(log, nameof(log));
            _log = log;
            ConnectKeyVaultClient();
        }

        private void ConnectKeyVaultClient()
        {
            var keyVaultUserId = ConfigurationManager.AppSettings["Paycor.Storage.Blob.KeyVaultUserId"];
            var keyVaultUserPassword = ConfigurationManager.AppSettings["Paycor.Storage.Blob.KeyVaultUserPassword"];

            if (string.IsNullOrWhiteSpace(keyVaultUserPassword) || string.IsNullOrWhiteSpace(keyVaultUserId))
            {
                _log.Error("Unable to Connect to KeyVault, UserId Or Password is Empty.");
                return;
            }

            ClientCredential = new ClientCredential(keyVaultUserId, keyVaultUserPassword);
            KeyVaultClient = new KeyVaultClient(GetAccessToken);
            KeyVaultKeyResolver = new KeyVaultKeyResolver(GetAccessToken);

            if (CachingKeyResolver != null)
                return;
            CachingKeyResolver = SetUpCachingKeyResolver(KeyVaultKeyResolver);
        }
        public async Task<string> RetrieveAsync(string keyName)
        {
            try
            {
                var vaultUrl = ConfigurationManager.AppSettings["Paycor.Storage.Blob.KeyVaultUrl"];

                if (string.IsNullOrWhiteSpace(vaultUrl))
                {
                    _log.Error("KeyVaultURL is not Specified");
                    return string.Empty;
                }
                var secret = await KeyVaultClient.GetSecretAsync(vaultUrl, keyName);
                return secret.Value;
            }
            catch (Exception ex)
            {
                _log.Error($"An exception occurred while retrieving secretkey:{keyName}.", ex);
            }

            _log.Error($"SecretKey: {keyName} is empty");
            return string.Empty;
        }

        private async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, ClientCredential);
            return result.AccessToken;
        }

        private static ThreadSafeCachingKeyResolver SetUpCachingKeyResolver(IKeyResolver nonCachedResolver)
        {
            return new ThreadSafeCachingKeyResolver(10, nonCachedResolver);
        }
    }
}