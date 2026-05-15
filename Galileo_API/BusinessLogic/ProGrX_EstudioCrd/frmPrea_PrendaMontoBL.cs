using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaPrendaMontoBL
    {
        private readonly FrmPreaPrendaMontoDB _db;
        public FrmPreaPrendaMontoBL(IConfiguration config)
        {
            _db = new FrmPreaPrendaMontoDB(config);
        }

        public ErrorDto<List<PrendaGastoDto>> CrdPrea_Prendas_Gastos(int codEmpresa, string preanalisis, string tipo)
            => _db.CrdPrea_Prendas_Gastos(codEmpresa, preanalisis, tipo);

        public ErrorDto<PreaAsignaHonorariosPrenResultDto> CrdPrea_AsignaHonorariosPren(int codEmpresa, PreaAsignaHonorariosPrenRequestDto request)
            => _db.CrdPrea_AsignaHonorariosPren(codEmpresa, request);
    }
}
