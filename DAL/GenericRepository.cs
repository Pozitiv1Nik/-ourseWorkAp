using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace DAL
{
	public class GenericRepository<T> : IRepository<T> where T : class
	{
		protected readonly ApDbContext _context;
		public GenericRepository(ApDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();

		public async Task<T> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);

		public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);

		public void Remove(T entity) => _context.Set<T>().Remove(entity);

		public void Update(T entity) => _context.Set<T>().Update(entity);
	}

}
