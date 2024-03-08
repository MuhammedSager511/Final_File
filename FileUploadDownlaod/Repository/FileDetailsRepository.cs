using FileUploadDownlaod.DataContext;
using FileUploadDownload.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadDownlaod.Repository
{
    public class FileDetailsRepository : IFileDetailsRepository
    {
        private readonly FileDetailsDbContext _context;

        public FileDetailsRepository(FileDetailsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FileDetails>> GetAllFiles()
        {
            return await _context.Files.ToListAsync();
        }

        public async Task<FileDetails> GetFileById(int id)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddFile(FileDetails file)
        {
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }

       
        public async Task DeleteFile(int id)
        {
            var file = await _context.Files.FindAsync(id);
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
        }

        public async Task<FileDetails> GetFileByName(string filename)
        {

            return await _context.Files.FirstOrDefaultAsync(f => f.Name == filename);

        }
    }
}
