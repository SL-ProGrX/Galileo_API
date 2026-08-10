using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de Bancos habilitados para Beneficios (frmAF_BeneficiosBancosX).
    /// </summary>
    public class FrmAfBeneficiosBancosXbl
    {
        private readonly FrmAfBeneficiosBancosXdb _db;

        public FrmAfBeneficiosBancosXbl(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosBancosXdb(config);
        }

        /// <summary>Lista de bancos habilitados para beneficios con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<AfBeneficiosBancosDataLista> BeneficiosBancosX_Obtener(int CodCliente, string? filtros)
            => _db.BeneficiosBancosX_Obtener(CodCliente, DeserializarFiltros(filtros));

        /// <summary>Exporta la lista de bancos aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<AfBeneficiosBancosData>> BeneficiosBancosX_Exportar(int CodCliente, string? filtros)
            => _db.BeneficiosBancosX_Exportar(CodCliente, DeserializarFiltros(filtros));

        /// <summary>Actualiza la configuración de un banco (cheque/transferencia).</summary>
        public ErrorDto<AfBeneficiosBancosData> BeneficiosBancosX_Actualizar(int CodCliente, AfBeneficiosBancosData data)
            => _db.BeneficiosBancosX_Actualizar(CodCliente, data);

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
