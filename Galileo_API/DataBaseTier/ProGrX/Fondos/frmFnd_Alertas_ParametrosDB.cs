using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndAlertasParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 18;
        private const string conErrorDesc = "Error desconocido";
        private const string SqlListaEmail = @"
                    SELECT COUNT(1)
                    FROM dbo.FND_ALERTAS_EMAIL
                    WHERE @hasFilter = 0 OR
                    (
                        email LIKE @filtro OR
                        CONVERT(varchar(30), idRegistro) LIKE @filtro
                    );

                    SELECT
                        idRegistro,
                        email,
                        usuarioinserta,
                        fechainserta
                    FROM dbo.FND_ALERTAS_EMAIL
                    WHERE @hasFilter = 0 OR
                    (
                        email LIKE @filtro OR
                        CONVERT(varchar(30), idRegistro) LIKE @filtro
                    )
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN idRegistro END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN idRegistro END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN email END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN email END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN usuarioinserta END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN usuarioinserta END DESC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 1 THEN fechainserta END ASC,
                        CASE WHEN @sortCode = 4 AND @isAsc = 0 THEN fechainserta END DESC,
                        idRegistro ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private static readonly IReadOnlyDictionary<string, int> AlertasEmailSortMap = new Dictionary<string, int>
        {
            ["idRegistro"] = 1,
            ["email"] = 2,
            ["usuarioinserta"] = 3,
            ["fechainserta"] = 4
        };

        public FrmFndAlertasParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Método para obtener las operadoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_Operadora_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        descripcion,
                        cod_operadora AS item
                    FROM dbo.FND_Operadoras
                    ORDER BY cod_operadora;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Método para obtener los planes de una operadora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_Planes_Obtener(int CodEmpresa, string operadora)
        {
            const string query = @"
                    SELECT
                        cod_plan AS item,
                        descripcion
                    FROM dbo.vFnd_Alerta_Planes_Programados
                    WHERE Cod_operadora = @operadora
                    ORDER BY cod_plan;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { operadora = NormalizarTexto(operadora) });
        }

        /// <summary>
        /// Método para obtener la descripción de un plan
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<string> Fnd_AlertasParametros_Plan_Obtener(int CodEmpresa, string operadora, string plan)
        {
            const string query = @"
                    SELECT TOP 1 Descripcion
                    FROM dbo.Fnd_Planes
                    WHERE Cod_Operadora = @operadora
                      AND Cod_Plan = @plan;";

            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                string.Empty,
                new { operadora = NormalizarTexto(operadora), plan = NormalizarTexto(plan) });

            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? string.Empty
            };
        }

        /// <summary>
        /// Método para obtener el plan siguiente o anterior
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlanActual"></param>
        /// <param name="siguiente"></param>
        /// <returns></returns>
        public ErrorDto<FndalertasData> Fnd_AlertasPlanes_Scroll_Obtener(int codEmpresa, int codOperadora, string codPlanActual, bool siguiente)
        {
            var query = CrearSqlScrollPlanes(siguiente);
            var planResult = DbHelper.ExecuteSingleQuery<FndalertasData>(
                new PortalDB(_config),
                codEmpresa,
                query,
                new FndalertasData(),
                new
                {
                    CodOperadora = codOperadora,
                    CodPlanActual = NormalizarTexto(codPlanActual)
                });

            if (planResult.Code != 0)
            {
                return CrearErrorAlerta(planResult.Description ?? "Error al consultar plan.");
            }

            if (planResult.Result == null || string.IsNullOrWhiteSpace(planResult.Result.cod_plan))
            {
                return DbHelper.CreateErrorResponse<FndalertasData>("No se encontró un plan siguiente o anterior.", -2, null!);
            }

            return Fnd_AlertasParametros_Alerta_Obtener(codEmpresa, codOperadora, planResult.Result.cod_plan);
        }


        #region ALERTAS
        /// <summary>
        /// Método para obtener los parámetros de las alertas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDto<FndalertasData> Fnd_AlertasParametros_Alerta_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            const string query = @"
                    SELECT *,
                           CASE
                               WHEN UnidadTiempo = 'MINUTE' THEN 'Minutos'
                               WHEN UnidadTiempo = 'DAY' THEN 'Días'
                               WHEN UnidadTiempo = 'HOUR' THEN 'Horas'
                               ELSE ''
                           END AS UnidadTiempoEsp,
                           (
                               SELECT TOP 1 P.Descripcion
                               FROM dbo.Fnd_Planes P
                               WHERE P.Cod_Operadora = A.COD_OPERADORA
                                 AND P.Cod_Plan = A.COD_PLAN
                           ) AS descripcion
                    FROM dbo.FND_ALERTAS_PARAMETROS A
                    WHERE COD_OPERADORA = @CodOperadora
                      AND COD_PLAN = @CodPlan;";

            var result = DbHelper.ExecuteSingleQuery<FndalertasData>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new FndalertasData(),
                new { CodOperadora, CodPlan = NormalizarTexto(CodPlan) });

            return new ErrorDto<FndalertasData>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndalertasData()
            };
        }

        /// <summary>
        /// Método para obtener la lista de correos electrónicos registrados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> Fnd_AlertasParametros_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<FndAlertasContactosDto>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, AlertasEmailSortMap, "idRegistro");
                var queryResult = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(SqlListaEmail, spec.Params);
                    return new TablasListaGenericaModel
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<FndAlertasContactosDto>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorListaEmail(queryResult.Description ?? "Error al consultar correos de alerta.");
                }

                result.Result = queryResult.Result ?? new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<FndAlertasContactosDto>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorListaEmail(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Método para obtener los planes disponibles para nuevas alertas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_AlertasParametros_NuevoPlan_Obtener(int CodEmpresa, string operadora)
        {
            const string query = @"
                    SELECT
                        cod_plan AS item,
                        descripcion
                    FROM dbo.vFnd_Alerta_Planes_Pendientes
                    WHERE Cod_operadora = @operadora
                    ORDER BY cod_plan;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { operadora = NormalizarTexto(operadora) });
        }

        /// <summary>
        /// Método para registrar los parámetros de las alertas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="listaAlertas"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_AlertasParametros_Registrar(int codEmpresa, string usuario, FndalertasData alerta)
        {
            if (alerta is null)
            {
                return DbHelper.ErrorResponse("Los datos de la alerta son requeridos.", -2);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
                connection.QueryFirstOrDefault<dynamic>(
                    "spFnd_Alertas_Parametros_Add",
                    CrearParametrosRegistroAlerta(alerta, usuario),
                    commandType: CommandType.StoredProcedure));

            return ResolverResultadoSp(
                result,
                codEmpresa,
                usuario,
                $"Mantenimiento Alertas: Operadora {alerta.cod_operadora}, Plan {alerta.cod_plan}",
                "Registra - WEB",
                "Semaforo Registrado Satisfactoriamente! ",
                "No fue posible registrar el semáforo");
        }

        /// <summary>
        /// Método para guardar los parámetros de las alertas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Fnd_AlertasParametros_Alerta_Guardar(int CodEmpresa, FndalertasData data)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@pData", data);

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    "dbo.SP_Fnd_Alertas_Parametros_Guardar",
                    parametros,
                    commandType: CommandType.StoredProcedure));

            return result.Code == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar parámetros de alerta.", result.Code ?? -1);
        }


        #endregion



        #region EMAIL

        /// <summary>
        /// Método para guardar un correo electrónico
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="contacto"></param>
        /// <returns></returns>
        public ErrorDto Fnd_AlertasEmail_Guardar(int codEmpresa, FndAlertasContactosDto contacto)
        {
            if (contacto is null || !EsEmailValido(contacto.email))
            {
                return DbHelper.ErrorResponse("El formato del correo no es válido.", -1);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
                connection.QueryFirstOrDefault<dynamic>(
                    "spFnd_Alertas_Email_Add",
                    new { Email = NormalizarTexto(contacto.email), Usuario = contacto.usuarioinserta },
                    commandType: CommandType.StoredProcedure));

            return ResolverResultadoSp(
                result,
                codEmpresa,
                contacto.usuarioinserta,
                $"Registro de correo electrónico: {contacto.email}",
                "Registra - WEB",
                "Correo registrado satisfactoriamente.",
                "No fue posible registrar el correo");
        }

        /// <summary>
        /// Método para eliminar correos electrónicos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="listaContactos"></param>
        /// <returns></returns>
        public ErrorDto Fnd_AlertasEmail_Eliminar(int codEmpresa, string usuario, List<FndAlertasContactosDto> listaContactos)
        {
            var contactos = listaContactos?.Where(c => c != null).ToList() ?? new List<FndAlertasContactosDto>();
            if (contactos.Count == 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar al menos un correo para eliminar.", 1);
            }

            foreach (var contacto in contactos)
            {
                var result = EliminarEmailIndividual(codEmpresa, usuario, contacto.idregistro, contacto.email);
                if (result.Code != 0)
                {
                    return result;
                }
            }

            return DbHelper.OkResponse("Correo(s) eliminado(s) satisfactoriamente.");
        }

        /// <summary>
        /// Método para eliminar correos electrónicos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="listaContactos"></param>
        /// <returns></returns>
        public ErrorDto Fnd_AlertasEmailId_Eliminar(int codEmpresa, string usuario, int idregistro)
        {
            var result = EliminarEmailIndividual(codEmpresa, usuario, idregistro, $"ID: {idregistro}");
            return result.Code == 0
                ? DbHelper.OkResponse("Correo(s) eliminado(s) satisfactoriamente.")
                : result;
        }


        /// <summary>
        /// Método para validar el formato de un correo electrónico
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private static bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return string.Equals(addr.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }


        #endregion
        private static string CrearSqlScrollPlanes(bool siguiente)
        {
            return siguiente
                ? @"
                    SELECT TOP 1 cod_plan, descripcion
                    FROM dbo.vFnd_Alerta_Planes_Programados
                    WHERE cod_operadora = @CodOperadora
                      AND cod_plan > @CodPlanActual
                    ORDER BY cod_plan ASC;"
                : @"
                    SELECT TOP 1 cod_plan, descripcion
                    FROM dbo.vFnd_Alerta_Planes_Programados
                    WHERE cod_operadora = @CodOperadora
                      AND cod_plan < @CodPlanActual
                    ORDER BY cod_plan DESC;";
        }



        private static ErrorDto<TablasListaGenericaModel> CrearErrorListaEmail(string mensaje) =>
            DbHelper.CreateErrorResponse(mensaje, -1, new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<FndAlertasContactosDto>()
            });

        private static object CrearParametrosRegistroAlerta(FndalertasData alerta, string usuario)
        {
            return new
            {
                Operadora = alerta.cod_operadora,
                Plan = alerta.cod_plan,
                UnidadTiempo = NormalizarUnidadTiempo(alerta.unidadtiempo),
                AlertaRoja = alerta.alertaroja,
                AlertaAmarilla = alerta.alertaamarilla,
                Usuario = usuario,
                ContactoOficina = alerta.contacto_oficina,
                ContactoTelefono = alerta.contacto_telefono,
                ContactoEmail = alerta.contacto_email
            };
        }

        private static string NormalizarUnidadTiempo(string? unidadTiempo)
        {
            return (unidadTiempo ?? string.Empty).Trim().ToUpper() switch
            {
                "D" or "DAY" => "DAY",
                "M" or "MINUTE" => "MINUTE",
                "H" or "HOUR" => "HOUR",
                _ => "DAY"
            };
        }

        private ErrorDto ResolverResultadoSp(
            ErrorDto<dynamic?> result,
            int codEmpresa,
            string usuario,
            string detalleMovimiento,
            string movimiento,
            string mensajeOk,
            string mensajeError)
        {
            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? mensajeError);
            }

            if ((result.Result?.Pass ?? 0) != 1)
            {
                return DbHelper.ErrorResponse($"{mensajeError}: {result.Result?.Mensaje ?? conErrorDesc}", 1);
            }

            RegistrarBitacora(codEmpresa, usuario, detalleMovimiento, movimiento);
            return DbHelper.OkResponse(mensajeOk);
        }

        private ErrorDto EliminarEmailIndividual(int codEmpresa, string usuario, int idregistro, string detalleCorreo)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
                connection.QueryFirstOrDefault<dynamic>(
                    "spFnd_Alertas_Email_Delete",
                    new { RegistroId = idregistro, Usuario = usuario },
                    commandType: CommandType.StoredProcedure));

            return ResolverResultadoSp(
                result,
                codEmpresa,
                usuario,
                $"Eliminación de correo electrónico: {detalleCorreo}",
                "Elimina - WEB",
                "Correo eliminado satisfactoriamente.",
                $"No fue posible eliminar el correo {detalleCorreo}");
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private static ErrorDto<FndalertasData> CrearErrorAlerta(string mensaje) =>
            DbHelper.CreateErrorResponse<FndalertasData>(mensaje, -1, null!);

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            var bitacora = new BitacoraInsertarDto { EmpresaId = codEmpresa, Usuario = usuario, DetalleMovimiento = detalleMovimiento, Movimiento = movimiento, Modulo = vModulo };
            _Security_MainDB.Bitacora(bitacora);
        }

    }
}
