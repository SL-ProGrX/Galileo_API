using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del formulario de Asignación de Beneficios (frmAF_BeneficioAsg).
    /// </summary>
    public class FrmAfBeneficioAsgBL
    {
        private readonly FrmAfBeneficioAsgDB _db;

        public FrmAfBeneficioAsgBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficioAsgDB(config);
        }

        /// <summary>Lista paginada de beneficios otorgados al socio.</summary>
        public ErrorDto<AfiBeneOtorgaAsgDataList> AfiBeneOtorga_Obtener(int CodCliente, string cedula, int? pagina, int? paginacion, string? filtro)
            => _db.AfiBeneOtorga_Obtener(CodCliente, cedula, pagina, paginacion, filtro);

        /// <summary>Tipos de beneficio disponibles para el usuario.</summary>
        public ErrorDto<List<BeneficioData>> BeneficioUsuario_Obtener(int CodCliente, string usuario)
            => _db.BeneficioUsuario_Obtener(CodCliente, usuario);

        /// <summary>Detalle del beneficio (catálogo).</summary>
        public ErrorDto<List<AfiBeneDto>> BeneficioDetalle_Obtener(int CodCliente, string cod_beneficio)
            => _db.BeneficioDetalle_Obtener(CodCliente, cod_beneficio);

        /// <summary>Beneficio otorgado a un socio.</summary>
        public ErrorDto<List<AfiBeneOtorgaData>> AfiBeneOtorgaSocio_Obtener(int CodCliente, string codBeneficio, int consec)
            => _db.AfiBeneOtorgaSocio_Obtener(CodCliente, codBeneficio, consec);

        /// <summary>Cálculo de monto de la ayuda.</summary>
        public FxMontosResult fxMonto(int CodCliente, FxMontoModel datos)
            => _db.fxMonto(CodCliente, datos);

        /// <summary>Pagos (órdenes) de un beneficio.</summary>
        public ErrorDto<List<AfiBeneficioPago>> AfiBeneficioPagos_Obtener(int CodCliente, string codBeneficio, int consec)
            => _db.AfiBeneficioPagos_Obtener(CodCliente, codBeneficio, consec);

        /// <summary>Nombre del beneficiario asociado.</summary>
        public ErrorDto Beneficiario_Obtener(int CodCliente, string cedulabn, string cedula)
            => _db.Beneficiario_Obtener(CodCliente, cedulabn, cedula);

        /// <summary>Cuentas bancarias por identificación/banco/divisa.</summary>
        public ErrorDto<List<CuentaListaData>> Cuentas_Obtener(int CodCliente, string Identificacion, int BancoId, int DivisaCheck)
            => _db.Cuentas_Obtener(CodCliente, Identificacion, BancoId, DivisaCheck);

        /// <summary>Cuentas bancarias del usuario.</summary>
        public ErrorDto<List<CuentaListaData>> CuentasUsuario_Obtener(int CodCliente, string usuario)
            => _db.CuentasUsuario_Obtener(CodCliente, usuario);

        /// <summary>Productos asignados a un beneficio.</summary>
        public ErrorDto<List<AfiBeneficioPago>> AfiBeneficioProducto_Obtener(int CodCliente, string codBeneficio, int consec)
            => _db.AfiBeneficioProducto_Obtener(CodCliente, codBeneficio, consec);

        /// <summary>Consulta de membresía activa.</summary>
        public ErrorDto Menbrecia_Consulta(int CodCliente, string? cedula)
            => _db.Menbrecia_Consulta(CodCliente, cedula);

        /// <summary>Monto del grupo del beneficio.</summary>
        public ErrorDto Monto_Obtener(int CodCliente, string cod_beneficio, string cedula, string solicita)
            => _db.Monto_Obtener(CodCliente, cod_beneficio, cedula, solicita);

        /// <summary>Datos del asiento contable del beneficio.</summary>
        public ErrorDto<AsientoContableData> AsientoContableData_Obtener(int CodCliente, string cod_beneficio, string cedula, int consec)
            => _db.AsientoContableData_Obtener(CodCliente, cod_beneficio, cedula, consec);

        /// <summary>Guarda la asignación del beneficio (monetario o de productos).</summary>
        public ErrorDto AfBeneficioAsg_Guardar(int CodCliente, string usuario, AfiBeneficioAsgInsertar beneficio)
            => _db.AfBeneficioAsg_Guardar(CodCliente, usuario, beneficio);
    }
}
