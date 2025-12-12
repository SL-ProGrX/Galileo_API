using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier
{
    public class MTesoreria
    {
        private readonly IConfiguration _config;
        private readonly string dirRDLC;

        private const string PERM_SOLICITA = "SOLICITA";
        private const string PERM_AUTORIZA = "AUTORIZA";
        private const string PERM_GENERA = "GENERA";
        private const string PERM_ASIENTOS = "ASIENTOS";
        private const string PERM_ANULA = "ANULA";


        public MTesoreria(IConfiguration config)
        {
            _config = config;
            dirRDLC = _config.GetSection("AppSettings").GetSection("RutaRDLC").Value ?? string.Empty;
        }

        // -----------------------------
        // Helpers anti-inyección (S2077)
        // -----------------------------

        private static string MapGestionColumn(string gestion) => (gestion ?? "").Trim().ToUpperInvariant() switch
        {
            PERM_SOLICITA => PERM_SOLICITA,
            PERM_AUTORIZA => PERM_AUTORIZA,
            PERM_GENERA => PERM_GENERA,
            PERM_ASIENTOS => PERM_ASIENTOS,
            PERM_ANULA => PERM_ANULA,
            _ => throw new ArgumentException("Gestión inválida", nameof(gestion))
        };

        private static string MapGestionFromCodigo(string vGestion) => (vGestion ?? "S").Trim().ToUpperInvariant() switch
        {
            "S" => PERM_SOLICITA,
            "A" => PERM_AUTORIZA,
            "G" => PERM_GENERA,
            "X" => PERM_ASIENTOS,
            "N" => PERM_ANULA,
            _ => PERM_SOLICITA
        };

        private static string MapPermisoColumn(string permiso) => (permiso ?? "").Trim().ToUpperInvariant() switch
        {
            PERM_SOLICITA => PERM_SOLICITA,
            PERM_AUTORIZA => PERM_AUTORIZA,
            PERM_GENERA => PERM_GENERA,
            PERM_ASIENTOS => PERM_ASIENTOS,
            PERM_ANULA => PERM_ANULA,
            _ => throw new ArgumentException("Permiso inválido", nameof(permiso))
        };

        private static string MapTesBancoDocsField(string campo) => (campo ?? "").Trim() switch
        {
            // SOLO permití las columnas reales que vas a consultar.
            "Comprobante" => "Comprobante",
            "Consecutivo" => "Consecutivo",
            "CONSECUTIVO_DET" => "CONSECUTIVO_DET",
            "DOC_AUTO" => "DOC_AUTO",
            "Movimiento" => "Movimiento",
            _ => throw new ArgumentException("Campo inválido", nameof(campo))
        };

        private static string NormalizePlan(string tipo, string plan)
        {
            // Mantengo tu regla original: solo TE permite planes distintos a "-sp-"
            if (!string.Equals(tipo, "TE", StringComparison.OrdinalIgnoreCase) && plan != "-sp-")
                return "-sp-";
            return string.IsNullOrWhiteSpace(plan) ? "-sp-" : plan;
        }

        // -----------------------------
        // Métodos
        // -----------------------------

        public ErrorDto<List<DropDownListaGenericaModel>> tes_TiposDocumentos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"select TIPO AS ITEM, DESCRIPCION from tes_tipos_doc";
                resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCbo(int CodEmpresa, string usuario, int banco, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select rtrim(C.cod_unidad) as item, rtrim(C.descripcion) as descripcion
                    from tes_unidad_ASG A
                    inner join CntX_Unidades C on A.cod_unidad = C.cod_unidad and C.cod_contabilidad = @contabilidad
                    where A.id_Banco = @banco and A.nombre = @usuario and activa = 1
                    order by C.Descripcion";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new
                {
                    contabilidad,
                    banco,
                    usuario
                }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCbo(int CodEmpresa, string usuario, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select rtrim(C.cod_Concepto) as item, rtrim(C.Descripcion) as descripcion
                    from tes_conceptos_ASG A
                    inner join Tes_Conceptos C on A.cod_concepto = C.cod_concepto
                    where A.id_Banco = @banco and A.nombre = @usuario and estado = 'A'
                    order by C.Descripcion";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { banco, usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<bool> fxTesTipoAccesoValida(int CodEmpresa, string vBanco, string vUsuario, string vTipo, string vGestion = "S")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<bool> { Code = 0, Result = false };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // En vez de concatenar "and A.X = 1", metemos columna validada.
                var col = MapGestionColumn(MapGestionFromCodigo(vGestion));

                var query = $@"
                    select isnull(Count(*),0) as Existe
                    from tes_documentos_ASG A
                    inner join Tes_Tipos_Doc T on A.tipo = T.tipo
                    where A.id_Banco = @banco
                      and A.nombre = @usuario
                      and A.tipo = @tipo
                      and isnull(A.[{col}],0) = 1";

                var result = connection.QueryFirstOrDefault<int>(query, new { banco = vBanco, usuario = vUsuario, tipo = vTipo });
                resp.Result = result > 0;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = false;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGestion(int CodEmpresa, string usuario, string gestion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var col = MapGestionColumn(gestion);

                var query = $@"
                    select id_banco as item, descripcion
                    from Tes_Bancos
                    where Estado = 'A'
                      and id_Banco in (
                          select id_banco
                          from tes_documentos_ASG
                          where nombre = @usuario
                            and isnull([{col}],0) = 1
                          group by id_banco
                      )";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<string> fxTesBancoDocsValor(int CodEmpresa, int vBanco, string vTipo, string vCampo = "Comprobante")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<string> { Code = 0, Result = "" };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var col = MapTesBancoDocsField(vCampo);
                var query = $@"select [{col}] as Campo from tes_Banco_docs where id_Banco = @banco and tipo = @tipo";

                resp.Result = connection.QueryFirstOrDefault<string>(query, new { banco = vBanco, tipo = vTipo }) ?? "";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCboAcceso(int CodEmpresa, string Usuario, int Banco, string? Tipo = "S")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var col = MapGestionColumn(MapGestionFromCodigo(Tipo ?? "S"));

                var query = $@"
                    SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
                           RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
                    FROM tes_documentos_ASG A
                    INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
                    WHERE A.id_Banco = @banco
                      AND A.nombre = @usuario
                      AND isnull(A.[{col}],0) = 1
                    ORDER BY T.Descripcion";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { banco = Banco, usuario = Usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCboAccesoFirmas(int CodEmpresa, string Usuario, int Banco, string? Tipo = "S")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var col = MapGestionColumn(MapGestionFromCodigo(Tipo ?? "S"));

                var query = $@"
                    SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
                           RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
                    FROM tes_documentos_ASG A
                    INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
                    INNER JOIN TES_BANCO_DOCS D ON T.tipo = D.tipo AND A.id_banco = D.id_Banco
                    WHERE A.id_Banco = @banco
                      AND A.nombre = @usuario
                      AND D.comprobante = '01'
                      AND isnull(A.[{col}],0) = 1";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { banco = Banco, usuario = Usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<long> fxTesTipoDocConsec(int CodEmpresa, int id_banco, string tipo, string avance = "+", string plan = "-sp-")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<long> { Code = 0, Result = 0 };

            try
            {
                using var connection = new SqlConnection(stringConn);

                plan = NormalizePlan(tipo, plan);

                string selectSql = plan == "-sp-"
                    ? @"SELECT ISNULL(Consecutivo,0) FROM tes_banco_docs WHERE tipo = @Tipo AND id_banco = @Banco"
                    : @"SELECT ISNULL(NUMERO_TE,0) FROM TES_BANCO_PLANES_TE WHERE id_banco = @Banco AND COD_PLAN = @Plan";

                var current = connection.QueryFirstOrDefault<long>(selectSql, new { Tipo = tipo, Banco = id_banco, Plan = plan });

                long consecutivo = avance switch
                {
                    "+" => current + 1,
                    "-" => current - 1,
                    "/" => current,
                    _ => current
                };

                resp.Result = consecutivo;

                if (avance != "/")
                {
                    string updateSql = plan == "-sp-"
                        ? @"
                            UPDATE tes_banco_docs
                            SET consecutivo = ISNULL(consecutivo,0) + CASE @avance WHEN '+' THEN 1 WHEN '-' THEN -1 ELSE 0 END
                            WHERE Tipo = @Tipo AND id_banco = @Banco"
                        : @"
                            UPDATE TES_BANCO_PLANES_TE
                            SET NUMERO_TE = ISNULL(NUMERO_TE,0) + CASE @avance WHEN '+' THEN 1 WHEN '-' THEN -1 ELSE 0 END
                            WHERE COD_PLAN = @Plan AND id_banco = @Banco";

                    connection.Execute(updateSql, new { avance, Tipo = tipo, Banco = id_banco, Plan = plan });
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

        public ErrorDto<long> fxTesTipoDocConsecInterno(int CodEmpresa, int id_banco, string tipo, string avance = "+", string plan = "-sp-")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<long> { Code = 0, Result = 0 };

            try
            {
                using var connection = new SqlConnection(stringConn);

                plan = NormalizePlan(tipo, plan);

                string selectSql = plan == "-sp-"
                    ? @"select isnull(CONSECUTIVO_DET,0) from tes_banco_docs where tipo = @Tipo and id_banco = @Banco"
                    : @"select isnull(NUMERO_INTERNO,0) from TES_BANCO_PLANES_TE where id_banco = @Banco and COD_PLAN = @Plan";

                var current = connection.QueryFirstOrDefault<long>(selectSql, new { Tipo = tipo, Banco = id_banco, Plan = plan });

                long consecutivo = avance switch
                {
                    "+" => current + 1,
                    "-" => current - 1,
                    "/" => current,
                    _ => current
                };

                resp.Result = consecutivo;

                if (avance != "/")
                {
                    string updateSql = plan == "-sp-"
                        ? @"
                            update tes_banco_docs
                            set CONSECUTIVO_DET = isnull(CONSECUTIVO_DET,0) + CASE @avance WHEN '+' THEN 1 WHEN '-' THEN -1 ELSE 0 END
                            where Tipo = @Tipo and id_banco = @Banco"
                        : @"
                            update TES_BANCO_PLANES_TE
                            set NUMERO_INTERNO = isnull(NUMERO_INTERNO,0) + CASE @avance WHEN '+' THEN 1 WHEN '-' THEN -1 ELSE 0 END
                            where COD_PLAN = @Plan and id_banco = @Banco";

                    connection.Execute(updateSql, new { avance, Tipo = tipo, Banco = id_banco, Plan = plan });
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

        public ErrorDto<string> fxTesTipoDocExtraeDato(int CodEmpresa, int Banco, string TipoDoc, string Campo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<string> { Code = 0, Description = "" };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var col = MapTesBancoDocsField(Campo);
                var query = $@"select [{col}] as item from tes_banco_docs where tipo = @tipoDoc and id_banco = @banco";

                resp.Result = connection.QueryFirstOrDefault<string>(query, new { banco = Banco, tipoDoc = TipoDoc }) ?? "";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }

            return resp;
        }

        public ErrorDto<TesArchivosEspecialesData> sbCargaArchivosEspeciales(int CodEmpresa, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesArchivosEspecialesData> { Code = 0, Result = new TesArchivosEspecialesData() };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"Select UTILIZA_FORMATO_ESPECIAL,ARCHIVO_CHEQUES_FIRMAS,ARCHIVO_CHEQUES_SIN_FIRMAS from TES_BANCOS where ID_BANCO = @banco";
                var archivosData = connection.QueryFirstOrDefault<TesBancosArchivosData>(query, new { banco });

                if (archivosData != null)
                {
                    string archivoFirmas = Path.Combine(dirRDLC, CodEmpresa.ToString(), archivosData.archivo_cheques_firmas) ?? "";
                    string archivoSinFirmas = Path.Combine(dirRDLC, CodEmpresa.ToString(), archivosData.archivo_cheques_sin_firmas) ?? "";

                    if (archivosData.utiliza_formato_especial == 1)
                    {
                        response.Result.chequesFirmas = File.Exists(archivoFirmas) ? archivosData.archivo_cheques_firmas : "Banking_DocFormat01";
                        response.Result.chequesSinFirmas = File.Exists(archivoSinFirmas) ? archivosData.archivo_cheques_sin_firmas : "Banking_DocFormat02";
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

        public ErrorDto sbTesBancosAfectacion(int CodEmpresa, int vSolicitud, string vTipo = "E")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = "exec spTESAfectaBancos @solicitud, @tipo";
                connection.Execute(query, new { solicitud = vSolicitud, tipo = vTipo });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"
                    select id_banco as item, descripcion
                    from Tes_Bancos
                    where Estado = 'A'
                    order by descripcion";
                resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto sbTesBitacoraEspecial(int CodEmpresa, int pSolicitud, string pMovimiento, string pDetalle, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = "exec spTesBitacora @solicitud, @movimiento, @detalle, @usuario";

                connection.Execute(query, new
                {
                    solicitud = pSolicitud,
                    movimiento = pMovimiento,
                    detalle = pDetalle.Length > 150 ? pDetalle.Substring(0, 150) : pDetalle,
                    usuario = Usuario
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTESCombos(string tipo)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                switch ((tipo ?? "").Trim().ToLowerInvariant())
                {
                    case "estado":
                        resp.Result = new List<DropDownListaGenericaModel>
                        {
                            new() { item = "T", descripcion = "Todos" },
                            new() { item = "S", descripcion = "Solicitados" },
                            new() { item = "E", descripcion = "Emitidos" },
                            new() { item = "A", descripcion = "Anulados" }
                        };
                        break;

                    case "busqueda":
                        resp.Result = new List<DropDownListaGenericaModel>
                        {
                            new() { item = "T", descripcion = "Todos" },
                            new() { item = "1", descripcion = "Por Número de Caso / Solicitud" },
                            new() { item = "2", descripcion = "Por Nombre Beneficiario" },
                            new() { item = "3", descripcion = "Por Número de Documento" },
                            new() { item = "4", descripcion = "Por Número de Referencia (OP)" }
                        };
                        break;

                    case "documento":
                        resp.Result = new List<DropDownListaGenericaModel>
                        {
                            new() { item = "C", descripcion = "Cheques" },
                            new() { item = "T", descripcion = "Transferencias" },
                            new() { item = "R", descripcion = "Reporte" }
                        };
                        break;

                    default:
                        resp.Result = new List<DropDownListaGenericaModel> { new() { item = "T", descripcion = "Todos" } };
                        resp.Code = 0;
                        resp.Description = "No se encontró el pTipo de Combo que se desea llenar.";
                        break;
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

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCboGeneral(int CodEmpresa, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion
                    from CntX_Unidades
                    where cod_contabilidad = @contabilidad";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCbo(int CodEmpresa, int Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select T.Tipo as item, rtrim(T.Descripcion) as descripcion
                    from tes_banco_docs A
                    inner join Tes_Tipos_Doc T on A.tipo = T.tipo
                    where A.id_Banco = @banco
                    order by T.Descripcion";

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { banco = Banco }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCboGeneral(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"select rtrim(cod_Concepto) as item, rtrim(Descripcion) as descripcion from Tes_Conceptos";
                resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

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

        public ErrorDto sbTESActualizaCC(int CodEmpresa, ActualizaCCParams parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                if (parametros.Modulo?.Trim() != "CC" || parametros.SubModulo?.Trim() != "C")
                {
                    resp.Code = -1;
                    resp.Description = "Módulo o Submódulo inválido";
                    return resp;
                }

                using var connection = new SqlConnection(stringConn);

                if (parametros.Referencia > 0)
                {
                    const string query = @"
                        UPDATE DesemBolsos
                        SET Cod_Banco = @Banco,
                            TDocumento = @Tipo,
                            NDocumento = @Documento
                        WHERE ID_Desembolso = @Codigo";

                    connection.Execute(query, new
                    {
                        Banco = parametros.Banco,
                        Tipo = parametros.Tipo,
                        Documento = parametros.Documento,
                        Codigo = parametros.Codigo
                    });
                }
                else
                {
                    const string query = @"
                        UPDATE Reg_Creditos
                        SET Cod_Banco = @Banco,
                            Documento_Referido = @documentoReferido
                        WHERE ID_Solicitud = @OP";

                    string documentoReferido = $"{parametros.Tipo}-{parametros.Documento}";

                    connection.Execute(query, new
                    {
                        Banco = parametros.Banco,
                        documentoReferido,
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

        public string fxTesParametro(int CodEmpresa, string xCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = "select valor from tes_parametros where cod_parametro = @codigo";
                return connection.QueryFirst<string>(query, new { codigo = xCodigo });
            }
            catch
            {
                return "";
            }
        }

        public ErrorDto<bool> fxValidaEmpresaSinpe(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = false };

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = "select sinpe_activo from SIF_EMPRESA WHERE PORTAL_ID = @empresa";
                result.Result = connection.QueryFirst<bool>(query, new { empresa = CodEmpresa });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<bool> fxTesBancoValida(int CodEmpresa, int vBanco, string vUsuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select isnull(Count(*),0) as Existe
                    from Tes_Bancos B
                    inner join tes_Banco_ASG A on B.id_Banco = A.id_Banco
                    where A.nombre = @usuario
                      and B.estado = 'A'
                      and B.id_Banco = @banco";

                var resp = connection.QueryFirstOrDefault<int>(query, new { usuario = vUsuario, banco = vBanco });

                if (resp <= 0)
                {
                    result.Code = -1;
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<bool> fxTesConceptoValida(int CodEmpresa, int vBanco, string vUsuario, string vConcepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select isnull(Count(*),0) as Existe
                    from tes_conceptos_ASG A
                    inner join Tes_Conceptos C on A.cod_concepto = C.cod_concepto
                    where A.id_Banco = @banco
                      and A.nombre = @usuario
                      and A.cod_concepto = @concepto";

                var resp = connection.QueryFirstOrDefault<int>(query, new { banco = vBanco, usuario = vUsuario, concepto = vConcepto });

                if (resp <= 0)
                {
                    result.Code = -1;
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<bool> fxTesUnidadValida(int CodEmpresa, int vBanco, string vUsuario, string vUnidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select isnull(Count(*),0) as Existe
                    from tes_unidad_ASG A
                    inner join CntX_Unidades C on A.cod_unidad = C.cod_unidad
                    where A.id_Banco = @banco
                      and A.nombre = @usuario
                      and A.cod_unidad = @unidad";

                var resp = connection.QueryFirstOrDefault<int>(query, new { banco = vBanco, usuario = vUsuario, unidad = vUnidad });

                if (resp <= 0)
                {
                    result.Code = -1;
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<bool> fxTesDocumentoVerifica(int CodEmpresa, int vBanco, string vtipo, string vDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select isnull(count(*),0) as Existe
                    from Tes_Transacciones
                    where id_banco = @banco
                      and tipo = @tipo
                      and Ndocumento = @documento
                      and estado <> 'P'";

                var resp = connection.QueryFirstOrDefault<int>(query, new { banco = vBanco, tipo = vtipo, documento = vDocumento });

                if (resp > 0)
                {
                    result.Code = -1;
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public static string fxTESCifrado(string vClave)
        {
            var strPass = new StringBuilder();

            for (int i = 0; i < vClave.Length; i++)
            {
                char cifrado = (char)(vClave[i] + 7);
                strPass.Append(cifrado);
            }

            return strPass.ToString();
        }

        public static string fxStringCifrado(string pCadena)
        {
            if (string.IsNullOrEmpty(pCadena))
                return string.Empty;

            var vResBuilder = new StringBuilder(pCadena.Length * 3);
            for (int i = pCadena.Length - 1; i >= 0; i--)
            {
                int xChar = (int)pCadena[i];
                vResBuilder.Append(xChar.ToString("D3"));
            }

            string vRes = vResBuilder.ToString();

            var deltas = new[] { +1, -5, +7, -13, -2, +3 };
            int vSec = 0;

            var vResXBuilder = new StringBuilder(vRes.Length + vRes.Length / 3);

            for (int i = 0; i < vRes.Length; i += 3)
            {
                int len = Math.Min(3, vRes.Length - i);
                if (!int.TryParse(vRes.AsSpan(i, len), out int num))
                    continue;

                int transformed = num + deltas[vSec];
                vResXBuilder.Append(transformed);

                vSec++;
                if (vSec == deltas.Length) vSec = 0;
            }

            return FxDepuraCadena(vResXBuilder.ToString());
        }

        public static string FxDepuraCadena(string xCadena)
        {
            var vResBuilder = new StringBuilder();

            for (int i = 0; i < xCadena.Length; i += 2)
            {
                string chunk = xCadena.Substring(i, Math.Min(2, xCadena.Length - i));
                if (int.TryParse(chunk, out int num) && num > 31 && num != 39 && num != 34)
                {
                    vResBuilder.Insert(0, (char)num);
                }
            }

            return vResBuilder.ToString();
        }

        public bool fxValidaPermisoUserBancosTipo(int CodEmpresa, int vBanco, string vtipo, string vUsuario, string vPermiso)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                var col = MapPermisoColumn(vPermiso);

                var query = $@"
                    select count(T.Tipo)
                    from tes_tipos_doc T
                    left join tes_documentos_asg A
                           on T.tipo = A.tipo
                          and A.id_banco = @banco
                          and A.nombre = @usuario
                    where T.tipo in (select Tipo from tes_banco_docs where id_banco = @banco)
                      and T.Tipo = @tipo
                      and isnull(A.[{col}],0) = 1";

                return connection.QueryFirstOrDefault<int>(query, new { tipo = vtipo, banco = vBanco, usuario = vUsuario }) > 0;
            }
            catch
            {
                return false;
            }
        }

        public ErrorDto sbTesEmitirDocumento(int CodEmpresa, string vUsuario, int vModulo, int vSolicitud, string vDocumento = "", DateTime? vFecha = null)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    SELECT C.monto, C.nsolicitud, T.doc_auto, T.comprobante,
                           ISNULL(B.firmas_desde,0) AS Firmas_Desde, B.Lugar_Emision,
                           X.descripcion AS TipoX, ISNULL(B.firmas_hasta,0) AS Firmas_Hasta,
                           dbo.MyGetdate() AS FechaX, C.id_Banco, C.tipo, C.modulo,
                           C.op, C.referencia, C.codigo, C.subModulo, C.cod_divisa
                    FROM Tes_Transacciones C
                    INNER JOIN Tes_Bancos B ON C.id_Banco = B.id_banco
                    INNER JOIN tes_banco_docs T ON B.id_Banco = T.id_Banco AND C.tipo = T.tipo
                    INNER JOIN tes_tipos_doc X ON T.tipo = X.tipo
                    WHERE C.nsolicitud = @solicitud";

                var data = connection.QueryFirstOrDefault<MTesTransaccionDto>(query, new { solicitud = vSolicitud });

                if (data == null)
                {
                    response.Code = -3;
                    response.Description = "No se encontró la solicitud especificada.";
                    return response;
                }

                string vTipo = data.tipo;
                string vComprobante = data.comprobante;
                bool vAutoConsec = data.doc_auto;
                DateTime fechaEmision = vFecha ?? data.fechaX;
                string vConsecutivo = string.Empty;

                switch (vComprobante)
                {
                    case "01":
                    case "02":
                    case "03":
                        if (vAutoConsec)
                            vConsecutivo = fxTesTipoDocConsec(CodEmpresa, data.id_banco, vTipo, "+").Result.ToString();

                        string nDocumentoClause = "";
                        if (vAutoConsec || !string.IsNullOrWhiteSpace(vDocumento))
                            nDocumentoClause = ", NDocumento = @documento";

                        var updateSql = @"
                            UPDATE Tes_Transacciones
                            SET Estado = 'I',
                                Fecha_Emision = @fecha,
                                Ubicacion_Actual = 'T',
                                Fecha_Traslado = @fecha,
                                User_Genera = @usuario"
                            + nDocumentoClause +
                            " WHERE nsolicitud = @solicitud";

                        connection.Execute(updateSql, new
                        {
                            fecha = fechaEmision.ToString("yyyy-MM-dd"),
                            usuario = vUsuario,
                            documento = vAutoConsec ? vConsecutivo : vDocumento,
                            solicitud = vSolicitud
                        });

                        sbTESActualizaCC(CodEmpresa, new ActualizaCCParams
                        {
                            Codigo = data.codigo,
                            Tipo = vTipo,
                            Documento = vConsecutivo,
                            Banco = data.id_banco,
                            OP = data.op != null ? (int)data.op : 0,
                            Modulo = data.modulo,
                            SubModulo = data.subModulo,
                            Referencia = data.referencia != null ? (int)data.referencia : 0
                        });

                        break;

                    case "04":
                        response.Code = -1;
                        response.Description = "Las Transferencias Electrónicas no se pueden procesar directamente...";
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public string fxTesTiposDocAsiento(int CodEmpresa, string vTipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"select Movimiento from tes_tipos_doc Where tipo = @tipo";
                return connection.QueryFirstOrDefault<string>(query, new { tipo = vTipo }) ?? "";
            }
            catch
            {
                return "";
            }
        }

        public ErrorDto<bool> fxTesCuentaObligatoriaVerifica(int CodEmpresa, int vBanco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"
                    select isnull(count(*),0) as Existe
                    from Tes_Bancos
                    where id_banco = @banco
                      and INT_REQUIERE_CUENTA_DESTINO = 1";

                var resp = connection.QueryFirstOrDefault<int>(query, new { banco = vBanco });

                if (resp > 0)
                {
                    result.Code = -1;
                    result.Result = true;
                }
                else
                {
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

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

        public ErrorDto<TesReporteTransferenciaDto> sbTesReporteTransferencia(int CodEmpresa, int vBanco, long vTransac, string? vTipo = "C", string? vDocumento = "TE", string? vPlan = "-sp-")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<TesReporteTransferenciaDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesReporteTransferenciaDto()
            };

            decimal curMonto = 0;
            long lngCasos = 0;
            string strDivisa = "", vLetra = "";

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string bancoSql = @"select cta as item, descripcion from Tes_Bancos where id_banco = @vBanco";
                var banco = connection.QueryFirstOrDefault(bancoSql, new { vBanco });

                if (banco != null)
                {
                    vLetra = "Sirva la Presente para saludarlo y a la vez solicitarle debitar de nuestra cuenta corriente"
                           + " # " + banco.item + " la suma de ¢ ";
                }

                if (vTipo == "C")
                {
                    string strSQL = @"
                        select sum(Monto) as Monto, Count(*) as Casos, cod_divisa
                        from Tes_Transacciones
                        where tipo = @vDocumento
                          and id_banco = @vBanco
                          and documento_Base = @vTransac";

                    if (vPlan != "-sp-")
                        strSQL += " and Cod_Plan = @vPlan";

                    strSQL += " group by cod_divisa";

                    var rs = connection.QueryFirstOrDefault(strSQL, new { vDocumento, vBanco, vTransac, vPlan });

                    if (rs != null)
                    {
                        curMonto = rs.Monto;
                        lngCasos = rs.Casos;
                        strDivisa = rs.cod_divisa;
                    }

                    string vMontoLetras = MProGrXAuxiliarDB.NumeroALetras(curMonto).Result + fxDescDivisa(CodEmpresa, strDivisa).Result;

                    resp.Result.registros = lngCasos;
                    resp.Result.montoLetras = vMontoLetras;
                    resp.Result.totalMonto = curMonto;
                    resp.Result.fxNombre = fxTesParametro(CodEmpresa, "01");
                    resp.Result.fxPuesto = fxTesParametro(CodEmpresa, "02");
                    resp.Result.fxDepartamento = fxTesParametro(CodEmpresa, "03");
                    resp.Result.letras1 = vLetra;
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

        public ErrorDto<string> fxDescDivisa(int CodEmpresa, string vDivisa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<string> { Code = 0, Description = "Ok", Result = "" };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string query = @"select top 1 descripcion from CNTX_DIVISAS where cod_divisa = @vDivisa";
                string descripcion = connection.QueryFirstOrDefault<string>(query, new { vDivisa }) ?? "";

                if (!string.IsNullOrEmpty(descripcion))
                {
                    string strDescripcion = descripcion.Trim().ToLowerInvariant();
                    string fxCodText = strDescripcion.Split(' ')[0].Trim();
                    if (string.IsNullOrEmpty(fxCodText)) fxCodText = strDescripcion;

                    fxCodText = char.ToUpper(fxCodText[0]) + fxCodText.Substring(1).Trim();

                    char ultima = fxCodText[^1];
                    if ("aeiouáéíóú".Contains(char.ToLowerInvariant(ultima)))
                        fxCodText += "s";
                    else
                        fxCodText += "es";

                    resp.Result = string.IsNullOrEmpty(fxCodText) ? strDescripcion : fxCodText;
                }
                else
                {
                    resp.Result = " Colones";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }

            return resp;
        }

        public ErrorDto<List<TokenConsultaModel>> spTes_Token_Consulta(int CodEmpresa, string usuario)
        {
            var response = new ErrorDto<List<TokenConsultaModel>> { Code = 0, Result = new() };

            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"exec spTes_Token_Consulta '', 'A' , @usuario";
                response.Result = connection.Query<TokenConsultaModel>(query, new { usuario }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDto spTes_Token_New(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // Stored procedure con parámetro (sin interpolación)
                connection.Execute(
                    "spTes_Token_New",
                    new { usuario = usuario.ToUpperInvariant() },
                    commandType: System.Data.CommandType.StoredProcedure);

                const string query = @"
                    select top 1 ID_TOKEN
                    from Tes_Tokens
                    where REGISTRO_USUARIO = @usuario
                    order by REGISTRO_FECHA desc";

                response.Description = connection.QueryFirstOrDefault<string>(query, new { usuario = usuario.ToUpperInvariant() }) ?? "";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public void sbCrdOperacionTags(int CodEmpresa, long pOperacion, string pLinea, string pTag, string pUsuario, string? pAsignado = "", string? pNotas = "")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                connection.Execute(
                    "[spCrdOperacionTagRegistra]",
                    new
                    {
                        Operacion = pOperacion,
                        CrdLinea = pLinea,
                        Tag = pTag,
                        Usuario = pUsuario,
                        Asignado = pAsignado,
                        Notas = pNotas
                    },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }
    }
}