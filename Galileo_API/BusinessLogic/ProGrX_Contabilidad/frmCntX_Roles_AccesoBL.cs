using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntxRolesAccesoBL
    {
        private readonly FrmCntxRolesAccesoDB _db;

        public FrmCntxRolesAccesoBL(IConfiguration config)
        {
            _db = new FrmCntxRolesAccesoDB(config);
        }

        public ErrorDto<List<CntXAcRolDto>> CntXAcRol_Lista(int codEmpresa, int codContabilidad, string usuario)
            => _db.CntXAcRol_Lista(codEmpresa, codContabilidad, usuario);

        public ErrorDto<List<CntXAcCuentaDto>> CntXAcCuentas_Consulta(int codEmpresa, int codContabilidad, string rol, string ctaInicio, string ctaCorte, string filtro, string usuario)
            => _db.CntXAcCuentas_Consulta(codEmpresa, codContabilidad, rol, ctaInicio, ctaCorte, filtro, usuario);

        public ErrorDto<List<CntXAcCuentaDto>> CntXAcCuentas_Consulta_Asignadas(int codEmpresa, int codContabilidad, string rol, string filtro, string usuario)
            => _db.CntXAcCuentas_Consulta_Asignadas(codEmpresa, codContabilidad, rol, filtro, usuario);

        public ErrorDto<List<CntXAcUnidadDto>> CntXAcUnidades_Consulta(int codEmpresa, int codContabilidad, string rol, string filtro, string usuario)
            => _db.CntXAcUnidades_Consulta(codEmpresa, codContabilidad, rol, filtro, usuario);

        public ErrorDto<List<CntXAcCentroCostoDto>> CntXAcCentroCosto_Consulta(int codEmpresa, int codContabilidad, string rol, string unidad, string filtro, string usuario)
            => _db.CntXAcCentroCosto_Consulta(codEmpresa, codContabilidad, rol, unidad, filtro, usuario);

        public ErrorDto<List<CntXAcMiembroDto>> CntXAcMiembros_Consulta(int codEmpresa, int codContabilidad, string rol, string filtro, string usuario)
            => _db.CntXAcMiembros_Consulta(codEmpresa, codContabilidad, rol, filtro, usuario);

        public ErrorDto<bool> CntXAcCuentas_Asigna(int codEmpresa, CntXAcCuentaAsignaParams param)
            => _db.CntXAcCuentas_Asigna(codEmpresa, param);

        public ErrorDto<bool> CntXAcUnidades_Asigna(int codEmpresa, CntXAcUnidadAsignaParams param)
            => _db.CntXAcUnidades_Asigna(codEmpresa, param);

        public ErrorDto<bool> CntXAcCentroCosto_Asigna(int codEmpresa, CntXAcCentroCostoAsignaParams param)
            => _db.CntXAcCentroCosto_Asigna(codEmpresa, param);

        public ErrorDto<bool> CntXAcMiembros_Asigna(int codEmpresa, CntXAcMiembroAsignaParams param)
            => _db.CntXAcMiembros_Asigna(codEmpresa, param);

        public ErrorDto<bool> CntXAcRol_Add(int codEmpresa, CntXAcRolAddParams param)
            => _db.CntXAcRol_Add(codEmpresa, param);

        public ErrorDto<bool> CntXAcRol_Delete(int codEmpresa, CntXAcRolDeleteParams param)
            => _db.CntXAcRol_Delete(codEmpresa, param);
    }
}
