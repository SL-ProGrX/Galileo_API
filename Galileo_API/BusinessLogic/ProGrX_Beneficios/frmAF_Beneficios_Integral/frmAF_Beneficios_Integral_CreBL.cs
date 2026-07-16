using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Crece de Beneficios Integrales (frmAF_Beneficios_Integral_Cre).
    /// </summary>
    public class frmAF_Beneficios_Integral_CreBL
    {
        private readonly frmAF_Beneficios_Integral_CreDB _db;

        public frmAF_Beneficios_Integral_CreBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new frmAF_Beneficios_Integral_CreDB(config);
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
