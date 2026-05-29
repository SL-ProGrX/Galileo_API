namespace Galileo_API.Models.ProGrX.Creditos
{
    public class RetencionCreditoData
    {
        public int Id_Solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Cuota { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public DateTime? FechaForp { get; set; }
        public int Plazo { get; set; }
        public string Amortiza { get; set; } = string.Empty;
        public int Cuotas_Planilla { get; set; }
        public int Cuotas_Directas { get; set; }
        public string Documento_Referido { get; set; } = string.Empty;
        public decimal Prideduc { get; set; }
        public string UserRec { get; set; } = string.Empty;
        public string Cod_Destino { get; set; } = string.Empty;
        public string Garantia { get; set; } = string.Empty;
        public string GarantiaDesc { get; set; } = string.Empty;
        public string DestinoDesc { get; set; } = string.Empty;
        public decimal Base_Calculo { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
    }

    public class SocioData
    {
        public string Cedula { get; set; } = string.Empty;
        public string Cedular { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
    public class CatalogoRetencionData
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
    public class InstitucionFrecuenciaData
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Frecuencia_Id { get; set; } = string.Empty;
    }
    public class SocioDeduccionData
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Deduccion { get; set; }
        public string Cod_Institucion { get; set; } = string.Empty;
        public string DeductoraCod { get; set; } = string.Empty;
        public string DeductoraDesc { get; set; } = string.Empty;
    }

    public class PrimerDeduccionData
    {
        public DateTime FechaCorte { get; set; }
        public string FrecuenciaId { get; set; } = "M";
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int Quincena { get; set; }
        public decimal PrimerDeduccion { get; set; }
    }

    public class PrimerDeduccionRawData
    {
        public DateTime FechaCorte { get; set; }
        public string FrecuenciaId { get; set; } = "M";
    }

    public class CatalogoDetalleData
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public int? Id_Comite { get; set; }
        public string Comite_Desc { get; set; } = string.Empty;
        public decimal Base_Calculo { get; set; }
    }

    public class SiguienteSolicitudData
    {
        public int Id_Solicitud { get; set; }
    }
}
