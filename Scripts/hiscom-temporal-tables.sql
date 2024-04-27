USE [HISCOM]
GO
/****** Object:  Table [dbo].[Categoria]    Script Date: 2/24/21 1:35:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categoria](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Denominacion] [varchar](64) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CIE10]    Script Date: 2/24/21 1:35:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CIE10](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [varchar](20) NOT NULL,
	[Diagnostico] [varchar](512) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CitaMedica]    Script Date: 2/24/21 1:35:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CitaMedica](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProgramacionMedicaId] [int] NOT NULL,
	[ServicioId] [int] NOT NULL,
	[PacienteId] [int] NOT NULL,
	[ComprobantePagoId] [int] NULL,
	[Fecha] [date] NOT NULL,
	[Hora] [time](7) NOT NULL,
	[Estado] [varchar](32) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Consulta]    Script Date: 2/24/21 1:35:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Consulta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CitaId] [int] NOT NULL,
	[FechaIngreso] [datetime] NOT NULL,
	[FechaEgreso] [datetime] NOT NULL,
	[Descripcion] [varchar](256) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DetalleConsulta]    Script Date: 2/24/21 1:35:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DetalleConsulta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ConsultaId] [int] NOT NULL,
	[TriajeId] [int] NOT NULL,
	[Motivo] [varchar](256) NULL,
	[TiempoEnfermedad] [int] NULL,
	[UnidadCronologica] [varchar](16) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Diagnostico]    Script Date: 2/24/21 1:35:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Diagnostico](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DetalleConsultaId] [int] NOT NULL,
	[Observacion] [varchar](512) NULL,
	[Estado] [varchar](16) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DiagnosticoDetalle]    Script Date: 2/24/21 1:35:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiagnosticoDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DiagnosticoId] [int] NOT NULL,
	[CIE10Id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmpleadoProgramacion]    Script Date: 2/24/21 1:35:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmpleadoProgramacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmpleadoId] [int] NOT NULL,
	[ProgramacionMedicaId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Examen]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Examen](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CategoriaId] [int] NOT NULL,
	[Codigo] [varchar](8) NOT NULL,
	[Descripcion] [varchar](64) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExamenAuxiliar]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExamenAuxiliar](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PlanTratamientoId] [int] NOT NULL,
	[ExamenId] [int] NOT NULL,
	[Resultado] [text] NULL,
	[Observaciones] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HistoriaClinica]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistoriaClinica](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PacienteId] [int] NOT NULL,
	[AntecendenteId] [int] NULL,
	[Codigo] [varchar](8) NOT NULL,
	[FechaCreacion] [datetime] NOT NULL,
	[FechaPasoPasivo] [datetime] NULL,
	[Estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HistoriaClinicaDetalle]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistoriaClinicaDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HistoriaClinicaId] [int] NOT NULL,
	[TriajeId] [int] NOT NULL,
	[ConsultaId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Medicamento]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Medicamento](
	[Id] [int] NOT NULL,
	[Nombre] [varchar](128) NULL,
	[FF] [varchar](32) NOT NULL,
	[Presentacion] [varchar](32) NULL,
	[Concentracion] [varchar](128) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Paciente]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paciente](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ApellidoPaterno] [varchar](32) NOT NULL,
	[ApellidoMaterno] [varchar](32) NOT NULL,
	[Nombres] [varchar](64) NOT NULL,
	[Dni] [varchar](8) NOT NULL,
	[Nacimiento] [date] NOT NULL,
	[Domicilio] [varchar](256) NULL,
	[Correo] [varchar](128) NULL,
	[Telefono] [varchar](16) NULL,
	[Sexo] [char](1) NOT NULL,
	[GrupoSanguineo] [varchar](16) NOT NULL,
	[EstadoCivil] [char](1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlanTratamiento]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanTratamiento](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DetalleConsultaId] [int] NOT NULL,
	[Indicaciones] [text] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProgramacionMedica]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProgramacionMedica](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmpleadoId] [int] NOT NULL,
	[TurnoId] [int] NOT NULL,
	[HoraInicio] [time](7) NOT NULL,
	[HoraFin] [time](7) NOT NULL,
	[Duracion] [int] NOT NULL,
	[DiaSemana] [char](1) NOT NULL,
	[Ano] [char](4) NOT NULL,
	[Estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecetaMedica]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecetaMedica](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MedicamentoId] [int] NOT NULL,
	[PlanTratamientoId] [int] NOT NULL,
	[Frecuencia] [varchar](16) NOT NULL,
	[Tiempo] [varchar](16) NOT NULL,
	[Cantidad] [int] NOT NULL,
	[ViaAdministracion] [varchar](16) NOT NULL,
	[Observaciones] [varchar](256) NULL,
	[Dosis] [int] NOT NULL,
	[UnidadCronologica] [varchar](16) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sintomas]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sintomas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Codigo] [varchar](24) NOT NULL,
	[Denominacion] [varchar](512) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SintomasDetalleConsulta]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SintomasDetalleConsulta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SintomasId] [int] NOT NULL,
	[DetalleConsultaId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Triaje]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Triaje](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmpleadoId] [int] NOT NULL,
	[CitaId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TriajeDetalle]    Script Date: 2/24/21 1:35:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TriajeDetalle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TriajeId] [int] NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[Peso] [decimal](10, 2) NOT NULL,
	[Talla] [decimal](10, 2) NOT NULL,
	[Diastole] [decimal](10, 2) NOT NULL,
	[Sistole] [decimal](10, 2) NOT NULL,
	[Pulso] [decimal](10, 2) NOT NULL,
	[Temperatura] [decimal](10, 2) NOT NULL,
	[IMC] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
