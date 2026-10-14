using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosConsultasBl
    {
        private readonly FrmCrConveniosConsultasDb _db;

        public FrmCrConveniosConsultasBl(IConfiguration config)
        {
            _db = new FrmCrConveniosConsultasDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrConveniosConsultas_Tipos_Obtener(int codEmpresa)
            => _db.CrConveniosConsultas_Tipos_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrConveniosConsultas_Usuarios_Obtener(int codEmpresa)
            => _db.CrConveniosConsultas_Usuarios_Obtener(codEmpresa);

        public ErrorDto<List<CrConvenioConsultaData>> CrConveniosConsultas_Convenios_Obtener(int codEmpresa)
            => _db.CrConveniosConsultas_Convenios_Obtener(codEmpresa);

        public ErrorDto<CrConvenioConsultaData?> CrConveniosConsultas_Convenio_Obtener(
            int codEmpresa,
            string? codConvenio,
            string? descripcion)
            => _db.CrConveniosConsultas_Convenio_Obtener(codEmpresa, codConvenio, descripcion);

        /// <summary>
        /// Consulta las órdenes y calcula los totales de la barra de estado (Casos, Total Pagar, Total Orden, Total Deducciones).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CrConvenioOrdenesConsulta> CrConveniosConsultas_Ordenes_Obtener(
            int codEmpresa,
            CrConvenioConsultaFiltros filtros)
        {
            var resp = _db.CrConveniosConsultas_Ordenes_Obtener(codEmpresa, filtros ?? new CrConvenioConsultaFiltros());

            if (resp.Code < 0)
                return DbHelper.CreateErrorResponse<CrConvenioOrdenesConsulta>(resp.Description ?? string.Empty);

            var lista = resp.Result ?? new List<CrConvenioOrdenData>();

            return DbHelper.CreateOkResponse(new CrConvenioOrdenesConsulta
            {
                casos = lista.Count,
                total_pagar = lista.Sum(o => o.total_pagar ?? 0),
                total_orden = lista.Sum(o => o.total_orden ?? 0),
                total_deducciones = lista.Sum(o => o.total_deducciones ?? 0),
                lista = lista
            });
        }

        public ErrorDto<CrConvenioDetallePagoData?> CrConveniosConsultas_DetallePago_Obtener(
            int codEmpresa,
            string codFactura)
        {
            if (string.IsNullOrWhiteSpace(codFactura))
            {
                return DbHelper.CreateErrorResponse<CrConvenioDetallePagoData?>("Debe indicar la factura de la orden.");
            }

            return _db.CrConveniosConsultas_DetallePago_Obtener(codEmpresa, codFactura.Trim());
        }
    }
}
