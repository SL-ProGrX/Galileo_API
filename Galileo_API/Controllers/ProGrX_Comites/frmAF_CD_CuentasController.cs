using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;

namespace Galileo_API.Controllers.ProGrX_Comites
{
        [Route("api/[controller]")]
        [ApiController]
        [Authorize]
        public class FrmAfCdCuentasController : ControllerBase
        {
            private readonly FrmAfCdCuentasBl _bl;

            public FrmAfCdCuentasController(IConfiguration config)
            {
                _bl = new FrmAfCdCuentasBl(config);
            }

            [HttpGet("AfCdCuenta_Obtener")]
            public ErrorDto<AfCdCuentaData?> AfCdCuenta_Obtener(int codEmpresa, int operacion)
            {
                return _bl.AfCdCuenta_Obtener(codEmpresa, operacion);
            }

            [HttpGet("AfCdActividades_Lista_Obtener")]
            public ErrorDto<List<AfCdActividadData>> AfCdActividades_Lista_Obtener(
                int codEmpresa, string tipo, int totalAsoc, int operacion, string comite)
            {
                return _bl.AfCdActividades_Lista_Obtener(codEmpresa, tipo, totalAsoc, operacion, comite);
            }

            [HttpGet("AfCdCuenta_Adjuntos_Obtener")]
            public ErrorDto<List<AfCdCuentaAdjuntosData>> AfCdCuenta_Adjuntos_Obtener(int codEmpresa, int operacion)
            {
                return _bl.AfCdCuenta_Adjuntos_Obtener(codEmpresa, operacion);
            }

            [HttpGet("AfCdCuenta_Bitacora_Obtener")]
            public ErrorDto<List<AfCdCuentaBitacoraData>> AfCdCuenta_Bitacora_Obtener(int codEmpresa, int operacion)
            {
                return _bl.AfCdCuenta_Bitacora_Obtener(codEmpresa, operacion);
            }

            [HttpGet("AfCdCuentas_Lista_Obtener")]
            public ErrorDto<List<AfCdCuentaData>> AfCdCuentas_Lista_Obtener(int codEmpresa)
            {
                return _bl.AfCdCuentas_Lista_Obtener(codEmpresa);
            }

            [HttpGet("AfCdComites_Lista_Obtener")]
            public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
            {
                return _bl.AfCdComites_Lista_Obtener(codEmpresa);
            }

            [HttpGet("AfCdCatalogo_Lista_Obtener")]
            public ErrorDto<List<DropDownListaGenericaModel>> AfCdCatalogo_Lista_Obtener(int codEmpresa, string origen)
            {
                return _bl.AfCdCatalogo_Lista_Obtener(codEmpresa, origen);
            }

            [HttpGet("AfCdCuentasBancarias_Obtener")]
            public ErrorDto<List<AfCdCuentaBancariaData>> AfCdCuentasBancarias_Obtener(int codEmpresa, string cedula, int idBanco)
            {
                return _bl.AfCdCuentasBancarias_Obtener(codEmpresa, cedula, idBanco);
            }

            [HttpGet("AfCdMiembros_Obtener")]
            public ErrorDto<List<DropDownListaGenericaModel>> AfCdMiembros_Obtener(int codEmpresa, int codComite)
            {
                return _bl.AfCdMiembros_Obtener(codEmpresa, codComite);
            }
        }
    }

