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

    public virtual DbSet<TPlantillaAsiento> TPlantillasAsiento { get; set; }

    public virtual DbSet<TPlantillaAsientoDetalle> TPlantillasAsientoDetalle { get; set; }

    public virtual DbSet<TModeloAsiento> TModelosAsiento { get; set; }

    public virtual DbSet<TModeloAsientoDetalle> TModelosAsientoDetalle { get; set; }

    public virtual DbSet<THistorialModeloAsiento> THistorialesModelosAsiento { get; set; }

    public virtual DbSet<TTipoOperacionAsiento> TTiposOperacionAsiento { get; set; }

    public virtual DbSet<TTipoDocumento> TTiposDocumento { get; set; }

    public virtual DbSet<TTipoDocumentoLetra> TTiposDocumentoLetra { get; set; }

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

        modelBuilder.Entity<TTipoOperacionAsiento>(entity =>
        {
            entity.ToTable("T_TIPOS_OPERACION_ASIENTO");
            entity.HasKey(e => e.IdTipoOperacion);

            entity.Property(e => e.IdTipoOperacion).HasColumnName("ID_TIPO_OPERACION").ValueGeneratedNever();
            entity.Property(e => e.Codigo).HasColumnName("CODIGO").HasMaxLength(30);
            entity.Property(e => e.NTipoOperacion).HasColumnName("N_TIPO_OPERACION").HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasColumnName("DESCRIPCION").HasMaxLength(200);
            entity.Property(e => e.Orden).HasColumnName("ORDEN");
            entity.Property(e => e.UsrIng).HasColumnName("USR_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UsrMod).HasColumnName("USR_MOD").HasMaxLength(50);
            entity.Property(e => e.FecMod).HasColumnName("FEC_MOD");
            entity.Property(e => e.UsrBaja).HasColumnName("USR_BAJA").HasMaxLength(50);
            entity.Property(e => e.FecBaja).HasColumnName("FEC_BAJA");

            entity.HasIndex(e => e.FecBaja);
        });

        modelBuilder.Entity<TTipoDocumento>(entity =>
        {
            entity.ToTable("T_TIPODOC");
            entity.HasKey(e => e.IdTipodoc);

            entity.Property(e => e.IdTipodoc).HasColumnName("ID_TIPODOC").ValueGeneratedNever();
            entity.Property(e => e.Codigo).HasColumnName("CODIGO");
            entity.Property(e => e.Descripcion).HasColumnName("DESCRIPCION").HasMaxLength(100);
            entity.Property(e => e.Baja).HasColumnName("BAJA").HasMaxLength(1);
            entity.Property(e => e.IdTipoComprobante).HasColumnName("ID_TIPO_COMPROBANTE");
            entity.Property(e => e.FechaHasta).HasColumnName("FECHA_HASTA");
            entity.Property(e => e.IdEtapa).HasColumnName("ID_ETAPA");
            entity.Property(e => e.UsrAlta).HasColumnName("USR_ALTA").HasMaxLength(50);
            entity.Property(e => e.FecAlta).HasColumnName("FEC_ALTA");
            entity.Property(e => e.UsrModif).HasColumnName("USR_MODIF").HasMaxLength(50);
            entity.Property(e => e.FecModif).HasColumnName("FEC_MODIF");
        });

        modelBuilder.Entity<TTipoDocumentoLetra>(entity =>
        {
            entity.ToTable("T_TIPODOC_LETRA");
            entity.HasKey(e => e.IdTipodocLetra);

            entity.Property(e => e.IdTipodocLetra).HasColumnName("ID_TIPODOC_LETRA").ValueGeneratedNever();
            entity.Property(e => e.IdTipoDoc).HasColumnName("ID_TIPO_DOC");
            entity.Property(e => e.Codigo).HasColumnName("CODIGO").HasMaxLength(1);
            entity.Property(e => e.Activo).HasColumnName("ACTIVO");
            entity.Property(e => e.FecBaja).HasColumnName("FEC_BAJA");
            entity.Property(e => e.UsrBaja).HasColumnName("USR_BAJA").HasMaxLength(50);
            entity.Property(e => e.UsrIng).HasColumnName("USR_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UsrMod).HasColumnName("USR_MOD").HasMaxLength(50);
            entity.Property(e => e.FecMod).HasColumnName("FEC_MOD");

            entity.HasOne(e => e.Documento)
                .WithMany(d => d.Letras)
                .HasForeignKey(e => e.IdTipoDoc)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TPlantillaAsiento>(entity =>
        {
            entity.ToTable("T_PLANTILLAS_ASIENTO");
            entity.HasKey(e => e.IdPlantillaAsiento);

            entity.Property(e => e.IdPlantillaAsiento).HasColumnName("ID_PLANTILLA_ASIENTO").ValueGeneratedNever();
            entity.Property(e => e.IdTipoOperacion).HasColumnName("ID_TIPO_OPERACION");
            entity.Property(e => e.IdTipodoc).HasColumnName("ID_TIPODOC");
            entity.Property(e => e.IdTipodocLetra).HasColumnName("ID_TIPODOC_LETRA");
            entity.Property(e => e.DiscriminaOt).HasColumnName("DISCRIMINA_OT").HasMaxLength(1);
            entity.Property(e => e.UsrIng).HasColumnName("USR_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UsrMod).HasColumnName("USR_MOD").HasMaxLength(50);
            entity.Property(e => e.FecMod).HasColumnName("FEC_MOD");
            entity.Property(e => e.UsrBaja).HasColumnName("USR_BAJA").HasMaxLength(50);
            entity.Property(e => e.FecBaja).HasColumnName("FEC_BAJA");

            entity.HasOne(e => e.TipoOperacion)
                .WithMany(o => o.Plantillas)
                .HasForeignKey(e => e.IdTipoOperacion)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.TipoDocumento)
                .WithMany()
                .HasForeignKey(e => e.IdTipodoc)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.TipoDocumentoLetra)
                .WithMany()
                .HasForeignKey(e => e.IdTipodocLetra)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.Renglones)
                .WithOne(d => d.Plantilla)
                .HasForeignKey(d => d.IdPlantillaAsiento)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.IdTipoOperacion);
            entity.HasIndex(e => e.IdTipodoc);
            entity.HasIndex(e => e.IdTipodocLetra);
        });

        modelBuilder.Entity<TPlantillaAsientoDetalle>(entity =>
        {
            entity.ToTable("T_PLANTILLAS_ASIENTO_DET");
            entity.HasKey(e => e.IdPlantillaAsientoDetalle);

            entity.Property(e => e.IdPlantillaAsientoDetalle).HasColumnName("ID_PLANTILLA_ASIENTO_DET").ValueGeneratedNever();
            entity.Property(e => e.IdPlantillaAsiento).HasColumnName("ID_PLANTILLA_ASIENTO");
            entity.Property(e => e.Orden).HasColumnName("ORDEN");
            entity.Property(e => e.OrigenCuenta).HasColumnName("ORIGEN_CUENTA").HasMaxLength(20);
            entity.Property(e => e.Concepto).HasColumnName("CONCEPTO").HasMaxLength(200);
            entity.Property(e => e.Debita).HasColumnName("DEBITA").HasMaxLength(1);
            entity.Property(e => e.Acredita).HasColumnName("ACREDITA").HasMaxLength(1);
            entity.Property(e => e.BaseCalculo).HasColumnName("BASE_CALCULO").HasMaxLength(200);
            entity.Property(e => e.UsrIng).HasColumnName("USR_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UsrMod).HasColumnName("USR_MOD").HasMaxLength(50);
            entity.Property(e => e.FecMod).HasColumnName("FEC_MOD");
            entity.Property(e => e.UsrBaja).HasColumnName("USR_BAJA").HasMaxLength(50);
            entity.Property(e => e.FecBaja).HasColumnName("FEC_BAJA");

            entity.HasIndex(e => new { e.IdPlantillaAsiento, e.Orden }).IsUnique();
        });

        modelBuilder.Entity<TModeloAsiento>(entity =>
        {
            entity.ToTable("T_MODELOS_ASIENTO");
            entity.HasKey(e => e.IdModeloAsiento);

            entity.Property(e => e.IdModeloAsiento).HasColumnName("ID_MODELO_ASIENTO").ValueGeneratedNever();
            entity.Property(e => e.IdPlantillaAsiento).HasColumnName("ID_PLANTILLA_ASIENTO");
            entity.Property(e => e.UsrIng).HasColumnName("USR_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UsrMod).HasColumnName("USR_MOD").HasMaxLength(50);
            entity.Property(e => e.FecMod).HasColumnName("FEC_MOD");
            entity.Property(e => e.UsrBaja).HasColumnName("USR_BAJA").HasMaxLength(50);
            entity.Property(e => e.FecBaja).HasColumnName("FEC_BAJA");

            entity.HasOne(e => e.Plantilla)
                .WithMany()
                .HasForeignKey(e => e.IdPlantillaAsiento)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.Detalles)
                .WithOne(d => d.Modelo)
                .HasForeignKey(d => d.IdModeloAsiento)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.Historial)
                .WithOne(h => h.Modelo)
                .HasForeignKey(h => h.IdModeloAsiento)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => e.IdPlantillaAsiento);
        });

        modelBuilder.Entity<TModeloAsientoDetalle>(entity =>
        {
            entity.ToTable("T_MODELOS_ASIENTO_DET");
            entity.HasKey(e => e.IdModeloAsientoDetalle);

            entity.Property(e => e.IdModeloAsientoDetalle).HasColumnName("ID_MODELO_ASIENTO_DET").ValueGeneratedNever();
            entity.Property(e => e.IdModeloAsiento).HasColumnName("ID_MODELO_ASIENTO");
            entity.Property(e => e.IdPlantillaAsientoDetalle).HasColumnName("ID_PLANTILLA_ASIENTO_DET");
            entity.Property(e => e.IdCtaContable).HasColumnName("ID_CTA_CONTABLE");
            entity.Property(e => e.UsrIng).HasColumnName("USR_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UsrMod).HasColumnName("USR_MOD").HasMaxLength(50);
            entity.Property(e => e.FecMod).HasColumnName("FEC_MOD");
            entity.Property(e => e.UsrBaja).HasColumnName("USR_BAJA").HasMaxLength(50);
            entity.Property(e => e.FecBaja).HasColumnName("FEC_BAJA");

            entity.HasOne(e => e.RenglonPlantilla)
                .WithMany()
                .HasForeignKey(e => e.IdPlantillaAsientoDetalle)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.CuentaContable)
                .WithMany()
                .HasForeignKey(e => e.IdCtaContable)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasIndex(e => new { e.IdModeloAsiento, e.IdPlantillaAsientoDetalle }).IsUnique();
            entity.HasIndex(e => e.IdPlantillaAsientoDetalle);
            entity.HasIndex(e => e.IdCtaContable);
        });

        modelBuilder.Entity<THistorialModeloAsiento>(entity =>
        {
            entity.ToTable("T_HISTORIALES_MODELOS_ASIENTO");
            entity.HasKey(e => e.IdHistorialModeloAsiento);

            entity.Property(e => e.IdHistorialModeloAsiento).HasColumnName("ID_HISTORIAL_MODELO_ASIENTO").ValueGeneratedNever();
            entity.Property(e => e.IdModeloAsiento).HasColumnName("ID_MODELO_ASIENTO");
            entity.Property(e => e.Accion).HasColumnName("ACCION").HasMaxLength(20);
            entity.Property(e => e.Comentarios).HasColumnName("COMENTARIOS").HasColumnType("VARCHAR2(4000)");
            entity.Property(e => e.UsrIng).HasColumnName("USR_ING").HasMaxLength(50);
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UsrMod).HasColumnName("USR_MOD").HasMaxLength(50);
            entity.Property(e => e.FecMod).HasColumnName("FEC_MOD");
            entity.Property(e => e.UsrBaja).HasColumnName("USR_BAJA").HasMaxLength(50);
            entity.Property(e => e.FecBaja).HasColumnName("FEC_BAJA");

            entity.HasIndex(e => e.IdModeloAsiento);
        });
    }
}
