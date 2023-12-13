using Microsoft.EntityFrameworkCore;

namespace P3.Web.Models.DTO
{
    [Keyless]
    public class ErrorModel
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
