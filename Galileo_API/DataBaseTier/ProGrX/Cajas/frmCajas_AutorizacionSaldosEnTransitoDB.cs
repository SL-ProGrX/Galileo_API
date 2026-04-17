using Dapper;
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
                const string defaultSortColumn = "REGISTRO_FECHA";

                // Extracted filter logic to helper
                var where = BuildWhereClauseAndParameters(filtros, p);

                var qTotal = $"SELECT COUNT(1) FROM vCajas_Saldos_Favor {where};";
                resp.Result.Total = cn.Query<int>(qTotal, p).FirstOrDefault();

                var sortField = string.IsNullOrWhiteSpace(filtros.sortField)
                    ? defaultSortColumn
                    : filtros.sortField.Trim();

                var orden = filtros.sortOrder == 0 ? "DESC" : "ASC";
                var sortColumn = ObtenerColumnaOrden(sortField, defaultSortColumn);
                var orderBy = $" ORDER BY {sortColumn} {orden} ";
                bool sinPaginacion = filtros.paginacion <= 0;
                string paging = "";

                if (!sinPaginacion)
                {
                    var pageIndex = filtros.pagina <= 0 ? 0 : filtros.pagina - 1;
                    var offset = pageIndex * filtros.paginacion;
                    paging = $" OFFSET {offset} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                }

                var qDatos = $@"
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
                {orderBy}
                {paging};";

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
            var where = @"
                WHERE 1 = 1
                  AND Saldo > 0
                  AND VALIDA_REQUIERE = 1
                ";

            if (!string.IsNullOrWhiteSpace(filtros.Estado))
            {
                where += " AND ISNULL(VALIDA_ESTADO,'P') = @Estado ";
                p.Add("@Estado", filtros.Estado.Trim().Substring(0, 1));
            }
            if (!filtros.TodasLasFechas &&
                !string.IsNullOrWhiteSpace(filtros.FechaInicio) &&
                !string.IsNullOrWhiteSpace(filtros.FechaCorte))
            {
                where += " AND REGISTRO_FECHA BETWEEN @FechaInicio AND @FechaCorte ";
                p.Add("@FechaInicio", DateTime.ParseExact($"{filtros.FechaInicio} 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                p.Add("@FechaCorte", DateTime.ParseExact($"{filtros.FechaCorte} 23:59:59", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            }
            if (!string.IsNullOrWhiteSpace(filtros.filtro))
            {
                where += @"
                     AND (
                            Cedula           LIKE @Filtro
                         OR Nombre           LIKE @Filtro
                         OR Doc_Numero       LIKE @Filtro
                         OR Registro_Usuario LIKE @Filtro
                        )";
                p.Add("@Filtro", "%" + filtros.filtro.Trim() + "%");
            }
            if (!string.IsNullOrWhiteSpace(filtros.Cedula))
            {
                where += " AND Cedula LIKE @Cedula ";
                p.Add("@Cedula", "%" + filtros.Cedula.Trim() + "%");
            }
            if (!string.IsNullOrWhiteSpace(filtros.Nombre))
            {
                where += " AND ISNULL(Nombre,'') LIKE @Nombre ";
                p.Add("@Nombre", "%" + filtros.Nombre.Trim() + "%");
            }
            if (!string.IsNullOrWhiteSpace(filtros.TipoDocumento) &&
                filtros.TipoDocumento.Trim().ToUpper() != "TODOS")
            {
                where += " AND Doc_Tipo = @DocTipo ";
                p.Add("@DocTipo", filtros.TipoDocumento.Trim());
            }
            if (!string.IsNullOrWhiteSpace(filtros.NumeroDocumento))
            {
                where += " AND Doc_Numero LIKE @NumDoc ";
                p.Add("@NumDoc", "%" + filtros.NumeroDocumento.Trim() + "%");
            }
            if (!string.IsNullOrWhiteSpace(filtros.UsuarioRegistro))
            {
                where += " AND Registro_Usuario LIKE @UsuarioReg ";
                p.Add("@UsuarioReg", "%" + filtros.UsuarioRegistro.Trim() + "%");
            }
            if (!string.IsNullOrWhiteSpace(filtros.EntidadPagadora) &&
                filtros.EntidadPagadora.Trim().ToUpper() != "TODOS")
            {
                where += " AND COD_ENTIDAD_PAGO = @EntidadPago ";
                p.Add("@EntidadPago", filtros.EntidadPagadora.Trim());
            }
            if (!string.IsNullOrWhiteSpace(filtros.OrigenRecursos) &&
                filtros.OrigenRecursos.Trim().ToUpper() != "TODOS")
            {
                where += " AND COD_ORIGEN_RECURSOS = @OrigenRec ";
                p.Add("@OrigenRec", filtros.OrigenRecursos.Trim());
            }
            where += " AND Monto BETWEEN @MontoDesde AND @MontoHasta ";
            p.Add("@MontoDesde", filtros.MontoDesde);
            p.Add("@MontoHasta", filtros.MontoHasta);

            return where;
        }

        private static string ObtenerColumnaOrden(string sortField, string defaultSortColumn)
        {
            return sortField.ToUpperInvariant() switch
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
