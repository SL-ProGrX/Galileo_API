using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier
{
    /// <summary>
    /// Funciones compartidas del módulo de compras.
    /// </summary>
    public class MComprasDB
    {
        private const string MensajeOrdenRequerida =
            "El c&oacute;digo de la orden es requerido.";

        private const string MensajeFacturaRequerida =
            "El c&oacute;digo de la factura es requerido.";

        private const string MensajeProveedorInvalido =
            "El c&oacute;digo del proveedor no es v&aacute;lido.";

        private readonly PortalDB _portalDB;

        /// <summary>
        /// Inicializa las funciones compartidas del módulo de compras.
        /// </summary>
        /// <param name="config">
        /// Configuración de la aplicación.
        /// </param>
        public MComprasDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDB = new PortalDB(config);
        }

        #region Helpers privados

        /// <summary>
        /// Deserializa los filtros enviados por los procesos de compras.
        /// </summary>
        /// <param name="filtros">
        /// Filtros serializados en formato JSON.
        /// </param>
        /// <returns>
        /// Filtros normalizados.
        /// </returns>
        private static MComprasFiltros
            MCompras_Filtros_Deserializar(
                string? filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return new MComprasFiltros();
            }

            return JsonConvert.DeserializeObject<MComprasFiltros>(
                       filtros)
                   ?? new MComprasFiltros();
        }

        /// <summary>
        /// Agrega los parámetros comunes de filtro y paginación.
        /// </summary>
        /// <param name="filtros">
        /// Filtros de la consulta.
        /// </param>
        /// <param name="parameters">
        /// Parámetros Dapper.
        /// </param>
        private static void
            MCompras_FiltroYPaginacion_Parametros_Agregar(
                MComprasFiltros filtros,
                DynamicParameters parameters)
        {
            var filtro = filtros.filtro?.Trim();
            var tieneFiltro =
                !string.IsNullOrWhiteSpace(filtro);

            var paginar =
                filtros.pagina.HasValue
                && filtros.paginacion.HasValue
                && filtros.paginacion.Value > 0;

            parameters.Add(
                "@HasFiltro",
                tieneFiltro ? 1 : 0,
                DbType.Int32);

            parameters.Add(
                "@Filtro",
                tieneFiltro
                    ? $"%{filtro}%"
                    : DBNull.Value,
                DbType.String);

            parameters.Add(
                "@Paginar",
                paginar ? 1 : 0,
                DbType.Int32);

            parameters.Add(
                "@Offset",
                paginar
                    ? Math.Max(
                        filtros.pagina.GetValueOrDefault(),
                        0)
                    : 0,
                DbType.Int32);

            parameters.Add(
                "@PageSize",
                paginar
                    ? filtros.paginacion.GetValueOrDefault()
                    : int.MaxValue,
                DbType.Int32);
        }

        /// <summary>
        /// Ejecuta una consulta paginada y devuelve su total y registros.
        /// </summary>
        /// <typeparam name="T">
        /// Tipo de registro retornado.
        /// </typeparam>
        /// <param name="connection">
        /// Conexión activa del cliente.
        /// </param>
        /// <param name="sqlCount">
        /// Consulta para obtener el total.
        /// </param>
        /// <param name="sqlData">
        /// Consulta para obtener los registros.
        /// </param>
        /// <param name="parameters">
        /// Parámetros de ambas consultas.
        /// </param>
        /// <returns>
        /// Total y registros encontrados.
        /// </returns>
        private static (
            int total,
            List<T> registros)
            MCompras_ConsultaPaginada_Ejecutar<T>(
                SqlConnection connection,
                string sqlCount,
                string sqlData,
                DynamicParameters parameters)
        {
            var total =
                connection.QueryFirstOrDefault<int>(
                    sqlCount,
                    parameters);

            var registros =
                connection.Query<T>(
                        sqlData,
                        parameters)
                    .ToList();

            return (total, registros);
        }

        /// <summary>
        /// Garantiza que un resultado genérico contenga una instancia
        /// válida aun cuando la operación haya fallado.
        /// </summary>
        /// <typeparam name="T">
        /// Tipo del resultado.
        /// </typeparam>
        /// <param name="resultado">
        /// Resultado obtenido desde DbHelper.
        /// </param>
        /// <param name="valorPredeterminado">
        /// Valor utilizado cuando no existe resultado.
        /// </param>
        /// <returns>
        /// Respuesta normalizada.
        /// </returns>
        private static ErrorDto<T>
            MCompras_Resultado_Normalizar<T>(
                ErrorDto<T> resultado,
                T valorPredeterminado)
        {
            return new ErrorDto<T>
            {
                Code = resultado.Code,
                Description = resultado.Description,
                Result = resultado.Result
                         ?? valorPredeterminado
            };
        }

        #endregion

        #region Cargos / Tipos Orden

        /// <summary>
        /// Obtiene los cargos periódicos disponibles.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <returns>
        /// Cargos periódicos.
        /// </returns>
        public List<CargoPeriodicoDto>
            sbCprCboCargosPer(
                int codEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(cod_cargo) AS cod_cargo,
                    RTRIM(descripcion) AS descripcion
                FROM cxp_cargos
                ORDER BY cod_cargo;
                """;

            var resultado =
                DbHelper.ExecuteListQuery<CargoPeriodicoDto>(
                    _portalDB,
                    codEmpresa,
                    sql);

            return resultado.Result
                   ?? new List<CargoPeriodicoDto>();
        }

        /// <summary>
        /// Obtiene los tipos de orden de compra.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <returns>
        /// Tipos de orden.
        /// </returns>
        public List<TipoOrdenDto>
            sbCprCboTiposOrden(
                int codEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(tipo_orden) AS tipo_orden,
                    RTRIM(descripcion) AS descripcion
                FROM cpr_tipo_orden
                ORDER BY tipo_orden;
                """;

            var resultado =
                DbHelper.ExecuteListQuery<TipoOrdenDto>(
                    _portalDB,
                    codEmpresa,
                    sql);

            return resultado.Result
                   ?? new List<TipoOrdenDto>();
        }

        #endregion

        #region Cambia Fecha

        /// <summary>
        /// Indica si el usuario tiene autorización para cambiar fechas.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="usuario">
        /// Usuario que se desea validar.
        /// </param>
        /// <returns>
        /// Verdadero cuando existe al menos una autorización.
        /// </returns>
        public bool fxCprCambiaFecha(
            int codEmpresa,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return false;
            }

            const string sql = """
                SELECT COUNT(1)
                FROM cpr_INVUSRFECHAS
                WHERE usuario = @Usuario;
                """;

            var resultado =
                DbHelper.ExecuteSingleQuery<int>(
                    _portalDB,
                    codEmpresa,
                    sql,
                    defaultValue: 0,
                    parameters: new
                    {
                        Usuario = usuario.Trim()
                    });

            return resultado.Code == 0
                   && resultado.Result > 0;
        }

        #endregion

        #region Órdenes Despacho

        /// <summary>
        /// Actualiza el proceso de una orden según sus cantidades
        /// pendientes de despacho.
        /// </summary>
        /// <remarks>
        /// Conserva la paridad con VB6:
        /// D cuando no existen cantidades pendientes y X cuando existen.
        /// </remarks>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codOrden">
        /// Código de la orden.
        /// </param>
        /// <returns>
        /// Resultado de la actualización.
        /// </returns>
        public ErrorDto sbCprOrdenesDespacho(
            int codEmpresa,
            string codOrden)
        {
            if (string.IsNullOrWhiteSpace(codOrden))
            {
                return DbHelper.ErrorResponse(
                    MensajeOrdenRequerida);
            }

            const string sql = """
                UPDATE cpr_ordenes
                SET proceso =
                    CASE
                        WHEN EXISTS
                        (
                            SELECT 1
                            FROM cpr_ordenes_detalle
                            WHERE cod_orden = @CodOrden
                              AND cantidad
                                  - ISNULL(
                                      cantidad_despachada,
                                      0) > 1
                        )
                        THEN 'X'
                        ELSE 'D'
                    END
                WHERE cod_orden = @CodOrden;
                """;

            var resultado =
                DbHelper.WithConn(
                    _portalDB,
                    codEmpresa,
                    connection =>
                        connection.Execute(
                            sql,
                            new
                            {
                                CodOrden = codOrden.Trim()
                            }));

            if (resultado.Code < 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description
                    ?? "No fue posible actualizar la orden.");
            }

            return DbHelper.OkResponse("Ok");
        }

        #endregion

        #region Unidades

        /// <summary>
        /// Obtiene las unidades contables de manera paginada.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="filtros">
        /// Filtros serializados en formato JSON.
        /// </param>
        /// <returns>
        /// Unidades y cantidad total de registros.
        /// </returns>
        public ErrorDto<UnidadesDtoList>
            UnidadesObtener(
                int codEmpresa,
                string? filtros)
        {
            var resultado =
                DbHelper.WithConn(
                    _portalDB,
                    codEmpresa,
                    connection =>
                    {
                        var valores =
                            MCompras_Filtros_Deserializar(
                                filtros);

                        var parameters =
                            new DynamicParameters();

                        parameters.Add(
                            "@CodConta",
                            valores.CodConta,
                            DbType.Int32);

                        MCompras_FiltroYPaginacion_Parametros_Agregar(
                            valores,
                            parameters);

                        const string sqlCount = """
                            SELECT COUNT(*)
                            FROM CntX_Unidades
                            WHERE COD_CONTABILIDAD = @CodConta
                              AND
                              (
                                  @HasFiltro = 0
                                  OR COD_UNIDAD LIKE @Filtro
                                  OR descripcion LIKE @Filtro
                              );
                            """;

                        const string sqlData = """
                            SELECT
                                RTRIM(cod_unidad) AS unidad,
                                RTRIM(descripcion) AS descripcion
                            FROM CntX_Unidades
                            WHERE COD_CONTABILIDAD = @CodConta
                              AND
                              (
                                  @HasFiltro = 0
                                  OR COD_UNIDAD LIKE @Filtro
                                  OR descripcion LIKE @Filtro
                              )
                            ORDER BY COD_UNIDAD DESC
                            OFFSET
                                CASE
                                    WHEN @Paginar = 1
                                    THEN @Offset
                                    ELSE 0
                                END ROWS
                            FETCH NEXT
                                CASE
                                    WHEN @Paginar = 1
                                    THEN @PageSize
                                    ELSE 2147483647
                                END ROWS ONLY;
                            """;

                        var consulta =
                            MCompras_ConsultaPaginada_Ejecutar
                                <UnidadesDto>(
                                    connection,
                                    sqlCount,
                                    sqlData,
                                    parameters);

                        return new UnidadesDtoList
                        {
                            Total = consulta.total,
                            Unidades = consulta.registros
                        };
                    });

            return MCompras_Resultado_Normalizar(
                resultado,
                new UnidadesDtoList());
        }

        #endregion

        #region Centros de Costo

        /// <summary>
        /// Obtiene los centros de costo de manera paginada.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="filtros">
        /// Filtros serializados en formato JSON.
        /// </param>
        /// <returns>
        /// Centros de costo y cantidad total de registros.
        /// </returns>
        public ErrorDto<CentroCostoDtoList>
            CentroCostosObtener(
                int codEmpresa,
                string? filtros)
        {
            var resultado =
                DbHelper.WithConn(
                    _portalDB,
                    codEmpresa,
                    connection =>
                    {
                        var valores =
                            MCompras_Filtros_Deserializar(
                                filtros);

                        var parameters =
                            new DynamicParameters();

                        parameters.Add(
                            "@CodConta",
                            valores.CodConta,
                            DbType.Int32);

                        MCompras_FiltroYPaginacion_Parametros_Agregar(
                            valores,
                            parameters);

                        const string sqlCount = """
                            SELECT COUNT(*)
                            FROM CNTX_CENTRO_COSTOS
                            WHERE COD_CONTABILIDAD = @CodConta
                              AND
                              (
                                  @HasFiltro = 0
                                  OR cod_centro_costo LIKE @Filtro
                                  OR descripcion LIKE @Filtro
                              );
                            """;

                        const string sqlData = """
                            SELECT
                                RTRIM(cod_centro_costo)
                                    AS centrocosto,
                                RTRIM(descripcion)
                                    AS descripcion
                            FROM CNTX_CENTRO_COSTOS
                            WHERE COD_CONTABILIDAD = @CodConta
                              AND
                              (
                                  @HasFiltro = 0
                                  OR cod_centro_costo LIKE @Filtro
                                  OR descripcion LIKE @Filtro
                              )
                            ORDER BY cod_centro_costo DESC
                            OFFSET
                                CASE
                                    WHEN @Paginar = 1
                                    THEN @Offset
                                    ELSE 0
                                END ROWS
                            FETCH NEXT
                                CASE
                                    WHEN @Paginar = 1
                                    THEN @PageSize
                                    ELSE 2147483647
                                END ROWS ONLY;
                            """;

                        var consulta =
                            MCompras_ConsultaPaginada_Ejecutar
                                <CentroCostoDto>(
                                    connection,
                                    sqlCount,
                                    sqlData,
                                    parameters);

                        return new CentroCostoDtoList
                        {
                            Total = consulta.total,
                            Centrocostos = consulta.registros
                        };
                    });

            return MCompras_Resultado_Normalizar(
                resultado,
                new CentroCostoDtoList());
        }

        #endregion

        #region Catálogo Compras

        /// <summary>
        /// Obtiene las opciones activas de un catálogo de compras.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="tipo">
        /// Descripción del tipo de catálogo.
        /// </param>
        /// <returns>
        /// Opciones activas del catálogo.
        /// </returns>
        public ErrorDto<List<CatalogoDto>>
            CatalogoCompras_Obtener(
                int codEmpresa,
                string tipo)
        {
            const string sql = """
                SELECT
                    CATALOGO_ID AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM CPR_CATALOGOS
                WHERE Tipo_Id =
                (
                    SELECT TIPO_ID
                    FROM CPR_CATALOGOS_TIPOS
                    WHERE DESCRIPCION = @Tipo
                )
                  AND Activo = 1
                ORDER BY DESCRIPCION;
                """;

            var resultado =
                DbHelper.WithConn(
                    _portalDB,
                    codEmpresa,
                    connection =>
                        connection.Query<CatalogoDto>(
                                sql,
                                new
                                {
                                    Tipo = tipo.Trim()
                                })
                            .ToList());

            return MCompras_Resultado_Normalizar(
                resultado,
                new List<CatalogoDto>());
        }

        #endregion

        #region Facturas / Órdenes

        /// <summary>
        /// Marca como relacionada la factura asociada con una orden.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codFactura">
        /// Código del documento de la factura.
        /// </param>
        /// <param name="codProveedor">
        /// Código del proveedor.
        /// </param>
        /// <returns>
        /// Resultado de la actualización.
        /// </returns>
        public ErrorDto FacturaOrdenes_Actualizar(
            int codEmpresa,
            string codFactura,
            int codProveedor)
        {
            if (string.IsNullOrWhiteSpace(codFactura))
            {
                return DbHelper.ErrorResponse(
                    MensajeFacturaRequerida);
            }

            if (codProveedor <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeProveedorInvalido);
            }

            var resultado =
                DbHelper.WithConn(
                    _portalDB,
                    codEmpresa,
                    connection =>
                    {
                        const string sqlProveedor = """
                            SELECT CEDJUR
                            FROM CXP_PROVEEDORES
                            WHERE COD_PROVEEDOR =
                                @CodProveedor;
                            """;

                        var cedulaJuridica =
                            connection
                                .QueryFirstOrDefault<string>(
                                    sqlProveedor,
                                    new
                                    {
                                        CodProveedor =
                                            codProveedor
                                    })
                            ?? string.Empty;

                        cedulaJuridica =
                            cedulaJuridica
                                .Replace(
                                    "-",
                                    string.Empty,
                                    StringComparison.Ordinal)
                                .Replace(
                                    " ",
                                    string.Empty,
                                    StringComparison.Ordinal);

                        const string sqlActualizar = """
                            UPDATE CPR_FACTURAS_XML
                            SET ESTADO = 'R'
                            WHERE COD_DOCUMENTO =
                                    @CodFactura
                              AND CED_JUR_PROV =
                                    @CedulaJuridica;
                            """;

                        return connection.Execute(
                            sqlActualizar,
                            new
                            {
                                CodFactura =
                                    codFactura.Trim(),
                                CedulaJuridica =
                                    cedulaJuridica
                            });
                    });

            if (resultado.Code < 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description
                    ?? "No fue posible actualizar la factura.");
            }

            return DbHelper.OkResponse("Ok");
        }

        #endregion
    }
}