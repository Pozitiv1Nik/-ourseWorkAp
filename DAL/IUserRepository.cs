using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace DAL
	{
	public interface IAppUserRepository : IRepository<AppUser>
	{
		Task<AppUser> GetByAppUsernameAsync(string username);
		Task<IEnumerable<AppUser>> FindAsync(Expression<Func<AppUser, bool>> predicate);
	}
}
