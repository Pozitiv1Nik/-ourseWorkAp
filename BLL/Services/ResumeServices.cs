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
	public class ResumeService : IResumeService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public ResumeService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<ResumeDTO>> GetAllResumesAsync(AppUserDTO requester)
		{
			var resumes = await _unitOfWork.Resumes.GetAllAsync();

			if (requester.Role == AppUserRole.Admin || requester.Role == AppUserRole.Employer)
			{
				return resumes.Select(r => _mapper.Map<ResumeDTO>(r));
			}

			if (requester.Role == AppUserRole.Worker)
			{
				return resumes
					.Where(r => r.AppUserId == requester.Id)
					.Select(r => _mapper.Map<ResumeDTO>(r));
			}

			return Enumerable.Empty<ResumeDTO>();
		}

		public async Task<ResumeDTO> GetResumeByIdAsync(int id, AppUserDTO requester)
		{
			var resume = await _unitOfWork.Resumes.GetByIdAsync(id);
			if (resume == null)
				throw new KeyNotFoundException("Resume not found.");

			if (requester.Role == AppUserRole.Admin || requester.Role == AppUserRole.Employer)
			{
				return _mapper.Map<ResumeDTO>(resume);
			}

			if (requester.Role == AppUserRole.Worker && resume.AppUserId == requester.Id)
			{
				return _mapper.Map<ResumeDTO>(resume);
			}

			throw new UnauthorizedAccessException("Access denied to this resume.");
		}

		public async Task AddResumeAsync(ResumeDTO resumeDTO, AppUserDTO requester)
		{
			if (requester.Role != AppUserRole.Worker)
				throw new UnauthorizedAccessException("Only workers can add resumes.");

			resumeDTO.AppUserId = requester.Id;
			var resume = _mapper.Map<Resume>(resumeDTO);
			await _unitOfWork.Resumes.AddAsync(resume);
			await _unitOfWork.CompleteAsync();
		}

		public async Task UpdateResumeAsync(ResumeDTO resumeDTO, AppUserDTO requester)
		{
			var existing = await _unitOfWork.Resumes.GetByIdAsync(resumeDTO.Id);
			if (existing == null)
				throw new KeyNotFoundException("Resume not found");

			if (requester.Role != AppUserRole.Worker || existing.AppUserId != requester.Id)
				throw new UnauthorizedAccessException("Only the owner can update the resume.");

			_mapper.Map(resumeDTO, existing);
			_unitOfWork.Resumes.Update(existing);
			await _unitOfWork.CompleteAsync();
		}

		public async Task DeleteResumeAsync(int id, AppUserDTO requester)
		{
			var resume = await _unitOfWork.Resumes.GetByIdAsync(id);
			if (resume == null)
				throw new KeyNotFoundException("Resume not found");

			if (requester.Role != AppUserRole.Worker || resume.AppUserId != requester.Id)
				throw new UnauthorizedAccessException("Only the owner can delete the resume.");

			_unitOfWork.Resumes.Remove(resume);
			await _unitOfWork.CompleteAsync();
		}
	}
}
