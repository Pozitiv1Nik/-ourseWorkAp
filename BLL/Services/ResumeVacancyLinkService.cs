using AutoMapper;
using BLL.DTO;
using BLL.Interfaces;
using DAL;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ResumeVacancyLinkService : IResumeVacancyLinkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ResumeVacancyLinkService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ResumeVacancyLinkDTO>> GetAllLinksAsync(UserDTO requester)
        {
            if (requester.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can view all links.");

            var links = await _unitOfWork.ResumeVacancyLinks.GetAllAsync();
            return await MapLinksWithDetailsAsync(links);
        }

        public async Task<ResumeVacancyLinkDTO> GetLinkByIdAsync(int id, UserDTO requester)
        {
            var link = await _unitOfWork.ResumeVacancyLinks.GetByIdAsync(id);
            if (link == null)
                throw new KeyNotFoundException("Link not found.");

            // Перевіряємо права доступу
            var resume = await _unitOfWork.Resumes.GetByIdAsync(link.ResumeId);
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(link.VacancyId);

            if (requester.Role == UserRole.Admin ||
                (requester.Role == UserRole.Worker && resume.UserId == requester.Id) ||
                (requester.Role == UserRole.Employer && vacancy.UserId == requester.Id))
            {
                return await MapLinkWithDetailsAsync(link);
            }

            throw new UnauthorizedAccessException("Access denied to this link.");
        }

        public async Task ApplyResumeToVacancyAsync(int resumeId, int vacancyId, UserDTO requester)
        {
            if (requester.Role != UserRole.Worker)
                throw new UnauthorizedAccessException("Only workers can apply resumes to vacancies.");

            // Перевіряємо, чи належить резюме користувачу
            var resume = await _unitOfWork.Resumes.GetByIdAsync(resumeId);
            if (resume == null)
                throw new KeyNotFoundException("Resume not found.");

            if (resume.UserId != requester.Id)
                throw new UnauthorizedAccessException("You can only apply your own resumes.");

            // Перевіряємо, чи існує вакансія
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(vacancyId);
            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found.");

            // Перевіряємо, чи вже існує такий зв'язок
            if (await LinkExistsAsync(resumeId, vacancyId))
                throw new InvalidOperationException("Resume already applied to this vacancy.");

            var link = new ResumeVacancyLink
            {
                ResumeId = resumeId,
                VacancyId = vacancyId,
                SubmittedAt = DateTime.UtcNow
            };

            await _unitOfWork.ResumeVacancyLinks.AddAsync(link);
            await _unitOfWork.CompleteAsync();
        }

        public async Task OfferVacancyToResumeAsync(int vacancyId, int resumeId, UserDTO requester)
        {
            if (requester.Role != UserRole.Employer)
                throw new UnauthorizedAccessException("Only employers can offer vacancies to resumes.");

            // Перевіряємо, чи належить вакансія користувачу
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(vacancyId);
            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found.");

            if (vacancy.UserId != requester.Id)
                throw new UnauthorizedAccessException("You can only offer your own vacancies.");

            // Перевіряємо, чи існує резюме
            var resume = await _unitOfWork.Resumes.GetByIdAsync(resumeId);
            if (resume == null)
                throw new KeyNotFoundException("Resume not found.");

            // Перевіряємо, чи вже існує такий зв'язок
            if (await LinkExistsAsync(resumeId, vacancyId))
                throw new InvalidOperationException("Vacancy already offered to this resume.");

            var link = new ResumeVacancyLink
            {
                ResumeId = resumeId,
                VacancyId = vacancyId,
                SubmittedAt = DateTime.UtcNow
            };

            await _unitOfWork.ResumeVacancyLinks.AddAsync(link);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<ResumeVacancyLinkDTO>> GetLinksByResumeAsync(int resumeId, UserDTO requester)
        {
            var resume = await _unitOfWork.Resumes.GetByIdAsync(resumeId);
            if (resume == null)
                throw new KeyNotFoundException("Resume not found.");

            // Перевіряємо права доступу
            if (requester.Role != UserRole.Admin &&
                !(requester.Role == UserRole.Worker && resume.UserId == requester.Id))
            {
                throw new UnauthorizedAccessException("Access denied to this resume's links.");
            }

            var links = await _unitOfWork.ResumeVacancyLinks.FindAsync(l => l.ResumeId == resumeId);
            return await MapLinksWithDetailsAsync(links);
        }

        public async Task<IEnumerable<ResumeVacancyLinkDTO>> GetLinksByVacancyAsync(int vacancyId, UserDTO requester)
        {
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(vacancyId);
            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found.");

            // Перевіряємо права доступу
            if (requester.Role != UserRole.Admin &&
                !(requester.Role == UserRole.Employer && vacancy.UserId == requester.Id))
            {
                throw new UnauthorizedAccessException("Access denied to this vacancy's links.");
            }

            var links = await _unitOfWork.ResumeVacancyLinks.FindAsync(l => l.VacancyId == vacancyId);
            return await MapLinksWithDetailsAsync(links);
        }

        public async Task DeleteLinkAsync(int id, UserDTO requester)
        {
            var link = await _unitOfWork.ResumeVacancyLinks.GetByIdAsync(id);
            if (link == null)
                throw new KeyNotFoundException("Link not found.");

            var resume = await _unitOfWork.Resumes.GetByIdAsync(link.ResumeId);
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(link.VacancyId);

            // Перевіряємо права доступу
            if (requester.Role != UserRole.Admin &&
                !(requester.Role == UserRole.Worker && resume.UserId == requester.Id) &&
                !(requester.Role == UserRole.Employer && vacancy.UserId == requester.Id))
            {
                throw new UnauthorizedAccessException("Access denied to delete this link.");
            }

            _unitOfWork.ResumeVacancyLinks.Remove(link);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> LinkExistsAsync(int resumeId, int vacancyId)
        {
            var links = await _unitOfWork.ResumeVacancyLinks.FindAsync(l =>
                l.ResumeId == resumeId && l.VacancyId == vacancyId);
            return links.Any();
        }

        // Допоміжні методи для маппінгу з деталями
        private async Task<IEnumerable<ResumeVacancyLinkDTO>> MapLinksWithDetailsAsync(IEnumerable<ResumeVacancyLink> links)
        {
            var result = new List<ResumeVacancyLinkDTO>();

            foreach (var link in links)
            {
                result.Add(await MapLinkWithDetailsAsync(link));
            }

            return result;
        }

        private async Task<ResumeVacancyLinkDTO> MapLinkWithDetailsAsync(ResumeVacancyLink link)
        {
            var dto = _mapper.Map<ResumeVacancyLinkDTO>(link);

            var resume = await _unitOfWork.Resumes.GetByIdAsync(link.ResumeId);
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(link.VacancyId);
            var worker = await _unitOfWork.Users.GetByIdAsync(resume.UserId);
            var employer = await _unitOfWork.Users.GetByIdAsync(vacancy.UserId);

            dto.ResumeTitle = resume?.Title;
            dto.VacancyTitle = vacancy?.Title;
            dto.WorkerName = worker?.UserName;
            dto.EmployerName = employer?.UserName;

            return dto;
        }

        // Старі методи для сумісності
        public async Task<IEnumerable<ResumeVacancyLinkDTO>> GetAllLinksAsync()
        {
            var links = await _unitOfWork.ResumeVacancyLinks.GetAllAsync();
            return links.Select(l => _mapper.Map<ResumeVacancyLinkDTO>(l));
        }

        public async Task AddLinkAsync(ResumeVacancyLinkDTO linkDTO)
        {
            var link = _mapper.Map<ResumeVacancyLink>(linkDTO);
            link.SubmittedAt = DateTime.UtcNow;
            await _unitOfWork.ResumeVacancyLinks.AddAsync(link);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteLinkAsync(int id)
        {
            var link = await _unitOfWork.ResumeVacancyLinks.GetByIdAsync(id);
            if (link == null)
                throw new KeyNotFoundException("Link not found");

            _unitOfWork.ResumeVacancyLinks.Remove(link);
            await _unitOfWork.CompleteAsync();
        }
    }
}