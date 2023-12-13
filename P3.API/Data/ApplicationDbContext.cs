using Microsoft.EntityFrameworkCore;
using P3.API.Models.Domain;

namespace P3.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //public DbSet<tblHistoryData_1Min> tblHistoryData_1Min { get; set; }
        public DbSet<tblHistoryData_1Min> tblHistoryData_1Min { get; set; }
		public DbSet<tblHistoryData> tblHistoryData { get; set; }

    }
}
