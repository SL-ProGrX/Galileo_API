using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXDivisasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 20;

        public FrmCntXDivisasDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCntXDivisasDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el listado de unidades 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Unidades_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion 
                from CntX_Unidades where cod_contabilidad = @codConta";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Obtiene el listado de centros de costos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_CentroCostos_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select COD_CENTRO_COSTO AS item, RTRIM(DESCRIPCION) AS descripcion 
                From CNTX_CENTRO_COSTOS 
                Where Activo = 1 And COD_CONTABILIDAD = @codConta";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Obtiene el listado de divisas 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Lista_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select cod_divisa as item,descripcion from CntX_Divisas 
                Where COD_CONTABILIDAD = @codConta";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Obtiene la informacion de una divisa 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<CntXDivisaData?> CntXDivisas_Obtener(int codEmpresa, int codConta, string codDivisa)
        {
            const string sql = @"select M.* 
                , isnull(I.Cod_Cuenta_Mask,'') as 'CtaIng', isnull(G.Cod_Cuenta_Mask,'') as 'CtaGst'
                , isnull(I.descripcion,'') as 'CtaIng_Desc', isnull(G.Descripcion,'') as 'CtaGst_Desc'
                , isnull(U.Descripcion,'') as 'Unidad_Desc', isnull(Cc.Descripcion,'') as 'Centro_Desc'
            from CntX_Divisas M
            left join CntX_Cuentas I
                on M.COD_CONTABILIDAD = I.COD_CONTABILIDAD 
               and M.cod_cuenta = I.cod_cuenta
            left join CntX_Cuentas G
                on M.COD_CONTABILIDAD = G.COD_CONTABILIDAD 
               and M.cod_cuenta_Gasto = G.cod_cuenta
            left join CntX_Unidades U
                on M.COD_CONTABILIDAD = U.COD_CONTABILIDAD 
               and M.cod_Unidad = U.cod_Unidad
            left join CntX_Centro_Costos Cc
                on M.COD_CONTABILIDAD = Cc.COD_CONTABILIDAD 
               and M.cod_Centro_Costo = Cc.Cod_Centro_Costo
            where M.cod_divisa = @codDivisa
              and M.COD_CONTABILIDAD = @codConta;";

            var result = DbHelper.ExecuteSingleQuery<CntXDivisaData>(
                _portalDb,
                codEmpresa,
                sql,
                new CntXDivisaData(),
                new { codDivisa, codConta }
            );

            if (result.Result == null)
            {
                result.Result = new CntXDivisaData();
            }

            return result;
        }

        /// <summary>
        /// Navegacion por scroll en el catálogo de divisas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="scrollCode"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<CntXDivisaData?> CntXDivisas_Scroll_Obtener(int CodEmpresa, int codConta, int scrollCode, string codDivisa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            try
            {
                const string query = @"
                    select Top 1 cod_divisa from Cntx_Divisas
                    WHERE cod_contabilidad = @codConta AND 
                          ((@scroll = 1 AND cod_divisa > @codDivisa)
                           OR (@scroll <> 1 AND cod_divisa < @codDivisa))
                    ORDER BY
                        CASE WHEN @scroll = 1 THEN cod_divisa END ASC,
                        CASE WHEN @scroll <> 1 THEN cod_divisa END DESC;";

                var divisa = conn.Query<string>(query, new { scroll = scrollCode, codConta, codDivisa }).FirstOrDefault();
                var divisaObjetivo = !string.IsNullOrEmpty(divisa) ? divisa : codDivisa;

                return CntXDivisas_Obtener(CodEmpresa, codConta, divisaObjetivo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CntXDivisaData?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el historial de diferenciales de una divisa 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXDivisaHistorialData>> CntXDivisas_Historial_Obtener(int codEmpresa, int codConta, string codDivisa)
            {
                const string query = @"select * from CntX_Divisas_historial 
                    WHERE COD_CONTABILIDAD = @codConta AND cod_divisa = @codDivisa";

                return DbHelper.ExecuteListQuery<CntXDivisaHistorialData>(
                    _portalDb,
                    codEmpresa,
                    query,
                    new { codConta, codDivisa }
                );
        }

        /// <summary>
        /// Obtiene tipos de cambio de una divisa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXDivisaTipoCambioData>> CntXDivisas_TipoCambio_Obtener(int codEmpresa, int codConta, string codDivisa)
        {
            const string query = @"select Top 100 * from CNTX_DIVISAS_TIPO_CAMBIO 
                    WHERE COD_CONTABILIDAD = @codConta AND cod_divisa = @codDivisa 
                    order by Corte Desc";

            return DbHelper.ExecuteListQuery<CntXDivisaTipoCambioData>(
                _portalDb,
                codEmpresa,
                query,
                new { codConta, codDivisa }
            );
        }

        /// <summary>
        /// Guarda la informacion de una divisa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="edita"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXDivisas_Guardar(int codEmpresa, string usuario, bool edita, CntXDivisaData request)
        {
            request.ctaing = request.ctaing.Replace("-", "").Trim();
            request.ctagst = request.ctagst.Replace("-", "").Trim();
            ErrorDto resp = edita
                ? ActualizarDivisa(codEmpresa, usuario, request)
                : InsertarDivisa(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            if (request.divisa_local)
            {
                resp = DesactivarOtrasDivisasLocales(codEmpresa, request.cod_contabilidad, request.cod_divisa);
                if (resp.Code < 0)
                    return resp;
            }

            return new ErrorDto { Code = 0, Description = "Información guardada satisfactoriamente..." };
        }

        /// <summary>
        /// Elimina una divisa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDivisa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CntXDivisas_Eliminar(int codEmpresa, int codConta, string codDivisa, string usuario)
        {
            const string sqlUpdate = @"delete CntX_Divisas 
                where COD_CONTABILIDAD = @CodConta AND cod_divisa = @CodDivisa;";

            var respUpdate = DbHelper.ExecuteNonQuery(
               _portalDb,
               codEmpresa,
               sqlUpdate,
               new
               {
                   CodConta = codConta,
                   CodDivisa = codDivisa
               }
           );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Divisa : {codDivisa} Conta.{codConta}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Actualizar divisa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarDivisa(int codEmpresa, string usuario, CntXDivisaData request)
        {
            const string sqlUpdate = @"
                UPDATE CntX_Divisas
                SET
                    descripcion       = @Descripcion,
                    observacion       = @Observacion,
                    cod_cuenta        = @CodCuenta,
                    cod_cuenta_gasto  = @CodCuentaGasto,
                    divisa_local      = @DivisaLocal,
                    CURRENCY_SIM      = @CurrencySim,
                    cod_Unidad        = @CodUnidad,
                    cod_Centro_Costo  = @CodCentroCosto
                WHERE COD_CONTABILIDAD = @CodConta
                  AND cod_divisa       = @CodDivisa;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    Descripcion = request.descripcion,
                    Observacion = request.observacion,
                    CodCuenta = request.ctaing,
                    CodCuentaGasto = request.ctagst,
                    DivisaLocal = request.divisa_local ? 1 : 0,
                    CurrencySim = request.currency_sim,
                    CodUnidad = request.cod_unidad,
                    CodCentroCosto = request.cod_centro_costo,
                    CodConta = request.cod_contabilidad,
                    CodDivisa = request.cod_divisa
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Divisa : {request.cod_divisa} Conta.{request.cod_contabilidad}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Insertar divisa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarDivisa(int codEmpresa, string usuario, CntXDivisaData request)
        {
            const string sqlInsert = @"
                INSERT INTO CntX_Divisas
                (COD_CONTABILIDAD, cod_divisa, descripcion, observacion, divisa_local, tc_compra,
                 tc_venta, consecutivo, cod_cuenta, cod_cuenta_gasto, CURRENCY_SIM, cod_Unidad, Cod_Centro_Costo)
                VALUES
                (@CodConta, @CodDivisa, @Descripcion, @Observacion, @DivisaLocal, @TcCompra,
                 @TcVenta, 0, @CodCuenta, @CodCuentaGasto, @CurrencySim, @CodUnidad, @CodCentroCosto);";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodConta = request.cod_contabilidad,
                    CodDivisa = request.cod_divisa,
                    Descripcion = request.descripcion,
                    Observacion = request.observacion,
                    DivisaLocal = request.divisa_local ? 1 : 0,
                    TcCompra = request.tc_compra,
                    TcVenta = request.tc_venta,
                    CodCuenta = request.ctaing,
                    CodCuentaGasto = request.ctagst,
                    CurrencySim = request.currency_sim,
                    CodUnidad = request.cod_unidad,
                    CodCentroCosto = request.cod_centro_costo
                }
            );

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Divisa : {request.cod_divisa} Conta.{request.cod_contabilidad}"
            );

            return respInsert;
        }

        /// <summary>
        /// Desactivar otra divisas locales
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        private ErrorDto DesactivarOtrasDivisasLocales(int codEmpresa, int codConta, string codDivisa)
        {
            const string sqlUnset = @"
                UPDATE CntX_Divisas 
                SET divisa_local = 0
                WHERE COD_CONTABILIDAD = @CodConta 
                  AND cod_divisa <> @CodDivisa;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUnset,
                new { CodConta = codConta, CodDivisa = codDivisa.Trim() }
            );
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
