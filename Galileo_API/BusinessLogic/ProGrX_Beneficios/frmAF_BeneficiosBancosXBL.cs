using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de Bancos habilitados para Beneficios (frmAF_BeneficiosBancosX).
    /// </summary>
    public class FrmAfBeneficiosBancosXbl
    {
        private readonly FrmAfBeneficiosBancosXdb _db;

        public FrmAfBeneficiosBancosXbl(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosBancosXdb(config);
        }

        /// <summary>Lista de bancos habilitados para beneficios.</summary>
        public ErrorDto<AfBeneficiosBancosDataLista> BeneficiosBancosX_Obtener(int CodCliente, string filtros)
            => _db.BeneficiosBancosX_Obtener(CodCliente, filtros);

        /// <summary>Actualiza la configuración de un banco (cheque/transferencia).</summary>
        public ErrorDto<AfBeneficiosBancosData> BeneficiosBancosX_Actualizar(int CodCliente, AfBeneficiosBancosData data)
            => _db.BeneficiosBancosX_Actualizar(CodCliente, data);
    }
}
