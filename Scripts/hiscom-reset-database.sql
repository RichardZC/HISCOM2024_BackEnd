USE HISCOM
GO
DELETE FROM rh.RolTurnoRevision
GO
DBCC CHECKIDENT ('rh.RolTurnoRevision', RESEED, 0)
GO
DELETE FROM rh.RolTurnoIntento
GO
DBCC CHECKIDENT ('rh.RolTurnoIntento', RESEED, 0)
GO
DELETE FROM rh.RolTurnoDetalle
GO
DBCC CHECKIDENT ('rh.RolTurnoDetalle', RESEED, 0)
GO
DELETE FROM rh.RolTurnoEstab
GO
DBCC CHECKIDENT ('rh.RolTurnoEstab', RESEED, 0)
GO
DELETE FROM rh.RolTurno
GO
DBCC CHECKIDENT ('rh.RolTurno', RESEED, 0)
GO
DELETE FROM rh.Turno
GO
DBCC CHECKIDENT ('rh.Turno', RESEED, 0)
GO
DELETE FROM dbo.Auditoria
GO
DELETE FROM dbo.EnlaceReset
GO
DELETE FROM dbo.ExcepcionInformacion
GO
DBCC CHECKIDENT ('dbo.ExcepcionInformacion', RESEED, 0)
GO
DELETE FROM dbo.Excepcion
GO
DBCC CHECKIDENT ('dbo.Excepcion', RESEED, 0)
GO
DELETE FROM dbo.Notificacion
GO
DBCC CHECKIDENT ('dbo.Notificacion', RESEED, 0)
GO
DELETE FROM seguridad.Usuario
GO
DBCC CHECKIDENT ('seguridad.Usuario', RESEED, 0)
GO
DELETE FROM rh.Empleado
GO
DBCC CHECKIDENT ('rh.Empleado', RESEED, 0)
GO
DELETE FROM rh.Banco
GO
DBCC CHECKIDENT ('rh.Banco', RESEED, 0)
GO
DELETE FROM rh.Cargo
GO
DELETE FROM rh.CategoriaEmpleado
GO
DBCC CHECKIDENT ('rh.CategoriaEmpleado', RESEED, 0)
GO
DELETE FROM rh.Categoria
GO
DBCC CHECKIDENT ('rh.Categoria', RESEED, 0)
GO
DELETE FROM rh.Clasificacion
GO
DBCC CHECKIDENT ('rh.Clasificacion', RESEED, 0)
GO
DELETE FROM rh.Profesion
GO
DELETE FROM rh.ColegioProfesional
GO
DBCC CHECKIDENT ('rh.ColegioProfesional', RESEED, 0)
GO
DELETE FROM rh.CondicionLaboral
GO
DELETE FROM rh.RegimenLaboral
GO
DELETE FROM rh.Especialidad
GO
DBCC CHECKIDENT ('rh.Especialidad', RESEED, 0)
GO
DELETE FROM rh.EstadoCivil
GO
DBCC CHECKIDENT ('rh.EstadoCivil', RESEED, 0)
GO
DELETE FROM rh.Nacionalidad
GO
DBCC CHECKIDENT ('rh.Nacionalidad', RESEED, 0)
GO
DELETE FROM rh.TipoEmpleado
GO
DBCC CHECKIDENT ('rh.TipoEmpleado', RESEED, 0)
GO
DELETE FROM rh.TipoDocumento
GO
DBCC CHECKIDENT ('rh.TipoDocumento', RESEED, 0)
GO
DELETE FROM rh.TipoCuenta
GO
DBCC CHECKIDENT ('rh.TipoCuenta', RESEED, 0)
GO
DELETE FROM seguridad.UsuarioRol
GO
DBCC CHECKIDENT ('seguridad.UsuarioRol', RESEED, 0)
GO
DELETE FROM seguridad.RolPermiso
GO
DBCC CHECKIDENT ('seguridad.RolPermiso', RESEED, 0)
GO
DELETE FROM rh.Organigrama
GO
DBCC CHECKIDENT ('rh.Organigrama', RESEED, 0)
GO
DELETE FROM rh.PlhPlanillaConcepto
GO
DBCC CHECKIDENT ('rh.PlhPlanillaConcepto', RESEED, 0)
GO
DELETE FROM rh.PlhConcepto
GO
DBCC CHECKIDENT ('rh.PlhConcepto', RESEED, 0)
GO
DELETE FROM rh.PlhPlanilla
GO
DBCC CHECKIDENT ('rh.PlhPlanilla', RESEED, 0)
GO
DELETE FROM rh.Marcacion
GO
DBCC CHECKIDENT ('rh.Marcacion', RESEED, 0)