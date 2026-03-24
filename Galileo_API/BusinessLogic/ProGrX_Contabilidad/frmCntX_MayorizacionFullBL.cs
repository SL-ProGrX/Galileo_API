using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXMayorizacionFullBl
    {
        private readonly FrmCntXMayorizacionFullDb _db;

        public FrmCntXMayorizacionFullBl(IConfiguration config)
        {
            _db = new FrmCntXMayorizacionFullDb(config);
        }

        public ErrorDto<List<CntxTipoAsientoDto>> CntX_TiposAsientos_Listar(int codEmpresa,int codContabilidad)
        {
            return _db.CntX_TiposAsientos_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<bool> Procesar(int codEmpresa, int codContabilidad,CntxMayorizacionProcesarDto request)
        {
            return _db.Procesar(codEmpresa, codContabilidad, request);
        }
    }
}