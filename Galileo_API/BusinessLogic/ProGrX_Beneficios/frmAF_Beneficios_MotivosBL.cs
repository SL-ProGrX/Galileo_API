using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

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

        /// <summary>Lista de motivos de beneficios.</summary>
        public ErrorDto<BeneMotivosDataLista> BeneficiosMotivos_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
            => _db.BeneficiosMotivos_Obtener(CodEmpresa, pagina, paginacion, filtro);

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
