using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_CxC;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_CxC
{
    public class FrmAfCdControlBl
    {
        private readonly FrmAfCdControlDb _db;

        public FrmAfCdControlBl(IConfiguration config)
        {
            _db = new FrmAfCdControlDb(config);
        }

        public ErrorDto<List<AfcdCuentaDto>> Listar(int codEmpresa, AfcdCuentaFiltroDto filtro)
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