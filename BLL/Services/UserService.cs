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
	public class UserService : IUserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UserService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<UserDTO> GetByUsernameAsync(string username)
		{
			var user = await _unitOfWork.Users.GetByUsernameAsync(username);
			if (user == null) return null;
			return _mapper.Map<UserDTO>(user);
		}

		public async Task<UserDTO> GetUserByIdAsync(int id)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(id);
			if (user == null) return null;
			return _mapper.Map<UserDTO>(user);
		}

		public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
		{
			var users = await _unitOfWork.Users.GetAllAsync();
			return users.Select(u => _mapper.Map<UserDTO>(u));
		}

		public async Task AddUserAsync(UserDTO dto, string password)
		{
			var user = _mapper.Map<User>(dto);
			user.Password = password;

			await _unitOfWork.Users.AddAsync(user);
			await _unitOfWork.CompleteAsync();
		}

		public async Task UpdateUserAsync(UserDTO dto)
		{
			var existingUser = await _unitOfWork.Users.GetByIdAsync(dto.Id);
			if (existingUser == null) throw new KeyNotFoundException("User not found");

			_mapper.Map(dto, existingUser);

			_unitOfWork.Users.Update(existingUser);
			await _unitOfWork.CompleteAsync();
		}

		public async Task DeleteUserAsync(int id)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(id);
			if (user == null) throw new KeyNotFoundException("User not found");

			_unitOfWork.Users.Remove(user);
			await _unitOfWork.CompleteAsync();
		}

		public async Task<UserDTO> AuthenticateAsync(string username, string password)
		{
			var user = await _unitOfWork.Users.GetByUsernameAsync(username);
			if (user == null) return null;

			if (user.Password == password)
				return _mapper.Map<UserDTO>(user);

			return null;
		}
	}
}
