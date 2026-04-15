using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOGestionesMasivasDB
    {
        private const int ModuloCobros = 4;
        private const string TipoCarga = "C";
        private const string ProcesoId = "CBR-GST";
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MProGrxMain _proGrxMain;
        public FrmCOGestionesMasivasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _proGrxMain = new MProGrxMain(config);
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
        /// Obtiene la lista de ejecutivos de cobro activos para el selector principal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(usuario) as item,
                        rtrim(nombre) as descripcion
                    from cbr_usuarios
                    where isnull(estado, 0) = 1
                    order by nombre;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }
        /// <summary>
        /// Obtiene la lista de gestiones activas de nivel usuario para el selector principal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Gestiones_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(cod_gestion) as item,
                        rtrim(descripcion) as descripcion
                    from cbr_gestiones
                    where estado = 1
                      and nivel_gestion = 'U'
                    order by cod_gestion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }
        /// <summary>
        /// Obtiene la lista de causas de morosidad activas para el selector principal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Causas_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(cod_causa) as item,
                        rtrim(descripcion) as descripcion
                    from cbr_causas_morosidad
                    where isnull(activa, 0) = 1
                    order by cod_causa;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }
        /// <summary>
        /// Obtiene la lista de tipos de arreglo activos para el selector principal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Arreglos_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(cod_arreglo) as item,
                        rtrim(descripcion) as descripcion
                    from cbr_tipos_arreglos
                    where isnull(activo, 0) = 1
                    order by cod_arreglo;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }
        /// <summary>
        /// Carga las cédulas al staging temporal y devuelve la revisión del lote.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioSesion"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CoGestionesMasivasCargaResultDto> CO_GestionesMasivas_Cargar(int CodEmpresa,string usuarioSesion,CoGestionesMasivasCargaRequest request)
        {
            var response = new CoGestionesMasivasCargaResultDto();

            try
            {
                if (request == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "La solicitud es requerida.",
                        -1,
                        response);
                }

                var usuarioCobro = (request.usuario_cobro ?? string.Empty).Trim().ToUpperInvariant();
                var usuarioBitacora = (usuarioSesion ?? string.Empty).Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(usuarioCobro))
                {
                    return DbHelper.CreateErrorResponse(
                        "El ejecutivo de cobro es requerido.",
                        -1,
                        response);
                }

                if (request.cedulas == null || request.cedulas.Count == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "No existen cédulas para cargar.",
                        -1,
                        response);
                }

                var cedulas = NormalizarCedulas(request.cedulas);
                if (cedulas.Count == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "No existen cédulas válidas para cargar.",
                        -1,
                        response);
                }

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                ValidarUsuarioCobro(conn, usuarioCobro);

                using var tx = conn.BeginTransaction();

                EjecutarCargaMasiva(conn, tx, usuarioCobro, string.Empty, true);

                foreach (var cedula in cedulas)
                {
                    EjecutarCargaMasiva(conn, tx, usuarioCobro, cedula, false);
                }

                var lista = ObtenerRevision(conn, tx, usuarioCobro);

                tx.Commit();

                response.lista = lista;
                response.total = lista.Count;
                response.total_mora_financiera = lista.Sum(x => x.mora_financiera);
                response.total_mora_legal = lista.Sum(x => x.mora_legal);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioBitacora,
                    DetalleMovimiento = $"Gestiones Masivas - Carga lote [{usuarioCobro}] Registros={response.total}",
                    Movimiento = "MODIFICA-WEB",
                    Modulo = ModuloCobros
                });

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }
        /// <summary>
        /// Procesa el lote temporal de gestiones masivas y registra las gestiones finales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioSesion"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CO_GestionesMasivas_Procesar(int CodEmpresa,string usuarioSesion,CoGestionesMasivasProcesarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse("La solicitud es requerida.");
                }

                var usuarioCobro = (request.usuario_cobro ?? string.Empty).Trim().ToUpperInvariant();
                var usuarioBitacora = (usuarioSesion ?? string.Empty).Trim().ToUpperInvariant();
                var codGestion = (request.cod_gestion ?? string.Empty).Trim().ToUpperInvariant();
                var codCausa = (request.cod_causa ?? string.Empty).Trim().ToUpperInvariant();
                var codArreglo = (request.cod_arreglo ?? string.Empty).Trim().ToUpperInvariant();
                var notas = (request.notas ?? string.Empty).Trim();
                var monto = request.monto ?? 0m;
                var usuarioSesionNorm = (usuarioSesion ?? string.Empty).Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(usuarioCobro))
                {
                    return DbHelper.ErrorResponse("El ejecutivo de cobro es requerido.");
                }

                if (string.IsNullOrWhiteSpace(codGestion))
                {
                    return DbHelper.ErrorResponse("La gestión es requerida.");
                }

                if (string.IsNullOrWhiteSpace(codCausa))
                {
                    return DbHelper.ErrorResponse("La causa es requerida.");
                }

                if (string.IsNullOrWhiteSpace(codArreglo))
                {
                    return DbHelper.ErrorResponse("El arreglo es requerido.");
                }

                if (monto < 0)
                {
                    return DbHelper.ErrorResponse("El monto no puede ser negativo.");
                }

                if (string.IsNullOrWhiteSpace(usuarioSesionNorm))
                {
                    return DbHelper.ErrorResponse("El usuario de sesión es requerido.");
                }

                var oficinaResult = ObtenerOficinaTitular(CodEmpresa, usuarioSesionNorm);
                if (oficinaResult.Code == -1 || string.IsNullOrWhiteSpace(oficinaResult.Result))
                {
                    return DbHelper.ErrorResponse(
                        oficinaResult.Description ?? "No fue posible obtener la oficina titular del usuario.");
                }

                var oficina = oficinaResult.Result.Trim().ToUpperInvariant();

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                ValidarUsuarioCobro(conn, usuarioCobro);
                ValidarCatalogosProceso(conn, codGestion, codCausa, codArreglo);
                ValidarLoteTemporal(conn, usuarioCobro);

                const string sp = @"
                    exec spCBR_Gestiones_Masivo_Procesa
                        @Tipo,
                        @ProcesoId,
                        @Usuario,
                        @Gestion,
                        @Causa,
                        @Arreglo,
                        @GestionMonto,
                        @Vence,
                        @Notas,
                        @Oficina;";

                conn.Execute(sp, new
                {
                    Tipo = TipoCarga,
                    ProcesoId,
                    Usuario = usuarioCobro,
                    Gestion = codGestion,
                    Causa = codCausa,
                    Arreglo = codArreglo,
                    GestionMonto = monto,
                    Vence = request.vence,
                    Notas = notas,
                    Oficina = oficina
                });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioBitacora,
                    DetalleMovimiento =
                        $"Gestiones Masivas - Procesa lote [{usuarioCobro}] Gestión={codGestion} Causa={codCausa} Arreglo={codArreglo} Monto={monto}",
                    Movimiento = "MODIFICA-WEB",
                    Modulo = ModuloCobros
                });

                return DbHelper.OkResponse("Gestiones masivas procesadas satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene la oficina titular del usuario de sesión desde los parámetros globales del sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioSesion"></param>
        /// <returns></returns>
        private ErrorDto<string> ObtenerOficinaTitular(int CodEmpresa, string usuarioSesion)
        {
            var usuario = (usuarioSesion ?? string.Empty).Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<string>(
                    "El usuario de sesión es requerido para obtener la oficina titular.",
                    -1);
            }

            var globalesResult = _proGrxMain.sbSifParametrosInicializa(CodEmpresa, usuario);
            if (globalesResult.Code == -1 || globalesResult.Result == null)
            {
                return DbHelper.CreateErrorResponse<string>(
                    globalesResult.Description ?? "No fue posible cargar los parámetros globales del usuario.",
                    -1);
            }

            var oficinaTitular = (globalesResult.Result.GOficinaTitular ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(oficinaTitular))
            {
                return DbHelper.CreateErrorResponse<string>(
                    "El usuario no tiene oficina titular definida.",
                    -1);
            }

            return DbHelper.CreateOkResponse(oficinaTitular);
        }
        /// <summary>
        /// Normaliza y deduplica las cédulas recibidas desde el frontend.
        /// </summary>
        /// <param name="cedulas"></param>
        /// <returns></returns>
        private static List<string> NormalizarCedulas(List<string> cedulas)
        {
            return cedulas
                .Select(x => (x ?? string.Empty).Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        /// <summary>
        /// Ejecuta el SP estándar de carga masiva al staging temporal.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="usuarioCobro"></param>
        /// <param name="cedula"></param>
        /// <param name="clean"></param>
        private static void EjecutarCargaMasiva(SqlConnection conn,SqlTransaction tx,string usuarioCobro,string cedula,bool clean)
        {
            const string sp = @"
                exec spSys_Carga_Masiva
                    @Tipo,
                    @ProcesoId,
                    @Usuario,
                    @Llave01,
                    @Llave02,
                    @Clean;";

            conn.Execute(sp, new
            {
                Tipo = TipoCarga,
                ProcesoId,
                Usuario = usuarioCobro,
                Llave01 = cedula,
                Llave02 = string.Empty,
                Clean = clean ? 1 : 0
            }, tx);
        }
        /// <summary>
        /// Obtiene la revisión del lote cargado en staging.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="usuarioCobro"></param>
        /// <returns></returns>
        private static List<CoGestionesMasivasCargaItemDto> ObtenerRevision( SqlConnection conn,SqlTransaction tx,string usuarioCobro)
        {
            const string sp = @"
                exec spCBR_Gestiones_Masivo_Revisa
                    @Tipo,
                    @ProcesoId,
                    @Usuario;";

            return conn.Query<CoGestionesMasivasCargaItemDto>(sp, new
            {
                Tipo = TipoCarga,
                ProcesoId,
                Usuario = usuarioCobro
            }, tx).ToList();
        }
        /// <summary>
        /// Valida que el usuario de cobro exista y esté activo.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="usuarioCobro"></param>
        private static void ValidarUsuarioCobro(SqlConnection conn, string usuarioCobro)
        {
            const string sql = @"
                select count(1)
                from cbr_usuarios
                where usuario = @usuario
                  and isnull(estado, 0) = 1;";

            var existe = conn.QueryFirstOrDefault<int>(sql, new { usuario = usuarioCobro });
            if (existe <= 0)
            {
                ThrowValidation("El ejecutivo de cobro indicado no existe o está inactivo.");
            }
        }
        /// <summary>
        /// Valida que los catálogos requeridos del proceso existan y estén activos.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="codGestion"></param>
        /// <param name="codCausa"></param>
        /// <param name="codArreglo"></param>
        private static void ValidarCatalogosProceso(SqlConnection conn,string codGestion,string codCausa,string codArreglo)
        {
            const string sqlGestion = @"
                select count(1)
                from cbr_gestiones
                where cod_gestion = @cod_gestion
                  and estado = 1
                  and nivel_gestion = 'U';";

            const string sqlCausa = @"
                select count(1)
                from cbr_causas_morosidad
                where cod_causa = @cod_causa
                  and isnull(activa, 0) = 1;";

            const string sqlArreglo = @"
                select count(1)
                from cbr_tipos_arreglos
                where cod_arreglo = @cod_arreglo
                  and isnull(activo, 0) = 1;";

            if (conn.QueryFirstOrDefault<int>(sqlGestion, new { cod_gestion = codGestion }) <= 0)
            {
                ThrowValidation("La gestión indicada no existe o está inactiva.");
            }

            if (conn.QueryFirstOrDefault<int>(sqlCausa, new { cod_causa = codCausa }) <= 0)
            {
                ThrowValidation("La causa indicada no existe o está inactiva.");
            }

            if (conn.QueryFirstOrDefault<int>(sqlArreglo, new { cod_arreglo = codArreglo }) <= 0)
            {
                ThrowValidation("El arreglo indicado no existe o está inactivo.");
            }
        }
        /// <summary>
        /// Valida que exista al menos un registro en el staging temporal para el usuario indicado.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="usuarioCobro"></param>
        private static void ValidarLoteTemporal(SqlConnection conn, string usuarioCobro)
        {
            const string sql = @"
                select count(1)
                from sys_carga_masiva_load
                where tipo = @tipo
                  and proceso_id = @procesoId
                  and usuario = @usuario;";

            var cantidad = conn.QueryFirstOrDefault<int>(sql, new
            {
                tipo = TipoCarga,
                procesoId = ProcesoId,
                usuario = usuarioCobro
            });

            if (cantidad <= 0)
            {
                ThrowValidation("No existen registros cargados para procesar.");
            }
        }
        /// <summary>
        /// Lanza una excepción controlada para validaciones de negocio.
        /// </summary>
        /// <param name="message"></param>
        private static void ThrowValidation(string message)
        {
            throw new InvalidOperationException(message);
        }
    }
}