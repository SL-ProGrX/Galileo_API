using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.GEN;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_ConsultaExcedenteDB
    {
        private readonly IConfiguration _config;
        mProGrx_Main mProGrx_Main;

        public frmCC_ConsultaExcedenteDB(IConfiguration config)
        {
            _config = config;
            mProGrx_Main = new mProGrx_Main(_config);
        }

        //public ErrorDTO ValidaAccesoExpediente(string Cedula, string Usuario)
        //{
        //    List<ConsultaStatusResultDTO> RA_Consulta = new List<ConsultaStatusResultDTO>();
        //    ErrorDTO resp = new ErrorDTO();
        //    RA_Consulta = mProGrx_Main.DatosObtener(Cedula, Usuario);
        //    if (RA_Consulta[0].PERSONA_ID > 0 && RA_Consulta[0].AUTORIZACION_ID == 0)
        //    {
        //        resp.Code = 0;
        //        resp.Description = "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!";
        //    }
        //    else
        //    {
        //        resp.Code = 1;
        //        resp.Description = "Acceso permitido!";
        //    }

        //    return resp;
        //}

        public List<CC_PeriodoList> CC_Periodos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CC_PeriodoList> resp = new List<CC_PeriodoList>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Idx, ItmX  From vExc_Periodos where ESTADO = 'C' order by IdX desc";
                    resp = connection.Query<CC_PeriodoList>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public CC_Exc_PeriodoData CC_Exc_Periodos_Obtener(int CodEmpresa, int Id_Periodo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CC_Exc_PeriodoData resp = new CC_Exc_PeriodoData();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select NC_MORA, NC_OPCF, NC_SALDOS from Exc_Periodos where id_periodo = {Id_Periodo}";
                    resp = connection.Query<CC_Exc_PeriodoData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDTO CC_ValidaCedula_Obtener(int CodEmpresa, string Cedula, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            ConsultaStatusResultDTO ra_consulta = new ConsultaStatusResultDTO();

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

                    ra_consulta = connection.Query<ConsultaStatusResultDTO>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    if (ra_consulta.PERSONA_ID > 0 && ra_consulta.AUTORIZACION_ID == 0)
                    {
                        resp.Code = 0;
                        resp.Description = "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!";
                        return resp;
                    }

                    //Valida que exista la cedula dentro de la tabla socios
                    var query = $@"select nombre from socios where cedula = '{Cedula}'";
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

        public CC_ConsultaExcedenteData CC_ConsultaExcedente_Obtener(int CodEmpresa, int Id_Periodo, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CC_ConsultaExcedenteData resp = new CC_ConsultaExcedenteData();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select E.*,isnull(S.DESCRIPCION,'No Identificada') as 'SalidaDesc'
                        from exc_cierre E left join EXC_TIPOS_SALIDAS S on E.SALIDA_CODIGO = S.COD_SALIDA
                        where E.id_periodo = '{Id_Periodo}'
                        and E.cedula = '{Cedula}'";
                    resp = connection.Query<CC_ConsultaExcedenteData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<vSIFAuxCreditosMovDetalle> CC_NotasMora_Obtener(int CodEmpresa, int NC_Mora, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<vSIFAuxCreditosMovDetalle> resp = new List<vSIFAuxCreditosMovDetalle>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select M.* 
                        from vSIFAuxCreditosMovDetalle M 
                        where M.tcon in('7','NC') and M.ncon = '{NC_Mora}' and M.cedula = '{Cedula}'";
                    resp = connection.Query<vSIFAuxCreditosMovDetalle>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<vSIFAuxCreditosMovDetalle> CC_NotasOPCF_Obtener(int CodEmpresa, int NC_OPCF, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<vSIFAuxCreditosMovDetalle> resp = new List<vSIFAuxCreditosMovDetalle>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from vSIFAuxCreditosMovDetalle where tcon in('7','NC')  and ncon = '{NC_OPCF}' 
                        and id_solicitud in((select id_solicitud from reg_creditos where referencia in(
                        select id_solicitud from reg_creditos where cedula = '{Cedula}' and garantia = 'F')))";
                    resp = connection.Query<vSIFAuxCreditosMovDetalle>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<vSIFAuxCreditosMovDetalle> CC_NotasSaldos_Obtener(int CodEmpresa, int NC_Saldos, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<vSIFAuxCreditosMovDetalle> resp = new List<vSIFAuxCreditosMovDetalle>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.*
                        from vSIFAuxCreditosMovDetalle C
                        where C.tcon in('7','NC')  and C.ncon = '{NC_Saldos}' and C.cedula = '{Cedula}'";
                    resp = connection.Query<vSIFAuxCreditosMovDetalle>(query).ToList();
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