using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Apremiantes de Beneficios Integrales (FrmAfBeneficiosIntegralApr).
    /// </summary>
    public class FrmAfBeneficiosIntegralAprBL
    {
        private readonly FrmAfBeneficiosIntegralAprDB _db;

        public FrmAfBeneficiosIntegralAprBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosIntegralAprDB(config);
        }

        /// <summary>Categorías de apremiantes.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> CategoriaAPT_Obtener(int CodCliente)
            => _db.CategoriaAPT_Obtener(CodCliente);

        /// <summary>Profesionales de apremiantes.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> ProfesionalAPT_Obtener(int CodCliente)
            => _db.ProfecionalAPT_Obtener(CodCliente);

        /// <summary>Guarda un miembro del núcleo familiar.</summary>
        public ErrorDto MiembroFamiliar_Guardar(int CodCliente, BeneIntNucleoFamDto miembro)
            => _db.MiembroFamiliar_Guardar(CodCliente, miembro);

        /// <summary>Miembros del núcleo familiar del socio.</summary>
        public ErrorDto<List<BeneIntNucleoFamLista>> MiembrosFamiliar_Obtener(int CodCliente, string? cedula)
            => _db.MiembrosFamiliar_Obtener(CodCliente, cedula);

        /// <summary>Elimina un miembro del núcleo familiar.</summary>
        public ErrorDto MiembroFamiliar_Eliminar(int CodCliente, long id, string usuario)
            => _db.MiembroFamiliar_Eliminar(CodCliente, id, usuario);

        /// <summary>Situación financiera del socio por tipo.</summary>
        public ErrorDto<List<AfiBeneSocioFinanzas>> SituacionFinSocio_Obtener(int CodCliente, string? cedula, string tipo)
            => _db.SituacionFinSocio_Obtener(CodCliente, cedula, tipo);

        /// <summary>Guarda la situación financiera del socio.</summary>
        public ErrorDto SituacionFinanciera_Guardar(int CodCliente, AfiBeneSocioFinanzasGuardar finanza)
            => _db.SituacionFinanciera_Guardar(CodCliente, finanza);

        /// <summary>Elimina un registro de situación financiera.</summary>
        public ErrorDto SituacionFinanciera_Eliminar(int CodCliente, int id, string usuario)
            => _db.SituacionFinanciera_Eliminar(CodCliente, id, usuario);

        /// <summary>Síntesis financiera del socio.</summary>
        public ErrorDto<AfiBeneSintesisFinanzas> SintecisFinanciera_Obtener(int CodCliente, string? cedula)
            => _db.SintecisFinanciera_Obtener(CodCliente, cedula);

        /// <summary>Comportamiento financiero del socio.</summary>
        public ErrorDto<AfiBeneCompFinanciero> ComportamientoFinanciero_Obtener(int CodCliente, string cedula)
            => _db.ComportamientoFinanciero_Obtener(CodCliente, cedula);

        /// <summary>Lista de motivos de justificación de la categoría.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneMotivoLista_Obtener(int CodCliente, string? categoria)
            => _db.BeneMotivoLista_Obtener(CodCliente, categoria);

        /// <summary>Justificaciones del expediente del socio.</summary>
        public ErrorDto<List<AfiBeneApreJustificacion>> BeneJustificaciones_Obtener(int CodCliente, string cedula, int expediente)
            => _db.BeneJustificaciones_Obtener(CodCliente, cedula, expediente);

        /// <summary>Guarda una justificación.</summary>
        public ErrorDto BeneJustificacion_Guardar(int CodCliente, AfiBeneApreJustificacionGuardar justificacion)
            => _db.BeneJustificacion_Guardar(CodCliente, justificacion);

        /// <summary>Elimina una justificación.</summary>
        public ErrorDto BeneJustificacion_Eliminar(int CodCliente, int id_justificacion, string usuario)
            => _db.BeneJustificacion_Eliminar(CodCliente, id_justificacion, usuario);

        /// <summary>Costo de manutención configurado.</summary>
        public ErrorDto<float> CostoManutencion_Obtener(int CodCliente)
            => _db.CostoManutencion_Obtener(CodCliente);

        /// <summary>Costo de deducción configurado.</summary>
        public ErrorDto<float> CostoDeduccion_Obtener(int CodCliente)
            => _db.CostoDeduccion_Obtener(CodCliente);
    }
}
