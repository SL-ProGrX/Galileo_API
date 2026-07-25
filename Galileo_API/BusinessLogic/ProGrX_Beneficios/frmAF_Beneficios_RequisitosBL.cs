using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

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

        /// <summary>Lista de requisitos para beneficios.</summary>
        public ErrorDto<BeneRequisitosDataLista> AfBeneRequisitos_Obtener(int CodCliente, string filtros)
            => _db.AfBeneRequisitos_Obtener(CodCliente, filtros);

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
