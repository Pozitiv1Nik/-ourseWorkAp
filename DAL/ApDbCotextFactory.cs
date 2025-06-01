using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ApDbContextFactory : IDesignTimeDbContextFactory<ApDbContext>
{
    public ApDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApDbContext>();
        optionsBuilder.UseSqlite("Data Source=app.db"); // або твій рядок підключення

        return new ApDbContext(optionsBuilder.Options);
    }
}