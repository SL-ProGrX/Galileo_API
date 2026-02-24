using System.Data;

namespace Galileo_API.Models.ProGrX_Polizas
{
    public sealed class CrdPolizasCargaLoteCuentaCatalogoRequest
    {
        public string CodAseguradora { get; set; } = string.Empty;
        public int IdBanco { get; set; } = 0;
    }

    public sealed class CrdPolizasCargaLoteCargaRequest
    {
        public string CodigoCliente { get; set; } = string.Empty;     // cboCliente.ItemData
        public string CodAseguradora { get; set; } = string.Empty;    // cboAseguradora.ItemData
        public long Proceso { get; set; } = 0;                           // cboPrideduc.Text (yyyymm)
        public List<CrdPolizasCargaLoteGridItem> Items { get; set; } = new();
    }

    public sealed class CrdPolizasCargaLoteGridItem
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Monto { get; set; } = 0;
        public int Plazo { get; set; } = 0;
        public decimal Tasa { get; set; } = 0;
        public decimal Cuota { get; set; } = 0;       
        public decimal Comision { get; set; } = 0;
    }

    public sealed class CrdPolizasCargaLoteCargaResponse
    {
        public List<CrdPolizasCargaLoteGridItem> Grid { get; set; } = new();
        public decimal TotalMonto { get; set; } = 0;
        public decimal TotalComision { get; set; } = 0;
        public decimal TotalNeto { get; set; } = 0;
        public int LineasCargadas { get; set; } = 0;
    }

    public sealed class CrdPolizasCargaLoteProcesarRequest
    {
        public string CodigoCliente { get; set; } = string.Empty;     // cboCliente.ItemData
        public string CodigoConfirma { get; set; } = string.Empty;    // cboConfirma.ItemData
        public string CodAseguradora { get; set; } = string.Empty;    // cboAseguradora.ItemData
        public long Proceso { get; set; } = 0;                            // cboPrideduc.Text (yyyymm)
        public int IdBanco { get; set; } = 0;                             // cboBanco.ItemData
        public string CuentaAhorros { get; set; } = string.Empty;     // cboCuenta.ItemData (o texto, depende tu SP)
        public string TipoDocumentoUi { get; set; } = "TE"; // "CK" | "TE"
        public decimal MontoNeto { get; set; } = 0;                      // txtNeto
        public int Ops { get; set; } = 0;                           // vGrid.MaxRows
        public string AseguradoraNombre { get; set; } = string.Empty; // cboAseguradora.Text (beneficiario)
    }

    public class RegistrarDocumentoRequest
    {
        public IDbConnection? Conn { get; set; }

        public string TipoDocumento { get; set; } = string.Empty;

        public int IdBanco { get; set; } = 0;

        public decimal Monto { get; set; } = 0;

        public string Codigo { get; set; } = string.Empty;

        public string Beneficiario { get; set; } = string.Empty;

        public string CodigoCliente { get; set; } = string.Empty;

        public string CtaAhorros { get; set; } = string.Empty;

        public string Detalle1 { get; set; } = string.Empty;

        public string Detalle2 { get; set; } = string.Empty;

        public DateTime? Fecha { get; set; }

        public string Unidad { get; set; } = string.Empty;

        public string Concepto { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;
    }

}
