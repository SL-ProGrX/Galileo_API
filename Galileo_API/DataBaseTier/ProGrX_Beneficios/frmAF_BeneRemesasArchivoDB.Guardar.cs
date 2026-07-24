using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneRemesasArchivoDB
    {
        /// <summary>
        /// Inserta una remesa nueva (calculando su consecutivo) o actualiza la nota de origen si ya existe.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="remesa">Datos de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto RemesaArchivo_Guardar(int CodCliente, RmsRemesasData remesa)
        {
            var origen = _config.GetSection("AFI_Beneficios").GetSection("BeneDepOrigen").Value;
            var destino = _config.GetSection("AFI_Beneficios").GetSection("BeneDepDestino").Value;

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                if (remesa.IdRemesa == 0)
                {
                    const string sqlCod = @"SELECT ISNULL(MAX(CodRemesa), 0) + 1
                                            FROM RMS_Remesas WHERE IdTipoDocumento = @IdTipoDocumento";
                    var codRemesa = connection.QueryFirstOrDefault<int>(sqlCod, new { remesa.IdTipoDocumento });

                    const string sqlInsert = @"INSERT INTO RMS_Remesas
                                                (CodRemesa, IdTipoDocumento, CodDepartamentoOrigen, CodDepartamentoDestino,
                                                 RegistroUsuario, RegistroFecha, NotaOrigen, IdEstado, Activa)
                                               VALUES
                                                (@codRemesa, @IdTipoDocumento, @origen, @destino,
                                                 @RegistroUsuario, GETDATE(), @NotaOrigen, @estado, 0)";

                    connection.Execute(sqlInsert, new
                    {
                        codRemesa,
                        remesa.IdTipoDocumento,
                        origen,
                        destino,
                        remesa.RegistroUsuario,
                        remesa.NotaOrigen,
                        estado = EstadoRemesa
                    });
                }
                else
                {
                    const string sqlUpdate = "UPDATE RMS_Remesas SET NotaOrigen = @NotaOrigen WHERE IdRemesa = @IdRemesa";
                    connection.Execute(sqlUpdate, new { remesa.NotaOrigen, remesa.IdRemesa });
                }

                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta el detalle de documentos de una remesa.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="idRemesa">Identificador de la remesa.</param>
        /// <param name="usuario">Usuario que registra el detalle.</param>
        /// <param name="documentos">Documentos a incluir.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto RemesaDetalle_Guardar(int CodCliente, int idRemesa, string usuario, List<RmsRemesaDocuementos> documentos)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sql = @"INSERT INTO RMS_RemesasDetalle
                                        (IdRemesa, Documento, DocumentoRegistroFecha, DocumentoRegistroUsuario,
                                         DocumentoIdAsociado, DocumentoNombreAsociado, RegistroUsuario, RegistroFecha, IdEstado, Mascara)
                                     VALUES
                                        (@idRemesa, @documento, @fecha, @registra_user,
                                         @cedula, @nombre, @usuario, GETDATE(), @estado, @n_expediente)";

                foreach (var item in documentos)
                {
                    var fecha = MProGrXAuxiliarDB.validaFechaGlobal(item.registra_fecha, FechaFormat);

                    connection.Execute(sql, new
                    {
                        idRemesa,
                        documento = item.id_beneficio,
                        fecha,
                        item.registra_user,
                        item.cedula,
                        item.nombre,
                        usuario,
                        estado = EstadoRemesa,
                        item.n_expediente
                    });
                }

                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
