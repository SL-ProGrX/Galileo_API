using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using Galileo.Models.ProGrX_Contabilidad;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmPresFormulacionAutomaticaDb
    {
        private readonly IConfiguration _config;

        public FrmPresFormulacionAutomaticaDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codEmpresa)
        {
            var connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(connString);
        }

        #endregion

        /// <summary>
        /// Obtiene los modelos de presupuesto según la contabilidad y el usuario
        /// </summary>
        public ErrorDto<List<PresModelisLista>> Pres_Modelos_Obtener(int codEmpresa, int codContab, string usuario)
        {
            var resp = new ErrorDto<List<PresModelisLista>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresModelisLista>()
            };

            const string sql = @"
                SELECT 
                    P.cod_modelo AS IdX,
                    P.DESCRIPCION AS ItmX,
                    P.ESTADO,
                    Cc.Inicio_Anio
                FROM PRES_MODELOS P 
                INNER JOIN PRES_MODELOS_USUARIOS Pmu 
                    ON P.cod_Contabilidad = Pmu.cod_contabilidad
                    AND P.cod_Modelo      = Pmu.cod_Modelo
                    AND Pmu.Usuario       = @Usuario
                INNER JOIN CNTX_CIERRES Cc 
                    ON P.cod_Contabilidad = Cc.cod_Contabilidad
                    AND P.ID_CIERRE       = Cc.ID_CIERRE 
                WHERE P.COD_CONTABILIDAD = @Contabilidad
                GROUP BY P.cod_Modelo, P.Descripcion, P.ESTADO, Cc.Inicio_Anio 
                ORDER BY Cc.INICIO_ANIO DESC, P.Cod_Modelo;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                resp.Result = connection
                    .Query<PresModelisLista>(
                        sql,
                        new
                        {
                            Contabilidad = codContab,
                            Usuario = usuario
                        })
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Modelos_Obtener - " + ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene datos de formulación automática
        /// </summary>
        public ErrorDto<List<PresFormulacionAutoDto>> Pres_Formulacion_Automatica(
            int codEmpresa,
            string codModelo,
            string vTipo,
            string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var resp = new ErrorDto<List<PresFormulacionAutoDto>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresFormulacionAutoDto>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                //Inicializa Tabla de Resultados
                var procedure = "[spPres_W_Formulacion_Automatica]";
                var values = new
                {
                    Modelo = codModelo,
                    Usuario = usuario,
                    vTipo = vTipo
                };
                resp.Result = connection.Query<PresFormulacionAutoDto>(procedure, values,
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: 0).ToList();
                if (resp.Result == null)
                {
                    resp.Code = -1;
                    resp.Description = "Error al aplicar la formulación";
                    resp.Result = null;
                    return resp;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "Pres_Formulacion_Automatica - " + ex.Message;
                resp.Result = null;
            }
            return resp;
        }
    }
}