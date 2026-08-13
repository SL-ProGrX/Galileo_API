using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ARF;

namespace Galileo_API.BusinessLogic.ProGrX_ARF
{
    public class FrmArfInformesBl
    {
        private readonly FrmArfInformesDb _db;

        public FrmArfInformesBl(IConfiguration config)
        {
            _db = new FrmArfInformesDb(config);
        }

        /// <summary>
        /// Obtiene las oficinas o unidades disponibles para los informes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Resultado con las unidades disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ARF_Unidades_Listar(int codEmpresa)
        {
            return _db.ARF_Unidades_Listar(codEmpresa);
        }

        /// <summary>
        /// Obtiene los arrendadores disponibles para los informes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Resultado con los arrendadores disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ARF_Arrendadores_Listar(int codEmpresa)
        {
            return _db.ARF_Arrendadores_Listar(codEmpresa);
        }
    }
}
