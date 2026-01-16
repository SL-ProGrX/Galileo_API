using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Microsoft.Data.SqlClient;
using System.Text;


namespace Galileo_API.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosExploradorDB
    {
        private readonly PortalDB _portalDB;

        public FrmActivosExploradorDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        private ErrorDto<List<T>> EjecutarLista<T>(
            int codEmpresa,
            Func<SqlConnection, List<T>> query)
        {
            string connString =
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<T>>
            {
                Code = 0,
                Result = new List<T>()
            };

            try
            {
                using var cn = new SqlConnection(connString);
                response.Result = query(cn);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<T>();
            }

            return response;
        }

        /// <summary>
        /// Obtiene los departamentos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Departamentos(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn =>
                    cn.Query<DropDownListaGenericaModel>(@"
                        SELECT 
                            cod_departamento AS item,
                            descripcion
                        FROM Activos_Departamentos
                        ORDER BY cod_departamento
                    ").ToList()
            );
        }

       /// <summary>
       /// Obtiene las secciones
       /// </summary>
       /// <param name="codEmpresa"></param>
       /// <param name="codDepartamento"></param>
       /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Secciones(
            int codEmpresa,
            string codDepartamento)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn =>
                    cn.Query<DropDownListaGenericaModel>(@"
                        SELECT 
                            cod_seccion AS item,
                            descripcion
                        FROM Activos_Secciones
                        WHERE cod_departamento = @codDepartamento
                        ORDER BY cod_seccion
                    ", new { codDepartamento }).ToList()
            );
        }

        /// <summary>
        /// Obtiene los tipos activos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposActivo(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn =>
                    cn.Query<DropDownListaGenericaModel>(@"
                        SELECT 
                            TIPO_ACTIVO AS item,
                            descripcion
                        FROM Activos_TIPO_ACTIVO
                        ORDER BY TIPO_ACTIVO
                    ").ToList()
            );
        }

    /// <summary>
    /// Obtiene las justificaciones
    /// </summary>
    /// <param name="codEmpresa"></param>
    /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Justificaciones(int codEmpresa)
        {
            return EjecutarLista<DropDownListaGenericaModel>(
                codEmpresa,
                cn =>
                    cn.Query<DropDownListaGenericaModel>(@"
                        SELECT 
                            COD_JUSTIFICACION AS item,
                            descripcion
                        FROM Activos_JUSTIFICACIONES
                        ORDER BY COD_JUSTIFICACION
                    ").ToList()
            );
        }

      
        /// <summary>
        /// Obtiene la lista de resultados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivoExploradorDto>> Listar(
            int codEmpresa,
            ActivosExploradorFiltrosDto f)
        {
            return EjecutarLista<ActivoExploradorDto>(
                codEmpresa,
                cn =>
                {
                    var sql = new StringBuilder(@"
                        SELECT *
                        FROM vActivos_General
                        WHERE 1 = 1 ");

                    var param = new DynamicParameters();

                    if (!string.IsNullOrEmpty(f.nombre))
                    {
                        sql.Append(" AND Nombre LIKE @nombre");
                        param.Add("nombre", $"%{f.nombre}%");
                    }

                    if (!string.IsNullOrEmpty(f.descripcion))
                    {
                        sql.Append(" AND Descripcion LIKE @descripcion");
                        param.Add("descripcion", $"%{f.descripcion}%");
                    }

                    if (!string.IsNullOrEmpty(f.tipoActivo))
                    {
                        sql.Append(" AND Tipo_Activo = @tipoActivo");
                        param.Add("tipoActivo", f.tipoActivo);
                    }

                    if (!string.IsNullOrEmpty(f.departamento))
                    {
                        sql.Append(" AND cod_Departamento = @departamento");
                        param.Add("departamento", f.departamento);
                    }

                    if (!string.IsNullOrEmpty(f.seccion))
                    {
                        sql.Append(" AND cod_Seccion = @seccion");
                        param.Add("seccion", f.seccion);
                    }

                    sql.Append(" ORDER BY Num_Placa");

                    return cn
                        .Query<ActivoExploradorDto>(
                            sql.ToString(),
                            param
                        )
                        .ToList();
                }
            );
        }

        /// <summary>
        /// Obtiene las ubicaciones
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Ubicaciones(int codEmpresa)
        {
            string connString =
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
            SELECT
                RTRIM(COD_LOCALIZA) AS item,
                RTRIM(descripcion) AS descripcion
            FROM ACTIVOS_LOCALIZACIONES
            WHERE Activa = 1
            ORDER BY descripcion";

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }


        /// <summary>
        /// Obtiene la fecha del servidor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            string connString =_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operaci�n realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(connString);
                string sql = "SELECT dbo.MyGetdate() AS Fecha";

                var fechaServidor = connection.QuerySingle<DateTime>(sql);

                response.Description = fechaServidor.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene los responsables
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Responsables(int codEmpresa)
        {
            string connString =
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
                        SELECT
                            RTRIM(Identificacion) AS item,
                            RTRIM(Nombre) AS descripcion
                        FROM Activos_Personas
                        ORDER BY Nombre";

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }

       

        /// <summary>
        /// Obtiene los proveedores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Proveedores(int codEmpresa)
        {
            string connString =
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
            SELECT
                RTRIM(cod_proveedor) AS item,
                RTRIM(descripcion) AS descripcion
            FROM Activos_proveedores
            ORDER BY descripcion";

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }

        /// <summary>
        /// Obtiene los periodos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto<List<PeriodoExploradorDto>> Periodos(int codEmpresa, string estado)
        {
            var response = new ErrorDto<List<PeriodoExploradorDto>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                string sql = @"SELECT
                        anio,
                        mes,
                        EOMONTH(DATEFROMPARTS(anio, mes, 1)) AS fecha_periodo,
                        UPPER(
                            DATENAME(MONTH, DATEFROMPARTS(anio, mes, 1))
                            + ' DE '
                            + CAST(anio AS VARCHAR)
                        ) AS periodo
                    FROM Activos_Periodos
                    WHERE estado = @estado
                    ORDER BY anio DESC, mes DESC";

                response.Result = cn.Query<PeriodoExploradorDto>(sql, new { estado }).ToList();
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
