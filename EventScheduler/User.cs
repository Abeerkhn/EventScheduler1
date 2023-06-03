using System.ComponentModel.DataAnnotations;

namespace EventScheduler
{
    public class User
    {
        [Key]
        public string Username { get; set; } = string.Empty;
        public  byte[] Passwordhash { get; set; }
        public byte[] Passwordsalt { get; set; }
    }
}
