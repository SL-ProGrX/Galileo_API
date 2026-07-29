using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.DataBaseTier;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Dapper;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCFacturasCancelaDB
    {
        private readonly PortalDB _portalDb;

        public FrmCxCFacturasCancelaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catálogo de pagadores con facturas pendientes de cancelación por cliente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedulaCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaPagadores_Obtener(
            int codEmpresa,
            string cedulaCliente)
        {
            const string sql = @"
                SELECT
                    Per.Cedula AS item,
                    Per.Nombre AS descripcion
                FROM vCxC_Facturas_Pendientes_Cancelacion Ft
                INNER JOIN CxC_Personas Per
                    ON Ft.Cedula_Pagador = Per.Cedula
                WHERE Ft.Cedula = @CedulaCliente
                GROUP BY
                    Per.Cedula,
                    Per.Nombre;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { CedulaCliente = (cedulaCliente ?? string.Empty).Trim() });
        }

        /// <summary>
        /// Obtiene el catálogo de divisas de facturas pendientes de cancelación por cliente y pagador.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedulaCliente"></param>
        /// <param name="cedulaPagador"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaDivisas_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaPagador)
        {
            const string sql = @"
                SELECT
                    Cod_Divisa AS item,
                    Cod_Divisa AS descripcion
                FROM vCxC_Facturas_Pendientes_Cancelacion
                WHERE Cedula = @CedulaCliente
                  AND Cedula_Pagador = @CedulaPagador
                GROUP BY Cod_Divisa;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CedulaCliente = (cedulaCliente ?? string.Empty).Trim(),
                    CedulaPagador = (cedulaPagador ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Obtiene la lista de facturas pendientes de cancelación por cliente, pagador y divisa.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedulaCliente"></param>
        /// <param name="cedulaPagador"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCFacturasCancelaPendienteDto>> CxCFacturasCancelaFacturas_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaPagador,
            string codDivisa)
        {
            const string sql = @"
                SELECT
                    Operacion,
                    cod_Factura AS Cod_Factura,
                    Monto,
                    Fecha_Pago AS Fecha_Pago,
                    cod_Divisa AS Cod_Divisa,
                    Importe,
                    Fecha_Emision AS Fecha_Emision,
                    Activa_Fecha AS Activa_Fecha
                FROM vCxC_Facturas_Pendientes_Cancelacion
                WHERE cedula = @CedulaCliente
                  AND Cedula_Pagador = @CedulaPagador
                  AND cod_divisa = @CodDivisa
                ORDER BY CEDULA, FECHA_PAGO, COD_FACTURA;";

            return DbHelper.ExecuteListQuery<CxCFacturasCancelaPendienteDto>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CedulaCliente = (cedulaCliente ?? string.Empty).Trim(),
                    CedulaPagador = (cedulaPagador ?? string.Empty).Trim(),
                    CodDivisa = (codDivisa ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Obtiene el catálogo de tipos de documento por caja para cancelación de facturas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigoCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaTipoDocumento_Obtener(
            int codEmpresa,
            string codigoCaja)
        {
            const string sql = @"
                SELECT
                    RTRIM(C.tipo_documento) AS item,
                    RTRIM(D.Descripcion) AS descripcion
                FROM SIF_DOCUMENTOS D
                INNER JOIN CAJAS_DOCUMENTOS C
                    ON D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                WHERE C.cod_caja = @CodigoCaja
                  AND D.Tipo_Movimiento IN ('A', 'C')
                ORDER BY C.tipo_documento;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodigoCaja = (codigoCaja ?? string.Empty).Trim() });
        }

        /// <summary>
        /// Registra la cancelación de una factura en CxC.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> CxCFacturasCancelaFactura_Registrar(
            int codEmpresa,
            CxCFacturasCancelaFacturaRequestDto request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var dto = request ?? new CxCFacturasCancelaFacturaRequestDto
                {
                    Operacion = 0
                };

                conn.Execute(
                    "spCxC_Operacion_Factura_Cancela",
                    new
                    {
                        dto.Operacion,
                        Factura = (dto.Factura ?? string.Empty).Trim(),
                        dto.Abono,
                        TipoDoc = (dto.Tipo_Documento ?? string.Empty).Trim(),
                        NumDoc = (dto.Numero_Documento ?? string.Empty).Trim(),
                        Usuario = (dto.Usuario ?? string.Empty).Trim()
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });
        }

        /// <summary>
        /// Registra y procesa el abono de cancelación de facturas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> CxCFacturasCancelaAbono_Registrar(
            int codEmpresa,
            CxCFacturasCancelaAbonoRequestDto request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var dto = request ?? new CxCFacturasCancelaAbonoRequestDto();

                conn.Execute(
                    "spCxC_Operacion_Factura_Cancela_Abono",
                    new
                    {
                        TipoDoc = (dto.Tipo_Documento ?? string.Empty).Trim(),
                        NumDoc = (dto.Numero_Documento ?? string.Empty).Trim(),
                        Caja = (dto.Caja ?? string.Empty).Trim(),
                        Apertura = dto.Apertura,
                        Ticket = (dto.Tiquete ?? string.Empty).Trim(),
                        Usuario = (dto.Usuario ?? string.Empty).Trim()
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });
        }
    }
}
