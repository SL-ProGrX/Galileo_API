using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesEmisionDocumentosController : ControllerBase
    {
        private readonly FrmTesEmisionDocumentosBL _bl;

        public FrmTesEmisionDocumentosController(IConfiguration config)
        {
            _bl = new FrmTesEmisionDocumentosBL(config);
        }


        [HttpGet("TES_EmisionDocumentos_Ctas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumentos_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.TES_EmisionDocumentos_Ctas_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("TES_EmisionDocumentos_TiposDocs_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumentos_TiposDocs_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return _bl.TES_EmisionDocumentos_TiposDocs_Obtener(CodEmpresa, usuario, banco);
        }

        [HttpGet("TES_EmisionDocumento_Formato_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Formato_Obtener(int CodEmpresa, int banco)
        {
            return _bl.TES_EmisionDocumento_Formato_Obtener(CodEmpresa, banco);
        }

        [HttpGet("TES_EmisionDocumento_Plan_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Plan_Obtener(int CodEmpresa, int banco)
        {
            return _bl.TES_EmisionDocumento_Plan_Obtener(CodEmpresa, banco);
        }

        [HttpGet("TES_EmisionDocumento_Buscar")]
        public ErrorDto<TesTransaccionesData> TES_EmisionDocumento_Buscar(int CodEmpresa, string tipoDoc, int banco, string plan)
        {
            return _bl.TES_EmisionDocumento_Buscar(CodEmpresa, tipoDoc, banco, plan);
        }

        [HttpGet("TES_EmisionDocumento_Solicitudes_Obtener")]
        public ErrorDto<List<TesSolicitudesGenData>> TES_EmisionDocumento_Solicitudes_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.TES_EmisionDocumento_Solicitudes_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("TES_EmisionDocumento_TipoDocGestion")]
        public ErrorDto<string> TES_EmisionDocumento_TipoDocGestion(int CodEmpresa, int banco, string tipoDoc)
        {
            return _bl.TES_EmisionDocumento_TipoDocGestion(CodEmpresa, banco, tipoDoc);
        }

        [HttpPost("TES_EmisionDocumento_ValidaNumDocumento")]
        public ErrorDto TES_EmisionDocumento_ValidaNumDocumento(int CodEmpresa, int banco, string tipoDoc, int docInicial, int cantidadList)
        {
            return _bl.TES_EmisionDocumento_ValidaNumDocumento(CodEmpresa, banco, tipoDoc, docInicial, cantidadList);
        }

        [HttpPost("TES_EmisionDocumento_RevisaCuentas_SP")]
        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            return _bl.TES_EmisionDocumento_RevisaCuentas_SP(CodEmpresa, banco);
        }

        [HttpGet("TES_EmisionDocumento_SolicitudesCtaPuente_Obtener")]
        public ErrorDto<List<TesTransaccionDto>> TES_EmisionDocumento_SolicitudesCtaPuente_Obtener(int CodEmpresa, int banco, string tipoDoc)
        {
            return _bl.TES_EmisionDocumento_SolicitudesCtaPuente_Obtener(CodEmpresa, banco, tipoDoc);
        }

        [HttpGet("TES_EmisionDocumento_CtasPuente_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_CtasPuente_Obtener(int CodEmpresa, string Usuario)
        {
            return _bl.TES_EmisionDocumento_CtasPuente_Obtener(CodEmpresa, Usuario);
        }

        [HttpPost("TES_EmisionDocumento_CtaPuente_Aplicar")]
        public ErrorDto TES_EmisionDocumento_CtaPuente_Aplicar(int CodEmpresa, int Banco, string Usuario, string Solicitudes)
        {
            return _bl.TES_EmisionDocumento_CtaPuente_Aplicar(CodEmpresa, Banco, Usuario, Solicitudes);
        }

        [HttpGet("TES_EmisionDocumento_Generar")]
        public ErrorDto<object> TES_EmisionDocumento_Generar(int CodEmpresa, string filtros)
        {
            return _bl.TES_EmisionDocumento_Generar(CodEmpresa, filtros);
        }

        [HttpGet("ValidaUsuarioEspecial")]
        public ErrorDto<int> ValidaUsuarioEspecial(int CodEmpresa, string usuario)
        {
            return _bl.ValidaUsuarioEspecial(CodEmpresa, usuario);
        }
    }
}
