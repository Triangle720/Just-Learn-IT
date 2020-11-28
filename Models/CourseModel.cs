using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace JustLearnIT.Models
{
    public class CourseModel
    {
        public string Id { get; set; }

        [NotNull]
        [Required]
        public string CourseName { get; set; }

        [NotNull]
        [Required]
        public string CourseInfo { get; set; }
    }
}
