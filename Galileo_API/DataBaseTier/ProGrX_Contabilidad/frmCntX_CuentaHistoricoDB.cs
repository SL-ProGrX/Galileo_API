using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXCuentaHistoricoDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXCuentaHistoricoDb(IConfiguration config)
            : this(new PortalDB(config)) { }

        public FrmCntXCuentaHistoricoDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Obtiene las unidades de negocio para el historico de cuentas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentaHistorico_Unidades_Obtener(int codEmpresa, int codConta)
        {
            string query = @"select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion 
                from CntX_Unidades where cod_contabilidad = @codConta";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Obtiene los centros de costos para el historico de cuentas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentaHistorico_CentroCostos_Obtener(int codEmpresa, int codConta, string codUnidad)
        {
            string query = @"select rtrim(cod_centro_costo) as item, rtrim(descripcion) as descripcion 
                from CntX_Centro_Costos where cod_contabilidad = @codConta";
            object param;
            if (codUnidad == "[CONSOLIDADO]")
            {
                param = new { codConta };
            }
            else
            {
                query += @" and cod_centro_costo in ( select cod_centro_costo from CntX_Unidades_CC 
                        where cod_contabilidad = @codConta and cod_unidad = @codUnidad)";
                param = new { codConta, codUnidad };
            }
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, param);
        }

        /// <summary>
        /// Obtiene el historico de una cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="cuenta"></param>
        /// <param name="codUnidad"></param>
        /// <param name="codCentroCosto"></param>
        /// <param name="rbOpcion"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXCuentaHistoricoData>> CntXCuentaHistorico_Obtener(
            int codEmpresa, int codConta, string cuenta, string codUnidad, string codCentroCosto, int rbOpcion)
        {
            bool consolidado = codUnidad == "[CONSOLIDADO]";
            bool ccTodos = codCentroCosto == "TODOS";

            int periodoAnio = DateTime.Now.Year;
            int periodoMes = DateTime.Now.Month;

            var fechaDto = ObtenerFechaActual(_portalDb, codEmpresa, periodoAnio, periodoMes);
            if (fechaDto == null || (fechaDto.Code < 0))
            {
                return new ErrorDto<List<CntXCuentaHistoricoData>>
                {
                    Code = fechaDto?.Code,
                    Description = fechaDto?.Description ?? "Error al obtener la fecha.",
                    Result = null
                };
            }

            DateTime pFecha = fechaDto.Result;
            string fromSql = ObtenerFromSql(consolidado, ccTodos);
            string query = GenerarQuery(consolidado, ccTodos, fromSql);

            AgregarFiltros(ref query, consolidado, ccTodos);

            DateTime desde = pFecha.AddMonths(-24);
            DateTime hasta = pFecha.Date.AddDays(1).AddTicks(-1);

            AgregarFiltroPeriodo(ref query, rbOpcion);

            query += " order by M.anio DESC, M.mes DESC";

            return DbHelper.ExecuteListQuery<CntXCuentaHistoricoData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    codConta,
                    cuenta,
                    codUnidad,
                    codCentroCosto,
                    desde,
                    hasta,
                    periodoMes
                });
        }

        private static ErrorDto<DateTime> ObtenerFechaActual(PortalDB portalDb, int codEmpresa, int anio, int mes)
        {
            string qFecha = @"select dbo.fxSys_FechaAnioMesToDatetime(@anio, @mes) as Fecha";
            return DbHelper.ExecuteSingleQuery<DateTime>(
                portalDb,
                codEmpresa,
                qFecha,
                DateTime.Now,
                new { anio, mes });
        }

        private static string ObtenerFromSql(bool consolidado, bool ccTodos) =>
        (consolidado, ccTodos) switch
        {
            (true, true) => "vCntX_Mov_Cuentas_General",
            (true, false) => "vCntX_Mov_Cuentas_CentroCosto",
            (false, true) => "vCntX_Mov_Cuentas_Unidad",
            _ => "CntX_Mov_Cuentas_Detallado"
        };

        private static string GenerarQuery(bool consolidado, bool ccTodos, string fromSql)
        {
            string codUnidadSelect = (consolidado && ccTodos) ? "''" : "M.cod_unidad";

            if (!consolidado && !ccTodos)
            {
                return $@"
                select 
                    M.Anio as anio,
                    M.Mes as mes,
                    C.cod_Cuenta_Mask as cod_cuenta_mask,
                    M.cod_unidad as cod_unidad,
                    M.cod_Centro_Costo as cod_centro_costo,
                    M.Saldo_Inicial as saldo_inicial,
                    abs(M.Total_Debitos) as debitos,
                    abs(M.Total_Creditos) as creditos,
                    (M.Total_Debitos + M.Total_Creditos) as neto_mes,
                    (M.SALDO_Inicial + M.Total_Debitos + M.Total_Creditos) as saldofinal
                from {fromSql} M
                inner join CntX_Cuentas C
                    on M.cod_Contabilidad = C.cod_Contabilidad
                   and M.cod_Cuenta = C.cod_Cuenta
                inner join CntX_Periodos P
                    on M.cod_Contabilidad = P.cod_Contabilidad
                   and M.Anio = P.Anio
                   and M.Mes = P.Mes
                where M.cod_cuenta = @cuenta
                  and M.cod_contabilidad = @codConta
            ";
            }
            else
            {
                string codCentroCostoSelect = ccTodos ? "''" : "M.cod_Centro_Costo";
                return $@"
                select 
                    M.Anio as anio,
                    M.Mes as mes,
                    C.cod_Cuenta_Mask as cod_cuenta_mask,
                    {codUnidadSelect} as cod_unidad,
                    {codCentroCostoSelect} as cod_centro_costo,
                    M.Saldo_Inicial as saldo_inicial,
                    abs(M.Total_Debitos) as debitos,
                    abs(M.Total_Creditos) as creditos,
                    M.Neto_Mes as neto_mes,
                    M.SALDO_Final as saldofinal
                from {fromSql} M
                inner join CntX_Cuentas C
                    on M.cod_Contabilidad = C.cod_Contabilidad
                   and M.cod_Cuenta = C.cod_Cuenta
                inner join CntX_Periodos P
                    on M.cod_Contabilidad = P.cod_Contabilidad
                   and M.Anio = P.Anio
                   and M.Mes = P.Mes
                where M.cod_cuenta = @cuenta
                  and M.cod_contabilidad = @codConta
            ";
            }
        }

        private static void AgregarFiltros(ref string query, bool consolidado, bool ccTodos)
        {
            switch (consolidado, ccTodos)
            {
                case (true, false):
                    query += " and M.cod_centro_costo = @codCentroCosto";
                    break;

                case (false, true):
                    query += " and M.cod_unidad = @codUnidad";
                    break;

                case (false, false):
                    query += " and M.cod_unidad = @codUnidad and M.cod_centro_costo = @codCentroCosto";
                    break;
            }
        }

        private static void AgregarFiltroPeriodo(ref string query, int rbOpcion)
        {
            if (rbOpcion == 0) // Últimos 24 meses
            {
                query += " and P.PERIODO_CORTE between @desde and @hasta";
            }
            else if (rbOpcion == 1) // Histórico
            {
                query += " and M.mes = @periodoMes";
            }
        }
    }
}
