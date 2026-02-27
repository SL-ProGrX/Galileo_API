using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXEfPersonalBL
    {
        private readonly FrmCntXEfPersonalDB _db;

        public FrmCntXEfPersonalBL(IConfiguration config)
        {
            _db = new FrmCntXEfPersonalDB(config);
        }

        public ErrorDto<List<CntXEfPersonalDto>> CntXEfPersonal_Lista(int codEmpresa, int codContabilidad)
        {
            return _db.CntXEfPersonal_Lista(codEmpresa, codContabilidad);
        }

        public ErrorDto<bool> CntXEfPersonal_Guardar(int codEmpresa, string registroUsuario, CntXEfPersonalSaveParams param)
        {
            return _db.CntXEfPersonal_Guardar(codEmpresa, registroUsuario, param);
        }

        public ErrorDto<bool> CntXEfPersonal_Eliminar(int codEmpresa, string registroUsuario, CntXEfPersonalDeleteParams param)
        {
            return _db.CntXEfPersonal_Eliminar(codEmpresa, registroUsuario, param);
        }

        public ErrorDto<List<CntXEfSeccionDto>> CntXEfSecciones_Lista(int codEmpresa, int codContabilidad, string codEf)
        {
            return _db.CntXEfSecciones_Lista(codEmpresa, codContabilidad, codEf);
        }
    }
}
