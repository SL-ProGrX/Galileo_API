using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrConsultaCrdExcDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 3;

        private const string ParametroLineaExcedente = "05";
        private const string ParametroMontoMaximoRetiroCaja = "15";
        private const string TipoDocumentoCheque = "CK";
        private const string TipoDocumentoTransferencia = "TE";
        private const string TipoDocumentoRetiroCaja = "RC";
        private const string AplicacionFormalizacion = "FrmCR_ConsultaCrdExc";

        public FrmCrConsultaCrdExcDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la información inicial de la consulta de crédito por excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCrdExcInicialDto> CR_ConsultaCrdExc_Inicial_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var ctx = CrearContextoInicial(cedula, usuario);
                var validacion = ValidarContextoConsulta(ctx);

                if (validacion.Code != 0)
                {
                    return DbHelper.CreateErrorResponse<CrConsultaCrdExcInicialDto>(validacion.Description ?? string.Empty);
                }

                var result = new CrConsultaCrdExcInicialDto
                {
                    cedula = ctx.Cedula,
                    linea = ObtenerLineaExcedente(conn),
                    tipos_documento = ObtenerTiposDocumento()
                };

                CargarValidacionPersona(conn, result);
                result.bancos = ObtenerBancos(conn, ctx.Usuario);
                result.recursos = ObtenerRecursos(conn, result.linea);
                result.resumen = ObtenerResumenExcedente(conn, ctx.Cedula);
                result.nombre = result.resumen.nombre;

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCrdExcInicialDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las cuentas bancarias de una persona por banco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdExcCuentaBancoDto>> CR_ConsultaCrdExc_CuentasBanco_Obtener(int CodEmpresa, string cedula, int banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var ctx = CrearContextoBanco(cedula, banco);
                var validacion = ValidarContextoBanco(ctx);

                if (validacion.Code != 0)
                {
                    return DbHelper.CreateErrorResponse<List<CrConsultaCrdExcCuentaBancoDto>>(validacion.Description ?? string.Empty);
                }

                var lista = conn.Query<CrConsultaCrdExcCuentaBancoDto>(
                    "spSys_Cuentas_Bancarias",
                    new
                    {
                        Identificacion = ctx.Cedula,
                        BancoId = ctx.Banco,
                        DivisaCheck = 1
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrConsultaCrdExcCuentaBancoDto>>(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene la oficina asignada al usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCrdExcOficinaUsuarioDto> CR_ConsultaCrdExc_OficinaUsuario_Obtener(int CodEmpresa, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                usuario = (usuario ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(usuario))
                {
                    return DbHelper.CreateErrorResponse<CrConsultaCrdExcOficinaUsuarioDto>("Debe indicar el usuario.");
                }

                var result = ObtenerOficinaUsuario(conn, usuario);

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCrdExcOficinaUsuarioDto>(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene el disponible del recurso seleccionado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="recurso"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCrdExcDisponibleRecursoDto> CR_ConsultaCrdExc_DisponibleRecurso_Obtener(int CodEmpresa, string recurso)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                recurso = (recurso ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(recurso))
                {
                    return DbHelper.CreateErrorResponse<CrConsultaCrdExcDisponibleRecursoDto>("Debe seleccionar el recurso.");
                }

                var fecha = ObtenerFechaServidor(conn);
                var result = ObtenerDisponibleRecurso(conn, recurso, fecha);

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCrdExcDisponibleRecursoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Formaliza el crédito de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCrdExcFormalizarDto> CR_ConsultaCrdExc_Formalizar(int CodEmpresa, CrConsultaCrdExcFormalizarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var ctx = CrearContextoFormalizacion(request);
                var validacion = ValidarFormalizacion(conn, ctx);

                if (validacion.Code != 0)
                {
                    return DbHelper.CreateErrorResponse<CrConsultaCrdExcFormalizarDto>(validacion.Description ?? string.Empty);
                }

                var operacion = EjecutarFormalizacion(conn, ctx);

                if (operacion <= 0)
                {
                    return DbHelper.CreateErrorResponse<CrConsultaCrdExcFormalizarDto>("No se generó la operación.");
                }

                RegistrarBitacoraFormalizacion(CodEmpresa, ctx, operacion);

                return DbHelper.CreateOkResponse(new CrConsultaCrdExcFormalizarDto
                {
                    operacion = operacion
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCrdExcFormalizarDto>(ex.Message);
            }
        }

        /// <summary>
        /// Valida el contexto común de consulta.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarContextoConsulta(CrConsultaCrdExcConsultaContext ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.Cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar la cédula.");
            }

            if (string.IsNullOrWhiteSpace(ctx.Usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.");
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida el contexto común de consulta.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarContextoBanco(CrConsultaCrdExcBancoContext ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.Cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar la cédula.");
            }

            if (ctx.Banco <= 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar el banco.");
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida el contexto común de consulta.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarFormalizacion(SqlConnection conn, CrConsultaCrdExcFormalizarContext ctx)
        {
            var validacionBase = ValidarFormalizacionBase(ctx);

            if (validacionBase.Code != 0)
            {
                return validacionBase;
            }

            var condicion = ObtenerCondicionPersona(conn, ctx.Cedula);
            var mora = ObtenerMoraPersona(conn, ctx.Cedula);

            if (condicion != 0)
            {
                return DbHelper.ErrorResponse(ObtenerMensajeCondicion(condicion, mora));
            }

            return ValidarMontosFormalizacion(conn, ctx);
        }
        /// <summary>
        /// Obtiene la oficina asignada al usuario.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static CrConsultaCrdExcOficinaUsuarioDto ObtenerOficinaUsuario(SqlConnection conn, string usuario)
        {
            return conn.QueryFirstOrDefault<CrConsultaCrdExcOficinaUsuarioDto>(
                "sbSIFOficinasUsuario",
                new
                {
                    Usuario = usuario
                },
                commandType: CommandType.StoredProcedure) ?? new CrConsultaCrdExcOficinaUsuarioDto();
        }
        /// <summary>
        /// Valida los datos base de formalización.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarFormalizacionBase(CrConsultaCrdExcFormalizarContext ctx)
        {
            if (string.IsNullOrWhiteSpace(ctx.Cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar la cédula.");
            }

            if (string.IsNullOrWhiteSpace(ctx.Linea))
            {
                return DbHelper.ErrorResponse("No se encontró la línea de excedente.");
            }

            if (ctx.Monto <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar un monto válido.");
            }

            if (ctx.Banco <= 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar el banco.");
            }

            if (string.IsNullOrWhiteSpace(ctx.TipoDocumento))
            {
                return DbHelper.ErrorResponse("Debe seleccionar el tipo de documento.");
            }

            if (ctx.TipoDocumento == TipoDocumentoTransferencia && string.IsNullOrWhiteSpace(ctx.CuentaBanco))
            {
                return DbHelper.ErrorResponse("Debe seleccionar la cuenta bancaria.");
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida montos y límites de formalización.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarMontosFormalizacion(SqlConnection conn, CrConsultaCrdExcFormalizarContext ctx)
        {
            var resumen = ObtenerResumenExcedente(conn, ctx.Cedula);

            if (ctx.Monto > resumen.giro_maximo)
            {
                return DbHelper.ErrorResponse("El monto a aplicar no puede ser mayor al giro máximo neto.");
            }

            var recurso = ObtenerRecursoLinea(conn, ctx.Linea);
            var disponible = ObtenerDisponibleRecurso(conn, recurso, ObtenerFechaServidor(conn));

            if (ctx.Monto > disponible.disponible)
            {
                return DbHelper.ErrorResponse("El monto a aplicar no puede ser mayor al disponible del recurso.");
            }

            return ValidarRetiroCaja(conn, ctx);
        }

        /// <summary>
        /// Valida el límite de retiro por caja.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static ErrorDto ValidarRetiroCaja(SqlConnection conn, CrConsultaCrdExcFormalizarContext ctx)
        {
            if (ctx.TipoDocumento != TipoDocumentoRetiroCaja)
            {
                return DbHelper.OkResponse("Ok");
            }

            var montoMaximo = ObtenerMontoMaximoRetiroCaja(conn);

            if (ctx.Monto > montoMaximo)
            {
                return DbHelper.ErrorResponse($"El monto máximo para retiro en caja es {montoMaximo:N2}.");
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Carga la validación de condiciones de la persona.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="result"></param>
        private static void CargarValidacionPersona(SqlConnection conn, CrConsultaCrdExcInicialDto result)
        {
            result.condicion = ObtenerCondicionPersona(conn, result.cedula);
            result.mora = ObtenerMoraPersona(conn, result.cedula);
            result.mensaje_condicion = ObtenerMensajeCondicion(result.condicion, result.mora);
        }

        /// <summary>
        /// Obtiene la línea configurada para crédito de excedentes.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static string ObtenerLineaExcedente(SqlConnection conn)
        {
            const string sql = @"
                select rtrim(valor)
                from EXC_PARAMETROS
                where COD_PARAMETRO = @codigo;";

            return conn.QueryFirstOrDefault<string>(sql, new
            {
                codigo = ParametroLineaExcedente
            })?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Obtiene la mora y condiciones del expediente.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private static decimal ObtenerMoraPersona(SqlConnection conn, string cedula)
        {
            const string sql = @"
                select isnull(sum(intc + intm + cargos + amortiza), 0)
                from VISTA_MOROSIDAD
                where cedula = @cedula;";

            return conn.QueryFirstOrDefault<decimal>(sql, new
            {
                cedula
            });
        }

        /// <summary>
        /// Obtiene la condición de validación de la persona.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private static short ObtenerCondicionPersona(SqlConnection conn, string cedula)
        {
            const string sql = @"select dbo.fxCrdPersonaValidaCondiciones(@cedula);";

            return conn.QueryFirstOrDefault<short>(sql, new
            {
                cedula
            });
        }

        /// <summary>
        /// Obtiene el resumen del crédito de excedentes.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private static CrConsultaCrdExcResumenDto ObtenerResumenExcedente(SqlConnection conn, string cedula)
        {
            var raw = conn.QueryFirstOrDefault<CrConsultaCrdExcResumenDbDto>(
                "spVoxExcedenteCredito",
                new
                {
                    Cedula = cedula
                },
                commandType: CommandType.StoredProcedure) ?? new CrConsultaCrdExcResumenDbDto();

            return new CrConsultaCrdExcResumenDto
            {
                periodo_de = raw.Periodo_De,
                periodo_hasta = raw.Periodo_Hasta,
                mes_aplicado = raw.Mes_Aplicado,
                bruto = raw.Bruto,
                por_cap_gen = raw.PorCapGen,
                capitalizacion = raw.Capitalizacion,
                por_renta = raw.PorRenta,
                renta = raw.Renta,
                por_acumulado = raw.PorAcumulado,
                base_credito = raw.Base,
                saldos = raw.Saldos,
                por_cap_ind = raw.PorCapInd,
                cap_individual = raw.CapIndividual,
                neto = raw.Neto,
                dias = raw.Dias,
                tasa = raw.Tasa,
                intereses = raw.Intereses,
                giro_maximo = raw.Giro_Maximo,
                nombre = raw.Nombre,
                poliza_factor = raw.PolizaFactor
            };
        }

        /// <summary>
        /// Obtiene los bancos habilitados para el usuario.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static List<CrConsultaCrdExcBancoDto> ObtenerBancos(SqlConnection conn, string usuario)
        {
            return conn.Query<CrConsultaCrdExcBancoDto>(
                "spCrd_SGT_Bancos",
                new
                {
                    Usuario = usuario
                },
                commandType: CommandType.StoredProcedure).ToList();
        }

        /// <summary>
        /// Obtiene los recursos asociados a la línea.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        private static List<CrConsultaCrdExcRecursoDto> ObtenerRecursos(SqlConnection conn, string linea)
        {
            const string sql = @"
                select
                    G.cod_grupo as IdX,
                    rtrim(G.descripcion) as ItmX
                from catalogo_grupos G
                inner join catalogo_asignaGrp A
                    on G.cod_grupo = A.cod_grupo
                where G.estado = 1
                  and A.codigo = @linea;";

            return conn.Query<CrConsultaCrdExcRecursoDto>(sql, new
            {
                linea
            }).ToList();
        }

        /// <summary>
        /// Obtiene el primer recurso asociado a la línea.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        private static string ObtenerRecursoLinea(SqlConnection conn, string linea)
        {
            const string sql = @"
                select top 1 rtrim(cod_grupo)
                from catalogo_asignaGrp
                where codigo = @linea;";

            return conn.QueryFirstOrDefault<string>(sql, new
            {
                linea
            })?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el disponible del recurso.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="recurso"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        private static CrConsultaCrdExcDisponibleRecursoDto ObtenerDisponibleRecurso(SqlConnection conn, string recurso, DateTime fecha)
        {
            return conn.QueryFirstOrDefault<CrConsultaCrdExcDisponibleRecursoDto>(
                "spCRDDisponibleRecurso",
                new
                {
                    RECURSO = recurso,
                    FECHA = fecha.ToString("yyyy/MM/dd")
                },
                commandType: CommandType.StoredProcedure) ?? new CrConsultaCrdExcDisponibleRecursoDto();
        }

        /// <summary>
        /// Obtiene la fecha del servidor.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static DateTime ObtenerFechaServidor(SqlConnection conn)
        {
            const string sql = @"select dbo.MyGetdate();";
            return conn.QueryFirstOrDefault<DateTime>(sql);
        }

        /// <summary>
        /// Obtiene el monto máximo permitido para retiro por caja.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private static decimal ObtenerMontoMaximoRetiroCaja(SqlConnection conn)
        {
            const string sql = @"
                select try_convert(decimal(16,2), valor)
                from CAJAS_PARAMETROS
                where COD_PARAMETRO = @codigo;";

            return conn.QueryFirstOrDefault<decimal?>(sql, new
            {
                codigo = ParametroMontoMaximoRetiroCaja
            }) ?? 0;
        }

        /// <summary>
        /// Ejecuta el proceso de formalización del crédito.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static int EjecutarFormalizacion(SqlConnection conn, CrConsultaCrdExcFormalizarContext ctx)
        {
            var appCod = string.IsNullOrWhiteSpace(ctx.AppCod)
                ? AplicacionFormalizacion
                : ctx.AppCod;

            return conn.QueryFirstOrDefault<int>(
                "spCrdCreditoExcedentesRapido",
                new
                {
                    Linea = ctx.Linea,
                    Cedula = ctx.Cedula,
                    MontoSol = ctx.Monto,
                    Banco = ctx.Banco,
                    Emitir = ctx.TipoDocumento,
                    CuentaBanco = ctx.CuentaBanco,
                    Usuario = ctx.Usuario,
                    Oficina = ctx.Oficina,
                    AppCod = appCod
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Registra bitácora de formalización.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ctx"></param>
        /// <param name="operacion"></param>
        private void RegistrarBitacoraFormalizacion(int CodEmpresa, CrConsultaCrdExcFormalizarContext ctx, int operacion)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = ctx.Usuario,
                DetalleMovimiento = $"Formaliza crédito de excedentes. Operación: {operacion}, Cédula: {ctx.Cedula}, Monto: {ctx.Monto:N2}",
                Movimiento = "INSERTA-WEB",
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Obtiene el mensaje de validación de condiciones.
        /// </summary>
        /// <param name="condicion"></param>
        /// <param name="mora"></param>
        /// <returns></returns>
        private static string ObtenerMensajeCondicion(short condicion, decimal mora)
        {
            return condicion switch
            {
                1 => mora > 0
                    ? $"Esta persona tienen una morosidad de : {mora:N2}{Environment.NewLine} -> No puede formalizar desde esta opción!"
                    : "Esta persona presenta morosidad. -> No puede formalizar desde esta opción!",
                2 => $"Esta persona Presenta Operaciones con Traslado de Deudas{Environment.NewLine} -> No puede formalizar desde esta opción!",
                3 => $"Esta persona Tiene Operaciones en Cobro Judicial!{Environment.NewLine} -> No puede formalizar desde esta opción!",
                4 => $"Esta persona se encuentra bloqueada para nuevas operaciones {Environment.NewLine} -> No puede formalizar desde esta opción!",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Obtiene los tipos de documento permitidos.
        /// </summary>
        /// <returns></returns>
        private static List<CrConsultaCrdExcTipoDocumentoDto> ObtenerTiposDocumento()
        {
            return new List<CrConsultaCrdExcTipoDocumentoDto>
            {
                new() { item = TipoDocumentoCheque, descripcion = "Cheque" },
                new() { item = TipoDocumentoTransferencia, descripcion = "Transferencia" },
                new() { item = TipoDocumentoRetiroCaja, descripcion = "Retiro Caja" }
            };
        }

        /// <summary>
        /// Crea el contexto inicial de consulta.
        /// </summary>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static CrConsultaCrdExcConsultaContext CrearContextoInicial(string cedula, string usuario)
        {
            return new CrConsultaCrdExcConsultaContext
            {
                Cedula = (cedula ?? string.Empty).Trim(),
                Usuario = (usuario ?? string.Empty).Trim()
            };
        }

        /// <summary>
        /// Crea el contexto de cuentas bancarias.
        /// </summary>
        /// <param name="cedula"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        private static CrConsultaCrdExcBancoContext CrearContextoBanco(string cedula, int banco)
        {
            return new CrConsultaCrdExcBancoContext
            {
                Cedula = (cedula ?? string.Empty).Trim(),
                Banco = banco
            };
        }

        /// <summary>
        /// Crea el contexto de formalización.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static CrConsultaCrdExcFormalizarContext CrearContextoFormalizacion(CrConsultaCrdExcFormalizarRequest request)
        {
            request ??= new CrConsultaCrdExcFormalizarRequest();

            return new CrConsultaCrdExcFormalizarContext
            {
                Cedula = (request.cedula ?? string.Empty).Trim(),
                Linea = (request.linea ?? string.Empty).Trim(),
                Monto = request.monto.GetValueOrDefault(),
                Banco = request.banco.GetValueOrDefault(),
                TipoDocumento = (request.tipo_documento ?? string.Empty).Trim().ToUpperInvariant(),
                CuentaBanco = (request.cuenta_banco ?? string.Empty).Trim(),
                Usuario = (request.usuario ?? string.Empty).Trim(),
                Oficina = (request.oficina ?? string.Empty).Trim(),
                AppCod = (request.app_cod ?? string.Empty).Trim()
            };
        }

        private sealed class CrConsultaCrdExcConsultaContext
        {
            public string Cedula { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
        }

        private sealed class CrConsultaCrdExcBancoContext
        {
            public string Cedula { get; set; } = string.Empty;
            public int Banco { get; set; }
        }

        private sealed class CrConsultaCrdExcFormalizarContext
        {
            public string Cedula { get; set; } = string.Empty;
            public string Linea { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public int Banco { get; set; }
            public string TipoDocumento { get; set; } = string.Empty;
            public string CuentaBanco { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string Oficina { get; set; } = string.Empty;
            public string AppCod { get; set; } = string.Empty;
        }
        public sealed class CrConsultaCrdExcResumenDbDto
        {
            public DateTime? Periodo_De { get; set; }
            public DateTime? Periodo_Hasta { get; set; }
            public short Mes_Aplicado { get; set; }
            public decimal Bruto { get; set; }
            public decimal PorCapGen { get; set; }
            public decimal Capitalizacion { get; set; }
            public decimal PorRenta { get; set; }
            public decimal Renta { get; set; }
            public decimal PorAcumulado { get; set; }
            public decimal Base { get; set; }
            public decimal Saldos { get; set; }
            public decimal PorCapInd { get; set; }
            public decimal CapIndividual { get; set; }
            public decimal Neto { get; set; }
            public short Dias { get; set; }
            public decimal Tasa { get; set; }
            public decimal Intereses { get; set; }
            public decimal Giro_Maximo { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public decimal PolizaFactor { get; set; }
        }
    }
}