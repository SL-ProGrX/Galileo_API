using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data.Common;

namespace Galileo.DataBaseTier
{
    public partial class MTesFuncionesDb
    {
        public long fxgTesoreriaMaestro(int CodEmpresa, string usuario, TesoreriaMaestroModel tesoreria)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var detalle1 = Trunc(tesoreria.vDetalle1, 26);
                var detalle2 = Trunc(tesoreria.vDetalle2, 26);

                // Insert + retorno del ID insertado (evita MAX(nsolicitud))
                const string sqlInsertCk = @"
                        INSERT INTO Tes_Transacciones (
                            id_banco, tipo, codigo, beneficiario, monto, fecha_solicitud,
                            estado, estadoi, modulo, submodulo, cta_ahorros, detalle1, detalle2,
                            referencia, op, genera, actualiza, cod_unidad, cod_concepto,
                            user_solicita, autoriza, fecha_autorizacion, user_autoriza,
                            ref_01, ref_02, ref_03, cod_app, ID_TOKEN, REMESA_TIPO, REMESA_ID
                        ) VALUES (
                            @Banco, @TipoDocumento, @Codigo, @Beneficiario, @Monto, @Fecha,
                            'P', 'P', 'CC', 'C', @Cuenta, @Detalle1, @Detalle2,
                            @Referencia, @OP, 'S', 'S', @Unidad, @Concepto,
                            @Usuario, 'S', GETDATE(), @Usuario,
                            @Ref01, @Ref02, @Ref03, @CodApp, @Token, @RemesaTipo, @RemesaId
                        );
                        SELECT CAST(SCOPE_IDENTITY() as bigint);";

                                        const string sqlInsertNoCk = @"
                        INSERT INTO Tes_Transacciones (
                            id_banco, tipo, codigo, beneficiario, monto, fecha_solicitud,
                            estado, estadoi, modulo, submodulo, cta_ahorros, detalle1, detalle2,
                            referencia, op, genera, actualiza, cod_unidad, cod_concepto,
                            ref_01, ref_02, ref_03, cod_app, ID_TOKEN, REMESA_TIPO, REMESA_ID,
                            user_solicita
                        ) VALUES (
                            @Banco, @TipoDocumento, @Codigo, @Beneficiario, @Monto, @Fecha,
                            'P', 'P', 'CC', 'C', @Cuenta, @Detalle1, @Detalle2,
                            @Referencia, @OP, 'S', 'S', @Unidad, @Concepto,
                            @Ref01, @Ref02, @Ref03, @CodApp, @Token, @RemesaTipo, @RemesaId,
                            @Usuario
                        );
                        SELECT CAST(SCOPE_IDENTITY() as bigint);";

                var args = new
                {
                    Banco = tesoreria.vBanco,
                    TipoDocumento = tesoreria.vTipoDocumento,
                    Codigo = tesoreria.vCodigo,
                    Beneficiario = tesoreria.vBeneficiario,
                    Monto = tesoreria.vMonto,
                    Fecha = tesoreria.vFecha,
                    Cuenta = tesoreria.vCuenta,
                    Detalle1 = detalle1,
                    Detalle2 = detalle2,
                    Referencia = tesoreria.vReferencia,
                    OP = tesoreria.vOP,
                    Unidad = tesoreria.vUnidad,
                    Concepto = tesoreria.vConcepto,
                    Usuario = usuario,
                    Ref01 = tesoreria.vRef_01,
                    Ref02 = tesoreria.vRef_02,
                    Ref03 = tesoreria.vRef_03,
                    CodApp = tesoreria.vCodApp,
                    Token = tesoreria.vToken,
                    RemesaTipo = tesoreria.vRemesaTipo,
                    RemesaId = tesoreria.vRemesa
                };

                var isCk = tesoreria.vTipoDocumento.Equals("CK", StringComparison.OrdinalIgnoreCase);
                var nsolicitud = connection.QuerySingle<long>(isCk ? sqlInsertCk : sqlInsertNoCk, args);

                // Validación de consistencia (opcional, pero mantiene tu lógica)
                const string sqlCheck = @"SELECT TOP 1 * FROM Tes_Transacciones WHERE nsolicitud = @Nsolicitud;";
                var row = connection.QueryFirstOrDefault<TesTransaccionesDto>(sqlCheck, new { Nsolicitud = nsolicitud });

                if (row != null && string.Equals(row.CODIGO?.Trim(), tesoreria.vCodigo?.Trim(), StringComparison.Ordinal))
                    return nsolicitud;

                // Fallback (si por alguna razón no coincidiera)
                const string sqlFallback = @"
SELECT TOP 1 CAST(nsolicitud as bigint)
FROM Tes_Transacciones
WHERE codigo = @Codigo AND op = @OP
ORDER BY nsolicitud DESC;";

                return connection.QueryFirstOrDefault<long>(sqlFallback, new { Codigo = tesoreria.vCodigo, OP = tesoreria.vOP });
            }
            catch
            {
                return 0;
            }
        }

        public void sbgTesoreriaDetalle(int CodEmpresa, TesoreriaDetalleModel detalle)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
INSERT INTO Tes_Trans_Asiento (
    nsolicitud, cuenta_contable, monto, debehaber, linea, cod_unidad, cod_cc
) VALUES (
    @Solicitud, @CtaConta, @Monto, @DH, @Linea, @Unidad, @CC
);";

                connection.Execute(sql, new
                {
                    Solicitud = detalle.vSolicitud,
                    CtaConta = detalle.vCtaConta,
                    Monto = detalle.vMonto,
                    DH = detalle.vDH,
                    Linea = detalle.vLinea,
                    Unidad = detalle.vUnidad,
                    CC = detalle.vCC
                });
            }
            catch
            {
                // ideal: log
            }
        }

        public static string fxTipoDocumento(string tipo)
        {
            return tipo switch
            {
                "CK" => "Cheque",
                "TE" => "Transferencia",
                "EF" or "RE" => "Efectivo",
                "ND" => "Nota Debito",
                "NC" => "Nota Credito",
                "OT" => "Otro...",
                "CD" => "Ctrl Desembolsos",
                "CP" => "Proveedor",
                "RC" => "Retiro en Caja",
                "FD" => "Fondo Transitorio",
                "TS" => "Transferencia SINPE",

                "Cheque" => "CK",
                "Transferencia" => "TE",
                "Efectivo" => "EF",
                "Nota Debito" => "ND",
                "Nota Credito" => "NC",
                "Otro..." => "OT",
                "Ctrl Desembolsos" => "CD",
                "Proveedor" => "CP",
                "Retiro en Caja" => "RC",
                "Fondo Transitorio" => "FD",
                "Transferencia SINPE" => "TS",

                _ => string.Empty
            };
        }

        public string fxTesToken(int CodEmpresa, string usuario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var prefix = DateTime.Now.ToString("yyyy.MM.dd");

                const string sqlConsec = @"
SELECT ISNULL(COUNT(id_token),0) + 1
FROM tes_tokens
WHERE id_token LIKE @PrefixLike;";

                var consec = connection.QuerySingle<int>(sqlConsec, new { PrefixLike = prefix + "%" });
                var token = $"{prefix}{consec}";

                const string sqlInsert = @"
INSERT tes_tokens (id_token, registro_fecha, registro_usuario, estado)
VALUES (@Token, GETDATE(), @Usuario, 'A');";

                connection.Execute(sqlInsert, new { Token = token, Usuario = usuario });

                return token;
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool fxgTESValidaDatos(int CodEmpresa, int Contabilidad, string vTipo, string vCodigo, string vFiltro = "")
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                string sql = vTipo.ToUpperInvariant() switch
                {
                    "CONCEPTO" => @"
SELECT ISNULL(COUNT(*),0)
FROM tes_conceptos
WHERE cod_concepto = @Codigo AND Estado = 'A';",

                    "UNIDAD" => @"
SELECT ISNULL(COUNT(*),0)
FROM CntX_unidades
WHERE cod_unidad = @Codigo AND Activa = 1 AND cod_Contabilidad = @Contabilidad;",

                    "CC" => @"
SELECT ISNULL(COUNT(*),0)
FROM CNTX_CENTRO_COSTOS
WHERE COD_CENTRO_COSTO = @Codigo
  AND Activo = 1
  AND cod_contabilidad = @Contabilidad
  AND (
        @Filtro = '' OR COD_CENTRO_COSTO IN (
            SELECT COD_CENTRO_COSTO
            FROM CNTX_UNIDADES_CC
            WHERE cod_unidad = @Filtro AND cod_contabilidad = @Contabilidad
        )
  );",

                    _ => ""
                };

                if (string.IsNullOrWhiteSpace(sql)) return false;

                var existe = connection.QuerySingle<int>(sql, new
                {
                    Codigo = vCodigo,
                    Contabilidad,
                    Filtro = vFiltro ?? ""
                });

                return existe > 0;
            }
            catch
            {
                return false;
            }
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbgTESBusqueda(int CodEmpresa, int Contabilidad, string vTipo, string vFiltro = "")
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                string sql = vTipo.ToUpperInvariant() switch
                {
                    "CONCEPTO" => @"
SELECT cod_concepto as item, descripcion
FROM tes_conceptos
WHERE Estado = 'A'
ORDER BY cod_concepto;",

                    "UNIDAD" => @"
SELECT cod_unidad as item, descripcion
FROM CntX_unidades
WHERE Activa = 1 AND cod_Contabilidad = @Contabilidad
ORDER BY cod_unidad;",

                    "CC" => @"
SELECT COD_CENTRO_COSTO as item, descripcion
FROM CNTX_CENTRO_COSTOS
WHERE Activo = 1
  AND cod_contabilidad = @Contabilidad
  AND (
        @Filtro = '' OR COD_CENTRO_COSTO IN (
            SELECT COD_CENTRO_COSTO
            FROM CNTX_UNIDADES_CC
            WHERE cod_unidad = @Filtro AND cod_contabilidad = @Contabilidad
        )
  )
ORDER BY COD_CENTRO_COSTO;",

                    _ => ""
                };

                if (string.IsNullOrWhiteSpace(sql))
                {
                    response.Code = -1;
                    response.Description = $"Tipo no soportado: {vTipo}";
                    response.Result = null;
                    return response;
                }

                response.Result = connection.Query<DropDownListaGenericaModel>(sql, new
                {
                    Contabilidad,
                    Filtro = vFiltro ?? ""
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
                return response;
            }
        }
    }
}
