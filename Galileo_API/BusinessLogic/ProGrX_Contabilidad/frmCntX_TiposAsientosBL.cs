using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXTiposAsientosBl
    {
        private readonly FrmCntXTiposAsientosDb _db;

        public FrmCntXTiposAsientosBl(IConfiguration config) => _db = new FrmCntXTiposAsientosDb(config);

        public ErrorDto<List<CntXTiposAsientosData>> CntXTiposAsientos_Obtener(int codEmpresa, int codConta)
        { 
            return _db.CntXTiposAsientos_Obtener(codEmpresa, codConta);
        }

        public ErrorDto CntXTiposAsientos_Guardar(int codEmpresa, int codConta, string usuario, CntXTiposAsientosData request)
        {
            return _db.CntXTiposAsientos_Guardar(codEmpresa, codConta, usuario, request);
        }

        public ErrorDto CntXTiposAsientos_Eliminar(int codEmpresa, int codConta, string usuario, string tipoAsiento)
        {
            return _db.CntXTiposAsientos_Eliminar(codEmpresa, codConta, usuario, tipoAsiento);
        }

        public ErrorDto CntXTiposAsientos_Importar(int codEmpresa, int codConta, string usuario)
        {
            return _db.CntXTiposAsientos_Importar(codEmpresa, codConta, usuario);
        }
    }
}