using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAHExcedentesPagoBL
    {
        private readonly FrmAHExcedentesPagoDB _db;

        public FrmAHExcedentesPagoBL(IConfiguration config)
        {
            _db = new FrmAHExcedentesPagoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AH_Excedentes_Pago_Periodos_Lista_Obtener(int CodEmpresa)
        {
            return _db.AH_Excedentes_Pago_Periodos_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto AH_Excedentes_Pago_Separar_Casos_Aplicar(int CodEmpresa, ExcSepararCasosRequestDto? request)
        {
            return _db.AH_Excedentes_Pago_Separar_Casos_Aplicar(CodEmpresa, request);
        }

        public ErrorDto AH_Excedentes_Pago_Casos_Especiales_Aplicar(int CodEmpresa, ExcCasosEspecialesRequestDto? request)
        {
            return _db.AH_Excedentes_Pago_Casos_Especiales_Aplicar(CodEmpresa, request);
        }

        public ErrorDto<ExcPendientesDto> AH_Excedentes_Pago_Cuentas_Internas_Aplicar(int CodEmpresa, ExcAcreditarCuentasInternasRequestDto? request)
        {
            return _db.AH_Excedentes_Pago_Cuentas_Internas_Aplicar(CodEmpresa, request);
        }

        public ErrorDto AH_Excedentes_Pago_Tesoreria_Aplicar(int CodEmpresa, ExcTesoreriaRequestDto? request)
        {
            return _db.AH_Excedentes_Pago_Tesoreria_Aplicar(CodEmpresa, request);
        }

        public ErrorDto AH_Excedentes_Pago_Fondos_Aplicar(int CodEmpresa, ExcFondosRequestDto? request)
        {
            return _db.AH_Excedentes_Pago_Fondos_Aplicar(CodEmpresa, request);
        }

        public ErrorDto AH_Excedentes_Pago_Reclasificaciones_Aplicar(int CodEmpresa, ExcReclasificacionesRequestDto? request)
        {
            return _db.AH_Excedentes_Pago_Reclasificaciones_Aplicar(CodEmpresa, request);
        }
    }
}