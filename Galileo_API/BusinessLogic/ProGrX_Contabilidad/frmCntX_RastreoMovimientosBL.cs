using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXRastreoMovimientosBl
    {
        private readonly FrmCntXRastreoMovimientosDb _db;

        public FrmCntXRastreoMovimientosBl(IConfiguration config)
        {
            _db = new FrmCntXRastreoMovimientosDb(config);
        }

        public ErrorDto<List<RastreoMovimientosTablaDto>> Buscar(int codEmpresa,RastreoMovimientosFiltroDto filtros)
        {
            return _db.Buscar(codEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Contabilidades_Buscar(int codEmpresa,string tipo)
        {
            return _db.Contabilidades_Buscar(codEmpresa, tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Buscar(
            int codEmpresa,
            string tipo,
            int codigo)
        {
            return _db.Cuentas_Buscar(codEmpresa, tipo, codigo);
        }
    }
}
