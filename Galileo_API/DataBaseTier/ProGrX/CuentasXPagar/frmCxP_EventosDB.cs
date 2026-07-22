using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCxPEventosDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPEventosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPEventosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la información de un evento de cuentas por pagar.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="cod_evento">Código del evento.</param>
        /// <returns>Información del evento solicitado.</returns>
        public ErrorDto<CxPEventos> Eventos_Obtener(int CodCliente, string cod_evento)
        {
            var result = DbHelper.ExecuteSingleQuery<CxPEventos>(
                CreatePortalDb(),
                CodCliente,
                @"SELECT E.*, 
                         ISNULL(Cta.Descripcion, '') AS cod_comision_cuenta,
                         ISNULL(Cta.Cod_Cuenta_Mask, '') AS comision_cuenta,
                         ISNULL(Crd.Codigo, '') AS cod_linea_crd,
                         ISNULL(Crd.Descripcion, '') AS descripcion_linea_crd
                  FROM cxp_Eventos E
                  LEFT JOIN CntX_Cuentas Cta 
                    ON E.Comision_Cuenta = Cta.cod_Cuenta 
                   AND Cta.cod_contabilidad = 1
                  LEFT JOIN Catalogo Crd 
                    ON E.cod_Linea_Crd = Crd.Codigo
                  WHERE E.cod_Evento = @cod_evento",
                null,
                new { cod_evento });

            if (result.Code != 0)
            {
                return new ErrorDto<CxPEventos>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener el evento.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<CxPEventos>
                {
                    Code = -2,
                    Description = "No se encontró el evento.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene el código del primer evento o del siguiente/anterior según el desplazamiento indicado.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="Scroll">Dirección del desplazamiento.</param>
        /// <param name="cod_evento">Código base para buscar el siguiente o anterior.</param>
        /// <returns>Código del evento encontrado en la descripción del resultado.</returns>
        public ErrorDto top1EventoObtener(int CodCliente, int Scroll, string cod_evento)
        {
            string sql;
            object parametros;

            if (string.IsNullOrWhiteSpace(cod_evento))
            {
                sql = "select Top 1 cod_evento from CXP_EVENTOS order by cod_evento asc";
                parametros = new { };
            }
            else if (Scroll == 1)
            {
                sql = "select Top 1 cod_evento from CXP_EVENTOS where cod_evento > @cod_evento order by cod_evento asc";
                parametros = new { cod_evento };
            }
            else
            {
                sql = "select Top 1 cod_evento from CXP_EVENTOS where cod_evento < @cod_evento order by cod_evento desc";
                parametros = new { cod_evento };
            }

            var result = DbHelper.ExecuteSingleQuery<string>(
                CreatePortalDb(),
                CodCliente,
                sql,
                string.Empty,
                parametros);

            return result.Code == 0
                ? new ErrorDto { Code = 0, Description = result.Result ?? string.Empty }
                : DbHelper.ErrorResponse(result.Description ?? "Error al obtener el evento solicitado.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registra o actualiza un evento de cuentas por pagar.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="evento">Datos del evento a guardar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Evento_Guardar(int CodCliente, CxPEventos evento)
        {
            if (evento is null)
            {
                return DbHelper.ErrorResponse("Los datos del evento son requeridos.", -2);
            }

            var activo = evento.activo == true ? 1 : 2;
            var codigoEvento = string.IsNullOrWhiteSpace(evento.cod_evento) ? "0" : evento.cod_evento.Trim();
            const string sql = @"
                EXEC spCxP_Eventos_Add
                    @Evento,
                    @Descripcion,
                    @Activo,
                    @FechaInicio,
                    @FechaFinaliza,
                    @LugarVenta,
                    @Notas,
                    @ComisionPorc,
                    @ComisionCuenta,
                    @CodLineaCrd,
                    @RegistroUsuario;";
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                sql,
                new
                {
                    Evento = codigoEvento,
                    Descripcion = evento.descripcion,
                    Activo = activo,
                    FechaInicio = evento.fecha_inicio,
                    FechaFinaliza = evento.fecha_finaliza,
                    LugarVenta = evento.lugar_venta,
                    Notas = evento.notas,
                    ComisionPorc = evento.comision_porc,
                    ComisionCuenta = evento.comision_cuenta,
                    CodLineaCrd = evento.cod_linea_crd,
                    RegistroUsuario = evento.registro_usuario
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al guardar el evento.", result.Code.GetValueOrDefault(-1));
            }

            if (codigoEvento != "0")
            {
                return new ErrorDto { Code = 0, Description = codigoEvento };
            }

            var generado = DbHelper.ExecuteSingleQuery<string>(
                CreatePortalDb(),
                CodCliente,
                @"SELECT TOP 1 CONVERT(varchar(20), cod_evento)
                  FROM CXP_EVENTOS
                  WHERE registro_usuario = @RegistroUsuario
                    AND descripcion = @Descripcion
                  ORDER BY registro_fecha DESC, cod_evento DESC;",
                string.Empty,
                new { RegistroUsuario = evento.registro_usuario, Descripcion = evento.descripcion });

            if (generado.Code != 0 || string.IsNullOrWhiteSpace(generado.Result))
            {
                return DbHelper.ErrorResponse(generado.Description ?? "No fue posible obtener el código generado del evento.", generado.Code.GetValueOrDefault(-1));
            }

            return new ErrorDto { Code = 0, Description = generado.Result.Trim() };
        }

        /// <summary>
        /// Elimina un evento de cuentas por pagar.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="cod_evento">Código del evento a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Evento_Eliminar(int CodCliente, string cod_evento)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                "delete CXP_EVENTOS where cod_evento = @cod_evento",
                new { cod_evento });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse("No se puede eliminar el evento, ya que tiene registros asociados", -1);
        }

        /// <summary>
        /// Obtiene los proveedores asociados a un evento.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_evento">Código del evento.</param>
        /// <returns>Listado de proveedores relacionados al evento.</returns>
        public ErrorDto<List<CxPEventosProveedor>> ObtenerProveedoresEvento(int CodEmpresa, string? cod_evento)
        {
            return DbHelper.ExecuteListQuery<CxPEventosProveedor>(
                CreatePortalDb(),
                CodEmpresa,
                "spCxP_Eventos_Proveedores_List",
                new { Evento = cod_evento });
        }

        /// <summary>
        /// Asigna o desasigna un proveedor a un evento.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="proveedor">Identificador del proveedor.</param>
        /// <param name="evento">Código del evento.</param>
        /// <param name="activa">Indicador de activación.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AsignaEventoProveedor(int CodCliente, int proveedor, string evento, int activa, string usuario)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                "spCxP_Proveedores_Eventos_Asigna",
                new
                {
                    Proveedor = proveedor,
                    Evento = evento,
                    Activa = activa,
                    Usuario = usuario
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al asignar el proveedor al evento.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene el listado resumido de eventos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de eventos para búsqueda.</returns>
        public ErrorDto<List<CxPEventosBusqueda>> EventosLista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<CxPEventosBusqueda>(
                CreatePortalDb(),
                CodEmpresa,
                "Select cod_evento, descripcion from CXP_EVENTOS");
        }

        /// <summary>
        /// Obtiene el listado de líneas disponibles para eventos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_evento">Código del evento.</param>
        /// <returns>Listado de líneas asociables al evento.</returns>
        public ErrorDto<List<CxPEventosLineas>> EventosLineas_Obtener(int CodEmpresa, string cod_evento)
        {
            return DbHelper.ExecuteListQuery<CxPEventosLineas>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT CODIGO AS CrdCod, DESCRIPCION AS CrdDesc from Catalogo");
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}
