using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
namespace DAL
{
	public class ResumeRepository : GenericRepository<Resume>, IResumeRepository
	{
		public ResumeRepository(ApDbContext context) : base(context) { }

		public async Task<IEnumerable<Resume>> SearchAsync(string keyword)
		{
			return await _context.Resumes
				.Where(r => r.Title.Contains(keyword) || r.Description.Contains(keyword))
				.ToListAsync();
		}
	}

}
