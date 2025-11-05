using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ProGrX_Procesos;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_PrincipalDB
    {
        private readonly IConfiguration _config;

        public frmAH_PrincipalDB(IConfiguration config)
        {
            _config = config;
        }



        public ResumenPatrimonioDTO obtener_ResumenPatrimonio(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ResumenPatrimonioDTO info = new ResumenPatrimonioDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT * FROM vPAT_Consolidado WHERE cedula = '{cedula}'";

                    info = connection.Query<ResumenPatrimonioDTO>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<PatrimonioPrincipalDTO> obtener_Patrimonio(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<PatrimonioPrincipalDTO> info = new List<PatrimonioPrincipalDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT 
                                            A.*, 
                                            ISNULL(D.descripcion, '') AS TipoDoc, 
                                            ISNULL(C.descripcion, '') AS Concepto,
                                            CASE 
                                                WHEN ISNULL(ESTADO, '') = 'O' THEN 'Obrero'
		                                        WHEN ISNULL(ESTADO, '') = 'A' THEN 'Ahorro'
		                                        WHEN ISNULL(ESTADO, '') = 'P' THEN 'Patronal'
                                            END AS TIPO
                                        FROM 
                                            ahorro_detallado A
                                            LEFT JOIN SIF_Documentos D ON A.Tcon = D.Tipo_Documento 
                                            LEFT JOIN SIF_Conceptos C ON A.cod_Concepto = C.cod_Concepto  
                                        WHERE A.cedula = '{cedula}' ";

                    info = connection.Query<PatrimonioPrincipalDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<HistoricoPatrimonioDTO> obtener_HistoricoPatrimonio(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<HistoricoPatrimonioDTO> info = new List<HistoricoPatrimonioDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT A.*, 
                                           ISNULL(E.Descripcion, A.EstadoActual) AS Estado_Desc
                                    FROM ase_per_aportes A
                                    LEFT JOIN AFI_ESTADOS_PERSONA E 
                                        ON A.EstadoActual = E.cod_Estado
                                     WHERE cedula = '{cedula}' ORDER BY A.anio DESC, A.mes DESC;";

                    info = connection.Query<HistoricoPatrimonioDTO>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<ExcedentePatrimonioDTO> obtener_ExcedentePatrimonio(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePatrimonioDTO> info = new List<ExcedentePatrimonioDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT P.Inicio, 
                                           P.CORTE, 
                                           E.*
                                    FROM exc_cierre E
                                    INNER JOIN EXC_PERIODOS P ON E.ID_PERIODO = P.ID_PERIODO
                                     WHERE cedula = '{cedula}'";

                    info = connection.Query<ExcedentePatrimonioDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<LiquidacionPatrimonioDTO> LiqudacionesPatrimonio_Obtener(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<LiquidacionPatrimonioDTO> info = new List<LiquidacionPatrimonioDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT Consec, FecLiq, Aporte_Liq, Ahorro_Liq, Extra_Liq, Capitalizado_Liq, Usuario
                            FROM liquidacion
                            WHERE estado = 'P' AND cedula = '{cedula}'
                                  ORDER BY FecLiq DESC ";

                    info = connection.Query<LiquidacionPatrimonioDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


    }
}