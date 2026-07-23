using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public sealed class CntXAsientosInvData
    {
        public int cod_contabilidad { get; set; } = 0;

        public string num_asiento { get; set; } = string.Empty;

        public int anio { get; set; } = 0;

        public int mes { get; set; } = 0;

        public DateTime fecha_asiento { get; set; } = DateTime.Now;

        public string descripcion { get; set; } = string.Empty;

        public string notas { get; set; } = string.Empty;
    }

    public sealed class CntXAsientosInvDetalleData
    {
        public string cod_cuenta { get; set; } = string.Empty;

        public string cod_cuenta_mask { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;

        public string documento { get; set; } = string.Empty;

        public string detalle { get; set; } = string.Empty;

        public decimal monto_debito { get; set; } = 0;

        public decimal monto_credito { get; set; } = 0;

        public int num_linea { get; set; } = 0;
    }

    public sealed class CntXAsientosInvResponse
    {
        public CntXAsientosInvData asiento { get; set; } =  new();

        public List<CntXAsientosInvDetalleData> detalle { get; set; } = [];
    }

    public sealed class CntXAsientosInvCuentaData
    {
        public string cod_cuenta { get; set; } = string.Empty;

        public string cod_cuenta_mask { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class CntXAsientosInvGuardarRequest
    {
        public bool edita { get; set; } = false;

        public string usuario { get; set; } = string.Empty;

        public CntXAsientosInvData asiento { get; set; } = new();

        public List<CntXAsientosInvDetalleData> detalle { get; set; } = [];
    }

    public sealed class CntXAsientosInvEliminarRequest
    {
        public int cod_contabilidad { get; set; } = 0;

        public string num_asiento { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;
    }

    public sealed class CntXAsientosInvListaRequest
    {
        public int cod_contabilidad { get; set; } = 0;

        public int anio { get; set; } = 0;

        public int mes { get; set; } = 0;
    }
}