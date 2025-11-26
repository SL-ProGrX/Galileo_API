using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCcConsultaExcedenteDb
    {
        private readonly IConfiguration _config;

        public FrmCcConsultaExcedenteDb(IConfiguration config)
        {
            _config = config;
        }

        public List<CCPeriodoList> CC_Periodos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCPeriodoList> resp = new List<CCPeriodoList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Idx, ItmX From vExc_Periodos where ESTADO = 'C' order by IdX desc";
                    resp = connection.Query<CCPeriodoList>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public CCExcPeriodoData CC_Exc_Periodos_Obtener(int CodEmpresa, int Id_Periodo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CCExcPeriodoData resp = new CCExcPeriodoData();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select NC_MORA, NC_OPCF, NC_SALDOS 
                                  from Exc_Periodos 
                                  where id_periodo = @Id_Periodo";

                    resp = connection
                        .Query<CCExcPeriodoData>(query, new { Id_Periodo })
                        .FirstOrDefault() ?? new CCExcPeriodoData();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_ValidaCedula_Obtener(int CodEmpresa, string Cedula, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Valida Acceso a Expediente
                    var procedure = "[spSYS_RA_Consulta_Status]";
                    var values = new
                    {
                        Cedula,
                        Usuario
                    };

                    var consultaResult = connection
                        .Query<ConsultaStatusResultDto>(procedure, values, commandType: CommandType.StoredProcedure)
                        .FirstOrDefault();

                    if (consultaResult != null)
                    {
                        var ra_consulta = consultaResult;
                        if (ra_consulta.PERSONA_ID > 0 && ra_consulta.AUTORIZACION_ID == 0)
                        {
                            resp.Code = 0;
                            resp.Description = "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!";
                            return resp;
                        }
                    }
                    else
                    {
                        resp.Code = 0;
                        resp.Description = "No se encontró información de la persona.";
                        return resp;
                    }

                    // Valida que exista la cédula dentro de la tabla socios
                    var query = @"select nombre 
                                  from socios 
                                  where cedula = @Cedula";

                    resp.Description = connection.QuerySingleOrDefault<string>(query, new { Cedula });

                    if (string.IsNullOrEmpty(resp.Description))
                    {
                        resp.Code = 0;
                        resp.Description = "No se encontró registro de la persona...";
                    }
                    else
                    {
                        resp.Code = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public CCConsultaExcedenteData CC_ConsultaExcedente_Obtener(int CodEmpresa, int Id_Periodo, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CCConsultaExcedenteData resp = new CCConsultaExcedenteData();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                        select  E.*,
                                isnull(S.DESCRIPCION, 'No Identificada') as SalidaDesc
                        from    exc_cierre E
                        left join EXC_TIPOS_SALIDAS S 
                               on E.SALIDA_CODIGO = S.COD_SALIDA
                        where   E.id_periodo = @Id_Periodo
                        and     E.cedula = @Cedula";

                    resp = connection
                        .Query<CCConsultaExcedenteData>(query, new { Id_Periodo, Cedula })
                        .FirstOrDefault() ?? new CCConsultaExcedenteData();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<VSifAuxCreditosMovDetalle> CC_NotasMora_Obtener(int CodEmpresa, int NC_Mora, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<VSifAuxCreditosMovDetalle> resp = new List<VSifAuxCreditosMovDetalle>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                        select M.* 
                        from vSIFAuxCreditosMovDetalle M 
                        where M.tcon in ('7','NC') 
                          and M.ncon = @NC_Mora 
                          and M.cedula = @Cedula";

                    resp = connection
                        .Query<VSifAuxCreditosMovDetalle>(query, new { NC_Mora, Cedula })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<VSifAuxCreditosMovDetalle> CC_NotasOPCF_Obtener(int CodEmpresa, int NC_OPCF, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<VSifAuxCreditosMovDetalle> resp = new List<VSifAuxCreditosMovDetalle>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                        select * 
                        from vSIFAuxCreditosMovDetalle 
                        where tcon in ('7','NC')  
                          and ncon = @NC_OPCF 
                          and id_solicitud in (
                                select id_solicitud 
                                from reg_creditos 
                                where referencia in (
                                    select id_solicitud 
                                    from reg_creditos 
                                    where cedula = @Cedula 
                                      and garantia = 'F'
                                )
                          )";

                    resp = connection
                        .Query<VSifAuxCreditosMovDetalle>(query, new { NC_OPCF, Cedula })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<VSifAuxCreditosMovDetalle> CC_NotasSaldos_Obtener(int CodEmpresa, int NC_Saldos, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<VSifAuxCreditosMovDetalle> resp = new List<VSifAuxCreditosMovDetalle>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                        select C.*
                        from vSIFAuxCreditosMovDetalle C
                        where C.tcon in ('7','NC')  
                          and C.ncon   = @NC_Saldos 
                          and C.cedula = @Cedula";

                    resp = connection
                        .Query<VSifAuxCreditosMovDetalle>(query, new { NC_Saldos, Cedula })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }
    }
}
