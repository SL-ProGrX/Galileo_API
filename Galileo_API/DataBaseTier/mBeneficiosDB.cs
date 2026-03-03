using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.mBeneficios;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class MBeneficiosDB
    {
        private readonly IConfiguration _config;

        public MBeneficiosDB(IConfiguration config)
        {
            _config = config;
        }

        #region Conexión + helpers base

        private SqlConnection CreateEmpresaConnection(int codEmpresa)
        {
            var connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            if (string.IsNullOrWhiteSpace(connString))
                throw new InvalidOperationException("Cadena de conexión de empresa no configurada.");

            return new SqlConnection(connString);
        }

        private ErrorDto WithConn(int codEmpresa, Func<SqlConnection, ErrorDto> work, string opName)
        {
            try
            {
                using var connection = CreateEmpresaConnection(codEmpresa);
                return work(connection);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = $"{opName} - {ex.Message}" };
            }
        }

        private static BeneCategoriaValidaListaRequest ToValidaRequest(BeneficioGeneralDatos b) => new()
        {
            cedula = b.cedula,
            
            cod_beneficio = b.id_beneficio.ToString(CultureInfo.InvariantCulture),
            usuario = b.registra_user,
            cod_categoria = b.cod_categoria,
            monto_usuario = Convert.ToDecimal(b.monto_aplicado).ToString(CultureInfo.InvariantCulture),
            sepelio_identificacion = b.sepelio_identificacion
        };

        private static DynamicParameters BuildParams(BeneCategoriaValidaListaRequest request)
        {
            var p = new DynamicParameters();
            p.Add("cod_categoria", request.cod_categoria);
            p.Add("cod_beneficio", request.cod_beneficio);
            p.Add("cedula", request.cedula);
            p.Add("usuario", request.usuario);
            p.Add("id_beneficio", request.id_beneficio);
            p.Add("monto_usuario", request.monto_usuario);
            p.Add("sepelio_identificacion", request.sepelio_identificacion);
            return p;
        }



        #endregion

        #region Métodos simples (sin tocar lógica)

        public ErrorDto fxNombre(int CodEmpresa, string cedula)
        {
            return WithConn(CodEmpresa, connection =>
            {
                const string query = "select nombre from socios where cedula = @cedula";
                var nombre = connection.Query<string>(query, new { cedula = cedula.Trim() }).FirstOrDefault();

                return new ErrorDto { Code = 0, Description = nombre };
            }, nameof(fxNombre));
        }

        public ErrorDto fxDescribeBanco(int CodEmpresa, int codBanco)
        {
            return WithConn(CodEmpresa, connection =>
            {
                const string query = "select descripcion from Tes_Bancos where id_banco = @codBanco";
                var desc = connection.Query<string>(query, new { codBanco }).FirstOrDefault();

                return new ErrorDto { Code = 0, Description = desc };
            }, nameof(fxDescribeBanco));
        }

        public static string fxEstadoBeneficio(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return "DESCONOCIDO";

            return estado.ToUpperInvariant() switch
            {
                "A" => "APROBADO",
                "S" => "SOLICITADO",
                "R" => "RECHAZADO",
                "E" => "EJECUTADO",
                "P" => "PENDIENTE",
                "APROBADO" => "A",
                "SOLICITADO" => "S",
                "RECHAZADO" => "R",
                "EJECUTADO" => "E",
                "PENDIENTE" => "P",
                _ => "DESCONOCIDO"
            };
        }

        public string fxSIFParametros(int CodEmpresa, string cod_parametro)
        {
            try
            {
                using var connection = CreateEmpresaConnection(CodEmpresa);
                const string query = "Select valor from SIF_parametros where cod_parametro = @cod_parametro";
                return connection.Query<string>(query, new { cod_parametro }).FirstOrDefault() ?? string.Empty;
            }
            catch
            {
                return "N";
            }
        }

        public string fxFSL_Parametros(int CodEmpresa, string cod_parametro)
        {
            try
            {
                using var connection = CreateEmpresaConnection(CodEmpresa);
                const string query = "select valor from fsl_parametros where cod_parametro = @cod_parametro";
                return connection.Query<string>(query, new { cod_parametro }).FirstOrDefault() ?? string.Empty;
            }
            catch
            {
                return "N";
            }
        }

        public ErrorDto BitacoraBeneficios(BitacoraBeneInsertarDto req)
        {
            return WithConn(req.EmpresaId, connection =>
            {
                var strSQL = @"
                    INSERT INTO [dbo].[AFI_BENE_REGISTRO_BITACORA]
                               ([COD_BENEFICIO]
                               ,[CONSEC]
                               ,[MOVIMIENTO]
                               ,[DETALLE]
                               ,[REGISTRO_FECHA]
                               ,[REGISTRO_USUARIO])
                         VALUES
                               (@cod_beneficio
                               ,@consec
                               ,@movimiento
                               ,@detalle
                               ,getdate()
                               ,@registro_usuario)";

                var rows = connection.Execute(strSQL, new
                {
                    req.cod_beneficio,
                    req.consec,
                    req.movimiento,
                    req.detalle,
                    req.registro_usuario
                });

                return new ErrorDto { Code = rows, Description = "Ok" };
            }, nameof(BitacoraBeneficios));
        }

        public long fxConsec(int CodCliente, string cod_beneficio)
        {
            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);
                const string query = @"Select isnull(Max(consec),0) as consecutivo 
                                       from afi_bene_otorga 
                                       where cod_beneficio = @cod_beneficio";
                return connection.Query<long>(query, new { cod_beneficio }).FirstOrDefault() + 1;
            }
            catch
            {
                return 0;
            }
        }

        public ErrorDto<BeneficioGeneralDatos> ValidaEstadoSocio(int CodCliente, string cedula)
        {
            try
            {
                using var connection = CreateEmpresaConnection(CodCliente);
                const string query = @"SELECT ESTADOACTUAL FROM SOCIOS WHERE CEDULA = @cedula";
                var estado = connection.Query<string>(query, new { cedula }).FirstOrDefault();

                if (estado != "S")
                {
                    return new ErrorDto<BeneficioGeneralDatos>
                    {
                        Code = -1,
                        Description = "El asociado se encuentra inactivo",
                        Result = null
                    };
                }

                return new ErrorDto<BeneficioGeneralDatos> { Code = 0, Description = "Ok", Result = null };
            }
            catch (Exception ex)
            {
                return new ErrorDto<BeneficioGeneralDatos>
                {
                    Code = -1,
                    Description = "ValidaEstadoSocio - " + ex.Message,
                    Result = null
                };
            }
        }

        public ErrorDto ValidaRequisitos(int CodCliente, string estado, string cod_beneficio, int consec)
        {
            return WithConn(CodCliente, connection =>
            {
                const string dtEstado = @"
                    SELECT COD_ESTADO
                    FROM [dbo].[AFI_BENE_ESTADOS]
                    WHERE COD_ESTADO = @estado 
                      AND P_FINALIZA = 1 
                      AND PROCESO = 'A'";

                var finaliza = connection.Query<string>(dtEstado, new { estado }).FirstOrDefault();
                if (finaliza == null)
                    return new ErrorDto { Code = 0 };

                var query = @"
                    SELECT 
                        CASE 
                            WHEN COUNT(CASE WHEN R.REQUERIDO = 1 AND RR.COD_BENEFICIO IS NOT NULL THEN 1 END) 
                                 = COUNT(CASE WHEN R.REQUERIDO = 1 THEN 1 END)
                            THEN 0
                            ELSE 1
                        END AS CumplenRequisito
                    FROM [AFI_BENE_GRUPO_REQUISITOS] GR
                    LEFT JOIN AFI_BENE_REQUISITOS R 
                        ON R.COD_REQUISITO = GR.COD_REQUISITO
                    LEFT JOIN AFI_BENE_REGISTRO_REQUISITOS RR 
                        ON RR.COD_REQUISITO = GR.COD_REQUISITO
                       AND RR.COD_BENEFICIO = @cod_beneficio
                       AND RR.CONSEC = @consec
                    WHERE GR.COD_GRUPO = 
                          (SELECT COD_GRUPO 
                           FROM AFI_BENEFICIOS 
                           WHERE COD_BENEFICIO = @cod_beneficio)";

                var cumple = connection.Query<int>(query, new { cod_beneficio, consec }).FirstOrDefault();
                if (cumple == 1)
                    return new ErrorDto { Code = -1, Description = "No cumple con los requisitos del beneficio" };

                return new ErrorDto { Code = 0 };
            }, nameof(ValidaRequisitos));
        }

        public ErrorDto ValidaFallecido(int CodCliente, string cedulafallecido)
        {
            return WithConn(CodCliente, connection =>
            {
                const string query = @"
                    SELECT CONCAT(O.ID_BENEFICIO, TRIM(O.COD_BENEFICIO), FORMAT(O.CONSEC,'00000'), '- cédula: ', O.CEDULA) as Texto
                    FROM AFI_BENE_OTORGA O 
                    WHERE SEPELIO_IDENTIFICACION = @cedulafallecido";

                var fallecido = connection.Query<string>(query, new { cedulafallecido }).ToList();

                if (fallecido.Count == 0)
                    return new ErrorDto { Code = 0 };

                var otros = new StringBuilder();
                foreach (var item in fallecido)
                    otros.Append(item).Append(" - ");

                return new ErrorDto
                {
                    Code = -1,
                    Description = "La cédula del fallecido se encuentra en los siguientes expedientes: " + otros
                };
            }, nameof(ValidaFallecido));
        }

        #endregion

        #region Validaciones unificadas

        public ErrorDto ValidarPersona(int CodCliente, string cedula, string? cod_beneficio)
        {
           
            var req = new BeneCategoriaValidaListaRequest
            {
                cedula = cedula,
                cod_beneficio = cod_beneficio
            };

            return fxValidaciones(
                CodCliente,
                tipo: "P",
                col: QuerysStringValidaciones.registroVal,
                request: req,
                codeOnMatch: 0,
                marcarJustificables: false);
        }

        public ErrorDto ValidarPersonaPago(int CodCliente, string cedula, string? cod_beneficio)
        {
           
            var req = new BeneCategoriaValidaListaRequest
            {
                cedula = cedula,
                cod_beneficio = cod_beneficio
            };

            return fxValidaciones(
                CodCliente,
                tipo: "P",
                col: QuerysStringValidaciones.pagoVal,
                request: req,
                codeOnMatch: 0,
                marcarJustificables: false);
        }

        public ErrorDto ValidarBeneficioDato(int CodCliente, BeneficioGeneralDatos beneficio)
        {
           
            return fxValidaciones(
                CodCliente,
                tipo: "G",
                col: QuerysStringValidaciones.registroVal,
                request: ToValidaRequest(beneficio),
                codeOnMatch: -1,
                marcarJustificables: false);
        }

        public ErrorDto ValidarBeneficioPagoDato(int CodCliente, BeneficioGeneralDatos beneficio)
        {
           
            return fxValidaciones(
                CodCliente,
                tipo: "G",
                col: QuerysStringValidaciones.pagoVal,
                request: ToValidaRequest(beneficio),
                codeOnMatch: -1,
                marcarJustificables: false);
        }

        public ErrorDto ValidaCargaPagos(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            // Mantengo tu semántica actual: mensajes con codeOnMatch=0,
            // y si viene pago_justifica, se marca con **...**
            return fxValidaciones(
                CodCliente,
                tipo: "!G",
                col: QuerysStringValidaciones.pagoVal,
                request: ToValidaRequest(beneficio),
                codeOnMatch: 0,
                marcarJustificables: true);
        }

        public ErrorDto ValidarBeneficioJustificaDato(int CodCliente, BeneficioGeneralDatos beneficio, bool justifica)
        {
            // Mantengo tu lógica original (obligatorias/justificadas/activas) pero con helper para el foreach.
            return fxValidacionesJustifica(
                CodCliente,
                col: QuerysStringValidaciones.registroVal,
                request: ToValidaRequest(beneficio),
                justifica: justifica);
        }

        public ErrorDto ValidarBeneficioPagoJustificaDato(int CodCliente, BeneficioGeneralDatos beneficio, bool justifica)
        {
            return fxValidacionesJustifica(
                CodCliente,
                col: QuerysStringValidaciones.pagoVal,
                request: ToValidaRequest(beneficio),
                justifica: justifica);
        }

        /// <summary>
        /// Ejecuta validaciones estándar (persona/beneficio/carga pagos) usando query_val parametrizada con Dapper.
        /// </summary>
        public ErrorDto fxValidaciones(
            int CodCliente,
            string tipo,
            string col,
            BeneCategoriaValidaListaRequest request,
            int codeOnMatch,
            bool marcarJustificables)
        {
            return WithConn(CodCliente, connection =>
            {
                var query = QuerysStringValidaciones.ResolveQuery(tipo, col, request.cod_beneficio);
                var validaciones = connection
                    .Query<ValidacionRow>(query, new { cod_beneficio = request.cod_beneficio })
                    .ToList();

                var parms = BuildParams(request);
                var sb = new StringBuilder();
                var code = 0;

                foreach (var v in validaciones)
                {
                    if (string.IsNullOrWhiteSpace(v.query_val)) continue;

                    var result = connection.QueryFirstOrDefault<int>(v.query_val, parms);
                    if (result != v.resultado_val) continue;

                    code = codeOnMatch;

                    var msg = FormatMsg(v.msj_val, marcarJustificables && v.pago_justifica);
                    if (msg.Length > 0) sb.AppendLine(msg);
                }

                return new ErrorDto
                {
                    Code = code,
                    Description = sb.ToString()
                };
            }, nameof(fxValidaciones));
        }

        private static string FormatMsg(string? raw, bool destacar)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            // Mantengo tu formato "...", y el marcado con ** **
            return destacar
                ? $" ** {raw} **..."
                : $"{raw}...";
        }

        /// <summary>
        /// Ejecuta validaciones con lógica de "justifica" (registro/pago), conservando comportamiento original.
        /// </summary>
        private ErrorDto fxValidacionesJustifica(
    int CodCliente,
    string col,
    BeneCategoriaValidaListaRequest request,
    bool justifica)
        {
            return WithConn(CodCliente, connection =>
            {
                var query = BuildJustificaQuery(col);

                var validaciones = connection
                    .Query<ValidacionRow>(query, new { cod_beneficio = request.cod_beneficio })
                    .ToList();

                var parms = BuildParams(request);
                var sb = new StringBuilder();

                int obligatorias = 0;
                int justificadas = 0;

                foreach (var v in validaciones)
                {
                    if (string.IsNullOrWhiteSpace(v.query_val)) continue;

                    var result = connection.QueryFirstOrDefault<int>(v.query_val, parms);
                    if (result != v.resultado_val) continue;

                    obligatorias++;

                    if (IsJustificable(col, v)) justificadas++;

                    AppendMsg(sb, v.msj_val);
                }

                var desc = sb.ToString();
                var code = DecideJustificaCode(justifica, justificadas, obligatorias, desc);

                return new ErrorDto { Code = code, Description = desc };
            }, nameof(fxValidacionesJustifica));
        }

        private static string BuildJustificaQuery(string col)
        {
            // por tu lógica original: TIPO <> 'G'
            const string tipo = "!G";

            return QuerysStringValidaciones.BuildCategoriaQuery(
                tipo: tipo,
                col: col,
                incluirPagoJustifica: col == QuerysStringValidaciones.pagoVal,
                incluirRegistroJustifica: col == QuerysStringValidaciones.registroVal);
        }

        private static bool IsJustificable(string col, ValidacionRow v) =>
            col == QuerysStringValidaciones.registroVal ? v.registro_justifica : v.pago_justifica;

        private static void AppendMsg(StringBuilder sb, string? msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            sb.Append(msg).AppendLine("...");
        }

        private static int DecideJustificaCode(bool justifica, int justificadas, int obligatorias, string desc)
        {
            // Misma regla que tenías:
            if (justificadas > 0 && !justifica) return -1;

            var activas = obligatorias - justificadas;
            if (activas > 0 && !string.IsNullOrWhiteSpace(desc)) return -1;

            return 0;
        }

        #endregion
    }
}