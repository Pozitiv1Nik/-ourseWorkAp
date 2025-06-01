using AutoMapper;
using BLL.DTO;
using BLL.Interfaces;
using DAL;
using Domain.Entities;
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

		public async Task<IEnumerable<VacancyDTO>> GetAllVacanciesAsync()
		{
			var vacancies = await _unitOfWork.Vacancies.GetAllAsync();
			return vacancies.Select(v => _mapper.Map<VacancyDTO>(v));
		}

		public async Task<VacancyDTO> GetVacancyByIdAsync(int id)
		{
			var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);
			if (vacancy == null) return null;
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
			var existingVacancy = await _unitOfWork.Vacancies.GetByIdAsync(vacancyDTO.Id);
			if (existingVacancy == null) throw new KeyNotFoundException("Vacancy not found");

			_mapper.Map(vacancyDTO, existingVacancy);
			_unitOfWork.Vacancies.Update(existingVacancy);
			await _unitOfWork.CompleteAsync();
		}

		public async Task DeleteVacancyAsync(int id)
		{
			var vacancy = await _unitOfWork.Vacancies.GetByIdAsync(id);
			if (vacancy == null) throw new KeyNotFoundException("Vacancy not found");

			_unitOfWork.Vacancies.Remove(vacancy);
			await _unitOfWork.CompleteAsync();
		}
	}
}
