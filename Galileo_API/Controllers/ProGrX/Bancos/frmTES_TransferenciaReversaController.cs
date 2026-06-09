using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesTransferenciaReversaController : ControllerBase
    {
        private readonly FrmTesTransferenciaReversaBL _TransferenciaReversaBL;

        public FrmTesTransferenciaReversaController(IConfiguration config)
        {
            _TransferenciaReversaBL = new FrmTesTransferenciaReversaBL(config);
        }

        [HttpGet("Tes_BancoCargaCboAccesoGestion")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGestion(int CodEmpresa, string usuario, string gestion)
        {
            return _TransferenciaReversaBL.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);
        }

        [HttpGet("Tes_BancoCargaCboSinpe")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboSinpe(int CodEmpresa, string usuario)
        {
            return _TransferenciaReversaBL.sbTesBancoCargaCboSinpe(CodEmpresa, usuario);
        }

        [HttpGet("TES_ReversaPlanes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_ReversaPlanes_Obtener(int CodEmpresa, string id_banco)
        {
            return _TransferenciaReversaBL.TES_ReversaPlanes_Obtener(CodEmpresa, id_banco);
        }

        [HttpGet("TES_sbNTrasnferencia_Obtener")]
        public ErrorDto<long> TES_sbNTrasnferencia_Obtener(int CodEmpresa, int id_banco, string tipo, string avance, string plan)
        {
            return _TransferenciaReversaBL.sbNTrasnferencia(CodEmpresa, id_banco, tipo, avance, plan);
        }

        [HttpGet("TES_TransferenciaReversa_Obtener")]
        public ErrorDto<List<TransferenciaSolicitudData>> TES_TransferenciaReversa_Obtener(int CodEmpresa, string solicitud)
        {
            return _TransferenciaReversaBL.TES_TransferenciaReversa_Obtener(CodEmpresa, solicitud);
        }

        [HttpPost("TES_TransferenciaReversa_Aplicar")]
        public ErrorDto TES_TransferenciaReversa_Aplicar(int CodEmpresa, TransferenciaReversaAplicaModel transferencia)
        {
            return _TransferenciaReversaBL.TES_TransferenciaReversa_Aplicar(CodEmpresa, transferencia);
        }

        [HttpGet("TES_TransferenciaConsulta_Obtener")]
        public ErrorDto<List<TesReversionData>> TES_TransferenciaConsulta_Obtener(
            int CodEmpresa,
            int id_banco,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            return _TransferenciaReversaBL.TES_TransferenciaConsulta_Obtener(CodEmpresa, id_banco, fechaInicio, fechaFin);
        }

        [HttpGet("TES_TransferenciaReversa_Detalle")]
        public ErrorDto<List<TransferenciaDetalleModel>> TES_TransferenciaReversa_Detalle(int CodEmpresa, string id_reversion)
        {
            return _TransferenciaReversaBL.TES_TransferenciaReversa_Detalle(CodEmpresa, id_reversion);
        }

        [HttpGet("TES_TransferenciaRevSinpe_Obtener")]
        public ErrorDto<List<TransferenciaSolicitudData>> TES_TransferenciaRevSinpe_Obtener(string reversa)
        {
            return _TransferenciaReversaBL.TES_TransferenciaRevSinpe_Obtener(reversa);
        }

        [HttpPost("TES_TransferenciaRevSinpe_Aplicar")]
        public ErrorDto TES_TransferenciaRevSinpe_Aplicar([FromBody] TesReversaSinpeModel reversa)
        {
            return _TransferenciaReversaBL.TES_TransferenciaRevSinpe_Aplicar(reversa);
        }

    }
}
