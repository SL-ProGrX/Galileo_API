using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCrParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 1;

        public FrmAFCrParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de parámetros de control de renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrParametrosData>> AF_CRParametros_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AfCrParametrosData>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT * FROM afi_cr_parametros");
        }

        /// <summary>
        /// Guarda (inserta o actualiza) los parámetros de control de renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto AF_CRParametros_Guardar(int CodEmpresa, string usuario, AfCrParametrosData parametros)
        {
            if (parametros is null)
            {
                return DbHelper.ErrorResponse("Los parámetros de renuncia son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var queryExiste = @"SELECT TOP 1 id FROM afi_cr_parametros ORDER BY id";
                var existe = connection.QueryFirstOrDefault<int?>(queryExiste);

                if (parametros.isNew)
                {
                    return existe.HasValue
                        ? DbHelper.ErrorResponse("Ya existe un registro de parámetros.", -2)
                        : AF_CRParametros_Insertar(connection, CodEmpresa, usuario, parametros);
                }

                if (!existe.HasValue)
                {
                    return DbHelper.ErrorResponse("No existe ningún registro de parámetros para actualizar.", -2);
                }

                parametros.id = existe.Value;
                return AF_CRParametros_Actualizar(connection, CodEmpresa, usuario, parametros);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar parámetros de renuncia.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo registro de parámetros.
        /// </summary>
        private ErrorDto AF_CRParametros_Insertar(SqlConnection connection, int CodEmpresa, string usuario, AfCrParametrosData parametros)
        {
            const string queryInsert = @"INSERT INTO afi_cr_parametros
                (dias_vence, liq_pat_control, fecha_limite, tipo_vencimiento, utiliza_zonas, activar_control)
                VALUES (@dias_vence, @liq_pat_control, @fecha_limite, @tipo_vencimiento, @utiliza_zonas, @activar_control)";

            connection.Execute(queryInsert, new
            {
                parametros.dias_vence,
                liq_pat_control = parametros.liq_pat_control == true ? 1 : 0,
                parametros.fecha_limite,
                parametros.tipo_vencimiento,
                utiliza_zonas = parametros.utiliza_zonas == true ? 1 : 0,
                activar_control = parametros.activar_control == true ? 1 : 0
            });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                parametros,
                "Registra - WEB");

            return DbHelper.OkResponse("Insertado correctamente");
        }

        /// <summary>
        /// Actualiza un registro de parámetros existente.
        /// </summary>
        private ErrorDto AF_CRParametros_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, AfCrParametrosData parametros)
        {
            const string queryUpdate = @"UPDATE afi_cr_parametros
                SET dias_vence = @dias_vence,
                    liq_pat_control = @liq_pat_control,
                    fecha_limite = @fecha_limite,
                    tipo_vencimiento = @tipo_vencimiento,
                    utiliza_zonas = @utiliza_zonas,
                    activar_control = @activar_control
                WHERE id = @id";

            connection.Execute(queryUpdate, new
            {
                parametros.id,
                parametros.dias_vence,
                liq_pat_control = parametros.liq_pat_control == true ? 1 : 0,
                parametros.fecha_limite,
                parametros.tipo_vencimiento,
                utiliza_zonas = parametros.utiliza_zonas == true ? 1 : 0,
                activar_control = parametros.activar_control == true ? 1 : 0
            });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                parametros,
                "Modifica - WEB");

            return DbHelper.OkResponse("Actualizado correctamente");
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, AfCrParametrosData parametros, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Parámetros Renuncia: {parametros.dias_vence}, {parametros.liq_pat_control}, {parametros.fecha_limite}, {parametros.tipo_vencimiento}, {parametros.utiliza_zonas}, {parametros.activar_control}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
