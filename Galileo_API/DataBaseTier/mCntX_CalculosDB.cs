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

        public bool FxCntX_PeriodoVerifica(int CodEmpresa, int codConta, int pAnion, int pMes)
        {
            string sql = @"select isnull(count(*),0) as existe from CntX_Periodos where anio = @Anio
                and mes = @Mes and cod_contabilidad = @CodConta
                and estado = 'P'";
            var parametros = new { Anio = pAnion, Mes = pMes, CodConta = codConta };
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, sql, 0, parametros).Result;
            return existe > 0;
        }
    }
}
