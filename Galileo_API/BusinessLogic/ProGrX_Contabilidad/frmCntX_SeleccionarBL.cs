using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXSeleccionarBl
    {
        private readonly FrmCntXSeleccionarDb _db;

        public FrmCntXSeleccionarBl(IConfiguration config)
            => _db = new FrmCntXSeleccionarDb(config);

        public ErrorDto<CntXSeleccionarCargaResponse> CntX_Seleccionar_CargaInicial(int codEmpresa, string usuario, bool muestraTodas)
        {
            return _db.CntX_Seleccionar_CargaInicial(codEmpresa, usuario, muestraTodas);
        }

        public ErrorDto<List<CntXSeleccionarContabilidadItem>> CntX_Seleccionar_Buscar(int codEmpresa, string usuario, string filtro)
        {
            return _db.CntX_Seleccionar_Buscar(codEmpresa, usuario, filtro);
        }

        public ErrorDto<CntXParametrosDto> CntX_Seleccionar_Seleccionar(int codEmpresa, string usuario, int codContabilidad)
        {
            return _db.CntX_Seleccionar_Seleccionar(codEmpresa, usuario, codContabilidad);
        }
    }
}
