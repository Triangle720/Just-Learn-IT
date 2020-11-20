﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace JustLearnIT.Models
{
    public class VerificationCodeModel
    {
        public int Id { get; set; }

        [NotNull]
        public string UserModelId { get; set; }

        [NotNull]
        public string RadnomUriCode { get; set; }

        //public DateTime ExpirationTime { get; set; } -- todo sheluder implementation needed
    }
}
