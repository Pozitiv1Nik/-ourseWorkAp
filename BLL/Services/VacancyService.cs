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
    public class VacancyService : IVacancyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VacancyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VacancyDTO>> GetAllVacanciesAsync(UserDTO requester)
        {
            var vacancies = await _unitOfWork.Vacancies.GetAllAsync();

            if (requester.Role == UserRole.Admin)
            {
                return vacancies.Select(v => _mapper.Map<VacancyDTO>(v));
            }

            if (requester.Role == UserRole.Worker)
            {
                // Працівники бачать всі вакансії для пошуку роботи
                return vacancies.Select(v => _mapper.Map<VacancyDTO>(v));
            }

            if (requester.Role == UserRole.Employer)
            {
                // Роботодавці бачать тільки свої вакансії
                return vacancies
                    .Where(v => v.UserId == requester.Id)
                    .Select(v => _mapper.Map<VacancyDTO>(v));
            }

            return Enumerable.Empty<VacancyDTO>();
        }

        public async Task<VacancyDTO> GetVacancyByIdAsync(int id, UserDTO requester)
        {
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);
            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found.");

            if (requester.Role == UserRole.Admin || requester.Role == UserRole.Worker)
            {
                return _mapper.Map<VacancyDTO>(vacancy);
            }

            if (requester.Role == UserRole.Employer && vacancy.UserId == requester.Id)
            {
                return _mapper.Map<VacancyDTO>(vacancy);
            }

            throw new UnauthorizedAccessException("Access denied to this vacancy.");
        }

        public async Task AddVacancyAsync(VacancyDTO vacancyDTO, UserDTO requester)
        {
            if (requester.Role != UserRole.Employer)
                throw new UnauthorizedAccessException("Only employers can add vacancies.");

            vacancyDTO.UserId = requester.Id;
            var vacancy = _mapper.Map<Vacancy>(vacancyDTO);
            await _unitOfWork.Vacancies.AddAsync(vacancy);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateVacancyAsync(VacancyDTO vacancyDTO, UserDTO requester)
        {
            var existing = await _unitOfWork.Vacancies.GetByIdAsync(vacancyDTO.Id);
            if (existing == null)
                throw new KeyNotFoundException("Vacancy not found");

            if (requester.Role != UserRole.Employer || existing.UserId != requester.Id)
                throw new UnauthorizedAccessException("Only the owner can update the vacancy.");

            _mapper.Map(vacancyDTO, existing);
            _unitOfWork.Vacancies.Update(existing);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteVacancyAsync(int id, UserDTO requester)
        {
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);
            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found");

            if (requester.Role == 0 || (requester.Role == UserRole.Employer && requester.Id == vacancy.UserId))
            {
                _unitOfWork.Vacancies.Remove(vacancy);
                await _unitOfWork.CompleteAsync();
            }
            else 
            { 
                throw new UnauthorizedAccessException("Only the owner or administrator can delete the vacancy."); 
            }
        }

        public async Task<IEnumerable<VacancyDTO>> SearchVacanciesAsync(string keyword, UserDTO requester)
        {
            // Тільки працівники та адміни можуть шукати вакансії
            if (requester.Role != UserRole.Worker && requester.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only workers and admins can search vacancies.");

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAllVacanciesAsync(requester);
            }

            var filtered = await _unitOfWork.Vacancies.FindAsync(v =>
                v.Title.Contains(keyword) || v.Description.Contains(keyword));

            return filtered.Select(v => _mapper.Map<VacancyDTO>(v));
        }

        public async Task<IEnumerable<VacancyDTO>> GetVacanciesByEmployerAsync(int employerId)
        {
            var vacancies = await _unitOfWork.Vacancies.FindAsync(v => v.UserId == employerId);
            return vacancies.Select(v => _mapper.Map<VacancyDTO>(v));
        }

        // Для старого інтерфейсу - залишаємо для сумісності
        public async Task<IEnumerable<VacancyDTO>> GetAllVacanciesAsync()
        {
            var vacancies = await _unitOfWork.Vacancies.GetAllAsync();
            return vacancies.Select(v => _mapper.Map<VacancyDTO>(v));
        }

        public async Task<VacancyDTO> GetVacancyByIdAsync(int id)
        {
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);
            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found");
            return _mapper.Map<VacancyDTO>(vacancy);
        }

        public async Task AddVacancyAsync(VacancyDTO vacancyDTO)
        {
            var vacancy = _mapper.Map<Vacancy>(vacancyDTO);
            await _unitOfWork.Vacancies.AddAsync(vacancy);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateVacancyAsync(VacancyDTO vacancyDTO)
        {
            var existing = await _unitOfWork.Vacancies.GetByIdAsync(vacancyDTO.Id);
            if (existing == null)
                throw new KeyNotFoundException("Vacancy not found");

            _mapper.Map(vacancyDTO, existing);
            _unitOfWork.Vacancies.Update(existing);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteVacancyAsync(int id)
        {
            var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);
            if (vacancy == null)
                throw new KeyNotFoundException("Vacancy not found");

            _unitOfWork.Vacancies.Remove(vacancy);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<VacancyDTO>> SearchVacanciesAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var allVacancies = await _unitOfWork.Vacancies.GetAllAsync();
                return allVacancies.Select(v => _mapper.Map<VacancyDTO>(v));
            }
            var filtered = await _unitOfWork.Vacancies.FindAsync(v =>
                v.Title.Contains(keyword) || v.Description.Contains(keyword));

            return filtered.Select(v => _mapper.Map<VacancyDTO>(v));
        }
    }
}