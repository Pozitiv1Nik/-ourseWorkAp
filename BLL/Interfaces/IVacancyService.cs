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
        // Методи з авторизацією (нові)
        Task<IEnumerable<VacancyDTO>> GetAllVacanciesAsync(UserDTO requester);
        Task<VacancyDTO> GetVacancyByIdAsync(int id, UserDTO requester);
        Task AddVacancyAsync(VacancyDTO vacancyDTO, UserDTO requester);
        Task UpdateVacancyAsync(VacancyDTO vacancyDTO, UserDTO requester);
        Task DeleteVacancyAsync(int id, UserDTO requester);
        Task<IEnumerable<VacancyDTO>> SearchVacanciesAsync(string keyword, UserDTO requester);
        Task<IEnumerable<VacancyDTO>> GetVacanciesByEmployerAsync(int employerId);

        // Старі методи для сумісності (можна видалити пізніше)
        Task<IEnumerable<VacancyDTO>> GetAllVacanciesAsync();
        Task<VacancyDTO> GetVacancyByIdAsync(int id);
        Task AddVacancyAsync(VacancyDTO vacancyDTO);
        Task UpdateVacancyAsync(VacancyDTO vacancyDTO);
        Task DeleteVacancyAsync(int id);
        Task<IEnumerable<VacancyDTO>> SearchVacanciesAsync(string keyword);
    }
}