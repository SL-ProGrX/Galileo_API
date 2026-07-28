using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;

namespace Galileo_API.BusinessLogic.ProGrX_Pasivos
{
    public class FrmCrApaLineasBL
    {
        private readonly FrmCrApaLineasDB _db;

        public FrmCrApaLineasBL(IConfiguration config) => _db = new FrmCrApaLineasDB(config);

        public ErrorDto<FrmCrApaLineaCatalogosDto> CR_APA_Lineas_Catalogos_Obtener(int codEmpresa) =>
            _db.CR_APA_Lineas_Catalogos_Obtener(codEmpresa);

        public ErrorDto<List<FrmCrApaLineaCatalogoDto>> CR_APA_Lineas_CentrosCosto_Obtener(int codEmpresa, string cod_unidad) =>
            _db.CR_APA_Lineas_CentrosCosto_Obtener(codEmpresa, cod_unidad);

        public ErrorDto<List<FrmCrApaLineaGridDto>> CR_APA_Lineas_Consultar(int codEmpresa, FrmCrApaLineaConsultaRequest request) =>
            _db.CR_APA_Lineas_Consultar(codEmpresa, request);

        public ErrorDto<FrmCrApaLineaDatosDto> CR_APA_Lineas_Obtener(int codEmpresa, int cod_linea) =>
            _db.CR_APA_Lineas_Obtener(codEmpresa, cod_linea);

        public ErrorDto<FrmCrApaLineaGuardarResultadoDto> CR_APA_Lineas_Guardar(int codEmpresa, FrmCrApaLineaGuardarRequest request) =>
            _db.CR_APA_Lineas_Guardar(codEmpresa, request);
    }
}
