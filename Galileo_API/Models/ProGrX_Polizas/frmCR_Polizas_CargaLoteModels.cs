namespace Galileo_API.Models.ProGrX_Polizas
{
    public sealed class CrdPolizasCargaLoteCuentaCatalogoRequest
    {
        public string CodAseguradora { get; set; } = string.Empty;
        public int IdBanco { get; set; }
    }

    public sealed class CrdPolizasCargaLote_CargaRequest
    {
        public string CodigoCliente { get; set; } = string.Empty;     // cboCliente.ItemData
        public string CodAseguradora { get; set; } = string.Empty;    // cboAseguradora.ItemData
        public long Proceso { get; set; }                             // cboPrideduc.Text (yyyymm)
        public List<CrdPolizasCargaLoteGridItem> Items { get; set; } = new();
    }
    public sealed class CrdPolizasCargaLoteGridItem
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public int Plazo { get; set; }
        public decimal Tasa { get; set; }
        public decimal Cuota { get; set; }        // En carga puede venir 0; el SP la recalcula en la revisión
        public decimal Comision { get; set; }
    }

    public sealed class CrdPolizasCargaLote_CargaResponse
    {
        public List<CrdPolizasCargaLoteGridItem> Grid { get; set; } = new();
        public decimal TotalMonto { get; set; }
        public decimal TotalComision { get; set; }
        public decimal TotalNeto { get; set; }
        public int LineasCargadas { get; set; }
    }

    public sealed class CrdPolizasCargaLote_ProcesarRequest
    {
        public string CodigoCliente { get; set; } = string.Empty;     // cboCliente.ItemData
        public string CodigoConfirma { get; set; } = string.Empty;    // cboConfirma.ItemData
        public string CodAseguradora { get; set; } = string.Empty;    // cboAseguradora.ItemData
        public long Proceso { get; set; }                             // cboPrideduc.Text (yyyymm)
        public int IdBanco { get; set; }                              // cboBanco.ItemData
        public string CuentaAhorros { get; set; } = string.Empty;     // cboCuenta.ItemData (o texto, depende tu SP)
        public string TipoDocumentoUi { get; set; } = "TRANSFERENCIA"; // "CHEQUE" | "TRANSFERENCIA"
        public decimal MontoNeto { get; set; }                        // txtNeto
        public int Ops { get; set; }                                  // vGrid.MaxRows
        public string AseguradoraNombre { get; set; } = string.Empty; // cboAseguradora.Text (beneficiario)
    }

    public sealed class CrdPolizasCargaLote_ProcesarResponse
    {
        public long NSolicitud { get; set; }
    }

}
