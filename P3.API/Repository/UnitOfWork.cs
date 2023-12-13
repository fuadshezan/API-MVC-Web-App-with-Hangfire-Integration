using P3.API.Data;
using P3.API.Models.Domain;
using P3.API.Models.DTO;
using P3.API.Repository.IRepository;

namespace P3.API.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        private GenericRepository<tblHistoryData_1Min> _HistoryData_1Min;
        private GenericRepository<tblHistoryData> _tblHistoryData;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<tblHistoryData_1Min> HistoryData_1Min => _HistoryData_1Min ??= new GenericRepository<tblHistoryData_1Min>(_context);
        public IGenericRepository<tblHistoryData> tblHistoryData => _tblHistoryData ??= new GenericRepository<tblHistoryData>(_context);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool dispose)
        {
            if (dispose)
            {
                _context.Dispose();
            }
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
