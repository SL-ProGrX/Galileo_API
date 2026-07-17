using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del proceso Orden de Pago de Beneficios Integrales (FrmAfBeneficiosIntegralOrP).
    /// </summary>
    [Route("api/frmAF_Beneficios_Integral_OrP")]
    [ApiController]
    public class FrmAfBeneficiosIntegralOrPController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralOrPBL _bl;

        public FrmAfBeneficiosIntegralOrPController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralOrPBL(config);
        }

        /// <summary>Tipos de identificación.</summary>
        [Authorize]
        [HttpGet("TiposIdentificacion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodCliente)
            => _bl.TiposIdentificacion_Obtener(CodCliente);

        /// <summary>Lista de divisas.</summary>
        [Authorize]
        [HttpGet("DivisasLista_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> DivisasLista_Obtener(int CodCliente)
            => _bl.DivisasLista_Obtener(CodCliente);

        /// <summary>Lista de bancos.</summary>
        [Authorize]
        [HttpGet("BancosLista_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralGenericLista>> BancosLista_Obtener(int CodCliente, string Usuario)
            => _bl.BancosLista_Obtener(CodCliente, Usuario);

        /// <summary>Cuentas bancarias del socio.</summary>
        [Authorize]
        [HttpGet("CuentasBancariasLista_Obtener")]
        public ErrorDto<List<AfBeneIntegralCuentasLista>> CuentasBancariasLista_Obtener(int CodCliente, string? Cedula, int CodBanco)
            => _bl.CuentasBancariasLista_Obtener(CodCliente, Cedula, CodBanco);

        /// <summary>Lista de productos.</summary>
        [Authorize]
        [HttpGet("ProductosLista_Obtener")]
        public ErrorDto<List<AfiBeneProductos>> ProductosLista_Obtener(int CodCliente)
            => _bl.ProductosLista_Obtener(CodCliente);

        /// <summary>Beneficio otorgado del socio.</summary>
        [Authorize]
        [HttpGet("AfiBeneOtorga_CedulaSocio_Obtener")]
        public ErrorDto<AfiBeneOtorgaData> AfiBeneOtorga_CedulaSocio_Obtener(int CodCliente, string Filtros)
            => _bl.AfiBeneOtorga_CedulaSocio_Obtener(CodCliente, Filtros);

        /// <summary>Tabla de órdenes de pago del beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioPagosTabla_Obtener")]
        public ErrorDto<List<AfiBeneIntegralOrP>> AfiBeneficioPagosTabla_Obtener(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
            => _bl.AfiBeneficioPagosTabla_Obtener(CodCliente, Cedula, Cod_Beneficio, Consec);

        /// <summary>Valida si ya existe una orden de pago para el expediente.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioPagos_ValidaExiste")]
        public ErrorDto AfiBeneficioPagos_ValidaExiste(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
            => _bl.AfiBeneficioPagos_ValidaExiste(CodCliente, Cedula, Cod_Beneficio, Consec);

        /// <summary>Agrega una orden de pago.</summary>
        [Authorize]
        [HttpPost("AfiBeneficioIntegralOrdenPago_Agregar")]
        public ErrorDto AfiBeneficioIntegralOrdenPago_Agregar(int CodCliente, [FromBody] AfiBeneIntegralOrP beneficio)
            => _bl.AfiBeneficioIntegralOrdenPago_Agregar(CodCliente, beneficio);

        /// <summary>Actualiza una orden de pago.</summary>
        [Authorize]
        [HttpPost("AfiBeneficioIntegralOrdenPago_Actualizar")]
        public ErrorDto AfiBeneficioIntegralOrdenPago_Actualizar(int CodCliente, [FromBody] AfiBeneIntegralOrP beneficio)
            => _bl.AfiBeneficioIntegralOrdenPago_Actualizar(CodCliente, beneficio);

        /// <summary>Proyecciones de pago del beneficio.</summary>
        [Authorize]
        [HttpGet("AfiBeneficioIntegralProyeccionPago_Obtener")]
        public ErrorDto<List<AfiBenePagoProyecta>> AfiBeneficioIntegralProyeccionPago_Obtener(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
            => _bl.AfiBeneficioIntegralProyeccionPago_Obtener(CodCliente, Cedula, Cod_Beneficio, Consec);

        /// <summary>Inserta una proyección de pago.</summary>
        [Authorize]
        [HttpPost("AfiBeneficioIntegralProyeccionPago_Insert")]
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Insert(int CodCliente, [FromBody] AfiBenePagoProyecta beneficio)
            => _bl.AfiBeneficioIntegralProyeccionPago_Insertar(CodCliente, beneficio);

        /// <summary>Actualiza una proyección de pago.</summary>
        [Authorize]
        [HttpPost("AfiBeneficioIntegralProyeccionPago_Actualizar")]
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Actualizar(int CodCliente, [FromBody] AfiBenePagoProyecta beneficio)
            => _bl.AfiBeneficioIntegralProyeccionPago_Actualizar(CodCliente, beneficio);

        /// <summary>Elimina una proyección de pago.</summary>
        [Authorize]
        [HttpPost("AfiBeneficioIntegralProyeccionPago_Eliminar")]
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Eliminar(int CodCliente, int Plan_Id)
            => _bl.AfiBeneficioIntegralProyeccionPago_Eliminar(CodCliente, Plan_Id);
    }
}