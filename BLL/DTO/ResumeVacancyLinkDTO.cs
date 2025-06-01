using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
	public class ResumeVacancyLinkDTO
	{
		public int Id { get; set; }
		public int ResumeId { get; set; }
		public int VacancyId { get; set; }
		public DateTime SubmittedAt { get; set; }
	}
}
