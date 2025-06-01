using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class ResumeVacancyLink
	{
		public int Id { get; set; }
		public int ResumeId { get; set; }
		public Resume Resume { get; set; }
		public int VacancyId { get; set; }
		public Vacancy Vacancy { get; set; }
		public DateTime SubmittedAt { get; set; }
	}
}
