using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_ConsultaExcedenteDB
    {
        private readonly IConfiguration _config;
        MProGrxMain mProGrx_Main;

        public frmCC_ConsultaExcedenteDB(IConfiguration config)
        {
            _config = config;
            mProGrx_Main = new MProGrxMain(_config);
        }

        //public ErrorDto ValidaAccesoExpediente(string Cedula, string Usuario)
        //{
        //    List<ConsultaStatusResultDTO> RA_Consulta = new List<ConsultaStatusResultDTO>();
        //    ErrorDto resp = new ErrorDto();
        //    RA_Consulta = mProGrx_Main.DatosObtener(Cedula, Usuario);
        //    if (RA_Consulta[0].PERSONA_ID > 0 && RA_Consulta[0].AUTORIZACION_ID == 0)
        //    {
        //        resp.Code = 0;
        //        resp.Description = "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorizaci�n para Consultar!";
        //    }
        //    else
        //    {
        //        resp.Code = 1;
        //        resp.Description = "Acceso permitido!";
        //    }

        //    return resp;
        //}

        public List<CCPeriodoList> CC_Periodos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CCPeriodoList> resp = new List<CCPeriodoList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Idx, ItmX  From vExc_Periodos where ESTADO = 'C' order by IdX desc";
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
                    var query = $@"select NC_MORA, NC_OPCF, NC_SALDOS from Exc_Periodos where id_periodo = {Id_Periodo}";
                    resp = connection.Query<CCExcPeriodoData>(query).FirstOrDefault() ?? new CCExcPeriodoData();
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
            ConsultaStatusResultDto ra_consulta = new ConsultaStatusResultDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Valida Acceso a Expediente
                    var procedure = "[spSYS_RA_Consulta_Status]";
                    var values = new
                    {
                        Cedula = Cedula,
                        Usuario = Usuario
                    };

                    var consultaResult = connection.Query<ConsultaStatusResultDto>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if (consultaResult != null)
                    {
                        ra_consulta = consultaResult;
                        if (ra_consulta.PERSONA_ID > 0 && ra_consulta.AUTORIZACION_ID == 0)
                        {
                            resp.Code = 0;
                            resp.Description = "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorizaci�n para Consultar!";
                            return resp;
                        }
                    }
                    else
                    {
                        resp.Code = 0;
                        resp.Description = "No se encontró información de la persona.";
                        return resp;
                    }

                    //Valida que exista la cedula dentro de la tabla socios
                    var query = $@"select nombre from socios where cedula = '{Cedula}'";
                    resp.Description = connection.QuerySingleOrDefault<string>(query, new { Cedula });

                    if (string.IsNullOrEmpty(resp.Description))
                    {
                        resp.Code = 0;
                        resp.Description = "No se encontr� registro de la persona...";
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
                    var query = $@"select E.*,isnull(S.DESCRIPCION,'No Identificada') as 'SalidaDesc'
                        from exc_cierre E left join EXC_TIPOS_SALIDAS S on E.SALIDA_CODIGO = S.COD_SALIDA
                        where E.id_periodo = '{Id_Periodo}'
                        and E.cedula = '{Cedula}'";
                    resp = connection.Query<CCConsultaExcedenteData>(query).FirstOrDefault() ?? new CCConsultaExcedenteData();
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
                    var query = $@"select M.* 
                        from vSIFAuxCreditosMovDetalle M 
                        where M.tcon in('7','NC') and M.ncon = '{NC_Mora}' and M.cedula = '{Cedula}'";
                    resp = connection.Query<VSifAuxCreditosMovDetalle>(query).ToList();
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
                    var query = $@"select * from vSIFAuxCreditosMovDetalle where tcon in('7','NC')  and ncon = '{NC_OPCF}' 
                        and id_solicitud in((select id_solicitud from reg_creditos where referencia in(
                        select id_solicitud from reg_creditos where cedula = '{Cedula}' and garantia = 'F')))";
                    resp = connection.Query<VSifAuxCreditosMovDetalle>(query).ToList();
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
                    var query = $@"select C.*
                        from vSIFAuxCreditosMovDetalle C
                        where C.tcon in('7','NC')  and C.ncon = '{NC_Saldos}' and C.cedula = '{Cedula}'";
                    resp = connection.Query<VSifAuxCreditosMovDetalle>(query).ToList();
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