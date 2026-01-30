using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasCatGruposBL
    {
        private readonly FrmPolizasCatGruposDB _db;

        public FrmPolizasCatGruposBL(IConfiguration config)
        {
            _db = new FrmPolizasCatGruposDB(config);
        }

        public ErrorDto<List<PolizaGrupoDto>> PolizaGrupos_Lista(int codEmpresa)
            => _db.PolizaGrupos_Lista(codEmpresa);

        public ErrorDto<PolizaGrupoExisteResult?> PolizaGrupos_Existe(int codEmpresa, int id)
            => _db.PolizaGrupos_Existe(codEmpresa, id);

        public ErrorDto<bool> PolizaGrupos_Guardar(int codEmpresa, PolizaGrupoSaveParams param)
            => _db.PolizaGrupos_Guardar(codEmpresa, param);

        public ErrorDto<bool> PolizaGrupos_Eliminar(int codEmpresa, PolizaGrupoDeleteParams param)
            => _db.PolizaGrupos_Eliminar(codEmpresa, param);
    }
}
