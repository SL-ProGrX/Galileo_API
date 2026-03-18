namespace Galileo_API.Models.ProGrX_Polizas
{
  
    public abstract class CrPolizaMacHogarRecepcionRequestBase
    {
        public string Poliza { get; set; } = string.Empty;
        public DateTime Corte { get; set; } = DateTime.Now;
        public DateTime? Factura { get; set; }
        public List<CrPolizaMacHogarRecepcionRowDto> Filas { get; set; } = new();
    }

    public class CrPolizaMacHogarEnvioConsultaRequest: PolizaEnvioConsultaRequestBase
    {
        public string pMacH1 { get; set; } = string.Empty;
    }

    public class CrPolizaMacHogarEnvioRow : PolizaEnvioRow
    {
        public string pMacH2 { get; set; } = string.Empty;
    }

    public class CrPolizaMacHogarBeneficiariosRowDto : PolizaBeneficiariosB1B6Base
    {
        public string pMacH3 { get; set; } = string.Empty;
    }

    public class CrPolizaMacHogarRecepcionValidarRequest : CrPolizaMacHogarRecepcionRequestBase
    {
        public string pMacH4 { get; set; } = string.Empty;
    }

    public class CrPolizaMacHogarRecepcionProcesarRequest : CrPolizaMacHogarRecepcionRequestBase
    {
        public string pMacH5 { get; set; } = string.Empty;
    }

    public abstract class BeneficiariosB1B6MacHogar : PolizaBeneficiariosB1B6Base
    {
        public string pMacH6 { get; set; } = string.Empty;
    }

    public class CrPolizaMacHogarRecepcionRowDto : PolizaRecepcionRowBase
    {
        public string pMacH7 { get; set; } = string.Empty;
    }

}
