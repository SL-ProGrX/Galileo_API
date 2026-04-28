using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmARFOperacionesBL
    {
        private readonly FrmARFOperacionesDB _db;

        public FrmARFOperacionesBL(IConfiguration config)
        {
            _db = new FrmARFOperacionesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Listar(int codEmpresa)
        {
            return _db.Divisas_Listar(codEmpresa);
        }

        public ErrorDto<List<ArfOperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _db.Operaciones_Listar(codEmpresa);
        }

        public ErrorDto<ArfOperacionRegistroDto> Consultar(int codEmpresa, int operacion)
        {
            return _db.Consultar(codEmpresa, operacion);
        }

        public ErrorDto<ArfOperacionRegistroDto> Scroll(int codEmpresa, int operacion, int direccion)
        {
            return _db.Scroll(codEmpresa, operacion, direccion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Listar(int codEmpresa)
        {
            return _db.Arrendadores_Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Listar(int codEmpresa)
        {
            return _db.Unidades_Listar(codEmpresa);
        }

        public ErrorDto<ArfOperacionGuardarResponseDto> Guardar(int codEmpresa, ArfOperacionGuardarRequestDto request)
        {
            return _db.Guardar(codEmpresa, request);
        }

        public ErrorDto Activar(int codEmpresa, ArfOperacionActivarRequestDto request)
        {
            return _db.Activar(codEmpresa, request);
        }

        public ErrorDto<List<ArfOperacionPlanDto>> Plan_Listar(int codEmpresa, int operacion)
        {
            return _db.Plan_Listar(codEmpresa, operacion);
        }

        public ErrorDto<List<ArfOperacionCierreDto>> Cierres_Listar(int codEmpresa, int operacion)
        {
            return _db.Cierres_Listar(codEmpresa, operacion);
        }

        public ErrorDto<List<ArfOperacionAsientoMainDto>> AsientosMain_Listar(
            int codEmpresa,
            int operacion,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            return _db.AsientosMain_Listar(codEmpresa, operacion, fechaInicio, fechaCorte);
        }

        public ErrorDto<List<ArfOperacionAsientoDetalleDto>> AsientoDetalle_Listar(
            int codEmpresa,
            int codContabilidad,
            string tipoAsiento,
            string numAsiento)
        {
            return _db.AsientoDetalle_Listar(codEmpresa, codContabilidad, tipoAsiento, numAsiento);
        }

        public ErrorDto<List<ArfOperacionCambioDto>> Cambios_Listar(int codEmpresa, int operacion)
        {
            return _db.Cambios_Listar(codEmpresa, operacion);
        }

        public ErrorDto<ArfOperacionFiniquitoPreviewDto> CierreActual_Obtener(int codEmpresa, int operacion)
        {
            return _db.CierreActual_Obtener(codEmpresa, operacion);
        }

        public ErrorDto Cambio_Aplicar(int codEmpresa, ArfOperacionCambioRequestDto request)
        {
            return _db.Cambio_Aplicar(codEmpresa, request);
        }

        public ErrorDto Finiquito_Aplicar(int codEmpresa, ArfOperacionFiniquitoRequestDto request)
        {
            return _db.Finiquito_Aplicar(codEmpresa, request);
        }
    }
}
