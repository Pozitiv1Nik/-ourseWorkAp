﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Vacancy
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
        public string Experience { get; set; }
        public string ExpectedSalary { get; set; }
        public int UserId { get; set; }
		public User User { get; set; }
		public ICollection<ResumeVacancyLink> ResumeVacancyLinks { get; set; }
	}
}
