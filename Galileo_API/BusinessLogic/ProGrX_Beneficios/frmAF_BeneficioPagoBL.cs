using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del Pago de Beneficios (frmAF_BeneficioPago).
    /// </summary>
    public class FrmAfBeneficioPagoBL
    {
        private readonly FrmAfBeneficioPagoDB _db;

        public FrmAfBeneficioPagoBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficioPagoDB(config);
        }

        /// <summary>Lista de beneficios habilitados para pago.</summary>
        public ErrorDto<List<AfiBenePagoData>> AfiBeneficioPagoLista_Obtener(int CodCliente)
            => _db.AfiBeneficioPagoLista_Obtener(CodCliente);

        /// <summary>Tabla de pagos pendientes de un beneficio.</summary>
        public ErrorDto<List<AfiBenePago>> AfiBeneficioPagosTabla_Obtener(int CodCliente, string cod_beneficio)
            => _db.AfiBeneficioPagosTabla_Obtener(CodCliente, cod_beneficio);

        /// <summary>Nombre del beneficiario según cédula bancaria.</summary>
        public ErrorDto Beneficiarios_Obtener(int CodCliente, int consec, string cedulabn, string cod_beneficio)
            => _db.Beneficiarios_Obtener(CodCliente, consec, cedulabn, cod_beneficio);

        /// <summary>Genera el pago de los beneficios.</summary>
        public ErrorDto AfiBeneficioPago_Generar(int CodCliente, string usuario, List<AfiBenePago> tabla)
            => _db.AfiBeneficioPago_Generar(CodCliente, usuario, tabla);
    }
}
