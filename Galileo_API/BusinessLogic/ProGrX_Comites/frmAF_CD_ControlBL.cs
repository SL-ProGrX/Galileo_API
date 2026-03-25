using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_CxC;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_CxC
{
    public class FrmAF_CD_ControlBL
    {
        private readonly FrmAF_CD_ControlDB _db;

        public FrmAF_CD_ControlBL(IConfiguration config)
        {
            _db = new FrmAF_CD_ControlDB(config);
        }

        public ErrorDto<List<AFCDCuentaDto>> Listar(int codEmpresa, AFCDCuentaFiltroDto filtro)
        {
            return _db.Listar(codEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tipos(int codEmpresa)
        {
            return _db.Tipos(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Procesos(int codEmpresa)
        {
            return _db.Procesos(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Estados(int codEmpresa)
        {
            return _db.Estados(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Comites(int codEmpresa)
        {
            return _db.Comites(codEmpresa);
        }
    }
}