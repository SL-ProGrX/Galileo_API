using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXRepMovTipoDocumentoBL
    {
        private readonly FrmCntXRepMovTipoDocumentoDB _db;

        public FrmCntXRepMovTipoDocumentoBL(IConfiguration config)
        {
            _db = new FrmCntXRepMovTipoDocumentoDB(config);
        }

        public ErrorDto<List<CntXTipoAsientoDto>> TiposAsiento_Lista(int codEmpresa, int codContabilidad)
            => _db.TiposAsiento_Lista(codEmpresa, codContabilidad);

        public ErrorDto<List<CntXAsientoDto>> Asientos_Lista(int codEmpresa, CntXAsientoParams param)
            => _db.Asientos_Lista(codEmpresa, param);
    }
}
