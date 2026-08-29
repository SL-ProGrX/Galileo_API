using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Cobros;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Cobros
{
    public class FrmCOControlAsgManualDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCOControlAsgManualDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Lista para popup de Expedientes (F4 / doble click).
        /// <param name="CodEmpresa"></param>
        /// <param name="soloSinAsignar"></param>
        /// <param name="soloMorosos"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<CoControlAsgManualExpedienteItem>> Co_ControlAsgManual_Expedientes_Obtener(int CodEmpresa,int soloSinAsignar, int soloMorosos)
        {
            var sql = @"
                select
                    rtrim(Soc.Cedula) as cedula,
                    rtrim(Soc.Nombre) as nombre,
                    cast(Reg.Id_Solicitud as varchar(50)) as operacion,
                    rtrim(Reg.Codigo) as linea,
                    rtrim(Cat.Descripcion) as linea_desc
                from socios Soc
                inner join Reg_Creditos Reg
                    on Soc.Cedula = Reg.Cedula and Reg.Estado = 'A'
                inner join Catalogo Cat
                    on Reg.Codigo = Cat.Codigo and Cat.LINEA_INTERNA = 1
                left join Vista_Morosidad Vm
                    on Reg.Id_Solicitud = Vm.Id_Solicitud
                where 1=1
            ";

            if (soloSinAsignar == 1)
                sql += " and Soc.Cedula not in (select cedula from CBR_ASIGNACION) ";

            if (soloMorosos == 1)
                sql += " and isnull(Vm.Id_Solicitud,0) > 1 ";

            sql += " order by Soc.Cedula ";

            return DbHelper.ExecuteListQuery<CoControlAsgManualExpedienteItem>(new PortalDB(_config), CodEmpresa, sql);
        }

        /// <summary>
        /// Lista de oficiales (cbr_usuarios) para popup "Trasladar a".
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<CoControlAsgManualUsuarioItem>> Co_ControlAsgManual_Usuarios_Obtener(int CodEmpresa)
        {
            var sql = @"
                select rtrim(usuario) as usuario, rtrim(nombre) as nombre
                from cbr_usuarios
                where estado = 1
                order by usuario";

            return DbHelper.ExecuteListQuery<CoControlAsgManualUsuarioItem>(new PortalDB(_config), CodEmpresa, sql);
        }

        /// <summary>
        /// Detalle del expediente (al salir del campo / al seleccionar en popup).
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CoControlAsgManualExpedienteDetalle> Co_ControlAsgManual_Expediente_Detalle_Obtener(int CodEmpresa, string cedula)
        {
            var result = DbHelper.CreateOkResponse(new CoControlAsgManualExpedienteDetalle());

            try
            {
                var ced = NormalizarTexto(cedula);

                var data = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, conn =>
                {
                    var detalle = new CoControlAsgManualExpedienteDetalle
                    {
                        cedula = ced,
                        nombre = ObtenerNombre(conn, ced)
                    };

                    var asg = ObtenerAsignacion(conn, ced);
                    AplicarAsignacion(detalle, asg);

                    var tieneMora = TieneMorosidad(conn, ced);
                    detalle.tiene_morosidad = tieneMora ? 1 : 0;
                    detalle.estado_morosidad = tieneMora ? "MOROSO" : "AL DÍA";

                    return detalle;
                });

                if (data.Code != 0)
                    return DbHelper.CreateErrorResponse<CoControlAsgManualExpedienteDetalle>(data.Description ?? "Error al obtener detalle.");

                result.Result = data.Result ?? new CoControlAsgManualExpedienteDetalle();
            }
            catch (Exception ex)
            {
                result = DbHelper.CreateErrorResponse<CoControlAsgManualExpedienteDetalle>(ex.Message);
            }

            return result;
        }

        private static string NormalizarTexto(string? valor)
            => (valor ?? string.Empty).Trim();

        private static string ObtenerNombre(SqlConnection conn, string cedula)
        {
            return conn.QueryFirstOrDefault<string>(
                "select top 1 rtrim(Nombre) from socios where Cedula = @cedula",
                new { cedula },
                commandTimeout: 0) ?? string.Empty;
        }

        private static dynamic? ObtenerAsignacion(SqlConnection conn, string cedula)
        {
            return conn.QueryFirstOrDefault<dynamic>(
                @"select top 1 rtrim(Usuario) as Usuario, fecha_asignacion as Fecha, isnull(mantener,1) as Mantener
                  from cbr_asignacion where cedula = @cedula",
                new { cedula },
                commandTimeout: 0);
        }

        private static void AplicarAsignacion(CoControlAsgManualExpedienteDetalle detalle, dynamic? asg)
        {
            if (asg == null)
            {
                const string msg = "** Este Expediente no ha sido asignado a ningún oficial **";
                detalle.usuario_actual = string.Empty;
                detalle.mantener = 1;
                detalle.asignacion_texto = msg;
                detalle.oficina_agencia = msg;
                return;
            }

            var usuario = ((string?)asg.Usuario ?? string.Empty).Trim();
            var mantener = asg.Mantener != null ? Convert.ToInt32(asg.Mantener) : 1;
            var fecha = asg.Fecha != null ? (DateTime?)asg.Fecha : null;

            detalle.usuario_actual = usuario;
            detalle.mantener = mantener;

            var fechaTxt = fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy") : string.Empty;
            var texto = $"Oficial : {usuario} / Fecha : {fechaTxt} / Mantener : {(mantener == 1 ? "SI" : "NO")}";

            detalle.asignacion_texto = texto;
            detalle.oficina_agencia = texto;
        }

        private static bool TieneMorosidad(SqlConnection conn, string cedula)
        {
            var mora = conn.QueryFirstOrDefault<int>(
                @"select top 1 1 from Vista_Morosidad Vm
                  inner join Reg_Creditos Reg on Vm.Id_Solicitud = Reg.Id_Solicitud
                  where Reg.Cedula = @cedula",
                new { cedula },
                commandTimeout: 0);

            return mora == 1;
        }

        /// <summary>
        /// Ejecuta la asignación manual (spCBRControlAsg).
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Co_ControlAsgManual_Aplicar(int CodEmpresa,string usuario,CoControlAsgManualAplicarRequest data)
        {
            var cedula = (data?.cedula ?? "").Trim();
            var nuevo = (data?.usuario_nuevo ?? "").Trim();
            var mantener = data?.mantener ?? 1;

            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.ErrorResponse("No se especificó el expediente.", -2);

            if (string.IsNullOrWhiteSpace(nuevo))
                return DbHelper.ErrorResponse("No se especificó el Ejecutivo de cobro a trasladar.", -2);

            var exec = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, conn =>

            {
                var actual = conn.QueryFirstOrDefault<string>(
                    "select top 1 rtrim(Usuario) from cbr_asignacion where cedula = @cedula",
                    new { cedula },
                    commandTimeout: 0) ?? "";

                if (!string.IsNullOrWhiteSpace(actual) &&
                    string.Equals(actual.Trim(), nuevo.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return DbHelper.ErrorResponse("Esta persona ya se encuentra asignada al Ejecutivo.");
                }

                conn.Execute("exec spCBRControlAsg @cedula, @nuevo, @mantener",
                    new { cedula, nuevo, mantener },
                    commandTimeout: 0);

                return DbHelper.OkResponse("Asignación Manual realizada satisfactoriamente.");
            });

            if (exec.Code != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error al aplicar asignación.");

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $" Expediente: {cedula} | Nuevo: {nuevo} | Mantener: {mantener}",
                Movimiento = "Asigna Manual - WEB",
                Modulo = vModulo
            });

            return DbHelper.OkResponse("Asignación Manual realizada satisfactoriamente.");
        }
    }
}
