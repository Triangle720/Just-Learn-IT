using System.Linq;
using System.Threading.Tasks;
using JustLearnIT.Data;
using JustLearnIT.Security;
using JustLearnIT.Services;
using Microsoft.AspNetCore.Mvc;

namespace JustLearnIT.Controllers
{
    [RoleAuthFilter("USER,ADMIN")]
    [SubscriptionFilter(true)]
    public class VideoController : Controller
    {
        private readonly DatabaseContext _context;
        public VideoController(DatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index(string courseName)
        {
            if (string.IsNullOrEmpty(courseName) || !_context.Courses.Where(c => c.CourseName == courseName).Any())
            {
                return RedirectToAction("Courses", "Home");
            }
            return View("Index", courseName);
        }

        [HttpGet("getvideo")]
        public async Task<FileContentResult> GetIntroductoryVideos(string videoName)
        {
            return await BlobStorageService.GetVideo(videoName, "video");
        }
    }
}
