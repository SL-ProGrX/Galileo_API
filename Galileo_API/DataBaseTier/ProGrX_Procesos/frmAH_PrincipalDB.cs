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



        public ResumenPatrimonioDto obtener_ResumenPatrimonio(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ResumenPatrimonioDto info = new ResumenPatrimonioDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT * FROM vPAT_Consolidado WHERE cedula = '{cedula}'";

                    info = connection.Query<ResumenPatrimonioDto>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<PatrimonioPrincipalDto> obtener_Patrimonio(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<PatrimonioPrincipalDto> info = new List<PatrimonioPrincipalDto>();
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

                    info = connection.Query<PatrimonioPrincipalDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<HistoricoPatrimonioDto> obtener_HistoricoPatrimonio(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<HistoricoPatrimonioDto> info = new List<HistoricoPatrimonioDto>();
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

                    info = connection.Query<HistoricoPatrimonioDto>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<ExcedentePatrimonioDto> obtener_ExcedentePatrimonio(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePatrimonioDto> info = new List<ExcedentePatrimonioDto>();
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

                    info = connection.Query<ExcedentePatrimonioDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<LiquidacionPatrimonioDto> LiqudacionesPatrimonio_Obtener(int CodEmpresa, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<LiquidacionPatrimonioDto> info = new List<LiquidacionPatrimonioDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT Consec, FecLiq, Aporte_Liq, Ahorro_Liq, Extra_Liq, Capitalizado_Liq, Usuario
                            FROM liquidacion
                            WHERE estado = 'P' AND cedula = '{cedula}'
                                  ORDER BY FecLiq DESC ";

                    info = connection.Query<LiquidacionPatrimonioDto>(query).ToList();

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