using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfPadronSalariosBL
    {

        private readonly FrmAfPadronSalariosDB _db;

        public FrmAfPadronSalariosBL(IConfiguration config)
        {
            _db = new FrmAfPadronSalariosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronSalariosInstituciones_Obtener(int CodEmpresa)
        {
            return _db.AF_PadronSalariosInstituciones_Obtener(CodEmpresa);
        }

        public ErrorDto AF_PadronSalarios_Padron_Procesar(int CodEmpresa, string institucion, string usuario, List<AfPadronData> padron)
        {
            return _db.AF_PadronSalarios_Padron_Procesar(CodEmpresa, institucion, usuario, padron);
        }

        public ErrorDto AF_PadronSalarios_Salario_Procesar(int CodEmpresa, string institucion, string usuario, List<AfSalarioData> salario)
        {
            return _db.AF_PadronSalarios_Salario_Procesar(CodEmpresa, institucion, usuario, salario);
        }
    }
}