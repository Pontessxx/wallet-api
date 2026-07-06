namespace Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Carteira> Carteiras => Set<Carteira>();
        public DbSet<TransferenciaCarteira> TransferenciasCarteira => Set<TransferenciaCarteira>();
        public DbSet<TransacaoBolsa> TransacoesBolsa => Set<TransacaoBolsa>();

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

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Token).HasColumnName("token").IsRequired().HasMaxLength(64);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
                entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
                entity.Property(e => e.CreatedByIp).HasColumnName("created_by_ip").HasMaxLength(45);
                entity.Property(e => e.RevokedByIp).HasColumnName("revoked_by_ip").HasMaxLength(45);

                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ExpiresAt);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Carteira>(entity =>
            {
                entity.ToTable("wallets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Nome).HasColumnName("name").IsRequired().HasMaxLength(120);
                entity.Property(e => e.Categoria)
                    .HasColumnName("category")
                    .HasConversion<string>();
                entity.Property(e => e.SaldoInicial).HasColumnName("initial_balance").HasPrecision(18, 2);
                entity.Property(e => e.Receitas).HasColumnName("income").HasPrecision(18, 2);
                entity.Property(e => e.Despesas).HasColumnName("expenses").HasPrecision(18, 2);
                entity.Property(e => e.Transferencias).HasColumnName("transfers").HasPrecision(18, 2);
                entity.Property(e => e.Saldo).HasColumnName("balance").HasPrecision(18, 2);
                entity.Property(e => e.SaldoProjetado).HasColumnName("projected").HasPrecision(18, 2);

                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TransferenciaCarteira>(entity =>
            {
                entity.ToTable("wallet_transfers", table =>
                {
                    table.HasCheckConstraint("ck_wallet_transfers_amount_positive", "amount > 0");
                    table.HasCheckConstraint("ck_wallet_transfers_charges_non_negative", "charges >= 0");
                    table.HasCheckConstraint("ck_wallet_transfers_total_amount_consistent", "total_amount = amount + charges");
                    table.HasCheckConstraint("ck_wallet_transfers_different_wallets", "source_wallet_id <> destination_wallet_id");
                    table.HasCheckConstraint(
                        "ck_wallet_transfers_effective_date",
                        "(is_effective = false AND effective_at IS NULL) OR (is_effective = true AND effective_at IS NOT NULL AND effective_at >= posted_at)");
                });

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CarteiraOrigemId).HasColumnName("source_wallet_id");
                entity.Property(e => e.CarteiraDestinoId).HasColumnName("destination_wallet_id");
                entity.Property(e => e.Valor).HasColumnName("amount").HasPrecision(18, 2);
                entity.Property(e => e.Encargos).HasColumnName("charges").HasPrecision(18, 2).HasDefaultValue(0m);
                entity.Property(e => e.ValorTotal).HasColumnName("total_amount").HasPrecision(18, 2);
                entity.Property(e => e.Efetivada).HasColumnName("is_effective").HasDefaultValue(false);
                entity.Property(e => e.DataLancamento).HasColumnName("posted_at");
                entity.Property(e => e.DataVencimento).HasColumnName("due_date");
                entity.Property(e => e.DataEfetivacao).HasColumnName("effective_at");
                entity.Property(e => e.Observacoes).HasColumnName("notes").HasMaxLength(300);
                entity.Property(e => e.CriadaEm).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.AtualizadaEm).HasColumnName("updated_at");

                entity.HasIndex(e => e.CarteiraOrigemId);
                entity.HasIndex(e => e.CarteiraDestinoId);
                entity.HasIndex(e => e.DataLancamento);
                entity.HasIndex(e => e.DataVencimento);
                entity.HasIndex(e => e.Efetivada);

                entity.HasOne(e => e.CarteiraOrigem)
                    .WithMany(e => e.TransferenciasSaida)
                    .HasForeignKey(e => e.CarteiraOrigemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CarteiraDestino)
                    .WithMany(e => e.TransferenciasEntrada)
                    .HasForeignKey(e => e.CarteiraDestinoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TransacaoBolsa>(entity =>
            {
                entity.ToTable("stock_transactions", table =>
                {
                    table.HasCheckConstraint("ck_stock_transactions_quantity_positive", "quantity > 0");
                    table.HasCheckConstraint("ck_stock_transactions_unit_price_positive", "unit_price > 0");
                    table.HasCheckConstraint("ck_stock_transactions_amount_positive", "amount > 0");
                    table.HasCheckConstraint("ck_stock_transactions_charges_non_negative", "charges >= 0");
                    table.HasCheckConstraint(
                        "ck_stock_transactions_effective_date",
                        "(is_effective = false AND effective_at IS NULL) OR (is_effective = true AND effective_at IS NOT NULL AND effective_at >= posted_at)");
                });

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CarteiraId).HasColumnName("wallet_id");
                entity.Property(e => e.CodigoAtivo).HasColumnName("ticker").IsRequired().HasMaxLength(20);
                entity.Property(e => e.Lado)
                    .HasColumnName("side")
                    .HasConversion<string>();
                entity.Property(e => e.Quantidade).HasColumnName("quantity").HasPrecision(18, 8);
                entity.Property(e => e.PrecoUnitario).HasColumnName("unit_price").HasPrecision(18, 8);
                entity.Property(e => e.Valor).HasColumnName("amount").HasPrecision(18, 2);
                entity.Property(e => e.Encargos).HasColumnName("charges").HasPrecision(18, 2).HasDefaultValue(0m);
                entity.Property(e => e.ValorTotal).HasColumnName("total_amount").HasPrecision(18, 2);
                entity.Property(e => e.Efetivada).HasColumnName("is_effective").HasDefaultValue(false);
                entity.Property(e => e.DataLancamento).HasColumnName("posted_at");
                entity.Property(e => e.DataVencimento).HasColumnName("due_date");
                entity.Property(e => e.DataEfetivacao).HasColumnName("effective_at");
                entity.Property(e => e.Observacoes).HasColumnName("notes").HasMaxLength(300);
                entity.Property(e => e.CriadaEm).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.AtualizadaEm).HasColumnName("updated_at");

                entity.HasIndex(e => e.CarteiraId);
                entity.HasIndex(e => e.DataLancamento);
                entity.HasIndex(e => e.DataVencimento);
                entity.HasIndex(e => e.Efetivada);
                entity.HasIndex(e => e.CodigoAtivo);

                entity.HasOne(e => e.Carteira)
                    .WithMany(e => e.TransacoesBolsa)
                    .HasForeignKey(e => e.CarteiraId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}