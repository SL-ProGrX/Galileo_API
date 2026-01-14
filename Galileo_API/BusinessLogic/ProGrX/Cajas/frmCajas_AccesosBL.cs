using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

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

        public ErrorDto<CajasAperturaDto> Cajas_AbreCaja(int codEmpresa, string codCaja, string usuario, string appVersion, string clave)
        {
            return DbfrmCajas_Accesos.Cajas_AbreCaja(codEmpresa, codCaja, usuario, appVersion, clave);
        }
    }
}