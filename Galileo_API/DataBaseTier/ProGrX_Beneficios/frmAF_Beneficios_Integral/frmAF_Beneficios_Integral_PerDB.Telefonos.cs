using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralPerDB
    {
        /// <summary>
        /// Guarda el teléfono del socio: inserta (id_telefono 0) o actualiza.
        /// </summary>
        public ErrorDto Telefono_Guardar(int CodCliente, AfiBeneTelefonoGuardar telefono)
        {
            try
            {
                if (telefono.id_telefono != 0)
                {
                    return Telefono_Actualizar(CodCliente, telefono)
                        ? new ErrorDto { Code = 0, Description = telefono.id_telefono.ToString() }
                        : new ErrorDto { Code = -1, Description = "Error al actualizar el Asociado" };
                }

                return Telefono_Agregar(CodCliente, telefono);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = "Telefono_Guardar - " + ex.Message };
            }
        }

        /// <summary>
        /// Inserta un nuevo teléfono y devuelve el id generado en Description.
        /// </summary>
        private ErrorDto Telefono_Agregar(int CodCliente, AfiBeneTelefonoGuardar telefono)
        {
            const string sqlInsert = @"
                INSERT INTO [dbo].[AFI_BENE_REGISTRO_TELEFONOS]
                    ([COD_BENEFICIO],[CONSEC],[TIPO],[CONTACTO],[TELEFONO],[EXT],[CEDULA],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                VALUES
                    (@codBeneficio,@consec,@tipo,@contacto,@telefono,@ext,@cedula,GETDATE(),@usuario)";

            const string sqlId = "SELECT IDENT_CURRENT('AFI_BENE_REGISTRO_TELEFONOS') AS id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                connection.Execute(sqlInsert, new
                {
                    codBeneficio = telefono.cod_beneficio,
                    consec = telefono.consec,
                    tipo = telefono.tipo?.item,
                    contacto = telefono.contacto,
                    telefono = telefono.telefono,
                    ext = telefono.ext,
                    cedula = telefono.cedula,
                    usuario = telefono.user_registra
                });

                return connection.QueryFirstOrDefault<int>(sqlId);
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Result.ToString() : "Telefono_Agregar - " + result.Description
            };
        }

        /// <summary>
        /// Actualiza un teléfono existente. Devuelve false ante error.
        /// </summary>
        private bool Telefono_Actualizar(int CodCliente, AfiBeneTelefonoGuardar telefono)
        {
            const string sqlUpdate = @"
                UPDATE [dbo].[AFI_BENE_REGISTRO_TELEFONOS]
                   SET [TIPO] = @tipo, [TELEFONO] = @telefono, [CONTACTO] = @contacto, [EXT] = @ext
                 WHERE cedula = @cedula AND id_telefono = @idTelefono";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sqlUpdate, new
                {
                    tipo = telefono.tipo?.item,
                    telefono = telefono.telefono,
                    contacto = telefono.contacto,
                    ext = telefono.ext,
                    cedula = telefono.cedula,
                    idTelefono = telefono.id_telefono
                }));

            return result.Code == 0;
        }

        /// <summary>
        /// Obtiene los teléfonos del socio, sincronizando primero los del SIF que aún no existan.
        /// </summary>
        public ErrorDto<List<AfiBeneTelefono>> Telefonos_Obtener(int CodCliente, string cedula)
        
        {
            //const string sqlSync = @"
            //    INSERT INTO AFI_BENE_REGISTRO_TELEFONOS
            //        (COD_BENEFICIO, CONSEC, TIPO, TELEFONO, EXT, CONTACTO, REGISTRO_FECHA, REGISTRO_USUARIO, CEDULA)
            //    SELECT '1' AS COD_BENEFICIO, TELEFONO AS CONSEC, TIPO, NUMERO AS TELEFONO, EXT, CONTACTO,
            //           FECHA AS REGISTRO_FECHA, USUARIO AS REGISTRO_USUARIO, CEDULA
            //    FROM TELEFONOS T
            //    WHERE T.CEDULA = @cedula
            //      AND NUMERO IS NOT NULL
            //      AND NOT EXISTS (
            //            SELECT 1 FROM AFI_BENE_REGISTRO_TELEFONOS A
            //            WHERE A.CEDULA = T.CEDULA AND A.TELEFONO = T.NUMERO AND CEDULA = @cedula
            //      )";

            const string sqlSelect = "SELECT * FROM AFI_BENE_REGISTRO_TELEFONOS WHERE CEDULA = @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                //connection.Execute(sqlSync, new { cedula });
                return connection.Query<AfiBeneTelefono>(sqlSelect, new { cedula }).ToList();
            });

            return new ErrorDto<List<AfiBeneTelefono>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "Telefonos_Obtener - " + result.Description,
                Result = result.Result ?? new List<AfiBeneTelefono>()
            };
        }

        /// <summary>
        /// Elimina un teléfono del socio por identificador.
        /// </summary>
        public ErrorDto Telefono_Eliminar(int CodCliente, int id, string usuario)
        {
            const string sql = "DELETE AFI_BENE_REGISTRO_TELEFONOS WHERE id_telefono = @id";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sql, new { id }));

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "Telefono_Eliminar" + result.Description
            };
        }
    }
}
