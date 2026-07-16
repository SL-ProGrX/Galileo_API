using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del proceso Orden de Pago de Beneficios Integrales (FrmAfBeneficiosIntegralOrP).
    /// </summary>
    public class FrmAfBeneficiosIntegralOrPBL
    {
        private readonly FrmAfBeneficiosIntegralOrPDB _db;

        public FrmAfBeneficiosIntegralOrPBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficiosIntegralOrPDB(config);
        }

        /// <summary>Tipos de identificación.</summary>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodCliente)
            => _db.TiposIdentificacion_Obtener(CodCliente);

        /// <summary>Lista de divisas.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> DivisasLista_Obtener(int CodCliente)
            => _db.DivisasLista_Obtener(CodCliente);

        /// <summary>Lista de bancos.</summary>
        public ErrorDto<List<AfBeneficioIntegralGenericLista>> BancosLista_Obtener(int CodCliente, string Usuario)
            => _db.BancosLista_Obtener(CodCliente, Usuario);

        /// <summary>Cuentas bancarias del socio.</summary>
        public ErrorDto<List<AfBeneIntegralCuentasLista>> CuentasBancariasLista_Obtener(int CodCliente, string? Cedula, int CodBanco)
            => _db.CuentasBancariasLista_Obtener(CodCliente, Cedula, CodBanco);

        /// <summary>Lista de productos.</summary>
        public ErrorDto<List<AfiBeneProductos>> ProductosLista_Obtener(int CodCliente)
            => _db.ProductosLista_Obtener(CodCliente);

        /// <summary>Beneficio otorgado del socio.</summary>
        public ErrorDto<AfiBeneOtorgaData> AfiBeneOtorga_CedulaSocio_Obtener(int CodCliente, string Filtros)
            => _db.AfiBeneOtorga_CedulaSocio_Obtener(CodCliente, Filtros);

        /// <summary>Tabla de órdenes de pago del beneficio.</summary>
        public ErrorDto<List<AfiBeneIntegralOrP>> AfiBeneficioPagosTabla_Obtener(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
            => _db.AfiBeneficioPagosTabla_Obtener(CodCliente, Cedula, Cod_Beneficio, Consec);

        /// <summary>Valida si ya existe una orden de pago para el expediente.</summary>
        public ErrorDto AfiBeneficioPagos_ValidaExiste(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
            => _db.AfiBeneficioPagos_ValidaExiste(CodCliente, Cedula, Cod_Beneficio, Consec);

        /// <summary>Agrega una orden de pago.</summary>
        public ErrorDto AfiBeneficioIntegralOrdenPago_Agregar(int CodCliente, AfiBeneIntegralOrP beneficio)
            => _db.AfiBeneficioIntegralOrdenPago_Agregar(CodCliente, beneficio);

        /// <summary>Actualiza una orden de pago.</summary>
        public ErrorDto AfiBeneficioIntegralOrdenPago_Actualizar(int CodCliente, AfiBeneIntegralOrP beneficio)
            => _db.AfiBeneficioIntegralOrdenPago_Actualizar(CodCliente, beneficio);

        /// <summary>Proyecciones de pago del beneficio.</summary>
        public ErrorDto<List<AfiBenePagoProyecta>> AfiBeneficioIntegralProyeccionPago_Obtener(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
            => _db.AfiBeneficioIntegralProyeccionPago_Obtener(CodCliente, Cedula, Cod_Beneficio, Consec);

        /// <summary>Inserta una proyección de pago.</summary>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Insertar(int CodCliente, AfiBenePagoProyecta beneficio)
            => _db.AfiBeneficioIntegralProyeccionPago_Insertar(CodCliente, beneficio);

        /// <summary>Actualiza una proyección de pago.</summary>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Actualizar(int CodCliente, AfiBenePagoProyecta beneficio)
            => _db.AfiBeneficioIntegralProyeccionPago_Actualizar(CodCliente, beneficio);

        /// <summary>Elimina una proyección de pago.</summary>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Eliminar(int CodCliente, int Plan_Id)
            => _db.AfiBeneficioIntegralProyeccionPago_Eliminar(CodCliente, Plan_Id);
    }
}
