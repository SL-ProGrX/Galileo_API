using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del catálogo de Profesionales Apremiantes (frmAF_Beneficios_APT_Profesionales).
    /// </summary>
    public class FrmAfBeneficiosAptProfesionalesBL
    {
        private readonly FrmAfBeneficiosAptProfesionalesDB _db;

        public FrmAfBeneficiosAptProfesionalesBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosAptProfesionalesDB(config);
        }

        /// <summary>Lista de profesionales apremiantes.</summary>
        public ErrorDto<BeneAptProfesionalesDataLista> AfBeneAptPro_Obtener(int CodCliente, string filtros)
            => _db.AfBeneAptPro_Obtener(CodCliente, filtros);

        /// <summary>Inserta un profesional (o actualiza si existe).</summary>
        public ErrorDto AfBeneAptPro_Insertar(int CodCliente, BeneAptProfesionalesData profesional)
            => _db.AfBeneAptPro_Insertar(CodCliente, profesional);

        /// <summary>Actualiza un profesional.</summary>
        public ErrorDto AfBeneAptPro_Actualizar(int CodCliente, BeneAptProfesionalesData profesional)
            => _db.AfBeneAptPro_Actualizar(CodCliente, profesional);

        /// <summary>Elimina un profesional.</summary>
        public ErrorDto AfBeneAptPro_Eliminar(int CodCliente, int id_profesional)
            => _db.AfBeneAptPro_Eliminar(CodCliente, id_profesional);
    }
}
