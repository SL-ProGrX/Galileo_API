using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de integración Zoho Desk (frmAF_Beneficios_Zoho).
    /// </summary>
    [Route("api/Zoho")]
    [ApiController]
    public class ZohoController : ControllerBase
    {
        private readonly ZohoBL _bl;

        public ZohoController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new ZohoBL(config);
        }

        /// <summary>Lista paginada de tickets Zoho con filtros.</summary>
        [Authorize]
        [HttpGet("AfiBeneTicketsLista_Obtener")]
        public ErrorDto<AfiBeneTicketsLista> AfiBeneTicketsLista_Obtener(int CodEmpresa, string jFiltros)
            => _bl.AfiBeneTicketsLista_Obtener(CodEmpresa, jFiltros);

        /// <summary>Sincroniza tickets desde la API de Zoho Desk.</summary>
        [Authorize]
        [HttpGet("Casos_Sincronizar")]
        public ErrorDto Casos_Sincronizar(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, string entrada, string usuario)
            => _bl.Casos_Sincronizar(CodEmpresa, fechaInicio, fechaCorte, entrada, usuario);

        /// <summary>Obtiene nombres de campos custom de Zoho para homologación.</summary>
        [Authorize]
        [HttpGet("CamposCustom_Obtener")]
        public ErrorDto<List<string>> CamposCustom_Obtener(int CodEmpresa)
            => _bl.CamposCustom_Obtener(CodEmpresa);

        /// <summary>Marca un ticket como visto.</summary>
        [Authorize]
        [HttpPatch("MarcaVisto_Actualizar")]
        public ErrorDto MarcaVisto_Actualizar(int CodEmpresa, string ticket, string visto, string usuario)
            => _bl.MarcaVisto_Actualizar(CodEmpresa, ticket, visto, usuario);

        /// <summary>Retorna conteo de tickets pendientes (badge).</summary>
        [Authorize]
        [HttpGet("TicketsContador_Obtener")]
        public ErrorDto<int> TicketsContador_Obtener(int CodEmpresa)
            => _bl.TicketsContador_Obtener(CodEmpresa);

        /// <summary>Importa un ticket de Zoho como registro de beneficio.</summary>
        [Authorize]
        [HttpPost("IncluirTicket_Guardar")]
        public ErrorDto IncluirTicket_Guardar(int CodEmpresa, [FromBody] ZohoTicketAdd jsonZoho)
            => _bl.IncluirTicket_Guardar(CodEmpresa, jsonZoho);
    }
}
