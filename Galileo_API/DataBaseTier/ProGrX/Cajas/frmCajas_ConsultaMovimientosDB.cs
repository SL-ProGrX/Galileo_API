using Dapper;
using System.Text;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasConsultaMovimientosFormaPagoDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasConsultaMovimientosFormaPagoDB(IConfiguration? config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }
        /// <summary>
        /// Lista de movimientos formas de pago
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CajasMovimientosFormaPagoLista> Cajas_ConsultaMovimientos_FormaPago_ListaObtener(int CodEmpresa, FiltrosMovimientosFormaPago filtros)
        {
            var resp = new ErrorDto<CajasMovimientosFormaPagoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasMovimientosFormaPagoLista()
            };

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                var p = new DynamicParameters();
                const string todos = "TODOS";

                var where = BuildWhereClause(filtros, p, todos);

                resp.Result.Total = cn.Query<int>(BuildTotalQuery(where), p).FirstOrDefault();
                resp.Result.TotalMontoAplicado = cn.Query<decimal>(BuildTotalMontoAplicadoQuery(where), p).FirstOrDefault();
                resp.Result.Lista = cn.Query<CajasMovimientoFormaPagoItem>(BuildDataQuery(filtros, where, p), p).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        private static string BuildWhereClause(FiltrosMovimientosFormaPago filtros, DynamicParameters p, string todos)
        {
            var where = new StringBuilder(" WHERE 1 = 1 ");

            AddFechaFiltro(filtros, p, where);
            AddLikeFiltro(filtros.Cedula, "Cliente_Identificacion", "@Cedula", p, where);
            AddLikeFiltro(filtros.Nombre, "Cliente_Nombre", "@Nombre", p, where);
            AddFormaPagoFiltro(filtros, p, where, todos);
            AddLikeFiltro(filtros.NumDoc, "NUM_REFERENCIA", "@NumDoc", p, where);
            AddLikeFiltro(filtros.Usuario, "Registro_Usuario", "@Usuario", p, where);
            AddSaldosFavorFiltro(filtros, where);
            AddCajaFiltro(filtros, p, where, todos);
            AddAperturaFiltro(filtros, p, where);
            AgregarFiltroTipoMovimiento(where, filtros.TipoMov);
            AddEntidadPagoFiltro(filtros, p, where, todos);
            AddOrigenRecursosFiltro(filtros, p, where, todos);
            AddBusquedaGeneralFiltro(filtros, p, where);

            return where.ToString();
        }

        private static string BuildTotalQuery(string where)
        {
            return $@"
                    SELECT COUNT(1)
                    FROM vCaja_AfectacionFormaPago
                    {where};";
        }

        private static string BuildTotalMontoAplicadoQuery(string where)
        {
            return $@"
                    SELECT ISNULL(SUM(Monto_Aplicado), 0)
                    FROM vCaja_AfectacionFormaPago
                    {where};";
        }

        private static string BuildDataQuery(FiltrosMovimientosFormaPago filtros, string where, DynamicParameters p)
        {
            var sortColumn = ObtenerColumnaOrden(filtros.sortField, "registro_fecha");
            var sortDirection = filtros.sortOrder == 0 ? "DESC" : "ASC";
            var paging = BuildPagingClause(filtros, p);

            return $@"
                    SELECT
                        Cliente_Identificacion,
                        Cliente_Nombre,
                        TipoDocDesc,
                        Cod_Transaccion,
                        Monto_Doc,
                        Monto_Aplicado,
                        Cod_Divisa,
                        Tipo_Cambio,
                        REGISTRO_FECHA_FORMAT,
                        Registro_Usuario,
                        FormaPagoDesc,
                        Num_Referencia,
                        BancoDesc,
                        OrigenRecursoDesc,
                        EntidadPagoDesc,
                        cod_cuenta      AS Cod_Cuenta,
                        ConceptoDesc,
                        Cod_Caja,
                        Cod_Apertura
                    FROM vCaja_AfectacionFormaPago
                    {where}
                    ORDER BY {sortColumn} {sortDirection}
                    {paging};";
        }

        private static void AddFormaPagoFiltro(FiltrosMovimientosFormaPago filtros, DynamicParameters p, StringBuilder where, string todos)
        {
            if (string.IsNullOrWhiteSpace(filtros.CodFormaPago) ||
                filtros.CodFormaPago.Trim().Equals(todos, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            where.Append(" AND COD_FORMA_PAGO IN (@CodFormaPago, 'SF') ");
            p.Add("@CodFormaPago", filtros.CodFormaPago.Trim());
        }

        private static void AddSaldosFavorFiltro(FiltrosMovimientosFormaPago filtros, StringBuilder where)
        {
            if (!filtros.MostrarSaldosFavorRelacionados)
            {
                where.Append(" AND COD_FORMA_PAGO NOT IN ('SF') ");
            }
        }

        private static void AddCajaFiltro(FiltrosMovimientosFormaPago filtros, DynamicParameters p, StringBuilder where, string todos)
        {
            if (string.IsNullOrWhiteSpace(filtros.CodCaja) ||
                filtros.CodCaja.Trim().Equals(todos, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            where.Append(" AND COD_CAJA = @CodCaja ");
            p.Add("@CodCaja", filtros.CodCaja.Trim());
        }

        private static void AddAperturaFiltro(FiltrosMovimientosFormaPago filtros, DynamicParameters p, StringBuilder where)
        {
            if (!filtros.CodApertura.HasValue || filtros.CodApertura.Value <= 0)
            {
                return;
            }

            where.Append(" AND COD_APERTURA = @CodApertura ");
            p.Add("@CodApertura", filtros.CodApertura.Value);
        }

        private static void AddEntidadPagoFiltro(FiltrosMovimientosFormaPago filtros, DynamicParameters p, StringBuilder where, string todos)
        {
            if (string.IsNullOrWhiteSpace(filtros.CodEntidadPago) ||
                filtros.CodEntidadPago.Trim().Equals(todos, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            where.Append(" AND COD_ENTIDAD_PAGO = @CodEntidadPago ");
            p.Add("@CodEntidadPago", filtros.CodEntidadPago.Trim());
        }

        private static void AddOrigenRecursosFiltro(FiltrosMovimientosFormaPago filtros, DynamicParameters p, StringBuilder where, string todos)
        {
            if (string.IsNullOrWhiteSpace(filtros.CodOrigenRecursos) ||
                filtros.CodOrigenRecursos.Trim().Equals(todos, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            where.Append(" AND COD_ORIGEN_RECURSOS = @CodOrigenRecursos ");
            p.Add("@CodOrigenRecursos", filtros.CodOrigenRecursos.Trim());
        }

        private static void AddBusquedaGeneralFiltro(FiltrosMovimientosFormaPago filtros, DynamicParameters p, StringBuilder where)
        {
            if (string.IsNullOrWhiteSpace(filtros.filtro))
            {
                return;
            }

            where.Append(@"
                    AND (
                           Cliente_Identificacion LIKE @Filtro
                        OR Cliente_Nombre         LIKE @Filtro
                        OR Num_Referencia         LIKE @Filtro
                        OR Registro_Usuario       LIKE @Filtro
                    )");
            p.Add("@Filtro", $"%{filtros.filtro.Trim()}%");
        }

        private static void AddFechaFiltro(FiltrosMovimientosFormaPago filtros, DynamicParameters p, StringBuilder where)
        {
            if (!filtros.TodasLasFechas &&
                !string.IsNullOrWhiteSpace(filtros.FechaInicio) &&
                !string.IsNullOrWhiteSpace(filtros.FechaCorte))
            {
                where.Append(" AND registro_fecha BETWEEN @FechaInicio AND @FechaCorte ");
                p.Add("@FechaInicio", DateTime.ParseExact($"{filtros.FechaInicio} 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                p.Add("@FechaCorte", DateTime.ParseExact($"{filtros.FechaCorte} 23:59:59", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                return;
            }

            where.Append(" AND registro_fecha BETWEEN @FechaMin AND GETDATE() ");
            p.Add("@FechaMin", new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
        }

        private static void AddLikeFiltro(string? valor, string campo, string paramName, DynamicParameters p, StringBuilder where)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return;
            }

            where.Append(" AND ");
            where.Append(campo);
            where.Append(" LIKE ");
            where.Append(paramName);
            where.Append(' ');
            p.Add(paramName, $"%{valor.Trim()}%");
        }

        private static string BuildPagingClause(FiltrosMovimientosFormaPago filtros, DynamicParameters p)
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

        private static void AgregarFiltroTipoMovimiento(StringBuilder where, string? tipoMov)
        {
            var mov = (tipoMov ?? "T").Trim().ToUpperInvariant();
            if (mov.StartsWith('E'))
            {
                where.Append(" AND Monto_Aplicado >= 0 ");
            }
            else if (mov.StartsWith('S'))
            {
                where.Append(" AND Monto_Aplicado < 0 ");
            }
        }

        private static string ObtenerColumnaOrden(string? sortField, string defaultSortColumn)
        {
            if (string.IsNullOrWhiteSpace(sortField))
            {
                return defaultSortColumn;
            }

            return sortField.Trim().ToUpperInvariant() switch
            {
                "CLIENTE_IDENTIFICACION" => "Cliente_Identificacion",
                "CLIENTE_NOMBRE" => "Cliente_Nombre",
                "COD_TRANSACCION" => "Cod_Transaccion",
                "MONTO_DOC" => "Monto_Doc",
                "MONTO_APLICADO" => "Monto_Aplicado",
                "COD_DIVISA" => "Cod_Divisa",
                "TIPO_CAMBIO" => "Tipo_Cambio",
                "REGISTRO_USUARIO" => "Registro_Usuario",
                "REGISTRO_FECHA" => defaultSortColumn,
                _ => defaultSortColumn
            };
        }

        /// <summary>
        /// Obtiene última apertura para una caja
        /// <param name="CodEmpresa"></param>
        /// <param name="CodCaja"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<long> Cajas_UltimaApertura_Obtener(int CodEmpresa, string CodCaja)
        {
            var resp = new ErrorDto<long> { Code = 0, Description = "Ok", Result = 0 };

            try
            {
                const string sql = "SELECT dbo.fxSIFDocsCajaUltimaApertura(@CodCaja) AS Resultado;";
                var result = DbHelper.ExecuteSingleQuery<long>(_portalDb, CodEmpresa, sql, 0, new { CodCaja });

                resp.Code = result.Code;
                resp.Description = result.Description;
                resp.Result = result.Result;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene el catalogo de formas de pago
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_FormasPago_DropDown_Obtener(int CodEmpresa)
        {
            const string sql = @"
                    SELECT RTRIM(COD_FORMA_PAGO) AS item,
                           RTRIM(DESCRIPCION)     AS descripcion
                    FROM SIF_FORMAS_PAGO
                    WHERE ACTIVA = 1
                    ORDER BY COD_FORMA_PAGO;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene el catalogo de definicion de cajas
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Cajas_DropDown_Obtener(int CodEmpresa)
        {
            const string sql = @"
                    SELECT RTRIM(cod_caja)     AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM cajas_definicion
                    WHERE activa = 1
                    ORDER BY cod_caja;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene el catalogo de entidades pagadoras
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_EntidadesPagadoras_DropDown_Obtener(int CodEmpresa)
        {
            const string sql = @"
                    SELECT RTRIM(COD_ENTIDAD_PAGO) AS item,
                           RTRIM(DESCRIPCION)     AS descripcion
                    FROM SIF_ENTIDADES_PAGO
                    WHERE ACTIVA = 1
                    ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene el catalogo de origen de recursos
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_OrigenRecursos_DropDown_Obtener(int CodEmpresa)
        {
            const string sql = @"
                    SELECT RTRIM(COD_ORIGEN_RECURSOS) AS item,
                           RTRIM(DESCRIPCION)        AS descripcion
                    FROM SIF_ORIGEN_RECURSOS
                    WHERE ACTIVA = 1
                    ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }
    }
}