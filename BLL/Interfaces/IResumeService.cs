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
		Task<IEnumerable<ResumeDTO>> GetAllResumesAsync();
		Task<ResumeDTO> GetResumeByIdAsync(int id);
		Task AddResumeAsync(ResumeDTO resumeDTO);
		Task UpdateResumeAsync(ResumeDTO resumeDTO);
		Task DeleteResumeAsync(int id);
	}
}
