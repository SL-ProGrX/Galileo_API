using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de integración Zoho Desk (frmAF_Beneficios_Zoho).
    /// </summary>
    public class ZohoBL
    {
        private readonly ZohoDB _db;

        public ZohoBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new ZohoDB(config);
        }

        /// <summary>Lista paginada de tickets Zoho con filtros.</summary>
        public ErrorDto<AfiBeneTicketsLista> AfiBeneTicketsLista_Obtener(int CodEmpresa, string jFiltros)
            => _db.AfiBeneTicketsLista_Obtener(CodEmpresa, jFiltros);

        /// <summary>Sincroniza tickets desde la API de Zoho Desk.</summary>
        public ErrorDto Casos_Sincronizar(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, string entrada, string usuario)
            => _db.Casos_Sincronizar(CodEmpresa, fechaInicio, fechaCorte, entrada, usuario);

        /// <summary>Obtiene nombres de campos custom de Zoho para homologación.</summary>
        public ErrorDto<List<string>> CamposCustom_Obtener(int CodEmpresa)
            => _db.CamposCustom_Obtener(CodEmpresa);

        /// <summary>Marca un ticket como visto.</summary>
        public ErrorDto MarcaVisto_Actualizar(int CodEmpresa, string ticket, string visto, string usuario)
            => _db.MarcaVisto_Actualizar(CodEmpresa, ticket, visto, usuario);

        /// <summary>Retorna conteo de tickets pendientes (badge).</summary>
        public ErrorDto<int> TicketsContador_Obtener(int CodEmpresa)
            => _db.TicketsContador_Obtener(CodEmpresa);

        /// <summary>Importa un ticket de Zoho como registro de beneficio.</summary>
        public ErrorDto IncluirTicket_Guardar(int CodEmpresa, ZohoTicketAdd jsonZoho)
            => _db.IncluirTicket_Guardar(CodEmpresa, jsonZoho);
    }
}
