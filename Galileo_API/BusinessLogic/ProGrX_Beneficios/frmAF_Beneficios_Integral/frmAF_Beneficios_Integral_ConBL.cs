using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Consultas de Beneficios Integrales (FrmAfBeneficiosIntegralCon).
    /// </summary>
    public class FrmAfBeneficiosIntegralConBL
    {
        private readonly FrmAfBeneficiosIntegralConDB _db;

        /// <summary>
        /// Inicializa la lógica de negocio con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosIntegralConBL (IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosIntegralConDB(config);
        }

        /// <summary>
        /// Obtiene la lista filtrada de beneficios de la consulta general.
        /// </summary>
        public ErrorDto<BeneConsultaDatosLista> BeneConsultasLista_Obtener(string filtro)
        {
            return _db.BeneConsultasLista_Obtener(filtro);
        }

        /// <summary>
        /// Obtiene los estados configurados para el beneficio según la categoría.
        /// </summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneConsultaEstados_Obtener(int CodCliente, string categoria)
        {
            return _db.BeneConsultaEstados_Obtener(CodCliente, categoria);
        }

        /// <summary>
        /// Obtiene la información del beneficio seleccionado.
        /// </summary>
        public ErrorDto<BeneficioDto> BeneficioIntegral_Obtener(int CodCliente, long beneficio)
        {
            return _db.BeneficioIntegral_Obtener(CodCliente, beneficio);
        }
    }
}
