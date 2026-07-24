using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneRecargaTarjetaDB
    {
        /// <summary>
        /// Obtiene las tarjetas de regalo por estado, con filtro por fecha y paginación.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtros de búsqueda y paginación.</param>
        /// <param name="estado">Estado de la tarjeta a consultar.</param>
        /// <param name="sinAsignar">Si es true, solo tarjetas sin pago asignado.</param>
        /// <returns>Lista de tarjetas y total.</returns>
        public ErrorDto<AfiBeneTarjetasDataLista> AfiTarjetasRegalo_Obtener(int CodCliente, string filtros, string estado, bool? sinAsignar)
        {
            var infoFiltros = JsonConvert.DeserializeObject<AfiTarjetasFiltros>(filtros) ?? new AfiTarjetasFiltros();

            var parametros = new DynamicParameters();
            parametros.Add("estado", estado);
            AgregarFiltroTarjetaTexto(infoFiltros, parametros);
            AgregarFiltroTarjetaSinAsignar(sinAsignar, parametros);
            AgregarFiltroTarjetaFecha(infoFiltros, parametros);

            var offset = infoFiltros.pagina ?? 0;
            var fetch = infoFiltros.paginacion ?? 10;
            parametros.Add("offset", offset);
            parametros.Add("fetch", fetch);

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneTarjetasDataLista();

                const string sqlCount = @"SELECT COUNT(*)
                                          FROM AFI_BENE_TARJETAS_REGALO T
                                          WHERE T.ESTADO = @estado
                                            AND (@aplicaTexto = 0
                                                 OR T.cod_remesa_tr LIKE @filtroLike
                                                 OR T.registro_usuario LIKE @filtroLike
                                                 OR CONVERT(VARCHAR(19), T.registro_fecha, 120) LIKE @filtroLike
                                                 OR T.estado LIKE @filtroLike)
                                            AND (@sinAsignar = 0 OR T.ID_PAGO IS NULL)
                                            AND (@aplicaFecha = 0 OR T.registro_fecha BETWEEN @fechaInicio AND @fechaCorte)";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, parametros);

                const string sql = @"SELECT T.*,
                                            (SELECT NOMBRE FROM socios WHERE CEDULA = T.cedula) AS NOMBRE,
                                            (SELECT DESCRIPCION FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = T.COD_BENEFICIO) AS BENEFICIO_DESC
                                     FROM AFI_BENE_TARJETAS_REGALO T
                                     WHERE T.ESTADO = @estado
                                       AND (@aplicaTexto = 0
                                            OR T.cod_remesa_tr LIKE @filtroLike
                                            OR T.registro_usuario LIKE @filtroLike
                                            OR CONVERT(VARCHAR(19), T.registro_fecha, 120) LIKE @filtroLike
                                            OR T.estado LIKE @filtroLike)
                                       AND (@sinAsignar = 0 OR T.ID_PAGO IS NULL)
                                       AND (@aplicaFecha = 0 OR T.registro_fecha BETWEEN @fechaInicio AND @fechaCorte)
                                     ORDER BY T.registro_fecha DESC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Tarjetas = connection.Query<AfiBeneTarjetasData>(sql, parametros).ToList();
                return response;
            });
        }

        /// <summary>
        /// Agrega el filtro de texto de búsqueda de tarjetas.
        /// </summary>
        private static void AgregarFiltroTarjetaTexto(AfiTarjetasFiltros filtros, DynamicParameters parametros)
        {
            var filtroTexto = filtros.vfiltro?.Trim() ?? string.Empty;
            var aplicaTexto = filtroTexto.Length > 0;
            parametros.Add("aplicaTexto", aplicaTexto);
            parametros.Add("filtroLike", aplicaTexto ? $"%{filtroTexto}%" : string.Empty);
        }

        /// <summary>
        /// Agrega el filtro de tarjetas sin pago asignado.
        /// </summary>
        private static void AgregarFiltroTarjetaSinAsignar(bool? sinAsignar, DynamicParameters parametros)
        {
            parametros.Add("sinAsignar", sinAsignar == true);
        }

        /// <summary>
        /// Agrega el filtro de rango de fechas de registro de tarjetas.
        /// </summary>
        private static void AgregarFiltroTarjetaFecha(AfiTarjetasFiltros filtros, DynamicParameters parametros)
        {
            var aplicaFecha = filtros.fecha_inicio != null;
            parametros.Add("aplicaFecha", aplicaFecha);

            if (!aplicaFecha)
            {
                parametros.Add("fechaInicio", string.Empty);
                parametros.Add("fechaCorte", string.Empty);
                return;
            }

            var fechaInicio = MProGrXAuxiliarDB.validaFechaGlobal(filtros.fecha_inicio, "yyyy-MM-dd");
            var fechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(filtros.fecha_corte, "yyyy-MM-dd");

            parametros.Add("fechaInicio", $"{fechaInicio}T00:00:00");
            parametros.Add("fechaCorte", $"{fechaCorte}T11:59:59");
        }

        /// <summary>
        /// Inserta una tarjeta de regalo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tarjetas">JSON con los datos de la tarjeta.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiTarjetasRegalo_Insertar(int CodCliente, string tarjetas)
        {
            var item = JsonConvert.DeserializeObject<AfiBeneTarjetasData>(tarjetas) ?? new AfiBeneTarjetasData();

            const string sql = @"INSERT AFI_BENE_TARJETAS_REGALO
                                    (COD_PRODUCTO, REGISTRO_FECHA, REGISTRO_USUARIO, COD_BENEFICIO, ESTADO, NO_TARJETA, MONTO)
                                 VALUES
                                    (@cod_producto, GETDATE(), @registro_usuario, @cod_beneficio, 'P', @no_tarjeta, @monto)";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                item.cod_producto,
                item.registro_usuario,
                item.cod_beneficio,
                item.no_tarjeta,
                item.monto
            });

            if (result.Code == 0)
            {
                result.Description = "Tarjeta registrada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Actualiza una tarjeta de regalo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tarjetas">JSON con los datos de la tarjeta.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiTarjetasRegalo_Actualizar(int CodCliente, string tarjetas)
        {
            var item = JsonConvert.DeserializeObject<AfiBeneTarjetasData>(tarjetas) ?? new AfiBeneTarjetasData();

            const string sql = @"UPDATE AFI_BENE_TARJETAS_REGALO
                                 SET COD_PRODUCTO = @cod_producto, COD_BENEFICIO = @cod_beneficio,
                                     NO_TARJETA = @no_tarjeta, MONTO = @monto
                                 WHERE ID_TR = @id_tr";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                item.cod_producto,
                item.cod_beneficio,
                item.no_tarjeta,
                item.monto,
                item.id_tr
            });

            if (result.Code == 0)
            {
                result.Description = "Tarjeta actualizada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Elimina una tarjeta de regalo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="id_tr">Identificador de la tarjeta.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiTarjetasRegalo_Eliminar(int CodCliente, int id_tr)
        {
            const string sql = "DELETE FROM AFI_BENE_TARJETAS_REGALO WHERE ID_TR = @id_tr";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { id_tr });

            if (result.Code == 0)
            {
                result.Description = "Tarjeta eliminada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Obtiene los productos habilitados como tarjeta de regalo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de productos.</returns>
        public ErrorDto<List<ProductoData>> AfiTarjetasProductos_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT * FROM AFI_BENE_PRODUCTOS WHERE tarjeta_regalo = 1";
                return connection.Query<ProductoData>(sql).ToList();
            });
        }
    }
}
