namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class FrmCntxConInformeEspecialModels
    {
        public sealed class CntConsolidadoEspecialGenerarRequest
        {
            public int Contabilidad { get; set; }
            public int Anio { get; set; }
            public short Mes { get; set; }
        }


        public sealed class CntConsolidadoEspecialRegistro
        {
            public string COD_CUENTA_MASK { get; set; } = string.Empty;

            public string Cuenta_Desc { get; set; } = string.Empty;

            public decimal U_Central_Anio_0 { get; set; } = 0;

            public decimal U_Hotel_Anio_0 { get; set; } = 0;

            public decimal U_Jaules_Anio_0 { get; set; } = 0;           

            public decimal Consolidado_Anio_0 { get; set; } = 0;

            public decimal U_Central_Anio_1 { get; set; } = 0;

            public decimal U_Hotel_Anio_1 { get; set; } = 0;

            public decimal U_Jaules_Anio_1 { get; set; } = 0;

            public decimal Consolidado_Anio_1 { get; set; } = 0;

            public decimal Variacion_Anio_1 { get; set; } = 0;

            public decimal Variacion_Porc_Anio_1 { get; set; } = 0;

            public decimal U_Central_Anio_2 { get; set; } = 0;

            public decimal U_Hotel_Anio_2 { get; set; } = 0;

            public decimal U_Jaules_Anio_2 { get; set; } = 0;

            public decimal Consolidado_Anio_2 { get; set; } = 0;

            public decimal Variacion_Anio_2 { get; set; } = 0;

            public decimal Variacion_Porc_Anio_2 { get; set; } = 0;

            public string Tipo_Cuenta_Desc { get; set; } = string.Empty;

            public string COD_DIVISA { get; set; } = string.Empty;

            public int Anio { get; set; } = 0;

            public short Mes { get; set; } = 0;

            public int Nivel { get; set; } = 0;

            public string Acepta_Movimientos { get; set; } = string.Empty;

            public string Clasificacion { get; set; } = string.Empty;
        }

        public sealed class ArchivoGeneradoModel
        {
            //public byte[] Contenido { get; set; } = [];

            public string NombreArchivo { get; set; } = string.Empty;

            public string ContentType { get; set; } = string.Empty;
        }
    }
    public sealed class CntConsolidadoEspecialContext
    {
        public int Contabilidad { get; init; }

        public int Anio { get; init; }

        public short Mes { get; init; }

        public string Usuario { get; init; } = string.Empty;
    }
}
