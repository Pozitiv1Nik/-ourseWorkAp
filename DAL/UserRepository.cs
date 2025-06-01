using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
namespace DAL
{
	public class AppUserRepository : GenericRepository<AppUser>, IAppUserRepository
	{
		public AppUserRepository(ApDbContext context) : base(context) { }

		public async Task<AppUser> GetByAppUsernameAsync(string username)
		{
			return await _context.AppUsers.FirstOrDefaultAsync(u => u.AppUserName == username);
		}
		public async Task<IEnumerable<AppUser>> FindAsync(Expression<Func<AppUser, bool>> predicate)
		{
			return await _context.AppUsers.Where(predicate).ToListAsync();
		}
	}

}
