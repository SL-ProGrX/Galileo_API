using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPeriodosGraciaBl
    {
        private readonly FrmCrPeriodosGraciaDb _db;

        public FrmCrPeriodosGraciaBl(IConfiguration config)
        {
            _db = new FrmCrPeriodosGraciaDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Garantias_Obtener(int codEmpresa)
            => _db.CrPeriodosGracia_Garantias_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Divisas_Obtener(int codEmpresa)
            => _db.CrPeriodosGracia_Divisas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Recursos_Obtener(
            int codEmpresa,
            bool lineas,
            string? codigo)
            => _db.CrPeriodosGracia_Recursos_Obtener(codEmpresa, lineas, codigo);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Destinos_Obtener(
            int codEmpresa,
            bool lineas,
            string? codigo)
            => _db.CrPeriodosGracia_Destinos_Obtener(codEmpresa, lineas, codigo);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Instituciones_Obtener(int codEmpresa)
            => _db.CrPeriodosGracia_Instituciones_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Deductoras_Obtener(
            int codEmpresa,
            bool todos,
            string? codInstitucion)
            => _db.CrPeriodosGracia_Deductoras_Obtener(codEmpresa, todos, codInstitucion);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_EstadosPersona_Obtener(int codEmpresa)
            => _db.CrPeriodosGracia_EstadosPersona_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_EstadosLaborales_Obtener(int codEmpresa)
            => _db.CrPeriodosGracia_EstadosLaborales_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Lineas_Obtener(int codEmpresa)
            => _db.CrPeriodosGracia_Lineas_Obtener(codEmpresa);

        public ErrorDto<List<dynamic>> CrPeriodosGracia_Consulta_Obtener(
            int codEmpresa,
            CrPeriodosGraciaConsultaRequest request)
            => _db.CrPeriodosGracia_Consulta_Obtener(codEmpresa, request);

        public ErrorDto CrPeriodosGracia_Aplicar_Ejecutar(
            int codEmpresa,
            CrPeriodosGraciaConsultaRequest request)
            => _db.CrPeriodosGracia_Aplicar_Ejecutar(codEmpresa, request);
    }
}
