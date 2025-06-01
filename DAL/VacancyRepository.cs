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
	public class VacancyRepository : GenericRepository<Vacancy>, IVacancyRepository
	{
		public VacancyRepository(ApDbContext context) : base(context) { }

		public async Task<IEnumerable<Vacancy>> SearchAsync(string keyword)
		{
			return await _context.Vacancies
				.Where(v => v.Title.Contains(keyword) || v.Description.Contains(keyword))
				.ToListAsync();
		}
		public async Task<IEnumerable<Vacancy>> FindAsync(Expression<Func<Vacancy, bool>> predicate)
		{
			return await _context.Vacancies.Where(predicate).ToListAsync();
		}
	}
}
