using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

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

        /// <summary>Lista de profesionales con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<BeneAptProfesionalesDataLista> AfBeneAptPro_Obtener(int CodCliente, string? filtros)
            => _db.AfBeneAptPro_Obtener(CodCliente, DeserializarFiltros(filtros));

        /// <summary>Exporta la lista de profesionales aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<BeneAptProfesionalesData>> AfBeneAptPro_Exportar(int CodCliente, string? filtros)
            => _db.AfBeneAptPro_Exportar(CodCliente, DeserializarFiltros(filtros));

        /// <summary>
        /// Convierte el JSON de filtros recibido desde Angular en el modelo de carga perezosa.
        /// </summary>
        /// <param name="filtros">Filtros serializados en JSON.</param>
        /// <returns>Filtros deserializados; instancia vacía si el JSON viene nulo o inválido.</returns>
        private static FiltrosLazyLoadData DeserializarFiltros(string? filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return new FiltrosLazyLoadData();
            }

            return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
        }

        /// <summary>Lista de usuarios activos para asignar al profesional.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        public ErrorDto<List<DropDownListaGenericaModel>> AfBeneAptProUsuarios_Obtener(int CodCliente)
            => _db.AfBeneAptProUsuarios_Obtener(CodCliente);

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
