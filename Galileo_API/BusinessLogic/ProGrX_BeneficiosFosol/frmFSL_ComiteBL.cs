using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de los Comités Fosol (frmFSL_Comite).
    /// </summary>
    public class FrmFslComiteBL
    {
        private readonly FrmFslComiteDB _db;

        public FrmFslComiteBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslComiteDB(config);
        }

        /// <summary>Lista de comités.</summary>
        public ErrorDto<FslComitesDataLista> FslComites_Obtener(int CodCliente, string filtros)
            => _db.FslComites_Obtener(CodCliente, filtros);

        /// <summary>Comités activos.</summary>
        public ErrorDto<List<FslComitesActivosData>> FslComitesActivos_Obtener(int CodCliente)
            => _db.FslComitesActivos_Obtener(CodCliente);

        /// <summary>Miembros de un comité.</summary>
        public ErrorDto<FslMiembrosComitesDataLista> FslMiembrosComite_Obtener(int CodCliente, string filtros)
            => _db.FslMiembrosComite_Obtener(CodCliente, filtros);

        /// <summary>Guarda un comité (inserta o actualiza).</summary>
        public ErrorDto Comite_Guardar(int CodCliente, FslComitesDto comite)
            => _db.Comite_Guardar(CodCliente, comite);

        /// <summary>Elimina un comité.</summary>
        public ErrorDto FslComites_Eliminar(int CodCliente, string comite)
            => _db.FslComites_Eliminar(CodCliente, comite);

        /// <summary>Guarda un miembro de comité (inserta o actualiza).</summary>
        public ErrorDto ComiteMiembro_Guardar(int CodCliente, FslMiembrosComitesDto miembro)
            => _db.ComiteMiembro_Guardar(CodCliente, miembro);

        /// <summary>Elimina un miembro de un comité.</summary>
        public ErrorDto FslMiembrosComite_Eliminar(int CodCliente, string cedula, string comite)
            => _db.FslMiembrosComite_Eliminar(CodCliente, cedula, comite);
    }
}
