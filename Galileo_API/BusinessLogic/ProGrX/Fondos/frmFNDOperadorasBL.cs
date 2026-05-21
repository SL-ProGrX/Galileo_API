using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndOperadorasBL
    {
        private readonly FrmFndOperadorasDB _Db;

        public FrmFndOperadorasBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndOperadorasDB(config);
        }

        public ErrorDto<FndOperadoraDto> AF_Operadora_Obtener(int CodEmpresa, int cod_operadora)
        {

            return _Db.AF_Operadora_Obtener(CodEmpresa, cod_operadora);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Operadoras_Obtener(int CodEmpresa)
        {
            return _Db.AF_Operadoras_Obtener(CodEmpresa);
        }

        public ErrorDto<FndOperadoraDto> AF_Operadora_Guardar(int codEmpresa, FndOperadoraDto request)
        {
            return _Db.AF_Operadora_Guardar(codEmpresa, request);
        }

        public ErrorDto<List<OperadoraPlanDto>> FND_OperadoraPlanes_Obtener(int CodEmpresa, int cod_operadora)
        {
            return _Db.FND_OperadoraPlanes_Obtener(CodEmpresa, cod_operadora);
        }

        public ErrorDto AF_Operadora_Eliminar(int codEmpresa, int cod_operadora)
        {
            return _Db.AF_Operadora_Eliminar(codEmpresa, cod_operadora);
        }

        public ErrorDto<FndOperadoraDto> AF_Operadora_Scroll_Obtener(int CodEmpresa, int cod_operadora, int scrollCode)
        {
            return _Db.AF_Operadora_Scroll_Obtener(CodEmpresa, cod_operadora,scrollCode);
        }

    }
}
