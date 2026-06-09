using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Credito;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.BusinessLogic.ProGrX.Credito
{
    public class FrmCRConsultaCreditosBL
    {
        private readonly FrmCRConsultaCreditosDB _Db;

        public FrmCRConsultaCreditosBL(IConfiguration config)
        {
            _Db = new FrmCRConsultaCreditosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ConsultaCrdGarantiaTipo_Obtener(int CodEmpresa)
        {
            return _Db.CR_ConsultaCrdGarantiaTipo_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CrConsultaCrdSociosData>> CR_ConsultaCrdSocios_Obtener(int CodEmpresa)
        {
            return _Db.CR_ConsultaCrdSocios_Obtener(CodEmpresa);
        }

        public ErrorDto<CrConsultaCrdData> CR_ConsultaCrdConsulta_Integrada_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            return _Db.CR_ConsultaCrdConsulta_Integrada_Obtener(CodEmpresa, cedula, usuario);
        }

        public ErrorDto CR_Socios_RegistrarNota(int CodEmpresa, string cedula, string nota, string usuario)
        {
            return _Db.CR_Socios_RegistrarNota(CodEmpresa, cedula, nota, usuario);
        }

        public ErrorDto<decimal> fxCajas_SaldoaFavor(int CodEmpresa, string cedula)
        {
            return _Db.fxCajas_SaldoaFavor(CodEmpresa, cedula);
        }

        #region Créditos

        public ErrorDto<List<CrConsultaCrdCreditosData>> CR_ConsultaCrd_Creditos_Obtener(int CodEmpresa, string cedula, string sheetName)
        {
            return _Db.CR_ConsultaCrd_Creditos_Obtener(CodEmpresa, cedula, sheetName);
        }

        public ErrorDto<List<CrConsultaCreditosData>> CR_ConsultaCrd_Tramite_Obtener(int CodEmpresa, string cedula)
        {
            return _Db.CR_ConsultaCrd_Tramite_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<List<CrConsultaCrdPreanalisisData>> CR_ConsultaCrd_PreAnalisis_Obtener(int CodEmpresa, string cedula)
        {
            return _Db.CR_ConsultaCrd_PreAnalisis_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<List<CrConsultaCrdIncobrableData>> CR_ConsultaCrd_Incobrable_Obtener(int CodEmpresa, string cedula)
        {
            return _Db.CR_ConsultaCrd_Incobrable_Obtener(CodEmpresa, cedula);
        }

        #endregion

        #region Cobros

        public ErrorDto<List<CrConsultaCobroDto>> CR_ConsultaCobros_Obtener(int CodEmpresa, string cedula)
        {
            return _Db.CR_ConsultaCobros_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<List<CrConsultaAsignacionCobroData>> CR_ConsultaAsignacion_Obtener(int CodEmpresa, string cedula)
        {
            return _Db.CR_ConsultaAsignacion_Obtener(CodEmpresa, cedula);
        }


        #endregion

        #region Ahorros

        public ErrorDto<List<CrConsultaContratosData>> CR_ContratosConsulta_Obtener(int codEmpresa, string cedula, string usuario)
        {
            return _Db.CR_ContratosConsulta_Obtener(codEmpresa, cedula, usuario);
        }

        public ErrorDto<List<CrContratosMovimientosData>> CR_Contratos_Movimientos_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return _Db.CR_Contratos_Movimientos_Obtener(codEmpresa, codOperadora, codPlan, codContrato);
        }

        public ErrorDto<List<CrContratosCuponesData>> CR_Contratos_Cupones_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return _Db.CR_Contratos_Cupones_Obtener(codEmpresa, codOperadora, codPlan, codContrato);
        }

        public ErrorDto<List<CrContratosBitacoraData>> CR_Contratos_Bitacora_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return _Db.CR_Contratos_Bitacora_Obtener(codEmpresa, codOperadora, codPlan, codContrato);
        }

        public ErrorDto<List<CrContratosCierresData>> CR_Contratos_Cierres_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return _Db.CR_Contratos_Cierres_Obtener(codEmpresa, codOperadora, codPlan, codContrato);
        }


        public ErrorDto<CajasSesionDto> Cajas_Sesion_ObtenerActiva(int codEmpresa, string usuario, string identificacion)
        {
            return _Db.Cajas_Sesion_ObtenerActiva(codEmpresa, usuario, identificacion);
        }
        #endregion

        #region Patrimonio
        public ErrorDto<List<CrPatrimonioData>> CR_Patrimonio_Obtener(int CodEmpresa, string cedula, string tipo)
        {
            return _Db.CR_Patrimonio_Obtener(CodEmpresa, cedula, tipo);
        }

        public ErrorDto<List<ExcPeriodosVisiblesData>> EXC_Periodos_Visibles_Obtener(int CodEmpresa, string cedula)
        {
            return _Db.EXC_Periodos_Visibles_Obtener(CodEmpresa, cedula);
        }
        #endregion

        #region Beneficios
        public ErrorDto<List<AfiBeneficiosConsultaData>> AFI_Beneficios_Consulta(int CodEmpresa, string cedula)
        {
            return _Db.AFI_Beneficios_Consulta(CodEmpresa, cedula);
        }

        #endregion

        #region Renuncias

        public ErrorDto<List<AfiRenunciaTransitoData>> AFI_ConsultaRenunciaTransito(int CodEmpresa, string cedula)
        {
            return _Db.AFI_ConsultaRenunciaTransito(CodEmpresa, cedula);
        }

        public ErrorDto<List<AfiRenunciasConsultaData>> AFI_Renuncias_Consulta(int CodEmpresa, string cedula)
        {
            return _Db.AFI_Renuncias_Consulta(CodEmpresa, cedula);
        }
        #endregion

        #region Mensajes

        public ErrorDto<List<AfiSociosMensajesData>> AFI_Socios_Mensajes_Obtener(int CodEmpresa, string cedula, string tipo)
        {
            return _Db.AFI_Socios_Mensajes_Obtener(CodEmpresa, cedula, tipo);
        }

        public ErrorDto AFI_Socios_Mensajes_Guardar(int codEmpresa, AfiSociosMensajesData data)
        {
            return _Db.AFI_Socios_Mensajes_Guardar(codEmpresa, data);
        }

        public ErrorDto AFI_Socios_Mensajes_Elimina(int codEmpresa, string jData)
        {
            AfiSociosMensajesData data = JsonConvert.DeserializeObject<AfiSociosMensajesData>(jData) ?? new AfiSociosMensajesData();
            return _Db.AFI_Socios_Mensajes_Elimina(codEmpresa, data);
        }

        public ErrorDto AFI_Socios_Mensajes_Resolucion(int codEmpresa, string usuario, AfiSociosMensajesData data)
        {
            return _Db.AFI_Socios_Mensajes_Resolucion(codEmpresa, usuario, data);
        }

        #endregion

        #region Correo

        public ErrorDto<List<SysMailLoadData>> Sys_Mail_Load(int CodEmpresa, string cedula)
        {
            return _Db.Sys_Mail_Load(CodEmpresa, cedula);
        }



        #endregion

        #region Info

        public ErrorDto<CRConsultaInfoDto> AF_Persona_Consulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            return _Db.AF_Persona_Consulta_Obtener(CodEmpresa, cedula, usuario);
        }

        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string request)
        {
            return _Db.AF_Persona_Canales_Registra(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string request)
        {
            return _Db.AF_Persona_Bienes_Registra(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            return _Db.AF_Persona_Escolaridad_Registra(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Preferencia_Registra(int CodEmpresa, string request)
        {
            return _Db.AF_Persona_Preferencia_Registra(CodEmpresa, request);
        }

        #endregion

        #region Estado

        public ErrorDto<EmpresaEnlaceResultDto> ConsultaVersionEmpresa(int codEmpresa)
        {
            return _Db.ConsultaVersionEmpresa(codEmpresa);
        }

        #endregion

        #region @

        public ErrorDto<SocioCierresData> Email_SocioPeriodos_Obtener(int CodEmpresa, string cedula)
        {
            return _Db.Email_SocioPeriodos_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto Email_SocioEstadoCuenta_Enviar(int CodEmpresa, string usuario, string cedula, string email, string periodo, string tipo)
        {
            return _Db.Email_SocioEstadoCuenta_Enviar(CodEmpresa, usuario, cedula, email, periodo, tipo);
        }

        #endregion

        #region Aut/C.I

        public ErrorDto CR_RegistraConsentimiento(int CodEmpresa, string cedula, string usuario)
        {
            return _Db.CR_RegistraConsentimiento(CodEmpresa, cedula, usuario);
        }

        #endregion

    }
}