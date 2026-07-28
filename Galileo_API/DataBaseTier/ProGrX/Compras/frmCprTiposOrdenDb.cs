using System.Data;
using Dapper;
using Newtonsoft.Json;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCprTiposOrdenDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCompras = 35;

        public FrmCprTiposOrdenDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto<TiposOrdenLista> ObtenerTiposOrdenes(int codEmpresa, string jFiltros)
        {
            var filtro = SafeParseFiltro(jFiltros);

            // En tu modelo, pagina funciona como OFFSET.
            var offset = filtro.pagina < 0 ? 0 : filtro.pagina;
            var fetch = filtro.paginacion <= 0 ? 50 : filtro.paginacion;

            var raw = (filtro.filtro ?? string.Empty).Trim();
            var like = string.IsNullOrWhiteSpace(raw) ? null : $"%{raw}%";
            var sortColumn = Cpr_TiposOrden_SortColumn_Resolver(filtro.sortField);
            var sortOrder = filtro.sortOrder == 0 ? 0 : 1;

            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var p = new DynamicParameters();
                p.Add("Like", like, DbType.String);
                p.Add("Offset", offset, DbType.Int32);
                p.Add("Fetch", fetch, DbType.Int32);
                p.Add("SortColumn", sortColumn, DbType.Int32);
                p.Add("SortOrder", sortOrder, DbType.Int32);

                // Total (mismo filtro)
                var total = conn.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(Tipo_Orden)
                      FROM cpr_Tipo_Orden
                      WHERE (@Like IS NULL OR Tipo_Orden LIKE @Like OR descripcion LIKE @Like);",
                    p
                );

                // Lista (sin concatenar SQL)
                var lista = conn.Query<TiposOrdenDto>(
                    @"SELECT Tipo_Orden, descripcion, activo
                      FROM cpr_Tipo_Orden
                      WHERE (@Like IS NULL OR Tipo_Orden LIKE @Like OR descripcion LIKE @Like)
                      ORDER BY
                          CASE WHEN @SortColumn = 1 AND @SortOrder = 1 THEN Tipo_Orden END ASC,
                          CASE WHEN @SortColumn = 1 AND @SortOrder = 0 THEN Tipo_Orden END DESC,
                          CASE WHEN @SortColumn = 2 AND @SortOrder = 1 THEN descripcion END ASC,
                          CASE WHEN @SortColumn = 2 AND @SortOrder = 0 THEN descripcion END DESC,
                          CASE WHEN @SortColumn = 3 AND @SortOrder = 1 THEN activo END ASC,
                          CASE WHEN @SortColumn = 3 AND @SortOrder = 0 THEN activo END DESC,
                          Tipo_Orden ASC
                      OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;",
                    p
                ).ToList();

                return new TiposOrdenLista
                {
                    total = total,
                    lista = lista
                };
            });

            var code = Convert.ToInt32(r.Code);
            if (code != 0)
                return DbHelper.CreateErrorResponse<TiposOrdenLista>(r.Description ?? "Error", code, default);

            return DbHelper.CreateOkResponse(
                r.Result ?? new TiposOrdenLista { total = 0, lista = new List<TiposOrdenDto>() }
            );
        }

        /// <summary>
        /// Inserta o actualiza un tipo de orden según el estado de la fila.
        /// </summary>
        public ErrorDto Cpr_TiposOrden_Guardar(
            int codEmpresa,
            string usuario,
            TiposOrdenDto tipoOrden)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codigo = (tipoOrden.tipo_orden ?? string.Empty).Trim();
                var descripcion = (tipoOrden.descripcion ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descripcion))
                {
                    return DbHelper.ErrorResponse(
                        "Los campos Código y Descripción son requeridos.",
                        -2);
                }

                const string sqlExiste = @"
                    SELECT COUNT(1)
                    FROM cpr_Tipo_Orden
                    WHERE UPPER(Tipo_Orden) = UPPER(@TipoOrden);";

                var existe = conn.QuerySingle<int>(sqlExiste, new { TipoOrden = codigo });

                if (tipoOrden.isNew && existe > 0)
                {
                    return DbHelper.ErrorResponse(
                        $"El tipo de orden {codigo} ya existe.",
                        -2);
                }

                if (!tipoOrden.isNew && existe == 0)
                {
                    return DbHelper.ErrorResponse(
                        $"El tipo de orden {codigo} no existe.",
                        -2);
                }

                var movimiento = tipoOrden.isNew ? "Registra - WEB" : "Modifica - WEB";

                if (tipoOrden.isNew)
                {
                    const string sqlInsertar = @"
                        INSERT INTO cpr_Tipo_Orden
                            (Tipo_Orden, descripcion, activo)
                        VALUES
                            (@TipoOrden, @Descripcion, @Activo);";

                    conn.Execute(sqlInsertar, new
                    {
                        TipoOrden = codigo,
                        Descripcion = descripcion,
                        Activo = tipoOrden.activo
                    });
                }
                else
                {
                    const string sqlActualizar = @"
                        UPDATE cpr_Tipo_Orden
                        SET descripcion = @Descripcion,
                            activo = @Activo
                        WHERE Tipo_Orden = @TipoOrden;";

                    conn.Execute(sqlActualizar, new
                    {
                        TipoOrden = codigo,
                        Descripcion = descripcion,
                        Activo = tipoOrden.activo
                    });
                }

                Cpr_TiposOrden_Bitacora_Registrar(
                    codEmpresa,
                    usuario,
                    $"Tipo de Orden de Compra: {codigo} - {descripcion}",
                    movimiento);

                return DbHelper.OkResponse("Tipo de orden guardado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un tipo de orden y registra el movimiento en bitácora.
        /// </summary>
        public ErrorDto Cpr_TiposOrden_Eliminar(
            int codEmpresa,
            string usuario,
            string tipoOrden)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codigo = (tipoOrden ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    return DbHelper.ErrorResponse("El código es requerido.", -2);
                }

                const string sqlEliminar = @"
                    DELETE FROM cpr_Tipo_Orden
                    WHERE Tipo_Orden = @TipoOrden;";

                var filas = conn.Execute(sqlEliminar, new { TipoOrden = codigo });
                if (filas == 0)
                {
                    return DbHelper.ErrorResponse(
                        $"El tipo de orden {codigo} no existe.",
                        -2);
                }

                Cpr_TiposOrden_Bitacora_Registrar(
                    codEmpresa,
                    usuario,
                    $"Tipo de Orden de Compra: {codigo}",
                    "Elimina - WEB");

                return DbHelper.OkResponse("Tipo de orden eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto<List<RangosMontos>> rangosMontos_Obtener(int codEmpresa, string usuario)
        {
            return DbHelper.ExecuteListQuery<RangosMontos>(
                _portalDb,
                codEmpresa,
                @"SELECT
                      r.cod_rango as item,
                      CONCAT(r.descripcion, ' - Mínimo: ', r.monto_minimo, ' Máximo: ', r.monto_maximo) AS descripcion,
                      r.MONTO_MAXIMO, r.MONTO_MINIMO
                  FROM cpr_orden_rangos r
                  INNER JOIN CPR_RANGO_USUARIO u ON r.cod_rango = u.cod_rango
                  WHERE r.REGISTRO_USUARIO = @Usuario",
                new { Usuario = usuario }
            );
        }

        // ----------------- Helpers -----------------

        private static TipoOrdenFiltro SafeParseFiltro(string jFiltros)
        {
            try
            {
                return JsonConvert.DeserializeObject<TipoOrdenFiltro>(jFiltros) ?? new TipoOrdenFiltro();
            }
            catch
            {
                return new TipoOrdenFiltro();
            }
        }

        private static int Cpr_TiposOrden_SortColumn_Resolver(string? sortField)
        {
            return sortField?.Trim().ToLowerInvariant() switch
            {
                "descripcion" => 2,
                "activo" => 3,
                _ => 1
            };
        }

        private void Cpr_TiposOrden_Bitacora_Registrar(
            int codEmpresa,
            string usuario,
            string detalle,
            string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloCompras
            });
        }
    }
}
