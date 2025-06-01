using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Entities;

namespace DAL
{
    public interface IResumeVacancyLinkRepository : IRepository<ResumeVacancyLink>
    {
        Task<IEnumerable<ResumeVacancyLink>> FindAsync(Expression<Func<ResumeVacancyLink, bool>> predicate);
        Task<IEnumerable<ResumeVacancyLink>> GetByResumeIdAsync(int resumeId);
        Task<IEnumerable<ResumeVacancyLink>> GetByVacancyIdAsync(int vacancyId);
    }
}