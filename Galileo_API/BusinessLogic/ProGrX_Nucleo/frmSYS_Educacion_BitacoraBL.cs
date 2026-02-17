using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysEducacionBitacoraBL(IConfiguration config)
    {
        private readonly FrmSysEducacionBitacoraDB _db = new FrmSysEducacionBitacoraDB(config);

        public ErrorDto<List<SysEducacionListData>> SYS_Educacion_Combo_Obtener(int CodEmpresa, string tipo, string valor)
        {
            return _db.SYS_Educacion_Combo_Obtener(CodEmpresa, tipo, valor);
        }

        public ErrorDto<SysPadronLista> SYS_Padron_Obtener(int CodEmpresa, string jfiltro)
        {
            return _db.SYS_Padron_Obtener(CodEmpresa, jfiltro);
        }

        public ErrorDto<List<SysEducacionLogData>> SYS_Educacion_Obtener(int CodEmpresa, string jfiltro)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltro) ?? new FiltrosLazyLoadData();
            return _db.SYS_Educacion_Obtener(CodEmpresa, filtros);
        }
    }
}