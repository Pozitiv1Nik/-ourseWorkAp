using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
	public interface IUnitOfWork : IDisposable
	{
		IResumeRepository Resumes { get; }
		IVacancyRepository Vacancies { get; }
		IResumeVacancyLinkRepository ResumeVacancyLinks { get; }
		IAppUserRepository AppUsers { get; }
		Task<int> CompleteAsync();
	}
}
