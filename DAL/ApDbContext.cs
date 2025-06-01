using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace DAL
{
	public class ApDbContext : DbContext
	{
		public DbSet<User> Users { get; set; }
		public DbSet<Resume> Resumes { get; set; }
		public DbSet<Vacancy> Vacancies { get; set; }
		public DbSet<ResumeVacancyLink> ResumeVacancyLinks { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(@"Server=localhost;Database=BulletinBoardDb;Trusted_Connection=True;TrustServerCertificate=True;");
			}
		}
		public ApDbContext(DbContextOptions<ApDbContext> options) : base(options) { }

	}
}
