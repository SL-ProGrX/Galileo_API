using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using PgxAPI.DataBaseTier;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasAccesosBl
    {
        private readonly FrmCajasAccesosDb DbfrmCajas_Accesos;

        public FrmCajasAccesosBl(IConfiguration config)
        {
            DbfrmCajas_Accesos = new FrmCajasAccesosDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Apertura_Obtener(int CodEmpresa, string usuario)
        {
            return DbfrmCajas_Accesos.Cajas_Apertura_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto Cajas_AbreCaja(int codEmpresa, string codCaja, string usuario, string appVersion, string clave)
        {
            return DbfrmCajas_Accesos.Cajas_AbreCaja(codEmpresa, codCaja, usuario, appVersion, clave);
        }
    }
}