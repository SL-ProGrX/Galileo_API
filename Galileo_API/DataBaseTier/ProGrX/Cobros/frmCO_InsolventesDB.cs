using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoInsolventesModels;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOInsolventesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;

        // Módulo para bitácora
        private const int ModuloBitacora = 36;

        public FrmCOInsolventesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Registro de bitácora.
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloBitacora
            });
        }

        /// <summary>
        /// Helper: construye rango de fechas equivalente a VB6:
        /// - IgnorarFechas: 2000-01-01 a 2100-01-01
        /// - Caso contrario: Inicio 00:00:00 y Corte 23:59:59.999...
        /// (Se especifica DateTimeKind para cumplir Sonar.)
        /// </summary>
        private static (DateTime inicio, DateTime corte) ResolverRangoFechas(CbrInsolventesBuscarRequest request)
        {
            if (request.IgnorarFechas)
            {
                var inicioAll = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var corteAll = new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return (inicioAll, corteAll);
            }

            var inicio = DateTime.SpecifyKind(
                (request.FechaInicio ?? DateTime.Today).Date,
                DateTimeKind.Utc);

            var corte = DateTime.SpecifyKind(
                (request.FechaCorte ?? DateTime.Today).Date.AddDays(1).AddTicks(-1),
                DateTimeKind.Utc);

            return (inicio, corte);
        }

        /// <summary>
        /// Helper: construye respuesta de validación para SP Movimiento.
        /// </summary>
        private static ErrorDto<CbrSpMovimientoResult> ErrorValidacion(string validation)
            => DbHelper.CreateErrorResponse(
                validation,
                1,
                new CbrSpMovimientoResult
                {
                    Pass = 0,
                    Movimiento = null,
                    Mensaje = validation
                });

        /// <summary>
        /// Helper: ejecuta SP que retorna CbrSpMovimientoResult y centraliza:
        /// - manejo de error DbHelper
        /// - bitácora (si Pass=1)
        /// - respuesta estándar ErrorDto
        /// </summary>
        private ErrorDto<CbrSpMovimientoResult> EjecutarMovimiento(
            int codEmpresa,
            string usuario,
            string sql,
            object parameters,
            string msgErrorGenerico)
        {
            var spRes = DbHelper.ExecuteSingleQuery<CbrSpMovimientoResult>(
                _portalDB,
                codEmpresa,
                sql,
                defaultValue: null,
                parameters: parameters);

            if (spRes.Code != 0 || spRes.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    msgErrorGenerico,
                    -1,
                    new CbrSpMovimientoResult
                    {
                        Pass = 0,
                        Movimiento = null,
                        Mensaje = "Error interno."
                    });
            }

            if (spRes.Result.Pass == 1)
            {
                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    spRes.Result.Mensaje ?? string.Empty,
                    $"{spRes.Result.Movimiento}- WEB ");
            }

            return new ErrorDto<CbrSpMovimientoResult>
            {
                Code = 0,
                Description = spRes.Result.Mensaje ?? "OK",
                Result = spRes.Result
            };
        }

        /// <summary>
        /// Consulta de casos insolventes según filtros.
        /// </summary>
        public ErrorDto<List<CbrInsolventeGridItem>> CoInsolventes_Buscar(int codEmpresa, CbrInsolventesBuscarRequest request)
        {
            var (inicio, corte) = ResolverRangoFechas(request);

            var parameters = new
            {
                request.Estado,
                Inicio = inicio,
                Corte = corte,
                Filtro = request.Filtro?.Trim() ?? string.Empty,
                Expediente = request.Expediente?.Trim() ?? string.Empty,
                Usuario = request.Usuario?.Trim() ?? string.Empty
            };

            const string sql = @"
EXEC spCBR_Insolventes_List
    @Estado,
    @Inicio,
    @Corte,
    @Filtro,
    @Expediente,
    @Usuario;";

            return DbHelper.ExecuteListQuery<CbrInsolventeGridItem>(_portalDB, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Registra un nuevo caso insolvente.
        /// </summary>
        public ErrorDto<CbrSpMovimientoResult> CoInsolventes_Registrar(
            int codEmpresa,
            CbrInsolventeRegistrarRequest request,
            string usuario)
        {
            var validation = ValidateRegistrar(request);
            if (!string.IsNullOrWhiteSpace(validation))
            {
                return ErrorValidacion(validation);
            }

            DateTime? fechaSen = request.FechaSentencia.HasValue
                ? DateTime.SpecifyKind(request.FechaSentencia.Value, DateTimeKind.Utc)
                : null;

            var parameters = new
            {
                Cedula = request.Cedula.Trim(),
                Nombre = request.Nombre.Trim(),
                Expediente = request.Expediente.Trim(),
                FechaSentencia = fechaSen,
                Notas = request.Notas.Trim(),
                Usuario = usuario.Trim()
            };

            const string sql = @"
EXEC spCBR_Insolventes_Add
    @Cedula,
    @Nombre,
    @Expediente,
    @FechaSentencia,
    @Notas,
    @Usuario;";

            return EjecutarMovimiento(
                codEmpresa,
                usuario,
                sql,
                parameters,
                "Error al registrar caso insolvente.");
        }

        /// <summary>
        /// Reversa un caso insolvente activo, marcándolo como reversado.
        /// </summary>
        public ErrorDto<CbrSpMovimientoResult> CoInsolventes_Reversar(
            int codEmpresa,
            CbrInsolventeRegistrarRequest request,
            string usuario)
        {
            var validation = ValidateReversar(request);
            if (!string.IsNullOrWhiteSpace(validation))
            {
                return ErrorValidacion(validation);
            }

            var parameters = new
            {
                CasoId = request.CasoId,
                Notas = request.Notas.Trim(),
                Usuario = usuario.Trim()
            };

            const string sql = @"
EXEC spCBR_Insolventes_Del
    @CasoId,
    @Notas,
    @Usuario;";

            return EjecutarMovimiento(
                codEmpresa,
                usuario,
                sql,
                parameters,
                "Error al reversar caso insolvente.");
        }

        /// <summary>
        /// Valida datos para registrar un nuevo caso insolvente.
        /// </summary>
        private static string ValidateRegistrar(CbrInsolventeRegistrarRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Cedula))
                return "No se ha indicado los datos de la persona.";

            if (string.IsNullOrWhiteSpace(r.Expediente))
                return "No se ha indicado el No. Expediente.";

            if (string.IsNullOrWhiteSpace(r.Notas) || r.Notas.Trim().Length < 30)
                return "Indique una nota válida de al menos 30 caracteres.";

            return string.Empty;
        }

        /// <summary>
        /// Valida datos para reversar un caso insolvente.
        /// </summary>
        private static string ValidateReversar(CbrInsolventeRegistrarRequest r)
        {
            if (r.CasoId <= 0)
                return "No se ha indicado un caso actual de Insolvencia.";

            if (string.IsNullOrWhiteSpace(r.Notas) || r.Notas.Trim().Length < 30)
                return "Indique una nota válida de al menos 30 caracteres.";

            return string.Empty;
        }

        /// <summary>
        /// Obtiene socios para búsqueda (F4).
        /// </summary>
        public ErrorDto<List<CbrInsolventeSocioResult>> CoInsolventes_Socios_Obtener(int codEmpresa)
        {
            const string query = @"select Cedula, CedulaR, nombre from socios order by nombre";
            return DbHelper.ExecuteListQuery<CbrInsolventeSocioResult>(_portalDB, codEmpresa, query);
        }
    }
}