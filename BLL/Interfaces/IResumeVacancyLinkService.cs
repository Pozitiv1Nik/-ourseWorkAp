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
        // Методи з авторизацією
        Task<IEnumerable<ResumeVacancyLinkDTO>> GetAllLinksAsync(UserDTO requester);
        Task<ResumeVacancyLinkDTO> GetLinkByIdAsync(int id, UserDTO requester);

        // Подача резюме на вакансію (працівник)
        Task ApplyResumeToVacancyAsync(int resumeId, int vacancyId, UserDTO requester);

        // Пропозиція вакансії по резюме (роботодавець)
        Task OfferVacancyToResumeAsync(int vacancyId, int resumeId, UserDTO requester);

        // Перегляд зв'язків для конкретного резюме
        Task<IEnumerable<ResumeVacancyLinkDTO>> GetLinksByResumeAsync(int resumeId, UserDTO requester);

        // Перегляд зв'язків для конкретної вакансії
        Task<IEnumerable<ResumeVacancyLinkDTO>> GetLinksByVacancyAsync(int vacancyId, UserDTO requester);

        // Видалення зв'язку
        Task DeleteLinkAsync(int id, UserDTO requester);

        // Перевірка існування зв'язку
        Task<bool> LinkExistsAsync(int resumeId, int vacancyId);

        // Старі методи для сумісності
        Task<IEnumerable<ResumeVacancyLinkDTO>> GetAllLinksAsync();
        Task AddLinkAsync(ResumeVacancyLinkDTO linkDTO);
        Task DeleteLinkAsync(int id);
    }
}