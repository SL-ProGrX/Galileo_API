namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class FrmSysCorreosBandejaModels
    {
        public class SysCorreosBandejaData
        {
            public int? IdEmail { get; set; }
            public string? CodSmtp { get; set; }
            public string? Para { get; set; }
            public string? Asunto { get; set; }
            public string? EstadoDesc { get; set; }
            public DateTime? Fecha { get; set; }
            public DateTime? FechaEnvio { get; set; }
            public string? Usuario { get; set; }
            public int? Anio { get; set; }
            public int? MesId { get; set; }
            public string? Mes { get; set; }
        }

        public class SysCorreosBandejaResumenData
        {
            public string Cod_Smtp { get; set; } = string.Empty;
            public int Correos { get; set; }
            public string EstadoDesc { get; set; } = string.Empty;
            public int? Anio { get; set; }
            public int? MesId { get; set; }
            public string Mes { get; set; } = string.Empty;
        }

        public class SysCorreosBandejaLista
        {
            public int total { get; set; }
            public List<SysCorreosBandejaData>? lista { get; set; }
        }
        
        public class SysCorreosBandejaResumenLista
        {
            public int total { get; set; }
            public List<SysCorreosBandejaResumenData> lista { get; set; } = new();
        }
    }
}