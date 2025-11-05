using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Credito;

namespace PgxAPI.DataBaseTier.ProGrX.Credito
{
    public class frmCR_SeguimientoFirmasDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 3; // Modulo de Créditos

        public frmCR_SeguimientoFirmasDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Método para obtener el seguimiento de firmas de una operación de crédito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CRSeguimientoFirmasData>> CR_SeguimientoFirmas_Obtener(int CodEmpresa, int operacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CRSeguimientoFirmasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CRSeguimientoFirmasData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select 'Deudor' as Tipo,R.cedula,S.nombre,isnull(R.firma_deudor,0) as Firma, R.ID_SOLICITUD as operacion
                                        from reg_creditos R inner join Socios S on R.cedula = S.cedula
                                        Where R.ID_SOLICITUD = @operacion 
                                        Union 
                                        select 'Fiador' as Tipo,S.cedula,S.nombre,case when F.firma = 'N' then 0 else 1 end as Firma
                                        from fiadores F inner join Socios S on F.cedulaf = S.cedula
                                        where F.estado = 'A' and F.id_solicitud = @operacion ";
                    response.Result = connection.Query<CRSeguimientoFirmasData>(query, new
                        {
                        operacion = operacion
                    }).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Método para guardar la firma de un deudor o fiador
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="firmasData"></param>
        /// <returns></returns>
        public ErrorDto CR_SeguimientoFirmas_Guardar(int CodEmpresa, CRSeguimientoFirmasData firmasData)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    if (firmasData.tipo == "Deudor")
                    {
                        query = $@"update reg_creditos set firma_deudor = @Firma ,fechaforf = dbo.MyGetdate()
                                              where id_solicitud = @operacion";
                    }
                    else
                    {
                        int valor = firmasData.firma ? 1 : 0;
                        query = $@"UPDATE fiadores 
                                        SET firma = @FirmaLetra
                                        WHERE ID_SOLICITUD = @operacion 
                                          AND CedulaF = @Cedula";
                    }
                    
                    connection.Execute(query, new
                    {
                        Firma = firmasData.firma,
                        operacion = firmasData.operacion,
                        Cedula = firmasData.cedula,
                        FirmaLetra = firmasData.firma ? "S" : "N"
                    });


                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    }
}
