using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

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

        /// <summary>Lista de estados de beneficios.</summary>
        public ErrorDto<BeneEstadoDataLista> BeneficiosEstados_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
            => _db.BeneficiosEstados_Obtener(CodEmpresa, pagina, paginacion, filtro);

        /// <summary>Inserta un estado de beneficio.</summary>
        public ErrorDto BeneficiosEstados_Agregar(int CodEmpresa, BeneEstado request)
            => _db.BeneficiosEstados_Agregar(CodEmpresa, request);

        /// <summary>Actualiza un estado de beneficio.</summary>
        public ErrorDto BeneficiosEstados_Actualizar(int CodEmpresa, BeneEstado request)
            => _db.BeneficiosEstados_Actualizar(CodEmpresa, request);

        /// <summary>Elimina un estado de beneficio.</summary>
        public ErrorDto BeneficiosEstados_Eliminar(int CodEmpresa, string id)
            => _db.BeneficiosEstados_Eliminar(CodEmpresa, id);
    }
}
