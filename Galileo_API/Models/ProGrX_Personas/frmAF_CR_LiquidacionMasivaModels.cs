namespace PgxAPI.Models.ProGrX_Personas
{
    public class AF_LiquidacionMasiva_Filtros
    {
        public DateTime Inicio { get; set; }
        public DateTime Corte { get; set; }
        public string? Tipo { get; set; }
        public int? Institucion { get; set; }
        public int? Causa { get; set; }
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Ejecutivo { get; set; } = "";
        public string Usuario { get; set; } = "";
    }

    public class AF_Liquidacion_Masiva
    {
        public int Cod_Renuncia { get; set; }
        public string Cedula { get; set; }              // Cédula
        public string Nombre { get; set; }              // Nombre
        public string Tipo_Desc { get; set; }           // Tipo
        public string Causa_Desc { get; set; }          // Causa
        public string Estado_Desc { get; set; }         // Estado
        public string Resuelto_Fecha_Mask { get; set; } // Res.Fecha (formateada)
        public string Resuelto_User { get; set; }       // Res.Usuario
        public string Registro_Fecha_Mask { get; set; } // Reg.Fecha (formateada)
        public string Registro_User { get; set; }       // Reg.Usuario
        public string Promotor_Desc { get; set; }       // Ejecutivo
    }
}
