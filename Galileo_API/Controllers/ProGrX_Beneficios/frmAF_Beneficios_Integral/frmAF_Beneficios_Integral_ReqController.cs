using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.GA;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del proceso Requisitos de Beneficios Integrales (FrmAfBeneficiosIntegralReq).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfBeneficiosIntegralReqController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralReqBL _bl;

        public FrmAfBeneficiosIntegralReqController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralReqBL(config);
        }

        /// <summary>Lista de requisitos del beneficio.</summary>
        [Authorize]
        [HttpGet("Bene_Reg_Requisitos_Obtener")]
        public ErrorDto<List<BeneRegRequisito>> Bene_Reg_Requisitos_Obtener(int CodCliente, int consec)
            => _bl.Bene_Reg_Requisitos_Obtener(CodCliente, consec);

        /// <summary>Registra un requisito del beneficio.</summary>
        [Authorize]
        [HttpPost("BeneRegistroRequisitos_Guardar")]
        public ErrorDto BeneRegistroRequisitos_Guardar([FromBody] BeneRequisitosGuardar requisito)
            => _bl.BeneRegistroRequisitos_Guardar(requisito);

        /// <summary>Elimina un requisito del beneficio.</summary>
        [Authorize]
        [HttpDelete("BeneRegistroRequisitos_Eliminar")]
        public ErrorDto BeneRegistroRequisitos_Eliminar(int CodCliente, string cod_beneficio, int consec, string cod_requisito, string usuario)
            => _bl.BeneRegistroRequisitos_Eliminar(CodCliente, cod_beneficio, consec, cod_requisito, usuario);

        /// <summary>Asocia el archivo GA a un requisito del beneficio.</summary>
        [Authorize]
        [HttpPost("BeneRegistroRequisito_Asociar")]
        public ErrorDto BeneRegistroRequisito_Asociar(string modulo, string TypeId, string requisito, [FromBody] DocumentosArchivoDto data)
            => _bl.BeneRegistroRequisito_Asociar(modulo, TypeId, requisito, data);
    }
}
