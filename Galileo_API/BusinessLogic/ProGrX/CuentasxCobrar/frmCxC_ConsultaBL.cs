using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Newtonsoft.Json;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCConsultaBL
    {

        private readonly FrmCxCConsultaDB _db;

        public FrmCxCConsultaBL(IConfiguration config) => _db = new FrmCxCConsultaDB(config);


        public ErrorDto CxCClientesClasifica_Guardar(int CodEmpresa, string usuario, CxCConsultaData datos)
        {
            return _db.CxCClientesClasifica_Guardar(CodEmpresa, usuario, datos);
        }


    }
}
