using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;
using System.Data;

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
                FechaIngreso = GETDATE(),
                priDeduc = @PriDeduc,
                reg_user = @Usuario,
                reg_fecha = GETDATE(),
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
                GETDATE(),
                @IdPromotor,
                @Boleta,
                @Usuario,
                GETDATE(),
                @CodOficina
            );";

        private const string SpVinculaPatrimonio =
            "spAFI_PERSONA_PATRIMONIO_Vincula";

        public FrmAFReingresosDB(IConfiguration config)
        {
            _config = config
                ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los promotores activos disponibles para reingreso.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>>
            AF_PromotoresReingreso_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotoresActivos);
        }

        /// <summary>
        /// Activa el socio, registra su ingreso y vincula su patrimonio
        /// directamente, sin una transacción iniciada por este DB.
        /// </summary>
        public ErrorDto AF_Persona_ActivarYVincular(
            int CodEmpresa,
            string request)
        {
            AfPersonaActivacionDto req;

            try
            {
                req = DbHelper.DeserializeOrNew<AfPersonaActivacionDto>(
                    request);
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse(
                    "Los datos de activación no tienen un formato válido.",
                    -2);
            }

            var validacion = ValidarActivacion(req);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            var promotorActivo = ValidarPromotorActivo(
                CodEmpresa,
                req.id_promotor);

            if (promotorActivo.Code != 0)
            {
                return promotorActivo;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(
                    CreatePortalDb(),
                    CodEmpresa);

                connection.Open();

                int sociosActualizados;
                try
                {
                    sociosActualizados = ActivarSocio(connection, req);
                }
                catch (Exception)
                {
                    return DbHelper.ErrorResponse(
                        "Falló la activación del socio. Verifique su estado antes de reintentar.",
                        -1);
                }

                if (sociosActualizados == 0)
                {
                    return DbHelper.ErrorResponse(
                        "No se encontró el socio para activar. " +
                        "No se registró el ingreso ni se vinculó el patrimonio.",
                        -1);
                }

                try
                {
                    RegistrarIngreso(connection, req);
                }
                catch (Exception)
                {
                    return DbHelper.ErrorResponse(
                        "El socio quedó activado, pero falló el registro del ingreso. " +
                        "Verifique si se insertó algún ingreso antes de reintentar.",
                        -1);
                }

                try
                {
                    VincularPatrimonio(connection, req);
                }
                catch (Exception)
                {
                    return DbHelper.ErrorResponse(
                        "El socio fue activado y se registró su ingreso, pero falló " +
                        "la vinculación del patrimonio. Revise los tres datos antes de reintentar.",
                        -1);
                }

                return DbHelper.OkResponse(
                    "Persona activada, ingreso registrado y patrimonio vinculado.");
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse(
                    "No se pudo abrir la conexión para activar a la persona.",
                    -1);
            }
        }

        /// <summary>
        /// Valida los datos requeridos para activar una persona.
        /// </summary>
        private static ErrorDto ValidarActivacion(
            AfPersonaActivacionDto req)
        {
            if (req is null ||
                string.IsNullOrWhiteSpace(req.cedula))
            {
                return DbHelper.ErrorResponse(
                    "La cédula es requerida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(req.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    -2);
            }

            if (req.id_promotor <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El promotor es requerido.",
                    -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Actualiza el socio y devuelve la cantidad de filas afectadas.
        /// </summary>
        private static int ActivarSocio(
            SqlConnection connection,
            AfPersonaActivacionDto req)
        {
            return connection.Execute(
                SqlActivarSocio,
                CrearParametrosActivacion(req));
        }

        /// <summary>
        /// Registra el ingreso de la persona.
        /// </summary>
        private static void RegistrarIngreso(
            SqlConnection connection,
            AfPersonaActivacionDto req)
        {
            connection.Execute(
                SqlInsertarIngreso,
                CrearParametrosIngreso(req));
        }

        /// <summary>
        /// Ejecuta el procedimiento existente que vincula el patrimonio.
        /// </summary>
        private static void VincularPatrimonio(
            SqlConnection connection,
            AfPersonaActivacionDto req)
        {
            connection.Execute(
                SpVinculaPatrimonio,
                new { Cedula = NormalizarTexto(req.cedula) },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Crea los parámetros para activar el socio.
        /// </summary>
        private static object CrearParametrosActivacion(
            AfPersonaActivacionDto req)
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
        /// Crea los parámetros para registrar el ingreso.
        /// </summary>
        private static object CrearParametrosIngreso(
            AfPersonaActivacionDto req)
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
        /// Crea el acceso al portal con la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Verifica que el promotor exista y esté activo.
        /// </summary>
        private ErrorDto ValidarPromotorActivo(
            int CodEmpresa,
            int idPromotor)
        {
            var estado = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotorActivo,
                0,
                new { IdPromotor = idPromotor });

            if (estado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    estado.Description
                        ?? "Error al validar el promotor.",
                    estado.Code.GetValueOrDefault(-1));
            }

            return estado.Result == 1
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    "El promotor indicado se encuentra inactivo o no existe.",
                    -2);
        }

        /// <summary>
        /// Quita espacios al inicio y al final del texto recibido.
        /// </summary>
        private static string NormalizarTexto(string? valor)
            => (valor ?? string.Empty).Trim();
    }
}