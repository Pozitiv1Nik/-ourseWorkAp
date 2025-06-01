using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace DAL
	{
	public interface IUserRepository : IRepository<User>
	{
		Task<User> GetByUsernameAsync(string username);
		Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate);
	}
}
