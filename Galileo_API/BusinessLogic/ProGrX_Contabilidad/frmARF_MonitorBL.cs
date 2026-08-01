using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Activos;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmArfMonitorBl
    {
        private readonly FrmArfMonitorDb _db;

        public FrmArfMonitorBl(IConfiguration config)
        {
            _db = new FrmArfMonitorDb(config);
        }

        /// <summary>
        /// Busca las operaciones que cumplen los filtros del monitor.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <param name="filtros">Filtros seleccionados en el monitor.</param>
        /// <returns>Operaciones encontradas.</returns>
        public ErrorDto<List<ArfMonitorTablaDto>> Buscar(int codEmpresa,ArfMonitorFiltroDto filtros)
        {
            return _db.Buscar(codEmpresa, filtros);
        }

        /// <summary>
        /// Obtiene las unidades disponibles.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Lista de unidades.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Buscar(int codEmpresa)
        {
            return _db.Unidades_Buscar(codEmpresa);
        }

        /// <summary>
        /// Obtiene los arrendadores disponibles.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Lista de arrendadores.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Buscar(int codEmpresa)
        {
            return _db.Arrendadores_Buscar(codEmpresa);
        }

        /// <summary>
        /// Obtiene los cierres disponibles para consultar el auxiliar histórico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Lista de fechas de cierre.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cierres_Buscar(int codEmpresa)
        {
            return _db.Cierres_Buscar(codEmpresa);
        }
    }
}
