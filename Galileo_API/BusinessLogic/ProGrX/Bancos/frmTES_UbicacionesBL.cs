using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesUbicacionesBL
    {
        private readonly FrmTesUbicacionesDB _db;
        public FrmTesUbicacionesBL(IConfiguration config)
        {
            _db = new FrmTesUbicacionesDB(config);
        }
        public ErrorDto<TesUbicacionesLista> Tes_UbicacionesLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Tes_UbicacionesLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto Tes_Ubicaciones_Guardar(int CodEmpresa, string usuario, TesUbicacionesData ubicacion)
        {
            return _db.Tes_Ubicaciones_Guardar(CodEmpresa, usuario, ubicacion);
        }
        public ErrorDto Tes_Ubicaciones_Eliminar(int CodEmpresa, string tipo, string usuario)
        {
            return _db.Tes_Ubicaciones_Eliminar(CodEmpresa, tipo, usuario);
        }
        public ErrorDto<List<TesUbicacionesData>> Tes_Ubicaciones_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Tes_Ubicaciones_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_UbicacionesUsuarios_Obtener(int CodEmpresa)
        {
            return _db.Tes_UbicacionesUsuarios_Obtener(CodEmpresa);
        }

        public ErrorDto Tes_Ubicaciones_Valida(int CodEmpresa, string cod_ubicacion)
        {
            return _db.Tes_Ubicaciones_Valida(CodEmpresa, cod_ubicacion);
        }
    }
}
