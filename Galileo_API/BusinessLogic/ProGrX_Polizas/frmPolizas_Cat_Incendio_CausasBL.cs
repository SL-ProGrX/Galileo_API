using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasCatIncendioCausasBL
    {
        private readonly FrmPolizasCatIncendioCausasDB _db;

        public FrmPolizasCatIncendioCausasBL(IConfiguration config)
        {
            _db = new FrmPolizasCatIncendioCausasDB(config);
        }

        public ErrorDto<List<IncendioCausaDto>> IncendioCausas_Lista(int codEmpresa)
            => _db.IncendioCausas_Lista(codEmpresa);

        public ErrorDto<bool> IncendioCausas_Insertar(int codEmpresa, IncendioCausaSaveParams param)
            => _db.IncendioCausas_Insertar(codEmpresa, param);

        public ErrorDto<bool> IncendioCausas_Actualizar(int codEmpresa, IncendioCausaUpdateParams param)
            => _db.IncendioCausas_Actualizar(codEmpresa, param);

        public ErrorDto<bool> IncendioCausas_Eliminar(int codEmpresa, IncendioCausaDeleteParams param)
            => _db.IncendioCausas_Eliminar(codEmpresa, param);
    }
}
