using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXAsientosBl
    {
        private readonly FrmCntXAsientosDb _db;

        public FrmCntXAsientosBl(IConfiguration config) 
            => _db = new FrmCntXAsientosDb(config);

        public ErrorDto<CntXAsientoData?> CntXAsientos_Obtener(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            return _db.CntXAsientos_Obtener(codEmpresa, codConta, tipoAsiento, numAsiento);
        }

        public ErrorDto<List<CntXAsientoDetalleData>> CntXAsientos_Detalle_Obtener(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            return _db.CntXAsientos_Detalle_Obtener(codEmpresa, codConta, tipoAsiento, numAsiento);
        }

        public ErrorDto<CntXAsientoData?> CntXAsientos_Scroll_Obtener(int codEmpresa, string request, int scrollCode)
        {
            CntXAsientoData Jfiltros = JsonConvert.DeserializeObject<CntXAsientoData>(request) ?? new CntXAsientoData();
            return _db.CntXAsientos_Scroll_Obtener(codEmpresa, Jfiltros, scrollCode);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXAsientos_Lista_Obtener(int codEmpresa, int codConta, string tipoAsiento, bool periodoActual, int anio, int mes)
        {
            return _db.CntXAsientos_Lista_Obtener(codEmpresa, codConta, tipoAsiento, periodoActual, anio, mes);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXTiposAsientos_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXTiposAsientos_Lista_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<string?> CntXTiposAsientos_Descripcion_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            return _db.CntXTiposAsientos_Descripcion_Obtener(codEmpresa, codConta, tipoAsiento);    
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCentroCostosporUnidad_Lista_Obtener(int codEmpresa, int codConta, string codUnidad)
        {
            return _db.CntXCentroCostosporUnidad_Lista_Obtener(codEmpresa, codConta, codUnidad);
        }

        public ErrorDto<string?> CntXAsientos_Consecutivo_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            return _db.CntXAsientos_Consecutivo_Obtener(codEmpresa, codConta, tipoAsiento);
        }

        public ErrorDto CntXAsientos_Guardar(int codEmpresa, string usuario, bool edita, CntXAsientoGuardarRequest request)
        {
            return _db.CntXAsientos_Guardar(codEmpresa, usuario, edita, request);
        }

        public ErrorDto CntXAsientos_Eliminar(int codEmpresa, int codConta, string tipoAsiento, string numAsiento, string ts, string usuario)
        {
            byte[] tsBytes = Array.Empty<byte>();

            if (!string.IsNullOrWhiteSpace(ts))
            {
                tsBytes = Convert.FromBase64String(ts);
            }
            return _db.CntXAsientos_Eliminar(codEmpresa, codConta, tipoAsiento, numAsiento, tsBytes, usuario);
        }

        public ErrorDto CntXAsientos_Autorizar(int codEmpresa, int codConta, string tipoAsiento, string numAsiento, string usuario)
        {
            return _db.CntXAsientos_Autorizar(codEmpresa, codConta, tipoAsiento, numAsiento, usuario);
        }

        public ErrorDto CntXAsientos_Copiar(int codEmpresa, string usuario, CntXAsientoCopiarRequest request)
        {
            return _db.CntXAsientos_Copiar(codEmpresa, usuario, request);
        }

        public ErrorDto CntXAsientos_Reversar(int codEmpresa, CntXAsientoData request)
        {
            return _db.CntXAsientos_Reversar(codEmpresa, request);
        }

        public ErrorDto CntXAsientos_Mayorizar(int codEmpresa, CntXAsientoData request)
        {
            return _db.CntXAsientos_Mayorizar(codEmpresa, request);
        }

        public ErrorDto CntXAsientos_FxNotaCuenta_Obtener(int codEmpresa, int codConta, string vCuenta, int anio, int mes)
        {
            return _db.CntXAsientos_FxNotaCuenta_Obtener(codEmpresa, codConta, vCuenta, anio, mes);
        }
    }
}
