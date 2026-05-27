using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrComisionesCatalogoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 3;
        private const string GuardadoExito = "Información guardada satisfactoriamente...";
        private const string EliminadoExito = "Información eliminada satisfactoriamente...";
        private const string ValIndicaComision = "Debe indicar la comisión.";


        public FrmCrComisionesCatalogoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        private static string Limpiar(string? valor)
            => valor?.Trim() ?? string.Empty;

        private static string CuentaSinFormato(string? cuenta)
        {
            if (string.IsNullOrWhiteSpace(cuenta))
                return string.Empty;

            return cuenta.Replace("-", string.Empty).Trim();
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Movimiento = movimiento,
                DetalleMovimiento = detalle,
                Modulo = VModulo
            });
        }

        private ErrorDto FinalizarRespuestaConBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle,
            ErrorDto response,
            string descripcionExitosa)
        {
            if (response.Code < 0)
                return response;

            RegistrarBitacora(codEmpresa, usuario, movimiento, detalle);

            return new ErrorDto
            {
                Code = 0,
                Description = descripcionExitosa
            };
        }

        #region consultas

        /// <summary>
        /// Obtiene el catálogo principal de comisiones.
        /// </summary>
        public ErrorDto<List<CrComisionesCatalogoData>> Cr_ComisionesCatalogo_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(cod_comision) as cod_comision,
                    rtrim(isnull(descripcion, '')) as descripcion,
                    fecha_inicio,
                    rtrim(isnull(base_calculo, '')) as base_calculo,
                    rtrim(isnull(cod_cuenta_mask, '')) as cod_cuenta_mask,
                    rtrim(isnull(cuenta_desc, '')) as cuenta_desc,
                    cast(isnull(activa, 0) as bit) as activa,
                    rtrim(isnull(registro_usuario, '')) as registro_usuario,
                    registro_fecha
                from vCrd_Comisiones_Catalogo
                order by cod_comision;";

            return DbHelper.ExecuteListQuery<CrComisionesCatalogoData>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene las líneas de porcentaje asociadas a una comisión.
        /// </summary>
        public ErrorDto<List<CrComisionesCatalogoPorcentajeData>> Cr_ComisionesCatalogo_Porcentajes_Obtener(
            int codEmpresa,
            CrComisionesCatalogoPorcentajesRequest request)
        {
            request.cod_comision = Limpiar(request.cod_comision);

            if (string.IsNullOrWhiteSpace(request.cod_comision))
            {
                return new ErrorDto<List<CrComisionesCatalogoPorcentajeData>>
                {
                    Code = -1,
                    Description = ValIndicaComision,
                    Result = []
                };
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var lista = connection.Query<CrComisionesCatalogoPorcentajeData>(
                    "spCrd_Comisiones_TP_Consulta",
                    new
                    {
                        cod_comision = request.cod_comision,
                        usuario = string.Empty
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return new ErrorDto<List<CrComisionesCatalogoPorcentajeData>>
                {
                    Code = 0,
                    Description = string.Empty,
                    Result = lista
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<CrComisionesCatalogoPorcentajeData>>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = []
                };
            }
        }

        /// <summary>
        /// Obtiene las líneas autorizadas de una comisión.
        /// </summary>
        public ErrorDto<List<CrComisionesCatalogoLineaData>> Cr_ComisionesCatalogo_Lineas_Obtener(
            int codEmpresa,
            CrComisionesCatalogoLineasRequest request)
        {
            request.cod_comision = Limpiar(request.cod_comision);

            if (string.IsNullOrWhiteSpace(request.cod_comision))
            {
                return new ErrorDto<List<CrComisionesCatalogoLineaData>>
                {
                    Code = -1,
                    Description = ValIndicaComision,
                    Result = []
                };
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var lista = connection.Query<CrComisionesCatalogoLineaData>(
                    "spCrd_Comisiones_Lineas_Asigna_Consulta",
                    new { cod_comision = request.cod_comision },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return new ErrorDto<List<CrComisionesCatalogoLineaData>>
                {
                    Code = 0,
                    Description = string.Empty,
                    Result = lista
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<CrComisionesCatalogoLineaData>>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = []
                };
            }
        }

        /// <summary>
        /// Obtiene la cuenta contable formateada para apoyo del Angular.
        /// </summary>
        public ErrorDto<CrComisionesCatalogoCuentaLookupData?> Cr_ComisionesCatalogo_Cuenta_Obtener(
            int codEmpresa,
            string cuenta)
        {
            string cuentaLimpia = CuentaSinFormato(cuenta);

            if (string.IsNullOrWhiteSpace(cuentaLimpia))
            {
                return new ErrorDto<CrComisionesCatalogoCuentaLookupData?>
                {
                    Code = -1,
                    Description = "Debe indicar la cuenta.",
                    Result = null
                };
            }

            const string sql = @"
                select top 1
                    rtrim(cod_cuenta) as cod_cuenta,
                    rtrim(cod_cuenta_mask) as cod_cuenta_mask,
                    rtrim(descripcion) as descripcion
                from vCNTX_CUENTAS_LOCAL
                where cod_cuenta = @Cuenta
                   or cod_cuenta_mask = @Cuenta;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                default(CrComisionesCatalogoCuentaLookupData),
                new { Cuenta = cuentaLimpia }
            );
        }

        #endregion guardar

        #region Guardar

        /// <summary>
        /// Guarda o actualiza una comisión del catálogo.
        /// </summary>
        public ErrorDto Cr_ComisionesCatalogo_Guardar(int codEmpresa, CrComisionesCatalogoGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.comision.cod_comision = Limpiar(request.comision.cod_comision);
            request.comision.descripcion = Limpiar(request.comision.descripcion);
            request.comision.base_calculo = Limpiar(request.comision.base_calculo);
            request.comision.cod_cuenta_mask = CuentaSinFormato(request.comision.cod_cuenta_mask);

            if (string.IsNullOrWhiteSpace(request.comision.descripcion))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la descripción de la comisión."
                };
            }

            if (request.comision.fecha_inicio is null)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la fecha de inicio."
                };
            }

            if (string.IsNullOrWhiteSpace(request.comision.cod_cuenta_mask))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la cuenta contable."
                };
            }

            if (!CuentaExiste(codEmpresa, request.comision.cod_cuenta_mask))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La cuenta contable no es válida."
                };
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var resultado = connection.QueryFirstOrDefault<dynamic>(
                    "spCrd_Comisiones_Cat_Registro",
                    new
                    {
                        pCodigo = request.comision.cod_comision,
                        pCodigoDesc = request.comision.descripcion,
                        pFechaInicio = request.comision.fecha_inicio.Value,
                        pBase = request.comision.base_calculo,
                        pCuenta = request.comision.cod_cuenta_mask,
                        pActivo = request.comision.activa ? 1 : 0,
                        pUsuario = request.usuario,
                        pAccion = "A"
                    },
                    commandType: CommandType.StoredProcedure
                );

                string codigoResultado = Convert.ToString(resultado?.Cod_Comision ?? request.comision.cod_comision) ?? request.comision.cod_comision;

                return FinalizarRespuestaConBitacora(
                    codEmpresa,
                    request.usuario,
                    "Registra - WEB",
                    $"Comisión de Crédito Id: {codigoResultado}",
                    new ErrorDto { Code = 0, Description = string.Empty },
                    GuardadoExito
                );
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Elimina una comisión del catálogo.
        /// </summary>
        public ErrorDto Cr_ComisionesCatalogo_Eliminar(int codEmpresa, CrComisionesCatalogoEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_comision = Limpiar(request.cod_comision);

            if (string.IsNullOrWhiteSpace(request.cod_comision))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ValIndicaComision
                };
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Execute(
                    "spCrd_Comisiones_Cat_Registro",
                    new
                    {
                        pCodigo = request.cod_comision,
                        pCodigoDesc = string.Empty,
                        pFechaInicio = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                        pBase = string.Empty,
                        pCuenta = string.Empty,
                        pActivo = 0,
                        pUsuario = request.usuario,
                        pAccion = "E"
                    },
                    commandType: CommandType.StoredProcedure
                );

                return FinalizarRespuestaConBitacora(
                    codEmpresa,
                    request.usuario,
                    "Elimina - WEB",
                    $"Comisión de Crédito Id: {request.cod_comision}",
                    new ErrorDto { Code = 0, Description = string.Empty },
                    EliminadoExito
                );
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Guarda o actualiza una línea de porcentajes.
        /// </summary>
        public ErrorDto Cr_ComisionesCatalogo_Porcentaje_Guardar(
            int codEmpresa,
            CrComisionesCatalogoPorcentajeGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_comision = Limpiar(request.cod_comision);

            if (string.IsNullOrWhiteSpace(request.cod_comision))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ValIndicaComision
                };
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var resultado = connection.QueryFirstOrDefault<dynamic>(
                    "spCrd_Comisiones_TP_Registra",
                    new
                    {
                        pCodComision = request.cod_comision,
                        pLinea = request.porcentaje.linea_id,
                        pInicio = request.porcentaje.inicio,
                        pCorte = request.porcentaje.corte,
                        pVenta = request.porcentaje.venta,
                        pFormalizacion = request.porcentaje.formalizacion,
                        pUsuario = request.usuario,
                        pAccion = "A"
                    },
                    commandType: CommandType.StoredProcedure
                );

                long lineaResultado = Convert.ToInt64(resultado?.Linea_Id ?? request.porcentaje.linea_id);

                return FinalizarRespuestaConBitacora(
                    codEmpresa,
                    request.usuario,
                    "Registra - WEB",
                    $"Comisiones, Tabla Porcentajes > Código: {request.cod_comision} > Id: {lineaResultado}",
                    new ErrorDto { Code = 0, Description = string.Empty },
                    GuardadoExito
                );
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Elimina una línea de porcentajes.
        /// </summary>
        public ErrorDto Cr_ComisionesCatalogo_Porcentaje_Eliminar(
            int codEmpresa,
            CrComisionesCatalogoPorcentajeEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_comision = Limpiar(request.cod_comision);

            if (string.IsNullOrWhiteSpace(request.cod_comision) || request.linea_id <= 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la comisión y la línea del porcentaje."
                };
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Execute(
                    "spCrd_Comisiones_TP_Registra",
                    new
                    {
                        pCodComision = request.cod_comision,
                        pLinea = request.linea_id,
                        pInicio = (decimal?)null,
                        pCorte = (decimal?)null,
                        pVenta = (decimal?)null,
                        pFormalizacion = (decimal?)null,
                        pUsuario = request.usuario,
                        pAccion = "E"
                    },
                    commandType: CommandType.StoredProcedure
                );

                return FinalizarRespuestaConBitacora(
                    codEmpresa,
                    request.usuario,
                    "Elimina - WEB",
                    $"Comisiones, Tabla Porcentajes > Código: {request.cod_comision} > Id: {request.linea_id}",
                    new ErrorDto { Code = 0, Description = string.Empty },
                    EliminadoExito
                );
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Asigna o desasigna una línea de crédito autorizada.
        /// </summary>
        public ErrorDto Cr_ComisionesCatalogo_Linea_Asignar(
            int codEmpresa,
            CrComisionesCatalogoLineaAsignarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_comision = Limpiar(request.cod_comision);
            request.codigo = Limpiar(request.codigo);

            if (string.IsNullOrWhiteSpace(request.cod_comision) || string.IsNullOrWhiteSpace(request.codigo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la comisión y la línea."
                };
            }

            string accion = request.asignado ? "I" : "E";
            string movimiento = request.asignado ? "Registra - WEB" : "Elimina - WEB";
            string detalle = $"Comisiones Asignación Línea Id: {request.cod_comision} .. Código: {request.codigo}";

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Execute(
                    "spCrd_Comisiones_Lineas_Asigna_Registra",
                    new
                    {
                        pCodComision = request.cod_comision,
                        pCodigo = request.codigo,
                        pUsuario = request.usuario,
                        pAccion = accion
                    },
                    commandType: CommandType.StoredProcedure
                );

                return FinalizarRespuestaConBitacora(
                    codEmpresa,
                    request.usuario,
                    movimiento,
                    detalle,
                    new ErrorDto { Code = 0, Description = string.Empty },
                    GuardadoExito
                );
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        #endregion 

        #region helpers

        private bool CuentaExiste(int codEmpresa, string cuenta)
        {
            if (string.IsNullOrWhiteSpace(cuenta))
                return false;

            const string sql = @"
                select isnull(count(*), 0)
                from vCNTX_CUENTAS_LOCAL
                where cod_cuenta = @Cuenta
                   or cod_cuenta_mask = @Cuenta;";

            var response = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Cuenta = cuenta }
            );

            return response.Result > 0;
        }

        #endregion

    }
}
