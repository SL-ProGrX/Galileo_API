using System.ComponentModel.DataAnnotations;

namespace Galileo.Models
{
    public class LogonUpdateData
    {
        public string usuario { get; set; } = string.Empty;
        [Required]
        public int id { get; set; }
        public string email { get; set; } = string.Empty;
        public string tell_cell { get; set; } = string.Empty;
    }
}
