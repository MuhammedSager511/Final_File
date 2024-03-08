using System.Collections.Generic;
using System.Threading.Tasks;
using FileUploadDownload.Models;

namespace FileUploadDownlaod.Repository
{
    public interface IFileDetailsRepository
    {
        Task<IEnumerable<FileDetails>> GetAllFiles();
        Task<FileDetails> GetFileById(int id);
        Task AddFile(FileDetails file);
        
        Task DeleteFile(int id);
        Task<FileDetails> GetFileByName(string filename);
    }
}
