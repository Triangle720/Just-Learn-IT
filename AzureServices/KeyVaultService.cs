using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;

namespace JustLearnIT.AzureServices
{
    public static class KeyVaultService
    {
        public static string GetSecretByName(string secretName)
        {
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                 }
            };

            var client = new SecretClient(new Uri(Environment.GetEnvironmentVariable("VaultUri")),
                                                  new DefaultAzureCredential(),
                                                  options);

            KeyVaultSecret dbSecret = client.GetSecret(secretName);

            return dbSecret.Value;
        }
    }
}
