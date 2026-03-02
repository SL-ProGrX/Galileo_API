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

        public ErrorDto<bool> CntXEfSeccion_Guardar(int codEmpresa, CntXEfSeccionSaveParams param)
        {
            return _db.CntXEfSeccion_Guardar(codEmpresa, param);
        }

        public ErrorDto<bool> CntXEfSeccion_Eliminar(int codEmpresa, CntXEfSeccionDeleteParams param)
        {
            return _db.CntXEfSeccion_Eliminar(codEmpresa, param);
        }

        public ErrorDto<List<CntXEfSeccionSimpleDto>> CntXEfSeccionesItems_Lista(int codEmpresa, int codContabilidad, string codEf)
        {
            return _db.CntXEfSeccionesItems_Lista(codEmpresa, codContabilidad, codEf);
        }

        public ErrorDto<List<CntXCuentaDto>> CntXEfCuentasDisponibles_Lista(int codEmpresa, CntXCuentaFiltroParams param)
        {
            return _db.CntXEfCuentasDisponibles_Lista(codEmpresa, param);
        }

        public ErrorDto<List<CntXCuentaAsignadaDto>> CntXEfCuentasAsignadas_Lista(int codEmpresa, int codContabilidad, string codEf, string itemId)
        {
            return _db.CntXEfCuentasAsignadas_Lista(codEmpresa, codContabilidad, codEf, itemId);
        }

        public ErrorDto<List<CntXFxAsignadaDto>> CntXEfFuncionesAsignadas_Lista(int codEmpresa, int codContabilidad, string codEf, string itemId)
        {
            return _db.CntXEfFuncionesAsignadas_Lista(codEmpresa, codContabilidad, codEf, itemId);
        }

        public ErrorDto<bool> CntXEfCuenta_Proc(int codEmpresa, CntXEfCuentaProcParams param)
        {
            return _db.CntXEfCuenta_Proc(codEmpresa, param);
        }

        public ErrorDto<bool> CntXEfFx_Proc(int codEmpresa, CntXEfFxProcParams param)
        {
            return _db.CntXEfFx_Proc(codEmpresa, param);
        }

        public ErrorDto<bool> CntXEfProcesa(int codEmpresa, CntXEfProcesaParams param)
        {
            return _db.CntXEfProcesa(codEmpresa, param);
        }
    }
}
