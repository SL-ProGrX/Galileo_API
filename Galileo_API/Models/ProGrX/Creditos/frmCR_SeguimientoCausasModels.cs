namespace Galileo_API.Models.ProGrX.Creditos
{
    public class FrmCRSeguimientoCausasModels
    {
        public class CrSeguimientoCausasObtenerRequest
        {
            public int IdSolicitud { get; set; } = 0;
            public string Tipo { get; set; } = string.Empty;
        }


        public class CrSeguimientoCausasData
        {
            public string CodCausa { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public bool Seleccionado { get; set; } = 0;
        }
        public class CrSeguimientoCausasActualizarRequest
        {
            public int IdSolicitud { get; set; } = 0;
            public string Tipo { get; set; } = string.Empty;
            public string Codigo { get; set; } = string.Empty;
            public string CodCausa { get; set; } = string.Empty;
            public bool Seleccionado { get; set; } = false;
        }
    }
}
