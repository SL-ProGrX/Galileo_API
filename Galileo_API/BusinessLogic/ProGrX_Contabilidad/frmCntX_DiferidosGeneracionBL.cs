using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXDiferidosGeneracionBL
    {
        private readonly FrmCntXDiferidosGeneracionDB _db;

        public FrmCntXDiferidosGeneracionBL(IConfiguration config)
        {
            _db = new FrmCntXDiferidosGeneracionDB(config);
        }

        public ErrorDto<List<CntXDiferidoPendienteDto>> Diferidos_Pendientes_Lista(int codEmpresa, CntXDiferidoPendienteParams param)
            => _db.Diferidos_Pendientes_Lista(codEmpresa, param);

        public ErrorDto<CntXDiferidoAsientoResult?> Diferido_Asiento(int codEmpresa, CntXDiferidoAsientoParams param)
            => _db.Diferido_Asiento(codEmpresa, param);
    }
}
