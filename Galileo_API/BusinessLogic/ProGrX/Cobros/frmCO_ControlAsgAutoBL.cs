using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoControlAsgAutoBL
    {
        private readonly FrmCoControlAsgAutoDB _db;

        public FrmCoControlAsgAutoBL(IConfiguration config)
        {
            _db = new FrmCoControlAsgAutoDB(config);
        }

        public ErrorDto<List<CbrUsuarioResult>> CbrUsuarios_Activos_Lista(int codEmpresa)
        {
            return _db.CbrUsuarios_Activos_Lista(codEmpresa);
        }

        public ErrorDto<List<CbrUsuarioGrupoListResult>> CbrUsuarios_Grupos_List(int codEmpresa, CbrUsuarioGrupoListParams param)
        {
            return _db.CbrUsuarios_Grupos_List(codEmpresa, param);
        }

        public ErrorDto<CbrControlDistribucionResult?> CbrControlDistribucion(int codEmpresa, CbrControlDistribucionParams param)
        {
            return _db.CbrControlDistribucion(codEmpresa, param);
        }
    }
}
