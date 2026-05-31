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

    public class InsertarCreditoRequest
    {
        public string Codigo { get; set; } = string.Empty;
        public int? IdComite { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public decimal? MontoSol { get; set; }
        public decimal? MontoApr { get; set; }
        public decimal? MontoGirado { get; set; }
        public decimal? Saldo { get; set; }
        public decimal? Amortiza { get; set; }
        public decimal? Interesc { get; set; }
        public decimal? SaldoMes { get; set; }
        public decimal? Cuota { get; set; }
        public decimal? Int { get; set; }
        public decimal? Interesv { get; set; }
        public short? Plazo { get; set; }
        public string UserRec { get; set; } = string.Empty;
        public string UserRes { get; set; } = string.Empty;
        public string UserFor { get; set; } = string.Empty;
        public string UserTesoreria { get; set; } = string.Empty;
        public DateTime? Tesoreria { get; set; }
        public DateTime? FechaSol { get; set; }
        public DateTime? FechaRes { get; set; }
        public DateTime? FechaForp { get; set; }
        public DateTime? FechaForf { get; set; }
        public DateTime? FechaCalculoInt { get; set; }
        public string Garantia { get; set; } = string.Empty;
        public string PrimerCuota { get; set; } = "N";
        public string TDocumento { get; set; } = "OT";
        public string NDocumento { get; set; } = string.Empty;
        public int? Pagare { get; set; }
        public int? FirmaDeudor { get; set; }
        public int? Premio { get; set; }
        public string Observacion { get; set; } = string.Empty;
        public string Estado { get; set; } = "A";
        public decimal? PriDeduc { get; set; }
        public decimal? FecUlt { get; set; }
        public string EstadoSol { get; set; } = "F";
        public string DocumentoReferido { get; set; } = string.Empty;
        public string? CodDestino { get; set; }
        public string CodDivisa { get; set; } = "COL";
        public string BaseCalculo { get; set; } = "01";
    }

    public class ValidacionPreviaInsertarCreditoResponse
    {
        public bool ExisteCatalogo { get; set; }
        public bool ExisteSocio { get; set; }
        public string? CtaNintC { get; set; }
    }
}
