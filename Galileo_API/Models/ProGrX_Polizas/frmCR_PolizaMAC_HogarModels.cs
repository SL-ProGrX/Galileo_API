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
    }

    public class CrPolizaMacHogarEnvioRow : PolizaEnvioRow
    {
       
    }

    public class CrPolizaMacHogarBeneficiariosRowDto : PolizaBeneficiariosB1B6Base
    {
    }

    public class CrPolizaMacHogarRecepcionValidarRequest : CrPolizaMacHogarRecepcionRequestBase
    {
    }

    public class CrPolizaMacHogarRecepcionProcesarRequest : CrPolizaMacHogarRecepcionRequestBase
    {
    }

    public abstract class BeneficiariosB1B6MacHogar : PolizaBeneficiariosB1B6Base
    {
    }

    public class CrPolizaMacHogarRecepcionRowDto : PolizaRecepcionRowBase
    {
    }

}
