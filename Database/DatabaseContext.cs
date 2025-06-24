using Microsoft.EntityFrameworkCore;

namespace rugwatch.Database;

internal class DatabaseContext(string dbPath) : DbContext
{
	public DbSet<Trade> Trades { get; set; } = null!;

	public void Seed()
	{
		string seedSqlPath = Path.Combine(
			Environment.CurrentDirectory, "Database", "CreateTables.sql");
		if (!File.Exists(seedSqlPath))
		{
			return;
		}

		Database.ExecuteSqlRaw(File.ReadAllText(seedSqlPath));
		Task.Delay(100).Wait();
	}

	protected override void OnConfiguring(DbContextOptionsBuilder opts)
	{
		EnsureDbCreated();
		opts.UseSqlite($"Data Source={dbPath};");
	}

	protected override void OnModelCreating(ModelBuilder mb)
	{
		mb.Entity<Trade>().ToTable("trades");
	}

	private void EnsureDbCreated()
	{
		if (File.Exists(dbPath))
		{
			return;
		}

		File.Create(dbPath).Close();
		Task.Delay(100).Wait();
	}
}
