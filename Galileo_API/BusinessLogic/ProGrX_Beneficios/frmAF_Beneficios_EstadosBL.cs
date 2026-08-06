using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del catálogo de Estados de Beneficios (frmAF_Beneficios_Estados).
    /// </summary>
    public class FrmAfBeneficiosEstadosBL
    {
        private readonly FrmAfBeneficiosEstadosDB _db;

        public FrmAfBeneficiosEstadosBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosEstadosDB(config);
        }

        /// <summary>Lista de estados de beneficios con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<BeneEstadoDataLista> BeneficiosEstados_Obtener(int CodEmpresa, string? filtros)
            => _db.BeneficiosEstados_Obtener(CodEmpresa, DeserializarFiltros(filtros));

        /// <summary>Exporta la lista de estados aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<BeneEstado>> BeneficiosEstados_Exportar(int CodEmpresa, string? filtros)
            => _db.BeneficiosEstados_Exportar(CodEmpresa, DeserializarFiltros(filtros));

        /// <summary>Inserta un estado de beneficio.</summary>
        public ErrorDto BeneficiosEstados_Agregar(int CodEmpresa, BeneEstado request)
            => _db.BeneficiosEstados_Agregar(CodEmpresa, request);

        /// <summary>Actualiza un estado de beneficio.</summary>
        public ErrorDto BeneficiosEstados_Actualizar(int CodEmpresa, BeneEstado request)
            => _db.BeneficiosEstados_Actualizar(CodEmpresa, request);

        /// <summary>Elimina un estado de beneficio.</summary>
        public ErrorDto BeneficiosEstados_Eliminar(int CodEmpresa, string id)
            => _db.BeneficiosEstados_Eliminar(CodEmpresa, id);

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
    }
}
