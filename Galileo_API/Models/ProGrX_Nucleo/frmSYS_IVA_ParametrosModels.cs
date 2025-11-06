namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class FrmSysIvaParametrosModels
    {
        public class SysIvaParametrosData
        {
            public string? codParametro { get; set; }
            public string? descripcion { get; set; }
            public string? valor { get; set; }
            public string? tipo { get; set; }
            public string? visible { get; set; }
            public string? notas { get; set; }
            public string? registroUsuario { get; set; }
            public DateTime? registroFecha { get; set; }
            public string? modificaUsuario { get; set; }
            public DateTime? modificaFecha { get; set; }
            public string? valorMask { get; set; }
            public string? cuentaDescripcion { get; set; }
        }
        
        public class SysIvaParametrosLista
        {
            public int total { get; set; }
            public List<SysIvaParametrosData>? lista { get; set; }
        }

        public class SysIvaParametrosUpdateRequest
        {
            public string? valor { get; set; }
        }
        
        
        public class SysIvaCuentasResumenData
        {
            public string? codigo { get; set; }
            public string? codigoMask { get; set; }
            public string? codigoAlterna { get; set; }
            public string? nombre { get; set; }
            public string? movimientos { get; set; }
            public string? divisa { get; set; }
            public int? nivel { get; set; }
        }

        public class SysIvaCuentasResumenLista
        {
            public int total { get; set; }
            public List<SysIvaCuentasResumenData>? lista { get; set; }
        }
    }
}