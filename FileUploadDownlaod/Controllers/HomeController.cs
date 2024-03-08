using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FileUploadDownload.Models;
using System.IO;
using System.Threading.Tasks;
using FileUploadDownlaod.Models;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using FileUploadDownlaod.Repository;
using Microsoft.AspNetCore.Authorization;

namespace FileUploadDownload.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileDetailsRepository _fileRepository;

    
        public HomeController(ILogger<HomeController> logger, IFileDetailsRepository fileRepository)
        {
            _logger = logger;
            _fileRepository = fileRepository;
        }

        // «·œ«·… «·—∆Ì”Ì… ·⁄—÷ «·„·›« 
        public async Task<IActionResult> Index()
        {
            var files = await _fileRepository.GetAllFiles();
            return View(files);
        }





        [HttpPost]
        public async Task<IActionResult> Index(IFormFile[] files, string description, string createdBy)
        {
            // «· Õﬁﬁ „„« ≈–« ﬂ«‰  «·„·›«  „Õœœ… ·· Õ„Ì·
            if (files == null || files.Length == 0)
            {
                //  ⁄ÌÌ‰ —”«·… Œÿ√ ≈–« ·„ Ì „  ÕœÌœ „·›« 
                ViewBag.ErrorMessage = "«·—Ã«¡  ÕœÌœ «·„·›  ·· Õ„Ì·.";
                // «·⁄Êœ… ≈·Ï ’›Õ… «·›Â—”
                var model1 = await _fileRepository.GetAllFiles();
                return View(model1);
            }

            foreach (var file in files)
            {
                // «· Õﬁﬁ „„« ≈–« ﬂ«‰ «·„·› „ÊÃÊœ« »«·›⁄·
                var existingFile = await _fileRepository.GetFileByName(file.FileName);
                if (existingFile != null)
                {
                    ViewBag.ErrorMessage = $"«·„·› '{file.FileName}' „ÊÃÊœ »«·›⁄·.";
                    var model2 = await _fileRepository.GetAllFiles();
                    return View(model2);
                }

                var fileName = Path.GetFileName(file.FileName);
                var ext = Path.GetExtension(fileName).ToLowerInvariant();

                // ≈‰‘«¡ „Ã·œ ›—⁄Ì »‰«¡ ⁄·Ï «„ œ«œ «·„·›
                var subfolder = Path.Combine("wwwroot\\files", ext.Substring(1));

                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), subfolder);


                
                // «· Õﬁﬁ „„« ≈–« ﬂ«‰ ‰Ê⁄ «·„·› „œ⁄Ê„«
                if (!IsSupportedFileType(ext))
                {
                    //  ⁄ÌÌ‰ —”«·… Œÿ√ ·‰Ê⁄ «·„·› €Ì— «·„œ⁄Ê„
                    ViewBag.ErrorMessage = $"‰Ê⁄ «·„·› €Ì— „œ⁄Ê„: {ext}. «·—Ã«¡  Õ„Ì· „·›«  PDF √Ê JPEG √Ê JPG √Ê PNG √Ê GIF ›ﬁÿ.";

                    var model2 = await _fileRepository.GetAllFiles();
                    return View(model2);
                }


                // «· Õﬁﬁ „„« ≈–« ﬂ«‰ «·„Ã·œ «·›—⁄Ì „ÊÃÊœ«
                if (!Directory.Exists(directoryPath))
                {
                    // ≈‰‘«¡ «·„Ã·œ «·›—⁄Ì ≈–« ·„ Ìﬂ‰ „ÊÃÊœ«
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, fileName);

                // Õ›Ÿ «·„·› ≈·Ï «·ﬁ—’
                using (var localFile = System.IO.File.OpenWrite(filePath))
                using (var uploadedFile = file.OpenReadStream())
                {
                    uploadedFile.CopyTo(localFile);
                }

                // ≈÷«›…  ›«’Ì· «·„·› ≈·Ï „Œ“‰ «·»Ì«‰« 
                await _fileRepository.AddFile(new FileDetails
                {
                    Name = fileName,
                    Path = filePath,
                    FileType = GetContentType(filePath),
                    Description = description,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy
                });
            }

            //  ⁄ÌÌ‰ —”«·… ‰Ã«Õ
            ViewBag.Message = " „  Õ„Ì· «·„·›«  »‰Ã«Õ";
            // ⁄—÷ ﬂ«›… «·„·›«  
            var model = await _fileRepository.GetAllFiles();
            return View(model);
        }






        // œ«·… ·Õ–› „·›
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // «·Õ’Ê· ⁄·Ï  ›«’Ì· «·„·› »Ê«”ÿ… id
            var file = await _fileRepository.GetFileById(id);
            // «· Õﬁﬁ „„« ≈–« ﬂ«‰ «·„·› „ÊÃÊœ«
            if (file == null)
            {
                // ≈—Ã«⁄ Œÿ√ 404 ≈–« ·„ Ì „ «·⁄ÀÊ— ⁄·Ï «·„·›
                return NotFound();
            }

            // Õ–› «·„·› „‰ «· Œ“Ì‰
            System.IO.File.Delete(file.Path);

            // Õ–› «·„·› „‰ ﬁ«⁄œ… «·»Ì«‰« 
            await _fileRepository.DeleteFile(id);

            
            return RedirectToAction(nameof(Index));
        }

        // œ«·… · ‰“Ì· „·›
        public async Task<IActionResult> Download(string filename)
        {
            // «· Õﬁﬁ „„« ≈–« ﬂ«‰ «”„ «·„·› „Õœœ«
            if (filename == null)
                return Content("«”„ «·„·› €Ì— „ Ê›—");

            // «·Õ’Ê· ⁄·Ï  ›«’Ì· «·„·› »Ê«”ÿ… «·«”„
            var file = await _fileRepository.GetFileByName(filename);
            // «· Õﬁﬁ „„« ≈–« ﬂ«‰ «·„·› „ÊÃÊœ«
            if (file == null)
                return NotFound();

            // ﬁ—«¡… «·„·› „‰ «·ﬁ—’ ≈·Ï „Ì„Ê—Ì ” —Ì„
            var memory = new MemoryStream();
            using (var stream = new FileStream(file.Path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            // ≈—Ã«⁄ «·„·› ﬂ ‰“Ì·
            return File(memory, file.FileType, Path.GetFileName(file.Path));
        }

        // œ«·… ·· Õﬁﬁ „‰ „œÏ œ⁄„ ‰Ê⁄ «·„·›
        private bool IsSupportedFileType(string extension)
        {
            return extension == ".pdf" || extension == ".jpeg" || extension == ".jpg" || extension == ".png" || extension == ".gif";
        }

        // œ«·… ··Õ’Ê· ⁄·Ï ‰Ê⁄ «·„Õ ÊÏ ··„·›
        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            var types = GetMimeTypes();

            if (types.TryGetValue(ext, out var contentType))
            {
                return contentType;
            }
            else
            {
                // ≈À«—… «” À‰«¡ ·‰Ê⁄ „·› €Ì— „œ⁄Ê„
                throw new NotSupportedException("‰Ê⁄ „·› €Ì— „œ⁄Ê„. Ì—ÃÏ  Õ„Ì· „·›«  PDF √Ê JPEG √Ê JPG √Ê PNG √Ê GIF ›ﬁÿ.");
            }
        }

        // œ«·… ··Õ’Ê· ⁄·Ï √‰Ê«⁄ MIME
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
    {
        {".pdf", "application/pdf"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".png", "image/png"},
        {".gif", "image/gif"}
    };
        }

        public IActionResult Privacy()
        {
            return View();
        }

       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
