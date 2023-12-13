using Microsoft.EntityFrameworkCore;

namespace P3.Web.Models
{
    [Keyless]
    public class tblDownloadStatus
    {
        public DateTime dtDate {  get; set; }
        public string SymName { get; set; }
        public string MinDataStatus { get; set; }
        public string DailyDataStatus { get; set;}
    }
}
