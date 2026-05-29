namespace Galileo.Models.ProGrX.Clientes
{
    public class FndTrasladoPatrimonioPlan
    {
        public string? Plan { get; set; }
        public string? Descripcion { get; set; }
    }

    public class FndTrasladoPatrimonioDetalle
    {
        public string? Descripcion { get; set; }
        public string? Patrimonio_Tipo { get; set; }
        public string? Patrimonio { get; set; }
    }

    public class FndTrasladoPatrimonioContrato
    {
        public bool Marcado { get; set; }
        public int Cod_Contrato { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? EstadoPersona { get; set; }
        public decimal Monto { get; set; }
        public string? EstadoActual { get; set; }
    }

    public class FndDocumentoConsecutivoRequest
    {
        public string Tipo { get; set; } = string.Empty;
        public required long Operadora { get; set; }
        public required int SysDocVersion { get; set; }
    }

    public class FndDocumentoConsecutivoResult
    {
        public long Consecutivo { get; set; }
    }

    public class FndDocumentoConsecutivoAseRequest
    {
        public string Tipo { get; set; } = string.Empty;
        public required int SysDocVersion { get; set; }
    }

    public class FndDocumentoConsecutivoAseResult
    {
        public long Consecutivo { get; set; }
    }

    public class SimpleSuccessResult
    {
        public bool Success { get; set; }
    }

    public class FndContratoDetalleInsertRequest
    {
        public required int CodOperadora { get; set; }
        public string CodPlan { get; set; } = string.Empty;
        public required int CodContrato { get; set; }
        public required decimal Monto { get; set; }
        public DateTime? Fecha { get; set; }
        public string Tcon { get; set; } = string.Empty;
        public string Ncon { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class FndContratoUpdateRequest
    {
        public required int CodOperadora { get; set; }
        public string CodPlan { get; set; } = string.Empty;
        public required int CodContrato { get; set; }
    }

    public class FndDocumentoInsertRequest
    {
        public string Tipo { get; set; } = string.Empty;
        public string IdDocumento { get; set; } = string.Empty;
        public required int CodOperadora { get; set; }
        public string Cliente { get; set; } = "APLICACION GENERAL";
        public string Concepto { get; set; } = string.Empty;
        public required decimal Monto { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Detalle1 { get; set; } = string.Empty;
        public string Detalle2 { get; set; } = string.Empty;
        public string Detalle3 { get; set; } = string.Empty;
        public string Detalle4 { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Dp { get; set; } = string.Empty;
    }

    public class FndAsientoInsertRequest
    {
        public required int CodOperadora { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string IdDocumento { get; set; } = string.Empty;
        public string FndCuenta { get; set; } = string.Empty;
        public required decimal FndMonto { get; set; }
        public string FndDebehaber { get; set; } = string.Empty;
    }

    public class SifTransaccionInsertRequest
    {
        public string CodTransaccion { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = "FTRA";
        public string RegistroUsuario { get; set; } = string.Empty;
        public string ClienteIdentificacion { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = "APLICACION GENERAL";
        public string CodConcepto { get; set; } = "FND003";
        public required decimal Monto { get; set; }
        public string Estado { get; set; } = "P";
        public string Referencia01 { get; set; } = string.Empty;
        public string Referencia02 { get; set; } = string.Empty;
        public string Referencia03 { get; set; } = string.Empty;
        public string CodOficina { get; set; } = string.Empty;
        public string Linea1 { get; set; } = string.Empty;
        public string Linea2 { get; set; } = string.Empty;
        public string Linea3 { get; set; } = string.Empty;
        public string Linea4 { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
    }

    public class SifDocsAsientoRequest
    {
        public string Tipo { get; set; } = string.Empty;
        public string Transaccion { get; set; } = string.Empty;
        public required decimal Monto { get; set; }
        public string Movimiento { get; set; } = string.Empty; // 'D' o 'C'
        public string Divisa { get; set; } = string.Empty;
        public required decimal TipoCambio { get; set; }
        public required int Contabilidad { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public string CentroCosto { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string Referencia1 { get; set; } = string.Empty;
        public string Referencia2 { get; set; } = string.Empty;
        public string Referencia3 { get; set; } = string.Empty;
        public short DivisaRev { get; set; } = 0;
        public short NoReversa { get; set; } = 0;
    }

    public class FndTrasladoPatrimonioSocioDetalle
    {
        public string? Cedula { get; set; }
        public string? EstadoActual { get; set; }
        public decimal Monto { get; set; }
        public string? Existe { get; set; }
    }

    public class FndAhorroConsolidadoSocio
    {
        public string Cedula { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Existe { get; set; } = string.Empty;
    }

    public class FndAhorroConsolidadoRequest
    {
        public string Destino { get; set; } = string.Empty; // "O", "P", "C"
        public List<FndAhorroConsolidadoSocio> Socios { get; set; } = new();
        public DateTime? Fecha { get; set; }
        public string NC_Pat { get; set; } = string.Empty;
        public string TipoDoc { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Concepto { get; set; } = string.Empty;
    }

    public class SifTransaccionPatrimonioInsertRequest
    {
        public string NC_Pat { get; set; } = string.Empty;         // vNC_Pat
        public string TipoDoc { get; set; } = string.Empty;        // vTipoDoc
        public string Usuario { get; set; } = string.Empty;        // Usuario
        public string Concepto { get; set; } = string.Empty;       // vConcepto
        public string Operadora { get; set; } = string.Empty;      // vOperadora
        public string Plan { get; set; } = string.Empty;           // vPlan
        public string OficinaTitular { get; set; } = string.Empty; // OficinaTitular
        public string OperadoraText { get; set; } = string.Empty;  // OperadoraText
        public string Descripcion { get; set; } = string.Empty;    // txtDescripcion
        public string Destino { get; set; } = string.Empty;        // txtDestino
    }

    public class FndAhorroDetalladoResumen
    {
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
    }

    public class FndAhorroDetalladoResumenRequest
    {
        public string TipoDoc { get; set; } = string.Empty; // vTipoDoc
        public string NC_Pat { get; set; } = string.Empty;  // vNC_Pat
    }

    public class ParAfahCuentasResult
    {
        public string? Cta_Custodia { get; set; }
        public string? Cta_Obrero { get; set; }
        public string? Cta_Patronal { get; set; }
        public string? Cta_Capitaliza { get; set; }
        public string? Cta_Devoluciones { get; set; }
    }
}