namespace Galileo_API.Models.ProGrX_Procesos
{
    public sealed class CcCuotaMantenimientoEjecutarRequest
    {
        public required int CodEmpresa { get; set; }
        public required string Usuario { get; set; }
        public required int CodContabilidad { get; set; }
        public required int CodInstitucion { get; set; }
    }
}
