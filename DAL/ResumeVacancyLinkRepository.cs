using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
	public class ResumeVacancyLinkRepository : GenericRepository<ResumeVacancyLink>, IResumeVacancyLinkRepository
	{
		public ResumeVacancyLinkRepository(ApDbContext context) : base(context) { }

		public async Task<IEnumerable<ResumeVacancyLink>> GetByResumeIdAsync(int resumeId)
		{
			return await _context.ResumeVacancyLinks
				.Where(link => link.ResumeId == resumeId)
				.ToListAsync();
		}

		public async Task<IEnumerable<ResumeVacancyLink>> GetByVacancyIdAsync(int vacancyId)
		{
			return await _context.ResumeVacancyLinks
				.Where(link => link.VacancyId == vacancyId)
				.ToListAsync();
		}
	}
}
