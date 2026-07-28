using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del Pago de Productos de Beneficios (frmAF_BeneProdPago).
    /// </summary>
    public class FrmAfBeneProdPagoBL
    {
        private readonly FrmAfBeneProdPagoDB _db;

        public FrmAfBeneProdPagoBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneProdPagoDB(config);
        }

        /// <summary>Lista de productos asignados pendientes de entrega.</summary>
        public ErrorDto<AfiBeneProdAsgDataList> AfiBeneProdAsgLista_Obtener(int CodCliente, string cod_beneficio, int? pagina, int? paginacion, string? filtro)
            => _db.AfiBeneProdAsgLista_Obtener(CodCliente, cod_beneficio, pagina, paginacion, filtro);

        /// <summary>Beneficios con productos asignados pendientes de pago.</summary>
        public ErrorDto<List<AfiBeneProdData>> AfiBeneficios_Obtener(int CodCliente)
            => _db.AfiBeneficios_Obtener(CodCliente);

        /// <summary>Detalle de productos asignados a un beneficio y consecutivo.</summary>
        public ErrorDto<List<AfiBeneProdAsgData>> AfiBeneProdAsg_Obtener(int CodCliente, string consec, string cod_beneficio)
            => _db.AfiBeneProdAsg_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Procesa la entrega de productos de beneficios.</summary>
        public ErrorDto AfiBeneOtorga_Actualiza(int CodCliente, string beneficio)
            => _db.AfiBeneOtorga_Actualiza(CodCliente, beneficio);
    }
}
