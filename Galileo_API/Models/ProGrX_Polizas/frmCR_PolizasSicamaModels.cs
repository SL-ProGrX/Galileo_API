namespace Galileo_API.Models.ProGrX_Polizas
{
    public class CrPolizasSicamaEnvioConsultaRequest: PolizaEnvioConsultaRequestBase
    {

    }

    public class CrPolizasSicamaEnvioRow : PolizaEnvioRow
    {

    }

    public class CrPolizasSicamaBeneficiariosRowDto : PolizaBeneficiariosB1B6Base
    {
    }

    public class CrFndPlanillaDirectaSubeRequest
    {
        public int institucion { get; set; } = 0;
        public int operadora { get; set; } = 0;
        public string plan { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public int proceso { get; set; } = 0;

        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal fondos { get; set; } = 0;

        public short linea { get; set; } = 0;
        public short inicializa { get; set; } = 0;
    }

    public class CrFndPlanillaDirectaConsultaRequest
    {
        public int operadora { get; set; } = 0;
        public string plan { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public short revisar { get; set; } = 1;
    }

    public abstract class BeneficiariosB1B6 : PolizaBeneficiariosB1B6Base
    {
    }

    public class CrFndPlanillaDirectaConsultaRowDto : PolizaRecepcionRowBase
    {
    }
}