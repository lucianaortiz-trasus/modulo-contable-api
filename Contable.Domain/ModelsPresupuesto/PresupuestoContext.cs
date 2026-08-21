namespace Contable.Domain.ModelsPresupuesto;

/// <summary>
/// Contexto del esquema PRESUPUESTO: vigencias, jurisdicciones y unidades administrativas.
/// </summary>
/// <remarks>
/// Este modulo solo lee de PRESUPUESTO. Vigencias, ejercicios y jurisdicciones no se escriben
/// nunca desde aca.
/// </remarks>
public class PresupuestoContext(DbContextOptions<PresupuestoContext> options) : DbContext(options)
{
    public virtual DbSet<TVigencia> TVigencias { get; set; }

    public virtual DbSet<TJurisdiccion> TJurisdicciones { get; set; }

    public virtual DbSet<TUnidadAdministrativa> TUnidadesAdministrativa { get; set; }

    public virtual DbSet<TObjetoGasto> TObjetosGasto { get; set; }

    public virtual DbSet<TRecurso> TRecursos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("PRESUPUESTO")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<TVigencia>(entity =>
        {
            entity.ToTable("T_VIGENCIAS");
            entity.HasKey(e => e.IdVigencia);

            entity.Property(e => e.IdVigencia).HasColumnName("ID_VIGENCIA").ValueGeneratedNever();
            entity.Property(e => e.Ejercicio).HasColumnName("EJERCICIO");
            entity.Property(e => e.ActivoFormulacion).HasColumnName("ACTIVO_FORMULACION");
            entity.Property(e => e.ActivoEjecucion).HasColumnName("ACTIVO_EJECUCION");
            entity.Property(e => e.MaximoNivelCp).HasColumnName("MAXIMO_NIVEL_CP");
            entity.Property(e => e.UserIdIng).HasColumnName("USERID_ING");
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UserIdAct).HasColumnName("USERID_ACT");
            entity.Property(e => e.FecAct).HasColumnName("FEC_ACT");
            entity.Property(e => e.ProcesoCierre).HasColumnName("PROCESO_CIERRE");
        });

        modelBuilder.Entity<TJurisdiccion>(entity =>
        {
            entity.ToTable("T_JURISDICCIONES");
            entity.HasKey(e => e.IdJurisdiccion);

            entity.Property(e => e.IdJurisdiccion).HasColumnName("ID_JURISDICCION").ValueGeneratedNever();
            entity.Property(e => e.NumeroJurisdiccion).HasColumnName("NUMERO_JURISDICCION");
            entity.Property(e => e.NombreJurisdiccion).HasColumnName("NOMBRE_JURISDICCION");
            entity.Property(e => e.IdVigencia).HasColumnName("ID_VIGENCIA");
            entity.Property(e => e.EntId).HasColumnName("ENT_ID");
            entity.Property(e => e.IdSecIns).HasColumnName("ID_SEC_INS");
            entity.Property(e => e.Sigla).HasColumnName("SIGLA");
            entity.Property(e => e.UserIdIng).HasColumnName("USERID_ING");
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UserIdAct).HasColumnName("USERID_ACT");
            entity.Property(e => e.FecAct).HasColumnName("FEC_ACT");
            entity.Property(e => e.Descripcion).HasColumnName("DESCRIPCION");
            entity.Property(e => e.Notas).HasColumnName("NOTAS");
            entity.Property(e => e.IdGrupoSector).HasColumnName("ID_GRUPO_SECTOR");
        });

        modelBuilder.Entity<TUnidadAdministrativa>(entity =>
        {
            entity.ToTable("T_UNIDADES_ADMINISTRATIVA");
            entity.HasKey(e => e.IdUnidadAdm);

            entity.Property(e => e.IdUnidadAdm).HasColumnName("ID_UNIDAD_ADM").ValueGeneratedNever();
            entity.Property(e => e.NumeroUnidadAdm).HasColumnName("NUMERO_UNIDAD_ADM");
            entity.Property(e => e.NombreUnidadAdm).HasColumnName("NOMBRE_UNIDAD_ADM");
            entity.Property(e => e.IdVigencia).HasColumnName("ID_VIGENCIA");
            entity.Property(e => e.IdJurisdiccion).HasColumnName("ID_JURISDICCION");
            entity.Property(e => e.UserIdIng).HasColumnName("USERID_ING");
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UserIdAct).HasColumnName("USERID_ACT");
            entity.Property(e => e.FecAct).HasColumnName("FEC_ACT");
            entity.Property(e => e.Estado).HasColumnName("ESTADO");
            entity.Property(e => e.NroServicioAdm).HasColumnName("NRO_SERVICIO_ADM");
            entity.Property(e => e.IdProveedor).HasColumnName("ID_PROVEEDOR");

            entity.Ignore(e => e.Archivo);
        });

        modelBuilder.Entity<TObjetoGasto>(entity =>
        {
            entity.ToTable("T_OBJETOS_GASTO");
            entity.HasKey(e => e.IdObjetoGasto);

            entity.Property(e => e.IdObjetoGasto).HasColumnName("ID_OBJETO_GASTO").ValueGeneratedNever();
            entity.Property(e => e.IdObjetoGastoRel).HasColumnName("ID_OBJETO_GASTO_REL");
            entity.Property(e => e.NumeroObjeto).HasColumnName("NUMERO_OBJETO");
            entity.Property(e => e.NombreObjeto).HasColumnName("NOMBRE_OBJETO").HasMaxLength(200);
            entity.Property(e => e.IdVigencia).HasColumnName("ID_VIGENCIA");
            entity.Property(e => e.IdJurisdiccion).HasColumnName("ID_JURISDICCION");
            entity.Property(e => e.UltimoNivel).HasColumnName("ULTIMO_NIVEL").HasMaxLength(1);
            entity.Property(e => e.ImputaEjecucion).HasColumnName("IMPUTA_EJECUCION").HasMaxLength(1);

            entity.HasIndex(e => new { e.IdVigencia, e.NumeroObjeto });
        });

        modelBuilder.Entity<TRecurso>(entity =>
        {
            entity.ToTable("T_RECURSOS");
            entity.HasKey(e => e.IdRecurso);

            entity.Property(e => e.IdRecurso).HasColumnName("ID_RECURSO").ValueGeneratedNever();
            entity.Property(e => e.IdRecursoRel).HasColumnName("ID_RECURSO_REL");
            entity.Property(e => e.NumeroRecurso).HasColumnName("NUMERO_RECURSO");
            entity.Property(e => e.NombreRecurso).HasColumnName("NOMBRE_RECURSO").HasMaxLength(200);
            entity.Property(e => e.IdVigencia).HasColumnName("ID_VIGENCIA");
            entity.Property(e => e.IdJurisdiccion).HasColumnName("ID_JURISDICCION");

            entity.HasIndex(e => new { e.IdVigencia, e.NumeroRecurso });
        });
    }
}
