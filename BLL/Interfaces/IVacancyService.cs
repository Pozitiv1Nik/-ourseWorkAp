using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTO;
namespace BLL.Interfaces
{
	public interface IVacancyService
	{
		Task<IEnumerable<VacancyDTO>> GetAllVacanciesAsync();
		Task<VacancyDTO> GetVacancyByIdAsync(int id);
		Task AddVacancyAsync(VacancyDTO vacancyDTO);
		Task UpdateVacancyAsync(VacancyDTO vacancyDTO);
		Task DeleteVacancyAsync(int id);
	}
}
