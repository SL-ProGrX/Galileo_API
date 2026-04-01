using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdParametrosBL
    {
        private readonly FrmAfCdParametrosDB _db;

        public FrmAfCdParametrosBL(IConfiguration config)
        {
            _db = new FrmAfCdParametrosDB(config);
        }

        public ErrorDto<List<AfCdParametroDto>> AfCdParametros_Lista(int codEmpresa)
            => _db.AfCdParametros_Lista(codEmpresa);

        public ErrorDto<bool> AfCdParametros_Update(int codEmpresa, AfCdParametroUpdateDto dto)
            => _db.AfCdParametros_Update(codEmpresa, dto);
    }
}
