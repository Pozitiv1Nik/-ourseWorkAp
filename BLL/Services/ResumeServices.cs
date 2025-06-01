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
	public class ResumeService : IResumeService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public ResumeService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<ResumeDTO>> GetAllResumesAsync()
		{
			var resumes = await _unitOfWork.Resumes.GetAllAsync();
			return resumes.Select(r => _mapper.Map<ResumeDTO>(r));
		}

		public async Task<ResumeDTO> GetResumeByIdAsync(int id)
		{
			var resume = await _unitOfWork.Resumes.GetByIdAsync(id);
			if (resume == null) return null;
			return _mapper.Map<ResumeDTO>(resume);
		}

		public async Task AddResumeAsync(ResumeDTO resumeDTO)
		{
			var resume = _mapper.Map<Resume>(resumeDTO);
			await _unitOfWork.Resumes.AddAsync(resume);
			await _unitOfWork.CompleteAsync();
		}

		public async Task UpdateResumeAsync(ResumeDTO resumeDTO)
		{
			var existingResume = await _unitOfWork.Resumes.GetByIdAsync(resumeDTO.Id);
			if (existingResume == null) throw new KeyNotFoundException("Resume not found");

			_mapper.Map(resumeDTO, existingResume);

			_unitOfWork.Resumes.Update(existingResume);
			await _unitOfWork.CompleteAsync();
		}

		public async Task DeleteResumeAsync(int id)
		{
			var resume = await _unitOfWork.Resumes.GetByIdAsync(id);
			if (resume == null) throw new KeyNotFoundException("Resume not found");

			_unitOfWork.Resumes.Remove(resume);
			await _unitOfWork.CompleteAsync();
		}
	}
}
