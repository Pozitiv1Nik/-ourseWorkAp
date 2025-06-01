using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApDbContext _context;

		public IResumeRepository Resumes { get; }
		public IVacancyRepository Vacancies { get; }
		public IResumeVacancyLinkRepository ResumeVacancyLinks { get; }
		public IAppUserRepository AppUsers { get; }

		public UnitOfWork(ApDbContext context)
		{
			_context = context;
			Resumes = new ResumeRepository(context);
			Vacancies = new VacancyRepository(context);
			ResumeVacancyLinks = new ResumeVacancyLinkRepository(context);
			AppUsers = new AppUserRepository(context);
		}

		public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

		public void Dispose() => _context.Dispose();
	}

}
