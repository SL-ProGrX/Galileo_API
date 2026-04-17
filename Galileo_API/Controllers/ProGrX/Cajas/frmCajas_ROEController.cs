using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCajasRoeController : ControllerBase
    {
        private readonly FrmCajasRoeBL _bl;
        public FrmCajasRoeController(IConfiguration config)
        {
            _bl = new FrmCajasRoeBL(config);
        }

        [HttpGet("Cajas_RoeTiposIds_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeTiposIds_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_RoeTiposIds_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_RoePaises_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoePaises_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_RoePaises_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_RoeProvinciasPorPais_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeProvinciasPorPais_Obtener(
           int CodEmpresa,
           string cod_pais)
        {
            return _bl.Cajas_RoeProvinciasPorPais_Obtener(CodEmpresa, cod_pais);
        }

        [HttpGet("Cajas_RoeCantonesPorProvincia_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeCantonesPorProvincia_Obtener(int CodEmpresa,string provincia)
        {
            return _bl.Cajas_RoeCantonesPorProvincia_Obtener(CodEmpresa, provincia);
        }

        [HttpGet("Cajas_RoeDistritosPorProvinciaCanton_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeDistritosPorProvinciaCanton_Obtener(int CodEmpresa,string provincia,string canton)
        {
            return _bl.Cajas_RoeDistritosPorProvinciaCanton_Obtener(CodEmpresa, provincia, canton);
        }

        [HttpGet("Cajas_RoePorId_Obtener")]
        public ErrorDto<CajasRoeModelDto> Cajas_RoePorId_Obtener(int CodEmpresa,int id_roe)
        {
            return _bl.Cajas_RoePorId_Obtener(CodEmpresa, id_roe);
        }

        [HttpGet("Cajas_Roe_Imprime")]
        public ErrorDto<int> Cajas_Roe_Imprime(int CodEmpresa,int id_roe)
        {
            return _bl.Cajas_Roe_Imprime(CodEmpresa, id_roe);
        }

        [HttpPost("Cajas_Roe_Actualizar")]
        public ErrorDto<SpResultadoModel> Cajas_Roe_Actualizar(int CodEmpresa,CajasRoeActualizaParamsModel p)
        {
            return _bl.Cajas_Roe_Actualizar(CodEmpresa, p);
        }

        [HttpPost("Cajas_Roe_spImprime_Ejecutar")]
        public ErrorDto<SpResultadoModel> Cajas_Roe_spImprime_Ejecutar(int cod_empresa,CajasRoeImprimeParamsModel p)
        {
            return _bl.Cajas_Roe_spImprime_Ejecutar(cod_empresa, p);
        }
    }
}