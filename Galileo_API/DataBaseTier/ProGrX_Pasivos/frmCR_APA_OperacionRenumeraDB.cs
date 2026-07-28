using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public class FrmCrApaOperacionRenumeraDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMain;

        private const string MsgDebeAcreedor = "Debe indicar el acreedor.";
        private const string MsgDebeOperacionActual = "Debe indicar la operaci&oacute;n actual.";
        private const string MsgDebeOperacionNueva = "Debe indicar la nueva operaci&oacute;n.";
        private const string MsgNoExisteAcreedor = "No se encontr&oacute; informaci&oacute;n del acreedor.";
        private const string MsgNoExisteOperacion = "No se encontr&oacute; informaci&oacute;n de la operaci&oacute;n.";
        private const string MsgOperacionNuevaIgual = "La nueva operaci&oacute;n no puede ser igual a la operaci&oacute;n actual.";
        private const string MsgSolicitudRequerida = "No se recibi&oacute; la solicitud de renumeraci&oacute;n.";
        private const string MsgErrorAplicar = "No fue posible realizar la renumeraci&oacute;n de la operaci&oacute;n.";
        private const string MsgOkAplicar = "Renumeraci&oacute;n realizada satisfactoriamente!";
        private readonly int vModulo = 14;

        public FrmCrApaOperacionRenumeraDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mSecurityMain = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de acreedores.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_OperacionRenumera_Acreedores_Obtener(
            int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                @"
                select
                    isnull(cod_acreedor, '') as item,
                    isnull(descripcion, '') as descripcion
                from CRD_APA_ACREEDORES
                order by cod_acreedor;",
                null);
        }

        /// <summary>
        /// Consulta la informacion del acreedor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel?> CR_APA_OperacionRenumera_Acreedor_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            var response = DbHelper.ExecuteSingleQuery<DropDownListaGenericaModel?>(
                _portalDb,
                codEmpresa,
                @"
                select top 1
                    isnull(cod_acreedor, '') as item,
                    isnull(descripcion, '') as descripcion
                from CRD_APA_ACREEDORES
                where cod_acreedor = @Acreedor;",
                null,
                new
                {
                    Acreedor = cod_acreedor.Trim()
                });

            return response.Code == 0 && response.Result is null
                ? DbHelper.CreateErrorResponse<DropDownListaGenericaModel?>(
                    MsgNoExisteAcreedor,
                    -2,
                    null)
                : response;
        }

        /// <summary>
        /// Consulta la operacion actual del acreedor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaOperacionRenumeraOperacionDto?> CR_APA_OperacionRenumera_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            var validacion = ValidarConsultaOperacion(cod_acreedor, operacion);
            if (validacion is not null)
            {
                return validacion;
            }

            var response = DbHelper.ExecuteSingleQuery<FrmCrApaOperacionRenumeraOperacionDto?>(
                _portalDb,
                codEmpresa,
                @"
                select top 1
                    isnull(operacion, '') as operacion,
                    isnull(monto, 0) as monto,
                    isnull(saldo, 0) as saldo,
                    fecha_formaliza
                from CRD_APA_OPERACIONES
                where cod_acreedor = @Acreedor
                  and operacion = @Operacion;",
                null,
                new
                {
                    Acreedor = cod_acreedor.Trim(),
                    Operacion = operacion.Trim()
                });

            return response.Code == 0 && response.Result is null
                ? CrearErrorOperacionNoExiste()
                : response;
        }

        /// <summary>
        /// Lista operaciones del acreedor para busqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaOperacionRenumeraOperacionDto>> CR_APA_OperacionRenumera_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            if (string.IsNullOrWhiteSpace(cod_acreedor))
            {
                return DbHelper.CreateErrorResponse(
                    MsgDebeAcreedor,
                    -2,
                    new List<FrmCrApaOperacionRenumeraOperacionDto>());
            }

            return DbHelper.ExecuteListQuery<FrmCrApaOperacionRenumeraOperacionDto>(
                _portalDb,
                codEmpresa,
                @"
                select
                    isnull(operacion, '') as operacion,
                    isnull(monto, 0) as monto,
                    isnull(saldo, 0) as saldo,
                    fecha_formaliza
                from CRD_APA_OPERACIONES
                where cod_acreedor = @Acreedor
                order by operacion;",
                new
                {
                    Acreedor = cod_acreedor.Trim()
                });
        }

        /// <summary>
        /// Ejecuta la renumeracion de una operacion APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaOperacionRenumeraResultadoDto> CR_APA_OperacionRenumera_Aplicar(
            int codEmpresa,
            FrmCrApaOperacionRenumeraAplicarRequest request)
        {
            var validacion = ValidarAplicacion(request);
            if (validacion is not null)
            {
                return validacion;
            }

            var existeOperacion = CR_APA_OperacionRenumera_Operacion_Obtener(
                codEmpresa,
                request.cod_acreedor,
                request.operacion);

            if (existeOperacion.Code != 0)
            {
                return CrearErrorResultado(
                    existeOperacion.Description ?? MsgNoExisteOperacion,
                    existeOperacion.Code ?? -2);
            }

            var response = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"
                exec spAPA_OperacionRenumera
                    @Acreedor,
                    @OperacionActual,
                    @OperacionNueva;",
                new
                {
                    Acreedor = request.cod_acreedor.Trim(),
                    OperacionActual = request.operacion.Trim(),
                    OperacionNueva = request.operacion_nueva.Trim()
                });

            RegistrarBitacora(
                codEmpresa,
                (request.usuario ?? string.Empty).Trim(),
                "Aplica - WEB",
                $"Renumeracion de Operación.: {request.operacion} -> {request.operacion_nueva}");

            if (response.Code != 0)
            {
                string mensajeError = string.IsNullOrWhiteSpace(response.Description)
                    ? MsgErrorAplicar
                    : response.Description;

                return CrearErrorResultado(
                    mensajeError,
                    response.Code ?? -1);
            }

            return DbHelper.CreateOkResponse(
                new FrmCrApaOperacionRenumeraResultadoDto
                {
                    mensaje = MsgOkAplicar
                });
        }

        private static ErrorDto<FrmCrApaOperacionRenumeraOperacionDto?>? ValidarConsultaOperacion(
            string cod_acreedor,
            string operacion)
        {
            if (string.IsNullOrWhiteSpace(cod_acreedor))
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionRenumeraOperacionDto?>(
                    MsgDebeAcreedor,
                    -2,
                    null);
            }

            if (string.IsNullOrWhiteSpace(operacion))
            {
                return DbHelper.CreateErrorResponse<FrmCrApaOperacionRenumeraOperacionDto?>(
                    MsgDebeOperacionActual,
                    -2,
                    null);
            }

            return null;
        }

        private static ErrorDto<FrmCrApaOperacionRenumeraResultadoDto>? ValidarAplicacion(
            FrmCrApaOperacionRenumeraAplicarRequest? request)
        {
            if (request is null)
            {
                return CrearErrorResultado(MsgSolicitudRequerida, -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_acreedor))
            {
                return CrearErrorResultado(MsgDebeAcreedor, -2);
            }

            if (string.IsNullOrWhiteSpace(request.operacion))
            {
                return CrearErrorResultado(MsgDebeOperacionActual, -2);
            }

            if (string.IsNullOrWhiteSpace(request.operacion_nueva))
            {
                return CrearErrorResultado(MsgDebeOperacionNueva, -2);
            }

            return string.Equals(
                request.operacion.Trim(),
                request.operacion_nueva.Trim(),
                StringComparison.OrdinalIgnoreCase)
                ? CrearErrorResultado(MsgOperacionNuevaIgual, -2)
                : null;
        }

        private static ErrorDto<FrmCrApaOperacionRenumeraOperacionDto?> CrearErrorOperacionNoExiste()
        {
            return DbHelper.CreateErrorResponse<FrmCrApaOperacionRenumeraOperacionDto?>(
                MsgNoExisteOperacion,
                -2,
                null);
        }

        private static ErrorDto<FrmCrApaOperacionRenumeraResultadoDto> CrearErrorResultado(
            string descripcion,
            int code)
        {
            return DbHelper.CreateErrorResponse(
                descripcion,
                code,
                new FrmCrApaOperacionRenumeraResultadoDto());
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMain.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}