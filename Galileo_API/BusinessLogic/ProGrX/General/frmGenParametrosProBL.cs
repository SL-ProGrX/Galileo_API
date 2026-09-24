using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmGenParametrosProBL
    {
        readonly FrmGenParametrosProDb _db;

        public FrmGenParametrosProBL(IConfiguration config)
        {
            _db = new FrmGenParametrosProDb(config);
        }

        public ErrorDto Gen_ParametrosPro_Inicializar(int CodEmpresa)
            => _db.Gen_ParametrosPro_Inicializar(CodEmpresa);

        public ErrorDto<GenParametrosProData?> Gen_ParametrosPro_Obtener(int CodEmpresa)
            => _db.Gen_ParametrosPro_Obtener(CodEmpresa);

        public ErrorDto Gen_ParametrosProGeneral_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
            => _db.Gen_ParametrosProGeneral_Actualizar(CodEmpresa, usuario, parametros);

        public ErrorDto Gen_ParametrosProCxP_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
            => _db.Gen_ParametrosProCxP_Actualizar(CodEmpresa, usuario, parametros);

        public ErrorDto Gen_ParametrosProInv_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
            => _db.Gen_ParametrosProInv_Actualizar(CodEmpresa, usuario, parametros);

        public ErrorDto Gen_ParametrosProPos_Actualizar(int CodEmpresa, string usuario, GenParametrosProData parametros)
            => _db.Gen_ParametrosProPos_Actualizar(CodEmpresa, usuario, parametros);
    }
}
