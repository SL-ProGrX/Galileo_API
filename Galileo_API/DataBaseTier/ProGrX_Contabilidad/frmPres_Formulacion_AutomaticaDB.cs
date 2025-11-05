using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;
using PgxAPI.Models.ProGrX_Contabilidad;

namespace PgxAPI.DataBaseTier.ProGrX_Contabilidad
{
    public class frmPres_Formulacion_AutomaticaDB
    {
        private readonly IConfiguration _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmPres_Formulacion_AutomaticaDB(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Obtiene los modelos de presupuesto según la contabilidad y el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContab"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<presModelisLista>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<List<presModelisLista>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<presModelisLista>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select P.cod_modelo as 'IdX' , P.DESCRIPCION as 'ItmX', P.ESTADO ,Cc.Inicio_Anio
                    From PRES_MODELOS P INNER JOIN PRES_MODELOS_USUARIOS Pmu on P.cod_Contabilidad = Pmu.cod_contabilidad
                     and P.cod_Modelo = Pmu.cod_Modelo and Pmu.Usuario = @usuario
                    INNER JOIN CNTX_CIERRES Cc on P.cod_Contabilidad = Cc.cod_Contabilidad and P.ID_CIERRE = Cc.ID_CIERRE 
                    Where P.COD_CONTABILIDAD = @contabilidad
                    group by P.cod_Modelo, P.Descripcion,P.ESTADO ,Cc.Inicio_Anio 
                    order by Cc.INICIO_ANIO desc, P.Cod_Modelo";
                    resp.Result = connection.Query<presModelisLista>(query,
                        new
                        {
                            contabilidad = CodContab,
                            usuario = Usuario
                        }
                        ).ToList();
                }
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
        /// <param name="CodEmpresa"></param>
        /// <param name="CodModelo"></param>
        /// <param name="vTipo"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<PresFormulacionAutoDTO>> Pres_Formulacion_Automatica(
            int CodEmpresa, string CodModelo, string vTipo ,string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<List<PresFormulacionAutoDTO>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresFormulacionAutoDTO>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Inicializa Tabla de Resultados
                    var procedure = "[spPres_Formula_Inicia]";
                    var values = new
                    {
                        Modelo = CodModelo,
                        Usuario = Usuario
                    };
                    connection.Execute(procedure, values, commandType: System.Data.CommandType.StoredProcedure);
                    
                    switch (vTipo)
                    {
                        case "CA": //Cartera
                            procedure = "[spPres_Formula_Auxiliar_Credito]";
                            values = new
                            {
                                Modelo = CodModelo,
                                Usuario = Usuario
                            };

                            break;
                        case "AG": //Ahorros y Gasto Financiero
                            procedure = "[spPres_Formula_Auxiliar_Ahorros]";
                            values = new
                            {
                                Modelo = CodModelo,
                                Usuario = Usuario
                            };
                            break;
                        case "DA": //Depreciaciones de Activos 
                            procedure = "[spPres_Formula_Auxiliar_Activos]";
                            values = new
                            {
                                Modelo = CodModelo,
                                Usuario = Usuario
                            };
                            break;
                        case "PE": //Estandar de Partidas Contables
                            procedure = "[spPres_Formula_Auxiliar_Activos]";
                            values = new
                            {
                                Modelo = CodModelo,
                                Usuario = Usuario
                            };
                            break;

                    }

                    var auxiliar = connection.Execute(procedure, values, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: 600);

                    //Muestra el Resultado de la Formulacion
                    procedure = "[spPres_Formula_Resultado]";
                    values = new
                    {
                        Modelo = CodModelo,
                        Usuario = Usuario
                    };
                    resp.Result = connection.Query<PresFormulacionAutoDTO>(procedure, values, commandType: System.Data.CommandType.StoredProcedure).ToList();
                    if (resp.Result == null)
                    {
                        resp.Code = -1;
                        resp.Description = "Error al aplicar la formulación";
                        resp.Result = null;
                        return resp;
                    }

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
