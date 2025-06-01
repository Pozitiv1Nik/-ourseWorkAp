using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTO;
namespace BLL.Interfaces
{
	public interface IResumeVacancyLinkService
	{
		Task<IEnumerable<ResumeVacancyLinkDTO>> GetAllLinksAsync();
		Task AddLinkAsync(ResumeVacancyLinkDTO linkDTO);
		Task DeleteLinkAsync(int id);
	}
}
