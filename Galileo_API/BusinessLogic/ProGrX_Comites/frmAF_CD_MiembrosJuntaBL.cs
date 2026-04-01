using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdMiembrosJuntaBL
    {
        private readonly FrmAfCdMiembrosJuntaDB _db;

        public FrmAfCdMiembrosJuntaBL(IConfiguration config)
        {
            _db = new FrmAfCdMiembrosJuntaDB(config);
        }

        public ErrorDto<List<AfCdDirectorDto>> AfCdDirectores_Lista(int codEmpresa)
            => _db.AfCdDirectores_Lista(codEmpresa);

        public ErrorDto<List<AfCdComiteDirectorDto>> AfCdDirectores_ValidarComite(int codEmpresa, int codDirector)
            => _db.AfCdDirectores_ValidarComite(codEmpresa, codDirector);

        public ErrorDto<bool> AfCdDirectores_Guardar(int codEmpresa, AfCdDirectorSaveDto dto)
            => _db.AfCdDirectores_Guardar(codEmpresa, dto);

        public ErrorDto<bool> AfCdDirectores_Eliminar(int codEmpresa, int codDirector, string usuario)
            => _db.AfCdDirectores_Eliminar(codEmpresa, codDirector, usuario);
    }
}
