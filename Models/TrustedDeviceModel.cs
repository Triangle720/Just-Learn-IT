using System.Diagnostics.CodeAnalysis;

namespace JustLearnIT.Models
{
    public class TrustedDeviceModel
    {
        public string Id { get; set; }
        public string UserModelId { get; set; }

        [NotNull]
        public string Key { get; set; }
    }
}
