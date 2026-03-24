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
        public string FxCntX_PeriodoDesc(int pAnio, int pMes)
        {
            return pMes switch
            {
                1 => $"ENERO DE {pAnio}",
                2 => $"FEBRERO DE {pAnio}",
                3 => $"MARZO DE {pAnio}",
                4 => $"ABRIL DE {pAnio}",
                5 => $"MAYO DE {pAnio}",
                6 => $"JUNIO DE {pAnio}",
                7 => $"JULIO DE {pAnio}",
                8 => $"AGOSTO DE {pAnio}",
                9 => $"SETIEMBRE DE {pAnio}",
                10 => $"OCTUBRE DE {pAnio}",
                11 => $"NOVIEMBRE DE {pAnio}",
                12 => $"DICIEMBRE DE {pAnio}",
                13 => $"CIERRE FISCAL {pAnio}",
                _ => string.Empty
            };
        }

        public bool FxCntX_MesFiscal(int CodEmpresa, int codConta, int pAnio, int pMes)
        {
            string sql = @"select isnull(count(*),0) as Existe
                           from CntX_Asientos
                           where Anio = @Anio
                             and Mes = @Mes
                             and cod_Contabilidad = @CodConta
                             and Tipo_Asiento = 'CF'
                             and fecha_aplicado is not null";

            var parametros = new
            {
                Anio = pAnio,
                Mes = pMes,
                CodConta = codConta
            };

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, sql, 0, parametros).Result;
            return existe > 0;
        }
    }
}
