using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFTiposSociedadesBL
    {
        private readonly FrmAFTiposSociedadesDB _db;

        public FrmAFTiposSociedadesBL(IConfiguration config)
        {
            _db = new FrmAFTiposSociedadesDB(config);
        }

        public ErrorDto<AfTiposSociedadesLista> AF_TiposSociedades_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_TiposSociedades_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_TiposSociedades_Guardar(int CodEmpresa, string Usuario, AfTiposSociedadesDto Info)
        {
            return _db.AF_TiposSociedades_Guardar(CodEmpresa, Usuario, Info);
        }

        public ErrorDto AF_TiposSociedades_Eliminar(int CodEmpresa, string Usuario, string CodSociedad)
        {
            return _db.AF_TiposSociedades_Eliminar(CodEmpresa, Usuario, CodSociedad);
        }
    }
}