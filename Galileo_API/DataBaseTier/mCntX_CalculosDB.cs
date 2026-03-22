using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier
{
    public class MCntXCalculosDb
    {
        private readonly PortalDB _portalDB;

        public MCntXCalculosDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public bool FxCntX_UnidadVerifica(int codEmpresa, int codConta, string pUnidad)
        {
            string sql = @"select isnull(count(*),0)
                   from CntX_Unidades
                   where cod_contabilidad = @CodConta
                     and cod_unidad = @Unidad";

            var parametros = new
            {
                CodConta = codConta,
                Unidad = pUnidad
            };

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, parametros).Result;

            return existe > 0;
        }

        public bool FxCntX_CentroCostoVerifica(int codEmpresa, int codConta, string pCentroCosto, string pUnidad = "")
        {
            if (string.IsNullOrWhiteSpace(pCentroCosto))
                return true;

            string sql = @"select isnull(count(*),0)
                   from CntX_Centro_Costos
                   where cod_contabilidad = @CodConta
                     and cod_centro_costo = @CentroCosto";

            if (!string.IsNullOrWhiteSpace(pUnidad))
            {
                sql += @" and cod_centro_costo in (
                    select cod_centro_costo
                    from cntx_unidades_cc
                    where cod_contabilidad = @CodConta
                      and cod_unidad = @Unidad
                 )";
            }

            var parametros = new
            {
                CodConta = codConta,
                CentroCosto = pCentroCosto,
                Unidad = pUnidad
            };

            int existe = DbHelper
                .ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, parametros)
                .Result;

            return existe > 0;
        }

        public bool FxCntX_DivisaVerifica(int codEmpresa, int codConta, string pDivisa)
        {
            string sql = @"select isnull(count(*),0)
                   from CntX_Divisas
                   where cod_contabilidad = @CodConta
                     and cod_divisa = @Divisa";

            var parametros = new
            {
                CodConta = codConta,
                Divisa = pDivisa
            };

            int existe = DbHelper
                .ExecuteSingleQuery<int>(_portalDB, codEmpresa, sql, 0, parametros)
                .Result;

            return existe > 0;
        }

        public bool FxCntX_PeriodoVerifica(int CodEmpresa, int codConta, int pAnion, int pMes)
        {
            string sql = @"select isnull(count(*),0) as existe from CntX_Periodos where anio = @Anio
                and mes = @Mes and cod_contabilidad = @CodConta
                and estado = 'P'";
            var parametros = new { Anio = pAnion, Mes = pMes, CodConta = codConta };
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, sql, 0, parametros).Result;
            return existe > 0;
        }

        public byte[]? FxCntX_AsientoConcurrencia(int CodEmpresa, int codConta, string numAsiento, string tipoAsiento)
        {
            string sql = @"select ts from Cntx_Asientos where cod_contabilidad = @codConta 
                and num_asiento = @numAsiento and Tipo_Asiento = @tipoAsiento";
            var parametros = new { codConta, numAsiento, tipoAsiento };
            return DbHelper.ExecuteSingleQuery<byte[]?>(_portalDB, CodEmpresa, sql, null, parametros).Result;
        }
    }
}
