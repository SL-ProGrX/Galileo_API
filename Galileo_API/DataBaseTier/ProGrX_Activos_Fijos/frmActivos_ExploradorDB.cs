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


        public ErrorDto<List<DropDownListaGenericaModel>> Departamentos(int codEmpresa)
        {
            string connString =
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
                SELECT 
                    cod_departamento AS item,
                    descripcion
                FROM Activos_Departamentos
                ORDER BY cod_departamento";

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


        public ErrorDto<List<DropDownListaGenericaModel>> Secciones(
            int codEmpresa,
            string codDepartamento)
        {
            string connString =
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
                SELECT 
                    cod_seccion AS item,
                    descripcion
                FROM Activos_Secciones
                WHERE cod_departamento = @codDepartamento
                ORDER BY cod_seccion";

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(
                        sql,
                        new { codDepartamento }
                    )
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


        public ErrorDto<List<ActivoExploradorDto>> Listar(
            int codEmpresa,
            ActivosExploradorFiltrosDto f)
        {
            string connString =
                _portalDB.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<ActivoExploradorDto>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<ActivoExploradorDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

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

                response.Result = cn
                    .Query<ActivoExploradorDto>(
                        sql.ToString(),
                        param
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<ActivoExploradorDto>();
            }

            return response;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposActivo(int codEmpresa)
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
                TIPO_ACTIVO AS item,
                descripcion
            FROM Activos_TIPO_ACTIVO
            ORDER BY TIPO_ACTIVO";

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

        public ErrorDto<List<DropDownListaGenericaModel>> Justificaciones(int codEmpresa)
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
                COD_JUSTIFICACION AS item,
                descripcion
            FROM Activos_JUSTIFICACIONES
            ORDER BY COD_JUSTIFICACION";

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


    }



}