using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Arrendamientos;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Activos;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Arrendamientos
{
    public class FrmArfTrasladoAsientosBl
    {
        private readonly FrmArfTrasladoAsientosDb _db;

        public FrmArfTrasladoAsientosBl(IConfiguration config)
        {
            _db = new FrmArfTrasladoAsientosDb(config);
        }

        public ErrorDto<List<ArfTrasladoTablaDto>> Buscar(
            int codEmpresa,
            ArfTrasladoFiltroDto filtros)
        {
            return _db.Buscar(codEmpresa, filtros);
        }

        public ErrorDto<bool> Trasladar(
            int codEmpresa,
            List<ArfTrasladoRequestDto> asientos)
        {
            return _db.Trasladar(codEmpresa, asientos);
        }
    }
}
