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
	public class AppUserService : IAppUserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AppUserService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<AppUserDTO>> GetAllAppUsersAsync()
		{
			var users = await _unitOfWork.AppUsers.GetAllAsync();
			return users.Select(u => _mapper.Map<AppUserDTO>(u));
		}

		public async Task<AppUserDTO> GetAppUserByIdAsync(int id)
		{
			var user = await _unitOfWork.AppUsers.GetByIdAsync(id);
			if (user == null)
				throw new KeyNotFoundException("AppUser not found");
			return _mapper.Map<AppUserDTO>(user);
		}

		public async Task AddAppUserAsync(AppUserDTO userDTO, string password)
		{
			var existing = await _unitOfWork.AppUsers.FindAsync(u => u.AppUserName == userDTO.AppUserName);
			if (existing.Any())
				throw new InvalidOperationException("AppUsername already exists");

			var user = _mapper.Map<AppUser>(userDTO);
			user.Password = password;
			await _unitOfWork.AppUsers.AddAsync(user);
			await _unitOfWork.CompleteAsync();
		}

		public async Task<AppUserDTO> AuthenticateAsync(string username, string password)
		{
			var users = await _unitOfWork.AppUsers.FindAsync(u => u.AppUserName == username);
			var user = users.FirstOrDefault();
			if (user == null || user.Password != password)
				throw new UnauthorizedAccessException("Invalid username or password");

			return _mapper.Map<AppUserDTO>(user);
		}

		public async Task UpdateAppUserAsync(AppUserDTO userDTO)
		{
			var existing = await _unitOfWork.AppUsers.GetByIdAsync(userDTO.Id);
			if (existing == null)
				throw new KeyNotFoundException("AppUser not found");

			_mapper.Map(userDTO, existing);
			_unitOfWork.AppUsers.Update(existing);
			await _unitOfWork.CompleteAsync();
		}

		public async Task DeleteAppUserAsync(int id)
		{
			var user = await _unitOfWork.AppUsers.GetByIdAsync(id);
			if (user == null)
				throw new KeyNotFoundException("AppUser not found");

			_unitOfWork.AppUsers.Remove(user);
			await _unitOfWork.CompleteAsync();
		}
	}
}
