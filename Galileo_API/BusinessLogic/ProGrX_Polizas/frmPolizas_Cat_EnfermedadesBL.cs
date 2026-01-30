using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasCatEnfermedadesBL
    {
        private readonly FrmPolizasCatEnfermedadesDB _db;

        public FrmPolizasCatEnfermedadesBL(IConfiguration config)
        {
            _db = new FrmPolizasCatEnfermedadesDB(config);
        }

        public ErrorDto<List<EnfermedadVidaDto>> Enfermedades_Lista(int codEmpresa)
            => _db.Enfermedades_Lista(codEmpresa);

        public ErrorDto<bool> Enfermedades_Guardar(int codEmpresa, EnfermedadVidaSaveParams param)
            => _db.Enfermedades_Guardar(codEmpresa, param);

        public ErrorDto<bool> Enfermedades_Eliminar(int codEmpresa, EnfermedadVidaDeleteParams param)
            => _db.Enfermedades_Eliminar(codEmpresa, param);
    }
}
