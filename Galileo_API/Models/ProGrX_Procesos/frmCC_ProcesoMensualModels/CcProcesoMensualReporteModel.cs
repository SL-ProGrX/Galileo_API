namespace Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels
{
    public class CcProcesoMensualReporteModel
    {
        public string NombreReporte { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string SubTitulo { get; set; } = string.Empty;
        public string Filtros { get; set; } = string.Empty;
        public string Institucion { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; } = 0;
        public decimal PorcAhorro { get; set; } = 0;

    }
}
