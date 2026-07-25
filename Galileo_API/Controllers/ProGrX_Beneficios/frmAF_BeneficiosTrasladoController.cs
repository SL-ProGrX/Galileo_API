using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del Traslado de Beneficios a Tesorería (frmAF_BeneficiosTraslado).
    /// </summary>
    [Route("api/frmAF_BeneficiosTraslado")]
    [ApiController]
    public class FrmAfBeneficiosTrasladoController : ControllerBase
    {
        private readonly FrmAfBeneficiosTrasladoBL _bl;

        public FrmAfBeneficiosTrasladoController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosTrasladoBL(config);
        }

        /// <summary>Lista de remesas de traslado.</summary>
        [Authorize]
        [HttpGet("AfiRemesas_Obtener")]
        public ErrorDto<AfiBeneficiosRemesasDtoLista> AfiRemesas_Obtener(int CodCliente, string filtros)
            => _bl.AfiRemesas_Obtener(CodCliente, filtros);

        /// <summary>Remesa de traslado por código.</summary>
        [Authorize]
        [HttpGet("AfiRemesa_Obtener")]
        public ErrorDto<AfiBeneficiosRemesasDto> AfiRemesa_Obtener(int CodCliente, int cod_remesa)
            => _bl.AfiRemesa_Obtener(CodCliente, cod_remesa);

        /// <summary>Oficinas con créditos pendientes de traslado en un rango de fechas.</summary>
        [Authorize]
        [HttpGet("AfiRemesaOficinasFechas_Obtener")]
        public ErrorDto<List<AfiBeneTrasladoOpciones>> AfiRemesaOficinasFechas_Obtener(int CodCliente, string inicio, string corte)
            => _bl.AfiRemesaOficinasFechas_Obtener(CodCliente, inicio, corte);

        /// <summary>Catálogo completo de oficinas.</summary>
        [Authorize]
        [HttpGet("AfiRemesaOficinas_Obtener")]
        public ErrorDto<List<AfiBeneTrasladoOpciones>> AfiRemesaOficinas_Obtener(int CodCliente)
            => _bl.AfiRemesaOficinas_Obtener(CodCliente);

        /// <summary>Bancos con pagos pendientes de traslado.</summary>
        [Authorize]
        [HttpGet("CargarBancos_Obtener")]
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarBancos_Obtener(int CodCliente, string inicio, string corte)
            => _bl.CargarBancos_Obtener(CodCliente, inicio, corte);

        /// <summary>Usuarios con pagos pendientes de traslado.</summary>
        [Authorize]
        [HttpGet("CargarUsuarios_Obtener")]
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarUsuarios_Obtener(int CodCliente, string inicio, string corte)
            => _bl.CargarUsuarios_Obtener(CodCliente, inicio, corte);

        /// <summary>Beneficios con pagos pendientes de traslado.</summary>
        [Authorize]
        [HttpGet("CargarBeneficios_Obtener")]
        public ErrorDto<List<AfiBeneTrasladoOpciones>> CargarBeneficios_Obtener(int CodCliente)
            => _bl.CargarBeneficios_Obtener(CodCliente);

        /// <summary>Cargas de beneficios pendientes de traslado.</summary>
        [Authorize]
        [HttpGet("BusquedaCargas_Obtener")]
        public ErrorDto<AfiBeneficiosCargasDataLista> BusquedaCargas_Obtener(int CodCliente, string filtros)
            => _bl.BusquedaCargas_Obtener(CodCliente, filtros);

        /// <summary>Remesas abiertas para cargas.</summary>
        [Authorize]
        [HttpGet("AfiCargasRemesas_Obtener")]
        public ErrorDto<List<AfiBeneficiosRemesasDto>> AfiCargasRemesas_Obtener(int CodCliente)
            => _bl.AfiCargasRemesas_Obtener(CodCliente);

        /// <summary>Remesas cerradas listas para trasladar.</summary>
        [Authorize]
        [HttpGet("AfiTraslados_Obtener")]
        public ErrorDto<List<AfiBeneficiosRemesasDto>> AfiTraslados_Obtener(int CodCliente)
            => _bl.AfiTraslados_Obtener(CodCliente);

        /// <summary>Beneficios de una remesa lista para traslado.</summary>
        [Authorize]
        [HttpGet("AfiTraslado_Obtener")]
        public ErrorDto<AfiBeneficiosCargasDataLista> AfiTraslado_Obtener(int CodCliente, string filtros)
            => _bl.AfiTraslado_Obtener(CodCliente, filtros);

        /// <summary>Tokens disponibles para la liquidación.</summary>
        [Authorize]
        [HttpGet("Afi_LiqAsientosToken_Obtener")]
        public ErrorDto<List<TokenConsultaModel>> Afi_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
            => _bl.Afi_LiqAsientosToken_Obtener(CodEmpresa, usuario);

        /// <summary>Informe superior de remesas.</summary>
        [Authorize]
        [HttpGet("AfiInformesTop_Obtener")]
        public ErrorDto<AfiBeneficiosRemesasDtoLista> AfiInformesTop_Obtener(int CodCliente, string filtros)
            => _bl.AfiInformesTop_Obtener(CodCliente, filtros);

        /// <summary>Cubo de beneficios (consulta detallada) por rango de fechas.</summary>
        [Authorize]
        [HttpPost("Cubo_Beneficios_Obtener")]
        public ErrorDto<List<CuboBeneficiosData>> Cubo_Beneficios_Obtener(int CodCliente, [FromBody] CuboParametros remesa)
            => _bl.Cubo_Beneficios_Obtener(CodCliente, remesa);

        /// <summary>Inserta una remesa de traslado.</summary>
        [Authorize]
        [HttpPost("AfiRemesa_Insertar")]
        public ErrorDto AfiRemesa_Insertar(int CodCliente, [FromBody] AfiBeneficiosRemesasDto remesa)
            => _bl.AfiRemesa_Insertar(CodCliente, remesa);

        /// <summary>Actualiza una remesa de traslado.</summary>
        [Authorize]
        [HttpPut("AfiRemesa_Actualizar")]
        public ErrorDto AfiRemesa_Actualizar(int CodCliente, [FromBody] AfiBeneficiosRemesasDto remesa)
            => _bl.AfiRemesa_Actualizar(CodCliente, remesa);

        /// <summary>Elimina una remesa de traslado.</summary>
        [Authorize]
        [HttpDelete("AfiRemesa_Eliminar")]
        public ErrorDto AfiRemesa_Eliminar(int CodCliente, long cod_remesa)
            => _bl.AfiRemesa_Eliminar(CodCliente, cod_remesa);

        /// <summary>Aplica una remesa a los beneficios seleccionados.</summary>
        [Authorize]
        [HttpPost("CargaCarga_Aplicar")]
        public ErrorDto CargaCarga_Aplicar(int CodCliente, [FromBody] string carga)
            => _bl.CargaCarga_Aplicar(CodCliente, carga);

        /// <summary>Cierra una remesa de traslado.</summary>
        [Authorize]
        [HttpPost("CargasCarga_Cerrar")]
        public ErrorDto CargasCarga_Cerrar(int CodCliente, string cod_remesa, string usuario)
            => _bl.CargasCarga_Cerrar(CodCliente, cod_remesa, usuario);

        /// <summary>Aplica el traslado a tesorería de los beneficios de una remesa.</summary>
        [Authorize]
        [HttpPost("AfiTraslado_Aplicar")]
        public Task<ErrorDto> AfiTraslado_Aplicar(int CodCliente, [FromBody] string traslado)
            => _bl.AfiTraslado_Aplicar(CodCliente, traslado);

        /// <summary>Genera un nuevo token para la liquidación.</summary>
        [Authorize]
        [HttpPost("Afi_LiqAsientoToken_Nuevo")]
        public ErrorDto Afi_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
            => _bl.Afi_LiqAsientoToken_Nuevo(CodEmpresa, usuario);
    }
}
