using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_CR_Renuncias_TagsDB
    {
        private readonly IConfiguration? _config;

        public frmAF_CR_Renuncias_TagsDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista de renuncias tags.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Estado"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_CR_Renuncias_TagsData>> AF_CR_Renuncias_Tags_Obtener(int CodEmpresa, string Estado, string Filtro)
        {
            var result = new ErrorDto<List<AF_CR_Renuncias_TagsData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_CR_Renuncias_TagsData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Estado,
                    Filtro = Filtro ?? ""
                };

                result.Result = connection.Query<AF_CR_Renuncias_TagsData>(
                    "spAFI_Renuncias_Control_Consulta",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el SP spAFI_Renuncia_Recepcion_Aplica para aplicar la recepción de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="recepcionDatos"></param>
        /// <returns></returns>
        public ErrorDto AF_CR_Renuncia_Recepcion_Aplica(int CodEmpresa, AF_CR_RenunciaRecepcionAplica recepcionDatos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    recepcionDatos.RenunciaId,
                    recepcionDatos.Usuario,
                    Notas = recepcionDatos.Notas ?? "",
                    Equipo = recepcionDatos.Equipo ?? "",
                    Version = recepcionDatos.Version ?? ""
                };

                connection.Execute(
                    "spAFI_Renuncia_Recepcion_Aplica",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el SP spAFI_Renuncia_Revision_Aplica para aplicar la revisión de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="revisionDatos"></param>
        /// <returns></returns>
        public ErrorDto AF_CR_Renuncia_Revision_Aplica(int CodEmpresa, AF_CR_RenunciaRevisionAplica revisionDatos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    RenunciaId = revisionDatos.RenunciaId,
                    Usuario = revisionDatos.Usuario ?? "",
                    Notas = revisionDatos.Notas ?? "",
                    Equipo = revisionDatos.Equipo ?? "",
                    Version = revisionDatos.Version ?? "",
                    Estado = revisionDatos.Estado ?? ""
                };

                connection.Execute(
                    "spAFI_Renuncia_Revision_Aplica",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el SP spAFI_Renuncia_Etiquetas_Consulta para obtener las etiquetas de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="RenunciaId"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_CR_RenunciaEtiquetas>> AF_CR_Renuncia_Etiquetas_Consulta(int CodEmpresa, int RenunciaId)
        {
            var result = new ErrorDto<List<AF_CR_RenunciaEtiquetas>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_CR_RenunciaEtiquetas>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new { RenunciaId };

                result.Result = connection.Query<AF_CR_RenunciaEtiquetas>(
                    "spAFI_Renuncia_Etiquetas_Consulta",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta la función fxAFI_Renuncia_Revision_Reversar_Valida y retorna un int.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="RenunciaId"></param>
        /// <returns></returns>
        public ErrorDto<int> AF_CR_Renuncia_Revision_Reversar_Valida(int CodEmpresa, int RenunciaId)
        {
            var result = new ErrorDto<int>()
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = "SELECT dbo.fxAFI_Renuncia_Revision_Reversar_Valida(@RenunciaId)";
                result.Result = connection.QueryFirstOrDefault<int>(query, new { RenunciaId });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el SP spAFI_Renuncia_Revision_Reversar para reversar la revisión de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto AF_CR_Renuncia_Revision_Reversar(int CodEmpresa, AF_CR_RenunciaReversa dto)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    RenunciaId = dto.RenunciaId,
                    Usuario = dto.Usuario ?? "",
                    NotasReversa = dto.NotasReversa ?? "",
                    Equipo = dto.Equipo ?? "",
                    Version = dto.Version ?? "",                    
                };

                connection.Execute(
                    "spAFI_Renuncia_Revision_Reversar",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de renuncias pendientes de recibir.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_CR_Renuncias_TagsData>> AF_CR_Renuncias_Pendientes_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<AF_CR_Renuncias_TagsData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_CR_Renuncias_TagsData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT COD_RENUNCIA AS Cod_Renuncia, CEDULA, NOMBRE, Estado_Desc
                                 FROM vAFI_Renuncias_Pendientes_Recibir";

                result.Result = connection.Query<AF_CR_Renuncias_TagsData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de todas las renuncias.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_CR_Renuncias_TagsData>> AF_CR_Renuncias_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<AF_CR_Renuncias_TagsData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_CR_Renuncias_TagsData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT COD_RENUNCIA AS Cod_Renuncia, CEDULA, NOMBRE, Estado_Desc
                                 FROM vAFI_Renuncias";

                result.Result = connection.Query<AF_CR_Renuncias_TagsData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}
