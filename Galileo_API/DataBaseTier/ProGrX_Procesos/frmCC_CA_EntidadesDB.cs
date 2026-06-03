using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCcCaEntidadesDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 10;
        private readonly MCntLinkDB mCntLink;

        private const string CampoCodEntidad = "cod_entidad";
        private const string CampoDescripcion = "descripcion";
        private const string CampoNumeroAfiliado = "NUMERO_AFILIADO";

        public FrmCcCaEntidadesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
            mCntLink = new MCntLinkDB(_config);
        }

        /// <summary>
        /// Consulta el listado de entidades
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CaEntidadLista> CC_CA_Entidades_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de entidades son requeridos.", -2, CrearResultadoVacio());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new CaEntidadLista
                {
                    total = connection.QueryFirstOrDefault<int>("select COUNT(1) from prm_ca_Entidad"),
                    lista = new List<CaEntidadData>()
                };

                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldEntidades(filtros.sortField);
                var sortDirection = ObtenerSortDirectionEntidades(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                var query = @"
                    select cod_entidad,
                           descripcion,
                           NUMERO_AFILIADO,
                           formato,
                           cod_cuenta,
                           activo
                    from prm_ca_Entidad
                    where (
                        @Filtro is null
                        or cod_entidad like @Filtro
                        or descripcion like @Filtro
                        or NUMERO_AFILIADO like @Filtro
                    )
                    order by
                        CASE WHEN @SortField = 'cod_entidad' AND @SortDirection = 'ASC' THEN cod_entidad END ASC,
                        CASE WHEN @SortField = 'cod_entidad' AND @SortDirection = 'DESC' THEN cod_entidad END DESC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'ASC' THEN descripcion END ASC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'DESC' THEN descripcion END DESC,
                        CASE WHEN @SortField = 'NUMERO_AFILIADO' AND @SortDirection = 'ASC' THEN NUMERO_AFILIADO END ASC,
                        CASE WHEN @SortField = 'NUMERO_AFILIADO' AND @SortDirection = 'DESC' THEN NUMERO_AFILIADO END DESC,
                        cod_entidad ASC";

                if (fetchRows > 0)
                {
                    query += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                }

                salida.lista = connection.Query<CaEntidadData>(
                    query,
                    new
                    {
                        Filtro = string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%",
                        SortField = sortField,
                        SortDirection = sortDirection,
                        OffsetRows = offsetRows,
                        FetchRows = fetchRows
                    }).ToList();

                FormatearCuentasEntidad(CodEmpresa, salida.lista);
                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearResultadoVacio())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar entidades.", result.Code.GetValueOrDefault(-1), CrearResultadoVacio());
        }

        /// <summary>
        ///  Modifica o crea una entidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto frmCC_CA_Entidad_Guardar(int CodEmpresa, string usuario, CaEntidadData request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de la entidad son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(
                    "select isnull(count(*),0) as Existe from prm_ca_Entidad where cod_entidad = @cod_entidad",
                    new { request.cod_entidad });

                if (request.isNew)
                {
                    return existe > 0
                        ? DbHelper.ErrorResponse($"La entidad con el código {request.cod_entidad} ya existe.", -2)
                        : CC_CA_Entidad_Insertar(connection, CodEmpresa, usuario, request);
                }

                return existe == 0
                    ? DbHelper.ErrorResponse($"La entidad con el código {request.cod_entidad} no existe.", -2)
                    : CC_CA_Entidad_Actualizar(connection, CodEmpresa, usuario, request);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar entidad.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza un registro de entidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CC_CA_Entidad_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, CaEntidadData datos)
        {
            connection.Execute(
                @"update prm_ca_Entidad
                  set descripcion = @descripcion,
                      NUMERO_AFILIADO = @numero_afiliado,
                      Formato = @formato,
                      cod_cuenta = @cod_cuenta,
                      Activo = @activo
                  where cod_entidad = @cod_entidad",
                new
                {
                    datos.cod_entidad,
                    datos.descripcion,
                    datos.numero_afiliado,
                    datos.formato,
                    datos.cod_cuenta,
                    datos.activo,
                    usuario
                });

            RegistrarBitacora(CodEmpresa, usuario, $"Cargos Automáticos - Entidad: {datos.cod_entidad}", "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Inserta un nuevo registro de entidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto CC_CA_Entidad_Insertar(SqlConnection connection, int CodEmpresa, string usuario, CaEntidadData datos)
        {
            connection.Execute(
                @"insert into prm_ca_Entidad(cod_entidad, descripcion, NUMERO_AFILIADO, formato, cod_cuenta, activo, registro_Fecha, registro_usuario)
                  values(@cod_entidad, @descripcion, @numero_afiliado, @formato, @cod_cuenta, @activo, Getdate(), @usuario)",
                new
                {
                    datos.cod_entidad,
                    datos.descripcion,
                    datos.numero_afiliado,
                    datos.formato,
                    datos.cod_cuenta,
                    datos.activo,
                    usuario
                });

            RegistrarBitacora(CodEmpresa, usuario, $"Cargos Automáticos - Entidad: {datos.cod_entidad}", "Registra - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        public ErrorDto CC_CA_Entidad_Delete(int CodEmpresa, string Usuario, string Codigo)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                "delete prm_ca_Entidad where cod_entidad = @Codigo",
                new { Codigo });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar entidad.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(CodEmpresa, Usuario, $"Cargos Automáticos - Entidad: {Codigo}", "Elimina - WEB");
                return DbHelper.OkResponse("Ok");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }

        private static CaEntidadLista CrearResultadoVacio()
        {
            return new CaEntidadLista
            {
                total = 0,
                lista = new List<CaEntidadData>()
            };
        }

        private void FormatearCuentasEntidad(int codEmpresa, List<CaEntidadData> lista)
        {
            foreach (var item in lista)
            {
                item.cod_cuenta_mask = mCntLink.fxgCntCuentaFormato(codEmpresa, true, item.cod_cuenta, 1);
            }
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string ObtenerSortFieldEntidades(string? sortField)
        {
            return sortField switch
            {
                CampoCodEntidad => CampoCodEntidad,
                CampoDescripcion => CampoDescripcion,
                CampoNumeroAfiliado => CampoNumeroAfiliado,
                _ => CampoCodEntidad
            };
        }

        private static string ObtenerSortDirectionEntidades(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}