using System;

namespace JustLearnIT.Models
{
    public class OneTimePassword
    {
        public int Id { get; set; }

        public string UserModelId { get; set; }

        public string Value { get; set; }

        //public DateTime ExpirationTime { get; set; } -- todo sheluder implementation needed
    }
}
