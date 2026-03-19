using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXConAsientosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXConAsientosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config)
           )
        { }

        public FrmCntXConAsientosDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }



        /// <summary>
        /// Ejecuta una consulta segura sin transacción
        /// </summary>
        private ErrorDto<T> EjecutarSafe<T>(int codEmpresa, Func<SqlConnection, T> accion)
        {
            var response = new ErrorDto<T>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = accion(cn);
                response.Code = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Ejecuta una operación dentro de una transacción
        /// </summary>
        private ErrorDto<bool> EjecutarTransaccion(
            int codEmpresa,
            Func<SqlConnection, SqlTransaction, bool> accion)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Open();

                using var trx = cn.BeginTransaction();

                var result = accion(cn, trx);

                trx.Commit();

                response.Result = result;
                response.Code = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Lista las consolidaciones
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Consolidaciones_Listar(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                SELECT
                    COD_CONSOLIDA AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM CNTX_CONSOLIDA_DEFINICION
                ORDER BY COD_CONSOLIDA
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { }
            );
        }

        /// <summary>
        /// Busca asientos
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Asientos_Buscar(int codEmpresa, int codContabilidad, int? codConsolida)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_asiento) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM con_asientos
                WHERE cod_consolida = @cod_consolida
                ORDER BY cod_asiento
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_consolida = codConsolida }
            );
        }

        /// <summary>
        /// Obtiene las unidades
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return EjecutarSafe(codEmpresa, cn =>
                cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                        cod_unidad AS item,
                        descripcion
                      FROM CntX_Unidades
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY cod_unidad",
                    new { cod_contabilidad }
                ).ToList()
            );
        }

        /// <summary>
        /// Obtiene las divisas
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return EjecutarSafe(codEmpresa, cn =>
                cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                        cod_divisa AS item,
                        descripcion
                      FROM CntX_Divisas
                      WHERE cod_contabilidad = @cod_contabilidad
                      ORDER BY cod_divisa",
                    new { cod_contabilidad }
                ).ToList()
            );
        }

        /// <summary>
        /// Obtiene los centros de costo
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa, int cod_contabilidad, string codUnidad)
        {
            return EjecutarSafe(codEmpresa, cn =>
                cn.Query<DropDownListaGenericaModel>(
                    @"SELECT
                        C.cod_centro_costo AS item,
                        C.descripcion
                      FROM CntX_Centro_Costos C
                      INNER JOIN CntX_Unidades_CC U
                        ON C.cod_centro_costo = U.cod_centro_costo
                        AND C.cod_contabilidad = U.cod_contabilidad
                      WHERE C.cod_contabilidad = @cod_contabilidad
                        AND U.cod_unidad = @codUnidad
                      ORDER BY C.cod_centro_costo",
                    new { cod_contabilidad, codUnidad }
                ).ToList()
            );
        }

        /// <summary>
        /// Obtiene los asientos detalle
        /// </summary>
        public ErrorDto<List<CntxConAsientoDetalleDto>> AsientoDetalle_Obtener(
            int codEmpresa,
            int codContabilidad,
            int? codConsolida,
            string? codAsiento)
        {
            const string sql = @"
                SELECT 
                    A.cod_cuenta,
                    B.descripcion,
                    B.cod_unidad,
                    B.cod_centro_costo,
                    B.cod_divisa,
                    ISNULL(B.tipo_cambio, 1) AS tipo_cambio,
                    A.detalle AS documento,
                    A.debitos,
                    A.creditos,
                    A.linea
                FROM con_asientos_detalle A
                INNER JOIN cuentas B
                    ON A.cod_cuenta = B.cod_cuenta
                WHERE A.cod_consolida = @codConsolida
                AND A.cod_asiento = @codAsiento
                ORDER BY A.linea";

            return DbHelper.ExecuteListQuery<CntxConAsientoDetalleDto>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cod_consolida = codConsolida,
                    cod_asiento = codAsiento
                }
            );
        }

        /// <summary>
        /// Guarda el asiento
        /// </summary>
        public ErrorDto<bool> GuardarAsiento(CntxConAsientoGuardarDto request)
        {
            return EjecutarTransaccion(request.cod_empresa, (cn, trx) =>
            {

                if (request.es_edicion)
                {
                    const string update = @"
                        UPDATE con_asientos
                        SET descripcion = @descripcion,
                            fecha = @fecha
                        WHERE cod_consolida = @cod_consolida
                        AND cod_asiento = @cod_asiento";

                    cn.Execute(update, request, trx);

                    const string delete = @"
                        DELETE con_asientos_detalle
                        WHERE cod_consolida = @cod_consolida
                        AND cod_asiento = @cod_asiento";

                    cn.Execute(delete, request, trx);
                }
                else
                {
                    const string insert = @"
                        INSERT INTO con_asientos
                        (cod_consolida, cod_asiento, fecha, descripcion, aplicado)
                        VALUES
                        (@cod_consolida, @cod_asiento, @fecha, @descripcion, 'N')";

                    cn.Execute(insert, request, trx);
                }


                const string insertDetalle = @"
                    INSERT INTO con_asientos_detalle
                    (cod_asiento, cod_consolida, linea, cod_cuenta, detalle, debitos, creditos)
                    VALUES
                    (@cod_asiento, @cod_consolida, @linea, @cod_cuenta, @detalle, @debitos, @creditos)";

                int linea = 1;

                foreach (var d in request.detalle)
                {
                    if (string.IsNullOrWhiteSpace(d.cod_cuenta))
                        continue;

                    cn.Execute(insertDetalle, new
                    {
                        request.cod_asiento,
                        request.cod_consolida,
                        linea,
                        cod_cuenta = d.cod_cuenta,
                        detalle = d.detalle,
                        debitos = d.debitos,
                        creditos = d.creditos
                    }, trx);

                    linea++;
                }

                return true;
            });
        }

        /// <summary>
        /// Elimina el asiento
        /// </summary>
        public ErrorDto<bool> EliminarAsiento(
            int cod_empresa,
            int cod_contabilidad,
            int cod_consolida,
            string cod_asiento,
            string usuario)
        {
            return EjecutarTransaccion(cod_empresa, (cn, trx) =>
            {

                const string sqlValida = @"
                    SELECT aplicado
                    FROM con_asientos
                    WHERE cod_consolida = @cod_consolida
                    AND cod_asiento = @cod_asiento";

                var aplicado = cn.QueryFirstOrDefault<string>(
                    sqlValida,
                    new { cod_consolida, cod_asiento },
                    trx
                );

                if (aplicado == "S")
                    throw new Exception("Este asiento ya fue aplicado y no se puede eliminar");

 

                const string deleteDetalle = @"
                    DELETE con_asientos_detalle
                    WHERE cod_consolida = @cod_consolida
                    AND cod_asiento = @cod_asiento";

                cn.Execute(deleteDetalle, new { cod_consolida, cod_asiento }, trx);



                const string deleteCabecera = @"
                    DELETE con_asientos
                    WHERE cod_consolida = @cod_consolida
                    AND cod_asiento = @cod_asiento";

                cn.Execute(deleteCabecera, new { cod_consolida, cod_asiento }, trx);



                _mSecurityMainDb.Bitacora(
                    new Galileo.Models.Security.BitacoraInsertarDto
                    {
                        EmpresaId = cod_empresa,
                        Usuario = usuario,
                        Movimiento = "Elimina Asiento Consolidado",
                        DetalleMovimiento = $"Asiento: {cod_asiento} Consolida: {cod_consolida}"
                    }
                );

                return true;
            });
        }
    }
}