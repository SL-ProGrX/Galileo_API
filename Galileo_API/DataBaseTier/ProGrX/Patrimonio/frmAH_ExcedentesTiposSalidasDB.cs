using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhExcedentesTiposSalidasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 2;

        public FrmAhExcedentesTiposSalidasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista principal de tipos de salidas de excedentes.
        /// Respeta la homologación de columnas del VB6.
        /// </summary>
        public ErrorDto<List<FrmAhExcedentesTiposSalidasDto>> Patrimonio_frmAH_ExcedentesTiposSalidas_Lista(int codEmpresa)
        {
            const string sql = @"
select
    rtrim(isnull(cod_salida, '')) as cod_salida,
    rtrim(isnull(descripcion, '')) as descripcion,
    cast(isnull(activa, 0) as bit) as activa,
    cast(isnull(opcion_sistema, 0) as bit) as opcion_sistema,
    isnull(destino_operadora, 0) as destino_operadora,
    rtrim(isnull(destino_plan, '')) as destino_plan,
    isnull(destino_banco, 0) as destino_banco,
    rtrim(isnull(tipo_aplicacion, '')) as tipo_aplicacion,
    cast(isnull(permite_reclasificar, 0) as bit) as permite_reclasificar,
    cast(isnull(requiere_porcentaje, 0) as bit) as requiere_porcentaje,
    rtrim(isnull(tipo_aplicacion_desc, '')) as tipo_aplicacion_desc,
    rtrim(isnull(plan_desc, '')) as plan_desc,
    rtrim(isnull(banco_desc, '')) as banco_desc
from vExc_Salidas_Tipos
order by activa desc, cod_salida;";

            return DbHelper.ExecuteListQuery<FrmAhExcedentesTiposSalidasDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene la lista de planes para el lookup equivalente al F4 del VB6.
        /// </summary>
        public ErrorDto<List<FrmAhExcedentesTiposSalidasPlanDto>> Patrimonio_frmAH_ExcedentesTiposSalidas_Planes_Lista(int codEmpresa)
        {
            const string sql = @"
select
    rtrim(isnull(cod_plan, '')) as cod_plan,
    rtrim(isnull(descripcion, '')) as descripcion,
    isnull(cod_operadora, 0) as cod_operadora
from fnd_Planes
where Estado = 'A'
  and TIPO_CDP = 0
  and PATRIMONIO_ENLACE = 0
  and TIPO_DEDUC = 'M'
order by descripcion;";

            return DbHelper.ExecuteListQuery<FrmAhExcedentesTiposSalidasPlanDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene la lista de bancos para el lookup equivalente al F4 del VB6.
        /// </summary>
        public ErrorDto<List<FrmAhExcedentesTiposSalidasBancoDto>> Patrimonio_frmAH_ExcedentesTiposSalidas_Bancos_Lista(int codEmpresa)
        {
            const string sql = @"
select
    isnull(id_banco, 0) as id_banco,
    rtrim(isnull(descripcion, '')) as descripcion,
    rtrim(isnull(cta, '')) as cta
from Tes_Bancos
where Estado = 'A'
order by descripcion;";

            return DbHelper.ExecuteListQuery<FrmAhExcedentesTiposSalidasBancoDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Inserta un tipo de salida nuevo.
        /// </summary>
        public ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse> Patrimonio_frmAH_ExcedentesTiposSalidas_Insertar(
            int codEmpresa,
            FrmAhExcedentesTiposSalidasGuardarRequest request)
        {
            return Patrimonio_frmAH_ExcedentesTiposSalidas_Guardar(codEmpresa, request, true);
        }

        /// <summary>
        /// Actualiza un tipo de salida existente.
        /// </summary>
        public ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse> Patrimonio_frmAH_ExcedentesTiposSalidas_Actualizar(
            int codEmpresa,
            FrmAhExcedentesTiposSalidasGuardarRequest request)
        {
            return Patrimonio_frmAH_ExcedentesTiposSalidas_Guardar(codEmpresa, request, false);
        }

        /// <summary>
        /// Elimina un tipo de salida por código.
        /// </summary>
        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesTiposSalidas_Eliminar(
            int codEmpresa,
            string codSalida,
            string usuario)
        {
            var codSalidaNormalizado = Patrimonio_frmAH_ExcedentesTiposSalidas_NormalizarCodigo(codSalida);
            var usuarioNormalizado = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codSalidaNormalizado))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el código de salida.", -2, false);
            }

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, false);
            }

            const string sqlDelete = @"
delete from EXC_TIPOS_SALIDAS
where cod_salida = @cod_salida;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                if (!Patrimonio_frmAH_ExcedentesTiposSalidas_Existe(conn, codSalidaNormalizado))
                {
                    return DbHelper.CreateErrorResponse("El tipo de salida indicado no existe.", -2, false);
                }

                conn.Execute(sqlDelete, new { cod_salida = codSalidaNormalizado });

                Patrimonio_frmAH_ExcedentesTiposSalidas_RegistrarBitacora(
                    codEmpresa,
                    usuarioNormalizado,
                    "Elimina - WEB",
                    $"Excedentes: Tipo de Salida: {codSalidaNormalizado}");

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        private ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse> Patrimonio_frmAH_ExcedentesTiposSalidas_Guardar(
            int codEmpresa,
            FrmAhExcedentesTiposSalidasGuardarRequest? request,
            bool esNuevo)
        {
            var response = new FrmAhExcedentesTiposSalidasGuardarResponse();
            var validacion = Patrimonio_frmAH_ExcedentesTiposSalidas_ValidarGuardarRequest(request, response);

            if (validacion != null)
            {
                return validacion;
            }

            var codSalidaNormalizado = Patrimonio_frmAH_ExcedentesTiposSalidas_NormalizarCodigo(request!.cod_salida);
            var descripcionNormalizada = (request.descripcion ?? string.Empty).Trim();
            var tipoAplicacionNormalizado = Patrimonio_frmAH_ExcedentesTiposSalidas_NormalizarTipoAplicacion(request.tipo_aplicacion);
            var destinoPlanNormalizado = (request.destino_plan ?? string.Empty).Trim();
            var usuarioNormalizado = (request.usuario ?? string.Empty).Trim();

            const string sqlInsert = @"
insert into EXC_TIPOS_SALIDAS
(
    cod_salida,
    descripcion,
    activa,
    opcion_sistema,
    TIPO_APLICACION,
    destino_operadora,
    destino_plan,
    destino_banco,
    REQUIERE_PORCENTAJE,
    PERMITE_RECLASIFICAR,
    registro_fecha,
    registro_usuario
)
values
(
    @cod_salida,
    @descripcion,
    @activa,
    @opcion_sistema,
    @tipo_aplicacion,
    @destino_operadora,
    @destino_plan,
    @destino_banco,
    @requiere_porcentaje,
    @permite_reclasificar,
    dbo.MyGetdate(),
    @usuario
);";

            const string sqlUpdate = @"
update EXC_TIPOS_SALIDAS
set
    descripcion = @descripcion,
    activa = @activa,
    opcion_sistema = @opcion_sistema,
    tipo_aplicacion = @tipo_aplicacion,
    destino_operadora = @destino_operadora,
    destino_plan = @destino_plan,
    destino_banco = @destino_banco,
    requiere_porcentaje = @requiere_porcentaje,
    permite_reclasificar = @permite_reclasificar,
    modifica_fecha = dbo.MyGetdate(),
    modifica_usuario = @usuario
where cod_salida = @cod_salida;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var existe = Patrimonio_frmAH_ExcedentesTiposSalidas_Existe(conn, codSalidaNormalizado);

                if (esNuevo && existe)
                {
                    return DbHelper.CreateErrorResponse(
                        "El tipo de salida indicado ya existe.",
                        -2,
                        response);
                }

                if (!esNuevo && !existe)
                {
                    return DbHelper.CreateErrorResponse(
                        "El tipo de salida indicado no existe.",
                        -2,
                        response);
                }

                conn.Execute(
                    esNuevo ? sqlInsert : sqlUpdate,
                    new
                    {
                        cod_salida = codSalidaNormalizado,
                        descripcion = descripcionNormalizada,
                        activa = request.activa,
                        opcion_sistema = request.opcion_sistema,
                        tipo_aplicacion = tipoAplicacionNormalizado,
                        destino_operadora = request.destino_operadora,
                        destino_plan = destinoPlanNormalizado,
                        destino_banco = request.destino_banco,
                        requiere_porcentaje = request.requiere_porcentaje,
                        permite_reclasificar = request.permite_reclasificar,
                        usuario = usuarioNormalizado
                    });

                var accion = esNuevo ? "Registra" : "Modifica";
                var movimiento = esNuevo ? "Registra - WEB" : "Modifica - WEB";

                Patrimonio_frmAH_ExcedentesTiposSalidas_RegistrarBitacora(
                    codEmpresa,
                    usuarioNormalizado,
                    movimiento,
                    $"Excedentes: Tipo de Salida: {codSalidaNormalizado}");

                response.cod_salida = codSalidaNormalizado;
                response.accion = accion;
                response.mensaje = "Informacion guardada satisfactoriamente...";

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        private static ErrorDto<FrmAhExcedentesTiposSalidasGuardarResponse>? Patrimonio_frmAH_ExcedentesTiposSalidas_ValidarGuardarRequest(
            FrmAhExcedentesTiposSalidasGuardarRequest? request,
            FrmAhExcedentesTiposSalidasGuardarResponse response)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, response);
            }

            if (string.IsNullOrWhiteSpace(request.cod_salida))
            {
                return DbHelper.CreateErrorResponse("No se especificó el código de salida.", -2, response);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, response);
            }

            var tipoAplicacion = Patrimonio_frmAH_ExcedentesTiposSalidas_NormalizarTipoAplicacion(request.tipo_aplicacion);
            if (string.IsNullOrWhiteSpace(tipoAplicacion))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el tipo de aplicación.", -2, response);
            }

            if (request.destino_operadora < 0)
            {
                return DbHelper.CreateErrorResponse("La operadora indicada no es válida.", -2, response);
            }

            if (request.destino_banco < 0)
            {
                return DbHelper.CreateErrorResponse("El banco indicado no es válido.", -2, response);
            }

            return null;
        }

        private static bool Patrimonio_frmAH_ExcedentesTiposSalidas_Existe(SqlConnection conn, string codSalida)
        {
            const string sql = @"
select cast(count(1) as int)
from EXC_TIPOS_SALIDAS
where cod_salida = @cod_salida;";

            return conn.QueryFirstOrDefault<int>(sql, new { cod_salida = codSalida }) > 0;
        }

        private static string Patrimonio_frmAH_ExcedentesTiposSalidas_NormalizarCodigo(string? codSalida)
        {
            return (codSalida ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string Patrimonio_frmAH_ExcedentesTiposSalidas_NormalizarTipoAplicacion(string? tipoAplicacion)
        {
            var valor = (tipoAplicacion ?? string.Empty).Trim().ToUpperInvariant();
            return string.IsNullOrWhiteSpace(valor)
                ? string.Empty
                : valor[..1];
        }

        private void Patrimonio_frmAH_ExcedentesTiposSalidas_RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
