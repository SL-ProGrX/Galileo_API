using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifEntidadesCancelaBL(IConfiguration config)
    {
        private readonly FrmSifEntidadesCancelaDB _db = new FrmSifEntidadesCancelaDB(config);

        public ErrorDto<SifEntidadesCancelaLista> SIF_EntidadesCancelaLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SIF_EntidadesCancelaLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<SifEntidadesCancelaData>> SIF_EntidadesCancela_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SIF_EntidadesCancela_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto SIF_EntidadesCancela_Guardar(int CodEmpresa, string usuario, SifEntidadesCancelaData entidad)
        {
            return _db.SIF_EntidadesCancela_Guardar(CodEmpresa, usuario, entidad);
        }

        public ErrorDto SIF_EntidadesCancela_Eliminar(int CodEmpresa, string tipo, string usuario)
        {
            return _db.SIF_EntidadesCancela_Eliminar(CodEmpresa, tipo, usuario);
        }

        public ErrorDto SIF_EntidadesCancela_Valida(int CodEmpresa, string cod_entidad_pago)
        {
            return _db.SIF_EntidadesCancela_Valida(CodEmpresa, cod_entidad_pago);
        }
    }
}