using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRConsultaDetalleBL
    {
        private readonly FrmCRConsultaDetalleDB _db;

        public FrmCRConsultaDetalleBL(IConfiguration config)
        {
            _db = new FrmCRConsultaDetalleDB(config);
        }

        public ErrorDto<CrConsultaDetalleCompletoDto> CR_ConsultaDetalle_Obtener(
            int CodEmpresa,
            int operacion,
            string? tipoActa,
            string? tipoDetalle,
            string usuario,
            int codContabilidad)
        {
            return _db.CR_ConsultaDetalle_Obtener(
                CodEmpresa,
                operacion,
                tipoActa,
                tipoDetalle,
                usuario,
                codContabilidad);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleMovimientoDto>> CR_ConsultaDetalle_Movimientos_Lista_Obtener(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Movimientos_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleMovimientoDto>> CR_ConsultaDetalle_Movimientos_Lista_Export(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Movimientos_Lista_Export(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleMorosidadDto>> CR_ConsultaDetalle_Morosidad_Lista_Obtener(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Morosidad_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleMorosidadDto>> CR_ConsultaDetalle_Morosidad_Lista_Export(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Morosidad_Lista_Export(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleCierreDto>> CR_ConsultaDetalle_Cierre_Lista_Obtener(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Cierre_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleCierreDto>> CR_ConsultaDetalle_Cierre_Lista_Export(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Cierre_Lista_Export(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleCorreccionDto>> CR_ConsultaDetalle_Correcciones_Lista_Obtener(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Correcciones_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleCorreccionDto>> CR_ConsultaDetalle_Correcciones_Lista_Export(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Correcciones_Lista_Export(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleFiadorDto>> CR_ConsultaDetalle_Fiadores_Lista_Obtener(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Fiadores_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleFiadorDto>> CR_ConsultaDetalle_Fiadores_Lista_Export(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Fiadores_Lista_Export(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleRefundicionDto>> CR_ConsultaDetalle_Refundiciones_Lista_Obtener(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Refundiciones_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleRefundicionDto>> CR_ConsultaDetalle_Refundiciones_Lista_Export(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Refundiciones_Lista_Export(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleDesembolsoDto>> CR_ConsultaDetalle_Desembolsos_Lista_Obtener(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Desembolsos_Lista_Obtener(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleDesembolsoDto>> CR_ConsultaDetalle_Desembolsos_Lista_Export(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Desembolsos_Lista_Export(CodEmpresa, operacion, parametros);
        }

        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleTagDto>> CR_ConsultaDetalle_Tags_Lista_Obtener(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Tags_Lista_Obtener(CodEmpresa, operacion, parametros);
        }
        public ErrorDto<CrConsultaDetalleListaResult<CrConsultaDetalleTagDto>> CR_ConsultaDetalle_Tags_Lista_Export(int CodEmpresa, int operacion, string parametros)
        {
            return _db.CR_ConsultaDetalle_Tags_Lista_Export(CodEmpresa, operacion, parametros);
        }
    }
}