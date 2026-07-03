namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<WalletAccount> WalletAccounts => Set<WalletAccount>();
        public DbSet<Wallet> Wallets => Set<Wallet>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Username).HasColumnName("username").IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
                entity.Property(e => e.ResetCodeHash).HasColumnName("reset_code_hash");
                entity.Property(e => e.ResetCodeExpiresAt).HasColumnName("reset_code_expires_at");
                entity.Property(e => e.ResetCodeFailedAttempts).HasColumnName("reset_code_failed_attempts");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .HasConversion<string>();
            });

            modelBuilder.Entity<WalletAccount>(entity =>
            {
                entity.ToTable("wallet_accounts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(120);
                entity.Property(e => e.Category)
                    .HasColumnName("category")
                    .HasConversion<string>();
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Wallet)
                    .WithOne(e => e.WalletAccount)
                    .HasForeignKey<Wallet>(e => e.WalletAccountId);
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("wallets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.WalletAccountId).HasColumnName("wallet_account_id");
                entity.Property(e => e.Description).HasColumnName("description").IsRequired().HasMaxLength(200);
                entity.Property(e => e.InitialBalance).HasColumnName("initial_balance").HasPrecision(18, 2);
                entity.Property(e => e.Income).HasColumnName("income").HasPrecision(18, 2);
                entity.Property(e => e.Expenses).HasColumnName("expenses").HasPrecision(18, 2);
                entity.Property(e => e.Transfers).HasColumnName("transfers").HasPrecision(18, 2);
                entity.Property(e => e.Balance).HasColumnName("balance").HasPrecision(18, 2);
                entity.Property(e => e.Projected).HasColumnName("projected").HasPrecision(18, 2);
                entity.HasIndex(e => e.WalletAccountId).IsUnique();
            });
        }
    }
}