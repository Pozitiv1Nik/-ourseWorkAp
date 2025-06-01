using AutoMapper;
using BLL.DTO;
using BLL.Interfaces;
using DAL;
using Domain.Entities;
using System;
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
