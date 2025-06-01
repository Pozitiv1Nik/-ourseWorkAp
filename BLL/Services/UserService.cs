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
	public class UserService : IUserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UserService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
		{
			var users = await _unitOfWork.Users.GetAllAsync();
			return users.Select(u => _mapper.Map<UserDTO>(u));
		}

		public async Task<UserDTO> GetUserByIdAsync(int id)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(id);
			if (user == null)
				throw new KeyNotFoundException("User not found");
			return _mapper.Map<UserDTO>(user);
		}

		public async Task AddUserAsync(UserDTO userDTO, string password)
		{
			var existing = await _unitOfWork.Users.FindAsync(u => u.UserName == userDTO.UserName);
			if (existing.Any())
				throw new InvalidOperationException("Username already exists");

			var user = _mapper.Map<User>(userDTO);
			user.Password = password;
			await _unitOfWork.Users.AddAsync(user);
			await _unitOfWork.CompleteAsync();
		}

		public async Task<UserDTO> AuthenticateAsync(string username, string password)
		{
			var users = await _unitOfWork.Users.FindAsync(u => u.UserName == username);
			var user = users.FirstOrDefault();
			if (user == null || user.Password != password)
				throw new UnauthorizedAccessException("Invalid username or password");

			return _mapper.Map<UserDTO>(user);
		}

		public async Task UpdateUserAsync(UserDTO userDTO)
		{
			var existing = await _unitOfWork.Users.GetByIdAsync(userDTO.Id);
			if (existing == null)
				throw new KeyNotFoundException("User not found");

			_mapper.Map(userDTO, existing);
			_unitOfWork.Users.Update(existing);
			await _unitOfWork.CompleteAsync();
		}

		public async Task DeleteUserAsync(int id)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(id);
			if (user == null)
				throw new KeyNotFoundException("User not found");

			_unitOfWork.Users.Remove(user);
			await _unitOfWork.CompleteAsync();
		}
	}
}
