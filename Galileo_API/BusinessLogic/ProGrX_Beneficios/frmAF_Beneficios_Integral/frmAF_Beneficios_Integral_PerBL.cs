using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Personas de Beneficios Integrales (frmAF_Beneficios_Integral_Per).
    /// </summary>
    public class frmAF_Beneficios_Integral_PerBL
    {
        private readonly frmAF_Beneficios_Integral_PerDB _db;

        public frmAF_Beneficios_Integral_PerBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new frmAF_Beneficios_Integral_PerDB(config);
        }

        /// <summary>Lista de estados civiles.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> EstadoCivilLista_Obtener(int CodCliente)
            => _db.EstadoCivilLista_Obtener(CodCliente);

        /// <summary>Lista de niveles académicos.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> NivelAcademicoLista_Obtener(int CodCliente)
            => _db.NivelAcademicoLista_Obtener(CodCliente);

        /// <summary>Lista de nacionalidades.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> NacionalidadLista_Obtener(int CodCliente)
            => _db.NacionalidadLista_Obtener(CodCliente);

        /// <summary>Lista de países.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> PaisLista_Obtener(int CodCliente)
            => _db.PaisLista_Obtener(CodCliente);

        /// <summary>Lista de provincias.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> ProvinciaLista_Obtener(int CodCliente)
            => _db.ProvinciaLista_Obtener(CodCliente);

        /// <summary>Lista de estados laborales.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> EstadoLaboral_Obtener(int CodCliente)
            => _db.EstadoLaboral_Obtener(CodCliente);

        /// <summary>Cuentas bancarias del socio.</summary>
        public ErrorDto<List<CuentaListaData>> Cuentas_Obtener(int CodCliente, string Usuario)
            => _db.Cuentas_Obtener(CodCliente, Usuario);

        /// <summary>Datos de la persona (socio).</summary>
        public ErrorDto<AfiBeneficioIntegralPersonaData>? DatosPersona_Obtener(int CodCliente, string? cedula)
            => _db.DatosPersona_Obtener(CodCliente, cedula);

        /// <summary>Valida si el socio existe.</summary>
        public ErrorDto validaSocioExiste(int CodCliente, string cedula)
            => _db.validaSocioExiste(CodCliente, cedula);

        /// <summary>Actualiza los datos de la persona.</summary>
        public ErrorDto Persona_Actualizar(int CodCliente, string cedula, string persona)
            => _db.Persona_Actualizar(CodCliente, cedula, persona);

        /// <summary>Guarda (inserta o actualiza) un teléfono del socio.</summary>
        public ErrorDto Telefono_Guardar(int CodCliente, AfiBeneTelefonoGuardar telefono)
            => _db.Telefono_Guardar(CodCliente, telefono);

        /// <summary>Teléfonos del socio.</summary>
        public ErrorDto<List<AfiBeneTelefono>> Telefonos_Obtener(int CodCliente, string cedula)
            => _db.Telefonos_Obtener(CodCliente, cedula);

        /// <summary>Elimina un teléfono del socio.</summary>
        public ErrorDto Telefono_Eliminar(int CodCliente, int id, string usuario)
            => _db.Telefono_Eliminar(CodCliente, id, usuario);

        /// <summary>Ejecuta las validaciones de la persona.</summary>
        public ErrorDto ValidarPersona(int CodCliente, string cedula)
            => _db.ValidarPersona(CodCliente, cedula);
    }
}
