using Dapper;
using System.Text;
using System.Globalization;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasAutorizacionSaldosEnTransitoDB
    {
        private readonly PortalDB _portalDb;
        public FrmCajasAutorizacionSaldosEnTransitoDB(IConfiguration? config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }
        /// <summary>
        /// Lista de saldos a favor en transito.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CajasSaldosFavorLista> Cajas_SaldosFavor_ListaObtener(int CodEmpresa, FiltrosSaldosFavorTransito filtros)
        {
            var resp = new ErrorDto<CajasSaldosFavorLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasSaldosFavorLista
                {
                    Total = 0,
                    Lista = new List<CajasSaldosFavorItem>()
                }
            };

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var p = new DynamicParameters();

                var where = BuildWhereClauseAndParameters(filtros, p);
                var qTotal = BuildTotalQuery(where);
                resp.Result.Total = cn.Query<int>(qTotal, p).FirstOrDefault();

                var qDatos = BuildDataQuery(filtros, where, p);
                resp.Result.Lista = cn.Query<CajasSaldosFavorItem>(qDatos, p).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.Lista = new List<CajasSaldosFavorItem>();
                resp.Result.Total = 0;
            }

            return resp;
        }

        private static string BuildWhereClauseAndParameters(FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            var where = new StringBuilder(@"
                WHERE 1 = 1
                  AND Saldo > 0
                  AND VALIDA_REQUIERE = 1
                ");

            AddEstadoFilter(where, filtros, p);
            AddFechaFilter(where, filtros, p);
            AddBusquedaGeneralFilter(where, filtros, p);
            AddCedulaFilter(where, filtros, p);
            AddNombreFilter(where, filtros, p);
            AddTipoDocumentoFilter(where, filtros, p);
            AddNumeroDocumentoFilter(where, filtros, p);
            AddUsuarioRegistroFilter(where, filtros, p);
            AddEntidadPagadoraFilter(where, filtros, p);
            AddOrigenRecursosFilter(where, filtros, p);
            AddMontoFilter(where, filtros, p);

            return where.ToString();
        }

        private static string BuildTotalQuery(string where)
        {
            return $@"
                SELECT COUNT(1)
                FROM vCajas_Saldos_Favor
                {where};";
        }

        private static string BuildDataQuery(FiltrosSaldosFavorTransito filtros, string where, DynamicParameters p)
        {
            var sortColumn = ObtenerColumnaOrden(filtros.sortField, "REGISTRO_FECHA");
            var sortDirection = filtros.sortOrder == 0 ? "DESC" : "ASC";
            var paging = BuildPagingClause(filtros, p);

            return $@"
                SELECT
                      Linea,
                      Cedula,
                      Nombre,
                      Doc_Tipo,
                      Doc_Numero,
                      Monto,
                      Saldo,
                      Cod_Divisa,
                      REGISTRO_FECHA_FORMAT,
                      Registro_Usuario,
                      Liq_Fecha,
                      Liq_Usuario,
                      Liq_Monto,
                      Liq_NSolicitud,
                      Liq_Plan,
                      Liq_Contrato,
                      Liq_Tipo_Doc,
                      Liq_Num_Doc,
                      BancoDesc,
                      EntidadPagoDesc,
                      OrigenRecursoDesc,
                      Autoriza_Estado_Desc,
                      Valida_Usuario,
                      Valida_Fecha,
                      Valida_Notas
                FROM vCajas_Saldos_Favor
                {where}
                ORDER BY {sortColumn} {sortDirection}
                {paging};";
        }

        private static string BuildPagingClause(FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (filtros.paginacion <= 0)
            {
                return string.Empty;
            }

            var pageIndex = filtros.pagina <= 0 ? 0 : filtros.pagina - 1;
            var offset = pageIndex * filtros.paginacion;

            p.Add("@Offset", offset);
            p.Add("@PageSize", filtros.paginacion);

            return " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY ";
        }

        private static void AddEstadoFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.Estado))
            {
                return;
            }

            where.Append(" AND ISNULL(VALIDA_ESTADO,'P') = @Estado ");
            p.Add("@Estado", filtros.Estado.Trim().Substring(0, 1));
        }

        private static void AddFechaFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (filtros.TodasLasFechas ||
                string.IsNullOrWhiteSpace(filtros.FechaInicio) ||
                string.IsNullOrWhiteSpace(filtros.FechaCorte))
            {
                return;
            }

            where.Append(" AND REGISTRO_FECHA BETWEEN @FechaInicio AND @FechaCorte ");
            p.Add("@FechaInicio", DateTime.ParseExact($"{filtros.FechaInicio} 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            p.Add("@FechaCorte", DateTime.ParseExact($"{filtros.FechaCorte} 23:59:59", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        private static void AddBusquedaGeneralFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.filtro))
            {
                return;
            }

            where.Append(@"
                     AND (
                            Cedula           LIKE @Filtro
                         OR Nombre           LIKE @Filtro
                         OR Doc_Numero       LIKE @Filtro
                         OR Registro_Usuario LIKE @Filtro
                        )");
            p.Add("@Filtro", $"%{filtros.filtro.Trim()}%");
        }

        private static void AddCedulaFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.Cedula))
            {
                return;
            }

            where.Append(" AND Cedula LIKE @Cedula ");
            p.Add("@Cedula", $"%{filtros.Cedula.Trim()}%");
        }

        private static void AddNombreFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.Nombre))
            {
                return;
            }

            where.Append(" AND ISNULL(Nombre,'') LIKE @Nombre ");
            p.Add("@Nombre", $"%{filtros.Nombre.Trim()}%");
        }

        private static void AddTipoDocumentoFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.TipoDocumento) || IsTodos(filtros.TipoDocumento))
            {
                return;
            }

            where.Append(" AND Doc_Tipo = @DocTipo ");
            p.Add("@DocTipo", filtros.TipoDocumento.Trim());
        }

        private static void AddNumeroDocumentoFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.NumeroDocumento))
            {
                return;
            }

            where.Append(" AND Doc_Numero LIKE @NumDoc ");
            p.Add("@NumDoc", $"%{filtros.NumeroDocumento.Trim()}%");
        }

        private static void AddUsuarioRegistroFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.UsuarioRegistro))
            {
                return;
            }

            where.Append(" AND Registro_Usuario LIKE @UsuarioReg ");
            p.Add("@UsuarioReg", $"%{filtros.UsuarioRegistro.Trim()}%");
        }

        private static void AddEntidadPagadoraFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.EntidadPagadora) || IsTodos(filtros.EntidadPagadora))
            {
                return;
            }

            where.Append(" AND COD_ENTIDAD_PAGO = @EntidadPago ");
            p.Add("@EntidadPago", filtros.EntidadPagadora.Trim());
        }

        private static void AddOrigenRecursosFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            if (string.IsNullOrWhiteSpace(filtros.OrigenRecursos) || IsTodos(filtros.OrigenRecursos))
            {
                return;
            }

            where.Append(" AND COD_ORIGEN_RECURSOS = @OrigenRec ");
            p.Add("@OrigenRec", filtros.OrigenRecursos.Trim());
        }

        private static void AddMontoFilter(StringBuilder where, FiltrosSaldosFavorTransito filtros, DynamicParameters p)
        {
            where.Append(" AND Monto BETWEEN @MontoDesde AND @MontoHasta ");
            p.Add("@MontoDesde", filtros.MontoDesde);
            p.Add("@MontoHasta", filtros.MontoHasta);
        }

        private static bool IsTodos(string value)
        {
            return string.Equals(value.Trim(), "TODOS", StringComparison.OrdinalIgnoreCase);
        }

        private static string ObtenerColumnaOrden(string? sortField, string defaultSortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortField))
            {
                return defaultSortColumn;
            }

            return sortField.Trim().ToUpperInvariant() switch
            {
                "CEDULA" => "Cedula",
                "NOMBRE" => "Nombre",
                "DOC_TIPO" => "Doc_Tipo",
                "DOC_NUMERO" => "Doc_Numero",
                "MONTO" => "Monto",
                "SALDO" => "Saldo",
                "REGISTRO_USUARIO" => "Registro_Usuario",
                "REGISTRO_FECHA" => defaultSortColumn,
                _ => defaultSortColumn
            };
        }

        /// <summary>
        /// Lista de tipos de documentos
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_SaldosFavor_Tipos_Obtener(int CodEmpresa)
        {
            const string sql = @"
SELECT RTRIM(DOC_TIPO) AS item,
       RTRIM(DESCRIPCION) AS descripcion
FROM CAJAS_SALDOS_FAVOR_TIPOS
WHERE ACTIVO = 1
ORDER BY DOC_TIPO;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }
        /// <summary>
        /// Lista de entidades pagadoras
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_EntidadesPagadoras_Obtener(int CodEmpresa)
        {
            const string sql = @"
SELECT RTRIM(COD_ENTIDAD_PAGO) AS item,
       RTRIM(DESCRIPCION) AS descripcion
FROM SIF_ENTIDADES_PAGO
WHERE ACTIVA = 1
ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }
        /// <summary>
        /// Lista de origen de recursos
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_OrigenRecursos_Obtener(int CodEmpresa)
        {
            const string sql = @"
SELECT RTRIM(COD_ORIGEN_RECURSOS) AS item,
       RTRIM(DESCRIPCION) AS descripcion
FROM SIF_ORIGEN_RECURSOS
WHERE ACTIVA = 1
ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }
        /// <summary>
        /// Ejecuta la autorizacion o deniega el salgo a favor en transito
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Cajas_SaldosFavor_Autoriza(int CodEmpresa, CajasSaldosFavorAutorizaRequest req)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (req.SaldoFavorIds == null || req.SaldoFavorIds.Count == 0)
                {
                    resp.Code = -1;
                    resp.Description = "No se recibió ninguna línea para autorizar/denegar.";
                    return resp;
                }

                using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                cn.Open();

                using var tx = cn.BeginTransaction();

                try
                {
                    foreach (var linea in req.SaldoFavorIds)
                    {
                        var p = new DynamicParameters();
                        p.Add("@SaldoFavorId", linea);
                        p.Add("@Estado", req.Estado);
                        p.Add("@Usuario", req.Usuario);
                        p.Add("@Notas", req.Notas);

                        cn.Execute(
                            "spCajas_ValoresTransito_Autoriza",
                            p,
                            transaction: tx,
                            commandType: CommandType.StoredProcedure
                        );
                    }

                    tx.Commit();
                }
                catch (Exception exTx)
                {
                    tx.Rollback();
                    resp.Code = -1;
                    resp.Description = exTx.Message;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene cédula juridica y nombre de la empresa
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CajasEmpresaInfoDto> Cajas_SaldosFavor_EmpresaInfo_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<CajasEmpresaInfoDto>
            {
                Code = 0,
                Description = "Ok",
                Result = default!
            };

            try
            {
                const string sql = @"
SELECT TOP 1 
       RTRIM(NOMBRE)          AS Nombre,
       RTRIM(CEDULA_JURIDICA) AS Cedula_Juridica
FROM SIF_EMPRESA;";

                var data = DbHelper.ExecuteSingleQuery<CajasEmpresaInfoDto?>(
                    _portalDb,
                    CodEmpresa,
                    sql,
                    default);

                if (data.Code != 0)
                {
                    resp.Code = data.Code;
                    resp.Description = data.Description;
                    resp.Result = null;
                }
                else if (data.Result == null)
                {
                    resp.Code = -1;
                    resp.Description = "No se encontró información de empresa.";
                    resp.Result = null;
                }
                else
                {
                    resp.Result = data.Result;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }
    }
}
