using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_CRSeguimientoDB
    {
        private readonly IConfiguration? _config;

        public frmAF_CRSeguimientoDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene el seguimiento de renuncias según los filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrSeguimientoData>> AF_CR_Seguimiento_Obtener(int CodEmpresa, AfCrSeguimientoFiltros filtros)
        {
            var result = new ErrorDto<List<AfCrSeguimientoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfCrSeguimientoData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var sb = new StringBuilder(@"
                    SELECT
                        '' AS Btn,
                        R.cod_renuncia AS CodRenuncia,
                        R.Estado_Desc AS EstadoDesc,
                        R.cedula AS Cedula,
                        R.Nombre AS Nombre,
                        R.Tipo_Renuncia AS TipoRenuncia,
                        R.vencimiento AS Vencimiento,
                        R.Causa_Desc AS CausaDesc,
                        R.Ejecutivo_Desc AS EjecutivoDesc,
                        R.registro_user AS RegistroUser,
                        R.registro_Fecha AS RegistroFecha,
                        R.Resuelto_User AS ResueltoUser,
                        R.Resuelto_Fecha AS ResueltoFecha
                    FROM vAFI_Renuncias R
                    WHERE R.Cod_Renuncia BETWEEN @RenunciaIni AND @RenunciaFin
                ");

                var parameters = new DynamicParameters();
                parameters.Add("RenunciaIni", filtros.RenunciaIni ?? 0);
                parameters.Add("RenunciaFin", filtros.RenunciaFin ?? int.MaxValue);

                if (!string.IsNullOrWhiteSpace(filtros.Estado))
                {
                    sb.AppendLine("AND R.Estado = @Estado");
                    parameters.Add("Estado", filtros.Estado);
                }
                if (!string.IsNullOrWhiteSpace(filtros.TipoChar))
                {
                    sb.AppendLine("AND R.Tipo = @TipoChar");
                    parameters.Add("TipoChar", filtros.TipoChar);
                }
                if (!string.IsNullOrWhiteSpace(filtros.Cedula))
                {
                    sb.AppendLine("AND R.cedula LIKE @Cedula");
                    parameters.Add("Cedula", $"%{filtros.Cedula}%");
                }
                if (!string.IsNullOrWhiteSpace(filtros.Nombre))
                {
                    sb.AppendLine("AND R.Nombre LIKE @Nombre");
                    parameters.Add("Nombre", $"%{filtros.Nombre}%");
                }
                if (!string.IsNullOrWhiteSpace(filtros.Usuario))
                {
                    sb.AppendLine("AND R.registro_user LIKE @Usuario");
                    parameters.Add("Usuario", $"%{filtros.Usuario}%");
                }
                if (!string.IsNullOrWhiteSpace(filtros.Ejecutivo))
                {
                    sb.AppendLine("AND R.Ejecutivo_Desc LIKE @Ejecutivo");
                    parameters.Add("Ejecutivo", $"%{filtros.Ejecutivo}%");
                }
                if (filtros.IdCausa.HasValue && filtros.IdCausa > 0)
                {
                    sb.AppendLine("AND R.Id_Causa = @IdCausa");
                    parameters.Add("IdCausa", filtros.IdCausa);
                }
                if (filtros.IdInstitucion.HasValue && filtros.IdInstitucion > 0)
                {
                    sb.AppendLine("AND R.cod_Institucion = @IdInstitucion");
                    parameters.Add("IdInstitucion", filtros.IdInstitucion);
                }
                if (!string.IsNullOrWhiteSpace(filtros.Provincia) && filtros.Provincia != "0")
                {
                    sb.AppendLine("AND R.Provincia = @Provincia");
                    parameters.Add("Provincia", filtros.Provincia);
                }
                if (!string.IsNullOrWhiteSpace(filtros.Zona) && filtros.Zona != "0")
                {
                    sb.AppendLine("AND dbo.fxAfi_Zonas_Aplica(@Zona, @UsuarioActual, R.cod_Institucion, R.UP) = 1");
                    parameters.Add("Zona", filtros.Zona);
                    parameters.Add("UsuarioActual", filtros.UsuarioActual ?? "");
                }
                // Fechas
                if (!string.IsNullOrWhiteSpace(filtros.TipoFecha) && filtros.FIni.HasValue && filtros.FFin.HasValue)
                {
                    if (filtros.TipoFecha == "Registro")
                        sb.AppendLine("AND R.registro_Fecha BETWEEN @FIni AND @FFin");
                    else if (filtros.TipoFecha == "Vencimiento")
                        sb.AppendLine("AND R.Vencimiento BETWEEN @FIni AND @FFin");
                    else if (filtros.TipoFecha == "Resolución")
                        sb.AppendLine("AND R.Resuelto_Fecha BETWEEN @FIni AND @FFin");

                    parameters.Add("FIni", filtros.FIni.Value);
                    parameters.Add("FFin", filtros.FFin.Value);
                }
                // Checks
                if (filtros.AplicarChecks)
                {
                    if (filtros.Mortalidad.HasValue)
                    {
                        sb.AppendLine("AND R.Mortalidad = @Mortalidad");
                        parameters.Add("Mortalidad", filtros.Mortalidad);
                    }
                    if (filtros.Reingreso.HasValue)
                    {
                        sb.AppendLine("AND R.APLICA_REINGRESO = @Reingreso");
                        parameters.Add("Reingreso", filtros.Reingreso);
                    }
                    if (filtros.Volver.HasValue)
                    {
                        sb.AppendLine("AND R.Volver = @Volver");
                        parameters.Add("Volver", filtros.Volver);
                    }
                    if (filtros.AumentoTasas.HasValue)
                    {
                        sb.AppendLine("AND R.Aumenta_Puntos = @AumentoTasas");
                        parameters.Add("AumentoTasas", filtros.AumentoTasas);
                    }
                }

                sb.AppendLine("ORDER BY R.cod_renuncia");

                result.Result = connection.Query<AfCrSeguimientoData>(sb.ToString(), parameters).ToList();
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
        /// Obtiene la lista de gestiones para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Gestiones(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT RTRIM(cod_gestion) AS item, RTRIM(descripcion) AS descripcion FROM afi_cr_gestiones";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene la lista de causas activas para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Causas(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT id_causa AS item, RTRIM(descripcion) AS descripcion FROM causas_renuncias WHERE ACTIVO = 1";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene la lista de instituciones para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Institucion(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT cod_institucion AS item, RTRIM(descripcion) AS descripcion FROM Instituciones ORDER BY descripcion";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene la lista de provincias para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Provincia(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT Provincia AS item, RTRIM(descripcion) AS descripcion FROM Provincias ORDER BY Provincia";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene la lista de zonas para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Zona(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT COD_ZONA AS item, RTRIM(descripcion) AS descripcion FROM AFI_ZONAS ORDER BY descripcion";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene el detalle de una renuncia por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codRenuncia"></param>
        /// <returns></returns>
        public ErrorDto<AfCrSeguimientoDetalle> AF_CR_Seguimiento_Obtener_Detalle_Renuncia(int CodEmpresa, int codRenuncia)
        {
            var result = new ErrorDto<AfCrSeguimientoDetalle>()
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    SELECT R.*,
                           RTRIM(C.Descripcion)                    AS CausaX,
                           S.nombre,
                           ISNULL(P.id_promotor,0)                 AS Id_Promotor,
                           ISNULL(P.nombre,'AFILIACION UNIVERSAL') AS PromotorX
                    FROM afi_cr_renuncias R
                    INNER JOIN causas_renuncias C ON R.id_causa = C.id_causa
                    INNER JOIN Socios          S ON R.cedula   = S.cedula
                    LEFT  JOIN Promotores      P ON R.id_Promotor = P.id_Promotor
                    WHERE R.cod_renuncia = @codRenuncia";

                result.Result = connection.QueryFirstOrDefault<AfCrSeguimientoDetalle>(query, new { codRenuncia });
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
        /// Obtiene los motivos de una renuncia usando el SP spAFI_CR_Motivos_Consulta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="renunciaId"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrSeguimientoMotivo>> AF_CR_Seguimiento_Obtener_Motivos(int CodEmpresa, int renunciaId)
        {
            var result = new ErrorDto<List<AfCrSeguimientoMotivo>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfCrSeguimientoMotivo>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new { RenunciaId = renunciaId, Todos = 1 };

                result.Result = connection.Query<AfCrSeguimientoMotivo>(
                    "spAFI_CR_Motivos_Consulta",
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
        /// Obtiene el historial de seguimiento de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codRenuncia"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrSeguimientoHistorial>> AF_CR_Seguimiento_Obtener_Historial(int CodEmpresa, int codRenuncia)
        {
            var result = new ErrorDto<List<AfCrSeguimientoHistorial>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfCrSeguimientoHistorial>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT * FROM afi_cr_seguimiento WHERE cod_renuncia = @codRenuncia";

                result.Result = connection.Query<AfCrSeguimientoHistorial>(query, new { codRenuncia }).ToList();
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
        /// Obtiene la lista de gestiones para seguimiento (formato IdX/ItmX).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Gestion(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"SELECT RTRIM(cod_gestion) AS item, RTRIM(descripcion) AS descripcion FROM afi_cr_gestiones";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Registra un motivo de renuncia usando el SP spAFI_CR_Motivos_Registra.
        /// </summary>
        public ErrorDto AF_CR_Seguimiento_Motivos_Registrar(int CodEmpresa, AfCrSeguimientoMotivosRegistrar motivos)
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

                connection.Execute(
                    "spAFI_CR_Motivos_Registra",
                    motivos,
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
        /// Cambia el estado de una renuncia usando el SP spAFI_Renuncia_CambioEstado.
        /// </summary>
        public ErrorDto AF_CR_Seguimiento_Renuncia_Estado(int CodEmpresa, AfCrSeguimientoRenunciaEstado estado)
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

                connection.Execute(
                    "spAFI_Renuncia_CambioEstado",
                    estado,
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
    }
}
