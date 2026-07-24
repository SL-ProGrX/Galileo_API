using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhAutorizacionesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 2;
        private const string MensajeUsuarioNoAutorizador =
            "El usuario actual no es un Autorizador de Gestiones de Patrimonio!";

        public FrmAhAutorizacionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de gestiones de patrimonio según los filtros indicados.
        /// </summary>
        public ErrorDto<List<PatGestionesPatrimonio>> Ah_Autorizaciones_Obtener(
            int codEmpresa,
            FiltrosAutorizacionesPatrimonioDto filtros)
        {
            if (filtros == null)
            {
                return DbHelper.CreateErrorResponse<List<PatGestionesPatrimonio>>(
                    "Los filtros de consulta son requeridos.");
            }

            var filtrosNormalizados = Ah_Autorizaciones_NormalizarFiltros(filtros);

            const string sql = @"
select
    rtrim(isnull(Id_Autorizacion, '')) as id_autorizacion,
    rtrim(isnull(Cedula, '')) as cedula,
    rtrim(isnull(Tipo, '')) as tipo,
    convert(varchar(50), isnull(Monto_Calculado, 0)) as monto_calculado,
    convert(varchar(50), isnull(Monto_Solicitado, 0)) as monto_solicitado,
    rtrim(isnull(Estado, '')) as estado,
    isnull(Resuelve_Fecha, '19000101') as resuelve_fecha,
    rtrim(isnull(Resuelve_Usuario, '')) as resuelve_usuario,
    isnull(Registro_Fecha, '19000101') as registro_fecha,
    rtrim(isnull(Registro_Usuario, '')) as registro_usuario,
    isnull(Aplica_Fecha, '19000101') as aplica_fecha,
    rtrim(isnull(Aplica_Usuario, '')) as aplica_usuario,
    rtrim(isnull(Tcon, '')) as tcon,
    rtrim(isnull(Ncon, '')) as ncon,
    rtrim(isnull(Nombre, '')) as nombre,
    rtrim(isnull(Estado_Desc, '')) as estado_desc,
    rtrim(isnull(Tipo_Desc, '')) as tipo_desc,
    convert(varchar(50), isnull(Monto_Dif, 0)) as monto_dif
from vPAT_Gestiones_List
where Estado = @estado
  and Registro_Fecha between @fecha_inicio and @fecha_corte
  and (@usuario = '' or Registro_Usuario like @usuario_like)
  and (@cedula = '' or Cedula like @cedula_like)
  and (@nombre = '' or Nombre like @nombre_like)
order by Registro_Fecha;";

            return DbHelper.ExecuteListQuery<PatGestionesPatrimonio>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    estado = filtrosNormalizados.estado,
                    fecha_inicio = filtrosNormalizados.fecha_inicio,
                    fecha_corte = filtrosNormalizados.fecha_corte,
                    usuario = filtrosNormalizados.usuario,
                    usuario_like = $"%{filtrosNormalizados.usuario}%",
                    cedula = filtrosNormalizados.cedula,
                    cedula_like = $"%{filtrosNormalizados.cedula}%",
                    nombre = filtrosNormalizados.nombre,
                    nombre_like = $"%{filtrosNormalizados.nombre}%"
                });
        }

        /// <summary>
        /// Autoriza o deniega en lote las solicitudes seleccionadas.
        /// </summary>
        public ErrorDto<FrmAhAutorizacionesProcesarResponse> Ah_Autorizaciones_Procesar(
            int codEmpresa,
            FrmAhAutorizacionesProcesarRequest request)
        {
            var response = new FrmAhAutorizacionesProcesarResponse();

            var validacion = Ah_Autorizaciones_ValidarRequestProcesar(request, response);
            if (validacion != null)
            {
                return validacion;
            }

            var usuario = request!.usuario.Trim();
            var accion = request.accion.Trim().ToUpperInvariant();
            var idsAutorizacion = request.ids_autorizacion
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                if (!Ah_Autorizaciones_Autorizador_Valida(conn, tx, usuario))
                {
                    return DbHelper.CreateErrorResponse(
                        MensajeUsuarioNoAutorizador,
                        -2,
                        response);
                }

                var gestionesBitacora = Ah_Autorizaciones_GestionesBitacora_Obtener(
                    conn,
                    tx,
                    idsAutorizacion);

                foreach (var idAutorizacion in idsAutorizacion)
                {
                    conn.Execute(
                        @"exec spPAT_Autorizaciones_Registro
                            @id_autorizacion,
                            @accion,
                            @usuario;",
                        new
                        {
                            id_autorizacion = idAutorizacion,
                            accion,
                            usuario
                        },
                        tx);
                }

                tx.Commit();

                Ah_Autorizaciones_RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    accion,
                    idsAutorizacion,
                    gestionesBitacora);

                response.accion = accion;
                response.accion_desc = accion == "A" ? "Autorización" : "Denegación";
                response.solicitudes_procesadas = idsAutorizacion.Count;
                response.mensaje = $"{response.accion_desc} realizada satisfactoriamente.!";

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                try
                {
                    tx.Rollback();
                }
                catch (Exception)
                {
                    return DbHelper.CreateErrorResponse(
                        $"Error al procesar autorizaciones: {ex.Message}. Además, no se pudo revertir la transacción.",
                        -1,
                        response);
                }

                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        private static FiltrosAutorizacionesPatrimonioDto Ah_Autorizaciones_NormalizarFiltros(
            FiltrosAutorizacionesPatrimonioDto filtros)
        {
            var fechaInicio = filtros.fecha_inicio == DateTime.MinValue
                ? DateTime.Today
                : filtros.fecha_inicio;

            var fechaCorteBase = filtros.fecha_corte == DateTime.MinValue
                ? fechaInicio
                : filtros.fecha_corte;

            if (fechaCorteBase < fechaInicio)
            {
                fechaCorteBase = fechaInicio;
            }

            return new FiltrosAutorizacionesPatrimonioDto
            {
                cedula = (filtros.cedula ?? string.Empty).Trim(),
                usuario = (filtros.usuario ?? string.Empty).Trim(),
                nombre = (filtros.nombre ?? string.Empty).Trim(),
                estado = Ah_Autorizaciones_NormalizarEstado(filtros.estado),
                fecha_inicio = fechaInicio,
                fecha_corte = fechaCorteBase.HasValue ? fechaCorteBase.Value.AddDays(1).AddTicks(-1) : (DateTime?)null
            };
        }

        private static string Ah_Autorizaciones_NormalizarEstado(string? estado)
        {
            var estadoNormalizado = (estado ?? string.Empty).Trim().ToUpperInvariant();
            return estadoNormalizado switch
            {
                "A" => "A",
                "D" => "D",
                "V" => "V",
                _ => "P"
            };
        }

        private static ErrorDto<FrmAhAutorizacionesProcesarResponse>? Ah_Autorizaciones_ValidarRequestProcesar(
            FrmAhAutorizacionesProcesarRequest? request,
            FrmAhAutorizacionesProcesarResponse response)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud es requerida.",
                    -2,
                    response);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    response);
            }

            var accion = (request.accion ?? string.Empty).Trim().ToUpperInvariant();
            if (accion != "A" && accion != "D")
            {
                return DbHelper.CreateErrorResponse(
                    "La acción indicada no es válida.",
                    -2,
                    response);
            }

            if (request.ids_autorizacion == null || request.ids_autorizacion.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe seleccionar al menos una solicitud para procesar.",
                    -2,
                    response);
            }

            return null;
        }

        private static bool Ah_Autorizaciones_Autorizador_Valida(
            SqlConnection conn,
            SqlTransaction tx,
            string usuario)
        {
            const string sql = @"
select cast(isnull(dbo.fxPAT_Autorizado_Valida(@usuario), 0) as int);";

            var estado = conn.QueryFirstOrDefault<int>(
                sql,
                new { usuario },
                tx);

            return estado == 1;
        }

        private static List<PatGestionesBitacoraItem> Ah_Autorizaciones_GestionesBitacora_Obtener(
            SqlConnection conn,
            SqlTransaction tx,
            IReadOnlyCollection<string> idsAutorizacion)
        {
            if (idsAutorizacion.Count == 0)
            {
                return new List<PatGestionesBitacoraItem>();
            }

            const string sql = @"
select
    rtrim(isnull(Id_Autorizacion, '')) as id_autorizacion,
    rtrim(isnull(Cedula, '')) as cedula,
    rtrim(isnull(Nombre, '')) as nombre
from vPAT_Gestiones_List
where Id_Autorizacion in @ids;";

            return conn.Query<PatGestionesBitacoraItem>(
                sql,
                new { ids = idsAutorizacion },
                tx).ToList();
        }

        private void Ah_Autorizaciones_RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string accion,
            IReadOnlyCollection<string> idsAutorizacion,
            IReadOnlyCollection<PatGestionesBitacoraItem> gestiones)
        {
            var movimiento = accion == "A" ? "Autoriza-WEB" : "Deniega-WEB";
            var detalleAccion = accion == "A" ? "Autoriza" : "Deniega";

            var gestionesMap = gestiones.ToDictionary(
                item => item.id_autorizacion,
                item => item,
                StringComparer.OrdinalIgnoreCase);

            var detalles = idsAutorizacion.Select(idAutorizacion =>
                gestionesMap.TryGetValue(idAutorizacion, out var gestion)
                    ? $"Gestion Id:{idAutorizacion}..Id: {gestion.cedula}..Nombre: {gestion.nombre}"
                    : $"Gestion Id:{idAutorizacion}");

            foreach (var detalle in detalles)
            {
                _dbBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Movimiento = movimiento,
                    Modulo = vModulo,
                    DetalleMovimiento = $"{detalleAccion} de {detalle}"
                });
            }
        }

       
    }
}
