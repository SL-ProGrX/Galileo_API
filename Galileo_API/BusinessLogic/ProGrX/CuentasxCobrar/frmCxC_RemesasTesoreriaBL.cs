using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCRemesasTesoreriaBL
    {
        private readonly FrmCxCRemesasTesoreriaDB _db;

        public FrmCxCRemesasTesoreriaBL(IConfiguration config)
        {
            _db = new FrmCxCRemesasTesoreriaDB(config);
        }

        #region Remesas

        public ErrorDto<CxCRemesasTesoreriaRemesaLista> CxC_RemesasTesoreria_Remesas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CxC_RemesasTesoreria_Remesas_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CxCRemesasTesoreriaRemesaLista> CxC_RemesasTesoreria_Remesas_Lista_Export(int CodEmpresa, string parametros)
        {
            return _db.CxC_RemesasTesoreria_Remesas_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CxCRemesasTesoreriaRemesaData> CxC_RemesasTesoreria_Remesa_Obtener(int CodEmpresa, int tesoreriaRemesa)
        {
            return _db.CxC_RemesasTesoreria_Remesa_Obtener(CodEmpresa, tesoreriaRemesa);
        }

        public ErrorDto<CxCRemesasTesoreriaRemesaData> CxC_RemesasTesoreria_Remesa_Guardar(int CodEmpresa, CxCRemesasTesoreriaRemesaGuardarRequest request)
        {
            return _db.CxC_RemesasTesoreria_Remesa_Guardar(CodEmpresa, request);
        }

        public ErrorDto CxC_RemesasTesoreria_Remesa_Eliminar(int CodEmpresa, int tesoreriaRemesa, string usuario)
        {
            return _db.CxC_RemesasTesoreria_Remesa_Eliminar(CodEmpresa, tesoreriaRemesa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Remesas_Dropdown_Obtener(int CodEmpresa, string? estado)
        {
            return _db.CxC_RemesasTesoreria_Remesas_Dropdown_Obtener(CodEmpresa, estado);
        }

        #endregion

        #region Carga

        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Oficinas_Carga_Dropdown_Obtener(int CodEmpresa, int tesoreriaRemesa)
        {
            return _db.CxC_RemesasTesoreria_Oficinas_Carga_Dropdown_Obtener(CodEmpresa, tesoreriaRemesa);
        }

        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Carga_Lista_Obtener(int CodEmpresa, int tesoreriaRemesa, string? codOficina, string parametros)
        {
            return _db.CxC_RemesasTesoreria_Carga_Lista_Obtener(CodEmpresa, tesoreriaRemesa, codOficina, parametros);
        }

        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Carga_Lista_Export(int CodEmpresa, int tesoreriaRemesa, string? codOficina, string parametros)
        {
            return _db.CxC_RemesasTesoreria_Carga_Lista_Export(CodEmpresa, tesoreriaRemesa, codOficina, parametros);
        }

        public ErrorDto CxC_RemesasTesoreria_Carga_Aplicar(int CodEmpresa, CxCRemesasTesoreriaCargaAplicarRequest request)
        {
            return _db.CxC_RemesasTesoreria_Carga_Aplicar(CodEmpresa, request);
        }

        public ErrorDto CxC_RemesasTesoreria_Carga_Cerrar(int CodEmpresa, CxCRemesasTesoreriaCerrarRequest request)
        {
            return _db.CxC_RemesasTesoreria_Carga_Cerrar(CodEmpresa, request);
        }

        #endregion

        #region Traslado
        #region Traslado

        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Traslado_Lista_Obtener(int CodEmpresa,int tesoreriaRemesa,string parametros)
        {
            return _db.CxC_RemesasTesoreria_Traslado_Lista_Obtener(CodEmpresa,tesoreriaRemesa,parametros);
        }

        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Traslado_Lista_Export(int CodEmpresa,int tesoreriaRemesa,string parametros)
        {
            return _db.CxC_RemesasTesoreria_Traslado_Lista_Export(CodEmpresa,tesoreriaRemesa,parametros);
        }

        public ErrorDto CxC_RemesasTesoreria_Traslado_Aplicar(int CodEmpresa,CxCRemesasTesoreriaTrasladoAplicarRequest request)
        {
            return _db.CxC_RemesasTesoreria_Traslado_Aplicar(CodEmpresa,request);
        }

        #endregion
        #endregion

        #region Reportes
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Oficinas_Reporte_Dropdown_Obtener(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, bool todasFechas)
        {
            return _db.CxC_RemesasTesoreria_Oficinas_Reporte_Dropdown_Obtener(CodEmpresa, fechaInicio, fechaCorte, todasFechas);
        }

        #endregion

        #region Reactivacion
        public ErrorDto<CxCRemesasTesoreriaReactivacionDto> CxC_RemesasTesoreria_Reactivacion_Operacion_Obtener(int CodEmpresa, int operacion)
        {
            return _db.CxC_RemesasTesoreria_Reactivacion_Operacion_Obtener(CodEmpresa, operacion);
        }
        public ErrorDto CxC_RemesasTesoreria_Reactivacion_Aplicar(int CodEmpresa, CxCRemesasTesoreriaReactivacionAplicarRequest request)
        {
            return _db.CxC_RemesasTesoreria_Reactivacion_Aplicar(CodEmpresa, request);
        }
        #endregion
    }
}