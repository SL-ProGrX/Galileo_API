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

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentaHistorico_Unidades_Obtener(int codEmpresa, int codConta)
        {
            string query = @"select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion 
                from CntX_Unidades where cod_contabilidad = @codConta";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

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

        public ErrorDto<List<CntXCuentaHistoricoData>> CntXCuentaHistorico_Obtener(
    int codEmpresa, int codConta, string cuenta, string codUnidad, string codCentroCosto)
        {
            bool consolidado = codUnidad == "[CONSOLIDADO]";
            bool ccTodos = codCentroCosto == "TODOS";

            string qFecha = @"select dbo.fxSys_FechaAnioMesToDatetime(@anio, @mes) as Fecha";

            var fechaDto = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                qFecha,
                DateTime.Now,
                new
                {
                    anio = DateTime.Now.Year,
                    mes = DateTime.Now.Month
                });

            if (fechaDto != null && fechaDto.Code < 0)
            {
                var error = new ErrorDto<List<CntXCuentaHistoricoData>>
                {
                    Code = fechaDto.Code,
                    Description = fechaDto.Description,
                    Result = null
                };
                return error;
            }
                
            DateTime pFecha = fechaDto.Result;

            string fromSql =
                (consolidado && ccTodos) ? "vCntX_Mov_Cuentas_General" :
                (consolidado && !ccTodos) ? "vCntX_Mov_Cuentas_CentroCosto" :
                (!consolidado && ccTodos) ? "vCntX_Mov_Cuentas_Unidad" :
                "CntX_Mov_Cuentas_Detallado";

            string query;

            if (!consolidado && !ccTodos)
            {
                query = $@"
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
                query = $@"
            select 
                M.Anio as anio,
                M.Mes as mes,
                C.cod_Cuenta_Mask as cod_cuenta_mask,
                {(consolidado ? (ccTodos ? "''" : "M.cod_unidad") : "M.cod_unidad")} as cod_unidad,
                {(ccTodos ? "''" : "M.cod_Centro_Costo")} as cod_centro_costo,
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

            if (consolidado && !ccTodos)
                query += " and M.cod_centro_costo = @codCentroCosto";
            else if (!consolidado && ccTodos)
                query += " and M.cod_unidad = @codUnidad";
            else if (!consolidado && !ccTodos)
                query += " and M.cod_unidad = @codUnidad and M.cod_centro_costo = @codCentroCosto";

            DateTime desde = pFecha.AddMonths(-24);
            DateTime hasta = pFecha.Date.AddDays(1).AddTicks(-1);

            query += " and P.PERIODO_CORTE between @desde and @hasta";
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
                    hasta
                });
        }
        
    }
}
