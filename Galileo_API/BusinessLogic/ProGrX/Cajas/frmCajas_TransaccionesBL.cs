using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasTransaccionesBL
    {
        private readonly FrmCajasTransaccionesDb _db;

        public FrmCajasTransaccionesBL(IConfiguration config)
        {
            _db = new FrmCajasTransaccionesDb(config);
        }

        public ErrorDto<List<CajasSocioResult>> CajasTransacciones_Socios_Obtener(int codEmpresa)
        {
            return _db.CajasTransacciones_Socios_Obtener(codEmpresa);
        }

        public ErrorDto<List<CajasServicioResult>> CajasTransacciones_Servicios_Obtener(int codEmpresa, CajasServicioConsultaParams param)
        {
            return _db.CajasTransacciones_Servicios_Obtener(codEmpresa, param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasTransacciones_Divisas_Obtener(int codEmpresa, string codContabilidad)
        {
            return _db.CajasTransacciones_Divisas_Obtener(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasTransacciones_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            return _db.CajasTransacciones_Documentos_Obtener(codEmpresa, codCaja);
        }

        public ErrorDto<CajasTransacValidacionResult> CajasTransacciones_Validacion(
            int codEmpresa,
            CajasTransacValidacionParams param)
        {
            return _db.CajasTransacciones_Validacion(codEmpresa, param);
        }

        public ErrorDto<CajasServiciosDatosResult> CajasTransacciones_ServiciosDatos(int codEmpresa, CajasServiciosDatosParams param)
        {
            return _db.CajasTransacciones_ServiciosDatos(codEmpresa, param);
        }

        public ErrorDto<bool> SifTransacciones_Insertar(int codEmpresa, SifTransaccionInsertParams param)
        {
            return _db.SifTransacciones_Insertar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasServiciosTransac_Insertar(int codEmpresa, CajasServiciosTransacInsertParams param)
        {
            return _db.CajasServiciosTransac_Insertar(codEmpresa, param);
        }

        public ErrorDto<SifDocsAsientoResult> SifDocsAsiento_Ejecutar(int codEmpresa, SifDocsAsientoParams param)
        {
            return _db.SifDocsAsiento_Ejecutar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasDesglocePagosDocFinal_Ejecutar(int codEmpresa, CajasDesglocePagosDocFinalParams param)
        {
            return _db.CajasDesglocePagosDocFinal_Ejecutar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasIntercambioRegistra(int codEmpresa, CajasIntercambioRegistraParams param)
        {
            return _db.CajasIntercambioRegistra(codEmpresa, param);
        }

        public ErrorDto<bool> CajasValoresTransitoRegistra(int codEmpresa, CajasValoresTransitoRegistraParams param)
        {
            return _db.CajasValoresTransitoRegistra(codEmpresa, param);
        }

        public ErrorDto<bool> CajasGeneralTE_Ejecutar(int codEmpresa, CajasGeneralTEParams param)
        {
            return _db.CajasGeneralTE_Ejecutar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasReciboDigital(int codEmpresa, CajasReciboDigitalParams param)
        {
            return _db.CajasReciboDigital(codEmpresa, param);
        }
    }
}
