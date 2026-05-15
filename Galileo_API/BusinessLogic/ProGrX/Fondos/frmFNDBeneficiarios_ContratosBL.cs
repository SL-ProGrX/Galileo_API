using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndBeneficiariosContratosBL
    {
        private readonly FrmFndBeneficiariosContratosDB _BD;

        public FrmFndBeneficiariosContratosBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BD = new FrmFndBeneficiariosContratosDB(config);
        }

        public ErrorDto<List<FndBeneficiariosContratosData>> FND_Beneficiarios_Contratos_Lista_Obtener(int CodEmpresa, string cedula, int operadora, string plan, long contrato)
        {
            return _BD.FND_Beneficiarios_Contratos_Lista_Obtener(CodEmpresa, cedula, operadora, plan, contrato);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_Beneficiarios_Contratos_Parentescos_Obtener(int CodEmpresa)
        {
            return _BD.FND_Beneficiarios_Contratos_Parentescos_Obtener(CodEmpresa);
        }

        public ErrorDto FNDBeneficiarios_Contratos_Borrar(int CodEmpresa, int consec, string usuario)
        {
            return _BD.FNDBeneficiarios_Contratos_Borrar(CodEmpresa, consec, usuario);
        }

        public ErrorDto FND_Beneficiarios_Contratos_Guardar(int CodEmpresa, string usuario, FndBeneficiariosContratosData data)
        {
            return _BD.FND_Beneficiarios_Contratos_Guardar(CodEmpresa, usuario, data);
        }

        public ErrorDto<string> FNDBene_Cnt_CedulaBN_Obtener(int CodEmpresa, string cedula, string plan, long contrato, int operadora)
        {
            return _BD.FNDBene_Cnt_CedulaBN_Obtener(CodEmpresa, cedula, plan, contrato, operadora);
        }
    }
}