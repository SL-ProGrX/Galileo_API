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

        /// <summary>
        /// Obtener las cajas asignadas a un usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Asignadas_Obtener(int CodEmpresa, string Usuario)
        {
            var query = @"select rtrim(C.cod_caja) as item,rtrim(C.Descripcion) as descripcion 
            from cajas_definicion C inner join cajas_usuarios U on C.cod_caja = U.cod_caja and U.usuario = @Usuario
            where C.Activa = 1 order by C.cod_caja";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                query,
                new { Usuario });
        }

        /// <summary>
        /// Obtener los saldos iniciales por divisa para la apertura de caja
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodConta"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasDivisaDto>> Cajas_Apertura_Divisas_Obtener(int CodEmpresa, int CodConta)
        {
            var query = @"
            SELECT 
                cod_divisa,
                0 AS Efectivo,
                0 AS Documentos
            FROM CNTX_DIVISAS
            WHERE COD_CONTABILIDAD = @CodConta";

            return DbHelper.ExecuteListQuery<CajasDivisaDto>(
                _portalDb,
                CodEmpresa,
                query,
                new { CodConta });
        }

        /// <summary>
        /// Obtener el detalle de la apertura de caja
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodCaja"></param>
        /// <returns></returns>
        public ErrorDto<CajaAperturaDetalleDto?> Cajas_Apertura_Detalle_Obtener(int CodEmpresa, string CodCaja)
        {
            var query = @"SELECT TOP 1 *,  CASE WHEN Estado = 'A' THEN 'Abierta' ELSE 'Cerrada' END AS Estado
            FROM Cajas_Aperturas_Main WHERE cod_Caja = @CodCaja ORDER BY Cod_Apertura DESC;";

            return DbHelper.ExecuteSingleQuery<CajaAperturaDetalleDto>(
                _portalDb,
                CodEmpresa,
                query,
                default,
                new { CodCaja });
        }

        /// <summary>
        /// Obtener aprovisionamientos TEConsulta para la apertura de caja
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasAperturaTeConsultaData>> Cajas_Apertura_TEConsulta_Obtener(int CodEmpresa, string CodCaja)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);
            var procedureName = "spCajas_TE_Consulta";
            var parameters = new
            {
                CodCaja,
                Accion = "D",
                Valor = "",
                Estado = "P",
                FechaInicio = (DateTime?)null,
                FechaFin = (DateTime?)null
            };

            return DbHelper.ExecuteStoredProcedureList<CajasAperturaTeConsultaData>(
                connectionString,
                procedureName,
                parameters);
        }

        /// <summary>
        /// Validar si un usuario es autorizado para la apertura de caja
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Clave"></param>
        /// <param name="CodCaja"></param>
        /// <returns></returns>
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
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CajaAperturaResponseDto> Cajas_Apertura_Aplicar(int CodEmpresa, CajaAperturaRequestDto req)
        {
            var response = DbHelper.CreateOkResponse<CajaAperturaResponseDto>(new CajaAperturaResponseDto
            {
                codCaja = req?.codCaja ?? string.Empty,
                codCuentaConta = string.Empty
            });
            req ??= new CajaAperturaRequestDto
            {
                codCaja = string.Empty,
                usuario = string.Empty,
                clave = string.Empty
            };

            SqlConnection? connection = null;
            SqlTransaction? transaction = null;

            try
            {
                var validaClave = Cajas_Apertura_UsuarioAutorizado_Validar(CodEmpresa, req.usuario, req.clave, req.codCaja);
                if (validaClave.Code == -2)
                {
                    response.Code = -2;
                    response.Description = validaClave.Description;
                    return response;
                }

                connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                connection.Open();
                transaction = connection.BeginTransaction();

                var validacion = ValidarConfiguracionCaja(connection, transaction, req.codCaja);
                if (validacion.Code != 0)
                {
                    response.Code = validacion.Code;
                    response.Description = validacion.Description;
                    transaction.Rollback();
                    return response;
                }

                var definicion = ObtenerDefinicionCaja(connection, transaction, req.codCaja);
                var diasVence = CalcularDiasVence(definicion.CierrePeriocidad);
                var nuevaApertura = ObtenerNuevaApertura(connection, transaction, req.codCaja);

                InsertarAperturaMain(connection, transaction, req, nuevaApertura, definicion.AperturaCompartida, diasVence);
                InsertarSaldosIniciales(connection, transaction, req, nuevaApertura);
                ProcesarAprovisionamientos(connection, transaction, req, nuevaApertura);

                var cuentaDev = ObtenerCuentaDevolucion(connection, transaction, req.codCaja);

                transaction.Commit();
                response.Result = new CajaAperturaResponseDto
                {
                    codApertura = nuevaApertura,
                    codCaja = req.codCaja,
                    codCuentaConta = cuentaDev
                };

                response.Description = $"Apertura # {nuevaApertura} registrada satisfactoriamente...";
            }
            catch (Exception ex)
            {
                try
                {
                    if (connection?.State == System.Data.ConnectionState.Open)
                    {
                        transaction?.Rollback();
                    }
                }
                catch
                {
                    // Se conserva el error original; un fallo en rollback no debe ocultarlo.
                }

                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
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
            var existeFormasPago = connection.QuerySingle<int>(
                @"SELECT COUNT(*) FROM CAJAS_FORMAS_PAGO WHERE cod_caja = @CodCaja;",
                new { CodCaja = codCaja }, transaction: transaction);

            if (existeFormasPago == 0)
            {
                return DbHelper.ErrorResponse("Aun no se definen formas de pago para esta caja...", -2);
            }

            var existeDocumentos = connection.QuerySingle<int>(
                @"SELECT COUNT(*) FROM CAJAS_DOCUMENTOS WHERE cod_caja = @CodCaja;",
                new { CodCaja = codCaja }, transaction: transaction);

            if (existeDocumentos == 0)
            {
                return DbHelper.ErrorResponse("Aun no se definen documentos para esta caja...", -2);
            }

            var existeServicios = connection.QuerySingle<int>(
                @"SELECT COUNT(*) FROM cajas_servicios_asignados WHERE cod_caja = @CodCaja;",
                new { CodCaja = codCaja }, transaction: transaction);

            if (existeServicios == 0)
            {
                return DbHelper.ErrorResponse("Aun no se definen servicios para esta caja...", -2);
            }

            var abierta = connection.QuerySingle<int>(
                @"SELECT COUNT(*) FROM cajas_aperturas_main
                  WHERE cod_caja = @CodCaja AND estado = 'A';",
                new { CodCaja = codCaja }, transaction: transaction);

            if (abierta > 0)
            {
                return DbHelper.ErrorResponse("La caja se encuentra abierta", -2);
            }

            return DbHelper.CreateOkResponse();
        }
        
        private static (int AperturaCompartida, string CierrePeriocidad) ObtenerDefinicionCaja(SqlConnection connection, SqlTransaction transaction, string codCaja)
        {
            var definicion = connection.QueryFirstOrDefault<dynamic>(
                @"SELECT Apertura_Compartida, Cierre_Periocidad
                  FROM cajas_definicion
                  WHERE cod_caja = @CodCaja AND activa = 1;",
                new { CodCaja = codCaja }, transaction: transaction);

            var aperturaCompartida = definicion?.Apertura_Compartida ?? 0;
            var cierrePeriocidad = (definicion?.Cierre_Periocidad ?? string.Empty).ToString().Trim();

            return (aperturaCompartida, cierrePeriocidad);
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
            var ultimo = connection.QuerySingle<int>(
                @"SELECT ISNULL(MAX(cod_apertura), 0)
                  FROM cajas_aperturas_main
                  WHERE cod_caja = @CodCaja;",
                new { CodCaja = codCaja }, transaction: transaction);

            return ultimo + 1;
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

            const string insertDetalle = @"
                    INSERT INTO cajas_aperturas_cierres
                        (cod_apertura, cod_caja, si_efectivo, si_documentos, cod_divisa)
                    VALUES
                        (@CodApertura, @CodCaja, @Efectivo, @Documentos, @CodDivisa);";

            foreach (var row in req.saldosIniciales)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.cod_divisa))
                {
                    continue;
                }

                connection.Execute(insertDetalle, new
                {
                    CodApertura = nuevaApertura,
                    CodCaja = req.codCaja,
                    Efectivo = row.efectivo,
                    Documentos = row.documentos,
                    CodDivisa = row.cod_divisa.Trim()
                }, transaction: transaction);
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

            const string sp = @"
                    EXEC spCajas_TE_Resolucion
                        @TrasladoId,
                        @Accion,
                        @CodCaja,
                        @UsuarioCaja,
                        @CodApertura,
                        @UsuarioLogin,
                        @Flag;";

            foreach (var trasladoId in req.trasladosAprovisionamientos)
            {
                connection.Execute(sp, new
                {
                    TrasladoId = trasladoId.traslado_id,
                    Accion = "A",
                    CodCaja = req.codCaja,
                    UsuarioCaja = req.usuario,
                    CodApertura = nuevaApertura,
                    UsuarioLogin = req.usuario,
                    Flag = 1
                }, transaction: transaction);
            }
        }

        private static string ObtenerCuentaDevolucion(SqlConnection connection, SqlTransaction transaction, string codCaja)
        {
            return connection.QueryFirstOrDefault<string>(
                @"SELECT cod_cuenta_dev FROM cajas_definicion WHERE cod_caja = @CodCaja;",
                new { CodCaja = codCaja }, transaction: transaction) ?? string.Empty;
        }

        /// <summary>
        /// Cifrar cadena
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FxStringCifrado(string input)
        {
            var vResBuilder = new StringBuilder();
            var vResX = new StringBuilder();
            int vSec = 0;

            foreach (char c in input)
            {
                int ascii = (int)c;
                vResBuilder.Insert(0, ascii.ToString());
            }
            string vRes = vResBuilder.ToString();
            for (int i = 0; i < vRes.Length; i += 3)
            {
                int take = Math.Min(3, vRes.Length - i);
                string slice = vRes.Substring(i, take);
                int block = int.Parse(slice);
                int transformed = block;

                switch (vSec)
                {
                    case 0: transformed = block + 1; break;
                    case 1: transformed = block - 5; break;
                    case 2: transformed = block + 7; break;
                    case 3: transformed = block - 13; break;
                    case 4: transformed = block - 2; break;
                    case 5: transformed = block + 3; break;
                }

                vResX.Append(transformed);
                vSec = (vSec + 1) % 6;
            }

            return FxDepuraCadena(vResX.ToString());
        }

        /// <summary>
        /// Depurar cadena
        /// </summary>
        /// <param name="cadena"></param>
        /// <returns></returns>
        private static string FxDepuraCadena(string cadena)
        {
            var vRes = new StringBuilder();

            for (int i = 0; i < cadena.Length - 1; i++)
            {
                string sub = cadena.Substring(i, 2);

                if (int.TryParse(sub, out int num) && num > 31 && num != 39 && num != 34)
                {
                    vRes.Insert(0, (char)num);
                }
            }

            return vRes.ToString();
        }
    }
}