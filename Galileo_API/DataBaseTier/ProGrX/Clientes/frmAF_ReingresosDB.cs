using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier
{
    public class FrmAFReingresosDB
    {
        private readonly IConfiguration _config;

        private const string SqlPromotoresActivos = @"
                    SELECT id_promotor AS item,
                           nombre AS descripcion
                    FROM dbo.PROMOTORES
                    WHERE estado = 1;";

        private const string SqlActivarSocio = @"
                    UPDATE dbo.socios
                    SET estadoactual = 'S',
                        FechaIngreso = dbo.MyGetdate(),
                        priDeduc = @PriDeduc,
                        reg_user = @Usuario,
                        reg_fecha = dbo.MyGetdate(),
                        Fecha_Comision = NULL,
                        id_promotor = @IdPromotor,
                        cod_oficina = @CodOficina
                    WHERE cedula = @Cedula;";

        private const string SqlPromotorActivo = @"
                    SELECT ISNULL(MAX(estado), 0)
                    FROM dbo.promotores
                    WHERE id_promotor = @IdPromotor;";

        private const string SqlInsertarIngreso = @"
                    INSERT INTO dbo.afi_ingresos
                    (
                        Cedula,
                        fecha_ingreso,
                        id_promotor,
                        Boleta,
                        Usuario,
                        Fecha,
                        cod_oficina
                    )
                    VALUES
                    (
                        @Cedula,
                        dbo.MyGetdate(),
                        @IdPromotor,
                        @Boleta,
                        @Usuario,
                        dbo.MyGetdate(),
                        @CodOficina
                    );";

        private const string SpVinculaPatrimonio = "spAFI_PERSONA_PATRIMONIO_Vincula";

        public FrmAFReingresosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los promotores activos disponibles para reingreso.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de promotores activos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PromotoresReingreso_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotoresActivos);
        }


        /// <summary>
        /// Activa un socio, registra ingreso y vincula patrimonio en una sola transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos serializados de activación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_Persona_ActivarYVincular(int CodEmpresa, string request)
        {
            var req = DbHelper.DeserializeOrNew<AfPersonaActivacionDto>(request);
            var validacion = ValidarActivacion(req);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            var promotorActivo = ValidarPromotorActivo(CodEmpresa, req.id_promotor);
            if (promotorActivo.Code != 0)
            {
                return promotorActivo;
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    ActivarSocio(connection, transaction, req);
                    RegistrarIngreso(connection, transaction, req);
                    VincularPatrimonio(connection, transaction, req);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al activar y vincular persona.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Valida los datos requeridos para activar una persona.
        /// </summary>
        private static ErrorDto ValidarActivacion(AfPersonaActivacionDto req)
        {
            if (string.IsNullOrWhiteSpace(req.cedula))
            {
                return DbHelper.ErrorResponse("La cédula es requerida.", -2);
            }

            if (string.IsNullOrWhiteSpace(req.usuario))
            {
                return DbHelper.ErrorResponse("El usuario es requerido.", -2);
            }

            if (req.id_promotor <= 0)
            {
                return DbHelper.ErrorResponse("El promotor es requerido.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Actualiza la información del socio para activar su estado.
        /// </summary>
        /// <param name="connection">Conexión SQL abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="req">Datos de activación.</param>
        private static void ActivarSocio(SqlConnection connection, SqlTransaction transaction, AfPersonaActivacionDto req)
        {
            connection.Execute(
                SqlActivarSocio,
                CrearParametrosActivacion(req),
                transaction);
        }

        /// <summary>
        /// Registra el ingreso de la persona.
        /// </summary>
        /// <param name="connection">Conexión SQL abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="req">Datos de ingreso.</param>
        private static void RegistrarIngreso(SqlConnection connection, SqlTransaction transaction, AfPersonaActivacionDto req)
        {
            connection.Execute(
                SqlInsertarIngreso,
                CrearParametrosIngreso(req),
                transaction);
        }

        /// <summary>
        /// Vincula el patrimonio de la persona.
        /// </summary>
        /// <param name="connection">Conexión SQL abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="req">Datos de la persona.</param>
        private static void VincularPatrimonio(SqlConnection connection, SqlTransaction transaction, AfPersonaActivacionDto req)
        {
            connection.Execute(
                SpVinculaPatrimonio,
                new { Cedula = NormalizarTexto(req.cedula) },
                transaction,
                commandType: System.Data.CommandType.StoredProcedure);
        }

        /// <summary>
        /// Crea parámetros seguros para activar un socio.
        /// </summary>
        /// <param name="req">Datos de activación.</param>
        /// <returns>Parámetros para actualizar el socio.</returns>
        private static object CrearParametrosActivacion(AfPersonaActivacionDto req)
        {
            return new
            {
                Cedula = NormalizarTexto(req.cedula),
                PriDeduc = req.pri_deduc,
                Usuario = NormalizarTexto(req.usuario),
                IdPromotor = req.id_promotor,
                CodOficina = req.cod_oficina
            };
        }

        /// <summary>
        /// Crea parámetros seguros para registrar el ingreso.
        /// </summary>
        /// <param name="req">Datos de ingreso.</param>
        /// <returns>Parámetros para insertar el histórico de ingreso.</returns>
        private static object CrearParametrosIngreso(AfPersonaActivacionDto req)
        {
            return new
            {
                Cedula = NormalizarTexto(req.cedula),
                IdPromotor = req.id_promotor,
                Boleta = req.boleta,
                Usuario = NormalizarTexto(req.usuario),
                CodOficina = req.cod_oficina
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Verifica si el promotor recibido existe y está activo.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="idPromotor">Código del promotor.</param>
        /// <returns>Resultado de la validación.</returns>
        private ErrorDto ValidarPromotorActivo(int CodEmpresa, int idPromotor)
        {
            var estado = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotorActivo,
                0,
                new { IdPromotor = idPromotor });

            if (estado.Code != 0)
            {
                return DbHelper.ErrorResponse(estado.Description ?? "Error al validar el promotor.", estado.Code.GetValueOrDefault(-1));
            }

            return estado.Result == 1
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse("El promotor indicado se encuentra inactivo o no existe.", -2);
        }

        /// <summary>
        /// Normaliza valores de texto recibidos desde formularios.
        /// </summary>
        /// <param name="valor">Valor a normalizar.</param>
        /// <returns>Valor sin espacios al inicio o al final.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
