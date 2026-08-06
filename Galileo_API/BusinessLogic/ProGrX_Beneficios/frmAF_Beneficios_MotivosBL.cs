using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del catálogo de Motivos de Beneficios (frmAF_Beneficios_Motivos).
    /// </summary>
    public class FrmAfBeneficiosMotivosBL
    {
        private readonly FrmAfBeneficiosMotivosDB _db;

        public FrmAfBeneficiosMotivosBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosMotivosDB(config);
        }

        /// <summary>Lista de motivos con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<BeneMotivosDataLista> BeneficiosMotivos_Obtener(int CodEmpresa, string? filtros)
            => _db.BeneficiosMotivos_Obtener(CodEmpresa, DeserializarFiltros(filtros));

        /// <summary>Exporta la lista de motivos aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<BeneMotivos>> BeneficiosMotivos_Exportar(int CodEmpresa, string? filtros)
            => _db.BeneficiosMotivos_Exportar(CodEmpresa, DeserializarFiltros(filtros));

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

        /// <summary>Inserta un motivo de beneficio.</summary>
        public ErrorDto BeneficiosMotivos_Agregar(int CodEmpresa, BeneMotivos request)
            => _db.BeneficiosMotivos_Agregar(CodEmpresa, request);

        /// <summary>Actualiza un motivo de beneficio.</summary>
        public ErrorDto BeneficiosMotivos_Actualizar(int CodEmpresa, BeneMotivos request)
            => _db.BeneficiosMotivos_Actualizar(CodEmpresa, request);

        /// <summary>Elimina un motivo de beneficio.</summary>
        public ErrorDto BeneficiosMotivos_Eliminar(int CodEmpresa, string id)
            => _db.BeneficiosMotivos_Eliminar(CodEmpresa, id);
    }
}
