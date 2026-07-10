using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXBalancesLoadController : ControllerBase
    {
        private readonly FrmCntXBalancesLoadBL _bl;

        public FrmCntXBalancesLoadController(IConfiguration config)
        {
            _bl = new FrmCntXBalancesLoadBL(config);
        }

        [HttpGet("CntX_Balances_Load_Pantalla_Obtener")]
        public ActionResult<ErrorDto<CntXBalancesLoadPantallaDto>> CntX_Balances_Load_Pantalla_Obtener(
            int codEmpresa,
            int contabilidad,
            int anio,
            int mes)
            => _bl.CntX_Balances_Load_Pantalla_Obtener(codEmpresa, contabilidad, anio, mes);

        [HttpGet("CntX_Balances_Load_Historico_Listar")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntX_Balances_Load_Historico_Listar(
            int codEmpresa,
            string request)
            => _bl.CntX_Balances_Load_Historico_Listar(codEmpresa, request);

        [HttpGet("CntX_Balances_Load_Historico_Consultar")]
        public ActionResult<ErrorDto<List<CntXBalancesLoadResultadoDto>>> CntX_Balances_Load_Historico_Consultar(
            int codEmpresa,
            int historicoId)
            => _bl.CntX_Balances_Load_Historico_Consultar(codEmpresa, historicoId);

        [HttpPost("CntX_Balances_Load_Archivo_Cargar")]
        public ActionResult<ErrorDto<List<CntXBalancesLoadResultadoDto>>> CntX_Balances_Load_Archivo_Cargar(
            int codEmpresa,
            CntXBalancesLoadArchivoCargarRequestDto request)
            => _bl.CntX_Balances_Load_Archivo_Cargar(codEmpresa, request);

        [HttpPost("CntX_Balances_Load_Importar")]
        public ActionResult<ErrorDto<CntXBalancesLoadProcesoResultDto?>> CntX_Balances_Load_Importar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
            => _bl.CntX_Balances_Load_Importar(codEmpresa, request);

        [HttpPost("CntX_Balances_Load_Inicializar")]
        public ActionResult<ErrorDto<CntXBalancesLoadProcesoResultDto?>> CntX_Balances_Load_Inicializar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
            => _bl.CntX_Balances_Load_Inicializar(codEmpresa, request);

        [HttpPost("CntX_Balances_Load_ImportarContaBase")]
        public ActionResult<ErrorDto<CntXBalancesLoadProcesoResultDto?>> CntX_Balances_Load_ImportarContaBase(
            int codEmpresa,
            CntXBalancesLoadImportaContaBaseRequestDto request)
            => _bl.CntX_Balances_Load_ImportarContaBase(codEmpresa, request);
    }
}