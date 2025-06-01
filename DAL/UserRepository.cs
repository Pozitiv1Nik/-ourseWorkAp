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
	public class UserRepository : GenericRepository<User>, IUserRepository
	{
		public UserRepository(ApDbContext context) : base(context) { }

		public async Task<User> GetByUsernameAsync(string username)
		{
			return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
		}
		public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
		{
			return await _context.Users.Where(predicate).ToListAsync();
		}
	}

}
