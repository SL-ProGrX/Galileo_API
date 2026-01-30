using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesDepositosLoteController : ControllerBase
    {
        private readonly FrmTesDepositosLoteBL depositosLoteBL;

        public FrmTesDepositosLoteController(IConfiguration config)
        {
            depositosLoteBL = new FrmTesDepositosLoteBL(config);
        }

        
        [HttpGet("TES_DepositosLote_Ctas_Obtener")]
        public ErrorDto<List<TesCuentaBancariaDto>> TES_DepositosLote_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            return depositosLoteBL.TES_DepositosLote_Ctas_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("TES_DepositosLote_ArchivoCarga")]
        public ErrorDto<List<TesDepositosTramiteDto>> TES_DepositosLote_ArchivoCarga(int CodEmpresa, string archivoData)
        {
            return depositosLoteBL.TES_DepositosLote_ArchivoCarga(CodEmpresa, archivoData);
        }

        [HttpPost("TES_DepositosLote_Procesar")]
        public ErrorDto TES_DepositosLote_Procesar(int CodEmpresa, string cuenta, string usuario, string archivoData)
        {
            return depositosLoteBL.TES_DepositosLote_Procesar(CodEmpresa, cuenta, usuario, archivoData);
        }

        [HttpGet("TES_DepositosLote_Inconsistencias_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_DepositosLote_Inconsistencias_Obtener(int CodEmpresa, string filtros)
        {
            return depositosLoteBL.TES_DepositosLote_Inconsistencias_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("TES_DepositosLote_Registro_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_DepositosLote_Registro_Obtener(int CodEmpresa, string filtros)
        {
            return depositosLoteBL.TES_DepositosLote_Registro_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("TES_DepositosLote_CategoriaCta_Obtener")]
        public ErrorDto<string> TES_DepositosLote_CategoriaCta_Obtener(int CodEmpresa, string Categoria)
        {
            return depositosLoteBL.TES_DepositosLote_CategoriaCta_Obtener(CodEmpresa, Categoria);
        }

        [HttpPost("TES_DepositosLote_Registro_Aplicar")]
        public ErrorDto TES_DepositosLote_Registro_Aplicar(int CodEmpresa, string Usuario, string Datos)
        {
            return depositosLoteBL.TES_DepositosLote_Registro_Aplicar(CodEmpresa, Usuario, Datos);
        }

        [HttpPost("TES_DepositosLote_Registro_Actualizar")]
        public ErrorDto TES_DepositosLote_Registro_Actualizar(int CodEmpresa)
        {
            return depositosLoteBL.TES_DepositosLote_Registro_Actualizar(CodEmpresa);
        }

        [HttpPost("TES_DepositosLote_Registro_Desvincular")]
        public ErrorDto TES_DepositosLote_Registro_Desvincular(int CodEmpresa, string Usuario, string Datos)
        {
            return depositosLoteBL.TES_DepositosLote_Registro_Desvincular(CodEmpresa, Usuario, Datos);
        }
    }
}