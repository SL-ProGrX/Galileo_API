using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasAperturaDb
    {
        private readonly PortalDB _portalDb;

        public FrmCajasAperturaDb(IConfiguration? config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        private const string MensajeOk = "Ok";
        private const int CodigoErrorValidacion = -2;

        private static ErrorDto<T> CrearRespuestaOk<T>(T? result = default)
        {
            return new ErrorDto<T>
            {
                Code = 0,
                Description = MensajeOk,
                Result = result
            };
        }

        private static ErrorDto<T> CrearRespuestaValidacion<T>(string descripcion, T result)
        {
            return new ErrorDto<T>
            {
                Code = CodigoErrorValidacion,
                Description = descripcion,
                Result = result
            };
        }

        private static void AsignarError<T>(ErrorDto<T> response, Exception ex)
        {
            response.Code = -1;
            response.Description = ex.Message;
            response.Result = default;
        }


        private static void AsignarResultadoError<T>(ErrorDto<T> response, ErrorDto error)
        {
            response.Code = error.Code;
            response.Description = error.Description;
        }

        private static void IntentarRollback(SqlConnection? connection, SqlTransaction? transaction)
        {
            if (connection?.State != System.Data.ConnectionState.Open)
            {
                return;
            }

            try
            {
                transaction?.Rollback();
            }
            catch (InvalidOperationException)
            {
                // La transacción ya no estaba activa.
            }
            catch (SqlException)
            {
                // Error de SQL al intentar revertir; se conserva el error original.
            }
        }


        private static object CrearParametrosCaja(string codCaja)
        {
            return new { CodCaja = codCaja };
        }

        private static int ConsultarEnteroCaja(
            SqlConnection connection,
            SqlTransaction transaction,
            string query,
            string codCaja)
        {
            return connection.QuerySingle<int>(
                query,
                CrearParametrosCaja(codCaja),
                transaction: transaction);
        }

        private static T? ConsultarCaja<T>(
            SqlConnection connection,
            SqlTransaction transaction,
            string query,
            string codCaja)
        {
            return connection.QueryFirstOrDefault<T>(
                query,
                CrearParametrosCaja(codCaja),
                transaction: transaction);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Asignadas_Obtener(int CodEmpresa, string Usuario)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                @"select rtrim(C.cod_caja) as item,rtrim(C.Descripcion) as descripcion 
            from cajas_definicion C inner join cajas_usuarios U on C.cod_caja = U.cod_caja and U.usuario = @Usuario
            where C.Activa = 1 order by C.cod_caja",
                new { Usuario });
        }

        public ErrorDto<List<CajasDivisaDto>> Cajas_Apertura_Divisas_Obtener(int CodEmpresa, int CodConta)
        {
            return DbHelper.ExecuteListQuery<CajasDivisaDto>(
                _portalDb,
                CodEmpresa,
                @"
            SELECT 
                cod_divisa,
                0 AS Efectivo,
                0 AS Documentos
            FROM CNTX_DIVISAS
            WHERE COD_CONTABILIDAD = @CodConta",
                new { CodConta });
        }

        public ErrorDto<CajaAperturaDetalleDto?> Cajas_Apertura_Detalle_Obtener(int CodEmpresa, string CodCaja)
        {
            return DbHelper.ExecuteSingleQuery<CajaAperturaDetalleDto?>(
                _portalDb,
                CodEmpresa,
                @"SELECT TOP 1 *,  CASE WHEN Estado = 'A' THEN 'Abierta' ELSE 'Cerrada' END AS Estado
            FROM Cajas_Aperturas_Main WHERE cod_Caja = @CodCaja ORDER BY Cod_Apertura DESC;",
                default,
                new { CodCaja });
        }

        /// <summary>
        /// Obtener aprovisionamientos TEConsulta para la apertura de caja
        /// </summary>
        public ErrorDto<List<CajasAperturaTeConsultaData>> Cajas_Apertura_TEConsulta_Obtener(int CodEmpresa, string CodCaja)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);
            var procedureName = "spCajas_TE_Consulta";
            var parameters = new
            {
                Caja = CodCaja,
                OrigenDestino = "D",
                Movimiento = "",
                Estado = "P",
                fInicio = (DateTime?)null,
                fCorte = (DateTime?)null
            };

            return DbHelper.ExecuteStoredProcedureList<CajasAperturaTeConsultaData>(
                connectionString,
                procedureName,
                parameters);
        }

        /// <summary>
        /// Validar si un usuario es autorizado para la apertura de caja
        /// </summary>
        public ErrorDto Cajas_Apertura_UsuarioAutorizado_Validar(int CodEmpresa, string Usuario, string Clave, string CodCaja)
        {
            var claveCifrada = FxStringCifrado(Clave);

            var query = @"SELECT COUNT(*) FROM cajas_usuarios
            WHERE usuario = @Usuario AND contrasena = @ClaveCifrada AND cod_caja = @CodCaja;";

            var aceptado = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                CodEmpresa,
                query,
                0,
                new
                {
                    Usuario,
                    ClaveCifrada = claveCifrada,
                    CodCaja
                });

            if (aceptado.Code != 0)
            {
                return DbHelper.ErrorResponse(aceptado.Description ?? "Error desconocido", aceptado.Code ?? -1);
            }

            if (aceptado.Result > 0)
            {
                return DbHelper.CreateOkResponse();
            }

            return DbHelper.ErrorResponse("No se encuentra autorizado para utilizar esta caja...", -2);
        }

        /// <summary>
        /// Aplicar la apertura de caja
        /// </summary>
        public ErrorDto<CajaAperturaResponseDto> Cajas_Apertura_Aplicar(int CodEmpresa, CajaAperturaRequestDto req)
        {
            if (req is null)
            {
                return CrearRespuestaValidacion(
                    "La solicitud de apertura es requerida.",
                    new CajaAperturaResponseDto
                    {
                        codCaja = string.Empty,
                        codCuentaConta = string.Empty
                    });
            }

            var response = CrearRespuestaOk(new CajaAperturaResponseDto
            {
                codCaja = req.codCaja ?? string.Empty,
                codCuentaConta = string.Empty
            });

            SqlConnection? connection = null;
            SqlTransaction? transaction = null;

            try
            {
                var validaClave = Cajas_Apertura_UsuarioAutorizado_Validar(CodEmpresa, req.usuario, req.clave, req.codCaja ?? string.Empty);
                if (validaClave.Code == -2)
                {
                    AsignarResultadoError(response, validaClave);
                    return response;
                }

                connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                connection.Open();
                transaction = connection.BeginTransaction();

                var validacion = ValidarConfiguracionCaja(connection, transaction, req.codCaja ?? string.Empty);
                if (validacion.Code != 0)
                {
                    AsignarResultadoError(response, validacion);
                    transaction.Rollback();
                    return response;
                }

                var definicion = ObtenerDefinicionCaja(connection, transaction, req.codCaja ?? string.Empty);
                var diasVence = CalcularDiasVence(definicion.CierrePeriocidad);
                var nuevaApertura = ObtenerNuevaApertura(connection, transaction, req.codCaja ?? string.Empty);

                InsertarAperturaMain(connection, transaction, req, nuevaApertura, definicion.AperturaCompartida, diasVence);
                InsertarSaldosIniciales(connection, transaction, req, nuevaApertura);
                ProcesarAprovisionamientos(connection, transaction, req, nuevaApertura);

                var cuentaDev = ObtenerCuentaDevolucion(connection, transaction, req.codCaja ?? string.Empty);

                transaction.Commit();
                response.Result = new CajaAperturaResponseDto
                {
                    codApertura = nuevaApertura,
                    codCaja = req.codCaja ?? string.Empty,
                    codCuentaConta = cuentaDev
                };

                response.Description = $"Apertura # {nuevaApertura} registrada satisfactoriamente...";
            }
            catch (SqlException ex)
            {
                IntentarRollback(connection, transaction);
                AsignarError(response, ex);
            }
            catch (InvalidOperationException ex)
            {
                IntentarRollback(connection, transaction);
                AsignarError(response, ex);
            }
            finally
            {
                transaction?.Dispose();
                connection?.Dispose();
            }

            return response;
        }

        private static ErrorDto ValidarConfiguracionCaja(SqlConnection connection, SqlTransaction transaction, string codCaja)
        {
            var validaciones = new (string Query, string Mensaje)[]
            {
                ("SELECT COUNT(*) FROM CAJAS_FORMAS_PAGO WHERE cod_caja = @CodCaja;", "Aun no se definen formas de pago para esta caja..."),
                ("SELECT COUNT(*) FROM CAJAS_DOCUMENTOS WHERE cod_caja = @CodCaja;", "Aun no se definen documentos para esta caja..."),
                ("SELECT COUNT(*) FROM cajas_servicios_asignados WHERE cod_caja = @CodCaja;", "Aun no se definen servicios para esta caja...")
            };

            foreach (var item in validaciones)
            {
                var validacion = ValidarConteoConfiguracionCaja(connection, transaction, codCaja, item.Query, item.Mensaje);
                if (validacion.Code != 0)
                {
                    return validacion;
                }
            }

            if (ConsultarEnteroCaja(
                connection,
                transaction,
                @"SELECT COUNT(*) FROM cajas_aperturas_main
                  WHERE cod_caja = @CodCaja AND estado = 'A';",
                codCaja) > 0)
            {
                return DbHelper.ErrorResponse("La caja se encuentra abierta", CodigoErrorValidacion);
            }

            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto ValidarConteoConfiguracionCaja(
            SqlConnection connection,
            SqlTransaction transaction,
            string codCaja,
            string query,
            string mensajeError)
        {
            return ConsultarEnteroCaja(connection, transaction, query, codCaja) == 0
                ? DbHelper.ErrorResponse(mensajeError, CodigoErrorValidacion)
                : DbHelper.CreateOkResponse();
        }

        private static void EjecutarInsertDetalleApertura(
            SqlConnection connection,
            SqlTransaction transaction,
            string? codCaja,
            int nuevaApertura,
            CajasDivisaDto row)
        {
            const string insertDetalle = @"
                    INSERT INTO cajas_aperturas_cierres
                        (cod_apertura, cod_caja, si_efectivo, si_documentos, cod_divisa)
                    VALUES
                        (@CodApertura, @CodCaja, @Efectivo, @Documentos, @CodDivisa);";

            connection.Execute(insertDetalle, new
            {
                CodApertura = nuevaApertura,
                CodCaja = codCaja,
                Efectivo = row.efectivo,
                Documentos = row.documentos,
                CodDivisa = row.cod_divisa.Trim()
            }, transaction: transaction);
        }

        private static void EjecutarResolucionAprovisionamiento(
            SqlConnection connection,
            SqlTransaction transaction,
            string? codCaja,
            string? usuario,
            int nuevaApertura,
            CajasAperturaTeConsultaData traslado)
        {
            const string sp = @"
                    EXEC spCajas_TE_Resolucion
                        @TrasladoId,
                        @Accion,
                        @CodCaja,
                        @UsuarioCaja,
                        @CodApertura,
                        @UsuarioLogin,
                        @Flag;";

            connection.Execute(sp, new
            {
                TrasladoId = traslado.traslado_id,
                Accion = "A",
                CodCaja = codCaja,
                UsuarioCaja = usuario,
                CodApertura = nuevaApertura,
                UsuarioLogin = usuario,
                Flag = 1
            }, transaction: transaction);
        }


        private static (int AperturaCompartida, string CierrePeriocidad) ObtenerDefinicionCaja(SqlConnection connection, SqlTransaction transaction, string codCaja)
        {
            var definicion = ConsultarCaja<dynamic>(
                connection,
                transaction,
                @"SELECT Apertura_Compartida, Cierre_Periocidad
                  FROM cajas_definicion
                  WHERE cod_caja = @CodCaja AND activa = 1;",
                codCaja);

            return (
                definicion is null ? 0 : (int)definicion.Apertura_Compartida,
                (definicion?.Cierre_Periocidad ?? string.Empty).ToString().Trim());
        }

        private static int CalcularDiasVence(string cierrePeriocidad)
        {
            return cierrePeriocidad switch
            {
                "A" => 0,
                "D" => 1,
                "S" => 7,
                "Q" => 15,
                "M" => 30,
                _ => 0
            };
        }

        private static int ObtenerNuevaApertura(SqlConnection connection, SqlTransaction transaction, string codCaja)
        {
            return ConsultarEnteroCaja(
                connection,
                transaction,
                @"SELECT ISNULL(MAX(cod_apertura), 0)
                  FROM cajas_aperturas_main
                  WHERE cod_caja = @CodCaja;",
                codCaja) + 1;
        }

        private static void InsertarAperturaMain(
            SqlConnection connection,
            SqlTransaction transaction,
            CajaAperturaRequestDto req,
            int nuevaApertura,
            int aperturaCompartida,
            int diasVence)
        {
            const string insertMain = @"
                INSERT INTO cajas_aperturas_main
                    (cod_apertura, cod_caja, apertura_usuario, apertura_fecha, apertura_compartida, apertura_vence, estado)
                VALUES
                    (@CodApertura, @CodCaja, @UsuarioLogin, dbo.MyGetdate(), @AperturaCompartida,
                     CASE WHEN @DiasVence = 0 THEN NULL ELSE DATEADD(day, @DiasVence, dbo.MyGetdate()) END,
                     'A');";

            connection.Execute(insertMain, new
            {
                CodApertura = nuevaApertura,
                CodCaja = req.codCaja,
                UsuarioLogin = req.usuario,
                AperturaCompartida = aperturaCompartida,
                DiasVence = diasVence
            }, transaction: transaction);
        }

        private static void InsertarSaldosIniciales(
            SqlConnection connection,
            SqlTransaction transaction,
            CajaAperturaRequestDto req,
            int nuevaApertura)
        {
            if (req.saldosIniciales == null)
            {
                return;
            }

            foreach (var row in req.saldosIniciales)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.cod_divisa))
                {
                    continue;
                }

                EjecutarInsertDetalleApertura(connection, transaction, req.codCaja, nuevaApertura, row);
            }
        }

        private static void ProcesarAprovisionamientos(
            SqlConnection connection,
            SqlTransaction transaction,
            CajaAperturaRequestDto req,
            int nuevaApertura)
        {
            if (req.trasladosAprovisionamientos == null || req.trasladosAprovisionamientos.Count == 0)
            {
                return;
            }

            foreach (var trasladoId in req.trasladosAprovisionamientos)
            {
                EjecutarResolucionAprovisionamiento(connection, transaction, req.codCaja, req.usuario, nuevaApertura, trasladoId);
            }
        }

        private static string ObtenerCuentaDevolucion(SqlConnection connection, SqlTransaction transaction, string codCaja)
        {
            return ConsultarCaja<string>(
                connection,
                transaction,
                @"SELECT cod_cuenta_dev FROM cajas_definicion WHERE cod_caja = @CodCaja;",
                codCaja) ?? string.Empty;
        }

        private static int TransformarBloqueCifrado(int block, int indiceSecuencia)
        {
            int[] ajustes = { 1, -5, 7, -13, -2, 3 };
            return block + ajustes[indiceSecuencia % ajustes.Length];
        }

        /// <summary>
        /// Cifrar cadena
        /// </summary>
        public static string FxStringCifrado(string input)
        {
            var asciiInvertido = new StringBuilder();
            var cifrado = new StringBuilder();

            foreach (char caracter in input)
            {
                asciiInvertido.Insert(0, ((int)caracter).ToString());
            }

            string textoBase = asciiInvertido.ToString();

            for (int i = 0, secuencia = 0; i < textoBase.Length; i += 3, secuencia++)
            {
                int longitud = Math.Min(3, textoBase.Length - i);
                int bloque = int.Parse(textoBase.Substring(i, longitud));
                cifrado.Append(TransformarBloqueCifrado(bloque, secuencia));
            }

            return FxDepuraCadena(cifrado.ToString());
        }

        private static string FxDepuraCadena(string cadena)
        {
            var resultado = new StringBuilder();

            for (int i = 0; i < cadena.Length - 1; i++)
            {
                if (!int.TryParse(cadena.Substring(i, 2), out int numero))
                {
                    continue;
                }

                if (numero > 31 && numero != 39 && numero != 34)
                {
                    resultado.Insert(0, (char)numero);
                }
            }

            return resultado.ToString();
        }
    }
}