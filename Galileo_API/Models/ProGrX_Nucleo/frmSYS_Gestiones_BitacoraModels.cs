namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class FrmSysGestionesBitacoraModels
    {
        public class SysGestionesBitacorasData
        {
            public string? cedula { get; set; }
            public string? nombre { get; set; }
            public DateTime? registro_fecha { get; set; }
            public string? registro_Usuario { get; set; }
            public string? descripcion { get; set; }
            public string? notas { get; set; }
        }

        public class SysGestionesBitacorasLista
        {
            public int total { get; set; }
            public List<SysGestionesBitacorasData>? lista { get; set; }
        }

        public class SociosLookupData
        {
            public string? CEDULA { get; set; }
            public string? CEDULAR { get; set; }
            public string? NOMBRE { get; set; }
        }
        
        public class SociosLookupLista
        {
            public int total { get; set; }
            public List<SociosLookupData>? lista { get; set; }
        }
    }
}
