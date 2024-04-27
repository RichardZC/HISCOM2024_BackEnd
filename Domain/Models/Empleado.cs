using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class Empleado
    {
        public Empleado()
        {
            CategoriaEmpleado = new HashSet<CategoriaEmpleado>();
            Notificacion = new HashSet<Notificacion>();
            RolTurno = new HashSet<RolTurno>();
            RolTurnoDetalle = new HashSet<RolTurnoDetalle>();
        }

        public int Id { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Nombres { get; set; }
        public DateTime Nacimiento { get; set; }
        public string Sexo { get; set; }
        public int TipoDocumentoId { get; set; }
        public string NumeroDoc { get; set; }
        public int? TipoEmpleadoId { get; set; }
        public int? OrganigramaId { get; set; }
        public string CargoId { get; set; }
        public string CondicionLaboralId { get; set; }
        public string ProfesionId { get; set; }
        public int? ColegioProfesionalId { get; set; }
        public string RegimenLaboralId { get; set; }
        public int? NacionalidadId { get; set; }
        public int? BancoId { get; set; }
        public int? TipoCuentaId { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string NumeroColegiatura { get; set; }
        public string Correos { get; set; }
        public int? EstadoCivilId { get; set; }
        public string Direccion { get; set; }
        public DateTime? FechaNombramiento { get; set; }
        public string NumeroCuenta { get; set; }
        public string CuentaInterbancaria { get; set; }
        public string Telefonos { get; set; }
        public bool EsJefe { get; set; }
        public DateTime FechaReg { get; set; }
        public DateTime? FechaMod { get; set; }
        public bool Estado { get; set; }
        public bool IndInforhus { get; set; }
        public bool IndPlanillaNom { get; set; }
        public bool IndPlanillaCas { get; set; }

        public virtual Banco Banco { get; set; }
        public virtual Cargo Cargo { get; set; }
        public virtual ColegioProfesional ColegioProfesional { get; set; }
        public virtual CondicionLaboral CondicionLaboral { get; set; }
        public virtual EstadoCivil EstadoCivil { get; set; }
        public virtual Nacionalidad Nacionalidad { get; set; }
        public virtual Organigrama Organigrama { get; set; }
        public virtual Profesion Profesion { get; set; }
        public virtual RegimenLaboral RegimenLaboral { get; set; }
        public virtual TipoCuenta TipoCuenta { get; set; }
        public virtual TipoDocumento TipoDocumento { get; set; }
        public virtual TipoEmpleado TipoEmpleado { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<CategoriaEmpleado> CategoriaEmpleado { get; set; }
        public virtual ICollection<Notificacion> Notificacion { get; set; }
        public virtual ICollection<RolTurno> RolTurno { get; set; }
        public virtual ICollection<RolTurnoDetalle> RolTurnoDetalle { get; set; }
    }
}
