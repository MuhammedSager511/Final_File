using System.Collections.Generic;
using System;

namespace FileUploadDownload.Models
{
    public class FileDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string FileType { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

}
