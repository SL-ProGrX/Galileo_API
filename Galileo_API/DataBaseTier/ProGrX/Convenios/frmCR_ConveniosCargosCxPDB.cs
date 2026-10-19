using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    /// <summary>
    /// Acceso a datos de frmCR_ConveniosCargosCxP (rebajos vía cargos de CxP de la orden de liquidación).
    /// </summary>
    public class FrmCrConveniosCargosCxPDb
    {
        private readonly PortalDB _portalDb;

        public const string AccionAgregar = "A";
        public const string AccionEditar = "E";
        public const string AccionBorrar = "B";

        private const string SqlRegistro = @"
            exec spConvenios_Orden_Cargos_CxP_Registro
                @CodConvenio, @CodOrden, @Linea, @Transaccion, @CodCargo, @Monto, @Documento, @Detalle, @Usuario, @Accion";

        public FrmCrConveniosCargosCxPDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Encabezado de la orden y disponible (sbConsultaConvenio).
        /// </summary>
        public ErrorDto<CrCargosCxPCabecera?> CrConveniosCargosCxP_Cabecera_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            const string sql = @"
                select
                    rtrim(O.COD_CONVENIO) as cod_convenio,
                    rtrim(isnull(C.DESCRIPCION, '')) as descripcion,
                    O.COD_ORDEN as cod_orden,
                    rtrim(isnull(O.ESTADO, '')) as estado,
                    O.TOTAL_PAGAR + O.TOTAL_REBAJOS_CXP as disponible_bruto,
                    dbo.fxConvenio_CxP_CargosFlotantes(@CodConvenio) as cxp_flotante,
                    dbo.fxConvenio_CxP_Rebajos_Vencidos(@CodConvenio, @CodOrden) as rebajo_vencido
                from CRD_CONVENIOS_ORDENES O
                    inner join CRD_CONVENIOS C on O.COD_CONVENIO = C.COD_CONVENIO
                where O.COD_CONVENIO = @CodConvenio and O.COD_ORDEN = @CodOrden;";

            var respuesta = DbHelper.ExecuteSingleQuery<CrCargosCxPCabecera>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { CodConvenio = codConvenio.Trim(), CodOrden = codOrden });

            if (respuesta.Result != null)
            {
                var cabecera = respuesta.Result;
                cabecera.disponible = (cabecera.disponible_bruto ?? 0m) - (cabecera.cxp_flotante ?? 0m);
                cabecera.estado_desc = cabecera.estado switch
                {
                    "A" => "Abierta",
                    "C" => "Cerrada",
                    "T" => "Tramitada",
                    _ => string.Empty
                };
            }

            return respuesta;
        }

        /// <summary>
        /// Rebajos registrados a la orden (sbConsultaCargos: spConvenios_Orden_Cargos_CxP).
        /// </summary>
        public ErrorDto<List<CrCargosCxPLinea>> CrConveniosCargosCxP_Lineas_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            return DbHelper.ExecuteListQuery<CrCargosCxPLinea>(
                _portalDb,
                codEmpresa,
                "exec spConvenios_Orden_Cargos_CxP @CodConvenio, @CodOrden, 1",
                new { CodConvenio = codConvenio.Trim(), CodOrden = codOrden });
        }

        /// <summary>
        /// Rebajos vencidos de la orden (fxRebajoVencido): el VB6 devuelve 0 ante cualquier error.
        /// </summary>
        public decimal CrConveniosCargosCxP_RebajoVencido_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                return conn.QueryFirstOrDefault<decimal?>(
                    "select dbo.fxConvenio_CxP_Rebajos_Vencidos(@CodConvenio, @CodOrden);",
                    new { CodConvenio = codConvenio.Trim(), CodOrden = codOrden }) ?? 0m;
            }
            catch (Exception)
            {
                return 0m;
            }
        }

        /// <summary>
        /// Cargos pendientes de cobro (sbConsulta_EnCobro: spConvenios_Rebajos_Consulta_Pendientes).
        /// </summary>
        public ErrorDto<List<CrCargosCxPPendiente>> CrConveniosCargosCxP_Pendientes_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            return DbHelper.ExecuteListQuery<CrCargosCxPPendiente>(
                _portalDb,
                codEmpresa,
                "exec spConvenios_Rebajos_Consulta_Pendientes @CodConvenio, Null, @CodOrden",
                new { CodConvenio = codConvenio.Trim(), CodOrden = codOrden });
        }

        /// <summary>
        /// Cargos activos de CxP que el convenio permite rebajar (búsqueda F4 de txtCargoCod / txtCargoDesc).
        /// </summary>
        public ErrorDto<List<CrCargosCxPCargo>> CrConveniosCargosCxP_Cargos_Obtener(
            int codEmpresa,
            string codConvenio)
        {
            const string sql = @"
                select
                    rtrim(Ca.COD_CARGO) as cod_cargo,
                    rtrim(isnull(Ca.DESCRIPCION, '')) as descripcion
                from CRD_CONVENIOS_CARGOS_CXP Cc
                    inner join CXP_CARGOS Ca on Cc.COD_CARGO = Ca.COD_CARGO
                where Cc.COD_CONVENIO = @CodConvenio and Ca.ACTIVO = 1
                order by Ca.COD_CARGO;";

            return DbHelper.ExecuteListQuery<CrCargosCxPCargo>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodConvenio = codConvenio.Trim() });
        }

        /// <summary>
        /// Alta de un rebajo (btnGuardar_Click, acción 'A').
        /// </summary>
        public ErrorDto CrConveniosCargosCxP_Registrar(int codEmpresa, CrCargosCxPRegistroRequest request)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                SqlRegistro,
                new
                {
                    CodConvenio = request.cod_convenio,
                    CodOrden = request.cod_orden,
                    Linea = 0,
                    Transaccion = request.id_transac,
                    CodCargo = request.cod_cargo,
                    request.monto,
                    request.documento,
                    request.detalle,
                    request.usuario,
                    Accion = AccionAgregar
                });
        }

        /// <summary>
        /// Edición de una línea (vGrid_KeyDown, acción 'E'); devuelve el monto que registra el SP.
        /// </summary>
        public ErrorDto<decimal?> CrConveniosCargosCxP_Editar(int codEmpresa, CrCargosCxPEdicionRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var resultado = conn.QueryFirstOrDefault<CrCargosCxPEdicionResultado>(
                    SqlRegistro,
                    new
                    {
                        CodConvenio = request.cod_convenio,
                        CodOrden = request.cod_orden,
                        Linea = request.num_linea,
                        Transaccion = request.id_transac,
                        CodCargo = request.cod_cargo,
                        request.monto,
                        request.documento,
                        request.detalle,
                        request.usuario,
                        Accion = AccionEditar
                    });

                return DbHelper.CreateOkResponse(resultado?.monto);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<decimal?>(ex.Message);
            }
        }

        /// <summary>
        /// Borrado de una línea (vGrid_ButtonClicked, acción 'B').
        /// </summary>
        public ErrorDto CrConveniosCargosCxP_Borrar(int codEmpresa, CrCargosCxPBorradoRequest request)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                SqlRegistro,
                new
                {
                    CodConvenio = request.cod_convenio,
                    CodOrden = request.cod_orden,
                    Linea = request.num_linea,
                    Transaccion = 0,
                    CodCargo = string.Empty,
                    Monto = 0m,
                    Documento = string.Empty,
                    Detalle = string.Empty,
                    request.usuario,
                    Accion = AccionBorrar
                });
        }
    }
}
