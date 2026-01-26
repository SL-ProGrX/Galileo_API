using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using PgxAPI.DataBaseTier;

namespace PgxAPI.BusinessLogic
{
    public class FrmTesImpresorasBL
    {
        private readonly FrmTesImpresorasDb ImpresorasDb;

        public FrmTesImpresorasBL(IConfiguration config)
        {
            ImpresorasDb = new FrmTesImpresorasDb(config);
        }

        public ErrorDto Tes_Impresoras_Guardar(int CodEmpresa, string usuario, TesImpresorasDto impresora)
        {
            return ImpresorasDb.Tes_Impresoras_Guardar(CodEmpresa, usuario, impresora);
        }

        public ErrorDto<TesImpresorasDto> Tes_Impresoras_Obtener(int CodEmpresa)
        {
            return ImpresorasDb.Tes_Impresoras_Obtener(CodEmpresa);
        }


    }
}