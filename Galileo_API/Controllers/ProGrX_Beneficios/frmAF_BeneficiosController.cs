using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del Mantenimiento de Beneficios (frmAF_Beneficios).
    /// </summary>
    [Route("api/frmAF_Beneficios")]
    [ApiController]
    public class FrmAfBeneficiosController : ControllerBase
    {
        private readonly FrmAfBeneficiosBL _bl;

        public FrmAfBeneficiosController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosBL(config);
        }

        /// <summary>Navegación (scroll) de beneficios.</summary>
        [Authorize]
        [HttpGet("Top1Beneficio_Obtener")]
        public ErrorDto Top1Beneficio_Obtener(int CodCliente, int Scroll, string Cod_Beneficio)
            => _bl.Top1Beneficio_Obtener(CodCliente, Scroll, Cod_Beneficio);

        /// <summary>Detalle de un beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioDTO_Obtener")]
        public ErrorDto<AfiBeneficiosDto> AfiBeneficioDTO_Obtener(int CodCliente, string Cod_Beneficio)
            => _bl.AfiBeneficioDTO_Obtener(CodCliente, Cod_Beneficio);

        /// <summary>Montos configurados de un beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioMontos_Obtener")]
        public ErrorDto<List<AfiBeneficioMontoData>> AfiBeneficioMontos_Obtener(int CodCliente, string Cod_Beneficio)
            => _bl.AfiBeneficioMontos_Obtener(CodCliente, Cod_Beneficio);

        /// <summary>Grupos y su marca de asignación a un beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioGrupos_Obtener")]
        public ErrorDto<List<AfiBeneficioGruposData>> AfiBeneficioGrupos_Obtener(int CodCliente, string Cod_Beneficio)
            => _bl.AfiBeneficioGrupos_Obtener(CodCliente, Cod_Beneficio);

        /// <summary>Nombre de una cuenta contable.</summary>
        [Authorize]
        [HttpGet("NombreCuenta_Obtener")]
        public ErrorDto NombreCuenta_Obtener(int CodCliente, string cuenta)
            => _bl.NombreCuenta_Obtener(CodCliente, cuenta);

        /// <summary>Catálogo de categorías de beneficios activas.</summary>
        [Authorize]
        [HttpGet("AfiBeneCategoria_Obtener")]
        public ErrorDto<List<AfiBeneListas>> AfiBeneCategoria_Obtener(int CodCliente)
            => _bl.AfiBeneCategoria_Obtener(CodCliente);

        /// <summary>Grupos de una categoría de beneficios.</summary>
        [Authorize]
        [HttpGet("AfiBeneGrupos_Obtener")]
        public ErrorDto<List<AfiBeneListas>> AfiBeneGrupos_Obtener(int CodCliente, string categoria)
            => _bl.AfiBeneGrupos_Obtener(CodCliente, categoria);

        /// <summary>Bitácora de un beneficio.</summary>
        [Authorize]
        [HttpGet("BitacoraBeneficio_Obtener")]
        public ErrorDto<List<BitacoraBeneficioDto>> BitacoraBeneficio_Obtener(int CodEmpresa, string Cod_Beneficio, int Consec, string? cod_grupo, string? cod_categoria)
            => _bl.BitacoraBeneficio_Obtener(CodEmpresa, Cod_Beneficio, Consec, cod_grupo, cod_categoria);

        /// <summary>Fechas de pago automático de un beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneFechasPago_Obtener")]
        public ErrorDto<List<AfiBeneFechaPagoData>> AfiBeneFechasPago_Obtener(int CodCliente, string Cod_Beneficio, int Periodo)
            => _bl.AfiBeneFechasPago_Obtener(CodCliente, Cod_Beneficio, Periodo);

        /// <summary>Inserta un beneficio.</summary>
        [Authorize]
        [HttpPost("AfiBeneficios_Insertar")]
        public ErrorDto AfiBeneficios_Insertar(int CodCliente, [FromBody] AfiBeneficiosDto Beneficio)
            => _bl.AfiBeneficios_Insertar(CodCliente, Beneficio);

        /// <summary>Actualiza un beneficio.</summary>
        [Authorize]
        [HttpPut("AfiBeneficios_Actualiza")]
        public ErrorDto AfiBeneficios_Actualiza(int CodCliente, [FromBody] AfiBeneficiosDto Beneficio)
            => _bl.AfiBeneficios_Actualiza(CodCliente, Beneficio);

        /// <summary>Elimina un beneficio.</summary>
        [Authorize]
        [HttpDelete("AfiBeneficios_Eliminar")]
        public ErrorDto AfiBeneficios_Eliminar(int CodCliente, string Cod_Beneficio)
            => _bl.AfiBeneficios_Eliminar(CodCliente, Cod_Beneficio);

        /// <summary>Asocia un grupo a un beneficio.</summary>
        [Authorize]
        [HttpPost("AfiBeneGruposB_Insertar")]
        public ErrorDto AfiBeneGruposB_Insertar(int CodCliente, string cod_grupo, string cod_beneficio)
            => _bl.AfiBeneGruposB_Insertar(CodCliente, cod_grupo, cod_beneficio);

        /// <summary>Desasocia un grupo de un beneficio.</summary>
        [Authorize]
        [HttpDelete("AfiBeneGruposB_Eliminar")]
        public ErrorDto AfiBeneGruposB_Eliminar(int CodCliente, string cod_grupo, string cod_beneficio)
            => _bl.AfiBeneGruposB_Eliminar(CodCliente, cod_grupo, cod_beneficio);

        /// <summary>Guarda un monto de beneficio (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("AfiBeneficioMontos_Guardar")]
        public ErrorDto AfiBeneficioMontos_Guardar(int CodCliente, [FromBody] AfiBeneficioMontoData Monto)
            => _bl.AfiBeneficioMontos_Guardar(CodCliente, Monto);

        /// <summary>Elimina un monto de beneficio.</summary>
        [Authorize]
        [HttpDelete("AfiBeneficioMontos_Eliminar")]
        public ErrorDto AfiBeneficioMontos_Eliminar(int CodCliente, int id_bene, string cod_beneficio)
            => _bl.AfiBeneficioMontos_Eliminar(CodCliente, id_bene, cod_beneficio);

        /// <summary>Guarda las fechas de pago automático.</summary>
        [Authorize]
        [HttpPost("AfiBeneFechasPago_Guardar")]
        public ErrorDto AfiBeneFechasPago_Guardar(int CodCliente, string Usuario, [FromBody] List<AfiBeneFechaPagoData> DataFechas)
            => _bl.AfiBeneFechasPago_Guardar(CodCliente, DataFechas, Usuario);
    }
}
