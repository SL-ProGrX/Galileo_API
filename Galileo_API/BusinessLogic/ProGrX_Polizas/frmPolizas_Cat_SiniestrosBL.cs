using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasCatSiniestrosBL
    {
        private readonly FrmPolizasCatSiniestrosDB _db;

        public FrmPolizasCatSiniestrosBL(IConfiguration config)
        {
            _db = new FrmPolizasCatSiniestrosDB(config);
        }

        public ErrorDto<List<SiniestroTipoDto>> Siniestros_Lista(int codEmpresa)
            => _db.Siniestros_Lista(codEmpresa);

        public ErrorDto<SiniestroTipoExisteResult?> Siniestros_Existe(int codEmpresa, int id)
            => _db.Siniestros_Existe(codEmpresa, id);

        public ErrorDto<bool> Siniestros_Guardar(int codEmpresa, SiniestroTipoSaveParams param)
            => _db.Siniestros_Guardar(codEmpresa, param);

        public ErrorDto<bool> Siniestros_Eliminar(int codEmpresa, SiniestroTipoDeleteParams param)
            => _db.Siniestros_Eliminar(codEmpresa, param);
    }
}
