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
                Description = "Operación realizada correctamente",
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

        // =====================================================
        // DEPARTAMENTOS
        // =====================================================
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

        // =====================================================
        // SECCIONES
        // =====================================================
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

        // =====================================================
        // TIPOS DE ACTIVO
        // =====================================================
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

        // =====================================================
        // JUSTIFICACIONES
        // =====================================================
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

        // =====================================================
        // LISTAR ACTIVOS (EXPLORADOR)
        // =====================================================
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
    }
}
