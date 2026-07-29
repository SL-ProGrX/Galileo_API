using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFComisionesParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly MCntLinkDB _mCnt;
        private readonly MTesFuncionesDb _mFun;
        private readonly MAfilicacionDB _mAfi;
        private readonly MSecurityMainDb DBBitacora;

        private const string SpComisionesParametros = "spAFIComisionesParametros";

        private const string SqlComisionesParametrosTotal = @"
                    SELECT COUNT(*)
                    FROM dbo.AFI_COMISIONES_PARAMETROS
                    WHERE @hasFilter = 0 OR
                          cod_parametro LIKE @filtro OR
                          descripcion LIKE @filtro OR
                          valor LIKE @filtro;";

        private const string SqlComisionesParametrosLista = @"
                    SELECT cod_parametro,
                           descripcion,
                           valor
                    FROM dbo.AFI_COMISIONES_PARAMETROS
                    WHERE @hasFilter = 0 OR
                          cod_parametro LIKE @filtro OR
                          descripcion LIKE @filtro OR
                          valor LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_parametro END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_parametro END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN valor END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN valor END DESC,
                        cod_parametro ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlComisionesParametrosUpdate = @"
                    UPDATE dbo.AFI_COMISIONES_PARAMETROS
                    SET valor = @Valor
                    WHERE cod_parametro = @Parametro;";

        private static readonly IReadOnlyDictionary<string, int> ComisionesParametrosSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["cod_parametro"] = 1,
            ["descripcion"] = 2,
            ["valor"] = 3
        };

        public FrmAFComisionesParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mCnt = new MCntLinkDB(_config);
            _mFun = new MTesFuncionesDb(_config);
            _mAfi = new MAfilicacionDB(_config);
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }


        /// <summary>
        /// Obtiene la lista paginada de parámetros de comisiones de afiliación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtro">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de parámetros de comisiones.</returns>
        public ErrorDto<TablasListaGenericaModel> AF_ComisionesParametros_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            var response = DbHelper.CreateOkResponse(new TablasListaGenericaModel());

            try
            {
                var spec = LazyLoadHelper.Build(filtro, ComisionesParametrosSortMap, "cod_parametro");

                var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                {
                    connection.Execute(SpComisionesParametros, commandType: System.Data.CommandType.StoredProcedure);

                    return new TablasListaGenericaModel
                    {
                        total = connection.QueryFirstOrDefault<int>(SqlComisionesParametrosTotal, spec.Params),
                        lista = connection.Query<AFComisionesParametrosDto>(SqlComisionesParametrosLista, spec.Params).ToList()
                    };
                });

                if (result.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        result.Description ?? "Error al obtener parámetros de comisiones.",
                        result.Code.GetValueOrDefault(-1),
                        new TablasListaGenericaModel());
                }

                response.Result = result.Result ?? new TablasListaGenericaModel();
            }
            catch (Exception ex)
            {
                response = DbHelper.CreateErrorResponse(ex.Message, -1, new TablasListaGenericaModel());
            }

            return response;
        }


        /// <summary>
        /// Actualiza el valor de un parámetro de comisiones de afiliación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Contabilidad">Código de contabilidad.</param>
        /// <param name="Usuario">Usuario que realiza la actualización.</param>
        /// <param name="param">Parámetro a actualizar.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto AF_ComisionesParametros_Guardar(int CodEmpresa, int Contabilidad, string Usuario, AFComisionesParametrosDto param)
        {
            if (param is null)
            {
                return DbHelper.ErrorResponse("Los datos del parámetro son requeridos.", -2);
            }

            var valida = fxValida(CodEmpresa, Contabilidad, param);
            if (!string.IsNullOrWhiteSpace(valida))
            {
                return DbHelper.ErrorResponse(valida, -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlComisionesParametrosUpdate,
                new
                {
                    Parametro = NormalizarTexto(param.cod_parametro),
                    Valor = NormalizarTexto(param.valor)
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al actualizar parámetro de comisiones.",
                    result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraParametro(CodEmpresa, Usuario, param);
            return DbHelper.OkResponse("Registro actualizado correctamente");
        }


        /// <summary>
        /// Valida el valor de un parámetro según reglas contables o de tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Contabilidad">Código de contabilidad.</param>
        /// <param name="param">Parámetro a validar.</param>
        /// <returns>Mensaje de error cuando el valor no es válido; cadena vacía cuando es válido.</returns>
        private string fxValida(int CodEmpresa, int Contabilidad, AFComisionesParametrosDto param)
        {
            string vMensaje = "";
            try
            {
                string vParametro = NormalizarTexto(param.cod_parametro);

                switch (vParametro)
                {
                    case "01": //Cuenta Contable
                        if (!_mCnt.fxgCntCuentaValida(CodEmpresa, NormalizarTexto(param.valor)))
                        {
                            vMensaje = " - Cuenta Contable no es v&aacute;lida...!";
                        }
                        break;
                    case "19": //Tesoreria Unidad
                        if (!_mFun.fxgTESValidaDatos(CodEmpresa, Contabilidad, "UNIDAD", NormalizarTexto(param.valor)))
                        {
                            vMensaje = " - C&oacute;digo de Unidad no existe o se encuentra desactivado...!";
                        }
                        break;
                    case "20": //Tesoreria Centro de Costo
                        string vUnidad = _mAfi.fxgAFIParametroComision_Obtener(CodEmpresa, "19");
                        if (!_mFun.fxgTESValidaDatos(CodEmpresa, Contabilidad, "CC", NormalizarTexto(param.valor), vUnidad))
                        {
                            vMensaje = " - C&oacute;digo de Centro de Costo no existe o se encuentra desactivado, o no ha sido asignado a esta unidad: " + vUnidad + "...!";
                        }
                        break;
                    case "21": //Tesoreria Conceptos
                        if (!_mFun.fxgTESValidaDatos(CodEmpresa, Contabilidad, "CONCEPTO", NormalizarTexto(param.valor)))
                        {
                            vMensaje = " - C&oacute;digo de Concepto no existe o se encuentra desactivado...!";
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                vMensaje = ex.Message;
            }

            return vMensaje;
        }


        /// <summary>
        /// Obtiene la lista de búsqueda del valor asignable a un parámetro de comisión.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Contabilidad">Código de contabilidad.</param>
        /// <param name="Parametro">Código de parámetro.</param>
        /// <returns>Lista de valores disponibles según el parámetro.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ComisionesParametros_Busqueda(int CodEmpresa, int Contabilidad, string Parametro)
        {
            var response = DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());

            try
            {
                response = NormalizarTexto(Parametro) switch
                {
                    "19" => _mFun.sbgTESBusqueda(CodEmpresa, Contabilidad, "UNIDAD"),
                    "20" => _mFun.sbgTESBusqueda(CodEmpresa, Contabilidad, "CC", _mAfi.fxgAFIParametroComision_Obtener(CodEmpresa, "19")),
                    "21" => _mFun.sbgTESBusqueda(CodEmpresa, Contabilidad, "CONCEPTO"),
                    _ => response
                };
            }
            catch (Exception ex)
            {
                response = DbHelper.CreateErrorResponse(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }

            return response;
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Registra en bitácora la modificación de un parámetro de comisión.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la actualización.</param>
        /// <param name="param">Parámetro actualizado.</param>
        private void RegistrarBitacoraParametro(int codEmpresa, string usuario, AFComisionesParametrosDto param)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Parametro de Comisiones de Afiliación : {NormalizarTexto(param.cod_parametro)}",
                Movimiento = "MODIFICA - WEB",
                Modulo = 9
            });
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}