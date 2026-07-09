using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.DataBaseTier;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifFormasDePagoDB
    {
        private readonly PortalDB _portalDB;

        public FrmSifFormasDePagoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        private static string NormalizeUpper(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();

        private static string? BuildSearchLike(string? filtro)
        {
            var s = filtro?.Trim();
            return string.IsNullOrWhiteSpace(s) ? null : $"%{s}%";
        }

        private static DynamicParameters BuildFormaPagoParams(SifFormasPago forma_pago)
        {
            var p = new DynamicParameters();

            p.Add("@cod_forma_pago", NormalizeUpper(forma_pago.cod_forma_pago), DbType.String);
            p.Add("@descripcion", forma_pago.descripcion, DbType.String);
            p.Add("@activa", forma_pago.activa, DbType.Int32);
            p.Add("@efectivo", forma_pago.efectivo, DbType.Int32);
            p.Add("@aplica_saldos_favor", forma_pago.aplica_saldos_favor, DbType.Int32);
            p.Add("@cod_cuenta", forma_pago.cod_cuenta, DbType.String);
            p.Add("@tipo", forma_pago.tipo, DbType.String);
            p.Add("@aplica_para_deposito", forma_pago.aplica_para_deposito, DbType.Int32);
            p.Add("@maximo_apl", forma_pago.maximo_apl, DbType.Decimal);
            p.Add("@maximo_monto", forma_pago.maximo_monto, DbType.Decimal);
            p.Add("@or_aplica", forma_pago.or_aplica, DbType.Int32);
            p.Add("@or_diario_apl", forma_pago.or_diario_apl, DbType.Int32);
            p.Add("@or_diario_monto", forma_pago.or_diario_monto, DbType.Decimal);
            p.Add("@or_mensual_apl", forma_pago.or_mensual_apl, DbType.Int32);
            p.Add("@or_mensual_monto", forma_pago.or_mensual_monto, DbType.Decimal);
            p.Add("@codigo_fe", forma_pago.codigo_fe, DbType.String);
            p.Add("@recibo_digital", forma_pago.recibo_digital, DbType.Int32);
            p.Add("@registro_usuario", forma_pago.registro_usuario, DbType.String);

            return p;
        }

        private static ErrorDto ExecuteFormaPagoWrite(SqlConnection connection, string sql, DynamicParameters p, string okMsg)
        {
            try
            {
                connection.Execute(sql, p);
                return DbHelper.OkResponse(okMsg);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private const string InsertFormaPagoSql = @"
INSERT INTO sif_formas_pago (
    COD_FORMA_PAGO, DESCRIPCION, ACTIVA, EFECTIVO, APLICA_SALDOS_FAVOR, COD_CUENTA, TIPO, APLICA_PARA_DEPOSITO,
    MAXIMO_APL, MAXIMO_MONTO, OR_APLICA, OR_DIARIO_APL, OR_DIARIO_MONTO, OR_MENSUAL_APL, OR_MENSUAL_MONTO,
    CODIGO_FE, RECIBO_DIGITAL, REGISTRO_USUARIO, REGISTRO_FECHA
) VALUES (
    @cod_forma_pago, UPPER(LTRIM(RTRIM(@descripcion))), @activa, @efectivo, @aplica_saldos_favor, @cod_cuenta, @tipo, @aplica_para_deposito,
    @maximo_apl, @maximo_monto, @or_aplica, @or_diario_apl, @or_diario_monto, @or_mensual_apl, @or_mensual_monto,
    @codigo_fe, @recibo_digital, @registro_usuario, GETDATE()
)";

        private const string UpdateFormaPagoSql = @"
UPDATE sif_formas_pago SET
    DESCRIPCION = UPPER(LTRIM(RTRIM(@descripcion))),
    ACTIVA = @activa,
    EFECTIVO = @efectivo,
    APLICA_SALDOS_FAVOR = @aplica_saldos_favor,
    COD_CUENTA = @cod_cuenta,
    TIPO = @tipo,
    APLICA_PARA_DEPOSITO = @aplica_para_deposito,
    MAXIMO_APL = @maximo_apl,
    MAXIMO_MONTO = @maximo_monto,
    OR_APLICA = @or_aplica,
    OR_DIARIO_APL = @or_diario_apl,
    OR_DIARIO_MONTO = @or_diario_monto,
    OR_MENSUAL_APL = @or_mensual_apl,
    OR_MENSUAL_MONTO = @or_mensual_monto,
    CODIGO_FE = @codigo_fe,
    RECIBO_DIGITAL = @recibo_digital,
    REGISTRO_USUARIO = @registro_usuario,
    REGISTRO_FECHA = GETDATE()
WHERE UPPER(COD_FORMA_PAGO) = @cod_forma_pago";

        private ErrorDto<T?> Single<T>(int codEmpresa, string sql, T? defaultValue = default, object? parameters = null)
            => DbHelper.ExecuteSingleQuery<T>(_portalDB, codEmpresa, sql, defaultValue, parameters);

        private ErrorDto<List<T>> List<T>(int codEmpresa, string sql, object? parameters = null)
            => DbHelper.ExecuteListQuery<T>(_portalDB, codEmpresa, sql, parameters);

        private ErrorDto WithEmpresaConn(int codEmpresa, Func<SqlConnection, ErrorDto> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);
            if ((r.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(r.Description ?? "Error inesperado", r.Code ?? -1);

            return r.Result ?? DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Obtiene la forma de pago por código
        /// </summary>
        public ErrorDto<SifFormasPago> SIF_Formas_Pago_Obtener(int codEmpresa, string codFormaPago)
        {
            var query = @"SELECT *
                          FROM vSys_Formas_Pago
                          WHERE COD_FORMA_PAGO = @codFormaPago";

            var r = Single<SifFormasPago>(codEmpresa, query, default, new { codFormaPago });
            if ((r.Code ?? -1) != 0)
                return new ErrorDto<SifFormasPago> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<SifFormasPago> { Code = 0, Description = "Ok", Result = r.Result };
        }

        /// <summary>
        /// Obtiene el siguiente o anterior código de forma de pago según el orden.
        /// </summary>
        public ErrorDto<string> SIF_Formas_Pago_Obtener_SigAnt(int codEmpresa, string? codFormaPagoActual, string orden)
        {
            var ord = (orden ?? string.Empty).Trim().ToLowerInvariant();
            var actual = (codFormaPagoActual ?? string.Empty).Trim();

            string sql;
            object? prms = null;

            if (ord == "asc")
            {
                if (string.IsNullOrEmpty(actual))
                {
                    sql = @"SELECT TOP 1 COD_FORMA_PAGO FROM sif_formas_pago ORDER BY COD_FORMA_PAGO ASC";
                }
                else
                {
                    sql = @"SELECT TOP 1 COD_FORMA_PAGO
                            FROM sif_formas_pago
                            WHERE COD_FORMA_PAGO > @codFormaPagoActual
                            ORDER BY COD_FORMA_PAGO ASC";
                    prms = new { codFormaPagoActual = actual };
                }
            }
            else if (ord == "desc")
            {
                if (string.IsNullOrEmpty(actual))
                {
                    sql = @"SELECT TOP 1 COD_FORMA_PAGO FROM sif_formas_pago ORDER BY COD_FORMA_PAGO DESC";
                }
                else
                {
                    sql = @"SELECT TOP 1 COD_FORMA_PAGO
                            FROM sif_formas_pago
                            WHERE COD_FORMA_PAGO < @codFormaPagoActual
                            ORDER BY COD_FORMA_PAGO DESC";
                    prms = new { codFormaPagoActual = actual };
                }
            }
            else
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = "Parámetro 'orden' inválido. Debe ser 'asc' o 'desc'.",
                    Result = null
                };
            }

            var r = Single<string>(codEmpresa, sql, default, prms);
            if ((r.Code ?? -1) != 0)
                return new ErrorDto<string> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<string> { Code = 0, Description = "Ok", Result = r.Result };
        }

        private static bool FormaPagoExiste(SqlConnection connection, string codFormaPago)
        {
            const string q = @"SELECT ISNULL(COUNT(*),0) FROM sif_formas_pago WHERE COD_FORMA_PAGO = @codFormaPago";
            return connection.QueryFirstOrDefault<int>(q, new { codFormaPago }) > 0;
        }

        /// <summary>
        /// Inserta o actualiza una forma de pago.
        /// </summary>
        public ErrorDto SIF_Formas_Pago_Guardar(int codEmpresa, SifFormasPago forma_pago)
        {
            if (forma_pago == null)
                return DbHelper.ErrorResponse("Datos inválidos");

            return WithEmpresaConn(codEmpresa, connection =>
            {
                connection.Open();

                var codigo = NormalizeUpper(forma_pago.cod_forma_pago);
                var existe = FormaPagoExiste(connection, codigo);

                return existe
                    ? SIF_Formas_Pago_Actualizar(connection, forma_pago)
                    : SIF_Formas_Pago_Insertar(connection, forma_pago);
            });
        }

        private static ErrorDto SIF_Formas_Pago_Insertar(SqlConnection connection, SifFormasPago forma_pago)
        {
            var p = BuildFormaPagoParams(forma_pago);
            return ExecuteFormaPagoWrite(connection, InsertFormaPagoSql, p, "Forma de pago registrada correctamente.");
        }

        private static ErrorDto SIF_Formas_Pago_Actualizar(SqlConnection connection, SifFormasPago forma_pago)
        {
            var p = BuildFormaPagoParams(forma_pago);
            return ExecuteFormaPagoWrite(connection, UpdateFormaPagoSql, p, "Forma de pago actualizada correctamente.");
        }

        /// <summary>
        /// Obtiene formas de pago con base en filtros.
        /// </summary>
        public ErrorDto<List<SifFormasPagoList>> SIF_Formas_Pago_Obtener_Lista(int codEmpresa, string? filtro)
        {
            var searchLike = BuildSearchLike(filtro);

            const string query = @"SELECT COD_FORMA_PAGO, DESCRIPCION
                                   FROM vSys_Formas_Pago
                                   WHERE (@search IS NULL
                                          OR COD_FORMA_PAGO LIKE @search
                                          OR DESCRIPCION LIKE @search)";

            var r = List<SifFormasPagoList>(codEmpresa, query, new { search = searchLike });
            if ((r.Code ?? -1) != 0)
                return new ErrorDto<List<SifFormasPagoList>> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<List<SifFormasPagoList>> { Code = 0, Description = "Ok", Result = r.Result };
        }

        /// <summary>
        /// Obtiene la lista de cuentas bancarias segun la forma de pago
        /// </summary>
        public List<SysCuentasBancariasList> CuentasBancarias_Obtener_Lista(int CodEmpresa, string codFormaPago)
        {
            var r = DbHelper.WithConn(_portalDB, CodEmpresa, cn =>
            {
                var query = @"
                    select 
                      Ban.ID_BANCO,
                      Ban.DESCRIPCION,
                      Ban.Cta,
                      isnull(Fp.Id_Banco,0)      as Idx,
                      Ban.Cod_Divisa,
                      isnull(Eb.DESCRIPCION,'')  as Entidad_Desc
                    from TES_BANCOS Ban
                    left join SIF_FORMAS_PAGO_BANCOS_ASG Fp
                      on Ban.ID_BANCO = Fp.id_banco
                     and Fp.cod_forma_pago = @codFormaPago
                    left join TES_BANCOS_GRUPOS Eb
                      on Ban.Cod_Grupo = Eb.Cod_Grupo
                    where Ban.ESTADO = 'A'
                    order by Fp.id_Banco desc, Ban.ID_BANCO asc;";

                return cn.Query<SysCuentasBancariasList>(query, new { codFormaPago }).ToList();
            });

            return (r.Code ?? -1) == 0 && r.Result != null ? r.Result : new List<SysCuentasBancariasList>();
        }

        /// <summary>
        /// Asigna o elimina cuentas bancarias segun la forma de pago
        /// </summary>
        public ErrorDto CuentasBancarias_Asignar(int codEmpresa, SifFormasPagoBancoAsgDto data)
        {
            if (data == null)
                return DbHelper.ErrorResponse("Datos inválidos");

            return WithEmpresaConn(codEmpresa, connection =>
            {
                const string queryExiste = @"SELECT COUNT(*) FROM SIF_FORMAS_PAGO_BANCOS_ASG WHERE id_banco = @IdBanco AND cod_forma_pago = @CodFormaPago";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { data.IdBanco, data.CodFormaPago });

                if (existe > 0)
                {
                    const string queryDelete = @"DELETE SIF_FORMAS_PAGO_BANCOS_ASG WHERE id_banco = @IdBanco AND cod_forma_pago = @CodFormaPago";
                    connection.Execute(queryDelete, new { data.IdBanco, data.CodFormaPago });
                    return DbHelper.OkResponse("Eliminado correctamente.");
                }

                const string queryInsert = @"INSERT SIF_FORMAS_PAGO_BANCOS_ASG (id_banco, cod_forma_pago, registro_fecha, registro_usuario)
                                            VALUES (@IdBanco, @CodFormaPago, dbo.MyGetdate(), @RegistroUsuario)";
                connection.Execute(queryInsert, new { data.IdBanco, data.CodFormaPago, data.RegistroUsuario });
                return DbHelper.OkResponse("Insertado correctamente.");
            });
        }
    }
}