using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcCaEntidadesBL
    {
        private readonly FrmCcCaEntidadesDB _db;

        public FrmCcCaEntidadesBL(IConfiguration config)
        {
            _db = new FrmCcCaEntidadesDB(config); 
        }

        public ErrorDto<CaEntidadLista> CC_CA_Entidades_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CC_CA_Entidades_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto frmCC_CA_Entidad_Guardar(int CodEmpresa, string usuario, CaEntidadData request)
        {
            return _db.frmCC_CA_Entidad_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto CC_CA_Entidad_Delete(int CodEmpresa, string Usuario, string Codigo)
        {
            return _db.CC_CA_Entidad_Delete(CodEmpresa, Usuario, Codigo);
        }
    }
}