using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
                const string defaultSortColumn = "registro_fecha";
                const string todos = "TODOS";

                string where = BuildWhereClause(filtros, p, todos);

                var qTotal = $"SELECT COUNT(1) FROM vCaja_AfectacionFormaPago {where};";
                resp.Result.Total = cn.Query<int>(qTotal, p).FirstOrDefault();

                var qSum = $"SELECT ISNULL(SUM(Monto_Aplicado),0) FROM vCaja_AfectacionFormaPago {where};";
                resp.Result.TotalMontoAplicado = cn.Query<decimal>(qSum, p).FirstOrDefault();

                var sortField = string.IsNullOrWhiteSpace(filtros.sortField) ? defaultSortColumn : filtros.sortField.Trim();
                var orden = filtros.sortOrder == 0 ? "DESC" : "ASC";
                var sortColumn = ObtenerColumnaOrden(sortField, defaultSortColumn);
                var orderBy = $" ORDER BY {sortColumn} {orden} ";

                string paging = BuildPagingClause(filtros);

                var qDatos = $@"
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
                    {orderBy}
                    {paging};";

                resp.Result.Lista = cn.Query<CajasMovimientoFormaPagoItem>(qDatos, p).ToList();
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
            var where = " WHERE 1 = 1 ";

            AddFechaFiltro(filtros, p, ref where);
            AddLikeFiltro(filtros.Cedula, "Cliente_Identificacion", "@Cedula", p, ref where);
            AddLikeFiltro(filtros.Nombre, "Cliente_Nombre", "@Nombre", p, ref where);

            if (!string.IsNullOrWhiteSpace(filtros.CodFormaPago) &&
                !filtros.CodFormaPago.Trim().Equals(todos, StringComparison.OrdinalIgnoreCase))
            {
                where += " AND COD_FORMA_PAGO IN (@CodFormaPago, 'SF') ";
                p.Add("@CodFormaPago", filtros.CodFormaPago.Trim());
            }

            AddLikeFiltro(filtros.NumDoc, "NUM_REFERENCIA", "@NumDoc", p, ref where);
            AddLikeFiltro(filtros.Usuario, "Registro_Usuario", "@Usuario", p, ref where);

            if (!filtros.MostrarSaldosFavorRelacionados)
            {
                where += " AND COD_FORMA_PAGO NOT IN ('SF') ";
            }

            if (!string.IsNullOrWhiteSpace(filtros.CodCaja) &&
                !filtros.CodCaja.Trim().Equals(todos, StringComparison.OrdinalIgnoreCase))
            {
                where += " AND COD_CAJA IN (@CodCaja) ";
                p.Add("@CodCaja", filtros.CodCaja.Trim());
            }

            if (filtros.CodApertura.HasValue && filtros.CodApertura.Value > 0)
            {
                where += " AND COD_APERTURA = @CodApertura ";
                p.Add("@CodApertura", filtros.CodApertura.Value);
            }

            AgregarFiltroTipoMovimiento(ref where, filtros.TipoMov);

            if (!string.IsNullOrWhiteSpace(filtros.CodEntidadPago) &&
                !filtros.CodEntidadPago.Trim().Equals(todos, StringComparison.OrdinalIgnoreCase))
            {
                where += " AND COD_ENTIDAD_PAGO = @CodEntidadPago ";
                p.Add("@CodEntidadPago", filtros.CodEntidadPago.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filtros.CodOrigenRecursos) &&
                !filtros.CodOrigenRecursos.Trim().Equals(todos, StringComparison.OrdinalIgnoreCase))
            {
                where += " AND COD_ORIGEN_RECURSOS = @CodOrigenRecursos ";
                p.Add("@CodOrigenRecursos", filtros.CodOrigenRecursos.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filtros.filtro))
            {
                where += @"
                    AND (
                           Cliente_Identificacion LIKE @Filtro
                        OR Cliente_Nombre         LIKE @Filtro
                        OR Num_Referencia         LIKE @Filtro
                        OR Registro_Usuario       LIKE @Filtro
                    )";
                p.Add("@Filtro", "%" + filtros.filtro.Trim() + "%");
            }

            return where;
        }

        private static void AddFechaFiltro(FiltrosMovimientosFormaPago filtros, DynamicParameters p, ref string where)
        {
            if (!filtros.TodasLasFechas &&
                !string.IsNullOrWhiteSpace(filtros.FechaInicio) &&
                !string.IsNullOrWhiteSpace(filtros.FechaCorte))
            {
                where += " AND registro_fecha BETWEEN @FechaInicio AND @FechaCorte ";
                p.Add("@FechaInicio", DateTime.ParseExact($"{filtros.FechaInicio} 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                p.Add("@FechaCorte", DateTime.ParseExact($"{filtros.FechaCorte} 23:59:59", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            }
            else
            {
                where += " AND registro_fecha BETWEEN @FechaMin AND GETDATE() ";
                p.Add("@FechaMin", new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
            }
        }

        private static void AddLikeFiltro(string? valor, string campo, string paramName, DynamicParameters p, ref string where)
        {
            if (!string.IsNullOrWhiteSpace(valor))
            {
                where += $" AND {campo} LIKE {paramName} ";
                p.Add(paramName, "%" + valor.Trim() + "%");
            }
        }

        private static string BuildPagingClause(FiltrosMovimientosFormaPago filtros)
        {
            if (filtros.paginacion <= 0)
                return "";

            var pageIndex = filtros.pagina <= 0 ? 0 : filtros.pagina - 1;
            var offset = pageIndex * filtros.paginacion;
            return $" OFFSET {offset} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
        }

        private static void AgregarFiltroTipoMovimiento(ref string where, string? tipoMov)
        {
            var mov = (tipoMov ?? "T").Trim().ToUpperInvariant();
            if (mov.StartsWith('E'))
            {
                where += " AND Monto_Aplicado >= 0 ";
            }
            else if (mov.StartsWith('S'))
            {
                where += " AND Monto_Aplicado < 0 ";
            }
        }

        private static string ObtenerColumnaOrden(string sortField, string defaultSortColumn)
        {
            return sortField.ToUpperInvariant() switch
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
