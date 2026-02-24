using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXDivisasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb DBBitacora;
        private readonly int vModulo = 20;

        public FrmCntXDivisasDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCntXDivisasDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            DBBitacora = dbBitacora;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Unidades_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion 
                from CntX_Unidades where cod_contabilidad = @codConta";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_CentroCostos_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select COD_CENTRO_COSTO AS item, RTRIM(DESCRIPCION) AS descripcion 
                From CNTX_CENTRO_COSTOS 
                Where Activo = 1 And COD_CONTABILIDAD = @codConta";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Lista_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select cod_divisa as item,descripcion from CntX_Divisas 
                Where COD_CONTABILIDAD = @codConta";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        public ErrorDto<CntXDivisaData> CntXDivisas_Obtener(int codEmpresa, int codConta, string codDivisa)
        {
            const string sql = @"select M.* 
                , isnull(I.Cod_Cuenta_Mask,'') as 'CtaIng', isnull(G.Cod_Cuenta_Mask,'') as 'CtaGst'
                , isnull(I.descripcion,'') as 'CtaIng_Desc', isnull(G.Descripcion,'') as 'CtaGst_Desc'
                , isnull(U.Descripcion,'') as 'Unidad_Desc', isnull(Cc.Descripcion,'') as 'Centro_Desc'
            from CntX_Divisas M
            left join CntX_Cuentas I
                on M.COD_CONTABILIDAD = I.COD_CONTABILIDAD 
               and M.cod_cuenta = I.cod_cuenta
            left join CntX_Cuentas G
                on M.COD_CONTABILIDAD = G.COD_CONTABILIDAD 
               and M.cod_cuenta_Gasto = G.cod_cuenta
            left join CntX_Unidades U
                on M.COD_CONTABILIDAD = U.COD_CONTABILIDAD 
               and M.cod_Unidad = U.cod_Unidad
            left join CntX_Centro_Costos Cc
                on M.COD_CONTABILIDAD = Cc.COD_CONTABILIDAD 
               and M.cod_Centro_Costo = Cc.Cod_Centro_Costo
            where M.cod_divisa = @codDivisa
              and M.COD_CONTABILIDAD = @codConta;";

            var result = DbHelper.ExecuteSingleQuery<CntXDivisaData>(
                _portalDb,
                codEmpresa,
                sql,
                new CntXDivisaData(),
                new { codDivisa, codConta }
            );

            if (result.Result == null)
            {
                result.Result = new CntXDivisaData();
            }

            return result;
        }

        public ErrorDto<CntXDivisaData> CntXDivisas_Scroll_Obtener(int CodEmpresa, int codConta, int scrollCode, string codDivisa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            try
            {
                const string query = @"
                    select Top 1 cod_divisa from Cntx_Divisas
                    WHERE cod_contabilidad = @codConta AND 
                          ((@scroll = 1 AND cod_divisa > @codDivisa)
                           OR (@scroll <> 1 AND cod_divisa < @codDivisa))
                    ORDER BY
                        CASE WHEN @scroll = 1 THEN cod_divisa END ASC,
                        CASE WHEN @scroll <> 1 THEN cod_divisa END DESC;";

                var divisa = conn.Query<string>(query, new { scroll = scrollCode, codConta, codDivisa }).FirstOrDefault();
                var divisaObjetivo = !string.IsNullOrEmpty(divisa) ? divisa : codDivisa;

                return CntXDivisas_Obtener(CodEmpresa, codConta, divisaObjetivo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CntXDivisaData>(ex.Message);
            }
        }
    }
}
