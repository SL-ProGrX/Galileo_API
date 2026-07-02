using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoRefundicionesBL
    {
        private readonly FrmCrSeguimientoRefundicionesDB _db;

        public FrmCrSeguimientoRefundicionesBL(IConfiguration config)
        {
            _db = new FrmCrSeguimientoRefundicionesDB(config);
        }

        public ErrorDto<CrSeguimientoRefundicionesInicializarDto> CR_SeguimientoRefundiciones_Inicializar(
            int CodEmpresa,
            CrSeguimientoRefundicionesInicializarRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Inicializar(CodEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRefundicionesListaDto> CR_SeguimientoRefundiciones_Lista_Obtener(
            int CodEmpresa,
            CrSeguimientoRefundicionesListaRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRefundicionesListaDto> CR_SeguimientoRefundiciones_Lista_Exportar(
            int CodEmpresa,
            CrSeguimientoRefundicionesListaRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Lista_Exportar(CodEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Prestamos_Obtener(
            int CodEmpresa,
            CrSeguimientoRefundicionesPrestamosRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Prestamos_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Prestamos_Exportar(
            int CodEmpresa,
            CrSeguimientoRefundicionesPrestamosRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Prestamos_Exportar(CodEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Terceros_Obtener(
            int CodEmpresa,
            CrSeguimientoRefundicionesConsultaTercerosRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Terceros_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Terceros_Exportar(
            int CodEmpresa,
            CrSeguimientoRefundicionesConsultaTercerosRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Terceros_Exportar(CodEmpresa, request);
        }

        public ErrorDto<CrSeguimientoRefundicionDatosDto> CR_SeguimientoRefundiciones_Refunde_Datos(
            int CodEmpresa,
            CrSeguimientoRefundicionesRefundeDatosRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Refunde_Datos(CodEmpresa, request);
        }

        public ErrorDto CR_SeguimientoRefundiciones_Guardar(
            int CodEmpresa,
            CrSeguimientoRefundicionGuardarRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Guardar(CodEmpresa, request);
        }

        public ErrorDto CR_SeguimientoRefundiciones_Eliminar(
            int CodEmpresa,
            CrSeguimientoRefundicionesEliminarRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Eliminar(CodEmpresa, request);
        }

        public ErrorDto CR_SeguimientoRefundiciones_Actualizar(
            int CodEmpresa,
            CrSeguimientoRefundicionesActualizarRequest request)
        {
            return _db.CR_SeguimientoRefundiciones_Actualizar(CodEmpresa, request);
        }
    }
}