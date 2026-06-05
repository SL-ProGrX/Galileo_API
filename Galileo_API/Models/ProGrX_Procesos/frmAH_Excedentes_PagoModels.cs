
namespace Galileo.Models.ProGrX_Procesos
{

    public class ExcSepararCasosRequestDto
    {
        public int PeriodoId { get; set; }
        public short EnviarSinpe { get; set; }
        public short Paso { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class ExcCasosEspecialesRequestDto
    {
        public int PeriodoId { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class ExcAcreditarCuentasInternasRequestDto
    {
        public int PeriodoId { get; set; }
        public int Top { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class ExcTesoreriaRequestDto
    {
        public int PeriodoId { get; set; }
        public string Oficina { get; set; } = "AOC";
        public string Usuario { get; set; } = string.Empty;
    }

    public class ExcFondosRequestDto
    {
        public int PeriodoId { get; set; }
        public int Operadora { get; set; } = 1;
        public string Usuario { get; set; } = string.Empty;
        public string Concepto { get; set; } = "FND001";
    }

    public class ExcReclasificacionesRequestDto
    {
        public int PeriodoId { get; set; }
        public string Oficina { get; set; } = "AOC";
        public string Usuario { get; set; } = string.Empty;
    }

    public class ExcPendientesDto
    {
        public int Pendientes { get; set; }
    }
}