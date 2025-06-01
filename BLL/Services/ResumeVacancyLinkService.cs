using AutoMapper;
using BLL.DTO;
using BLL.Interfaces;
using DAL;
using Domain.Entities;
using System.Collections.Generic;
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

		public async Task<IEnumerable<ResumeVacancyLinkDTO>> GetAllLinksAsync()
		{
			var links = await _unitOfWork.ResumeVacancyLinks.GetAllAsync();
			return _mapper.Map<IEnumerable<ResumeVacancyLinkDTO>>(links);
		}

		public async Task AddLinkAsync(ResumeVacancyLinkDTO linkDTO)
		{
			var entity = _mapper.Map<ResumeVacancyLink>(linkDTO);
			await _unitOfWork.ResumeVacancyLinks.AddAsync(entity);
			await _unitOfWork.CompleteAsync();
		}

		public async Task DeleteLinkAsync(int id)
		{
			var entity = await _unitOfWork.ResumeVacancyLinks.GetByIdAsync(id);
			if (entity == null)
				throw new KeyNotFoundException("Link not found");

			_unitOfWork.ResumeVacancyLinks.Remove(entity);
			await _unitOfWork.CompleteAsync();
		}
	}
}
