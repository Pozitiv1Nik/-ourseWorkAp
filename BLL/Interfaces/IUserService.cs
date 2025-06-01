using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTO;
namespace BLL.Interfaces
{
	public interface IUserService
	{
		Task<IEnumerable<UserDTO>> GetAllUsersAsync();
		Task<UserDTO> GetUserByIdAsync(int id);
		Task AddUserAsync(UserDTO userDTO, string password);
		Task<UserDTO> AuthenticateAsync(string username, string password);
		Task UpdateUserAsync(UserDTO userDTO);
		Task DeleteUserAsync(int id);
        Task<UserDTO> GetByUsernameAsync(string username);
    }
}
