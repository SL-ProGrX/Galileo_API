using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Crece de Beneficios Integrales (FrmAfBeneficiosIntegralCre).
    /// </summary>
    public class FrmAfBeneficiosIntegralCreBL
    {
        private readonly FrmAfBeneficiosIntegralCreDB _db;

        public FrmAfBeneficiosIntegralCreBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosIntegralCreDB(config);
        }

        /// <summary>Registro Crece del beneficio.</summary>
        public ErrorDto<AfiBeneSocioCreceDto> BeneSocioCrece_Obtener(int CodCliente, int consec, string cod_beneficio)
            => _db.BeneSocioCrece_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Guarda (inserta o actualiza) el registro Crece.</summary>
        public ErrorDto BeneSocioCrece_Guardar(int CodCliente, AfiBeneSocioCreceDto beneficio)
            => _db.BeneSocioCrece_Guardar(CodCliente, beneficio);

        /// <summary>Sesiones del beneficio Crece.</summary>
        public ErrorDto<List<AfiBeneSocioCreceSesionesDto>> BeneSocioCreceSesiones_Obtener(int CodCliente, int consec, string cod_beneficio)
            => _db.BeneSocioCreceSesiones_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Guarda (inserta o actualiza) una sesión Crece.</summary>
        public ErrorDto BeneSocioCreceSesion_Guardar(int CodCliente, AfiBeneSocioCreceSesionesDto beneficio)
            => _db.BeneSocioCreceSesion_Guardar(CodCliente, beneficio);
    }
}
