using P3.API.Models.Domain;

namespace P3.API.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<tblHistoryData_1Min> HistoryData_1Min { get; }
        IGenericRepository<tblHistoryData> tblHistoryData { get; }
        //IGenericRepository<TblHistoryData1min> tblHistory_1Min { get; }
        //IGenericRepository<Target> Targets { get; }
        //IGenericRepository<Variable> Variables { get; }
        Task SaveAsync();
    }
}
