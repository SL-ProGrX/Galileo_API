using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier
{
    public class FrmAFTelefonosDB
    {
        private readonly IConfiguration _config;

        private const string SqlTiposTelefonos = @"
                    SELECT idTipoTelefono AS item,
                           nombreTipoTelefono AS descripcion
                    FROM dbo.AFI_TIPOS_TELEFONOS
                    WHERE Activo = 1
                    ORDER BY Prioridad;";

        private const string SqlTelefonosPorCedula = @"
                    SELECT T.Telefono,
                           T.Cedula,
                           T.Contacto,
                           T.Numero,
                           T.Tipo,
                           T.Ext,
                           T.Usuario,
                           T.Fecha
                    FROM dbo.Telefonos T
                    WHERE Cedula = @Cedula;";

        private const string SqlTelefonoInsert = @"
                    INSERT INTO dbo.Telefonos
                    (
                        cedula,
                        tipo,
                        numero,
                        ext,
                        contacto,
                        usuario,
                        fecha
                    )
                    VALUES
                    (
                        @Cedula,
                        @TipoId,
                        @Numero,
                        @Ext,
                        @Contacto,
                        @Usuario,
                        dbo.MyGetDate()
                    );";

        private const string SqlTelefonoUpdate = @"
                    UPDATE dbo.Telefonos
                    SET numero = @Numero,
                        ext = @Ext,
                        contacto = @Contacto,
                        tipo = @TipoId,
                        usuario = @Usuario,
                        fecha = dbo.MyGetDate()
                    WHERE telefono = @TelefonoId;";

        private const string SqlTelefonoDelete = @"
                    DELETE FROM dbo.Telefonos
                    WHERE telefono = @TelefonoId;";

        public FrmAFTelefonosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        /// <summary>
        /// Obtiene los tipos de teléfonos activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de tipos de teléfonos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposTelefonos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlTiposTelefonos);
        }

        /// <summary>
        /// Obtiene los teléfonos asociados a una cédula.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula a consultar.</param>
        /// <returns>Listado de teléfonos asociados.</returns>
        public ErrorDto<List<AfTelefonosDto>> AF_Telefonos_ObtenerPorCedula(int CodEmpresa, string cedula)
        {
            return DbHelper.ExecuteListQuery<AfTelefonosDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlTelefonosPorCedula,
                new { Cedula = NormalizarTexto(cedula) });
        }


        /// <summary>
        /// Inserta un nuevo teléfono.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula asociada.</param>
        /// <param name="tipoId">Tipo de teléfono.</param>
        /// <param name="numero">Número telefónico.</param>
        /// <param name="ext">Extensión.</param>
        /// <param name="contacto">Nombre del contacto.</param>
        /// <param name="usuario">Usuario que registra.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_Telefono_Insertar(int CodEmpresa, string cedula, int tipoId, string numero, string? ext, string? contacto, string usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlTelefonoInsert,
                CrearParametrosTelefono(cedula, tipoId, numero, ext, contacto, usuario));

            return result.Code == 0
                ? DbHelper.OkResponse("Guardado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar teléfono.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza un teléfono existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="telefonoId">Identificador del teléfono.</param>
        /// <param name="tipoId">Tipo de teléfono.</param>
        /// <param name="numero">Número telefónico.</param>
        /// <param name="ext">Extensión.</param>
        /// <param name="contacto">Nombre del contacto.</param>
        /// <param name="usuario">Usuario que actualiza.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_Telefono_Actualizar(int CodEmpresa, int telefonoId, int tipoId, string numero, string? ext, string? contacto, string usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlTelefonoUpdate,
                CrearParametrosTelefonoActualizacion(telefonoId, tipoId, numero, ext, contacto, usuario));

            return result.Code == 0
                ? DbHelper.OkResponse("Actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar teléfono.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un teléfono.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="telefonoId">Identificador del teléfono.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_Telefono_Eliminar(int CodEmpresa, int telefonoId)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlTelefonoDelete,
                new { TelefonoId = telefonoId });

            return result.Code == 0
                ? DbHelper.OkResponse("Eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar teléfono.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea parámetros seguros para insertar teléfonos.
        /// </summary>
        private static object CrearParametrosTelefono(string cedula, int tipoId, string numero, string? ext, string? contacto, string usuario)
        {
            return new
            {
                Cedula = NormalizarTexto(cedula),
                TipoId = tipoId,
                Numero = NormalizarTexto(numero),
                Ext = NormalizarOpcional(ext, "0"),
                Contacto = NormalizarOpcional(contacto, "N/A"),
                Usuario = NormalizarTexto(usuario)
            };
        }

        /// <summary>
        /// Crea parámetros seguros para actualizar teléfonos.
        /// </summary>
        private static object CrearParametrosTelefonoActualizacion(int telefonoId, int tipoId, string numero, string? ext, string? contacto, string usuario)
        {
            return new
            {
                TelefonoId = telefonoId,
                TipoId = tipoId,
                Numero = NormalizarTexto(numero),
                Ext = NormalizarOpcional(ext, "0"),
                Contacto = NormalizarOpcional(contacto, "N/A"),
                Usuario = NormalizarTexto(usuario)
            };
        }

        /// <summary>
        /// Normaliza un valor opcional usando un marcador inválido.
        /// </summary>
        private static string? NormalizarOpcional(string? valor, string marcadorInvalido)
        {
            var texto = NormalizarTexto(valor);
            if (string.Equals(texto, marcadorInvalido, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto;
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
