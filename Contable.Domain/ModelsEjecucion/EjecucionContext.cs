namespace Contable.Domain.ModelsEjecucion;

/// <summary>
/// Contexto del esquema EJECUCION: asientos, plan de cuentas y matriz contable.
/// </summary>
public class EjecucionContext(DbContextOptions<EjecucionContext> options) : DbContext(options)
{
    public virtual DbSet<TAsiento> TAsientos { get; set; }

    public virtual DbSet<TAsientoDetalle> TAsientosDetalle { get; set; }

    public virtual DbSet<TTipoAsiento> TTiposAsiento { get; set; }

    public virtual DbSet<TCuentaContable> TCuentasContables { get; set; }

    public virtual DbSet<TMatrizContable> TMatrizContable { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("EJECUCION")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<TAsiento>(entity =>
        {
            entity.ToTable("T_ASIENTOS");
            entity.HasKey(e => e.IdAsiento);

            entity.Property(e => e.IdAsiento).HasColumnName("ID_ASIENTO").ValueGeneratedNever();
            entity.Property(e => e.Numero).HasColumnName("NUMERO");
            entity.Property(e => e.Anio).HasColumnName("ANIO");
            entity.Property(e => e.IdVigencia).HasColumnName("ID_VIGENCIA");
            entity.Property(e => e.IdJurisdiccion).HasColumnName("ID_JURISDICCION");
            entity.Property(e => e.Observaciones).HasColumnName("OBSERVACIONES").HasMaxLength(500);
            entity.Property(e => e.FecAlta).HasColumnName("FEC_ALTA");
            entity.Property(e => e.UserAlta).HasColumnName("USER_ALTA").HasMaxLength(500);
            entity.Property(e => e.FecAct).HasColumnName("FEC_ACT");
            entity.Property(e => e.UserAct).HasColumnName("USER_ACT").HasMaxLength(500);
            entity.Property(e => e.IdAsientoRel).HasColumnName("ID_ASIENTO_REL");
            entity.Property(e => e.IdDevengado).HasColumnName("ID_DEVENGADO");
            entity.Property(e => e.IdDietdac).HasColumnName("ID_DIETDAC");
            entity.Property(e => e.IdAnticipo).HasColumnName("ID_ANTICIPO");
            entity.Property(e => e.IdTipoAsiento).HasColumnName("ID_TIPO_ASIENTO");
            entity.Property(e => e.FecContable).HasColumnName("FEC_CONTABLE");
            entity.Property(e => e.Leyenda).HasColumnName("LEYENDA").HasMaxLength(4000);
            entity.Property(e => e.IdTransferencia).HasColumnName("ID_TRANSFERENCIA");
            entity.Property(e => e.IdMovTeso).HasColumnName("ID_MOV_TESO");
            entity.Property(e => e.IdLibroBanco).HasColumnName("ID_LIBRO_BANCO");
            entity.Property(e => e.IdBien).HasColumnName("ID_BIEN");
            entity.Property(e => e.IdAporte).HasColumnName("ID_APORTE");
            entity.Property(e => e.IdSello).HasColumnName("ID_SELLO");
            entity.Property(e => e.IdProveedor).HasColumnName("ID_PROVEEDOR");
            entity.Property(e => e.IdRur).HasColumnName("ID_RUR");

            entity.HasOne(e => e.TipoAsiento)
                .WithMany()
                .HasForeignKey(e => e.IdTipoAsiento)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.Detalles)
                .WithOne(d => d.Asiento)
                .HasForeignKey(d => d.IdAsiento)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => new { e.IdVigencia, e.FecContable });
            entity.HasIndex(e => e.IdJurisdiccion);
        });

        modelBuilder.Entity<TAsientoDetalle>(entity =>
        {
            entity.ToTable("T_ASIENTOS_DET");
            entity.HasKey(e => e.IdAsientoDet);

            entity.Property(e => e.IdAsientoDet).HasColumnName("ID_ASIENTO_DET").ValueGeneratedNever();
            entity.Property(e => e.IdAsiento).HasColumnName("ID_ASIENTO");
            entity.Property(e => e.IdCtaContable).HasColumnName("ID_CTA_CONTABLE");
            entity.Property(e => e.FecContable).HasColumnName("FEC_CONTABLE");
            entity.Property(e => e.Debe).HasColumnName("DEBE").HasPrecision(18, 2);
            entity.Property(e => e.Haber).HasColumnName("HABER").HasPrecision(18, 2);
            entity.Property(e => e.IdIngresosDet).HasColumnName("ID_INGRESOS_DET");
            entity.Property(e => e.IdDevengadoDet).HasColumnName("ID_DEVENGADODET");
            entity.Property(e => e.IdDietdacDet).HasColumnName("ID_DIETDAC_DET");
            entity.Property(e => e.IdMovTesoDet).HasColumnName("ID_MOV_TESO_DET");
            entity.Property(e => e.IdElementoPago).HasColumnName("ID_ELEMENTO_PAGO");
            entity.Property(e => e.UserIng).HasColumnName("USER_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UserAct).HasColumnName("USER_ACT").HasMaxLength(50);
            entity.Property(e => e.FecAct).HasColumnName("FEC_ACT");
            entity.Property(e => e.Anulado).HasColumnName("ANULADO").HasMaxLength(1);
            entity.Property(e => e.IdRecursoDh).HasColumnName("ID_RECURSO_D_H");
            entity.Property(e => e.IdComprobanteDeduccion).HasColumnName("ID_COMPROBANTE_DEDUCCION");
            entity.Property(e => e.IdRetencionLiquidacion).HasColumnName("ID_RETENCION_LIQUIDACION");
            entity.Property(e => e.IdAporteDet).HasColumnName("ID_APORTE_DET");
            entity.Property(e => e.IdCobro).HasColumnName("ID_COBRO");
            entity.Property(e => e.IdCatProg).HasColumnName("ID_CAT_PROG");
            entity.Property(e => e.IdObjetoGasto).HasColumnName("ID_OBJETO_GASTO");
            entity.Property(e => e.IdCentroCosto).HasColumnName("ID_CENTRO_COSTO");
            entity.Property(e => e.IdOficina).HasColumnName("ID_OFICINA");
            entity.Property(e => e.IdSelloDetalle).HasColumnName("ID_SELLO_DETALLE");
            entity.Property(e => e.IdObras).HasColumnName("ID_OBRAS");
            entity.Property(e => e.IdRurDet).HasColumnName("ID_RUR_DET");
            entity.Property(e => e.IdAuxRec).HasColumnName("ID_AUX_REC");
            entity.Property(e => e.IdAuxCtaBan).HasColumnName("ID_AUX_CTA_BAN");
            entity.Property(e => e.IdAuxOg).HasColumnName("ID_AUX_OG");
            entity.Property(e => e.IdCuentaBancaria).HasColumnName("ID_CUENTA_BANCARIA");
            entity.Property(e => e.IdLibroBanco).HasColumnName("ID_LIBRO_BANCO");

            entity.HasOne(e => e.CuentaContable)
                .WithMany()
                .HasForeignKey(e => e.IdCtaContable)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.IdCtaContable);
        });

        modelBuilder.Entity<TTipoAsiento>(entity =>
        {
            entity.ToTable("T_TIPO_ASIENTO");
            entity.HasKey(e => e.IdTipoAsiento);

            entity.Property(e => e.IdTipoAsiento).HasColumnName("ID_TIPO_ASIENTO").ValueGeneratedNever();
            entity.Property(e => e.Codigo).HasColumnName("CODIGO").HasMaxLength(10);
            entity.Property(e => e.Descripcion).HasColumnName("DESCRIPCION").HasMaxLength(50);
        });

        modelBuilder.Entity<TCuentaContable>(entity =>
        {
            entity.ToTable("T_CUENTAS_CONTABLES");
            entity.HasKey(e => e.IdCtaContable);

            entity.Property(e => e.IdCtaContable).HasColumnName("ID_CTA_CONTABLE").ValueGeneratedNever();
            entity.Property(e => e.IdCtaContableRel).HasColumnName("ID_CTA_CONTABLE_REL");
            entity.Property(e => e.Codigo).HasColumnName("CODIGO").HasMaxLength(50);
            entity.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(500);
            entity.Property(e => e.Activa).HasColumnName("ACTIVA").HasMaxLength(1);
            entity.Property(e => e.TipoSaldo).HasColumnName("TIPO_SALDO").HasMaxLength(1);
            entity.Property(e => e.Nivel).HasColumnName("NIVEL");
            entity.Property(e => e.UserIng).HasColumnName("USER_ING").HasMaxLength(20);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UserAct).HasColumnName("USER_ACT").HasMaxLength(20);
            entity.Property(e => e.FecAct).HasColumnName("FEC_ACT");
            entity.Property(e => e.IdNaturaleza).HasColumnName("ID_NATURALEZA");
            entity.Property(e => e.IdRecursoDebe).HasColumnName("ID_RECURSO_DEBE");
            entity.Property(e => e.IdRecursoHaber).HasColumnName("ID_RECURSO_HABER");
            entity.Property(e => e.Imputable).HasColumnName("IMPUTABLE").HasMaxLength(1);
            entity.Property(e => e.Auxiliar).HasColumnName("AUXILIAR").HasMaxLength(1);
            entity.Property(e => e.IdJurisdiccion).HasColumnName("ID_JURISDICCION");
            entity.Property(e => e.IdVigencia).HasColumnName("ID_VIGENCIA");
            entity.Property(e => e.AsientoOps).HasColumnName("ASIENTO_OPS").HasMaxLength(1);
            entity.Property(e => e.IdCtaBancaria).HasColumnName("ID_CTA_BANCARIA");
            entity.Property(e => e.Inventariable).HasColumnName("INVENTARIABLE").HasMaxLength(1);
            entity.Property(e => e.ClasificadorRelacionado).HasColumnName("CLASIFICADOR_RELACIONADO").HasMaxLength(50);
            entity.Property(e => e.IdProveedor).HasColumnName("ID_PROVEEDOR");
            entity.Property(e => e.Amortizacion).HasColumnName("AMORTIZACION");
            entity.Property(e => e.IngresoPropio).HasColumnName("INGRESO_PROPIO");

            entity.HasOne(e => e.Padre)
                .WithMany(p => p.Hijas)
                .HasForeignKey(e => e.IdCtaContableRel)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.Codigo);
            entity.HasIndex(e => e.IdCtaContableRel);
        });

        modelBuilder.Entity<TMatrizContable>(entity =>
        {
            entity.ToTable("T_MATRIZ_CONTABLE");
            entity.HasKey(e => e.IdMatrizCble);

            entity.Property(e => e.IdMatrizCble).HasColumnName("ID_MATRIZ_CBLE").ValueGeneratedNever();
            entity.Property(e => e.IdCtaContable).HasColumnName("ID_CTA_CONTABLE");
            entity.Property(e => e.IdObjetoGasto).HasColumnName("ID_OBJETO_GASTO");
            entity.Property(e => e.IdRecurso).HasColumnName("ID_RECURSO");
            entity.Property(e => e.IdVigencia).HasColumnName("ID_VIGENCIA");
            entity.Property(e => e.UserIng).HasColumnName("USER_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UserAct).HasColumnName("USER_ACT").HasMaxLength(50);
            entity.Property(e => e.FecAct).HasColumnName("FEC_ACT");

            entity.HasOne(e => e.CuentaContable)
                .WithMany()
                .HasForeignKey(e => e.IdCtaContable)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
