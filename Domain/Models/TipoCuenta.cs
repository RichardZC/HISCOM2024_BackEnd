﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Domain.Models
{
    public partial class TipoCuenta
    {
        public TipoCuenta()
        {
            Empleado = new HashSet<Empleado>();
        }

        public int Id { get; set; }
        public string Denominacion { get; set; }

        public virtual ICollection<Empleado> Empleado { get; set; }
    }
}
