using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrGarantiasPatrimonialesBL
    {
        private readonly FrmCrGarantiasPatrimonialesDB DB;

        public FrmCrGarantiasPatrimonialesBL(IConfiguration config)
        {
            DB = new FrmCrGarantiasPatrimonialesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_GarantiasPatrimoniales_Garantias_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_GarantiasPatrimoniales_EstadosPersona_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_Operadoras_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_GarantiasPatrimoniales_Operadoras_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CrGarantiasPatrimonialesListaResult> CR_GarantiasPatrimoniales_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_GarantiasPatrimoniales_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrGarantiasPatrimonialesListaResult> CR_GarantiasPatrimoniales_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_GarantiasPatrimoniales_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CR_GarantiasPatrimoniales_Guardar(int CodEmpresa, CrGarantiasPatrimonialesRegistroRequest request, string usuario)
        {
            return DB.CR_GarantiasPatrimoniales_Guardar(CodEmpresa, request, usuario);
        }

        public ErrorDto CR_GarantiasPatrimoniales_Eliminar(int CodEmpresa, CrGarantiasPatrimonialesRegistroRequest request, string usuario)
        {
            return DB.CR_GarantiasPatrimoniales_Eliminar(CodEmpresa, request, usuario);
        }
    }
}