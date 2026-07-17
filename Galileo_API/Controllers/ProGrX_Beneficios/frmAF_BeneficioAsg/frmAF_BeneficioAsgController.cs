using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del formulario de Asignación de Beneficios (frmAF_BeneficioAsg).
    /// </summary>
    [Route("api/frmAF_BeneficioAsg")]
    [ApiController]
    public class FrmAfBeneficioAsgController : ControllerBase
    {
        private readonly FrmAfBeneficioAsgBL _bl;

        public FrmAfBeneficioAsgController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficioAsgBL(config);
        }

        /// <summary>Lista paginada de beneficios otorgados al socio.</summary>
        [Authorize]
        [HttpGet("AfiBeneOtorgaAsg_Obtener")]
        public ErrorDto<AfiBeneOtorgaAsgDataList> AfiBeneOtorgaAsg_Obtener(int CodCliente, string cedula, int? pagina, int? paginacion, string? filtro)
            => _bl.AfiBeneOtorga_Obtener(CodCliente, cedula, pagina, paginacion, filtro);

        /// <summary>Detalle del beneficio (catálogo).</summary>
        [Authorize]
        [HttpGet("BeneficioDetalle_Obtener")]
        public ErrorDto<List<AfiBeneDto>> BeneficioDetalle_Obtener(int CodCliente, string cod_beneficio)
            => _bl.BeneficioDetalle_Obtener(CodCliente, cod_beneficio);

        /// <summary>Tipos de beneficio disponibles para el usuario.</summary>
        [Authorize]
        [HttpGet("BeneficioUsuario_Obtener")]
        public ErrorDto<List<BeneficioData>> BeneficioUsuario_Obtener(int CodCliente, string usuario)
            => _bl.BeneficioUsuario_Obtener(CodCliente, usuario);

        /// <summary>Beneficio otorgado a un socio.</summary>
        [Authorize]
        [HttpGet("AfiBeneOtorgaSocio_Obtener")]
        public ErrorDto<List<AfiBeneOtorgaData>> AfiBeneOtorgaSocio_Obtener(int CodCliente, string codBeneficio, int consec)
            => _bl.AfiBeneOtorgaSocio_Obtener(CodCliente, codBeneficio, consec);

        /// <summary>Pagos (órdenes) de un beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioPagos_Obtener")]
        public ErrorDto<List<AfiBeneficioPago>> AfiBeneficioPagos_Obtener(int CodCliente, string codBeneficio, int consec)
            => _bl.AfiBeneficioPagos_Obtener(CodCliente, codBeneficio, consec);

        /// <summary>Nombre del beneficiario asociado.</summary>
        [Authorize]
        [HttpGet("Beneficiario_Obtener")]
        public ErrorDto Beneficiario_Obtener(int CodCliente, string cedulabn, string cedula)
            => _bl.Beneficiario_Obtener(CodCliente, cedulabn, cedula);

        /// <summary>Cuentas bancarias por identificación/banco/divisa.</summary>
        [Authorize]
        [HttpGet("Cuentas_Obtener")]
        public ErrorDto<List<CuentaListaData>> Cuentas_Obtener(int CodCliente, string Identificacion, int BancoId, int DivisaCheck)
            => _bl.Cuentas_Obtener(CodCliente, Identificacion, BancoId, DivisaCheck);

        /// <summary>Cuentas bancarias del usuario.</summary>
        [Authorize]
        [HttpGet("CuentasUsuario_Obtener")]
        public ErrorDto<List<CuentaListaData>> CuentasUsuario_Obtener(int CodCliente, string usuario)
            => _bl.CuentasUsuario_Obtener(CodCliente, usuario);

        /// <summary>Cálculo de monto de la ayuda.</summary>
        [Authorize]
        [HttpPost("fxMonto_Obtener")]
        public FxMontosResult fxMonto(int CodCliente, [FromBody] FxMontoModel datos)
            => _bl.fxMonto(CodCliente, datos);

        /// <summary>Productos asignados a un beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioProducto_Obtener")]
        public ErrorDto<List<AfiBeneficioPago>> AfiBeneficioProducto_Obtener(int CodCliente, string codBeneficio, int consec)
            => _bl.AfiBeneficioProducto_Obtener(CodCliente, codBeneficio, consec);

        /// <summary>Consulta de membresía activa.</summary>
        [Authorize]
        [HttpGet("Menbrecia_Consulta")]
        public ErrorDto Menbrecia_Consulta(int CodCliente, string? cedula)
            => _bl.Menbrecia_Consulta(CodCliente, cedula);

        /// <summary>Monto del grupo del beneficio.</summary>
        [Authorize]
        [HttpGet("Monto_Obtener")]
        public ErrorDto Monto_Obtener(int CodCliente, string cod_beneficio, string cedula, string solicita)
            => _bl.Monto_Obtener(CodCliente, cod_beneficio, cedula, solicita);

        /// <summary>Datos del asiento contable del beneficio.</summary>
        [Authorize]
        [HttpGet("AsientoContableData_Obtener")]
        public ErrorDto<AsientoContableData> AsientoContableData_Obtener(int CodCliente, string cod_beneficio, string cedula, int consec)
            => _bl.AsientoContableData_Obtener(CodCliente, cod_beneficio, cedula, consec);

        /// <summary>Guarda la asignación del beneficio (monetario o de productos).</summary>
        [Authorize]
        [HttpPost("AfBeneficioAsg_Guardar")]
        public ErrorDto AfBeneficioAsg_Guardar(int CodCliente, string usuario, [FromBody] AfiBeneficioAsgInsertar beneficio)
            => _bl.AfBeneficioAsg_Guardar(CodCliente, usuario, beneficio);
    }
}
