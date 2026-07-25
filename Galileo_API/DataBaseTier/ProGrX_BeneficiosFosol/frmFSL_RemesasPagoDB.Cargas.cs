using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslRemesasPagoDB
    {
        /// <summary>
        /// Obtiene las remesas abiertas para cargas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de remesas.</returns>
        public ErrorDto<List<FslRemesasListaDatos>> FslCargas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = @"SELECT *, CONCAT(TESORERIA_REMESA, REGISTRO_USUARIO, REGISTRO_FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION
                                     FROM FSL_REMESAS_TESORERIA WHERE estado = 'A' ORDER BY registro_fecha DESC";
                return connection.Query<FslRemesasListaDatos>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los expedientes elegibles para carga (traslado a tesorería) en un rango de fechas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="fecha_inicio">Fecha inicial.</param>
        /// <param name="fecha_corte">Fecha de corte.</param>
        /// <param name="filtro">Filtro por expediente, cédula o nombre.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <returns>Lista de expedientes y total.</returns>
        public ErrorDto<FslCargasLista> FslCargasLista_Obtener(int CodEmpresa, string fecha_inicio, string fecha_corte, string? filtro, int? pagina, int? paginacion)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var response = new FslCargasLista();

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                const string whereClause = @"WHERE E.RESOLUCION_FECHA BETWEEN @fecha_inicio AND @fecha_corte
                                               AND E.TESORERIA_REMESA IS NULL AND E.Tipo_Desembolso = 'T'
                                               AND E.Estado = 'X' AND E.TOTAL_SOBRANTE > 0
                                               AND (@like IS NULL OR E.COD_EXPEDIENTE LIKE @like OR E.CEDULA LIKE @like
                                                    OR S.NOMBRE LIKE @like OR E.PRESENTA_NOMBRE LIKE @like)";

                var sqlCount = $@"SELECT COUNT(E.COD_EXPEDIENTE) FROM FSL_EXPEDIENTES E
                                  INNER JOIN SOCIOS S ON E.CEDULA = S.CEDULA {whereClause}";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, new { fecha_inicio, fecha_corte, like });

                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;
                var sql = $@"SELECT E.COD_EXPEDIENTE AS cod_expediente, E.CEDULA AS cedula, S.NOMBRE AS nombre,
                                    E.TOTAL_SOBRANTE AS total_sobrante, E.PRESENTA_CEDULA AS presenta_cedula, E.PRESENTA_NOMBRE AS presenta_nombre
                             FROM FSL_EXPEDIENTES E
                             INNER JOIN SOCIOS S ON E.CEDULA = S.CEDULA {whereClause}
                             ORDER BY E.CEDULA, S.NOMBRE
                             OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Lista = connection.Query<FslCargasListaData>(sql, new { fecha_inicio, fecha_corte, like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Aplica una remesa a los expedientes seleccionados, validando que esté abierta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cargas">JSON con la remesa y los casos.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslCargas_Aplicar(int CodEmpresa, string cargas)
        {
            var carga = JsonConvert.DeserializeObject<FslCargasAplicar>(cargas) ?? new FslCargasAplicar();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                var abierta = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) AS Existe FROM FSL_REMESAS_TESORERIA WHERE TESORERIA_REMESA = @cod_remesa AND estado = 'A'",
                    new { carga.cod_remesa });

                if (abierta == 0)
                {
                    return DbHelper.ErrorResponse("La Remesa actual; ya se encuentra cerrada...");
                }

                foreach (var item in carga.casos)
                {
                    connection.Execute(
                        "UPDATE FSL_EXPEDIENTES SET TESORERIA_REMESA = @cod_remesa WHERE COD_EXPEDIENTE = @cod_expediente",
                        new { carga.cod_remesa, item.cod_expediente });
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = carga.usuario.ToUpper(),
                    DetalleMovimiento = "update FSL_EXPEDIENTES set :" + carga.cod_remesa,
                    Movimiento = "APLICA - WEB",
                    Modulo = 7
                });

                return DbHelper.OkResponse("Proceso Realizado Satisfactoriamente...");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Cierra una remesa de cargas (estado 'C'), validando que esté abierta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <param name="usuario">Usuario que cierra.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslCargas_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                var abierta = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) AS Existe FROM FSL_REMESAS_TESORERIA WHERE TESORERIA_REMESA = @cod_remesa AND estado = 'A'",
                    new { cod_remesa });

                if (abierta == 0)
                {
                    return DbHelper.ErrorResponse("La Remesa actual; ya se encuentra cerrada...");
                }

                connection.Execute("UPDATE FSL_REMESAS_TESORERIA SET estado = 'C' WHERE TESORERIA_REMESA = @cod_remesa", new { cod_remesa });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = "Cierra Remesa Traslado a Tesoreria :" + cod_remesa,
                    Movimiento = "APLICA - WEB",
                    Modulo = 7
                });

                return DbHelper.OkResponse("Remesa cerrada correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
