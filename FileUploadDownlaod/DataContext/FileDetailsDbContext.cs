using FileUploadDownlaod.Models;
using FileUploadDownload.Models;
using Microsoft.EntityFrameworkCore;

 
namespace FileUploadDownlaod.DataContext
{
    public class FileDetailsDbContext : DbContext
    {
        public FileDetailsDbContext(DbContextOptions<FileDetailsDbContext> options) : base(options)
        {
        }

        public DbSet<FileDetails> Files { get; set; }

        
    }
}
