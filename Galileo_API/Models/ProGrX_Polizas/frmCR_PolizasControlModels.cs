namespace Galileo_API.Models.ProGrX_Polizas
{
    public class PolizaLookupResponseDto
    {
        public string CodPoliza { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool poliza_general { get; set; } = false;
    }

    public class PolizaScrollRequestDto
    {
        public string CodEmpresa { get; set; } = string.Empty;
        public string CodPolizaActual { get; set; } = string.Empty;
        public int Direccion { get; set; } = 0; // 1 = siguiente, -1 = anterior
    }

    public class CrPolizasControlCierreRowDto
    {
        public int cod_corte { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string Tipo { get; set; } = string.Empty;              // 'P' / 'D'
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class CrPolizasControlNuevoRequestDto
    {
        public string Tipo { get; set; } = string.Empty;      // 'P' o 'D'
        public DateTime? FechaCorte { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string CodPoliza { get; set; } = string.Empty;
    }

}
