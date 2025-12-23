using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosLocalizacionesListBl
    {
        private readonly FrmActivosLocalizacionesListDb _db;

        public FrmActivosLocalizacionesListBl(IConfiguration config)
        {
            _db = new FrmActivosLocalizacionesListDb(config);
        }

        public ErrorDto<ActivosLocalizacionesLista> Activos_LocalizacionesLista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_LocalizacionesLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<List<ActivosLocalizacionesData>> Activos_Localizaciones_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_Localizaciones_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Activos_Localizaciones_Guardar(int CodEmpresa, string usuario, ActivosLocalizacionesData localizacion)
        {
            return _db.Activos_Localizaciones_Guardar(CodEmpresa, usuario, localizacion);
        }

        public ErrorDto Activos_Localizaciones_Eliminar(int CodEmpresa, string usuario, string cod_localiza)
        {
            return _db.Activos_Localizaciones_Eliminar(CodEmpresa, usuario, cod_localiza);
        }


        public ErrorDto Activos_Localizaciones_Valida(int CodEmpresa, string cod_localiza)
        {
            return _db.Activos_Localizaciones_Valida(CodEmpresa, cod_localiza);
        }
    }
}
