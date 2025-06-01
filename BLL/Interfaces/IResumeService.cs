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
		Task<IEnumerable<ResumeDTO>> GetAllResumesAsync(AppUserDTO requester);
		Task<ResumeDTO> GetResumeByIdAsync(int id, AppUserDTO requester);
		Task AddResumeAsync(ResumeDTO resumeDTO, AppUserDTO requester);
		Task UpdateResumeAsync(ResumeDTO resumeDTO, AppUserDTO requester);
		Task DeleteResumeAsync(int id, AppUserDTO requester);
	}
}
