using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del catálogo de Requisitos para Beneficios (frmAF_Beneficios_Requisitos).
    /// </summary>
    public class FrmAfBeneficiosRequisitosBL
    {
        private readonly FrmAfBeneficiosRequisitosDB _db;

        public FrmAfBeneficiosRequisitosBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosRequisitosDB(config);
        }

        /// <summary>Lista de requisitos con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<BeneRequisitosDataLista> AfBeneRequisitos_Obtener(int CodCliente, string? filtros)
            => _db.AfBeneRequisitos_Obtener(CodCliente, DeserializarFiltros(filtros));

        /// <summary>Exporta la lista de requisitos aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<BeneRequisitosData>> AfBeneRequisitos_Exportar(int CodCliente, string? filtros)
            => _db.AfBeneRequisitos_Exportar(CodCliente, DeserializarFiltros(filtros));

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

        /// <summary>Inserta un requisito (o actualiza si existe).</summary>
        public ErrorDto AfBeneRequisitos_Insertar(int CodCliente, BeneRequisitosData requisito)
            => _db.AfBeneRequisitos_Insertar(CodCliente, requisito);

        /// <summary>Actualiza un requisito.</summary>
        public ErrorDto AfBeneRequisitos_Actualizar(int CodCliente, BeneRequisitosData requisito)
            => _db.AfBeneRequisitos_Actualizar(CodCliente, requisito);

        /// <summary>Elimina un requisito.</summary>
        public ErrorDto AfBeneRequisitos_Eliminar(int CodCliente, string cod_requisito)
            => _db.AfBeneRequisitos_Eliminar(CodCliente, cod_requisito);
    }
}
