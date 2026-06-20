using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvParametrosDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvParametrosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Consulta SQL de actualización de parámetros generales.
        /// </summary>
        private const string ConsultaActualizacion = @"UPDATE PV_PARAMETROS_GEN SET
                        Cta_Comisiones = @Cta_Comisiones,
                        Cta_Imp_Renta = @Cta_Imp_Renta,
                        Cta_Imp_Consumo = @Cta_Imp_Consumo,
                        Cta_Gastos = @Cta_Gastos,
                        Cta_Costo_Ventas = @Cta_Costo_Ventas,
                        Cta_Recibos = @Cta_Recibos,
                        Cta_Notas = @Cta_Notas,
                        Cta_Ventas_Ing = @Cta_Ventas_Ing,
                        Ta_Factura_Man = @Ta_Factura_Man,
                        Ta_Factura_Auto = @Ta_Factura_Auto,
                        Ta_Entradas = @Ta_Entradas,
                        Ta_Salidas = @Ta_Salidas,
                        Ta_Traslados = @Ta_Traslados,
                        Ta_Devoluciones = @Ta_Devoluciones,
                        Ta_Nc = @Ta_Nc,
                        Ta_Recibos = @Ta_Recibos,
                        Ta_Nd = @Ta_Nd,
                        Ta_Gen = @Ta_Gen,
                        Enlace_Conta = @Enlace_Conta,
                        Enlace_Sif = @Enlace_Sif
                  WHERE COD_PAR = @Cod_Par;";

        /// <summary>
        /// Crea el objeto de parámetros para actualización de parámetros generales.
        /// </summary>
        /// <param name="data">Datos de parámetros generales.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosActualizacion(ParametrosGenDto data) => new
        {
            data.Cta_Comisiones,
            data.Cta_Imp_Renta,
            data.Cta_Imp_Consumo,
            data.Cta_Gastos,
            data.Cta_Costo_Ventas,
            data.Cta_Recibos,
            data.Cta_Notas,
            data.Cta_Ventas_Ing,
            data.Ta_Factura_Man,
            data.Ta_Factura_Auto,
            data.Ta_Entradas,
            data.Ta_Salidas,
            data.Ta_Traslados,
            data.Ta_Devoluciones,
            data.Ta_Nc,
            data.Ta_Recibos,
            data.Ta_Nd,
            data.Ta_Gen,
            data.Enlace_Conta,
            data.Enlace_Sif,
            data.Cod_Par
        };

        #endregion

        #region Consultas

        public ErrorDto<ParametrosGenDto?> Parametros_Obtener(int CodEmpresa) =>
            DbHelper.ExecuteSingleQuery<ParametrosGenDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT * FROM PV_PARAMETROS_GEN",
                null,
                null);

        public ErrorDto<List<CntXContaDto>> ObtenerContabilidades(int CodEmpresa) =>
            DbHelper.ExecuteListQuery<CntXContaDto>(CreatePortalDb(), CodEmpresa, "SELECT * FROM CntX_Contabilidades");

        public ErrorDto<List<DescripcionCuentasDto>> Obtener_DescripcionesCuenta(int CodEmpresa) =>
            DbHelper.ExecuteListQuery<DescripcionCuentasDto>(CreatePortalDb(), CodEmpresa, "SELECT Cod_Cuenta, Descripcion FROM CNTX_CUENTAS");

        public ErrorDto<List<DescripcionTipoAsientoDto>> Obtener_DescripcionesAsiento(int CodEmpresa) =>
            DbHelper.ExecuteListQuery<DescripcionTipoAsientoDto>(CreatePortalDb(), CodEmpresa, "SELECT Tipo_Asiento, Descripcion FROM CntX_Tipos_Asientos");

        public ErrorDto<List<DescripcionTipoAsientoDto>> Asientos_Obtener(int CodEmpresa) =>
            DbHelper.ExecuteListQuery<DescripcionTipoAsientoDto>(CreatePortalDb(), CodEmpresa, "SELECT * FROM CNTX_TIPOS_ASIENTOS");

        #endregion

        #region Mantenimiento

        public ErrorDto actualizar_Parametros(int CodEmpresa, ParametrosGenDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                ConsultaActualizacion,
                CrearParametrosActualizacion(data));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar los parámetros generales.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}