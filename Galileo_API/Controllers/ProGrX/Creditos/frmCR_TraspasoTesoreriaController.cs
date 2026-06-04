using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Credito;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRTraspasoTesoreriaController : ControllerBase
    {
        private readonly FrmCRTraspasoTesoreriaBL _BL;
        public FrmCRTraspasoTesoreriaController(IConfiguration config)
        {
            _BL = new FrmCRTraspasoTesoreriaBL(config);
        }

        #region remesas
        #endregion

        #region cargar
        #endregion

        #region trasladar

        [Authorize]
        [HttpGet("Cr_TraspasoTes_Remesas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_TraspasoTes_Remesas_Obtener(int CodEmpresa)
        {
            return _BL.Cr_TraspasoTes_Remesas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Cr_TraspasoTesToken_Obtener")]
        public ErrorDto<List<TokenConsultaModel>> Cr_TraspasoTesToken_Obtener(int CodEmpresa, string usuario)
        {
            return _BL.Cr_TraspasoTesToken_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("Cr_TraspasoTesToken_Nuevo")]
        public ErrorDto Cr_TraspasoTesToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _BL.Cr_TraspasoTesToken_Nuevo(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("Cr_TraspasoTesTraslado_Buscar")]
        public ErrorDto<List<TraspasoModel>> Cr_TraspasoTesTraslado_Buscar(int CodEmpresa, int cod_remesa)
        {
            return _BL.Cr_TraspasoTesTraslado_Buscar(CodEmpresa, cod_remesa);
        }

        #endregion

        #region informes
        #endregion

        #region reactivaciones
        #endregion

        #region cambio
        #endregion

        #region consultas
        #endregion

        #region aux.giro
        #endregion
    }
}