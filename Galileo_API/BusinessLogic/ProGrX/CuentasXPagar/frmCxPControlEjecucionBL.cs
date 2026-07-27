using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPControlEjecucionBL
    {
        private readonly FrmCxPControlEjecucionDB _db;

        public FrmCxPControlEjecucionBL(IConfiguration config)
        {
            _db = new FrmCxPControlEjecucionDB(config);
        }

        public ErrorDto SincronizaTesoreriaCxPReportes(int CodEmpresa)
        {
            return _db.SincronizaTesoreriaCxPReportes(CodEmpresa);
        }

        public ErrorDto<ProveedoresPagosLista> Proveedores_Obtener(
            int CodCliente,
            int? pagina,
            int? paginacion,
            string? filtro,
            string? filtroQ,
            int CodContabilidad = 1,
            DateTime? Vence = null,
            bool SoloPendientes = false)
        {
            return _db.Proveedores_Obtener(
                CodCliente,
                pagina,
                paginacion,
                filtro,
                filtroQ,
                CodContabilidad,
                Vence,
                SoloPendientes);
        }

        public ErrorDto<Divisa> DivisaFuncional_Obtener(int CodEmpresa)
        {
            return _db.DivisaFuncional_Obtener(CodEmpresa);
        }

        public ErrorDto<List<Cargo>> CargosAdicionales_Obtener(int CodEmpresa)
        {
            return _db.CargosAdicionales_Obtener(CodEmpresa);
        }

        public ErrorDto<List<Divisa>> Divisas_Obtener(int CodEmpresa, int CodContabilidad)
        {
            return _db.Divisas_Obtener(CodEmpresa, CodContabilidad);
        }

        public ErrorDto<List<UsuarioEjecucion>> Usuarios_Obtener(int CodEmpresa)
        {
            return _db.Usuarios_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FacturaPendientePago>> FacturasPendientePago_Obtener(int CodEmpresa, FactPenReq request)
        {
            return _db.FacturasPendientePago_Obtener(CodEmpresa, request);
        }

        public ErrorDto<Detalle> DetalleProveedor_Obtener(int CodEmpresa, int Cod_Proveedor, string Vence)
        {
            return _db.DetalleProveedor_Obtener(CodEmpresa, Cod_Proveedor, Vence);
        }

        public ErrorDto RevisionPagos_Reactivar(int CodEmpresa, string User)
        {
            return _db.RevisionPagos_Reactivar(CodEmpresa, User);
        }

        public ErrorDto<List<Autorizado>> Autorizaciones_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.Autorizaciones_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<Fusion> Fusion_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.Fusion_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<List<CuentaDesembolso>> CuentasDesembolso_Obtener(int CodEmpresa)
        {
            return _db.CuentasDesembolso_Obtener(CodEmpresa);
        }

        public ErrorDto<InfoCuenta> InfoCuenta_Obtener(int CodEmpresa, int Cod_Banco)
        {
            return _db.InfoCuenta_Obtener(CodEmpresa, Cod_Banco);
        }

        public ErrorDto<List<CuentaBancaria>> CuentasBancarias_Obtener(int CodEmpresa, string Identificacion, int BancoId, int DivisaCheck)
        {
            return _db.CuentasBancarias_Obtener(CodEmpresa, Identificacion, BancoId, DivisaCheck);
        }

        public ErrorDto<List<CargoPorcentual>> CargoPorcentual_Obtener(int CodEmpresa, int Cod_Proveedor, string Vence)
        {
            return _db.CargoPorcentual_Obtener(CodEmpresa, Cod_Proveedor, Vence);
        }

        public ErrorDto<ProveedorPagos> ConsultaAscDesc(int CodEmpresa, int Cod_Proveedor, int CodContabilidad, string Vence, string tipo)
        {
            return _db.ConsultaAscDesc(CodEmpresa, Cod_Proveedor, CodContabilidad, Vence, tipo);
        }

        public ErrorDto<ProveedorPagos> Proveedor_Obtener(int CodEmpresa, int Cod_Proveedor, int CodContabilidad)
        {
            return _db.Proveedor_Obtener(CodEmpresa, Cod_Proveedor, CodContabilidad);
        }

        public ErrorDto Detalle_Insertar(int CodEmpresa, TesTransAsiento data)
        {
            return _db.Detalle_Insertar(CodEmpresa, data);
        }

        public ErrorDto<Anticipo> MontoAnticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.MontoAnticipos_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto Tesoreria_Insertar(int CodEmpresa, TesTransacciones data)
        {
            return _db.Tesoreria_Insertar(CodEmpresa, data);
        }

        public ErrorDto<TesTransacciones> Tesoreria_Obtener(int CodEmpresa, int NSolicitud)
        {
            return _db.Tesoreria_Obtener(CodEmpresa, NSolicitud);
        }

        public ErrorDto EjecucionPagosCargos_Registra(int CodEmpresa, FacturaPendientePago data)
        {
            return _db.EjecucionPagosCargos_Registra(CodEmpresa, data);
        }

        public ErrorDto EjecucionPagos_CargosFlotantes_Aplicar(int CodEmpresa, FacturaPendientePago data)
        {
            return _db.EjecucionPagos_CargosFlotantes_Aplicar(CodEmpresa, data);
        }

        public ErrorDto EjecucionPagos_SaldosCargoPorc_Actualizar(int CodEmpresa)
        {
            return _db.EjecucionPagos_SaldosCargoPorc_Actualizar(CodEmpresa);
        }

        public ErrorDto<DesembolsoNetos> DesembolsoNetos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.DesembolsoNetos_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto Indicadores_Actualizar(int CodEmpresa, PagoProvUpdate data)
        {
            return _db.Indicadores_Actualizar(CodEmpresa, data);
        }

        public ErrorDto CancelacionCargos_Actualizar(int CodEmpresa, int Cod_Proveedor, string Usuario)
        {
            return _db.CancelacionCargos_Actualizar(CodEmpresa, Cod_Proveedor, Usuario);
        }

        public ErrorDto EjecucionPagos_TesoreriaDetalle_Actualizar(int CodEmpresa)
        {
            return _db.EjecucionPagos_TesoreriaDetalle_Actualizar(CodEmpresa);
        }

        public ErrorDto<List<CargoPer>> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.CargosPer_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<ProveedorInfoEjecucion> ProveedorTesoreria_Obtener(int CodEmpresa, int Cod_Proveedor, int cod_contabilidad )
        {
            return _db.ProveedorTesoreria_Obtener(CodEmpresa, Cod_Proveedor, cod_contabilidad);
        }

        public ErrorDto<List<Anticipo>> Anticipos_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.Anticipos_Obtener(CodEmpresa, Cod_Proveedor);
        }

        public ErrorDto<EjecucionPagosResultado> EjecucionPagos_Aplicar(int CodEmpresa, EjecucionPagosAplicar data)
        {
            return _db.EjecucionPagos_Aplicar(CodEmpresa, data);
        }
    }
}
