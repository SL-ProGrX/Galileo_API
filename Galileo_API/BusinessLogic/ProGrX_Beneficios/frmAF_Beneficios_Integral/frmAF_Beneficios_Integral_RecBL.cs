using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Reconocimientos de Beneficios Integrales (frmAF_Beneficios_Integral_Rec).
    /// </summary>
    public class frmAF_Beneficios_Integral_RecBL
    {
        private readonly frmAF_Beneficios_Integral_RecDB _db;

        public frmAF_Beneficios_Integral_RecBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new frmAF_Beneficios_Integral_RecDB(config);
        }

        /// <summary>Datos del reconocimiento asociado a un beneficio.</summary>
        public ErrorDto<AfiBeneReconocimientosDatos> BeneReconocimiento_Obtener(int CodCliente, int id_beneficio)
            => _db.BeneReconocimiento_Obtener(CodCliente, id_beneficio);

        /// <summary>Guarda (inserta o actualiza) el reconocimiento.</summary>
        public ErrorDto BeneReconocimiento_Guardar(int CodCliente, AfiBeneReconocimientos reconocimiento)
            => _db.BeneReconocimiento_Guardar(CodCliente, reconocimiento);

        /// <summary>Rechaza el expediente del reconocimiento.</summary>
        public ErrorDto BeneReconocimiento_Rechazar(int CodCliente, int id_beneficio, string usuario)
            => _db.BeneReconocimiento_Rechazar(CodCliente, id_beneficio, usuario);

        /// <summary>Valida si el estudiante ya está registrado en otro reconocimiento.</summary>
        public ErrorDto ValidaEstudiante_Obtener(int CodCliente, string cedula, string id_beneficio)
            => _db.ValidaEstudiante_Obtener(CodCliente, cedula, id_beneficio);

        /// <summary>Nota mínima configurada para el beneficio.</summary>
        public ErrorDto ValidaNotaMinima(int CodCliente)
            => _db.ValidaNotaMinima(CodCliente);

        /// <summary>Nota mínima para pasar la materia.</summary>
        public ErrorDto ValidaNotaPasaMateria(int CodCliente)
            => _db.ValidaNotaPasaMateria(CodCliente);
    }
}
