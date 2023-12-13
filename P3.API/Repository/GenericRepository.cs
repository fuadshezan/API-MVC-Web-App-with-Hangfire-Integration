using Microsoft.EntityFrameworkCore;
using P3.API.Data;
using P3.API.Repository.IRepository;
using System.Linq.Expressions;

namespace P3.API.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }
        public async Task CreateAsync(List<T> entity)
        {
            await _dbSet.AddRangeAsync(entity);
        }
        public void UpdateAsync(T entity)
        {
            //_context.Update(model);
            _dbSet.Attach(entity);
            _dbSet.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }
        public void DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? expression = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, List<string>? includes = null)
        {
            IQueryable<T> query = _dbSet;
            if (expression != null)
            {
                query = query.Where(expression);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            return await query.AsNoTracking().ToListAsync();
        }
        public async Task<T> GetFirstOrDefault(Expression<Func<T, bool>> expression, List<string>? includes = null)
        {
            IQueryable<T> query = _dbSet;
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return await query.AsNoTracking().FirstOrDefaultAsync(expression);
        }
        public async Task<bool> IsExistsAsync(Expression<Func<T, bool>>? expression = null)
        {
            IQueryable<T> query = _dbSet;
            return await query.AnyAsync(expression);
        }
        public async Task ExecuteSQLProcedureAsync(string param)
        {
            await _context.Database.ExecuteSqlRawAsync(param);
        }
    }
}
