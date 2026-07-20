using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralAprDB
    {
        /// <summary>
        /// Obtiene las justificaciones registradas del expediente del socio.
        /// </summary>
        public ErrorDto<List<AfiBeneApreJustificacion>> BeneJustificaciones_Obtener(int CodCliente, string cedula, int expediente)
        {
            const string sql = @"
                SELECT [ID_JUSTIFICACION],[COD_BENEFICIO],[CONSEC],[CEDULA],[JUST_LIST_ID],[JUSTIFICACION],[ADVERTENCIA],
                       [ESTADO],[TIPO_BENEFICIO],[REGISTRO_FECHA],[REGISTRO_USUARIO],[MODIFICA_FECHA],[MODIFICA_USUARIO]
                FROM AFI_BENE_REGISTRO_JUSTIFICACIONES
                WHERE CEDULA = @cedula AND CONSEC = @expediente";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneApreJustificacion>(sql, new { cedula = cedula.Trim(), expediente }).ToList());

            return new ErrorDto<List<AfiBeneApreJustificacion>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneApreJustificacion>()
            };
        }

        /// <summary>
        /// Guarda una justificación: inserta (id 0) o actualiza.
        /// </summary>
        public ErrorDto BeneJustificacion_Guardar(int CodCliente, AfiBeneApreJustificacionGuardar justificacion)
        {
            try
            {
                return justificacion.id_justificacion != 0
                    ? BeneJustificacion_Actualizar(CodCliente, justificacion)
                    : BeneJustificacion_Insertar(CodCliente, justificacion);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Inserta una justificación y devuelve el id generado en Description.
        /// </summary>
        private ErrorDto BeneJustificacion_Insertar(int CodCliente, AfiBeneApreJustificacionGuardar justificacion)
        {
            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_REGISTRO_JUSTIFICACIONES]
                    ([COD_BENEFICIO],[CONSEC],[CEDULA],[JUST_LIST_ID],[JUSTIFICACION],[ADVERTENCIA],[ESTADO],[TIPO_BENEFICIO],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                VALUES
                    (@codBeneficio,@consec,@cedula,@justListId,@justificacion,@advertencia,@estado,@tipoBeneficio,GETDATE(),@registroUsuario)";

            const string sqlId = "SELECT IDENT_CURRENT('AFI_BENE_REGISTRO_JUSTIFICACIONES') AS id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                connection.Execute(sqlInsert, new
                {
                    codBeneficio = justificacion.cod_beneficio,
                    consec = justificacion.consec,
                    cedula = justificacion.cedula,
                    justListId = ItemOrEmpty(justificacion.just_list_id),
                    justificacion = justificacion.just_list_id?.descripcion,
                    advertencia = justificacion.advertencia,
                    estado = justificacion.estado,
                    tipoBeneficio = justificacion.tipo_beneficio,
                    registroUsuario = justificacion.registro_usuario
                });

                return connection.QueryFirstOrDefault<int>(sqlId);
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Result.ToString() : result.Description
            };
        }

        /// <summary>
        /// Actualiza una justificación existente.
        /// </summary>
        private ErrorDto BeneJustificacion_Actualizar(int CodCliente, AfiBeneApreJustificacionGuardar justificacion)
        {
            const string sqlUpdate = @"
                UPDATE AFI_BENE_REGISTRO_JUSTIFICACIONES
                   SET [JUST_LIST_ID] = @justListId, [JUSTIFICACION] = @justificacion, [ADVERTENCIA] = @advertencia,
                       [ESTADO] = @estado, [MODIFICA_FECHA] = GETDATE(), [MODIFICA_USUARIO] = @modificaUsuario
                 WHERE ID_JUSTIFICACION = @idJustificacion";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sqlUpdate, new
                {
                    justListId = ItemOrEmpty(justificacion.just_list_id),
                    justificacion = justificacion.justificacion,
                    advertencia = justificacion.advertencia,
                    estado = justificacion.estado,
                    modificaUsuario = justificacion.modifica_usuario,
                    idJustificacion = justificacion.id_justificacion
                }));

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? justificacion.id_justificacion.ToString() : result.Description
            };
        }

        /// <summary>
        /// Elimina una justificación.
        /// </summary>
        public ErrorDto BeneJustificacion_Eliminar(int CodCliente, int id_justificacion, string usuario)
        {
            const string sql = "DELETE AFI_BENE_REGISTRO_JUSTIFICACIONES WHERE ID_JUSTIFICACION = @idJustificacion";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sql, new { idJustificacion = id_justificacion }));

            return new ErrorDto { Code = result.Code, Description = result.Description };
        }
    }
}
