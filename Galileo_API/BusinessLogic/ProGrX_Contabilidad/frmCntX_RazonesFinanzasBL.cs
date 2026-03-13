using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXRazonesFinanzasBL
    {
        private readonly FrmCntXRazonesFinanzasDB _db;

        public FrmCntXRazonesFinanzasBL(IConfiguration config)
        {
            _db = new FrmCntXRazonesFinanzasDB(config);
        }

        public ErrorDto<List<CntXRazonesFinanzasDto>> CntXRazonesFinanzas_Lista(int codEmpresa, int codContabilidad)
            => _db.CntXRazonesFinanzas_Lista(codEmpresa, codContabilidad);

        public ErrorDto<bool> CntXRazonesFinanzas_Existe(int codEmpresa, int codContabilidad)
            => _db.CntXRazonesFinanzas_Existe(codEmpresa, codContabilidad);

        public ErrorDto<bool> CntXRazonesFinanzas_Guardar(int codEmpresa, CntXRazonesFinanzasSaveParams param)
            => _db.CntXRazonesFinanzas_Guardar(codEmpresa, param);

        public ErrorDto<List<CntXRazonFinancieraDto>> CntXRazonFinanciera_Lista(int codEmpresa, int codContabilidad)
            => _db.CntXRazonFinanciera_Lista(codEmpresa, codContabilidad);

        public ErrorDto<List<CntXRazonFinancieraTipoDto>> CntXRazonFinancieraTipos_Lista(int codEmpresa, int codContabilidad)
            => _db.CntXRazonFinancieraTipos_Lista(codEmpresa, codContabilidad);

        public ErrorDto<bool> CntXRazonFinanciera_Guardar(int codEmpresa, CntXRazonFinancieraSaveParams param)
            => _db.CntXRazonFinanciera_Guardar(codEmpresa, param);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXRazonFinancieraGrupos_Combo(int codEmpresa, int codContabilidad)
        => _db.CntXRazonFinancieraGrupos_Combo(codEmpresa, codContabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXRazonFinancieraSimple_Lista(int codEmpresa, int codContabilidad, string codGrupo, string orden)
            => _db.CntXRazonFinancieraSimple_Lista(codEmpresa, codContabilidad, codGrupo, orden);

        public ErrorDto<CntXRazonNotasDto?> CntXRazonFinanciera_Notas(int codEmpresa, int codContabilidad, string codGrupo, string codRazon)
            => _db.CntXRazonFinanciera_Notas(codEmpresa, codContabilidad, codGrupo, codRazon);

        public ErrorDto<List<CntXRazonDetalleDto>> CntXRazonFinanciera_Detalle(int codEmpresa, int codContabilidad, string codRazon)
            => _db.CntXRazonFinanciera_Detalle(codEmpresa, codContabilidad, codRazon);

        public ErrorDto<CntXRazonDetalleIdxDto?> CntXRazonDetalle_ProximoIdx(int codEmpresa, int codContabilidad, string codRazon)
        => _db.CntXRazonDetalle_ProximoIdx(codEmpresa, codContabilidad, codRazon);

        public ErrorDto<int?> CntXRazonDetalle_ValidaB(int codEmpresa, int codContabilidad, string codRazon, int? excludeIdx = null)
            => _db.CntXRazonDetalle_ValidaB(codEmpresa, codContabilidad, codRazon, excludeIdx);

        public ErrorDto<bool> CntXRazonDetalle_Insertar(int codEmpresa, CntXRazonDetalleDto param)
            => _db.CntXRazonDetalle_Insertar(codEmpresa, param);

        public ErrorDto<bool> CntXRazonDetalle_Actualizar(int codEmpresa, CntXRazonDetalleDto param)
            => _db.CntXRazonDetalle_Actualizar(codEmpresa, param);

        public ErrorDto<bool> CntXRazonDetalle_Eliminar(int codEmpresa, int codContabilidad, string codRazon, int idx)
            => _db.CntXRazonDetalle_Eliminar(codEmpresa, codContabilidad, codRazon, idx);

        public ErrorDto<bool> CntXRazonFinanciera_ActualizarNotas(int codEmpresa, CntXRazonNotasUpdateParams param)
            => _db.CntXRazonFinanciera_ActualizarNotas(codEmpresa, param);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXUnidades_Combo(int codEmpresa, int codContabilidad)
            => _db.CntXUnidades_Combo(codEmpresa, codContabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXRazonesConOperadorB_Lista(int codEmpresa, int codContabilidad, string? codGrupo = null)
            => _db.CntXRazonesConOperadorB_Lista(codEmpresa, codContabilidad, codGrupo);

        public ErrorDto<bool> CntXRazonesReporte_Eliminar(int codEmpresa, string usuario, int codContabilidad)
            => _db.CntXRazonesReporte_Eliminar(codEmpresa, usuario, codContabilidad);

        public ErrorDto<bool> CntXRazonesReporte_Insertar(int codEmpresa, CntXRazonesReporteInsertParams param)
            => _db.CntXRazonesReporte_Insertar(codEmpresa, param);

        public ErrorDto<bool> CntXRazonesReporte_ActualizarMes(int codEmpresa, CntXRazonesReporteUpdateParams param)
            => _db.CntXRazonesReporte_ActualizarMes(codEmpresa, param);

        public ErrorDto<CntXRazonFormulaDto?> CntXRazonFinanciera_Formula(int codEmpresa, int codContabilidad, string codRazon)
            => _db.CntXRazonFinanciera_Formula(codEmpresa, codContabilidad, codRazon);

        public ErrorDto<CntXRazonMontoDto?> CntXRazonFinanciera_Monto(int codEmpresa, CntXRazonMontoParams param)
            => _db.CntXRazonFinanciera_Monto(codEmpresa, param);

        public ErrorDto<bool> CntXRazonFinanciera_ActualizarFormula(int codEmpresa, CntXRazonFormulaUpdateParams param)
            => _db.CntXRazonFinanciera_ActualizarFormula(codEmpresa, param);

    }
}
