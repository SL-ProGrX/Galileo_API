using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndContratosController : ControllerBase
    {
        private readonly FrmFndContratosBL _BL;
    
        public FrmFndContratosController(IConfiguration? config)
        {
            _BL = new FrmFndContratosBL(config);
        }

        #region General

        [Authorize]
        [HttpGet("Fnd_Contratos_Listas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_Listas_Obtener(int CodEmpresa, string lista)
        {
            return _BL.Fnd_Contratos_Listas_Obtener(CodEmpresa, lista);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_Obtener")]
        public ErrorDto<ContratosModels> Fnd_Contratos_Obtener(int CodEmpresa, int operadora, string cod_plan, int contrato, string usuario)
        {
            return _BL.Fnd_Contratos_Obtener(CodEmpresa, operadora, cod_plan, contrato, usuario);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_PlanLista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_PlanLista_Obtener(int CodEmpresa, int operadora, string usuario)
        {
            return _BL.Fnd_Contratos_PlanLista_Obtener(CodEmpresa, operadora, usuario);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_Plan_Obtener")]
        public ErrorDto<ContratosPlanModels> Fnd_Contratos_Plan_Obtener(int CodEmpresa, int operadora, string plan)
        {
            return _BL.Fnd_Contratos_Plan_Obtener(CodEmpresa, operadora, plan);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_InversionPlazos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_InversionPlazos_Obtener(int CodEmpresa, string codigo)
        {
            return _BL.Fnd_Contratos_InversionPlazos_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_Buscar")]
        public ErrorDto<FndContratosListaData> Fnd_Contratos_Buscar(int CodEmpresa, int operadora, string plan, string filtros)
        {
            return _BL.Fnd_Contratos_Buscar(CodEmpresa, operadora, plan, filtros);
        }

        [Authorize]
        [HttpPost("Fnd_Contratos_Email_Enviar")]
        public ErrorDto<string> Fnd_Contratos_Email_Enviar(int CodEmpresa, int operadora, string plan, int contrato, string usuario)
        {
            return _BL.Fnd_Contratos_Email_Enviar(CodEmpresa, operadora, plan, contrato, usuario);
        }

        [Authorize]
        [HttpDelete("Fnd_Contratos_Borrar")]
        public ErrorDto Fnd_Contratos_Borrar(int CodEmpresa, int operadora, string plan, int contrato, string usuario)
        {
            return _BL.Fnd_Contratos_Borrar(CodEmpresa, operadora, plan, contrato, usuario);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_FrecuenciaMeses_Obtener")]
        public ErrorDto<int> Fnd_Contratos_FrecuenciaMeses_Obtener(int CodEmpresa, string CuponFrecuencia)
        {
            return _BL.Fnd_Contratos_FrecuenciaMeses_Obtener(CodEmpresa, CuponFrecuencia);
        }

        [Authorize]
        [HttpPost("Fnd_Contratos_TasaRef")]
        public ErrorDto<decimal> fxTasaRef(FndContratoTasaRefParams param)
        {
            return _BL.fxTasaRef(param);
        }

        [Authorize]
        [HttpGet("Fnd_ContratosSocios_Obtener")]
        public ErrorDto<FndSociosListaData> Fnd_ContratosSocios_Obtener(int CodEmpresa, string filtros)
        {
            return _BL.Fnd_ContratosSocios_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_spFnd_Cupon_Frecuencia")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_spFnd_Cupon_Frecuencia(int CodEmpresa, string plazo_id, string plan)
        {
            return _BL.Fnd_Contratos_spFnd_Cupon_Frecuencia(CodEmpresa, plazo_id, plan);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_spFnd_Inversion_Plazos_Dias")]
        public ErrorDto<int> Fnd_Contratos_spFnd_Inversion_Plazos_Dias(int CodEmpresa, int plazo_inversion, string cboPlazo)
        {
            return _BL.Fnd_Contratos_spFnd_Inversion_Plazos_Dias(CodEmpresa, plazo_inversion, cboPlazo);
        }

        [Authorize]
        [HttpPost("Fnd_Contratos_Guardar")]
        public ErrorDto<long> Fnd_Contratos_Guardar(int CodEmpresa, string usuario, [FromBody] FndContratosGuardarRequest request)
        {
            return _BL.Fnd_Contratos_Guardar(CodEmpresa, usuario, request.Cambios, request.Contrato);
        }

        #endregion

        #region complementario

        [Authorize]
        [HttpGet("Fnd_Contratos_CuentasBancarias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_CuentasBancarias_Obtener(int CodEmpresa, string cedula, int cod_banco)
        {
            return _BL.Fnd_Contratos_CuentasBancarias_Obtener(CodEmpresa, cedula, cod_banco);
        }

        #endregion

        #region Destinos

        [Authorize]
        [HttpGet("Fnd_Contratos_Destinos_Obtener")]
        public ErrorDto<List<FndContratoDestinoData>> Fnd_Contratos_Destinos_Obtener(int CodEmpresa, int operadora, string plan, long contrato)
        {
            return _BL.Fnd_Contratos_Destinos_Obtener(CodEmpresa, operadora, plan, contrato);
        }

        [Authorize]
        [HttpPost("Fnd_Contratos_Destinos_Guardar")]
        public ErrorDto Fnd_Contratos_Destinos_Guardar(int CodEmpresa, FndContratoDestinoData destino)
        {
            return _BL.Fnd_Contratos_Destinos_Guardar(CodEmpresa, destino);
        }

        #endregion

        #region Beneficiarios
        [Authorize]
        [HttpGet("Fnd_Contratos_Beneficiarios_Obtener")]
        public ErrorDto<List<FndContratoBeneficiariosData>> Fnd_Contratos_Beneficiarios_Obtener(int CodEmpresa, int operadora, string plan, long contrato, string cedula)
        {
            return _BL.Fnd_Contratos_Beneficiarios_Obtener(CodEmpresa, operadora, plan, contrato, cedula);
        }

        #endregion

        #region SubCuentas
        [Authorize]
        [HttpGet("Fnd_Contratos_SubCuentas_Obtener")]
        public ErrorDto<List<FndContratoSubCuentasData>> Fnd_Contratos_SubCuentas_Obtener(int CodEmpresa, int operadora, string plan, long contrato, string cedula)
        {
            return _BL.Fnd_Contratos_SubCuentas_Obtener(CodEmpresa, operadora, plan, contrato, cedula);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_SubCuentaContrato")]
        public ErrorDto<int> Fnd_Contratos_SubCuentaContrato(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _BL.fxSubCuentaContrato(CodEmpresa, pOperadora, pPlan, pContrato);
        }

        [Authorize]
        [HttpPost("Fnd_Contratos_SubCuentas_Guardar")]
        public ErrorDto Fnd_Contratos_SubCuentas_Guardar(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            return _BL.Fnd_Contratos_SubCuentas_Guardar(CodEmpresa, usuario, subCuenta);
        }

        #endregion

        #region Retiros

        [Authorize]
        [HttpGet("Fnd_Contratos_Retiros_Obtener")]
        public ErrorDto<FndContratosLiquidacionesListaData> Fnd_Contratos_Retiros_Obtener(int CodEmpresa, int operadora, string plan, int contrato, string filtros)
        {
            return _BL.Fnd_Contratos_Retiros_Obtener(CodEmpresa, operadora, plan, contrato, filtros);
        }

        #endregion

        #region Cupones
        [Authorize]
        [HttpGet("Fnd_Contratos_Cupones_Obtener")]
        public ErrorDto<List<FndContratosCuponesData>> Fnd_Contratos_Cupones_Obtener(int CodEmpresa, int operadora, string plan, long contrato)
        {
            return _BL.Fnd_Contratos_Cupones_Obtener(CodEmpresa, operadora, plan, contrato);
        }

        #endregion

        #region Bitacora

        [Authorize]
        [HttpGet("Fnd_Contratos_Bitacora_Obtener")]
        public ErrorDto<List<FndContratoBitacoraData>> Fnd_Contratos_Bitacora_Obtener(int CodEmpresa, int operadora, string plan, long contrato)
        {
            return _BL.Fnd_Contratos_Bitacora_Obtener(CodEmpresa, operadora, plan, contrato);
        }

        #endregion

        #region TP

        [Authorize]
        [HttpGet("Fnd_Contratos_TP_Obtener")]
        public ErrorDto<FndContratoTasaPreferencial> Fnd_Contratos_TP_Obtener(int CodEmpresa, int operadora, string plan, int contrato, string cedula)
        {
            return _BL.Fnd_Contratos_TP_Obtener(CodEmpresa, operadora, plan, contrato, cedula);
        }

        [Authorize]
        [HttpPost("Fnd_Contratos_TP_Solicita")]
        public ErrorDto<FndSolicitudTpData> Fnd_Contratos_TP_Solicita(int CodEmpresa, FndContratoTasaPreferencial solicitud)
        {
            return _BL.Fnd_Contratos_TP_Solicita(CodEmpresa, solicitud);
        }

        [Authorize]
        [HttpGet("Fnd_Contratos_TP_Estado")]
        public ErrorDto<FndSolicitudTpData> Fnd_Contratos_TP_Estado(int CodEmpresa, int gestion_id)
        {
            return _BL.Fnd_Contratos_TP_Estado(CodEmpresa, gestion_id);
        }

        #endregion
    }
}