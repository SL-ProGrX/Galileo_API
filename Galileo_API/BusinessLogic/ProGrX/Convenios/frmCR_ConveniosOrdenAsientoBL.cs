using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosOrdenAsientoBl
    {
        private readonly FrmCrConveniosOrdenAsientoDb _db;

        /// <summary>
        /// Valor que spConvenios_Orden_Asiento devuelve en COD_FACTURA cuando la orden está abierta.
        /// </summary>
        private const string FacturaPendiente = "(Pendiente)";

        public FrmCrConveniosOrdenAsientoBl(IConfiguration config)
        {
            _db = new FrmCrConveniosOrdenAsientoDb(config);
        }

        public ErrorDto<CrOrdenAsientoConsulta> CrConveniosOrdenAsiento_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            var respuesta = _db.CrConveniosOrdenAsiento_Obtener(codEmpresa, codConvenio, codOrden);
            if ((respuesta.Code ?? 0) != 0)
                return DbHelper.CreateErrorResponse(respuesta.Description ?? string.Empty, respuesta.Code ?? -1, new CrOrdenAsientoConsulta());

            var lineas = respuesta.Result ?? new List<CrOrdenAsientoLinea>();

            var consulta = new CrOrdenAsientoConsulta
            {
                lineas = lineas,
                total_debitos = lineas.Sum(l => l.monto_debito ?? 0m),
                total_creditos = lineas.Sum(l => l.monto_credito ?? 0m),
                estado = ObtenerEstado(lineas)
            };

            return DbHelper.CreateOkResponse(consulta);
        }

        /// <summary>
        /// Equivalente a lblEstado del VB6: el VB6 evalúa cada fila y deja el resultado de la última.
        /// Sin filas, el VB6 no cambia el texto "(Estado)"; aquí se devuelve null.
        /// </summary>
        private static string? ObtenerEstado(List<CrOrdenAsientoLinea> lineas)
        {
            if (lineas.Count == 0)
                return null;

            return lineas[^1].cod_factura == FacturaPendiente ? "Abierta" : "Cerrada";
        }
    }
}
