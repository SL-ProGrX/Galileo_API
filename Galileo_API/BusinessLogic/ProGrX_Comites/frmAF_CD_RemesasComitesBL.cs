using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdRemesasComitesBL
    {
        private readonly FrmAfCdRemesasComitesDB _db;

        public FrmAfCdRemesasComitesBL(IConfiguration config)
        {
            _db = new FrmAfCdRemesasComitesDB(config);
        }

        public ErrorDto<List<AfCdRemesaTesDto>> AfCdRemesasTes_Lista(int codEmpresa)
            => _db.AfCdRemesasTes_Lista(codEmpresa);

        public ErrorDto<bool> AfCdRemesasTes_Guardar(int codEmpresa, AfCdRemesaTesSaveDto dto)
            => _db.AfCdRemesasTes_Guardar(codEmpresa, dto);

        public ErrorDto<bool> AfCdRemesasTes_Eliminar(int codEmpresa, int codRemesa)
            => _db.AfCdRemesasTes_Eliminar(codEmpresa, codRemesa);
    }
}
