using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace CinemaBooking.Data;
public class CinemaDbContextFactory : IDesignTimeDbContextFactory<CinemaDbContext>
{
    public CinemaDbContext CreateDbContext(string[] args)
    {
        var directory = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(directory, "appsettings.json"))) directory = Path.Combine(directory, "CinemaBooking");
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder().SetBasePath(directory).AddJsonFile("appsettings.json").AddJsonFile($"appsettings.{environment}.json", optional: true).AddEnvironmentVariables().Build();
        var options = new DbContextOptionsBuilder<CinemaDbContext>().UseSqlServer(configuration.GetConnectionString("CinemaDb")).Options;
        return new CinemaDbContext(options);
    }
}
