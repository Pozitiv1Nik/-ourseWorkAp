using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace DAL
{
    public interface IResumeVacancyLinkRepository : IRepository<ResumeVacancyLink>
    {
        Task<IEnumerable<ResumeVacancyLink>> GetByResumeIdAsync(int resumeId);
        Task<IEnumerable<ResumeVacancyLink>> GetByVacancyIdAsync(int vacancyId);
    }
}
