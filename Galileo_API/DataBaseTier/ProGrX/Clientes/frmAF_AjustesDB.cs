using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_AjustesDB
    {
        private readonly IConfiguration _config;

        public frmAF_AjustesDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene las instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>

        public ErrorDTO<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"
                SELECT COD_INSTITUCION AS item, RTRIM(Descripcion) AS descripcion
                FROM INSTITUCIONES
                ORDER BY COD_INSTITUCION;";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtiene los tipos ID
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_TiposId_Obtener(int CodEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"
                SELECT TIPO_ID AS item, RTRIM(Descripcion) AS descripcion
                FROM AFI_TIPOS_IDS;";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtiene los estados de persona activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_EstadosPersona_ObtenerActivos(int CodEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string query = @"
                SELECT RTRIM(E.COD_ESTADO) AS item, RTRIM(E.DESCRIPCION) AS Descripcion
                FROM AFI_ESTADOS_PERSONA E
                WHERE E.ACTIVO = 1;";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Cambia la identificacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="nuevoTipoId"></param>
        /// <returns></returns>
        public ErrorDTO AF_Ajustes_CambiarIdentificacion(int CodEmpresa, string cedula, int nuevoTipoId)
        {
            var response = new ErrorDTO { Code = 0, Description = "Actualizado correctamente" };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string sql = @"
                UPDATE socios
                SET tipo_id = @TipoId
                WHERE cedula = @Cedula;";

                connection.Execute(sql, new { TipoId = nuevoTipoId, Cedula = cedula });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Cambia estado del ajuste
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="nuevoEstado"></param>
        /// <returns></returns>

        public ErrorDTO AF_Ajustes_CambiarEstado(int CodEmpresa, string cedula, string nuevoEstado)
        {
            var response = new ErrorDTO { Code = -1 };

            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                // 3.1 Validar que el estado est� autorizado en la instituci�n actual del socio
                const string sqlInstit = @"
                SELECT COUNT(*) 
                FROM AFI_ESTADOS_INSTITUCIONES
                WHERE cod_estado = @Estado
                  AND cod_institucion IN (SELECT cod_institucion FROM socios WHERE cedula = @Cedula);";

                int existe = connection.QueryFirstOrDefault<int>(sqlInstit, new { Estado = nuevoEstado, Cedula = cedula });
                if (existe == 0)
                {
                    response.Description = "El estado indicado no est� autorizado en la instituci�n del socio.";
                    return response;
                }

                // 3.2 Validar que no tenga aporte registrado > 0
                const string sqlAporte = @"
                SELECT TOP 1 ISNULL(aporte,0) 
                FROM Ahorro_consolidado
                WHERE cedula = @Cedula AND Aporte > 0;";

                decimal aporte = connection.QueryFirstOrDefault<decimal>(sqlAporte, new { Cedula = cedula });
                if (aporte > 0)
                {
                    response.Description = "No procede el cambio de estado porque la persona tiene Aporte registrado.";
                    return response;
                }

                // 3.3 Actualizar
                const string sqlUpd = @"
                UPDATE socios
                SET estadoActual = @Estado
                WHERE cedula = @Cedula;";

                connection.Execute(sqlUpd, new { Estado = nuevoEstado, Cedula = cedula });

                response.Code = 0;
                response.Description = "Estado actualizado correctamente";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="up"></param>
        /// <param name="ut"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public ErrorDTO AF_Ajustes_CambiarInstitucion_ASECCSS(
            int CodEmpresa,
            string cedula,
            int? codInstitucion,
            string? up,
            string? ut,
            string? ct
        )
        {
            var response = new ErrorDTO { Code = -1 };

            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                // Si se desea cambiar de instituci�n, validar que el estado actual del socio sea permitido en la nueva instituci�n
                if (codInstitucion.HasValue)
                {
                    const string sqlVal = @"
                    SELECT COUNT(*)
                    FROM AFI_ESTADOS_INSTITUCIONES
                    WHERE cod_institucion = @CodInst
                      AND cod_estado IN (SELECT estadoActual FROM socios WHERE cedula = @Cedula);";

                    int ok = connection.QueryFirstOrDefault<int>(sqlVal, new { CodInst = codInstitucion.Value, Cedula = cedula });
                    if (ok == 0)
                    {
                        response.Description = "El estado actual del socio no est� autorizado en la instituci�n indicada.";
                        return response;
                    }
                }

                // Construcci�n din�mica del SET
                var sets = new List<string>();
                var param = new DynamicParameters();
                param.Add("@Cedula", cedula);

                if (codInstitucion.HasValue) { sets.Add("cod_institucion = @CodInst"); param.Add("@CodInst", codInstitucion.Value); }
                if (!string.IsNullOrWhiteSpace(up)) { sets.Add("UP = @UP"); param.Add("@UP", up!.Trim()); }
                if (!string.IsNullOrWhiteSpace(ut)) { sets.Add("UT = @UT"); param.Add("@UT", ut!.Trim()); }
                if (!string.IsNullOrWhiteSpace(ct)) { sets.Add("CT = @CT"); param.Add("@CT", ct!.Trim()); }

                if (sets.Count == 0)
                {
                    response.Code = 0;
                    response.Description = "No hay cambios para aplicar.";
                    return response;
                }

                string sql = $"UPDATE socios SET {string.Join(", ", sets)} WHERE cedula = @Cedula;";
                connection.Execute(sql, param);

                response.Code = 0;
                response.Description = "Actualizaci�n aplicada correctamente.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Ajuste en cambio de institucion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="codDepartamento"></param>
        /// <param name="codSeccion"></param>
        /// <returns></returns>

        public ErrorDTO AF_Ajustes_CambiarInstitucion(
            int CodEmpresa,
            string cedula,
            int codInstitucion,
            string codDepartamento,
            string codSeccion
        )
        {
            var response = new ErrorDTO { Code = -1 };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                const string sqlVal = @"
                SELECT COUNT(*)
                FROM AFI_ESTADOS_INSTITUCIONES
                WHERE cod_institucion = @CodInst
                  AND cod_estado IN (SELECT estadoActual FROM socios WHERE cedula = @Cedula);";

                int ok = connection.QueryFirstOrDefault<int>(sqlVal, new { CodInst = codInstitucion, Cedula = cedula });
                if (ok == 0)
                {
                    response.Description = "El estado actual del socio no est� autorizado en la instituci�n indicada.";
                    return response;
                }

                const string sqlUpd = @"
                UPDATE socios
                SET cod_institucion = @CodInst,
                    cod_departamento = @CodDept,
                    cod_seccion = @CodSec
                WHERE cedula = @Cedula;";

                connection.Execute(sqlUpd, new
                {
                    CodInst = codInstitucion,
                    CodDept = codDepartamento?.Trim(),
                    CodSec = codSeccion?.Trim(),
                    Cedula = cedula
                });

                response.Code = 0;
                response.Description = "Instituci�n y dependencias actualizadas correctamente.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Carga de datos ajustes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>

        public ErrorDTO<af_ajuste_persona_detalle> AF_Ajustes_CargarDatos(int CodEmpresa, string cedula)
        {
            var response = new ErrorDTO<af_ajuste_persona_detalle> { Code = 0 };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                string sql = @"
                            SELECT S.*,
                                   Est.Descripcion                          AS EstadoPersonaDesc,
                                   Est.Cod_Estado + ' - ' + Est.Descripcion AS EstadoPersona,
                                   I.descripcion                             AS DescInst,
                                   D.descripcion                             AS DescDept,
                                   X.descripcion                             AS DescSec,
                                   Tid.Descripcion                           AS TipoIdDesc
                            FROM socios S
                            INNER JOIN Instituciones I ON S.cod_institucion = I.cod_institucion
                            LEFT JOIN AFDepartamentos D ON S.cod_institucion = D.cod_institucion
                                                       AND S.cod_departamento = D.cod_departamento
                            LEFT JOIN AFSecciones X     ON S.cod_institucion = X.cod_institucion
                                                       AND S.cod_departamento = X.cod_departamento
                                                       AND S.cod_seccion = X.cod_seccion
                            INNER JOIN AFI_ESTADOS_PERSONA Est ON S.EstadoActual = Est.Cod_Estado
                            LEFT JOIN AFI_TIPOS_IDS Tid        ON S.tipo_id = Tid.tipo_id
                            WHERE cedula = @Cedula;";

                response.Result = connection.QueryFirstOrDefault<af_ajuste_persona_detalle>(sql, new { Cedula = cedula });

                if (response.Result == null)
                {
                    response.Code = 1;
                    response.Description = "No se encontraron datos para la c�dula indicada.";
                }
            }

            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDTO AF_Ajustes_Cambiar(int CodEmpresa, string ajuste, int codigo)
        {

            AF_Ajuste request = JsonConvert.DeserializeObject<AF_Ajuste>(ajuste) ?? new AF_Ajuste();

            switch (codigo)
            {
                case 1:
                    return AF_Ajustes_CambiarIdentificacion(CodEmpresa, request.cedula, request.nuevo_tipo_id);

                case 2:
                    return AF_Ajustes_CambiarEstado(CodEmpresa, request.cedula, request.nuevo_estado);

                case 3:
                    return AF_Ajustes_CambiarInstitucion(CodEmpresa, request.cedula, request.cod_institucion, request.cod_departamento, request.cod_seccion);

                default:
                    return new ErrorDTO { Code = -1, Description = "C�digo de ajuste no v�lido." };
            }

        }


        public ErrorDTO<AF_CatalogosGeneralesDTO> AF_Catalogos_Obtener(int CodEmpresa, string? cod_institucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<AF_CatalogosGeneralesDTO>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new AF_CatalogosGeneralesDTO()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var parameters = new DynamicParameters();
                if (string.IsNullOrWhiteSpace(cod_institucion) || cod_institucion == "undefined")
                    parameters.Add("@cod_institucion", null, DbType.String);
                else
                    parameters.Add("@cod_institucion", cod_institucion, DbType.String);

                using var multi = connection.QueryMultiple(
                    "spAF_Catalogos_Consulta",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                response.Result.EstadoCivil = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Divisas = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.TiposIdentificacion = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Profesiones = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Sectores = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Sociedades = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.ActividadesEconomicas = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Paises = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.EstadosPersonaIngreso = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Nacionalidades = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.NivelAcademico = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.EstadoLaboral = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.ActividadLaboral = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.RelacionParentesco = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Promotores = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Instituciones = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Deductoras = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Departamentos = multi.Read<DropDownListaGenericaModel>().ToList();
                response.Result.Secciones = multi.Read<DropDownListaGenericaModel>().ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }





    }



}
