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
    public class FrmFndRemesasController : ControllerBase
    {
        private readonly FrmFndRemesasBl _bl;

        public FrmFndRemesasController(IConfiguration config)
        {
            _bl = new FrmFndRemesasBl(config);
        }

        [Authorize]
        [HttpGet("FND_Remesa_Obtener")]
        public ErrorDto<FndRemesasData> FND_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            return _bl.FND_Remesa_Obtener(CodEmpresa, Remesa);
        }

        [Authorize]
        [HttpGet("FND_Remesas_Lista_Obtener")]
        public ErrorDto<List<FndRemesasData>> FND_Remesas_Lista_Obtener(int CodEmpresa, int TabIndex, int Lineas)
        {
            return _bl.FND_Remesas_Lista_Obtener(CodEmpresa, TabIndex, Lineas);
        }

        [Authorize]
        [HttpPost("FND_Remesas_Guardar")]
        public ErrorDto FND_Remesas_Guardar(int CodEmpresa, FndRemesasData RemesaData)
        {
            return _bl.FND_Remesas_Guardar(CodEmpresa, RemesaData);
        }

        [Authorize]
        [HttpDelete("FND_Remesas_Eliminar")]
        public ErrorDto FND_Remesas_Eliminar(int CodEmpresa, int Remesa, string Usuario)
        {
            return _bl.FND_Remesas_Eliminar(CodEmpresa, Remesa, Usuario);
        }

        [Authorize]
        [HttpGet("FND_Remesas_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Remesas_Bancos_Obtener(int CodEmpresa, int Remesa)
        {
            return _bl.FND_Remesas_Bancos_Obtener(CodEmpresa, Remesa);
        }

        [Authorize]
        [HttpGet("FND_Remesa_Carga_Obtener")]
        public ErrorDto<List<FndRemesasCargaData>> FND_Remesa_Carga_Obtener(int CodEmpresa, int Remesa, int Banco)
        {
            return _bl.FND_Remesa_Carga_Obtener(CodEmpresa, Remesa, Banco);
        }

        [Authorize]
        [HttpPost("FND_Remesas_Carga_Procesar")]
        public ErrorDto FND_Remesas_Carga_Procesar(int CodEmpresa, int Remesa, string Usuario, List<int> ConsecSeleccionados)
        {
            return _bl.FND_Remesas_Carga_Procesar(CodEmpresa, Remesa, Usuario, ConsecSeleccionados);
        }

        [Authorize]
        [HttpPost("FND_Remesas_Carga_Cerrar")]
        public ErrorDto FND_Remesas_Carga_Cerrar(int CodEmpresa, int Remesa, string Usuario)
        {
            return _bl.FND_Remesas_Carga_Cerrar(CodEmpresa, Remesa, Usuario);
        }

        [Authorize]
        [HttpGet("FND_Remesas_ConsultaRetiro_Obtener")]
        public ErrorDto<string> FND_Remesas_ConsultaRetiro_Obtener(int CodEmpresa, int Consec)
        {
            return _bl.FND_Remesas_ConsultaRetiro_Obtener(CodEmpresa, Consec);
        }
    }
}