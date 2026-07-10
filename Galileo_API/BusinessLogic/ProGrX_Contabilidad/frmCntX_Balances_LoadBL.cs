using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXBalancesLoadBL
    {
        private readonly FrmCntXBalancesLoadDB _db;

        public FrmCntXBalancesLoadBL(IConfiguration config)
        {
            _db = new FrmCntXBalancesLoadDB(config);
        }

        public ErrorDto<CntXBalancesLoadPantallaDto> CntX_Balances_Load_Pantalla_Obtener(
            int codEmpresa,
            int contabilidad,
            int anio,
            int mes)
            => _db.CntX_Balances_Load_Pantalla_Obtener(codEmpresa, contabilidad, anio, mes);

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Balances_Load_Historico_Listar(
            int codEmpresa,
            string request)
        {
            var dto = JsonConvert.DeserializeObject<CntXBalancesLoadHistoricoListarRequestDto>(request)
                      ?? new CntXBalancesLoadHistoricoListarRequestDto();

            return _db.CntX_Balances_Load_Historico_Listar(codEmpresa, dto);
        }

        public ErrorDto<List<CntXBalancesLoadResultadoDto>> CntX_Balances_Load_Historico_Consultar(
            int codEmpresa,
            int historicoId)
            => _db.CntX_Balances_Load_Historico_Consultar(codEmpresa, historicoId);

        public ErrorDto<List<CntXBalancesLoadResultadoDto>> CntX_Balances_Load_Archivo_Cargar(
            int codEmpresa,
            CntXBalancesLoadArchivoCargarRequestDto request)
            => _db.CntX_Balances_Load_Archivo_Cargar(codEmpresa, request);

        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_Importar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
            => _db.CntX_Balances_Load_Importar(codEmpresa, request);

        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_Inicializar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
            => _db.CntX_Balances_Load_Inicializar(codEmpresa, request);

        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_ImportarContaBase(
            int codEmpresa,
            CntXBalancesLoadImportaContaBaseRequestDto request)
            => _db.CntX_Balances_Load_ImportarContaBase(codEmpresa, request);
    }
}