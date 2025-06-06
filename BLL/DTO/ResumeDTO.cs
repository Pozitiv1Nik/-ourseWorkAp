﻿using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
	public class ResumeDTO
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Experience { get; set; }
		public decimal ExpectedSalary { get; set; }
		public int UserId { get; set; }
	}
}
