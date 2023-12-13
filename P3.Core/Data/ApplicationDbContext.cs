using Microsoft.EntityFrameworkCore;
using P3.Web.Models;

namespace P3.Core.Data
{
    public class ApplicationDbContext:DbContext
    {
        

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
            
        }
        public DbSet<Symbol>tblSymbol { get; set; }
        public DbSet<tblDownloadStatus> tblDownloadStatus{ get; set; }
    }
}
