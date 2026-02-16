using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.BusinessLogic
{
    public class FrmSifParametrosBL(IConfiguration config)
    {
        private readonly FrmSifParametrosDB _db = new FrmSifParametrosDB(config);

        public ErrorDto<List<SifParametrosDto>> obtener_ParametrosSistema(int CodEmpresa)
        {
            return _db.obtener_ParametrosSistema(CodEmpresa);
        }

        public ErrorDto Parametros_Actualizar(int CodEmpresa, string usuario, SifParametrosDto parametros)
        {
            return _db.Parametros_Actualizar(CodEmpresa, usuario, parametros);
        }

    }
}