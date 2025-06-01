using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL
{
	public class ApDbContextFactory : IDesignTimeDbContextFactory<ApDbContext>
	{
		public ApDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ApDbContext>();
			optionsBuilder.UseSqlServer(@"Server=localhost;Database=BulletinBoardDb;Trusted_Connection=True;TrustServerCertificate=True;");

			return new ApDbContext(optionsBuilder.Options);
		}
	}
}
