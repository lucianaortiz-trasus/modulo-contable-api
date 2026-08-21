namespace Contable.Domain.ModelsPortal;

/// <summary>
/// Contexto del esquema PORTAL: usuarios, personas y roles del portal SIGEMyT.
/// </summary>
/// <remarks>
/// Solo lectura. El modulo lee la identidad que ya resolvio el portal y emite su propio token.
/// </remarks>
public class PortalContext(DbContextOptions<PortalContext> options) : DbContext(options)
{
    public virtual DbSet<TUsuarioNew> TUsuariosNew { get; set; }

    public virtual DbSet<TPersona> TPersonas { get; set; }

    public virtual DbSet<RolNew> RolesNew { get; set; }

    public virtual DbSet<RolUsuarioNew> RolesUsuariosNew { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("PORTAL")
            .UseCollation("USING_NLS_COMP");

        // Las entidades auditables descartan las filas dadas de baja en toda consulta.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntityAuditable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(FiltroDeBaja(entityType.ClrType));
            }
        }

        modelBuilder.Entity<TUsuarioNew>(entity =>
        {
            entity.ToTable("T_USUARIOS_NEW");
            entity.HasKey(e => e.UsuarioId);

            entity.Property(e => e.UsuarioId).HasColumnName("USUARIO_ID").HasPrecision(10).ValueGeneratedNever();
            entity.Property(e => e.NombreUsuario).HasColumnName("NOMBRE_USUARIO").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Contrasena).HasColumnName("CONTRASENA").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Estado).HasColumnName("ESTADO").HasPrecision(4);
            entity.Property(e => e.ProveedorId).HasColumnName("PROVEEDOR_ID");
            entity.Property(e => e.JurisdiccionId).HasColumnName("JURISDICCION_ID");
            entity.Property(e => e.IdPersona).HasColumnName("ID_PERSONA").HasMaxLength(10).IsUnicode(false);

            entity.HasOne(e => e.Persona)
                .WithMany()
                .HasForeignKey(e => e.IdPersona)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TPersona>(entity =>
        {
            entity.ToTable("T_PERSONAS");
            entity.HasKey(e => e.IdPersona);

            entity.Property(e => e.IdPersona).HasColumnName("ID_PERSONA").HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Nombre).HasColumnName("NOMBRE");
            entity.Property(e => e.Apellido).HasColumnName("APELLIDO");
            entity.Property(e => e.DocumentoNro).HasColumnName("DOCUMENTO_NRO");
            entity.Property(e => e.Cuil).HasColumnName("CUIL");
            entity.Property(e => e.Email).HasColumnName("EMAIL");
            entity.Property(e => e.IdJurisdiccion).HasColumnName("ID_JURISDICCION");
            entity.Property(e => e.UsrIng).HasColumnName("USR_ING");
            entity.Property(e => e.FecIng).HasColumnName("FEC_ING");
            entity.Property(e => e.UsrMod).HasColumnName("USR_MOD");
            entity.Property(e => e.FecMod).HasColumnName("FEC_MOD");
            entity.Property(e => e.UserBaja).HasColumnName("USR_BAJA");
            entity.Property(e => e.FecBaja).HasColumnName("FEC_BAJA");
        });

        modelBuilder.Entity<RolNew>(entity =>
        {
            entity.ToTable("ROLES_NEW");
            entity.HasKey(e => e.RolId);

            entity.Property(e => e.RolId).HasColumnName("ROL_ID").ValueGeneratedNever();
            entity.Property(e => e.Nombre).HasColumnName("NOMBRE").HasMaxLength(100).IsRequired();
            entity.Property(e => e.AccessoId).HasColumnName("ACCESSO_ID");
        });

        modelBuilder.Entity<RolUsuarioNew>(entity =>
        {
            entity.ToTable("ROLES_USUARIOS_NEW");
            entity.HasKey(e => e.RolUsuarioId);

            entity.Property(e => e.RolUsuarioId).HasColumnName("ROL_USUARIO_ID").ValueGeneratedNever();
            entity.Property(e => e.RolId).HasColumnName("ROL_ID");
            entity.Property(e => e.UsuarioId).HasColumnName("USUARIO_ID");

            entity.HasOne(e => e.Rol)
                .WithMany(r => r.RolesUsuarios)
                .HasForeignKey(e => e.RolId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    /// <summary>
    /// Arma la expresion <c>x =&gt; x.FecBaja == null</c> para el tipo indicado.
    /// </summary>
    private static LambdaExpression FiltroDeBaja(Type tipo)
    {
        var parametro = Expression.Parameter(tipo, "x");
        var propiedad = Expression.Property(parametro, nameof(IEntityAuditable.FecBaja));
        var comparacion = Expression.Equal(propiedad, Expression.Constant(null, typeof(DateTime?)));

        return Expression.Lambda(comparacion, parametro);
    }
}
