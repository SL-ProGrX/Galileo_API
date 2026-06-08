using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndNotificacionesDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string SpNotificaLoad = "spFnd_Notifica_Load";
        private const string SpNotificaList = "spFnd_Notifica_List";
        private const string SpNotificaAdd = "spFnd_Notifica_Add";

        private const string SqlOperadoras = @"
                    SELECT
                        descripcion,
                        cod_operadora AS item
                    FROM dbo.FND_Operadoras
                    ORDER BY descripcion;";

        private const string SqlTiposMovimiento = @"
                    SELECT
                        COD_TIPO_MOVIMENTO AS item,
                        DESCRIPCION
                    FROM dbo.FND_SEG_TIPOSMOVIMIENTOS
                    ORDER BY DESCRIPCION;";

        private const string SqlPlanes = @"
                    SELECT
                        cod_plan AS item,
                        descripcion
                    FROM dbo.fnd_planes
                    WHERE Cod_operadora = @operadora
                    ORDER BY descripcion;";

        private const string SqlPlanDescripcion = @"
                    SELECT Descripcion
                    FROM dbo.Fnd_Planes
                    WHERE Cod_Operadora = @operadora
                      AND Cod_Plan = @plan;";

        private const string SqlPlanSiguiente = @"
                    SELECT TOP 1
                        cod_plan,
                        descripcion
                    FROM dbo.fnd_planes
                    WHERE cod_operadora = @CodOperadora
                      AND cod_plan > @CodPlanActual
                    ORDER BY cod_plan ASC;";

        private const string SqlPlanAnterior = @"
                    SELECT TOP 1
                        cod_plan,
                        descripcion
                    FROM dbo.fnd_planes
                    WHERE cod_operadora = @CodOperadora
                      AND cod_plan < @CodPlanActual
                    ORDER BY cod_plan DESC;";

        public FrmFndNotificacionesDB(IConfiguration config)
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
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_Operadora_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlOperadoras);
        }


        /// <summary>
        /// Método para obtener Tipos de Movimientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_TipoMov_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlTiposMovimiento);
        }

        /// <summary>
        /// Método para obtener los planes de una operadora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_Planes_Obtener(int CodEmpresa, string operadora)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanes,
                new { operadora = NormalizarTexto(operadora) });
        }

        /// <summary>
        /// Método para obtener la descripción de un plan
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<string> Fnd_Notificaciones_Plan_Obtener(int CodEmpresa, string operadora, string plan)
        {
            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanDescripcion,
                string.Empty,
                new
                {
                    operadora = NormalizarTexto(operadora),
                    plan = NormalizarTexto(plan)
                });

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
        public ErrorDto<List<FndNotificacionData>> Fnd_Notificaciones_Scroll_Obtener(int codEmpresa, int codOperadora, string codPlanActual, bool siguiente)
        {
            var planResult = DbHelper.ExecuteSingleQuery<PlanScrollResult>(
                new PortalDB(_config),
                codEmpresa,
                siguiente ? SqlPlanSiguiente : SqlPlanAnterior,
                null,
                new
                {
                    CodOperadora = codOperadora,
                    CodPlanActual = NormalizarTexto(codPlanActual)
                });

            if (planResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    planResult.Description ?? "Error al consultar el plan.",
                    planResult.Code.GetValueOrDefault(-1),
                    new List<FndNotificacionData>());
            }

            if (planResult.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró un plan siguiente o anterior.",
                    -2,
                    new List<FndNotificacionData>());
            }

            var lista = Fnd_Notifica_List(codEmpresa, codOperadora, planResult.Result.cod_plan);
            if (lista.Code != 0)
            {
                return lista;
            }

            lista.Result ??= new List<FndNotificacionData>();
            AplicarDescripcionPlan(lista.Result, planResult.Result);
            return lista;
        }

        /// <summary>
        /// Método para ejecutar el SP spFnd_Notifica_Load
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="pNotifica"></param>
        /// <returns></returns>
        public ErrorDto<FndNotificacionData> Fnd_Notificaciones_Obtener(int codEmpresa, string pNotifica)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
                connection.QueryFirstOrDefault<FndNotificacionData>(
                    SpNotificaLoad,
                    new { NotificaId = NormalizarTexto(pNotifica) },
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<FndNotificacionData>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndNotificacionData { rango = default, activo = default }
            };
        }

        /// <summary>
        /// Método para ejecutar el SP spFnd_Notifica_List
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<FndNotificacionData>> Fnd_Notifica_List(int codEmpresa, int codOperadora, string codigo)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
                connection.Query<FndNotificacionData>(
                    SpNotificaList,
                    new
                    {
                        Operadora = codOperadora,
                        Plan = NormalizarTexto(codigo)
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<FndNotificacionData>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<FndNotificacionData>()
            };
        }


        /// <summary>
        /// Método para ejecutar el SP spFnd_Notifica_Add
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto<int> Fnd_Notificaciones_Guardar(int CodEmpresa, FndNotificacionData data)
        {
            if (data is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de la notificación son requeridos.", -2, 0);
            }

            var validacion = ValidarNotificacion(data);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(validacion.Description ?? "Notificación inválida.", validacion.Code.GetValueOrDefault(-1), 0);
            }

            PrepararUsuarioRegistro(data);

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(
                    SpNotificaAdd,
                    CrearParametrosGuardar(data),
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al guardar la notificación.", result.Code.GetValueOrDefault(-1), 0);
            }

            RegistrarBitacora(CodEmpresa, data, result.Result);
            return DbHelper.CreateOkResponse(result.Result);
        }

        private static ErrorDto ValidarNotificacion(FndNotificacionData data)
        {
            if (string.IsNullOrWhiteSpace(data.descripcion))
            {
                return DbHelper.ErrorResponse("Indique una descripción para esta notificación.", -1);
            }

            return data.rango < 0
                ? DbHelper.ErrorResponse("El monto no es válido.", -1)
                : DbHelper.OkResponse("Ok");
        }

        private static void PrepararUsuarioRegistro(FndNotificacionData data)
        {
            if (data.cod_notificacion > 0)
            {
                data.registro_usuario = NormalizarTexto(data.registro_usuario);
                return;
            }

            data.cod_notificacion = 0;
            data.registro_usuario = NormalizarTexto(data.modifica_usuario);
        }

        private static object CrearParametrosGuardar(FndNotificacionData data)
        {
            return new
            {
                Id = data.cod_notificacion,
                Operadora = data.cod_operadora,
                Plan = NormalizarTexto(data.cod_plan),
                Descripcion = NormalizarTexto(data.descripcion),
                Rango = data.rango,
                Activa = data.activo ? 1 : 0,
                TipoMov = Convert.ToString(data.tipo_mov_codigo)?.Trim() ?? string.Empty,
                Notifica_1 = NormalizarTexto(data.notificacion1),
                Notifica_2 = NormalizarTexto(data.notificacion2),
                Notifica_3 = NormalizarTexto(data.notificacion3),
                Usuario = NormalizarTexto(data.registro_usuario)
            };
        }

        private static void AplicarDescripcionPlan(List<FndNotificacionData> notificaciones, PlanScrollResult plan)
        {
            if (notificaciones.Count == 0)

            {
                notificaciones.Add(new FndNotificacionData
                {
                    cod_plan = plan.cod_plan,
                    plan_descripcion = plan.descripcion,
                    rango = 0, // Valor por defecto, ajustar si es necesario
                    activo = false        // Valor por defecto, ajustar si es necesario
                });
                return;
            }

            notificaciones[0].plan_descripcion = plan.descripcion;
        }

        private void RegistrarBitacora(int codEmpresa, FndNotificacionData data, int idNotificacion)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(data.registro_usuario),
                DetalleMovimiento = $"Notificación Guardada: Id {idNotificacion} - Plan {NormalizarTexto(data.cod_plan)} - Operadora {data.cod_operadora}",
                Movimiento = "Registra - WEB",
                Modulo = vModulo
            });
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private sealed record PlanScrollResult(string cod_plan, string descripcion);
    }
}