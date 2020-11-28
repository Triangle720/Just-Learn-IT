using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;

namespace JustLearnIT.Services
{
    public static class KeyVaultService
    {
        private static string Uri { get; set; }
        public static void Create(string secret)
        {
            Uri = secret;
        }

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

            var client = new SecretClient(new Uri(Uri),
                                                  new DefaultAzureCredential(),
                                                  options);

            KeyVaultSecret dbSecret = client.GetSecret(secretName);

            return dbSecret.Value;
        }
    }
}
