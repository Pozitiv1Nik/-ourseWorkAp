using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTO;
namespace BLL.Interfaces
{
	public interface IAppUserService
	{
		Task<IEnumerable<AppUserDTO>> GetAllAppUsersAsync();
		Task<AppUserDTO> GetAppUserByIdAsync(int id);
		Task AddAppUserAsync(AppUserDTO userDTO, string password);
		Task<AppUserDTO> AuthenticateAsync(string username, string password);
		Task UpdateAppUserAsync(AppUserDTO userDTO);
		Task DeleteAppUserAsync(int id);
	}
}
