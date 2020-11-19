using System.Diagnostics.CodeAnalysis;

namespace JustLearnIT.Models
{
    public class SaltModel
    {
        public long Id { get; set; }

        [NotNull]
        public string UserModelId { get; set; }

        [NotNull]
        public byte[] Value { get; set; }
    }
}
