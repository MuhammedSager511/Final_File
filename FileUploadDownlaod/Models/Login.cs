using System.ComponentModel.DataAnnotations;

namespace FileUploadDownlaod.Models
{
    public class Login
    {
        [EmailAddress]
        public required string Username { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }
    


    }
}
