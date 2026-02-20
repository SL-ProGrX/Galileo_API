using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCFacturasCancelaPagadorDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb DBBitacora;
        private readonly MRecibos _mRecibos;
        private readonly int vModulo = 31;

        public FrmCxCFacturasCancelaPagadorDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config),
                 new MRecibos(config))
        {
        }

        public FrmCxCFacturasCancelaPagadorDb(PortalDB portalDB, MSecurityMainDb dbBitacora, MRecibos mRecibos)
        {
            _portalDb = portalDB;
            DBBitacora = dbBitacora;
            _mRecibos = mRecibos;
        }

        /// <summary>
        /// Obtiene los tipos de documento 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="caja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_TipoDoc_Obtener(int codEmpresa, string caja)
        {
            const string query = @"select rTrim(C.tipo_documento) as item, rtrim(D.Descripcion) as descripcion 
                from SIF_DOCUMENTOS D inner join CAJAS_DOCUMENTOS C on D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO 
                Where C.cod_caja = @caja and D.Tipo_Movimiento in('A','C') 
                order by C.tipo_documento";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { caja });
        }

        /// <summary>
        /// Obtiene los pagadores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_Pagadores_Obtener(int codEmpresa)
        {
            const string query = @"select Per.Cedula as item, Per.Nombre as descripcion  
                from vCxC_Facturas_Pendientes_Cancelacion Ft inner join CxC_Personas Per on Ft.Cedula_Pagador = Per.Cedula
                group by Per.Cedula, Per.Nombre";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene las divisas disponibles para el pagador
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPagador"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_Divisas_Obtener(int codEmpresa, string codPagador)
        {
            const string query = @"select Cod_Divisa as item, Cod_Divisa as descripcion 
                from vCxC_Facturas_Pendientes_Cancelacion 
                where cedula_pagador = @codPagador 
                group by Cod_Divisa";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codPagador });
        }

        /// <summary>
        /// Obtiene las facturas pendientes de cancelacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCFactPendienteCancelacionData>> CxCFactCancPag_FacturasPendientes_Obtener(int codEmpresa, CxCFactCancPagFacturasRequest filtro)
        {
            var query = @"select Operacion, Cod_Factura, Monto, Cod_Divisa, Fecha_Pago, 
                Importe, Fecha_Emision, Activa_Fecha, Cedula, Nombre 
                from vCxC_Facturas_Pendientes_Cancelacion 
                where Cedula_Pagador = @codPagador 
                  and Cod_Divisa = @codDivisa";

            if (!string.IsNullOrWhiteSpace(filtro.factura))
            {
                query += @" and Cod_Factura like '%' + @Factura + '%'";
            }

            if (!string.IsNullOrWhiteSpace(filtro.cliente))
            {
                query += @" and Nombre like '%' + @Cliente + '%'";
            }

            query += @" order by Nombre, Cod_Factura, Fecha_Pago";

            var param = new
            {
                codPagador = filtro.pagador,
                codDivisa = filtro.divisa,
                Factura = filtro.factura,
                Cliente = filtro.cliente
            };

            return DbHelper.ExecuteListQuery<CxCFactPendienteCancelacionData>(_portalDb, codEmpresa, query, param);
        }

        /// <summary>
        /// Registra el abono para las facturas seleccionadas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto CxCFactCancPag_Abono_Registrar(int codEmpresa, CxCFactCancPagRegistrarAbonoRequest req)
        {
            try
            {
                var vNumDoc = _mRecibos.FxDocumentoConsecutivo(codEmpresa, req.tipodoc).ToString();

                //Procesa las Facturas Canceladas
                var respP = CxCFacturasCanceladas_Procesar(codEmpresa, req.tipodoc, vNumDoc, req.usuario, req.lista);
                if (FailIfError(respP, out var ee)) return ee;

                //Procesa Abono + Documento + Asiento
                var spCOFCA = Exec(codEmpresa,
                        @"exec spCxC_Operacion_Factura_Cancela_Abono @tipoDoc, @numDoc, @caja, @apertura, @tiquete, @user, 1;",
                        new { 
                            tipoDoc = req.tipodoc,
                            numDoc = vNumDoc,
                            caja = req.mcaja,
                            apertura = req.mapertura,
                            tiquete = req.mtiquete,
                            user = req.usuario
                        });

                if (FailIfError(spCOFCA, out ee)) return ee;


                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (req.usuario ?? "").ToUpper(),
                    DetalleMovimiento = "Registra Cancelación de Facturas > Pagador Id: " + req.pagador,
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                return new ErrorDto { Code = 0, Description = vNumDoc };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        #region helpers CxCFactCancPag_Abono_Registrar

        private static bool FailIfError(ErrorDto? resp, out ErrorDto err)
        {
            if (resp is { Code: not null } && resp.Code != 0)
            {
                err = resp;
                return true;
            }

            err = new ErrorDto { Code = 0, Description = "" };
            return false;
        }

        private ErrorDto Exec(int codEmpresa, string sqlString, object param)
        {
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlString, param);
        }

        private ErrorDto CxCFacturasCanceladas_Procesar(int codEmpresa, string vTipoDoc, string vNumDoc, string usuario, List<CxCFactPendienteCancelacionData> lista)
        {
            const string query = @"exec spCxC_Operacion_Factura_Cancela @operacionId, @factura, @abono, @tipoDoc, @numDoc, @user;";

            foreach (var item in lista)
            {
                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    query,
                    new
                    {
                        operacionId = item.operacion,
                        factura = item.cod_factura,
                        abono = item.monto,
                        tipoDoc = vTipoDoc,
                        numDoc = vNumDoc,
                        user = usuario
                    });

                if (resp.Code == -1)
                    return resp;
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "Procesamiento de facturas canceladas exitosamente"
            };
        }

        #endregion
    }
}
