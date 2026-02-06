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

        /// <summary>
        /// Lista el catálogo de pólizas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista genérica de pólizas (item, descripcion).</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasCatalogo_Listar(int codEmpresa)
        {
            var query = @"
                SELECT Cp.COD_POLIZA as item,
                       Cp.DESCRIPCION as descripcion
                FROM CRD_CATALOGO_POLIZAS Cp
                ORDER BY Cp.DESCRIPCION";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de coberturas, motivos o causas configuradas para una póliza y tipo.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codPoliza">Código de la póliza.</param>
        /// <param name="tipo">Tipo de configuración (Cobertura, Motivo, Causa, etc.).</param>
        /// <returns>Lista de configuraciones asociadas.</returns>
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

        /// <summary>
        /// Agrega o actualiza una cobertura, motivo o causa para una póliza.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">
        /// <returns>Resultado de la operación (Pass, Mensaje, Movimiento, IdLLave).</returns>
        public ErrorDto<PolizasConceptosConfigAddResult> PolizasConceptosConfigAdd(int codEmpresa, PolizasConceptosConfigAddParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizasConceptosConfigAddResult>(
                    "spPolizas_Conceptos_Config_Add",
                    new
                    {
                        param.Id,
                        Poliza = param.Cod_Poliza,
                        param.Codigo,
                        param.Descripcion,
                        param.Activo,
                        param.Usuario,
                        param.Tipo
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }

        /// <summary>
        /// Elimina una cobertura, motivo o causa de una póliza.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">
        /// <returns>Resultado de la operación (Pass, Mensaje, Movimiento, IdLLave).</returns>
        public ErrorDto<PolizasConceptosConfigAddResult> PolizasConceptosConfigDel(int codEmpresa, PolizasConceptosConfigDelParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizasConceptosConfigAddResult>(
                    "spPolizas_Conceptos_Config_Del",
                    new { param.Id, param.Usuario },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }
    }
}
