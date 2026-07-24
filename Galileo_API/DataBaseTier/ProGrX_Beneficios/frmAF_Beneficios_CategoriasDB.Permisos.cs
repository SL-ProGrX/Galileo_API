using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosCategoriasDB
    {
        /// <summary>
        /// Registra los permisos de un usuario en una categoría y deja traza en bitácora por cada cambio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Cod_Categoria">Código de la categoría.</param>
        /// <param name="request">Permisos a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto registroPermisosCategoria(int CodCliente, string Cod_Categoria, BeneCategoriaPermisos request)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var anterior = ObtenerPermisosAnteriores(connection, Cod_Categoria, request.nombre);

                EjecutarRegistroPermisos(connection, Cod_Categoria, request);

                RegistrarCambiosPermisos(CodCliente, Cod_Categoria, request, anterior);

                return DbHelper.OkResponse("Registro actualizado satisfactoriamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los permisos actuales del usuario en la categoría (antes de actualizar).
        /// </summary>
        private static PermisosCategoriaRow ObtenerPermisosAnteriores(SqlConnection connection, string codCategoria, string nombre)
        {
            const string sql = @"SELECT I_CAMBIAR_ESTADO, I_MODIFICA_EXPEDIENTE, I_TRASLADO_TESORERIA, I_PAGO_PROGRAMAR,
                                        I_PAGO_APROBAR_M, I_PAGO_REALIZAR, I_INGRESAR_SOLICITUD, I_PERIODO, I_PAGO_CONSULTA,
                                        I_APROBAR, I_RECHAZAR, I_ANULAR, I_DEVOLVER_RESOLUCION
                                 FROM AFI_BENE_GRUPOS_ROLES
                                 WHERE COD_CATEGORIA = @codCategoria AND usuario = @nombre";

            return connection.QueryFirstOrDefault<PermisosCategoriaRow>(sql, new { codCategoria, nombre }) ?? new PermisosCategoriaRow();
        }

        /// <summary>
        /// Ejecuta el SP de registro de permisos de la categoría de forma parametrizada.
        /// </summary>
        private static void EjecutarRegistroPermisos(SqlConnection connection, string codCategoria, BeneCategoriaPermisos request)
        {
            const string sql = @"EXEC spAFI_Bene_CategoriaPermisoRegistro
                                    @codCategoria, @nombre, @cambiarEstado, @modificaExpediente, @trasladoTesoreria,
                                    @pagoProgramar, @pagoAprobarM, @pagoRealizar, @ingresarSolicitud, @periodo,
                                    @pagoConsulta, @aprobar, @rechazar, @anular, @devolverResolucion, @registroUsuario, @codRol";

            connection.Execute(sql, new
            {
                codCategoria,
                request.nombre,
                cambiarEstado = request.i_cambiar_estado ? 1 : 0,
                modificaExpediente = request.i_modifica_expediente ? 1 : 0,
                trasladoTesoreria = request.i_traslado_tesoreria ? 1 : 0,
                pagoProgramar = request.i_pago_programar ? 1 : 0,
                pagoAprobarM = request.i_pago_aprobar_m ? 1 : 0,
                pagoRealizar = request.i_pago_realizar ? 1 : 0,
                ingresarSolicitud = request.i_ingresar_solicitud ? 1 : 0,
                periodo = request.i_periodo ? 1 : 0,
                pagoConsulta = request.i_pago_consulta ? 1 : 0,
                aprobar = request.i_aprobar ? 1 : 0,
                rechazar = request.i_rechazar ? 1 : 0,
                anular = request.i_anular ? 1 : 0,
                devolverResolucion = request.i_devolver_resolucion ? 1 : 0,
                registroUsuario = request.registro_usuario,
                codRol = request.cod_rol
            });
        }

        /// <summary>
        /// Compara permisos anteriores contra los nuevos y registra en bitácora cada cambio detectado.
        /// </summary>
        private void RegistrarCambiosPermisos(int CodCliente, string codCategoria, BeneCategoriaPermisos request, PermisosCategoriaRow anterior)
        {
            var cambios = new (string etiqueta, int anterior, bool nuevo)[]
            {
                ("Cambiar Estado", anterior.I_CAMBIAR_ESTADO, request.i_cambiar_estado),
                ("Modificar Expediente", anterior.I_MODIFICA_EXPEDIENTE, request.i_modifica_expediente),
                ("Traslado Tesoreria", anterior.I_TRASLADO_TESORERIA, request.i_traslado_tesoreria),
                ("Programar Pago", anterior.I_PAGO_PROGRAMAR, request.i_pago_programar),
                ("Aprobar Monto", anterior.I_PAGO_APROBAR_M, request.i_pago_aprobar_m),
                ("Realizar Pago", anterior.I_PAGO_REALIZAR, request.i_pago_realizar),
                ("Ingresar Solicitud", anterior.I_INGRESAR_SOLICITUD, request.i_ingresar_solicitud),
                ("Consultar Pago", anterior.I_PAGO_CONSULTA, request.i_pago_consulta),
                ("Aprobar", anterior.I_APROBAR, request.i_aprobar),
                ("Rechazar", anterior.I_RECHAZAR, request.i_rechazar),
                ("Anular", anterior.I_ANULAR, request.i_anular),
                ("Devolver Resolucion", anterior.I_DEVOLVER_RESOLUCION, request.i_devolver_resolucion)
            };

            foreach (var (etiqueta, ant, nuevo) in cambios)
            {
                if (ant != (nuevo ? 1 : 0))
                {
                    LogCambioPermiso(CodCliente, codCategoria, etiqueta, nuevo, request);
                }
            }
        }

        /// <summary>
        /// Registra en bitácora el cambio de un permiso específico.
        /// </summary>
        private void LogCambioPermiso(int CodCliente, string codCategoria, string etiqueta, bool activo, BeneCategoriaPermisos request)
        {
            var origen = activo ? "Inactivo" : "Activo";
            var destino = activo ? "Activo" : "Inactivo";
            var detalle = $"El usuario [{request.registro_usuario}] actualiza permiso de {etiqueta} de [{origen}] por [{destino}] del usuario [{request.nombre}]";

            RegistrarBitacora(CodCliente, "Actualiza-Web", detalle, codCategoria, request.registro_usuario);
        }

        /// <summary>
        /// Representa la fila de permisos previos leída de AFI_BENE_GRUPOS_ROLES.
        /// </summary>
        private sealed class PermisosCategoriaRow
        {
            public int I_CAMBIAR_ESTADO { get; set; }
            public int I_MODIFICA_EXPEDIENTE { get; set; }
            public int I_TRASLADO_TESORERIA { get; set; }
            public int I_PAGO_PROGRAMAR { get; set; }
            public int I_PAGO_APROBAR_M { get; set; }
            public int I_PAGO_REALIZAR { get; set; }
            public int I_INGRESAR_SOLICITUD { get; set; }
            public int I_PERIODO { get; set; }
            public int I_PAGO_CONSULTA { get; set; }
            public int I_APROBAR { get; set; }
            public int I_RECHAZAR { get; set; }
            public int I_ANULAR { get; set; }
            public int I_DEVOLVER_RESOLUCION { get; set; }
        }
    }
}
