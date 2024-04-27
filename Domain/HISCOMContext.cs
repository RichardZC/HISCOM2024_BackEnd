using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public partial class HISCOMContext
    {
        public virtual DbSet<UspConsultarCita> ConsultarCita { get; set; }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Empleado>().HasQueryFilter(e => e.Estado);
            //modelBuilder.Entity<Usuario>().HasQueryFilter(e => e.Estado);
            modelBuilder.Entity<Notificacion>().HasQueryFilter(e => e.Estado);
            modelBuilder.Entity<RolTurno>().HasQueryFilter(e => true);
            modelBuilder.Entity<RolTurnoEstab>().HasQueryFilter(e => true);
            modelBuilder.Entity<RolTurnoIntento>().HasQueryFilter(e => true);
            modelBuilder.Entity<RolTurnoRevision>().HasQueryFilter(e => true);
            modelBuilder.Entity<CategoriaEmpleado>().HasQueryFilter(e => true);
            modelBuilder.Entity<UsuarioRol>().HasQueryFilter(e => true);
            modelBuilder.Entity<RolTurnoDetalle>().HasQueryFilter(e => true);
            modelBuilder.Entity<Auditoria>().HasQueryFilter(e => true);
            modelBuilder.Entity<UspConsultarCita>().HasNoKey();
        }

    }
}
