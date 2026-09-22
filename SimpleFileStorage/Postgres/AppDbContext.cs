using Microsoft.EntityFrameworkCore;
using SimpleFileStorage.Model;

namespace SimpleFileStorage.Postgres
{
    public class AppDbContext : DbContext
    {
        public DbSet<FileMetadata> Metadatas { get; set; }

        [Obsolete]
        public DbSet<FileData> Files { get; set; }

        private readonly IConfiguration _config;

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
        {
            _config = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // TODO: прочитать про env vars (environment variables - переменные окружения)
            string dbConnectionProfile = Environment.GetEnvironmentVariable("DB_CONNECTION_PROFILE") ?? "default";
            string? connectionString = _config.GetConnectionString(dbConnectionProfile);
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // настройка сущности FileMetadata
            modelBuilder.Entity<FileMetadata>(entity =>
            {
                // FileID как первичный ключ без автогенерации (EF fluent API)
                entity.HasKey(e => e.FileID);
                entity.Property(e => e.FileID).ValueGeneratedNever(); // отключить автозаполнение
            });

            // настройка сущности FileData
            modelBuilder.Entity<FileData>(entity =>
            {
                // FileID как первичный ключ без автогенерации (EF fluent API)
                entity.HasKey(e => e.FileID);
                entity.Property(e => e.FileID).ValueGeneratedNever(); // отключить автозаполнение
            });
        }
    }
}
