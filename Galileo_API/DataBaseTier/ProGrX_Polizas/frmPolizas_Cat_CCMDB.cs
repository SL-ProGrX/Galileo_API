using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasCatCcmDB
    {
        private readonly PortalDB _portalDb;

        public FrmPolizasCatCcmDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasCatalogo_Listar(int codEmpresa)
        {
            var query = @"
                SELECT Cp.COD_POLIZA as item,
                       Cp.DESCRIPCION as descripcion
                FROM CRD_CATALOGO_POLIZAS Cp
                ORDER BY Cp.DESCRIPCION";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<PolizasCoberturasMotivosCausasDto>> PolizasConceptosConfigListas(int codEmpresa, string codPoliza, string tipo)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.Query<PolizasCoberturasMotivosCausasDto>(
                    "spPolizas_Conceptos_Config_Listas",
                    new { Poliza = codPoliza, Tipo = tipo },
                    commandType: System.Data.CommandType.StoredProcedure
                ).AsList();
                return result;
            });
        }

        public ErrorDto<PolizasConceptosConfigAddResult> PolizasConceptosConfigAdd(int codEmpresa, PolizasConceptosConfigAddParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizasConceptosConfigAddResult>(
                    "spPolizas_Conceptos_Config_Add",
                    new
                    {
                        Id = param.Id,
                        Poliza = param.Cod_Poliza,
                        Codigo = param.Codigo,
                        Descripcion = param.Descripcion,
                        Activo = param.Activo,
                        Usuario = param.Usuario,
                        Tipo = param.Tipo
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }

        public ErrorDto<PolizasConceptosConfigAddResult> PolizasConceptosConfigDel(int codEmpresa, PolizasConceptosConfigDelParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizasConceptosConfigAddResult>(
                    "spPolizas_Conceptos_Config_Del",
                    new { Id = param.Id, Usuario = param.Usuario },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }
    }
}
