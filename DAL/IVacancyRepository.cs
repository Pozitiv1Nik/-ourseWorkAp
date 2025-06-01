using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace DAL
{
	public interface IVacancyRepository : IRepository<Vacancy>
	{
		Task<IEnumerable<Vacancy>> SearchAsync(string keyword);
	}
}
