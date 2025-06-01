using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTO;
using Domain.Entities;
namespace BLL.Interfaces
{
    public interface IResumeService
    {
		Task<IEnumerable<ResumeDTO>> GetAllResumesAsync(UserDTO requester);
		Task<ResumeDTO> GetResumeByIdAsync(int id, UserDTO requester);
		Task AddResumeAsync(ResumeDTO resumeDTO, UserDTO requester);
		Task UpdateResumeAsync(ResumeDTO resumeDTO, UserDTO requester);
		Task DeleteResumeAsync(int id, UserDTO requester);
	}
}
