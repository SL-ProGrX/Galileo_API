using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralDB
    {
        /// <summary>
        /// Obtiene las observaciones del beneficio seleccionado.
        /// </summary>
        public ErrorDto<List<AfiBeneObservaciones>> BeneIntegralObservaciones_Obtener(int CodCliente, int consec, string cod_beneficio)
        {
            const string sql = @"
                SELECT [ID_OBSERVACION],[COD_BENEFICIO],[CONSEC],[OBSERVACION],[REGISTRO_FECHA],[REGISTRO_USUARIO]
                FROM AFI_BENE_REGISTRO_OBSERVACIONES
                WHERE COD_BENEFICIO = @codBeneficio AND CONSEC = @consec";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneObservaciones>(sql, new { codBeneficio = cod_beneficio, consec }).ToList());

            return new ErrorDto<List<AfiBeneObservaciones>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BeneIntegralObservaciones_Obtener: " + result.Description,
                Result = result.Result ?? new List<AfiBeneObservaciones>()
            };
        }

        /// <summary>
        /// Guarda una observación: inserta (id 0) o actualiza.
        /// </summary>
        public ErrorDto BeneIntegralObservaciones_Guardar(int CodCliente, AfiBeneObservaciones observacion)
        {
            try
            {
                return observacion.id_observacion == 0
                    ? BeneIntegralObservaciones_Insertar(CodCliente, observacion)
                    : BeneIntegralObservaciones_Actualizar(CodCliente, observacion);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Inserta una observación nueva y deja traza en bitácora.
        /// </summary>
        private ErrorDto BeneIntegralObservaciones_Insertar(int CodCliente, AfiBeneObservaciones observacion)
        {
            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_REGISTRO_OBSERVACIONES]
                    ([COD_BENEFICIO],[CONSEC],[OBSERVACION],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                VALUES (@codBeneficio,@consec,@observacion,GETDATE(),@usuario)";

            const string sqlId = "SELECT IDENT_CURRENT('AFI_BENE_REGISTRO_OBSERVACIONES') AS id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                connection.Execute(sqlInsert, new
                {
                    codBeneficio = observacion.cod_beneficio,
                    consec = observacion.consec,
                    observacion = observacion.observacion,
                    usuario = observacion.registro_usuario
                });

                var id = connection.QueryFirstOrDefault<int>(sqlId);

                RegistrarBitacora(CodCliente, observacion.cod_beneficio, observacion.consec, "Inserta",
                    $"Inserta Observación [{observacion.observacion}]", observacion.registro_usuario);

                return id;
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Result.ToString() : result.Description
            };
        }

        /// <summary>
        /// Actualiza una observación existente y deja traza en bitácora.
        /// </summary>
        private ErrorDto BeneIntegralObservaciones_Actualizar(int CodCliente, AfiBeneObservaciones observacion)
        {
            const string sqlAnterior = "SELECT OBSERVACION FROM AFI_BENE_REGISTRO_OBSERVACIONES WHERE ID_OBSERVACION = @idObservacion";
            const string sqlUpdate = @"UPDATE [AFI_BENE_REGISTRO_OBSERVACIONES]
                                          SET [OBSERVACION] = @observacion
                                        WHERE ID_OBSERVACION = @idObservacion";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var anterior = connection.QueryFirstOrDefault<string>(sqlAnterior, new { idObservacion = observacion.id_observacion });

                connection.Execute(sqlUpdate, new { observacion = observacion.observacion, idObservacion = observacion.id_observacion });

                RegistrarBitacora(CodCliente, observacion.cod_beneficio, observacion.consec, "Actualiza",
                    $"Actualiza Observación [{anterior}] por [{observacion.observacion}]", observacion.registro_usuario);

                return observacion.id_observacion;
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Result.ToString() : result.Description
            };
        }

        /// <summary>
        /// Elimina una observación y deja traza en bitácora.
        /// </summary>
        public ErrorDto BeneIntegralObservaciones_Eliminar(int CodCliente, int id_observacion, string usuario)
        {
            const string sqlRegistro = "SELECT COD_BENEFICIO, CONSEC, OBSERVACION FROM AFI_BENE_REGISTRO_OBSERVACIONES WHERE ID_OBSERVACION = @idObservacion";
            const string sqlDelete = "DELETE FROM [AFI_BENE_REGISTRO_OBSERVACIONES] WHERE ID_OBSERVACION = @idObservacion";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var registro = connection.QueryFirstOrDefault<AfiBeneObservaciones>(sqlRegistro, new { idObservacion = id_observacion })
                               ?? new AfiBeneObservaciones();

                connection.Execute(sqlDelete, new { idObservacion = id_observacion });

                RegistrarBitacora(CodCliente, registro.cod_beneficio, registro.consec, "Elimina",
                    $"Elimina Observación [{registro.observacion}]", usuario);

                return 0;
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Description
            };
        }
    }
}
