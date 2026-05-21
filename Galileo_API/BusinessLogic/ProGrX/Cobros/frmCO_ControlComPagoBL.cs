using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoControlComPagoBL
    {
        private readonly FrmCoControlComPagoDB _db;

        public FrmCoControlComPagoBL(IConfiguration config)
        {
            _db = new FrmCoControlComPagoDB(config);
        }

        /// <summary>
        /// Consulta las ultimas remesas registradas para el control de pago de comisiones.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="top">Cantidad maxima de remesas a retornar.</param>
        public ErrorDto<List<CoControlComPagoRemesaData>> CO_ControlComPago_Remesas_Obtener(int CodEmpresa, int top)
        {
            return _db.CO_ControlComPago_Remesas_Obtener(CodEmpresa, top);
        }

        /// <summary>
        /// Consulta una remesa por su identificador.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa.</param>
        public ErrorDto<CoControlComPagoRemesaData> CO_ControlComPago_Remesa_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _db.CO_ControlComPago_Remesa_Obtener(CodEmpresa, cod_remesa);
        }

        /// <summary>
        /// Inserta o actualiza la remesa de pago de comisiones.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se registra la remesa.</param>
        /// <param name="usuario">Usuario que ejecuta el registro o modificacion.</param>
        /// <param name="request">Datos de la remesa a registrar o modificar.</param>
        public ErrorDto<int> CO_ControlComPago_Remesa_Guardar(int CodEmpresa, string usuario, CoControlComPagoRemesaGuardarRequest request)
        {
            return _db.CO_ControlComPago_Remesa_Guardar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Elimina una remesa abierta.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se elimina la remesa.</param>
        /// <param name="usuario">Usuario que ejecuta la eliminacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa abierta.</param>
        public ErrorDto CO_ControlComPago_Remesa_Eliminar(int CodEmpresa, string usuario, int cod_remesa)
        {
            return _db.CO_ControlComPago_Remesa_Eliminar(CodEmpresa, usuario, cod_remesa);
        }

        /// <summary>
        /// Consulta remesas por estado para los combos del proceso.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="estado">Estado funcional de la remesa.</param>
        public ErrorDto<List<CoControlComPagoRemesaComboData>> CO_ControlComPago_RemesasPorEstado_Obtener(int CodEmpresa, string estado)
        {
            return _db.CO_ControlComPago_RemesasPorEstado_Obtener(CodEmpresa, estado);
        }

        /// <summary>
        /// Consulta las cuentas bancarias disponibles para una remesa abierta.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa que define el rango de corte.</param>
        public ErrorDto<List<CoControlComPagoBancoData>> CO_ControlComPago_CargaBancos_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _db.CO_ControlComPago_CargaBancos_Obtener(CodEmpresa, cod_remesa);
        }

        /// <summary>
        /// Consulta las oficinas disponibles para el panel de informes de remesas.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ControlComPago_ReportesOficinas_Obtener(int CodEmpresa)
        {
            return _db.CO_ControlComPago_ReportesOficinas_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Consulta usuarios pendientes de carga para la remesa seleccionada.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa que define el rango de corte.</param>
        /// <param name="id_banco">Banco usado como filtro opcional.</param>
        public ErrorDto<List<CoControlComPagoCargaData>> CO_ControlComPago_CargaPendientes_Obtener(int CodEmpresa, int cod_remesa, int? id_banco)
        {
            return _db.CO_ControlComPago_CargaPendientes_Obtener(CodEmpresa, cod_remesa, id_banco);
        }

        /// <summary>
        /// Carga en la remesa los usuarios seleccionados.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se aplica la carga.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="request">Datos de la remesa y usuarios seleccionados.</param>
        public ErrorDto<CoControlComPagoProcesoResult> CO_ControlComPago_Carga_Aplicar(int CodEmpresa, string usuario, CoControlComPagoCargaAplicarRequest request)
        {
            return _db.CO_ControlComPago_Carga_Aplicar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Cierra una remesa abierta o en proceso.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se cierra la remesa.</param>
        /// <param name="usuario">Usuario que ejecuta el cierre.</param>
        /// <param name="cod_remesa">Identificador de la remesa abierta o en proceso.</param>
        public ErrorDto CO_ControlComPago_Remesa_Cerrar(int CodEmpresa, string usuario, int cod_remesa)
        {
            return _db.CO_ControlComPago_Remesa_Cerrar(CodEmpresa, usuario, cod_remesa);
        }

        /// <summary>
        /// Consulta las operaciones pendientes de traslado a tesoreria para una remesa cerrada.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa cerrada.</param>
        public ErrorDto<List<CoControlComPagoTrasladoData>> CO_ControlComPago_TrasladoPendientes_Obtener(int CodEmpresa, int cod_remesa)
        {
            return _db.CO_ControlComPago_TrasladoPendientes_Obtener(CodEmpresa, cod_remesa);
        }

        /// <summary>
        /// Traslada a tesoreria los pagos de comision seleccionados.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se aplica el traslado.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="request">Datos de la remesa y usuarios seleccionados para trasladar.</param>
        public ErrorDto<CoControlComPagoProcesoResult> CO_ControlComPago_Traslado_Aplicar(int CodEmpresa, string usuario, CoControlComPagoTrasladoAplicarRequest request)
        {
            return _db.CO_ControlComPago_Traslado_Aplicar(CodEmpresa, usuario, request);
        }
    }
}
