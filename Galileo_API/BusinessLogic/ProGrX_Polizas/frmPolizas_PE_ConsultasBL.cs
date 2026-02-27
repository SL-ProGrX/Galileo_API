using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using static Galileo_API.Models.ProGrX_Polizas.FrmPolizasPeConsultasModels;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasPeConsultasBL
    {
        private readonly FrmPolizasPeConsultasDB _db;

        public FrmPolizasPeConsultasBL(IConfiguration config)
        {
            _db = new FrmPolizasPeConsultasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_EstadosPersona_Obtener(int codEmpresa)
                => _db.PolizasPeConsultas_EstadosPersona_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Presentaciones_Obtener(int codEmpresa)
            => _db.PolizasPeConsultas_Presentaciones_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Modelos_Obtener(int codEmpresa)
            => _db.PolizasPeConsultas_Modelos_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Combustibles_Obtener(int codEmpresa)
            => _db.PolizasPeConsultas_Combustibles_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesPeso_Obtener(int codEmpresa)
            => _db.PolizasPeConsultas_UnidadesPeso_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesCapacidad_Obtener(int codEmpresa)
            => _db.PolizasPeConsultas_UnidadesCapacidad_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesCilindraje_Obtener(int codEmpresa)
            => _db.PolizasPeConsultas_UnidadesCilindraje_Obtener(codEmpresa);

        public ErrorDto<PolizasPeConsultasBuscarResponseDto> PolizasPeConsultas_Buscar(
         int codEmpresa,
         bool esExportar,
         PolizasPeConsultasBuscarRequestDto request)
         => _db.PolizasPeConsultas_Buscar(codEmpresa, esExportar, request);

    }
}
