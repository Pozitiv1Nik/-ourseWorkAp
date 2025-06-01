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

		public async Task<IEnumerable<ResumeDTO>> GetAllResumesAsync(UserDTO requester)
		{
			var resumes = await _unitOfWork.Resumes.GetAllAsync();

			if (requester.Role == UserRole.Admin || requester.Role == UserRole.Employer)
			{
				return resumes.Select(r => _mapper.Map<ResumeDTO>(r));
			}

			if (requester.Role == UserRole.Worker)
			{
				return resumes
					.Where(r => r.UserId == requester.Id)
					.Select(r => _mapper.Map<ResumeDTO>(r));
			}

			return Enumerable.Empty<ResumeDTO>();
		}

		public async Task<ResumeDTO> GetResumeByIdAsync(int id, UserDTO requester)
		{
			var resume = await _unitOfWork.Resumes.GetByIdAsync(id);
			if (resume == null)
				throw new KeyNotFoundException("Resume not found.");

			if (requester.Role == UserRole.Admin || requester.Role == UserRole.Employer)
			{
				return _mapper.Map<ResumeDTO>(resume);
			}

			if (requester.Role == UserRole.Worker && resume.UserId == requester.Id)
			{
				return _mapper.Map<ResumeDTO>(resume);
			}

			throw new UnauthorizedAccessException("Access denied to this resume.");
		}

		public async Task AddResumeAsync(ResumeDTO resumeDTO, UserDTO requester)
		{
			if (requester.Role != UserRole.Worker)
				throw new UnauthorizedAccessException("Only workers can add resumes.");

			resumeDTO.UserId = requester.Id;
			var resume = _mapper.Map<Resume>(resumeDTO);
			await _unitOfWork.Resumes.AddAsync(resume);
			await _unitOfWork.CompleteAsync();
		}

		public async Task UpdateResumeAsync(ResumeDTO resumeDTO, UserDTO requester)
		{
			var existing = await _unitOfWork.Resumes.GetByIdAsync(resumeDTO.Id);
			if (existing == null)
				throw new KeyNotFoundException("Resume not found");

			if (requester.Role != UserRole.Worker || existing.UserId != requester.Id)
				throw new UnauthorizedAccessException("Only the owner can update the resume.");

			_mapper.Map(resumeDTO, existing);
			_unitOfWork.Resumes.Update(existing);
			await _unitOfWork.CompleteAsync();
		}

		public async Task DeleteResumeAsync(int id, UserDTO requester)
		{
			var resume = await _unitOfWork.Resumes.GetByIdAsync(id);
			if (resume == null)
				throw new KeyNotFoundException("Resume not found");

			if (requester.Role != UserRole.Worker || resume.UserId != requester.Id)
				throw new UnauthorizedAccessException("Only the owner can delete the resume.");

			_unitOfWork.Resumes.Remove(resume);
			await _unitOfWork.CompleteAsync();
		}
	}
}
