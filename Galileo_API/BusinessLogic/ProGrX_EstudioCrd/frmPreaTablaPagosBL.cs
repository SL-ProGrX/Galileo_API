using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaTablaPagosBl
    {
        private readonly FrmPreaTablaPagosDb _db;

        public FrmPreaTablaPagosBl(IConfiguration config)
            => _db = new FrmPreaTablaPagosDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPreaTablaPagos_ObtenerInstituciones(int codEmpresa)
        {
            return _db.CrPreaTablaPagos_ObtenerInstituciones(codEmpresa);
        }

        public ErrorDto<List<CrdPreaTablaPagosData>> CrPreaTablaPagos_Obtener(int codEmpresa, int codInstitucion)
        {
            return _db.CrPreaTablaPagos_Obtener(codEmpresa, codInstitucion);
        }

        public ErrorDto CrPreaTablaPagos_Guardar(int codEmpresa, string usuario, CrdPreaTablaPagosData request)
        {
            return _db.CrPreaTablaPagos_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CrPreaTablaPagos_Eliminar(int codEmpresa, int idx, string usuario)
        {
            return _db.CrPreaTablaPagos_Eliminar(codEmpresa, idx, usuario);
        }
    }
}
