using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaAutorizacionesBL
    {
        private readonly FrmPreaAutorizacionesDB _db;
        public FrmPreaAutorizacionesBL(IConfiguration config)
        {
            _db = new FrmPreaAutorizacionesDB(config);
        }

        public ErrorDto<PreaComiteIdDto> PreaAutorizaciones_ObtenerComite(int codEmpresa, string expediente)
            => _db.PreaAutorizaciones_ObtenerComite(codEmpresa, expediente);

        public ErrorDto<List<PreaComiteMiembroDto>> PreaAutorizaciones_ObtenerMiembros(int codEmpresa, int comite, string expediente)
            => _db.PreaAutorizaciones_ObtenerMiembros(codEmpresa, comite, expediente);

        public ErrorDto<bool> PreaAutorizaciones_Insertar(int codEmpresa, PreaAutorizadorRequestDto request)
            => _db.PreaAutorizaciones_Insertar(codEmpresa, request);

        public ErrorDto<bool> PreaAutorizaciones_Eliminar(int codEmpresa, string expediente, string cedula)
            => _db.PreaAutorizaciones_Eliminar(codEmpresa, expediente, cedula);
    }
}
