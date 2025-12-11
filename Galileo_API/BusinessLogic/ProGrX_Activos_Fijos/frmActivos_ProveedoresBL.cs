using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosProveedoresBL
    {
        private readonly FrmActivosProveedoresDb _db;

        public FrmActivosProveedoresBL(IConfiguration config)
        {
            _db = new FrmActivosProveedoresDb(config);
        }
        public ErrorDto<ActivosProveedoresLista> Activos_ProveedoresLista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.Activos_ProveedoresLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<List<ActivosProveedoresData>> Activos_Proveedores_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.Activos_Proveedores_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto Activos_Proveedores_Guardar(int CodEmpresa, string usuario, ActivosProveedoresData proveedor)
        {
            return _db.Activos_Proveedores_Guardar(CodEmpresa, usuario, proveedor);
        }
        public ErrorDto Activos_Proveedores_Eliminar(int CodEmpresa, string usuario, string cod_proveedor)
        {
            return _db.Activos_Proveedores_Eliminar(CodEmpresa, usuario, cod_proveedor);
        }
        public ErrorDto Activos_Proveedores_Importar(int CodEmpresa, string usuario)
        {
            return _db.Activos_Proveedores_Importar(CodEmpresa, usuario);
        }
        public ErrorDto Activos_Proveedores_Valida(int CodEmpresa, string cod_proveedor)
        {
            return _db.Activos_Proveedores_Valida(CodEmpresa, cod_proveedor);
        }
    }
}
