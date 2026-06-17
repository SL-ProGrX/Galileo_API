using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Pasivos
{
    public class FrmCrApaOperacionesBL
    {
        private readonly FrmCrApaOperacionesDB _db;

        public FrmCrApaOperacionesBL(IConfiguration config)
        {
            _db = new FrmCrApaOperacionesDB(config);
        }

        /// <summary>
        /// Coordina la consulta de acreedores activos para el panel lateral APA.
        /// </summary>
        public ErrorDto<List<FrmCrApaOperacionAcreedorDto>> CR_APA_Operaciones_Acreedores_Obtener(int codEmpresa)
        {
            return _db.CR_APA_Operaciones_Acreedores_Obtener(codEmpresa);
        }

        /// <summary>
        /// Coordina la consulta de contactos del acreedor seleccionado.
        /// </summary>
        public ErrorDto<List<FrmCrApaOperacionContactoDto>> CR_APA_Operaciones_Contactos_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _db.CR_APA_Operaciones_Contactos_Obtener(codEmpresa, cod_acreedor);
        }

        /// <summary>
        /// Coordina la consulta lazy de operaciones APA por acreedor, estado y operación.
        /// </summary>
        public ErrorDto<FrmCrApaOperacionListaDto> CR_APA_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            string estado,
            string filtro)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro)
                          ?? new FiltrosLazyLoadData();

            return _db.CR_APA_Operaciones_Obtener(codEmpresa, cod_acreedor, operacion, estado, filtros);
        }

        /// <summary>
        /// Coordina la obtención del detalle de una operación APA.
        /// </summary>
        public ErrorDto<FrmCrApaOperacionDatosDto> CR_APA_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _db.CR_APA_Operacion_Obtener(codEmpresa, cod_acreedor, operacion);
        }

        /// <summary>
        /// Coordina el registro de una operación APA nueva.
        /// </summary>
        public ErrorDto<int> CR_APA_Operacion_Insertar(
            int codEmpresa,
            FrmCrApaOperacionGuardarRequest request)
        {
            return _db.CR_APA_Operacion_Insertar(codEmpresa, request);
        }

        /// <summary>
        /// Coordina la actualización de una operación APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Operacion_Actualizar(
            int codEmpresa,
            FrmCrApaOperacionGuardarRequest request)
        {
            return _db.CR_APA_Operacion_Actualizar(codEmpresa, request);
        }

        /// <summary>
        /// Coordina el cierre de una operación APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Operacion_Cerrar(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _db.CR_APA_Operacion_Cerrar(codEmpresa, cod_acreedor, operacion);
        }

        /// <summary>
        /// Coordina la consulta de cantidad de pagos asociados a una operación APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Operacion_PagosCantidad(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _db.CR_APA_Operacion_PagosCantidad(codEmpresa, cod_acreedor, operacion);
        }

        /// <summary>
        /// Coordina la consulta de líneas activas del acreedor.
        /// </summary>
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Lineas_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _db.CR_APA_Operaciones_Lineas_Obtener(codEmpresa, cod_acreedor);
        }

        /// <summary>
        /// Coordina la consulta de oficinas disponibles para operaciones APA.
        /// </summary>
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Oficinas_Obtener(int codEmpresa)
        {
            return _db.CR_APA_Operaciones_Oficinas_Obtener(codEmpresa);
        }

        /// <summary>
        /// Coordina la consulta de divisas disponibles para operaciones APA.
        /// </summary>
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Divisas_Obtener(int codEmpresa)
        {
            return _db.CR_APA_Operaciones_Divisas_Obtener(codEmpresa);
        }

        /// <summary>
        /// Coordina la consulta lazy de pagos APA por operación, fechas y estado.
        /// </summary>
        public ErrorDto<FrmCrApaOperacionPagoListaDto> CR_APA_Operaciones_Pagos_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            string estado,
            DateTime? fecha_desde,
            DateTime? fecha_hasta,
            string filtro)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro)
                          ?? new FiltrosLazyLoadData();

            return _db.CR_APA_Operaciones_Pagos_Obtener(
                codEmpresa,
                cod_acreedor,
                operacion,
                estado,
                fecha_desde,
                fecha_hasta,
                filtros);
        }

        /// <summary>
        /// Coordina la obtención del detalle de un pago APA.
        /// </summary>
        public ErrorDto<FrmCrApaOperacionPagoDatosDto> CR_APA_Operaciones_Pago_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            int npago)
        {
            return _db.CR_APA_Operaciones_Pago_Obtener(codEmpresa, cod_acreedor, operacion, npago);
        }

        /// <summary>
        /// Coordina la obtención de último saldo, tasa y cuota para cálculo de pago.
        /// </summary>
        public ErrorDto<FrmCrApaOperacionUltimoPagoDto> CR_APA_Operaciones_UltimoPago_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            int? npago)
        {
            return _db.CR_APA_Operaciones_UltimoPago_Obtener(codEmpresa, cod_acreedor, operacion, npago);
        }

        /// <summary>
        /// Coordina el registro de un nuevo pago APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Operaciones_Pago_Insertar(
            int codEmpresa,
            FrmCrApaOperacionPagoGuardarRequest request)
        {
            return _db.CR_APA_Operaciones_Pago_Insertar(codEmpresa, request);
        }

        /// <summary>
        /// Coordina la consulta de autorizados disponibles para un acreedor.
        /// </summary>
        public ErrorDto<List<FrmCrApaOperacionAutorizadoDto>> CR_APA_Operaciones_Autorizados_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _db.CR_APA_Operaciones_Autorizados_Obtener(codEmpresa, cod_acreedor);
        }

        /// <summary>
        /// Coordina la asignación o limpieza del autorizado de un pago APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Operaciones_Pago_Autorizado_Actualizar(
            int codEmpresa,
            FrmCrApaOperacionAsignarAutorizadoRequest request)
        {
            return _db.CR_APA_Operaciones_Pago_Autorizado_Actualizar(codEmpresa, request);
        }
    }
}
