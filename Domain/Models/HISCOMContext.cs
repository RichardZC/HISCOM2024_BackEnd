using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Domain.Models
{
    public partial class HISCOMContext : DbContext
    {
        public HISCOMContext()
        {
        }

        public HISCOMContext(DbContextOptions<HISCOMContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Auditoria> Auditoria { get; set; }
        public virtual DbSet<Banco> Banco { get; set; }
        public virtual DbSet<Cargo> Cargo { get; set; }
        public virtual DbSet<Categoria> Categoria { get; set; }
        public virtual DbSet<CategoriaEmpleado> CategoriaEmpleado { get; set; }
        public virtual DbSet<Clasificacion> Clasificacion { get; set; }
        public virtual DbSet<ColegioProfesional> ColegioProfesional { get; set; }
        public virtual DbSet<CondicionLaboral> CondicionLaboral { get; set; }
        public virtual DbSet<Empleado> Empleado { get; set; }
        public virtual DbSet<EnlaceReset> EnlaceReset { get; set; }
        public virtual DbSet<Establecimiento> Establecimiento { get; set; }
        public virtual DbSet<EstadoCivil> EstadoCivil { get; set; }
        public virtual DbSet<ExamenClinico> ExamenClinico { get; set; }
        public virtual DbSet<Marcacion> Marcacion { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<Nacionalidad> Nacionalidad { get; set; }
        public virtual DbSet<Nivel> Nivel { get; set; }
        public virtual DbSet<Notificacion> Notificacion { get; set; }
        public virtual DbSet<Organigrama> Organigrama { get; set; }
        public virtual DbSet<Parametro> Parametro { get; set; }
        public virtual DbSet<Permiso> Permiso { get; set; }
        public virtual DbSet<PlhConcepto> PlhConcepto { get; set; }
        public virtual DbSet<PlhPlanilla> PlhPlanilla { get; set; }
        public virtual DbSet<PlhPlanillaConcepto> PlhPlanillaConcepto { get; set; }
        public virtual DbSet<Profesion> Profesion { get; set; }
        public virtual DbSet<RegimenLaboral> RegimenLaboral { get; set; }
        public virtual DbSet<Rol> Rol { get; set; }
        public virtual DbSet<RolPermiso> RolPermiso { get; set; }
        public virtual DbSet<RolTurno> RolTurno { get; set; }
        public virtual DbSet<RolTurnoAprobador> RolTurnoAprobador { get; set; }
        public virtual DbSet<RolTurnoDetalle> RolTurnoDetalle { get; set; }
        public virtual DbSet<RolTurnoEstab> RolTurnoEstab { get; set; }
        public virtual DbSet<RolTurnoIntento> RolTurnoIntento { get; set; }
        public virtual DbSet<RolTurnoRevision> RolTurnoRevision { get; set; }
        public virtual DbSet<TipoCuenta> TipoCuenta { get; set; }
        public virtual DbSet<TipoDocumento> TipoDocumento { get; set; }
        public virtual DbSet<TipoEmpleado> TipoEmpleado { get; set; }
        public virtual DbSet<Turno> Turno { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<UsuarioRol> UsuarioRol { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("name=connectionDB");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Modern_Spanish_CI_AS");

            modelBuilder.Entity<Auditoria>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Accion)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Controlador)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.DireccionIp)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false)
                    .HasColumnName("DireccionIP");

                entity.Property(e => e.Fecha).HasColumnType("datetime");

                entity.Property(e => e.Navegador)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Auditoria)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Auditoria__Usuar__6B24EA82");
            });

            modelBuilder.Entity<Banco>(entity =>
            {
                entity.ToTable("Banco", "rh");

                entity.Property(e => e.Abreviacion)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Cargo>(entity =>
            {
                entity.ToTable("Cargo", "rh");

                entity.Property(e => e.Id)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Clasificacion)
                    .WithMany(p => p.Cargo)
                    .HasForeignKey(d => d.ClasificacionId)
                    .HasConstraintName("FK__Cargo__Clasifica__6D0D32F4");
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categoria", "rh");

                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg).HasColumnType("datetime");

                entity.HasOne(d => d.Organigrama)
                    .WithMany(p => p.Categoria)
                    .HasForeignKey(d => d.OrganigramaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Categoria__Organ__6E01572D");
            });

            modelBuilder.Entity<CategoriaEmpleado>(entity =>
            {
                entity.ToTable("CategoriaEmpleado", "rh");

                entity.HasOne(d => d.Categoria)
                    .WithMany(p => p.CategoriaEmpleado)
                    .HasForeignKey(d => d.CategoriaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Categoria__Categ__6EF57B66");

                entity.HasOne(d => d.Empleado)
                    .WithMany(p => p.CategoriaEmpleado)
                    .HasForeignKey(d => d.EmpleadoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Categoria__Emple__03F0984C");
            });

            modelBuilder.Entity<Clasificacion>(entity =>
            {
                entity.ToTable("Clasificacion", "rh");

                entity.Property(e => e.Abreviatura)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<ColegioProfesional>(entity =>
            {
                entity.ToTable("ColegioProfesional", "rh");

                entity.Property(e => e.Decano)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Direccion)
                    .IsRequired()
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Foto).HasColumnType("text");

                entity.Property(e => e.SitioWeb).HasColumnType("text");

                entity.Property(e => e.Telefonos)
                    .HasMaxLength(32)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CondicionLaboral>(entity =>
            {
                entity.ToTable("CondicionLaboral", "rh");

                entity.Property(e => e.Id)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.ToTable("Empleado", "rh");

                entity.HasIndex(e => e.NumeroDoc, "UQ_nroDocumento")
                    .IsUnique();

                entity.Property(e => e.ApellidoMaterno)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.ApellidoPaterno)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.CargoId)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.CondicionLaboralId)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Correos)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.CuentaInterbancaria)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.Direccion)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.FechaIngreso).HasColumnType("date");

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaNombramiento).HasColumnType("date");

                entity.Property(e => e.FechaReg).HasColumnType("datetime");

                entity.Property(e => e.Nacimiento).HasColumnType("date");

                entity.Property(e => e.Nombres)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.NumeroColegiatura)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.NumeroCuenta)
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.NumeroDoc)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.ProfesionId)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.RegimenLaboralId)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Sexo)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.Telefonos)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.Banco)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.BancoId)
                    .HasConstraintName("FK__Empleado__BancoI__04E4BC85");

                entity.HasOne(d => d.Cargo)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.CargoId)
                    .HasConstraintName("FK__Empleado__CargoI__05D8E0BE");

                entity.HasOne(d => d.ColegioProfesional)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.ColegioProfesionalId)
                    .HasConstraintName("FK__Empleado__Colegi__06CD04F7");

                entity.HasOne(d => d.CondicionLaboral)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.CondicionLaboralId)
                    .HasConstraintName("FK__Empleado__Condic__07C12930");

                entity.HasOne(d => d.EstadoCivil)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.EstadoCivilId)
                    .HasConstraintName("FK__Empleado__Estado__08B54D69");

                entity.HasOne(d => d.Nacionalidad)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.NacionalidadId)
                    .HasConstraintName("FK__Empleado__Nacion__09A971A2");

                entity.HasOne(d => d.Organigrama)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.OrganigramaId)
                    .HasConstraintName("FK__Empleado__Organi__0A9D95DB");

                entity.HasOne(d => d.Profesion)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.ProfesionId)
                    .HasConstraintName("FK__Empleado__Profes__0B91BA14");

                entity.HasOne(d => d.RegimenLaboral)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.RegimenLaboralId)
                    .HasConstraintName("FK__Empleado__Regime__0C85DE4D");

                entity.HasOne(d => d.TipoCuenta)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.TipoCuentaId)
                    .HasConstraintName("FK__Empleado__TipoCu__0D7A0286");

                entity.HasOne(d => d.TipoDocumento)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.TipoDocumentoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Empleado__TipoDo__0E6E26BF");

                entity.HasOne(d => d.TipoEmpleado)
                    .WithMany(p => p.Empleado)
                    .HasForeignKey(d => d.TipoEmpleadoId)
                    .HasConstraintName("FK__Empleado__TipoEm__0F624AF8");
            });

            modelBuilder.Entity<EnlaceReset>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FechaFin).HasColumnType("datetime");

                entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            });

            modelBuilder.Entity<Establecimiento>(entity =>
            {
                entity.ToTable("Establecimiento", "rh");

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<EstadoCivil>(entity =>
            {
                entity.ToTable("EstadoCivil", "rh");

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ExamenClinico>(entity =>
            {
                entity.ToTable("ExamenClinico", "rh");

                entity.Property(e => e.CategoriaId)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DniPaciente)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.ExamenPdf)
                    .IsRequired()
                    .HasColumnType("text");
            });

            modelBuilder.Entity<Marcacion>(entity =>
            {
                entity.ToTable("Marcacion", "rh");

                entity.Property(e => e.Fecha).HasColumnType("datetime");

                entity.Property(e => e.NumeroDoc)
                    .IsRequired()
                    .HasMaxLength(24)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menu", "seguridad");

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Icono)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Nacionalidad>(entity =>
            {
                entity.ToTable("Nacionalidad", "rh");

                entity.Property(e => e.Abreviacion)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Gentilicio)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Pais)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Nivel>(entity =>
            {
                entity.ToTable("Nivel", "rh");

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.FechaReg).HasColumnType("datetime");
            });

            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.Property(e => e.FechaReg).HasColumnType("datetime");

                entity.Property(e => e.Icono)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Mensaje)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Ruta)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.HasOne(d => d.Empleado)
                    .WithMany(p => p.Notificacion)
                    .HasForeignKey(d => d.EmpleadoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Notificac__Emple__00200768");
            });

            modelBuilder.Entity<Organigrama>(entity =>
            {
                entity.ToTable("Organigrama", "rh");

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Nivel)
                    .WithMany(p => p.Organigrama)
                    .HasForeignKey(d => d.NivelId)
                    .HasConstraintName("FK__Organigra__Nivel__7C4F7684");

                entity.HasOne(d => d.Padre)
                    .WithMany(p => p.InversePadre)
                    .HasForeignKey(d => d.PadreId)
                    .HasConstraintName("FK__Organigra__Padre__7D439ABD");
            });

            modelBuilder.Entity<Parametro>(entity =>
            {
                entity.ToTable("Parametro", "maestro");

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Llave)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Valor)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.ToTable("Permiso", "seguridad");

                entity.Property(e => e.Accion)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Descripcion)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.Ruta)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.SubMenu)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.Permiso)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("Menu_Permiso_FK");
            });

            modelBuilder.Entity<PlhConcepto>(entity =>
            {
                entity.ToTable("PlhConcepto", "rh");

                entity.Property(e => e.Abreviado)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Codigo)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg).HasColumnType("datetime");
            });

            modelBuilder.Entity<PlhPlanilla>(entity =>
            {
                entity.ToTable("PlhPlanilla", "rh");

                entity.Property(e => e.Afpcar)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("AFPCAR");

                entity.Property(e => e.CodCar)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Codsiaf)
                    .HasMaxLength(32)
                    .IsUnicode(false)
                    .HasColumnName("CODSIAF");

                entity.Property(e => e.Condic).HasColumnName("CONDIC");

                entity.Property(e => e.Ctaban)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("CTABAN");

                entity.Property(e => e.Fecafp)
                    .HasColumnType("date")
                    .HasColumnName("FECAFP");

                entity.Property(e => e.Fecalt)
                    .HasColumnType("date")
                    .HasColumnName("FECALT");

                entity.Property(e => e.FechaNac).HasColumnType("date");

                entity.Property(e => e.Ipsscar)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("IPSSCAR");

                entity.Property(e => e.Libele)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Mat)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.Nom)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Pat)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.Plaza)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Regim)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Sexo)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<PlhPlanillaConcepto>(entity =>
            {
                entity.ToTable("PlhPlanillaConcepto", "rh");

                entity.Property(e => e.Saldo).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.PlhConcepto)
                    .WithMany(p => p.PlhPlanillaConcepto)
                    .HasForeignKey(d => d.PlhConceptoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PlhPlanil__PlhCo__7E37BEF6");

                entity.HasOne(d => d.PlhPlanilla)
                    .WithMany(p => p.PlhPlanillaConcepto)
                    .HasForeignKey(d => d.PlhPlanillaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PlhPlanil__PlhPl__1332DBDC");
            });

            modelBuilder.Entity<Profesion>(entity =>
            {
                entity.ToTable("Profesion", "rh");

                entity.Property(e => e.Id)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Abreviacion)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<RegimenLaboral>(entity =>
            {
                entity.ToTable("RegimenLaboral", "rh");

                entity.Property(e => e.Id)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol", "seguridad");

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<RolPermiso>(entity =>
            {
                entity.ToTable("RolPermiso", "seguridad");

                entity.HasOne(d => d.Permiso)
                    .WithMany(p => p.RolPermiso)
                    .HasForeignKey(d => d.PermisoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolPermis__Permi__0F624AF8");

                entity.HasOne(d => d.Rol)
                    .WithMany(p => p.RolPermiso)
                    .HasForeignKey(d => d.RolId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolPermis__RolId__10566F31");
            });

            modelBuilder.Entity<RolTurno>(entity =>
            {
                entity.ToTable("RolTurno", "rh");

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg).HasColumnType("datetime");

                entity.Property(e => e.Observacion).HasColumnType("text");

                entity.Property(e => e.TipoRolTurno)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.HasOne(d => d.Jefe)
                    .WithMany(p => p.RolTurno)
                    .HasForeignKey(d => d.JefeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurno__JefeId__14270015");

                entity.HasOne(d => d.Organigrama)
                    .WithMany(p => p.RolTurno)
                    .HasForeignKey(d => d.OrganigramaId)
                    .HasConstraintName("FK__RolTurno__Organi__01142BA1");
            });

            modelBuilder.Entity<RolTurnoAprobador>(entity =>
            {
                entity.ToTable("RolTurnoAprobador", "rh");

                entity.Property(e => e.TipoRolTurno)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.HasOne(d => d.Anterior)
                    .WithMany(p => p.InverseAnterior)
                    .HasForeignKey(d => d.AnteriorId)
                    .HasConstraintName("FK__RolTurnoA__Anter__02084FDA");

                entity.HasOne(d => d.Aprobador)
                    .WithMany(p => p.RolTurnoAprobador)
                    .HasForeignKey(d => d.AprobadorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoA__Aprob__02FC7413");

                entity.HasOne(d => d.Siguiente)
                    .WithMany(p => p.InverseSiguiente)
                    .HasForeignKey(d => d.SiguienteId)
                    .HasConstraintName("FK__RolTurnoA__Sigui__03F0984C");
            });

            modelBuilder.Entity<RolTurnoDetalle>(entity =>
            {
                entity.ToTable("RolTurnoDetalle", "rh");

                entity.HasOne(d => d.Empleado)
                    .WithMany(p => p.RolTurnoDetalle)
                    .HasForeignKey(d => d.EmpleadoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoD__Emple__18EBB532");

                entity.HasOne(d => d.RolTurnoEstab)
                    .WithMany(p => p.RolTurnoDetalle)
                    .HasForeignKey(d => d.RolTurnoEstabId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoD__RolTu__05D8E0BE");

                entity.HasOne(d => d.Turno)
                    .WithMany(p => p.RolTurnoDetalle)
                    .HasForeignKey(d => d.TurnoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoD__Turno__06CD04F7");
            });

            modelBuilder.Entity<RolTurnoEstab>(entity =>
            {
                entity.ToTable("RolTurnoEstab", "rh");

                entity.HasOne(d => d.Establecimiento)
                    .WithMany(p => p.RolTurnoEstab)
                    .HasForeignKey(d => d.EstablecimientoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoE__Estab__07C12930");

                entity.HasOne(d => d.RolTurno)
                    .WithMany(p => p.RolTurnoEstab)
                    .HasForeignKey(d => d.RolTurnoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoE__RolTu__08B54D69");
            });

            modelBuilder.Entity<RolTurnoIntento>(entity =>
            {
                entity.ToTable("RolTurnoIntento", "rh");

                entity.Property(e => e.FechaCierre).HasColumnType("datetime");

                entity.Property(e => e.FechaEnvio)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.RolTurno)
                    .WithMany(p => p.RolTurnoIntento)
                    .HasForeignKey(d => d.RolTurnoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoI__RolTu__09A971A2");

                entity.HasOne(d => d.SiguienteAprobador)
                    .WithMany(p => p.RolTurnoIntento)
                    .HasForeignKey(d => d.SiguienteAprobadorId)
                    .HasConstraintName("FK__RolTurnoI__Sigui__0A9D95DB");
            });

            modelBuilder.Entity<RolTurnoRevision>(entity =>
            {
                entity.ToTable("RolTurnoRevision", "rh");

                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Fecha).HasColumnType("datetime");

                entity.Property(e => e.Observacion).HasColumnType("text");

                entity.HasOne(d => d.RolTurnoAprobador)
                    .WithMany(p => p.RolTurnoRevision)
                    .HasForeignKey(d => d.RolTurnoAprobadorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoR__RolTu__0C85DE4D");

                entity.HasOne(d => d.RolTurnoIntento)
                    .WithMany(p => p.RolTurnoRevision)
                    .HasForeignKey(d => d.RolTurnoIntentoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoR__RolTu__0B91BA14");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.RolTurnoRevision)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RolTurnoR__Usuar__0D7A0286");
            });

            modelBuilder.Entity<TipoCuenta>(entity =>
            {
                entity.ToTable("TipoCuenta", "rh");

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TipoDocumento>(entity =>
            {
                entity.ToTable("TipoDocumento", "rh");

                entity.Property(e => e.Abreviatura)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TipoEmpleado>(entity =>
            {
                entity.ToTable("TipoEmpleado", "rh");

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Turno>(entity =>
            {
                entity.ToTable("Turno", "rh");

                entity.Property(e => e.Denominacion)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Descripcion)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario", "seguridad");

                entity.HasIndex(e => e.EmpleadoId, "UQ_empleadoId")
                    .IsUnique();

                entity.Property(e => e.Contrasena)
                    .IsRequired()
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.Correo)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FechaMod).HasColumnType("datetime");

                entity.Property(e => e.FechaReg).HasColumnType("datetime");

                entity.Property(e => e.Foto).HasColumnType("text");

                entity.Property(e => e.NombreUsuario)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Empleado)
                    .WithOne(p => p.Usuario)
                    .HasForeignKey<Usuario>(d => d.EmpleadoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Usuario__Emplead__25518C17");
            });

            modelBuilder.Entity<UsuarioRol>(entity =>
            {
                entity.ToTable("UsuarioRol", "seguridad");

                entity.HasOne(d => d.Rol)
                    .WithMany(p => p.UsuarioRol)
                    .HasForeignKey(d => d.RolId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("rol_usuariorol_FK");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.UsuarioRol)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UsuarioRo__Usuar__123EB7A3");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
