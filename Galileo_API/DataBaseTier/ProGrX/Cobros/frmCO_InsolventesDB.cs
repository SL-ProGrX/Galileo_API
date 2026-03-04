using Dapper;
using Galileo.DataBaseTier; 
using Galileo.Models.ERROR; 
using Galileo.Models.Security; 
using static Galileo_API.Models.ProGrX.Cobros.FrmCoInsolventesModels; 

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOInsolventesDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;


        public FrmCOInsolventesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Registro de bitacora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="detalle"></param>
        /// <param name="movimiento"></param>
        private void RegistrarBitacora(
           int CodEmpresa,
           string usuario,
           string detalle,
           string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Consulta de casos insolventes según filtros. 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CbrInsolventeGridItem>> CoInsolventes_Buscar(
            int codEmpresa,
            CbrInsolventesBuscarRequest request)
        {
            DateTime inicio;
            DateTime corte;
            if (request.IgnorarFechas)
            {
                inicio = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                corte = new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                inicio = DateTime.SpecifyKind(
                 (request.FechaInicio ?? DateTime.Today).Date,
                 DateTimeKind.Utc);

                corte = DateTime.SpecifyKind(
                    (request.FechaCorte ?? DateTime.Today).Date.AddDays(1).AddTicks(-1),
                    DateTimeKind.Utc);
            }


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
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CbrSpMovimientoResult> CoInsolventes_Registrar(
      int codEmpresa,
      CbrInsolventeRegistrarRequest request,
      string usuario)
        {
            var validation = ValidateRegistrar(request);
            if (!string.IsNullOrWhiteSpace(validation))
            {
                return DbHelper.CreateErrorResponse(
                    validation,
                    1,
                    new CbrSpMovimientoResult
                    {
                        Pass = 0,
                        Movimiento = null,
                        Mensaje = validation
                    });
            }

            // Mantengo tu lógica, aunque ojo: SpecifyKind no convierte zona horaria, solo marca el Kind.
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

            var spRes = DbHelper.ExecuteSingleQuery<CbrSpMovimientoResult>(
                _portalDB,
                codEmpresa,
                sql,
                defaultValue: null,
                parameters: parameters);


            if (spRes.Code != 0 || spRes.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Error al registrar caso insolvente.",
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
                $"{spRes.Result.Mensaje}",
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
        /// Reversa un caso insolvente activo, marcándolo como reversado. Solo se puede revertir casos activos, no reversados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CbrSpMovimientoResult> CoInsolventes_Reversar(
            int codEmpresa,
            CbrInsolventeRegistrarRequest request,
            string usuario)
        {
            var validation = ValidateReversar(request);
            if (!string.IsNullOrWhiteSpace(validation))
            {
                return DbHelper.CreateErrorResponse(
                        validation,
                        1,
                        new CbrSpMovimientoResult
                        {
                            Pass = 0,
                            Movimiento = null,
                            Mensaje = validation
                        });
            }

            var parameters = new
            {
                request.CasoId,
                Notas = request.Notas.Trim(),
                Usuario = usuario.Trim()
            };

            const string sql = @"
                    EXEC spCBR_Insolventes_Del
                        @CasoId,
                        @Notas,
                        @Usuario;";

            var spRes = DbHelper.ExecuteSingleQuery<CbrSpMovimientoResult>(
                             _portalDB,
                             codEmpresa,
                             sql,
                             defaultValue: null,
                             parameters: parameters);


            if (spRes.Code != 0 || spRes.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Error al reversar caso insolvente.",
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
                $"{spRes.Result.Mensaje}",
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
        /// Valida los datos de entrada para registrar un nuevo caso insolvente. Retorna mensaje de error o string vacío si es válido.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
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
        /// Valida los datos de entrada para reversar un caso insolvente. Retorna mensaje de error o string vacío si es válido.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private static string ValidateReversar(CbrInsolventeRegistrarRequest r)
        {
            if (r.CasoId <= 0)
                return "No se ha indicado un caso actual de Insolvencia.";

            if (string.IsNullOrWhiteSpace(r.Notas) || r.Notas.Trim().Length < 30)
                return "Indique una nota válida de al menos 30 caracteres.";

            return string.Empty;
        }

        public ErrorDto<List<CbrInsolventeSocioResult>> CoInsolventes_Socios_Obtener(int codEmpresa)
        {
            var query = @"select Cedula, CedulaR, nombre from socios order by nombre";
            return DbHelper.ExecuteListQuery<CbrInsolventeSocioResult>(_portalDB, codEmpresa, query);
        }



    }

}
