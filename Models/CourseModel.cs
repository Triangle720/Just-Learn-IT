using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace JustLearnIT.Models
{
    public enum Type
    {
        VIDEO,
        ARTICLE
    }

    public class CourseModel
    {
        public string Id { get; set; }

        [NotNull]
        [StringLength(50, MinimumLength = 1)]
        [Required]
        public string CourseName { get; set; }

        [NotNull]
        public string BlobStorageUri { get; set; }

        [NotNull]
        [StringLength(400)]
        [Required]
        public string CourseInfo { get; set; }

        [Required]
        public Type Type { get; set; }
    }
}
