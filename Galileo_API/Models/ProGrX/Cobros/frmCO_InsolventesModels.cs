namespace Galileo_API.Models.ProGrX.Cobros
{
    public class FrmCoInsolventesModels
    {
        public sealed class CbrInsolventeGridItem
        {
            public int? Id { get; set; }
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string? Expediente { get; set; }
            public DateTime? FechaSentencia { get; set; }
            public string? Notas { get; set; }

            public string? UsuarioRegistro { get; set; }
            public DateTime? FechaRegistro { get; set; }

            public string? UsuarioModificacion { get; set; }
            public DateTime? FechaModificacion { get; set; }

            public string? UsuarioReversion { get; set; }
            public DateTime? FechaReversion { get; set; }

            public string Estado { get; set; } = string.Empty;
        }

        public class CbrInsolventesBuscarRequest
        {
            /// <summary>Estado: 'A' Activos, 'R' Reversados (desde combo VB6 toma primera letra).</summary>
            public string Estado { get; set; } = "A";

            /// <summary>Si true, equivalente a chkFechas = checked, ignora fechas.</summary>
            public bool IgnorarFechas { get; set; } = true;

            public DateTime? FechaInicio { get; set; }
            public DateTime? FechaCorte { get; set; }

            public string? Filtro { get; set; }
            public string? Expediente { get; set; }
            public string? Usuario { get; set; }
        }

        public sealed class CbrInsolventeRegistrarRequest { 

             public int CasoId { get; set; } = 0;
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Expediente { get; set; } = string.Empty;
            public DateTime? FechaSentencia { get; set; }
            public string Notas { get; set; } = string.Empty;
        }

        public class CbrInsolventeSocioResult
        {
            public string? Cedula { get; set; } = string.Empty;
            public string? CedulaR { get; set; } = string.Empty;
            public string? Nombre { get; set; } = string.Empty;
        }

      
        public sealed class CbrSpMovimientoResult
        {
            public int? Pass { get; set; }
            public string? Movimiento { get; set; }
            public string? Mensaje { get; set; }
        }
    }

}
