using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaConfiguracionesBL
    {
        private readonly FrmPreaConfiguracionesDB _db;

        public FrmPreaConfiguracionesBL(IConfiguration config)
        {
            _db = new FrmPreaConfiguracionesDB(config);
        }

        public ErrorDto<CrPreaConfiguracionesComiteMaxListaResult> CR_Prea_Configuraciones_ComiteMax_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_ComiteMax_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CrPreaConfiguracionesComiteMaxListaResult> CR_Prea_Configuraciones_ComiteMax_Lista_Export(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_ComiteMax_Lista_Export(CodEmpresa, filtros);
        }

        public ErrorDto CR_Prea_Configuraciones_ComiteMax_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesComiteMaxGuardarRequest request)
        {
            return _db.CR_Prea_Configuraciones_ComiteMax_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto<CrPreaConfiguracionesComiteLineasListaResult> CR_Prea_Configuraciones_ComiteLineas_Lista_Obtener(int CodEmpresa, string? codigoLinea, string filtros)
        {
            return _db.CR_Prea_Configuraciones_ComiteLineas_Lista_Obtener(CodEmpresa, codigoLinea ?? string.Empty, filtros);
        }

        public ErrorDto<CrPreaConfiguracionesComiteLineasListaResult> CR_Prea_Configuraciones_ComiteLineas_Lista_Export(int CodEmpresa, string? codigoLinea, string filtros)
        {
            return _db.CR_Prea_Configuraciones_ComiteLineas_Lista_Export(CodEmpresa, codigoLinea ?? string.Empty, filtros);
        }

        public ErrorDto CR_Prea_Configuraciones_ComiteLineas_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesComiteLineasGuardarRequest request)
        {
            return _db.CR_Prea_Configuraciones_ComiteLineas_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto<List<CrPreaConfiguracionesLineaDropdownDto>> CR_Prea_Configuraciones_ComiteLineas_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _db.CR_Prea_Configuraciones_ComiteLineas_Dropdown_Obtener(CodEmpresa, filtro ?? string.Empty);
        }

        public ErrorDto<CrPreaConfiguracionesComiteAdjuntosListaResult> CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CrPreaConfiguracionesComiteAdjuntosListaResult> CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Export(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Export(CodEmpresa, filtros);
        }

        public ErrorDto CR_Prea_Configuraciones_ComiteAdjuntos_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesComiteAdjuntosGuardarRequest request)
        {
            return _db.CR_Prea_Configuraciones_ComiteAdjuntos_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto<CrPreaConfiguracionesGarantiaLiquidezListaResult> CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CrPreaConfiguracionesGarantiaLiquidezListaResult> CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Export(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Export(CodEmpresa, filtros);
        }

        public ErrorDto CR_Prea_Configuraciones_GarantiaLiquidez_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesGarantiaLiquidezGuardarRequest request)
        {
            return _db.CR_Prea_Configuraciones_GarantiaLiquidez_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto<CrPreaConfiguracionesGarantiaRefundeListaResult> CR_Prea_Configuraciones_GarantiaRefunde_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_GarantiaRefunde_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CrPreaConfiguracionesGarantiaRefundeListaResult> CR_Prea_Configuraciones_GarantiaRefunde_Lista_Export(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_GarantiaRefunde_Lista_Export(CodEmpresa, filtros);
        }

        public ErrorDto CR_Prea_Configuraciones_GarantiaRefunde_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesGarantiaRefundeGuardarRequest request)
        {
            return _db.CR_Prea_Configuraciones_GarantiaRefunde_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto<CrPreaConfiguracionesCambioEstadoListaResult> CR_Prea_Configuraciones_CambioEstado_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_CambioEstado_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CrPreaConfiguracionesCambioEstadoListaResult> CR_Prea_Configuraciones_CambioEstado_Lista_Export(int CodEmpresa, string filtros)
        {
            return _db.CR_Prea_Configuraciones_CambioEstado_Lista_Export(CodEmpresa, filtros);
        }

        public ErrorDto CR_Prea_Configuraciones_CambioEstado_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesCambioEstadoGuardarRequest request)
        {
            return _db.CR_Prea_Configuraciones_CambioEstado_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto<CrPreaConfiguracionesEdadPensionListaResult> CR_Prea_Configuraciones_EdadPension_Lista_Obtener(int CodEmpresa, string? codigoLinea, string filtros)
        {
            return _db.CR_Prea_Configuraciones_EdadPension_Lista_Obtener(CodEmpresa, codigoLinea ?? string.Empty, filtros);
        }

        public ErrorDto<CrPreaConfiguracionesEdadPensionListaResult> CR_Prea_Configuraciones_EdadPension_Lista_Export(int CodEmpresa, string? codigoLinea, string filtros)
        {
            return _db.CR_Prea_Configuraciones_EdadPension_Lista_Export(CodEmpresa, codigoLinea ?? string.Empty, filtros);
        }

        public ErrorDto CR_Prea_Configuraciones_EdadPension_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesEdadPensionGuardarRequest request)
        {
            return _db.CR_Prea_Configuraciones_EdadPension_Guardar(CodEmpresa, usuario, request);
        }
    }
}