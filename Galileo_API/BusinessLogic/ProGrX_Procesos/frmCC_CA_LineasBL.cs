using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcCaLineasBL
    {
        private readonly FrmCcCaLineasDB _db;

        public FrmCcCaLineasBL(IConfiguration config)
        {
            _db = new FrmCcCaLineasDB(config);
        }

        public ErrorDto<CcCaLineasLista> CC_CA_Lineas_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CC_CA_Lineas_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto CC_CA_Lineas_Guardar(int CodEmpresa, string usuario, CcCaLineasData request)
        {
            return _db.CC_CA_Lineas_Guardar(CodEmpresa, usuario, request);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CC_CA_Lineas_Cbo_Obtener(int CodEmpresa)
        {
            return _db.CC_CA_Lineas_Cbo_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CC_CA_Origenes_Cbo_Obtener(int CodEmpresa, string tipoOrigen)
        {
            return _db.CC_CA_Origenes_Cbo_Obtener(CodEmpresa, tipoOrigen);
        }
        public ErrorDto<List<CcCaCatalogoLineasData>> CC_CA_CatalogoLineas_Obtener(int CodEmpresa, string cod_Linea)
        {
            return _db.CC_CA_CatalogoLineas_Obtener(CodEmpresa, cod_Linea);
        }
        public ErrorDto<List<CcCaCatalogoLineasData>> CC_CA_CatalogoAsignaciones_Obtener(
            int CodEmpresa,
            string tipoOrigen,
            string codigoOrigen)
        {
            return _db.CC_CA_CatalogoAsignaciones_Obtener(CodEmpresa, tipoOrigen, codigoOrigen);
        }
        public ErrorDto CC_CA_CatalogoLineas_Delete(int CodEmpresa, string Usuario, string cod_Linea)
        {
            return _db.CC_CA_CatalogoLineas_Delete(CodEmpresa, Usuario, cod_Linea);
        }
        public ErrorDto CC_CA_LineasDetalle_Insertar(int CodEmpresa, string usuario, string cod_Linea, string codigo)
        {
            return _db.CC_CA_LineasDetalle_Insertar(CodEmpresa, usuario, cod_Linea, codigo);
        }
        public ErrorDto CC_CA_LineasDetalle_Delete(int CodEmpresa, string usuario, string cod_Linea, string codigo)
        {
            return _db.CC_CA_LineasDetalle_Delete(CodEmpresa, usuario, cod_Linea, codigo);
        }
        public ErrorDto CC_CA_Asignacion_Guardar(CcCaAsignacionGuardarRequest request)
        {
            return _db.CC_CA_Asignacion_Guardar(request);
        }

    }
}
