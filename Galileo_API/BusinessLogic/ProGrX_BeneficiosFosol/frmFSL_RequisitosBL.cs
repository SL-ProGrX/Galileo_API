using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de los Requisitos Fosol (frmFSL_Requisitos).
    /// </summary>
    public class FrmFslRequisitosBL
    {
        private readonly FrmFslRequisitosDB _db;

        public FrmFslRequisitosBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslRequisitosDB(config);
        }

        /// <summary>Lista de requisitos Fosol.</summary>
        public ErrorDto<FslRequisitosDataLista> FslRequisitos_Obtener(int CodCliente, string filtros)
            => _db.FslRequisitos_Obtener(CodCliente, filtros);

        /// <summary>Causas activas de un plan.</summary>
        public ErrorDto<List<FslPanesCausasLista>> FslPlanesCausa_Obtener(int CodCliente, string cod_plan)
            => _db.FslPlanesCausa_Obtener(CodCliente, cod_plan);

        /// <summary>Requisitos y su asignación a una causa/plan.</summary>
        public ErrorDto<List<FslRequisitoCausa>> FslRequisitoCausa_Obtener(int CodCliente, string cod_plan, string cod_causa)
            => _db.FslRequisitoCausa_Obtener(CodCliente, cod_plan, cod_causa);

        /// <summary>Planes Fosol activos.</summary>
        public ErrorDto<List<FslPlanes>> FslPlanes_Obtener(int CodCliente)
            => _db.FslPlanes_Obtener(CodCliente);

        /// <summary>Guarda un requisito (inserta o actualiza).</summary>
        public ErrorDto Requisito_Guardar(int CodCliente, FslRequisitosData requisito)
            => _db.Requisito_Guardar(CodCliente, requisito);

        /// <summary>Elimina un requisito.</summary>
        public ErrorDto FslRequisito_Eliminar(int CodCliente, string cod_requisito)
            => _db.FslRequisito_Eliminar(CodCliente, cod_requisito);

        /// <summary>Edita la asignación de un requisito a una causa/plan.</summary>
        public ErrorDto FslAsignacion_Editar(int CodCliente, FslRequisitoEditar asignacion)
            => _db.FslAsignacion_Editar(CodCliente, asignacion);
    }
}
