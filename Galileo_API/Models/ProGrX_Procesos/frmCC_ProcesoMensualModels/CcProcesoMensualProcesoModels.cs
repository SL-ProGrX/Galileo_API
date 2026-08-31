namespace Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels
{
    /// <summary>
    /// Estados del proceso de proceso mensual.
    /// </summary>
    public static class CcProcesoMensualProcesoEstado
    {
        public const string Pendiente = "Pendiente";
        public const string Procesando = "Procesando";
        public const string Finalizando = "Finalizando";
        public const string Completado = "Completado";
        public const string Error = "Error";

        /// <summary>
        /// Determina si el proceso sigue activo (el polling debe continuar).
        /// </summary>
        public static bool EsActivo(string estado) =>
            estado is Pendiente or Procesando or Finalizando;
    }

    /// <summary>
    /// Request para iniciar un proceso resiliente.
    /// </summary>
    public sealed class CcProcesoMensualProcesoIniciarRequest
    {
        public int CodInstitucion { get; set; }
        public decimal FechaProceso { get; set; }
        public string TipoProceso { get; set; } = string.Empty; // '02' o '03'
        public string ContextoJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado del estado del proceso (retornado por Iniciar y Estado).
    /// </summary>
    public sealed class CcProcesoMensualProcesoResultado
    {
        public Guid ProcesoId { get; set; }
        public int CodEmpresa { get; set; }
        public int CodInstitucion { get; set; }
        public decimal FechaProceso { get; set; }
        public string TipoProceso { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Procesadas { get; set; }
        public int Exitosas { get; set; }
        public int Errores { get; set; }
        public decimal Porcentaje { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime UltimaActividad { get; set; }
        public string Propietario { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string? ContextoJson { get; set; }
    }

    /// <summary>
    /// Error individual de un registro del proceso.
    /// </summary>
    public sealed class CcProcesoMensualProcesoError
    {
        public int RegistroNumero { get; set; }
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contexto serializado para Genera Deducciones.
    /// </summary>
    public sealed class CcProcesoMensualGeneraDeduccionesContexto
    {
        public int CodInstitucion { get; set; }
        public decimal FechaProceso { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public bool UsaPlanillaTransito { get; set; }
        public bool AplicaCambioDeducciones { get; set; }
        public bool Redondeo { get; set; }
        public string NombreInstitucion { get; set; } = string.Empty;
        public string NombreEmpresa { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contexto serializado para Carga Deducciones.
    /// </summary>
    public sealed class CcProcesoMensualCargaDeduccionesContexto
    {
        public int CodInstitucion { get; set; }
        public decimal FechaProceso { get; set; }
        public int Pago { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string TipoCarga { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public string ArchivoBase64 { get; set; } = string.Empty;
        public List<CcProcesoMensualCargaDeduccionFilaContexto> Filas { get; set; } = [];
    }

    /// <summary>
    /// Fila individual de carga de deducciones (contexto).
    /// </summary>
    public sealed class CcProcesoMensualCargaDeduccionFilaContexto
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string CodigoDeduccion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Trabajo encolado para el Background Worker.
    /// </summary>
    public sealed class CcProcesoMensualProcesoTrabajo
    {
        public int CodEmpresa { get; set; }
        public Guid ProcesoId { get; set; }
    }
}
