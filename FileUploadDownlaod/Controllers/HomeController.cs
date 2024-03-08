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

        // ������ �������� ���� �������
        public async Task<IActionResult> Index()
        {
            var files = await _fileRepository.GetAllFiles();
            return View(files);
        }





        [HttpPost]
        public async Task<IActionResult> Index(IFormFile[] files, string description, string createdBy)
        {
            // ������ ��� ��� ���� ������� ����� �������
            if (files == null || files.Length == 0)
            {
                // ����� ����� ��� ��� �� ��� ����� �����
                ViewBag.ErrorMessage = "������ ����� �����  �������.";
                // ������ ��� ���� ������
                var model1 = await _fileRepository.GetAllFiles();
                return View(model1);
            }

            foreach (var file in files)
            {
                // ������ ��� ��� ��� ����� ������� ������
                var existingFile = await _fileRepository.GetFileByName(file.FileName);
                if (existingFile != null)
                {
                    ViewBag.ErrorMessage = $"����� '{file.FileName}' ����� ������.";
                    var model2 = await _fileRepository.GetAllFiles();
                    return View(model2);
                }

                var fileName = Path.GetFileName(file.FileName);
                var ext = Path.GetExtension(fileName).ToLowerInvariant();

                // ����� ���� ���� ����� ��� ������ �����
                var subfolder = Path.Combine("wwwroot\\files", ext.Substring(1));

                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), subfolder);


                
                // ������ ��� ��� ��� ��� ����� �������
                if (!IsSupportedFileType(ext))
                {
                    // ����� ����� ��� ���� ����� ��� �������
                    ViewBag.ErrorMessage = $"��� ����� ��� �����: {ext}. ������ ����� ����� PDF �� JPEG �� JPG �� PNG �� GIF ���.";

                    var model2 = await _fileRepository.GetAllFiles();
                    return View(model2);
                }


                // ������ ��� ��� ��� ������ ������ �������
                if (!Directory.Exists(directoryPath))
                {
                    // ����� ������ ������ ��� �� ��� �������
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, fileName);

                // ��� ����� ��� �����
                using (var localFile = System.IO.File.OpenWrite(filePath))
                using (var uploadedFile = file.OpenReadStream())
                {
                    uploadedFile.CopyTo(localFile);
                }

                // ����� ������ ����� ��� ���� ��������
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

            // ����� ����� ����
            ViewBag.Message = "�� ����� ������� �����";
            // ��� ���� ������� 
            var model = await _fileRepository.GetAllFiles();
            return View(model);
        }






        // ���� ���� ���
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // ������ ��� ������ ����� ������ id
            var file = await _fileRepository.GetFileById(id);
            // ������ ��� ��� ��� ����� �������
            if (file == null)
            {
                // ����� ��� 404 ��� �� ��� ������ ��� �����
                return NotFound();
            }

            // ��� ����� �� �������
            System.IO.File.Delete(file.Path);

            // ��� ����� �� ����� ��������
            await _fileRepository.DeleteFile(id);

            
            return RedirectToAction(nameof(Index));
        }

        // ���� ������ ���
        public async Task<IActionResult> Download(string filename)
        {
            // ������ ��� ��� ��� ��� ����� ������
            if (filename == null)
                return Content("��� ����� ��� �����");

            // ������ ��� ������ ����� ������ �����
            var file = await _fileRepository.GetFileByName(filename);
            // ������ ��� ��� ��� ����� �������
            if (file == null)
                return NotFound();

            // ����� ����� �� ����� ��� ������ �����
            var memory = new MemoryStream();
            using (var stream = new FileStream(file.Path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            // ����� ����� ������
            return File(memory, file.FileType, Path.GetFileName(file.Path));
        }

        // ���� ������ �� ��� ��� ��� �����
        private bool IsSupportedFileType(string extension)
        {
            return extension == ".pdf" || extension == ".jpeg" || extension == ".jpg" || extension == ".png" || extension == ".gif";
        }

        // ���� ������ ��� ��� ������� �����
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
                // ����� ������� ���� ��� ��� �����
                throw new NotSupportedException("��� ��� ��� �����. ���� ����� ����� PDF �� JPEG �� JPG �� PNG �� GIF ���.");
            }
        }

        // ���� ������ ��� ����� MIME
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
