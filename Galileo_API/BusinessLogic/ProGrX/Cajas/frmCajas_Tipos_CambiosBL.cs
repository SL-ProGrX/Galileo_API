using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasTiposCambiosBL
    {
        private readonly FrmCajasTiposCambiosDB _db;

        public FrmCajasTiposCambiosBL(IConfiguration config)
        {
            _db = new FrmCajasTiposCambiosDB(config);
        }

        public ErrorDto<List<CajasTiposCambiosData>> Cajas_TiposCambios_Obtener(int CodEmpresa,int codContabilidad,string cod_divisa,string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Cajas_TiposCambios_Obtener(CodEmpresa, codContabilidad, cod_divisa, filtros);
        }

        public ErrorDto Cajas_TiposCambios_Guardar(int CodEmpresa,string usuario,CajasTiposCambiosData cambio)
        {
            return _db.Cajas_TiposCambios_Guardar(CodEmpresa, usuario, cambio);
        }

        public ErrorDto Cajas_TiposCambios_Eliminar(int CodEmpresa,string usuario,int codContabilidad,string cod_divisa,int id_cambio)
        {
            return _db.Cajas_TiposCambios_Eliminar(CodEmpresa, usuario, codContabilidad, cod_divisa, id_cambio);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TiposCambios_Divisas_Obtener(int CodEmpresa,int codContabilidad)
        {
            return _db.Cajas_TiposCambios_Divisas_Obtener(CodEmpresa, codContabilidad);
        }
    }
}