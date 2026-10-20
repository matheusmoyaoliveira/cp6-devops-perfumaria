using Microsoft.EntityFrameworkCore;
using Perfumaria.API.Dominio.Entidades;

namespace Perfumaria.API.Infraestrutura.Dados;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Perfume> Perfumes => Set<Perfume>();
    public DbSet<NotaOlfativa> NotasOlfativas => Set<NotaOlfativa>();
    public DbSet<PerfumeNota> PerfumeNotas => Set<PerfumeNota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Perfume>(entity =>
        {
            entity.ToTable("T_PRF_PERFUMES");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("ID");
            entity.Property(p => p.Nome).HasColumnName("NOME").HasMaxLength(150).IsRequired();
            entity.Property(p => p.Marca).HasColumnName("MARCA").HasMaxLength(100).IsRequired();
            entity.Property(p => p.Genero).HasColumnName("GENERO").HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.VolumeMl).HasColumnName("VOLUME_ML");
            entity.Property(p => p.Preco).HasColumnName("PRECO").HasColumnType("NUMBER(10,2)");
            entity.Property(p => p.AnoLancamento).HasColumnName("ANO_LANCAMENTO");
        });

        modelBuilder.Entity<NotaOlfativa>(entity =>
        {
            entity.ToTable("T_PRF_NOTAS_OLFATIVAS");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Id).HasColumnName("ID");
            entity.Property(n => n.Nome).HasColumnName("NOME").HasMaxLength(100).IsRequired();
            entity.Property(n => n.Familia).HasColumnName("FAMILIA").HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<PerfumeNota>(entity =>
        {
            entity.ToTable("T_PRF_PERFUME_NOTA");
            entity.HasKey(pn => new { pn.PerfumeId, pn.NotaOlfativaId }); // chave composta

            entity.Property(pn => pn.PerfumeId).HasColumnName("PERFUME_ID");
            entity.Property(pn => pn.NotaOlfativaId).HasColumnName("NOTA_OLFATIVA_ID");
            entity.Property(pn => pn.Posicao).HasColumnName("POSICAO").HasConversion<string>().HasMaxLength(20);

            entity.HasOne(pn => pn.Perfume)
                  .WithMany(p => p.Notas)
                  .HasForeignKey(pn => pn.PerfumeId);

            entity.HasOne(pn => pn.NotaOlfativa)
                  .WithMany(n => n.Perfumes)
                  .HasForeignKey(pn => pn.NotaOlfativaId);
        });
    }
}