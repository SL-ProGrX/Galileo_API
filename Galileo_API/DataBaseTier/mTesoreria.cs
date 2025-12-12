using System.Data;
using System.Text;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class MTesoreria
    {
        private readonly PortalDB _portalDb;
        private readonly string _dirRDLC;

        public MTesoreria(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dirRDLC = config.GetSection("AppSettings").GetSection("RutaRDLC").Value ?? string.Empty;
        }

        // ------------------ LISTAS / QUERIES SIMPLES ------------------

        public ErrorDto<List<DropDownListaGenericaModel>> tes_TiposDocumentos_Obtener(int codEmpresa)
        {
            const string sql = @"SELECT TIPO AS ITEM, DESCRIPCION FROM tes_tipos_doc;";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCbo(int codEmpresa, string usuario, int banco, int contabilidad)
        {
            const string sql = @"
SELECT RTRIM(C.cod_unidad) AS item, RTRIM(C.descripcion) AS descripcion
FROM tes_unidad_ASG A
INNER JOIN CntX_Unidades C ON A.cod_unidad = C.cod_unidad AND C.cod_contabilidad = @Contabilidad
WHERE A.id_Banco = @Banco AND A.nombre = @Usuario AND activa = 1
ORDER BY C.Descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb, codEmpresa, sql,
                new { Contabilidad = contabilidad, Banco = banco, Usuario = usuario }
            );
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCbo(int codEmpresa, string usuario, int banco)
        {
            const string sql = @"
SELECT RTRIM(C.cod_Concepto) AS item, RTRIM(C.Descripcion) AS descripcion
FROM tes_conceptos_ASG A
INNER JOIN Tes_Conceptos C ON A.cod_concepto = C.cod_concepto
WHERE A.id_Banco = @Banco AND A.nombre = @Usuario AND estado = 'A'
ORDER BY C.Descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb, codEmpresa, sql,
                new { Banco = banco, Usuario = usuario }
            );
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int codEmpresa)
        {
            const string sql = @"
SELECT id_banco AS item, descripcion
FROM Tes_Bancos
WHERE Estado = 'A'
ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCboGeneral(int codEmpresa, int contabilidad)
        {
            const string sql = @"
SELECT RTRIM(cod_unidad) AS item, RTRIM(descripcion) AS descripcion
FROM CntX_Unidades
WHERE cod_contabilidad = @Contabilidad;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { Contabilidad = contabilidad });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCbo(int codEmpresa, int banco)
        {
            const string sql = @"
SELECT T.Tipo AS item, RTRIM(T.Descripcion) AS descripcion
FROM tes_banco_docs A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
WHERE A.id_Banco = @Banco
ORDER BY T.Descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { Banco = banco });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCboGeneral(int codEmpresa)
        {
            const string sql = @"SELECT RTRIM(cod_Concepto) AS item, RTRIM(Descripcion) AS descripcion FROM Tes_Conceptos;";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        // ------------------ ACCESO / VALIDACIONES ------------------

        public ErrorDto<bool> fxTesTipoAccesoValida(int codEmpresa, string vBanco, string vUsuario, string vTipo, string vGestion = "S")
        {
            var resp = DbHelper.CreateOkResponse(false);

            try
            {
                var extra = GestionClause(vGestion);
                const string baseSql = @"
SELECT ISNULL(COUNT(*),0)
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
WHERE A.id_Banco = @Banco AND A.nombre = @Usuario AND A.tipo = @Tipo";

                var sql = baseSql + extra;

                var countDto = DbHelper.ExecuteSingleQuery<int>(
                    _portalDb, codEmpresa, sql, 0,
                    new { Banco = vBanco, Usuario = vUsuario, Tipo = vTipo }
                );

                if (countDto.Code != 0)
                    return new ErrorDto<bool> { Code = countDto.Code, Description = countDto.Description, Result = false };

                resp.Result = countDto.Result > 0;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = false;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGestion(int codEmpresa, string usuario, string gestion)
        {
            try
            {
                var gestionCol = GestionColSeguro(gestion);

                var sql = $@"
SELECT id_banco AS item, descripcion
FROM Tes_Bancos
WHERE Estado = 'A'
  AND id_Banco IN (
      SELECT id_banco
      FROM tes_documentos_ASG
      WHERE nombre = @Usuario AND {gestionCol} = 1
      GROUP BY id_banco
  );";

                return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { Usuario = usuario });
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = null
                };
            }
        }

        public ErrorDto<string> fxTesBancoDocsValor(int codEmpresa, int vBanco, string vTipo, string vCampo = "Comprobante")
        {
            try
            {
                var campo = BancoDocsCampoSeguro(vCampo);
                var sql = $@"SELECT {campo} AS Campo FROM tes_Banco_docs WHERE id_Banco = @Banco AND tipo = @Tipo;";

                var result = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, sql, defaultValue: "", parameters: new { Banco = vBanco, Tipo = vTipo });
                return new ErrorDto<string>
                {
                    Code = result.Code,
                    Description = result.Description,
                    Result = result.Result ?? ""
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<string> { Code = -1, Description = ex.Message, Result = "" };
            }
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCboAcceso(int codEmpresa, string usuario, int banco, string? tipo = "S")
        {
            var extra = GestionClause(tipo);

            var sql = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item,
       RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
WHERE A.id_Banco = @Banco AND A.nombre = @Usuario"
+ extra +
@" ORDER BY T.Descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { Banco = banco, Usuario = usuario });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCboAccesoFirmas(int codEmpresa, string usuario, int banco, string? tipo = "S")
        {
            var extra = GestionClause(tipo);

            var sql = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item,
       RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
INNER JOIN TES_BANCO_DOCS D ON T.tipo = D.tipo AND A.id_banco = D.id_Banco
WHERE A.id_Banco = @Banco AND A.nombre = @Usuario AND D.comprobante = '01'"
+ extra;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { Banco = banco, Usuario = usuario });
        }

        public ErrorDto<string> fxTesTipoDocExtraeDato(int codEmpresa, int banco, string tipoDoc, string campo)
        {
            try
            {
                var campoSeguro = BancoDocsCampoSeguro(campo);
                var sql = $@"SELECT {campoSeguro} AS item FROM tes_banco_docs WHERE tipo = @TipoDoc AND id_banco = @Banco;";

                var result = DbHelper.ExecuteSingleQuery<string>(
                    _portalDb, codEmpresa, sql, defaultValue: "",
                    parameters: new { Banco = banco, TipoDoc = tipoDoc }
                );
                return new ErrorDto<string>
                {
                    Code = result.Code,
                    Description = result.Description,
                    Result = result.Result ?? ""
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<string> { Code = -1, Description = ex.Message, Result = "" };
            }
        }

        public ErrorDto sbTesBancosAfectacion(int codEmpresa, int vSolicitud, string vTipo = "E")
        {
            const string sql = @"EXEC spTESAfectaBancos @solicitud, @tipo;";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new { solicitud = vSolicitud, tipo = vTipo });
        }

        public ErrorDto sbTesBitacoraEspecial(int codEmpresa, int pSolicitud, string pMovimiento, string pDetalle, string usuario)
        {
            const string sql = @"EXEC spTesBitacora @solicitud, @movimiento, @detalle, @usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, sql,
                new
                {
                    solicitud = pSolicitud,
                    movimiento = pMovimiento,
                    detalle = pDetalle.Length > 150 ? pDetalle[..150] : pDetalle,
                    usuario
                });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTESCombos(string tipo)
        {
            var resp = DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());

            try
            {
                resp.Result = tipo switch
                {
                    "estado" => new()
                    {
                        new() { item = "T", descripcion = "Todos" },
                        new() { item = "S", descripcion = "Solicitados" },
                        new() { item = "E", descripcion = "Emitidos" },
                        new() { item = "A", descripcion = "Anulados" }
                    },
                    "busqueda" => new()
                    {
                        new() { item = "T", descripcion = "Todos" },
                        new() { item = "1", descripcion = "Por Número de Caso / Solicitud" },
                        new() { item = "2", descripcion = "Por Nombre Beneficiario" },
                        new() { item = "3", descripcion = "Por Número de Documento" },
                        new() { item = "4", descripcion = "Por Número de Referencia (OP)" }
                    },
                    "documento" => new()
                    {
                        new() { item = "C", descripcion = "Cheques" },
                        new() { item = "T", descripcion = "Transferencias" },
                        new() { item = "R", descripcion = "Reporte" }
                    },
                    _ => new()
                    {
                        new() { item = "T", descripcion = "Todos" }
                    }
                };

                if (tipo is not ("estado" or "busqueda" or "documento"))
                {
                    resp.Description = "No se encontró el pTipo de Combo que se desea llenar.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<string> fxTesParametro(int codEmpresa, string codigo)
        {
            const string sql = @"SELECT valor FROM tes_parametros WHERE cod_parametro = @Codigo;";
            var result = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, sql, defaultValue: "", parameters: new { Codigo = codigo });
            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? ""
            };
        }

        public ErrorDto<bool> fxValidaEmpresaSinpe(int codEmpresa)
        {
            const string sql = @"SELECT sinpe_activo FROM SIF_EMPRESA WHERE PORTAL_ID = @Empresa;";
            return DbHelper.ExecuteSingleQuery<bool>(_portalDb, codEmpresa, sql, defaultValue: false, parameters: new { Empresa = codEmpresa });
        }

        public ErrorDto<bool> fxTesBancoValida(int codEmpresa, int vBanco, string vUsuario)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*),0)
FROM Tes_Bancos B
INNER JOIN tes_Banco_ASG A ON B.id_Banco = A.id_Banco AND A.nombre = @Usuario
WHERE B.estado = 'A' AND B.id_Banco = @Banco;";

            var countDto = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, 0, new { Usuario = vUsuario, Banco = vBanco });
            if (countDto.Code != 0)
                return new ErrorDto<bool> { Code = -1, Description = countDto.Description, Result = false };

            return DbHelper.CreateOkResponse(countDto.Result > 0);
        }

        public ErrorDto<bool> fxTesConceptoValida(int codEmpresa, int vBanco, string vUsuario, string vConcepto)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*),0)
FROM tes_conceptos_ASG A
INNER JOIN Tes_Conceptos C ON A.cod_concepto = C.cod_concepto
WHERE A.id_Banco = @Banco AND A.nombre = @Usuario AND A.cod_concepto = @Concepto;";

            var countDto = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, 0, new { Banco = vBanco, Usuario = vUsuario, Concepto = vConcepto });
            if (countDto.Code != 0)
                return new ErrorDto<bool> { Code = -1, Description = countDto.Description, Result = false };

            return DbHelper.CreateOkResponse(countDto.Result > 0);
        }

        public ErrorDto<bool> fxTesUnidadValida(int codEmpresa, int vBanco, string vUsuario, string vUnidad)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*),0)
FROM tes_unidad_ASG A
INNER JOIN CntX_Unidades C ON A.cod_unidad = C.cod_unidad
WHERE A.id_Banco = @Banco AND A.nombre = @Usuario AND A.cod_unidad = @Unidad;";

            var countDto = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, 0, new { Banco = vBanco, Usuario = vUsuario, Unidad = vUnidad });
            if (countDto.Code != 0)
                return new ErrorDto<bool> { Code = -1, Description = countDto.Description, Result = false };

            return DbHelper.CreateOkResponse(countDto.Result > 0);
        }

        public ErrorDto<bool> fxTesDocumentoVerifica(int codEmpresa, int vBanco, string vTipo, string vDocumento)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*),0)
FROM Tes_Transacciones
WHERE id_banco = @Banco AND tipo = @Tipo AND Ndocumento = @Documento AND estado <> 'P';";

            var countDto = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, 0, new { Banco = vBanco, Tipo = vTipo, Documento = vDocumento });
            if (countDto.Code != 0)
                return new ErrorDto<bool> { Code = -1, Description = countDto.Description, Result = false };

            // Si existe => inválido
            return DbHelper.CreateOkResponse(countDto.Result == 0);
        }

        public ErrorDto<bool> fxTesCuentaObligatoriaVerifica(int codEmpresa, int vBanco)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*),0)
FROM Tes_Bancos
WHERE id_banco = @Banco AND INT_REQUIERE_CUENTA_DESTINO = 1;";

            var countDto = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, 0, new { Banco = vBanco });
            if (countDto.Code != 0)
                return new ErrorDto<bool> { Code = -1, Description = countDto.Description, Result = false };

            // Mantengo semántica original: si existe => Code=-1 y Result=true
            if (countDto.Result > 0)
                return new ErrorDto<bool> { Code = -1, Description = "Ok", Result = true };

            return DbHelper.CreateOkResponse(false);
        }

        // ------------------ CONSECUTIVOS (multi-step) ------------------

        public ErrorDto<long> fxTesTipoDocConsec(int codEmpresa, int idBanco, string tipo, string avance = "+", string plan = "-sp-")
        {
            var resp = DbHelper.CreateOkResponse(0L);

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                if (tipo != "TE" && plan != "-sp-") plan = "-sp-";

                var selectSql = (plan == "-sp-")
                    ? @"SELECT Consecutivo FROM tes_banco_docs WHERE tipo = @Tipo AND id_banco = @Banco;"
                    : @"SELECT ISNULL(NUMERO_TE, 0) AS Consecutivo FROM TES_BANCO_PLANES_TE WHERE id_banco = @Banco AND COD_PLAN = @Plan;";

                var current = connection.QueryFirstOrDefault<long>(selectSql, new { Tipo = tipo, Banco = idBanco, Plan = plan });

                var next = avance switch
                {
                    "+" => current + 1,
                    "-" => current - 1,
                    "/" => current,
                    _ => current
                };

                resp.Result = next;

                if (avance != "/")
                {
                    var updateSql = (plan == "-sp-")
                        ? $"UPDATE tes_banco_docs SET consecutivo = consecutivo {avance} 1 WHERE Tipo = @Tipo AND id_banco = @Banco;"
                        : $"UPDATE TES_BANCO_PLANES_TE SET NUMERO_TE = ISNULL(NUMERO_TE,0) {avance} 1 WHERE COD_PLAN = @Plan AND id_banco = @Banco;";

                    connection.Execute(updateSql, new { Tipo = tipo, Banco = idBanco, Plan = plan });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = 0;
            }

            return resp;
        }

        public ErrorDto<long> fxTesTipoDocConsecInterno(int codEmpresa, int idBanco, string tipo, string avance = "+", string plan = "-sp-")
        {
            var resp = DbHelper.CreateOkResponse(0L);

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                if (tipo != "TE" && plan != "-sp-") plan = "-sp-";

                var selectSql = (plan == "-sp-" || string.IsNullOrEmpty(plan))
                    ? @"SELECT ISNULL(CONSECUTIVO_DET,0) AS Consecutivo FROM tes_banco_docs WHERE tipo = @Tipo AND id_banco = @Banco;"
                    : @"SELECT ISNULL(NUMERO_INTERNO,0) AS Consecutivo FROM TES_BANCO_PLANES_TE WHERE id_banco = @Banco AND COD_PLAN = @Plan;";

                var current = connection.QueryFirstOrDefault<long>(selectSql, new { Tipo = tipo, Banco = idBanco, Plan = plan });

                var next = avance switch
                {
                    "+" => current + 1,
                    "-" => current - 1,
                    "/" => current,
                    _ => current
                };

                resp.Result = next;

                if (avance != "/")
                {
                    var updateSql = (plan == "-sp-")
                        ? $"UPDATE tes_banco_docs SET CONSECUTIVO_DET = ISNULL(CONSECUTIVO_DET,0) {avance} 1 WHERE Tipo = @Tipo AND id_banco = @Banco;"
                        : $"UPDATE TES_BANCO_PLANES_TE SET NUMERO_INTERNO = ISNULL(NUMERO_INTERNO,0) {avance} 1 WHERE COD_PLAN = @Plan AND id_banco = @Banco;";

                    connection.Execute(updateSql, new { Tipo = tipo, Banco = idBanco, Plan = plan });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = 0;
            }

            return resp;
        }

        // ------------------ ACTUALIZA CC (consistente) ------------------

        public class ActualizaCCParams
        {
            public string? Codigo { get; set; }
            public string? Tipo { get; set; }
            public string? Documento { get; set; }
            public int Banco { get; set; }
            public object? OP { get; set; }
            public string? Modulo { get; set; }
            public string? SubModulo { get; set; }
            public int Referencia { get; set; }
        }

        public ErrorDto sbTESActualizaCC(int codEmpresa, ActualizaCCParams parametros)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                if (parametros.Modulo?.Trim() != "CC" || parametros.SubModulo?.Trim() != "C")
                {
                    resp.Code = -1;
                    resp.Description = "Módulo o Submódulo inválido";
                    return resp;
                }

                using var connection = _portalDb.CreateConnection(codEmpresa);

                if (parametros.Referencia > 0)
                {
                    const string sql = @"
UPDATE DesemBolsos
SET Cod_Banco = @Banco,
    TDocumento = @Tipo,
    NDocumento = @Documento
WHERE ID_Desembolso = @Codigo;";

                    connection.Execute(sql, new
                    {
                        Banco = parametros.Banco,
                        Tipo = parametros.Tipo,
                        Documento = parametros.Documento,
                        Codigo = parametros.Codigo
                    });
                }
                else
                {
                    const string sql = @"
UPDATE Reg_Creditos
SET Cod_Banco = @Banco,
    Documento_Referido = @DocumentoReferido
WHERE ID_Solicitud = @OP;";

                    var documentoReferido = $"{parametros.Tipo}-{parametros.Documento}";
                    connection.Execute(sql, new
                    {
                        Banco = parametros.Banco,
                        DocumentoReferido = documentoReferido,
                        OP = parametros.OP
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        // ------------------ TIPOS DOC ASIENTO / DIVISA ------------------

        public ErrorDto<string> fxTesTiposDocAsiento(int codEmpresa, string vTipo)
        {
            const string sql = @"SELECT Movimiento FROM tes_tipos_doc WHERE tipo = @Tipo;";
            var result = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, sql, defaultValue: "", parameters: new { Tipo = vTipo });
            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? ""
            };
        }

        public ErrorDto<string> fxDescDivisa(int codEmpresa, string vDivisa)
        {
            var resp = DbHelper.CreateOkResponse("");

            try
            {
                const string sql = @"SELECT TOP 1 descripcion FROM CNTX_DIVISAS WHERE cod_divisa = @Divisa;";
                var dto = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, sql, defaultValue: "", parameters: new { Divisa = vDivisa });

                if (dto.Code != 0)
                    return new ErrorDto<string> { Code = dto.Code, Description = dto.Description, Result = "" };

                var descripcion = (dto.Result ?? "").Trim();
                if (string.IsNullOrEmpty(descripcion))
                    return DbHelper.CreateOkResponse(" Colones");

                var lower = descripcion.ToLowerInvariant();
                var primera = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? lower;
                primera = char.ToUpper(primera[0]) + primera[1..];

                var ultima = primera[^1];
                primera += "aeiouáéíóú".Contains(char.ToLowerInvariant(ultima)) ? "s" : "es";

                resp.Result = primera;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }

            return resp;
        }

        // ------------------ ARCHIVOS ESPECIALES (consistente) ------------------

        public ErrorDto<TesArchivosEspecialesData> sbCargaArchivosEspeciales(int codEmpresa, int banco)
        {
            var response = DbHelper.CreateOkResponse(new TesArchivosEspecialesData());

            try
            {
                const string sql = @"
SELECT UTILIZA_FORMATO_ESPECIAL, ARCHIVO_CHEQUES_FIRMAS, ARCHIVO_CHEQUES_SIN_FIRMAS
FROM TES_BANCOS
WHERE ID_BANCO = @Banco;";

                var dataDto = DbHelper.ExecuteSingleQuery<TesBancosArchivosData>(
                    _portalDb, codEmpresa, sql, defaultValue: null, parameters: new { Banco = banco }
                );

                if (dataDto.Code != 0)
                    return new ErrorDto<TesArchivosEspecialesData> { Code = dataDto.Code, Description = dataDto.Description, Result = new TesArchivosEspecialesData() };

                var archivosData = dataDto.Result;
                if (archivosData is null) return response;

                var archivoFirmas = Path.Combine(_dirRDLC, codEmpresa.ToString(), archivosData.archivo_cheques_firmas ?? "");
                var archivoSinFirmas = Path.Combine(_dirRDLC, codEmpresa.ToString(), archivosData.archivo_cheques_sin_firmas ?? "");

                if (response.Result != null)
                {
                    if (archivosData.utiliza_formato_especial == 1)
                    {
                        response.Result.chequesFirmas = File.Exists(archivoFirmas) ? (archivosData.archivo_cheques_firmas ?? "Banking_DocFormat01") : "Banking_DocFormat01";
                        response.Result.chequesSinFirmas = File.Exists(archivoSinFirmas) ? (archivosData.archivo_cheques_sin_firmas ?? "Banking_DocFormat02") : "Banking_DocFormat02";
                    }
                    else
                    {
                        response.Result.chequesFirmas = "Banking_DocFormat01";
                        response.Result.chequesSinFirmas = "Banking_DocFormat02";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        // ------------------ TOKENS (consistente) ------------------

        public ErrorDto<List<TokenConsultaModel>> spTes_Token_Consulta(int codEmpresa, string usuario)
        {
            const string sql = @"EXEC spTes_Token_Consulta '', 'A', @usuario;";
            return DbHelper.ExecuteListQuery<TokenConsultaModel>(_portalDb, codEmpresa, sql, new { usuario });
        }

        public ErrorDto<string?> spTes_Token_New(int codEmpresa, string usuario)
        {
            var resp = DbHelper.CreateOkResponse<string?>(null);

            try
            {
                const string genSql = @"EXEC spTes_Token_New @usuario;";
                var gen = DbHelper.ExecuteNonQueryWithResult(_portalDb, codEmpresa, genSql, new { usuario = usuario.ToUpper() });
                if (gen.Code != 0)
                    return new ErrorDto<string?> { Code = gen.Code, Description = gen.Description, Result = null };

                const string readSql = @"
SELECT TOP 1 ID_TOKEN
FROM Tes_Tokens
WHERE REGISTRO_USUARIO = @usuario
ORDER BY REGISTRO_FECHA DESC;";

                var token = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, readSql, defaultValue: "", parameters: new { usuario = usuario.ToUpper() });
                if (token.Code != 0)
                    return new ErrorDto<string?> { Code = token.Code, Description = token.Description, Result = null };

                resp.Result = token.Result;
                resp.Description = token.Result ?? "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        // ------------------ TAGS (consistente y realmente await) ------------------

        public async Task<ErrorDto> sbCrdOperacionTags(
            int codEmpresa,
            long pOperacion,
            string pLinea,
            string pTag,
            string pUsuario,
            string? pAsignado = "",
            string? pNotas = "")
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string proc = @"spCrdOperacionTagRegistra";
                var values = new
                {
                    Operacion = pOperacion,
                    CrdLinea = pLinea,
                    Tag = pTag,
                    Usuario = pUsuario,
                    Asignado = pAsignado,
                    Notas = pNotas
                };

                await connection.ExecuteAsync(proc, values, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        // ------------------ EMITIR DOCUMENTO (refactor completo) ------------------

        public ErrorDto sbTesEmitirDocumento(int codEmpresa, string vUsuario, int vModulo, int vSolicitud, string vDocumento = "", DateTime? vFecha = null)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var data = ObtenerTransaccionParaEmision(connection, vSolicitud);
                if (data is null)
                    return ErrorNoSolicitud();

                var fechaEmision = vFecha ?? data.fechaX;
                data.nSolicitud = vSolicitud;

                return ProcesarEmisionSegunComprobante(
                    codEmpresa,
                    connection,
                    data,
                    vUsuario,
                    vDocumento,
                    fechaEmision
                );
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                return resp;
            }
        }

        private static ErrorDto ErrorNoSolicitud()
            => new ErrorDto { Code = -3, Description = "No se encontró la solicitud especificada." };

        private static ErrorDto ErrorTransferenciaNoProcesable()
            => new ErrorDto { Code = -1, Description = "Las Transferencias Electrónicas no se pueden procesar directamente..." };

        private static MTesTransaccionDto? ObtenerTransaccionParaEmision(SqlConnection connection, int solicitud)
        {
            const string sql = @"
            SELECT C.monto, C.nsolicitud, T.doc_auto, T.comprobante,
                ISNULL(B.firmas_desde,0) AS Firmas_Desde, B.Lugar_Emision,
                X.descripcion AS TipoX, ISNULL(B.firmas_hasta,0) AS Firmas_Hasta,
                dbo.MyGetdate() AS FechaX, C.id_Banco, C.tipo, C.modulo,
                C.op, C.referencia, C.codigo, C.subModulo, C.cod_divisa
            FROM Tes_Transacciones C
            INNER JOIN Tes_Bancos B ON C.id_Banco = B.id_banco
            INNER JOIN tes_banco_docs T ON B.id_Banco = T.id_Banco AND C.tipo = T.tipo
            INNER JOIN tes_tipos_doc X ON T.tipo = X.tipo
            WHERE C.nsolicitud = @solicitud;";

            return connection.QueryFirstOrDefault<MTesTransaccionDto>(sql, new { solicitud });
        }

        private ErrorDto ProcesarEmisionSegunComprobante(int codEmpresa, SqlConnection connection, MTesTransaccionDto data, string usuario, string documentoInput, DateTime fechaEmision)
        {
            return data.comprobante switch
            {
                "01" or "02" or "03" => EmitirComprobanteImprimible(
                    codEmpresa, connection, data, usuario, documentoInput, fechaEmision),

                "04" => ErrorTransferenciaNoProcesable(),

                _ => DbHelper.CreateOkResponse()
            };
        }

        private ErrorDto EmitirComprobanteImprimible(int codEmpresa, SqlConnection connection, MTesTransaccionDto data, string usuario, string documentoInput, DateTime fechaEmision)
        {
            // 1) Consecutivo si aplica
            var consecutivoDto = ObtenerConsecutivoSiAplica(codEmpresa, data);
            if (consecutivoDto.Code != 0)
                return new ErrorDto { Code = consecutivoDto.Code, Description = consecutivoDto.Description };

            var consecutivo = consecutivoDto.Result ?? string.Empty;

            // 2) Documento final (autoconsec -> consecutivo, sino input)
            var documentoFinal = data.doc_auto ? consecutivo : documentoInput;

            // 3) Update transacción (solo setea NDocumento si hay valor)
            ActualizarTransaccionEmitida(connection, (int)data.nSolicitud, usuario, fechaEmision, documentoFinal);

            // 4) Actualiza CC (manteniendo tu semántica)
            var actCc = ActualizarCCDesdeTransaccion(codEmpresa, data, consecutivo);
            if (actCc.Code != 0)
                return actCc;

            return DbHelper.CreateOkResponse();
        }

        private ErrorDto<string?> ObtenerConsecutivoSiAplica(int codEmpresa, MTesTransaccionDto data)
        {
            if (!data.doc_auto)
                return DbHelper.CreateOkResponse<string?>(string.Empty);

            var consec = fxTesTipoDocConsec(codEmpresa, data.id_banco, data.tipo, "+");
            if (consec.Code != 0)
                return new ErrorDto<string?> { Code = consec.Code, Description = consec.Description, Result = null };

            return DbHelper.CreateOkResponse<string?>(consec.Result.ToString());
        }

        private static void ActualizarTransaccionEmitida(SqlConnection connection, int solicitud, string usuario, DateTime fechaEmision, string documentoFinal)
        {
            const string sqlSinDocumento = @"
                    UPDATE Tes_Transacciones
                    SET Estado = 'I',
                        Fecha_Emision = @fecha,
                        Ubicacion_Actual = 'T',
                        Fecha_Traslado = @fecha,
                        User_Genera = @usuario
                    WHERE nsolicitud = @solicitud;";

            const string sqlConDocumento = @"
                    UPDATE Tes_Transacciones
                    SET Estado = 'I',
                        Fecha_Emision = @fecha,
                        Ubicacion_Actual = 'T',
                        Fecha_Traslado = @fecha,
                        User_Genera = @usuario,
                        NDocumento = @documento
                    WHERE nsolicitud = @solicitud;";

            var parameters = new
            {
                fecha = fechaEmision.ToString("yyyy-MM-dd"),
                usuario,
                solicitud,
                documento = documentoFinal
            };

            if (string.IsNullOrWhiteSpace(documentoFinal))
                connection.Execute(sqlSinDocumento, parameters);
            else
                connection.Execute(sqlConDocumento, parameters);
        }


        private ErrorDto ActualizarCCDesdeTransaccion(int codEmpresa, MTesTransaccionDto data, string consecutivo)
        {
            return sbTESActualizaCC(
                codEmpresa,
                new ActualizaCCParams
                {
                    Codigo = data.codigo,
                    Tipo = data.tipo,
                    Documento = consecutivo,
                    Banco = data.id_banco,
                    OP = data.op != null ? (int)data.op : 0,
                    Modulo = data.modulo,
                    SubModulo = data.subModulo,
                    Referencia = data.referencia != null ? (int)data.referencia : 0
                });
        }

        // ------------------ REPORTE TRANSFERENCIA (refactor completo) ------------------

        public ErrorDto<TesReporteTransferenciaDto> sbTesReporteTransferencia(
            int codEmpresa, int vBanco, long vTransac, string? vTipo = "C",
            string? vDocumento = "TE", string? vPlan = "-sp-")
        {
            var resp = DbHelper.CreateOkResponse(new TesReporteTransferenciaDto());

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                // Banco / letras1
                const string bancoSql = @"SELECT cta AS item, descripcion FROM Tes_Bancos WHERE id_banco = @Banco;";
                var banco = connection.QueryFirstOrDefault(bancoSql, new { Banco = vBanco });

                var letraBase = "";
                if (banco != null)
                {
                    letraBase = "Sirva la Presente para saludarlo y a la vez solicitarle debitar de nuestra cuenta corriente"
                              + " # " + banco.item + " la suma de ¢ ";
                }

                if (vTipo == "C")
                {
                    var strSQL = @"
SELECT SUM(Monto) AS Monto, COUNT(*) AS Casos, cod_divisa
FROM Tes_Transacciones
WHERE tipo = @Documento AND id_banco = @Banco AND documento_Base = @Transac";

                    if (vPlan != "-sp-")
                        strSQL += " AND Cod_Plan = @Plan";

                    strSQL += " GROUP BY cod_divisa;";

                    var rs = connection.QueryFirstOrDefault(strSQL, new { Documento = vDocumento, Banco = vBanco, Transac = vTransac, Plan = vPlan });

                    decimal curMonto = 0;
                    long lngCasos = 0;
                    string strDivisa = "";

                    if (rs != null)
                    {
                        curMonto = rs.Monto;
                        lngCasos = rs.Casos;
                        strDivisa = rs.cod_divisa;
                    }

                    var letrasDivisa = fxDescDivisa(codEmpresa, strDivisa);
                    if (letrasDivisa.Code != 0)
                        return new ErrorDto<TesReporteTransferenciaDto> { Code = letrasDivisa.Code, Description = letrasDivisa.Description, Result = new TesReporteTransferenciaDto() };

                    var montoLetras = MProGrXAuxiliarDB.NumeroALetras(curMonto).Result + letrasDivisa.Result;

                    // Parametros (ahora consistentes)
                    var p1 = fxTesParametro(codEmpresa, "01");
                    var p2 = fxTesParametro(codEmpresa, "02");
                    var p3 = fxTesParametro(codEmpresa, "03");

                    if (resp.Result == null)
                        resp.Result = new TesReporteTransferenciaDto();

                    resp.Result.registros = lngCasos;
                    resp.Result.montoLetras = montoLetras;
                    resp.Result.totalMonto = curMonto;
                    resp.Result.fxNombre = p1.Result ?? string.Empty;
                    resp.Result.fxPuesto = p2.Result ?? string.Empty;
                    resp.Result.fxDepartamento = p3.Result ?? string.Empty;
                    resp.Result.letras1 = letraBase;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = new TesReporteTransferenciaDto();
            }

            return resp;
        }

        // ------------------ CIFRADOS (igual) ------------------

        public static string fxTESCifrado(string vClave)
        {
            var sb = new StringBuilder(vClave.Length);
            foreach (var c in vClave)
                sb.Append((char)(c + 7));
            return sb.ToString();
        }

        public static string fxStringCifrado(string pCadena)
        {
            if (string.IsNullOrEmpty(pCadena))
                return string.Empty;

            var vResBuilder = new StringBuilder(pCadena.Length * 3);
            for (int i = pCadena.Length - 1; i >= 0; i--)
                vResBuilder.Append(((int)pCadena[i]).ToString("D3"));

            var vRes = vResBuilder.ToString();
            var deltas = new[] { +1, -5, +7, -13, -2, +3 };
            int vSec = 0;

            var outBuilder = new StringBuilder(vRes.Length + vRes.Length / 3);

            for (int i = 0; i < vRes.Length; i += 3)
            {
                int len = Math.Min(3, vRes.Length - i);
                if (!int.TryParse(vRes.AsSpan(i, len), out int num)) continue;

                outBuilder.Append(num + deltas[vSec]);
                vSec = (vSec + 1) % deltas.Length;
            }

            return FxDepuraCadena(outBuilder.ToString());
        }

        public static string FxDepuraCadena(string xCadena)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < xCadena.Length; i += 2)
            {
                var chunk = xCadena.Substring(i, Math.Min(2, xCadena.Length - i));
                if (int.TryParse(chunk, out int num) && num > 31 && num != 39 && num != 34)
                    sb.Insert(0, (char)num);
            }

            return sb.ToString();
        }

        // ------------------ PERMISOS USER/BANCOS/TIPO (whitelist) ------------------

        public bool fxValidaPermisoUserBancosTipo(int codEmpresa, int vBanco, string vTipo, string vUsuario, string vPermiso)
        {
            try
            {
                var permisoCol = PermisoColSeguro(vPermiso);

                var sql = $@"
SELECT COUNT(T.Tipo)
FROM tes_tipos_doc T
LEFT JOIN tes_documentos_asg A ON T.tipo = A.tipo AND A.id_banco = @Banco AND A.nombre = @Usuario
WHERE T.tipo IN (SELECT Tipo FROM tes_banco_docs WHERE id_banco = @Banco)
  AND T.Tipo = @Tipo
  AND ISNULL(A.{permisoCol},0) = 1;";

                var countDto = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, 0, new { Tipo = vTipo, Banco = vBanco, Usuario = vUsuario });
                return countDto.Code == 0 && countDto.Result > 0;
            }
            catch
            {
                return false;
            }
        }

        // ------------------ MESES ------------------

        public static string fxTesMesDescripcion(int vMes) => vMes switch
        {
            1 => "ENERO",
            2 => "FEBRERO",
            3 => "MARZO",
            4 => "ABRIL",
            5 => "MAYO",
            6 => "JUNIO",
            7 => "JULIO",
            8 => "AGOSTO",
            9 => "SETIEMBRE",
            10 => "OCTUBRE",
            11 => "NOVIEMBRE",
            12 => "DICIEMBRE",
            _ => ""
        };

        // ------------------ HELPERS (whitelist) ------------------

        private static string GestionClause(string? tipo)
            => tipo switch
            {
                "S" => " AND A.SOLICITA = 1",
                "A" => " AND A.AUTORIZA = 1",
                "G" => " AND A.GENERA = 1",
                "X" => " AND A.ASIENTOS = 1",
                "N" => " AND A.ANULA = 1",
                _ => ""
            };

        private static string GestionColSeguro(string gestion)
            => gestion switch
            {
                "SOLICITA" => "SOLICITA",
                "AUTORIZA" => "AUTORIZA",
                "GENERA" => "GENERA",
                "ASIENTOS" => "ASIENTOS",
                "ANULA" => "ANULA",
                _ => throw new ArgumentException("Gestión inválida", nameof(gestion))
            };

        private static string PermisoColSeguro(string permiso)
            => permiso switch
            {
                "Solicita" => "SOLICITA",
                "Autoriza" => "AUTORIZA",
                "Genera" => "GENERA",
                "Asientos" => "ASIENTOS",
                "Anula" => "ANULA",
                _ => throw new ArgumentException("Permiso inválido", nameof(permiso))
            };

        private static string BancoDocsCampoSeguro(string campo)
            => campo switch
            {
                // agrega SOLO los campos reales de tes_banco_docs que uses dinámicamente
                "Comprobante" => "Comprobante",
                "Consecutivo" => "Consecutivo",
                "CONSECUTIVO_DET" => "CONSECUTIVO_DET",
                "doc_auto" => "doc_auto",
                "documento_Base" => "documento_Base",
                _ => throw new ArgumentException("Campo inválido", nameof(campo))
            };
    }
}