using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprProveedoresDB
    {
        private readonly PortalDB _portalDB;

        // Evitar literales repetidos (Sonar)
        private const string ParamOff = "off";
        private const string ParamTake = "take";

        private static ErrorDto Fail(Exception ex) => DbHelper.ErrorResponse(ex.Message, -1);

        private static ErrorDto<T> Fail<T>(Exception ex, T? fallback = default)
            => new ErrorDto<T> { Code = -1, Description = ex.Message, Result = fallback };

        private static string? NormalizeLike(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return null;

            var f = filtro.Trim();
            return f.Length == 0 ? null : $"%{f}%";
        }

        private static (int Off, int Take) NormalizePaging(int? pagina, int? paginacion)
        {
            if (pagina is null || paginacion is null || pagina < 0 || paginacion <= 0)
                return (0, int.MaxValue);

            return (pagina.Value, paginacion.Value);
        }

        public FrmCprProveedoresDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        // Helper: usa DbHelper.WithConn y devuelve ErrorDto plano (sin ErrorDto<ErrorDto>)
        private ErrorDto WithConn(int codEmpresa, Func<SqlConnection, ErrorDto> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);
            return r.Code == 0
                ? (r.Result ?? DbHelper.ErrorResponse("Error desconocido.", -1))
                : DbHelper.ErrorResponse(r.Description ?? "Error desconocido.", -1);
        }

        // Helper: igual pero para resultados tipados
        private ErrorDto<T> WithConn<T>(int codEmpresa, Func<SqlConnection, T> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);
            return r.Code == 0
                ? new ErrorDto<T> { Code = 0, Description = "Ok", Result = r.Result }
                : new ErrorDto<T> { Code = -1, Description = r.Description, Result = default };
        }

        public ErrorDto CprProveedores_Importar(int CodEmpresa)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    conn.Execute("exec spCPR_Proveedores_Importar;");
                    return DbHelper.OkResponse("Proveedores Sincronizados/Importados Satisfactoriamente!");
                });
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        public ErrorDto<CprProveedoresDto> CprProveedor_Scroll(int CodEmpresa, int scroll, string? codigo)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    if (string.IsNullOrWhiteSpace(codigo))
                        throw new InvalidOperationException("Debe indicar el código para realizar el scroll.");

                    const string sqlAsc = @"SELECT TOP 1 * FROM CPR_PROVEEDORES_TEMPO WHERE PROVEEDOR_CODIGO > @Codigo ORDER BY PROVEEDOR_CODIGO ASC;";
                    const string sqlDesc = @"SELECT TOP 1 * FROM CPR_PROVEEDORES_TEMPO WHERE PROVEEDOR_CODIGO < @Codigo ORDER BY PROVEEDOR_CODIGO DESC;";

                    var sql = (scroll == 1) ? sqlAsc : sqlDesc;

                    var result = conn.QueryFirstOrDefault<CprProveedoresDto>(sql, new { Codigo = codigo });

                    // ✅ Sin throw System.Exception (quita S112)
                    if (result == null)
                        throw new InvalidOperationException("No se encontró proveedor para el scroll solicitado.");

                    return result;
                });
            }
            catch (Exception ex)
            {
                return Fail<CprProveedoresDto>(ex);
            }
        }

        public ErrorDto<CprProveedoresLista> CprProveedoresLista_Obtener(int CodEmpresa, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<CprProveedoresFiltros>(filtros) ?? new CprProveedoresFiltros();

            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    var like = NormalizeLike(filtro.filtro);
                    var (off, take) = NormalizePaging(filtro.pagina, filtro.paginacion);

                    var dp = new DynamicParameters();
                    dp.Add("q", like);
                    dp.Add(ParamOff, off);
                    dp.Add(ParamTake, take);

                    var result = new CprProveedoresLista();

                    const string countSql = @"SELECT COUNT(*)
FROM CPR_PROVEEDORES_TEMPO
WHERE (@q IS NULL OR PROVEEDOR_CODIGO LIKE @q OR CEDJUR LIKE @q OR DESCRIPCION LIKE @q);";

                    result.total = conn.ExecuteScalar<int>(countSql, dp);

                    const string dataSql = @"SELECT PROVEEDOR_CODIGO, CEDJUR, DESCRIPCION
FROM CPR_PROVEEDORES_TEMPO
WHERE (@q IS NULL OR PROVEEDOR_CODIGO LIKE @q OR CEDJUR LIKE @q OR DESCRIPCION LIKE @q)
ORDER BY PROVEEDOR_CODIGO
OFFSET @off ROWS FETCH NEXT @take ROWS ONLY;";

                    result.proveedores = conn.Query<CprProveedoresDto>(dataSql, dp).ToList();
                    return result;
                });
            }
            catch (Exception ex)
            {
                return Fail<CprProveedoresLista>(ex);
            }
        }

        public ErrorDto<CprProveedoresDto> CprProveedores_Obtener(int CodEmpresa, string codigo)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string sql = @"SELECT P.* FROM CPR_PROVEEDORES_TEMPO P WHERE P.PROVEEDOR_CODIGO = @Codigo;";
                    var result = conn.QueryFirstOrDefault<CprProveedoresDto>(sql, new { Codigo = codigo });
                    return result ?? new CprProveedoresDto();
                });
            }
            catch (Exception ex)
            {
                return Fail(ex, new CprProveedoresDto());
            }
        }

        public ErrorDto CprProveedores_Guardar(int CodEmpresa, bool vEdita, CprProveedoresDto proveedor)
        {
            try
            {
                if (!vEdita)
                {
                    var errores = ValidarProveedorNuevo(CodEmpresa, proveedor);
                    if (!string.IsNullOrWhiteSpace(errores))
                        return DbHelper.ErrorResponse(errores.Trim(), -1);

                    return CprProveedores_Insertar(CodEmpresa, proveedor);
                }

                return CprProveedores_Actualizar(CodEmpresa, proveedor);
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        private string ValidarProveedorNuevo(int codEmpresa, CprProveedoresDto proveedor)
        {
            var sb = new System.Text.StringBuilder();

            if (string.IsNullOrWhiteSpace(proveedor.descripcion))
                sb.AppendLine(" - Nombre del Proveedor no es válido.");

            if (!MProGrXAuxiliarDB.fxCorreoValido(proveedor.email ?? string.Empty))
                sb.AppendLine(" - El Email principal no es válido.");

            try
            {
                var resp = WithConn(codEmpresa, conn =>
                {
                    var ced = (proveedor.cedjur ?? "").Replace("-", "").Replace(" ", "");

                    const string sql = @"
                        SELECT ISNULL(COUNT(*),0)
                        FROM CXP_PROVEEDORES
                        WHERE ( @cod_proveedor > 0 AND COD_PROVEEDOR = @cod_proveedor )
                           OR REPLACE(REPLACE(CEDJUR,' ',''),'-','') = @cedjur;";

                    var existe = conn.ExecuteScalar<int>(sql, new
                    {
                        cod_proveedor = proveedor.proveedor_codigo,
                        cedjur = ced
                    });

                    if (existe > 0)
                        return DbHelper.ErrorResponse(" - Existe ya un Proveedor registrado con la misma Cédula Jurídica.", -1);

                    return DbHelper.OkResponse("Ok");
                });

                if (resp.Code != 0 && !string.IsNullOrWhiteSpace(resp.Description))
                    sb.AppendLine(resp.Description);
            }
            catch
            {
                // no bloquear por fallo de validación
            }

            return sb.ToString();
        }

        private ErrorDto CprProveedores_Insertar(int CodEmpresa, CprProveedoresDto proveedor)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    using var tx = conn.BeginTransaction();

                    const string nextSql = @"SELECT ISNULL(MAX(COD_PROVEEDOR), 10000) + 1 FROM CXP_PROVEEDORES;";
                    proveedor.proveedor_codigo = conn.ExecuteScalar<int>(nextSql, transaction: tx);

                    const string insTempo = @"
                        INSERT INTO CPR_PROVEEDORES_TEMPO
                        (
                            PROVEEDOR_CODIGO, TIPO, CEDJUR, DESCRIPCION, OBSERVACION,
                            TELEFONO, EMAIL, ESTADO, COD_PROVEEDOR, REGISTRO_FECHA, REGISTRO_USUARIO
                        )
                        VALUES
                        (
                            @proveedor_codigo, @tipo, @cedjur, @descripcion, @observacion,
                            @telefono, @email, @estado, @proveedor_codigo, GETDATE(), @registro_usuario
                        );";

                    conn.Execute(insTempo, new
                    {
                        proveedor.proveedor_codigo,
                        proveedor.tipo,
                        proveedor.cedjur,
                        proveedor.descripcion,
                        proveedor.observacion,
                        proveedor.telefono,
                        proveedor.email,
                        proveedor.estado,
                        proveedor.registro_usuario
                    }, tx);

                    const string syncCxp = @"
                        INSERT INTO CXP_PROVEEDORES
                        (
                            COD_PROVEEDOR, COD_CLASIFICACION, TIPO,
                            CEDJUR, DESCRIPCION, OBSERVACION,
                            ESTADO, TELEFONO, EMAIL,
                            REGISTRO_FECHA, REGISTRO_USUARIO,
                            CREDITO_PLAZO, CREDITO_MONTO, DESCUENTO_PORC, SALDO
                        )
                        SELECT
                            P.COD_PROVEEDOR,
                            (SELECT TOP 1 COD_CLASIFICACION FROM CXP_PROV_CLAS WHERE ACTIVO = 1),
                            P.TIPO,
                            P.CEDJUR, P.DESCRIPCION, P.OBSERVACION,
                            P.ESTADO, P.TELEFONO, P.EMAIL,
                            GETDATE(), P.REGISTRO_USUARIO,
                            0, 0, 0, 0
                        FROM CPR_PROVEEDORES_TEMPO P
                        WHERE P.COD_PROVEEDOR = @cod_proveedor
                          AND NOT EXISTS (SELECT 1 FROM CXP_PROVEEDORES T2 WHERE T2.COD_PROVEEDOR = P.COD_PROVEEDOR);";

                    conn.Execute(syncCxp, new { cod_proveedor = proveedor.proveedor_codigo }, tx);

                    tx.Commit();
                    return new ErrorDto { Code = 0, Description = proveedor.proveedor_codigo.ToString() };
                });
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        private ErrorDto CprProveedores_Actualizar(int CodEmpresa, CprProveedoresDto proveedor)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string sql = @"
                        UPDATE CPR_PROVEEDORES_TEMPO
                           SET DESCRIPCION = @descripcion,
                               CEDJUR = @cedjur,
                               TIPO = @tipo,
                               OBSERVACION = @observacion,
                               ESTADO = @estado,
                               EMAIL = @email,
                               TELEFONO = @telefono,
                               MODIFICA_FECHA = GETDATE(),
                               MODIFICA_USUARIO = @modifica_usuario
                         WHERE PROVEEDOR_CODIGO = @proveedor_codigo;";

                    conn.Execute(sql, new
                    {
                        proveedor.descripcion,
                        proveedor.cedjur,
                        proveedor.tipo,
                        proveedor.observacion,
                        proveedor.estado,
                        proveedor.email,
                        proveedor.telefono,
                        proveedor.modifica_usuario,
                        proveedor_codigo = proveedor.proveedor_codigo
                    });

                    return new ErrorDto { Code = 0, Description = proveedor.proveedor_codigo.ToString() };
                });
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        public ErrorDto CprProveedores_Eliminar(int CodEmpresa, string codigo)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string sql = @"DELETE CPR_PROVEEDORES_TEMPO WHERE PROVEEDOR_CODIGO = @Codigo;";
                    conn.Execute(sql, new { Codigo = codigo });
                    return DbHelper.OkResponse("Proveedor eliminado correctamente");
                });
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        public ErrorDto<float> CprProveedorPuntaje_Obtener(int CodEmpresa, string codigo)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string sql = @"
                        SELECT AVG(V.VALORA_PUNTAJE) AS VALORA_PUNTAJE
                        FROM CPR_SOLICITUD_PROV V
                        WHERE V.PROVEEDOR_CODIGO = @Codigo
                        GROUP BY V.PROVEEDOR_CODIGO;";

                    return conn.QueryFirstOrDefault<float>(sql, new { Codigo = codigo });
                });
            }
            catch (Exception ex)
            {
                return Fail<float>(ex, 0);
            }
        }

        public ErrorDto<List<CprProveedorBitacoraData>> CprProveedoreBitacoraPuntaje(int CodEmpresa, string codigo)
        {
            try
            {
                return WithConn(CodEmpresa, conn =>
                {
                    const string sql = @"
                        SELECT V.CPR_ID, V.ESTADO, V.VALORA_FECHA, V.VALORA_USUARIO, V.VALORA_PUNTAJE
                        FROM CPR_SOLICITUD_PROV V
                        LEFT JOIN CPR_PROVEEDORES_TEMPO P ON V.PROVEEDOR_CODIGO = P.COD_PROVEEDOR
                        WHERE V.PROVEEDOR_CODIGO = @Codigo
                        ORDER BY V.PROVEEDOR_CODIGO DESC;";

                    return conn.Query<CprProveedorBitacoraData>(sql, new { Codigo = codigo }).ToList();
                });
            }
            catch (Exception ex)
            {
                return Fail<List<CprProveedorBitacoraData>>(ex);
            }
        }
    }
}