using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndSubCuentasBL
    {
        private readonly FrmFndSubCuentasDB _BD;

        public FrmFndSubCuentasBL(IConfiguration? config)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));
            _BD = new FrmFndSubCuentasDB(config);
        }

        public ErrorDto<List<FndSubCuentasData>> FND_SubCuentas_Lista_Obtener(int CodEmpresa, int operadora, string plan, long contrato)
        {
            return _BD.FND_SubCuentas_Lista_Obtener(CodEmpresa, operadora, plan, contrato);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_SubCuentas_Parentescos_Obtener(int CodEmpresa)
        {
            return _BD.FND_SubCuentas_Parentescos_Obtener(CodEmpresa);
        }

        public ErrorDto FND_SubCuentas_Guardar(int CodEmpresa, string usuario, FndSubCuentasData data)
        {
            return _BD.FND_SubCuentas_Guardar(CodEmpresa, usuario, data);
        }

        public ErrorDto FNDSubCuentas_Borrar(int CodEmpresa, int consec, string usuario)
        {
            return _BD.FNDSubCuentas_Borrar(CodEmpresa, consec, usuario);
        }

        public ErrorDto<string> FNDDSubCuentas_Cedula_Obtener(int CodEmpresa, string plan, long contrato, int operadora)
        {
            return _BD.FNDDSubCuentas_Cedula_Obtener(CodEmpresa, plan, contrato, operadora);
        }
    }
}