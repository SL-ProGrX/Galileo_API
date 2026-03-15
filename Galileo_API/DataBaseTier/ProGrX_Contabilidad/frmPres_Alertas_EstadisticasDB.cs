using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ProGrX_Contabilidad;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmPresAlertasEstadisticasDB
    {
        private readonly PortalDB _portalDb; 
        public FrmPresAlertasEstadisticasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Método para obtener una lista de tipos de alertas estadísticas.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PresAlertasEstadisticasTipos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"SELECT COD_DESVIACION as item, CONCAT(DESCRIPCION, ' | Tipo: ',TIPO, ' | Operador: ',OPERADOR,' | Valor: ',VALOR_DESVIACION ) as DESCRIPCION  FROM PRES_TIPOS_DESVIACIONES WHERE ACTIVA = 1";

                return conn.Query<DropDownListaGenericaModel>(
                    query
                ).ToList();
            });
        }


        /// <summary>
        /// Busca el presupuesto según filtros puestos por el usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<PresVistaPresupuestoAlertasData>> PresPlanning_Obtener(int CodCliente, string datos)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodCliente);
            PresVistaPresupuestoAlertasBuscar filtros = JsonConvert.DeserializeObject<PresVistaPresupuestoAlertasBuscar>(datos) ?? new PresVistaPresupuestoAlertasBuscar();

            var info = new ErrorDto<List<PresVistaPresupuestoAlertasData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresVistaPresupuestoAlertasData>()
            };
            try
            {
                var procedure = "[spPres_W_VistaPresupuestoAlertas]";
                var values = new
                {
                    COD_CONTA = filtros.cod_conta,
                    COD_MODELO = filtros.cod_modelo,
                    COD_UNIDAD = filtros.cod_unidad,
                    CENTRO_COSTO = filtros.centro_costo,
                    ANIO = filtros.anio,
                    MES = filtros.mes,
                    TIPO_VISTA = filtros.tipo_vista,
                    CtaMov = filtros.ctaMov ? (bool?)true : null,
                    Tipo_Alerta = string.IsNullOrEmpty(filtros.tipo_alerta) ? "T" : filtros.tipo_alerta,
                };

                info.Result = connection.Query<PresVistaPresupuestoAlertasData>(procedure, values, commandType: CommandType.StoredProcedure, commandTimeout: 600).ToList();

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<PresVistaPresupuestoAlertasData>();
            }

            return info;
        }



    }
}
