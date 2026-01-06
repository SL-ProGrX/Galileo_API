using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasTransaccionesDb
    {
        private readonly PortalDB _portalDb;
        private readonly string error = "Error";

        public FrmCajasTransaccionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de socios ordenada por nombre.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<CajasSocioResult>> CajasTransacciones_Socios_Obtener(int codEmpresa)
        {
            var query = @"select Cedula, CedulaR, nombre from socios order by nombre";
            return DbHelper.ExecuteListQuery<CajasSocioResult>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de servicios asignados a una caja, filtrando por nombre y excluyendo concepto 'CAJ002'.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de búsqueda.</param>
        /// <returns></returns>
        public ErrorDto<List<CajasServicioResult>> CajasTransacciones_Servicios_Obtener(int codEmpresa, CajasServicioConsultaParams param)
        {
            var query = @"
                Select
                  S.cod_servicio,
                  S.descripcion as ServicioDesc,
                  R.Cod_recaudador,
                  R.descripcion as RecaudadorDesc
                from cajas_servicios_asignados X
                inner join cajas_servicios S
                  on X.cod_Recaudador = S.Cod_Recaudador
                 and X.cod_Servicio  = S.cod_Servicio
                inner join cajas_recaudador R
                  on S.cod_recaudador = R.cod_recaudador
                where X.cod_Caja = @CodCaja
                  and S.descripcion like @ServicioBusqueda
                  and S.cod_Concepto not in('CAJ002')
                order by S.descripcion";
            var parameters = new
            {
                param.CodCaja,
                ServicioBusqueda = "%" + (param.ServicioBusqueda ?? "") + "%"
            };
            return DbHelper.ExecuteListQuery<CajasServicioResult>(_portalDb, codEmpresa, query, parameters);
        }

        /// <summary>
        /// Obtiene la lista de divisas para la contabilidad indicada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CajasTransacciones_Divisas_Obtener(int codEmpresa, string codContabilidad)
        {
            var query = @"select rtrim(Cod_Divisa) as item, rtrim(descripcion) as descripcion 
                          from cntx_divisas 
                          where cod_contabilidad = @CodContabilidad 
                          order by cod_divisa";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { CodContabilidad = codContabilidad });
        }

        /// <summary>
        /// Obtiene la lista de tipos de documentos asociados a una caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codCaja">Código de la caja.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CajasTransacciones_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            var query = @"select rtrim(Doc.Tipo_Documento) as item, rtrim(Doc.Descripcion) as descripcion
                          from Cajas_Documentos Cj
                          inner join SIF_Documentos Doc on Cj.Tipo_Documento = Doc.Tipo_Documento
                          where Cj.Cod_Caja = @CodCaja";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { CodCaja = codCaja });
        }

        /// <summary>
        /// Ejecuta el procedimiento de validación y advertencias para la caja.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de validación.</param>
        /// <returns></returns>
        public ErrorDto<CajasTransacValidacionResult> CajasTransacciones_Validacion(int codEmpresa, CajasTransacValidacionParams param)
        {
            var query = "exec spCajas_Transac_Validacion @Caja, @Usuario, @Apertura, @SesionId, @TipoProc, @Producto, @Monto, @Ticket";
            var parameters = new
            {
                param.Caja,
                param.Usuario,
                param.Apertura,
                param.SesionId,
                TipoProc = "CAJ",
                Producto = param.CodServicio,
                Monto = param.TotalCajas,
                Ticket = param.Tiquete
            };
            return DbHelper.ExecuteSingleQuery<CajasTransacValidacionResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado spCajas_ServiciosDatos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de consulta.</param>
        /// <returns></returns>
        public ErrorDto<CajasServiciosDatosResult> CajasTransacciones_ServiciosDatos(int codEmpresa, CajasServiciosDatosParams param)
        {
            var query = "exec spCajas_ServiciosDatos @Recaudador = @Cod_Recaudador, @Servicio = @Cod_Servicio, @Monto = @Monto, @Caja = @Cod_Caja";
            return DbHelper.ExecuteSingleQuery<CajasServiciosDatosResult>(_portalDb, codEmpresa, query, default, param);
        }

        /// <summary>
        /// Inserta una transacción en SIF_TRANSACCIONES.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de inserción.</param>
        /// <returns></returns>
        public ErrorDto<bool> SifTransacciones_Insertar(int codEmpresa, SifTransaccionInsertParams param)
        {
            var query = @"
                insert into SIF_TRANSACCIONES(
                  COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
                  Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
                  Referencia_01, Referencia_02, Referencia_03, cod_oficina,
                  linea1, linea2, linea3, linea4, detalle, documento,
                  cod_caja, cod_apertura, id_Sesion
                )
                values(
                  @Cod_Transaccion, @Tipo_Documento, dbo.MyGetdate(), @Registro_Usuario,
                  @Cliente_Identificacion, @Cliente_Nombre, @Cod_Concepto, @Monto, 'P',
                  @Referencia_01, @Referencia_02, @Referencia_03, @Cod_Oficina,
                  @Linea1, @Linea2, @Linea3, @Linea4, @Detalle, @Documento,
                  @Cod_Caja, @Cod_Apertura, @Id_Sesion
                )";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                 ? DbHelper.CreateOkResponse(true)
                 : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Inserta un registro en CAJAS_SERVICIOS_TRANSAC.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de inserción.</param>
        /// <returns></returns>
        public ErrorDto<bool> CajasServiciosTransac_Insertar(int codEmpresa, CajasServiciosTransacInsertParams param)
        {
            var query = @"
                insert into CAJAS_SERVICIOS_TRANSAC(
                  Linea, Cod_Caja, Cod_Apertura, Cod_Recaudador, Cod_Servicio,
                  Tipo_Documento, Cod_Transaccion, num_referencia,
                  monto, comision, impuesto, neto, cod_divisa, Tipo_Cambio
                )
                values(
                  (select isnull(max(Linea),0) + 1 from CAJAS_SERVICIOS_TRANSAC),
                  @Cod_Caja, @Cod_Apertura, @Cod_Recaudador, @Cod_Servicio,
                  @Tipo_Documento, @Cod_Transaccion, @Num_Referencia,
                  @Monto, @Comision, @Impuesto, @Neto, @Cod_Divisa, @Tipo_Cambio
                )";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                 ? DbHelper.CreateOkResponse(true)
                 : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado spSIFDocsAsiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de asiento.</param>
        /// <returns></returns>
        public ErrorDto<SifDocsAsientoResult> SifDocsAsiento_Ejecutar(int codEmpresa, SifDocsAsientoParams param)
        {
            decimal tipoCambioAplicado = Tipo_Cambio_Apl(param.Tipo_Cambio);
            decimal montoConvertido = param.Mnt_Bruto * tipoCambioAplicado;

            var query = @"exec spSIFDocsAsiento
                @Tipo = @Tipo_Documento,
                @Transaccion = @Cod_Transaccion,
                @Monto = @Monto,
                @Movimiento = @Movimiento,
                @Divisa = @Cod_Divisa,
                @TipoCambio = @Tipo_Cambio,
                @Contabilidad = @Cod_Contabilidad,
                @Unidad = @Cod_Unidad,
                @CentroCosto = @Cod_Centro_Costo,
                @Cuenta = @Ef_Cta,
                @Referencia1 = @Referencia_01,
                @Referencia2 = @Referencia_02,
                @Referencia3 = @Referencia_03,
                @DivisaRev = @Divisa_Rev,
                @NoReversa = @No_Reversa";

            var parameters = new
            {
                param.Tipo_Documento,
                param.Cod_Transaccion,
                Monto = montoConvertido,
                Movimiento = "C",
                param.Cod_Divisa,
                param.Tipo_Cambio,
                param.Cod_Contabilidad,
                param.Cod_Unidad,
                param.Cod_Centro_Costo,
                param.Ef_Cta,
                param.Referencia_01,
                param.Referencia_02,
                param.Referencia_03,
                Divisa_Rev = param.Divisa_Rev ?? 0,
                No_Reversa = param.No_Reversa ?? 0
            };

            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, parameters);
            return new ErrorDto<SifDocsAsientoResult>
            {
                Code = result.Code,
                Description = result.Description,
                Result = new SifDocsAsientoResult { Exito = result.Code == 0 }
            };
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado spCajas_DesglocePagosDocFinal.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de ejecución.</param>
        /// <returns></returns>
        public ErrorDto<bool> CajasDesglocePagosDocFinal_Ejecutar(int codEmpresa, CajasDesglocePagosDocFinalParams param)
        {
            var query = @"exec spCajas_DesglocePagosDocFinal
                @Caja = @Cod_Caja,
                @Apertura = @Cod_Apertura,
                @Ticket = @Tiquete,
                @Usuario = @Usuario,
                @TipoDoc = @Tipo_Documento,
                @NumDoc = @Cod_Transaccion,
                @Unidad = @Cod_Unidad,
                @Ref_01 = @Referencia_01,
                @Ref_02 = @Referencia_02";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                 ? DbHelper.CreateOkResponse(true)
                 : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado spCajas_IntercambioRegistra.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de ejecución.</param>
        /// <returns></returns>
        public ErrorDto<bool> CajasIntercambioRegistra(int codEmpresa, CajasIntercambioRegistraParams param)
        {
            var query = @"exec spCajas_IntercambioRegistra
                @TipoDoc = @Tipo_Documento,
                @NumDoc = @Cod_Transaccion,
                @FormaPago = @Ef_Codigo,
                @Monto = @Monto,
                @Cuenta = @Ef_Cta,
                @Unidad = @Cod_Unidad,
                @Usuario = @Usuario";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                 ? DbHelper.CreateOkResponse(true)
                 : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado spCajas_ValoresTransito_Registra.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de ejecución.</param>
        /// <returns></returns>
        public ErrorDto<bool> CajasValoresTransitoRegistra(int codEmpresa, CajasValoresTransitoRegistraParams param)
        {
            var query = @"exec spCajas_ValoresTransito_Registra
                @TipoDoc = @Tipo_Documento,
                @NumDoc = @Cod_Transaccion,
                @Recaudador = @Cod_Recaudador,
                @Servicio = @Cod_Servicio,
                @Caja = @Cod_Caja,
                @Apertura = @Cod_Apertura,
                @Usuario = @Usuario";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                 ? DbHelper.CreateOkResponse(true)
                 : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado spCajas_General_TE.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de ejecución.</param>
        /// <returns></returns>
        public ErrorDto<bool> CajasGeneralTE_Ejecutar(int codEmpresa, CajasGeneralTEParams param)
        {
            var query = @"exec spCajas_General_TE
                @TipoDoc = @Tipo_Documento,
                @NumDoc = @Cod_Transaccion,
                @Tipo = @Tipo";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                 ? DbHelper.CreateOkResponse(true)
                 : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado spCajasReciboDigital.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de ejecución.</param>
        /// <returns></returns>
        public ErrorDto<bool> CajasReciboDigital(int codEmpresa, CajasReciboDigitalParams param)
        {
            var query = @"exec spCajasReciboDigital
                @NumeroDocumento = @NumeroDocumento,
                @TipoDocumento = @TipoDocumento,
                @TipoComprobante = @TipoComprobante";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, param);
            return result.Code == 0
                 ? DbHelper.CreateOkResponse(true)
                 : DbHelper.CreateErrorResponse<bool>(result.Description ?? error, result.Code ?? -1, false);
        }

        // Utilidad para tipo de cambio
        private static decimal Tipo_Cambio_Apl(decimal pTipoCambio)
        {
            if (pTipoCambio == 0)
                pTipoCambio = 1;

            return pTipoCambio > 0
                ? pTipoCambio
                : 1 / Math.Abs(pTipoCambio);
        }
    }
}
