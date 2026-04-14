namespace Galileo_API.Models.ProGrX.Cobros
{
    public class FrmCoGestorExternoModels
    {

        public class CrdGestorExternoListaItemModel
        {
            public int Id { get; set; }
            public long Id_Solicitud { get; set; }
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string GestionUsuario { get; set; } = string.Empty;
            public string Expediente { get; set; } = string.Empty;
            public string Notas { get; set; } = string.Empty;
            public string UsuarioRegistro { get; set; } = string.Empty;
            public DateTime? FechaRegistro { get; set; }
            public string UsuarioModificacion { get; set; } = string.Empty;
            public DateTime? FechaModificacion { get; set; }
            public string UsuarioReversion { get; set; } = string.Empty;
            public DateTime? FechaReversion { get; set; }
            public string Estado { get; set; } = string.Empty;
        }

        public class CrdGestorExternoFiltroRequest
        {
            public string Estado { get; set; } = "A";
            public bool IgnorarFechas { get; set; }
            public DateTime? FechaInicio { get; set; }
            public DateTime? FechaCorte { get; set; }
            public string Filtro { get; set; } = string.Empty;
            public string Expediente { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public long? Operacion { get; set; }
            public string Gestiona { get; set; } = string.Empty;
        }

        public class CrdGestorExternoRegistrarRequest
        {
            public long Operacion { get; set; }
            public string GestionUsuario { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Expediente { get; set; } = string.Empty;
            public string Notas { get; set; } = string.Empty;
            public string UsuarioEjecuta { get; set; } = string.Empty;
        }

        public class CrdGestorExternoReversaRequest
        {
            public int CasoId { get; set; }
            public string Notas { get; set; } = string.Empty;
            public string UsuarioEjecuta { get; set; } = string.Empty;
        }

        public class CrdGestorExternoOperacionModel
        {
            public int Id_Solicitud { get; set; }
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
            public string Antiguedad { get; set; } = string.Empty;
            public decimal? Saldo { get; set; }
        }
        public class CrdGestorExternoCargaFilaRequest
        {
            public long Operacion { get; set; }
            public string Expediente { get; set; } = string.Empty;
            public string Notas { get; set; } = string.Empty;
        }
        public class CrdGestorExternoCargaMasivaRequest
        {
            public string EstadoProceso { get; set; } = "A";
            public string GestionUsuario { get; set; } = string.Empty;
            public string UsuarioEjecuta { get; set; } = string.Empty;
            public List<CrdGestorExternoCargaFilaRequest> Registros { get; set; } = [];
        }

        public class CrdGestorExternoCargaMasivaResponse
        {
            public int TotalRecibidos { get; set; }
            public int TotalProcesados { get; set; }
            public int TotalConError { get; set; }
            public List<string> Mensajes { get; set; } = [];
        }
        public class CrdGestorExternoSpResponse
        {
            public int Pass { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
        }
    }
}
