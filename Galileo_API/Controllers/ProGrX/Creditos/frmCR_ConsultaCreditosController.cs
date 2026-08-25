using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Credito;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRConsultaCreditosController : ControllerBase
    {
        private readonly FrmCRConsultaCreditosBL _BL;

        public FrmCRConsultaCreditosController(IConfiguration config)
        {
            _BL = new FrmCRConsultaCreditosBL(config);
        }


        [Authorize]
        [HttpGet("CR_ConsultaCrdGarantiaTipo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ConsultaCrdGarantiaTipo_Obtener(int CodEmpresa)
        {
            return _BL.CR_ConsultaCrdGarantiaTipo_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrdSocios_Obtener")]
        public ErrorDto<List<CrConsultaCrdSociosData>> CR_ConsultaCrdSocios_Obtener(int CodEmpresa)
        {
            return _BL.CR_ConsultaCrdSocios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrdConsulta_Integrada_Obtener")]
        public ErrorDto<CrConsultaCrdData> CR_ConsultaCrdConsulta_Integrada_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            return _BL.CR_ConsultaCrdConsulta_Integrada_Obtener(CodEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpPost("CR_Socios_RegistrarNota")]
        public ErrorDto CR_Socios_RegistrarNota(int CodEmpresa, string cedula, string nota, string usuario)
        {
            return _BL.CR_Socios_RegistrarNota(CodEmpresa, cedula, nota, usuario);
        }

        [Authorize]
        [HttpPost("CR_Socios_BloqueoCreditos_Guardar")]
        public ErrorDto CR_Socios_BloqueoCreditos_Guardar(
            int CodEmpresa,
            string cedula,
            bool bloqueo,
            string nota,
            string usuario)
        {
            return _BL.CR_Socios_BloqueoCreditos_Guardar(
                CodEmpresa,
                cedula,
                bloqueo,
                nota,
                usuario);
        }

        [Authorize]
        [HttpGet("fxCajas_SaldoaFavor")]
        public ErrorDto<decimal> fxCajas_SaldoaFavor(int CodEmpresa, string cedula)
        {
            return _BL.fxCajas_SaldoaFavor(CodEmpresa, cedula);
        }



        #region Créditos

        [Authorize]
        [HttpGet("CR_ConsultaCrd_CreditoContexto_Obtener")]
        public ErrorDto<CrConsultaCreditoContextoData> CR_ConsultaCrd_CreditoContexto_Obtener(
            int CodEmpresa)
        {
            return _BL.CR_ConsultaCrd_CreditoContexto_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrd_SoSResumen_Obtener")]
        public ErrorDto<List<CrConsultaSoSResumenData>> CR_ConsultaCrd_SoSResumen_Obtener(
            int CodEmpresa,
            string cedula,
            string usuario)
        {
            return _BL.CR_ConsultaCrd_SoSResumen_Obtener(CodEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrd_SoSOperaciones_Obtener")]
        public ErrorDto<List<CrConsultaSoSOperacionData>> CR_ConsultaCrd_SoSOperaciones_Obtener(
            int CodEmpresa,
            string cedula,
            decimal proceso,
            string usuario)
        {
            return _BL.CR_ConsultaCrd_SoSOperaciones_Obtener(
                CodEmpresa,
                cedula,
                proceso,
                usuario);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrd_SoSExclusion_Obtener")]
        public ErrorDto<CrConsultaSoSExclusionData> CR_ConsultaCrd_SoSExclusion_Obtener(
            int CodEmpresa,
            string cedula,
            string usuario)
        {
            return _BL.CR_ConsultaCrd_SoSExclusion_Obtener(CodEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpPost("CR_ConsultaCrd_SoSExclusion_Guardar")]
        public ErrorDto CR_ConsultaCrd_SoSExclusion_Guardar(
            int CodEmpresa,
            string cedula,
            bool excluir,
            string usuario)
        {
            return _BL.CR_ConsultaCrd_SoSExclusion_Guardar(
                CodEmpresa,
                cedula,
                excluir,
                usuario);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrd_Cancelacion_Obtener")]
        public ErrorDto<CrConsultaCancelacionData> CR_ConsultaCrd_Cancelacion_Obtener(
            int CodEmpresa,
            int operacion,
            DateTime corte)
        {
            return _BL.CR_ConsultaCrd_Cancelacion_Obtener(
                CodEmpresa,
                operacion,
                corte);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrd_PreAnalisisOperacion_Obtener")]
        public ErrorDto<string> CR_ConsultaCrd_PreAnalisisOperacion_Obtener(
            int CodEmpresa,
            int operacion)
        {
            return _BL.CR_ConsultaCrd_PreAnalisisOperacion_Obtener(
                CodEmpresa,
                operacion);
        }

        [Authorize]
        [HttpGet("CR_ConsultaPlanillaAbonoDist_Inicializar")]
        public ErrorDto<CrConsultaPlanillaAbonoDistInicialData> CR_ConsultaPlanillaAbonoDist_Inicializar(
            int CodEmpresa,
            string cedula)
        {
            return _BL.CR_ConsultaPlanillaAbonoDist_Inicializar(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CR_ConsultaPlanillaAbonoDist_UltimoMonto")]
        public ErrorDto<CrConsultaPlanillaAbonoDistUltimoData> CR_ConsultaPlanillaAbonoDist_UltimoMonto(
            int CodEmpresa,
            string cedula,
            int codInstitucion,
            int proceso)
        {
            return _BL.CR_ConsultaPlanillaAbonoDist_UltimoMonto(
                CodEmpresa,
                cedula,
                codInstitucion,
                proceso);
        }

        [Authorize]
        [HttpGet("CR_ConsultaPlanillaAbonoDist_Consultar")]
        public ErrorDto<List<CrConsultaPlanillaAbonoDistDetalleData>> CR_ConsultaPlanillaAbonoDist_Consultar(
            int CodEmpresa,
            string cedula,
            int codInstitucion,
            int proceso,
            decimal monto,
            DateTime corte)
        {
            return _BL.CR_ConsultaPlanillaAbonoDist_Consultar(
                CodEmpresa,
                cedula,
                codInstitucion,
                proceso,
                monto,
                corte);
        }
        
        [Authorize]
        [HttpGet("CR_ConsultaCrd_Creditos_Obtener")]
        public ErrorDto<List<CrConsultaCrdCreditosData>> CR_ConsultaCrd_Creditos_Obtener(int CodEmpresa, string cedula, string sheetName)
        {
            return _BL.CR_ConsultaCrd_Creditos_Obtener(CodEmpresa, cedula, sheetName);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrd_Tramite_Obtener")]
        public ErrorDto<List<CrConsultaCreditosData>> CR_ConsultaCrd_Tramite_Obtener(int CodEmpresa, string cedula)
        {
            return _BL.CR_ConsultaCrd_Tramite_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrd_PreAnalisis_Obtener")]
        public ErrorDto<List<CrConsultaCrdPreanalisisData>> CR_ConsultaCrd_PreAnalisis_Obtener(int CodEmpresa, string cedula)
        {
            return _BL.CR_ConsultaCrd_PreAnalisis_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CR_ConsultaCrd_Incobrable_Obtener")]
        public ErrorDto<List<CrConsultaCrdIncobrableData>> CR_ConsultaCrd_Incobrable_Obtener(int CodEmpresa, string cedula)
        {
            return _BL.CR_ConsultaCrd_Incobrable_Obtener(CodEmpresa, cedula);
        }

        #endregion

        #region Cobros

        [Authorize]
        [HttpGet("CR_ConsultaCobros_Obtener")]
        public ErrorDto<List<CrConsultaCobroDto>> CR_ConsultaCobros_Obtener(int CodEmpresa, string cedula)
        {
            return _BL.CR_ConsultaCobros_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CR_ConsultaAsignacion_Obtener")]
        public ErrorDto<List<CrConsultaAsignacionCobroData>> CR_ConsultaAsignacion_Obtener(int CodEmpresa, string cedula)
        {
            return _BL.CR_ConsultaAsignacion_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("CR_ConsultaCobros_NotificacionEmail_Procesar")]
        public ErrorDto CR_ConsultaCobros_NotificacionEmail_Procesar(
            int CodEmpresa,
            string cedula,
            string tipo,
            string usuario)
        {
            return _BL.CR_ConsultaCobros_NotificacionEmail_Procesar(
                CodEmpresa,
                cedula,
                tipo,
                usuario);
        }

        #endregion

        #region Ahorros

        [Authorize]
        [HttpGet("CR_ContratosConsulta_Obtener")]
        public ErrorDto<List<CrConsultaContratosData>> CR_ContratosConsulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            return _BL.CR_ContratosConsulta_Obtener(CodEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpGet("CR_Contratos_Movimientos_Obtener")]
        public ErrorDto<List<CrContratosMovimientosData>> CR_Contratos_Movimientos_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return _BL.CR_Contratos_Movimientos_Obtener(codEmpresa, codOperadora, codPlan, codContrato);

        }

        [Authorize]
        [HttpGet("CR_Contratos_Cupones_Obtener")]
        public ErrorDto<List<CrContratosCuponesData>> CR_Contratos_Cupones_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return _BL.CR_Contratos_Cupones_Obtener(codEmpresa, codOperadora, codPlan, codContrato);

        }

        [Authorize]
        [HttpGet("CR_Contratos_Bitacora_Obtener")]
        public ErrorDto<List<CrContratosBitacoraData>> CR_Contratos_Bitacora_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return _BL.CR_Contratos_Bitacora_Obtener(codEmpresa, codOperadora, codPlan, codContrato);

        }

        [Authorize]
        [HttpGet("CR_Contratos_Cierres_Obtener")]
        public ErrorDto<List<CrContratosCierresData>> CR_Contratos_Cierres_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            return _BL.CR_Contratos_Cierres_Obtener(codEmpresa, codOperadora, codPlan, codContrato);

        }

        [Authorize]
        [HttpGet("Cajas_Sesion_ObtenerActiva")]
        public ErrorDto<CajasSesionDto> Cajas_Sesion_ObtenerActiva(int CodEmpresa, string usuario, string identificacion)
        {
            return _BL.Cajas_Sesion_ObtenerActiva(CodEmpresa, usuario, identificacion);
        }

        #endregion

        #region Patrimonio

        [Authorize]
        [HttpGet("CR_Patrimonio_Obtener")]
        public ErrorDto<List<CrPatrimonioData>> CR_Patrimonio_Obtener(int CodEmpresa, string cedula, string tipo)
        {
            return _BL.CR_Patrimonio_Obtener(CodEmpresa, cedula, tipo);
        }

        [Authorize]
        [HttpGet("CR_Patrimonio_Garantia_Obtener")]
        public ErrorDto<CrPatrimonioGarantiaData?> CR_Patrimonio_Garantia_Obtener(
            int CodEmpresa,
            string cedula,
            string garantia)
        {
            return _BL.CR_Patrimonio_Garantia_Obtener(CodEmpresa, cedula, garantia);
        }

        [Authorize]
        [HttpGet("EXC_Periodos_Visibles_Obtener")]
        public ErrorDto<List<ExcPeriodosVisiblesData>> EXC_Periodos_Visibles_Obtener(int CodEmpresa, string cedula)
        {
            return _BL.EXC_Periodos_Visibles_Obtener(CodEmpresa, cedula);
        }

        #endregion

        #region Beneficios

        [Authorize]
        [HttpGet("AFI_Beneficios_Consulta")]
        public ErrorDto<List<AfiBeneficiosConsultaData>> AFI_Beneficios_Consulta(int CodEmpresa, string cedula)
        {
            return _BL.AFI_Beneficios_Consulta(CodEmpresa, cedula);
        }

        #endregion

        #region Renuncias

        [Authorize]
        [HttpGet("AFI_ConsultaRenunciaTransito")]
        public ErrorDto<List<AfiRenunciaTransitoData>> AFI_ConsultaRenunciaTransito(int CodEmpresa, string cedula)
        {
            return _BL.AFI_ConsultaRenunciaTransito(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("AFI_Renuncias_Consulta")]
        public ErrorDto<List<AfiRenunciasConsultaData>> AFI_Renuncias_Consulta(int CodEmpresa, string cedula)
        {
            return _BL.AFI_Renuncias_Consulta(CodEmpresa, cedula);
        }

        #endregion

        #region Mensajes

        [Authorize]
        [HttpGet("AFI_Socios_Mensajes_Obtener")]
        public ErrorDto<List<AfiSociosMensajesData>> AFI_Socios_Mensajes_Obtener(int CodEmpresa, string cedula, string tipo)
        {
            return _BL.AFI_Socios_Mensajes_Obtener(CodEmpresa, cedula, tipo);
        }

        [Authorize]
        [HttpPost("AFI_Socios_Mensajes_Guardar")]
        public ErrorDto AFI_Socios_Mensajes_Guardar(int codEmpresa, AfiSociosMensajesData data)
        {
            return _BL.AFI_Socios_Mensajes_Guardar(codEmpresa, data);
        }

        [Authorize]
        [HttpDelete("AFI_Socios_Mensajes_Elimina")]
        public ErrorDto AFI_Socios_Mensajes_Elimina(int codEmpresa, string data)
        {
            return _BL.AFI_Socios_Mensajes_Elimina(codEmpresa, data);
        }

        [Authorize]
        [HttpPatch("AFI_Socios_Mensajes_Resolucion")]
        public ErrorDto AFI_Socios_Mensajes_Resolucion(int codEmpresa, string usuario, AfiSociosMensajesData data)
        {
            return _BL.AFI_Socios_Mensajes_Resolucion(codEmpresa, usuario, data);
        }

        #endregion

        #region Correo

        [Authorize]
        [HttpGet("Sys_Mail_Load")]
        public ErrorDto<List<SysMailLoadData>> Sys_Mail_Load(int CodEmpresa, string cedula)
        {
            return _BL.Sys_Mail_Load(CodEmpresa, cedula);
        }

        #endregion

        #region Info

        [Authorize]
        [HttpGet("AF_Persona_Consulta_Obtener")]
        public ErrorDto<CRConsultaInfoDto> AF_Persona_Consulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            return _BL.AF_Persona_Consulta_Obtener(CodEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpPost("AF_Persona_Canales_Registra")]
        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string request)
        {
            return _BL.AF_Persona_Canales_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Bienes_Registra")]
        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string request)
        {
            return _BL.AF_Persona_Bienes_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Escolaridad_Registra")]
        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            return _BL.AF_Persona_Escolaridad_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Preferencia_Registra")]
        public ErrorDto AF_Persona_Preferencia_Registra(int CodEmpresa, string request)
        {
            return _BL.AF_Persona_Preferencia_Registra(CodEmpresa, request);
        }

        #endregion

        #region Estado

        [Authorize]
        [HttpGet("CR_ConsultaCrdEstado_Integrada_Obtener")]
        public ErrorDto<EmpresaEnlaceResultDto> CR_ConsultaCrdEstado_Integrada_Obtener(int codEmpresa)
        {
            return _BL.ConsultaVersionEmpresa(codEmpresa);
        }

        #endregion

        #region Aut/C.I

        [Authorize]
        [HttpPost("CR_RegistraConsentimiento")]
        public ErrorDto CR_RegistraConsentimiento(int CodEmpresa, string cedula, string usuario)
        {
            return _BL.CR_RegistraConsentimiento(CodEmpresa, cedula, usuario);
        }

         #endregion
    }
}
