using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrx_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.Controllers.ProGrx_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfSuspendidosGestionController : ControllerBase
    {
        private readonly FrmAfSuspendidosGestionBl BlAfSuspendidosGestion;
        
        public FrmAfSuspendidosGestionController(IConfiguration config)
        {
            BlAfSuspendidosGestion = new FrmAfSuspendidosGestionBl(config);
        }

        [Authorize]
        [HttpGet("AF_Suspendidos_Bitacora_Obtener")]
        public ErrorDto<List<AfSuspendidosBitacoraDto>> AF_Suspendidos_Bitacora_Obtener(int CodEmpresa, string Filtros)
        {
            return BlAfSuspendidosGestion.AF_Suspendidos_Bitacora_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpPost("AF_Suspendidos_Gestion_Registrar")]
        public ErrorDto AF_Suspendidos_Gestion_Registrar(int CodEmpresa, string Cedula, int Accion, string Notas, string Usuario)
        {
            return BlAfSuspendidosGestion.AF_Suspendidos_Gestion_Registrar(CodEmpresa, Cedula, Accion, Notas, Usuario);
        }

        [Authorize]
        [HttpPost("AF_Suspendidos_Archivo_Cargar")]
        public ErrorDto<List<AfSuspendidosArchivoDto>> AF_Suspendidos_Archivo_Cargar(int CodEmpresa, int Valor, string Usuario, string Lista)
        {
            return BlAfSuspendidosGestion.AF_Suspendidos_Archivo_Cargar(CodEmpresa, Valor, Usuario, Lista);
        }

        [Authorize]
        [HttpPost("AF_Suspendidos_Archivo_Procesar")]
        public ErrorDto AF_Suspendidos_Archivo_Procesar(int CodEmpresa, int Valor, string Usuario)
        {
            return BlAfSuspendidosGestion.AF_Suspendidos_Archivo_Procesar(CodEmpresa, Valor, Usuario);
        }

        [Authorize]
        [HttpGet("AF_Suspendidos_Personas_Obtener")]
        public ErrorDto<TablasListaGenericaModel> AF_Suspendidos_Personas_Obtener(int CodEmpresa, string Filtro)
        {
            return BlAfSuspendidosGestion.AF_Suspendidos_Personas_Obtener(CodEmpresa, Filtro);
        }
    }
}
