using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.GA;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmGaDocumentosDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "GAConnString";

        public FrmGaDocumentosDb(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los tipos de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Modulo"></param>
        /// <returns></returns>
        public ErrorDto<List<TiposDocumentosArchivosDto>> TiposDocumentos_Obtener(int CodEmpresa, string Usuario, string Modulo)
        {
            var resp = new ErrorDto<List<TiposDocumentosArchivosDto>>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spGA_Interface_Tipos_Documentos]";
                    var values = new
                    {
                        Empresa = CodEmpresa,
                        Usuario = Usuario,
                        Modulo = Modulo
                    };
                    resp.Result = connection.Query<TiposDocumentosArchivosDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Registra un documento y valida la combinación de empresa, módulo, tipo y llaves.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Documentos_Insertar(int CodEmpresa, DocumentosArchivoDto data)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                if (data is null)
                    return new ErrorDto { Code = -1, Description = "Debe indicar el documento." };

                NormalizarDocumento(data);
                if (CodEmpresa <= 0)
                    return new ErrorDto { Code = -1, Description = "Debe indicar una empresa válida." };

                data.empresaid = CodEmpresa.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(data.moduloid) || string.IsNullOrWhiteSpace(data.typeid)
                    || string.IsNullOrWhiteSpace(data.llave_01) || string.IsNullOrWhiteSpace(data.filename)
                    || data.filecontent is not { Length: > 0 })
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Seleccione el tipo, indique la llave 01 y cargue un archivo con contenido."
                    };

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string queryExist = @"
                        SELECT COUNT(*)
                        FROM GA_Files
                        WHERE EmpresaId = @EmpresaId
                          AND ModuloId = @ModuloId 
                          AND TypeId = @TipoId
                          AND Llave_01 = @Llave_01
                          AND ISNULL(Llave_02, '') = @Llave_02
                          AND ISNULL(Llave_03, '') = @Llave_03";
                    int count = connection.QuerySingle<int>(queryExist, new
                    {
                        EmpresaId = data.empresaid,
                        ModuloId = data.moduloid,
                        TipoId = data.typeid,
                        Llave_01 = data.llave_01,
                        Llave_02 = data.llave_02,
                        Llave_03 = data.llave_03
                    });
                    if (count > 0)
                    {
                        resp.Code = -2;
                        resp.Description = "Ya existe un documento del tipo seleccionado con estas referencias. " +
                            "Si el nuevo documento corresponde a otra referencia, indique su valor en la llave 02 o 03. " +
                            "El archivo nuevo no se guardó.";
                        return resp;
                    }


                    string query = @"
                INSERT INTO GA_Files (
                    EmpresaId, ModuloId, TypeId, Llave_01, Llave_02, Llave_03, FileType, FileName, FileContent, Vencimiento, 
                    FechaEmision, RegistroFecha, RegistroUsuario
                ) VALUES (
                    @EmpresaId, @ModuloId, @TypeId, @Llave_01, @Llave_02, @Llave_03, @FileType, @FileName, @FileContent, @Vencimiento, 
                    @FechaEmision, @RegistroFecha, @RegistroUsuario
                )";

                    using (var command = new SqlCommand(query, connection))
                    {

                        command.Parameters.AddWithValue("@EmpresaId", data.empresaid);
                        command.Parameters.AddWithValue("@ModuloId", data.moduloid);
                        command.Parameters.AddWithValue("@TypeId", data.typeid);
                        command.Parameters.AddWithValue("@Llave_01", data.llave_01);
                        command.Parameters.AddWithValue("@Llave_02", data.llave_02);
                        command.Parameters.AddWithValue("@Llave_03", data.llave_03);
                        command.Parameters.AddWithValue("@FileType", data.filetype);
                        command.Parameters.AddWithValue("@FileName", data.filename);
                        command.Parameters.AddWithValue("@FileContent", data.filecontent != null ? (object)data.filecontent : DBNull.Value); // Maneja datos binarios correctamente
                        command.Parameters.AddWithValue("@Vencimiento", DateTime.Parse(data.vencimiento, System.Globalization.CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("@FechaEmision", DateTime.Parse(data.fechaemision, System.Globalization.CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("@RegistroFecha", DateTime.Parse(data.registrofecha, System.Globalization.CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("@RegistroUsuario", data.registrousuario);

                        connection.Open();
                        command.ExecuteNonQuery(); // Usa ExecuteNonQuery para comandos INSERT
                    }
                }

                resp.Code = 0; // �xito
                resp.Description = "Datos insertados correctamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los documentos
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public List<DocumentosArchivoDto> Documentos_Obtener(GaDocumento filtros)
        {
            List<DocumentosArchivoDto> resp = new List<DocumentosArchivoDto>();
            try
            {
                filtros.llave1 = NormalizarTexto(filtros.llave1);
                filtros.llave2 = NormalizarFiltroOpcional(filtros.llave2);
                filtros.llave3 = NormalizarFiltroOpcional(filtros.llave3);

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    // Build parameterized query
                    string query = "SELECT * FROM GA_Files WHERE Llave_01 = @llave1";
                    var parameters = new DynamicParameters();
                    parameters.Add("@llave1", filtros.llave1, DbType.String);

                    if (filtros.llave2 != null)
                    {
                        query += " AND Llave_02 = @llave2";
                        parameters.Add("@llave2", filtros.llave2, DbType.String);
                    }

                    if (filtros.llave3 != null)
                    {
                        // Incluye la llave exacta y sus consecutivos de carga múltiple (llave3-2, llave3-3...)
                        query += @" AND (ISNULL(Llave_03, '') = @llave3
                                    OR ISNULL(Llave_03, '') LIKE @llave3Consecutivo ESCAPE '\')";
                        parameters.Add("@llave3", filtros.llave3, DbType.String);
                        parameters.Add("@llave3Consecutivo", EscaparLike(filtros.llave3) + "-[0-9]%", DbType.String);
                    }

                    resp = connection.Query<DocumentosArchivoDto>(query, parameters).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        private static void NormalizarDocumento(DocumentosArchivoDto data)
        {
            data.empresaid = NormalizarTexto(data.empresaid);
            data.moduloid = NormalizarTexto(data.moduloid);
            data.typeid = NormalizarTexto(data.typeid);
            data.llave_01 = NormalizarTexto(data.llave_01);
            data.llave_02 = NormalizarTexto(data.llave_02);
            data.llave_03 = NormalizarTexto(data.llave_03);
            data.filename = NormalizarTexto(data.filename);
            data.filetype = NormalizarTexto(data.filetype);
            data.registrousuario = NormalizarTexto(data.registrousuario);
        }

        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        private static string EscaparLike(string valor)
        {
            return valor.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
        }

        private static string? NormalizarFiltroOpcional(string? valor)
        {
            // Llave vacía o nula = sin filtro (paridad VB6: se buscan documentos relacionados a la llave 1)
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        /// <summary>
        /// Respalda y elimina únicamente el archivo seleccionado, validando empresa,
        /// módulo, tipo y llaves. El procedimiento devuelve 0 o -2.
        /// </summary>
        public ErrorDto Documentos_Eliminar(int CodEmpresa, GaDocumentoEliminarRequest request)
        {
            try
            {
                if (request is null || CodEmpresa <= 0 || request.fileid <= 0
                    || string.IsNullOrWhiteSpace(request.typeid)
                    || string.IsNullOrWhiteSpace(request.moduloid)
                    || string.IsNullOrWhiteSpace(request.llave01)
                    || string.IsNullOrWhiteSpace(request.usuario))
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Debe seleccionar un documento válido para eliminar."
                    };

                using var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));
                connection.Open();

                var codigo = connection.QuerySingle<int>(
                    "dbo.spGA_EliminarDocumentos",
                    new
                    {
                        Llave01 = NormalizarTexto(request.llave01),
                        Llave02 = NormalizarTexto(request.llave02),
                        Llave03 = NormalizarTexto(request.llave03),
                        Usuario = request.usuario.Trim(),
                        TypeId = request.typeid.Trim(),
                        EmpresaId = CodEmpresa,
                        ModuloId = request.moduloid.Trim(),
                        FileId = request.fileid
                    },
                    commandType: CommandType.StoredProcedure);

                return new ErrorDto
                {
                    Code = codigo,
                    Description = codigo == 0
                        ? "Documento eliminado correctamente."
                        : "El documento ya no existe o no coincide con la empresa, tipo o referencias seleccionadas. Actualice la lista."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }
    }
}
