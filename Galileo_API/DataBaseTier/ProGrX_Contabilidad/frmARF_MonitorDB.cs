using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Activos;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmArfMonitorDb
    {
        private readonly PortalDB _portalDB;

        public FrmArfMonitorDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Busca en el monitor de arrendamientos financieros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<ArfMonitorTablaDto>> Buscar(int codEmpresa,ArfMonitorFiltroDto filtros)
        {
            var response = new ErrorDto<List<ArfMonitorTablaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = new StringBuilder();
                var where = new StringBuilder();

                ConstruirSelect(sql, filtros);
                ConstruirFiltros(where, filtros);

                if (where.Length > 0)
                {
                    sql.Append(" WHERE ").Append(where);
                }

                response.Result = cn.Query<ArfMonitorTablaDto>(
                    sql.ToString(),
                    ObtenerParametros(filtros)
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        private static void ConstruirSelect(StringBuilder sql, ArfMonitorFiltroDto filtros)
        {
            if (filtros.tipo_fecha == "Cierre")
            {
                sql.Append("SELECT * FROM vARF_Cierre_Operacion_Consulta ");
            }
            else
            {
                sql.Append("SELECT * FROM vARF_Operacion_Consulta ");
            }
        }


        private static void ConstruirFiltros(StringBuilder where, ArfMonitorFiltroDto filtros)
        {
            if (filtros.tipo_fecha == "Cierre")
            {
                where.Append(" CORTE = @corte ");
                return;
            }

            AgregarFiltroFecha(where, filtros);
            AgregarFiltro(where, "COD_LOCAL = @cod_unidad", filtros.cod_unidad);
            AgregarFiltro(where, "COD_ACREEDOR = @cod_arrendador", filtros.cod_arrendador);
        }

        private static void AgregarFiltroFecha(StringBuilder where, ArfMonitorFiltroDto filtros)
        {
            if (!filtros.fecha_inicio.HasValue || !filtros.fecha_corte.HasValue)
                return;


            if (filtros.usar_fechas == true)
                return;

            string campoFecha = ObtenerCampoFecha(filtros.tipo_fecha);

            where.Append($@"
                            {campoFecha} BETWEEN
                            @fechaInicio AND @fechaCorte
                        ");
        }

        private static string ObtenerCampoFecha(string tipoFecha)
        {
            return tipoFecha switch
            {
                "Registro" => "REGISTRO_FECHA",
                "Activación" => "ACTIVA_FECHA",
                "Inicio" => "FECHA_INICIO",
                "Finaliza" => "FECHA_FINALIZA",
                _ => "ACTIVA_FECHA"
            };
        }

        private static void AgregarFiltro(StringBuilder where,string condicion,string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return;

            if (where.Length > 0)
                where.Append(" AND ");

            where.Append(condicion);
        }


        private object ObtenerParametros(ArfMonitorFiltroDto filtros)
        {
            return new
            {
                filtros.cod_unidad,
                filtros.cod_arrendador,
                filtros.corte,
                fechaInicio = filtros.fecha_inicio?.Date,
                fechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddSeconds(-1)
            };
        }



        /// <summary>
        /// Busca unidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Buscar(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = @"
                    SELECT
                        RTRIM(COD_LOCAL) AS item,
                        descripcion
                    FROM ARF_UNIDADES
                    ORDER BY COD_LOCAL;
                ";

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Busca arrendadores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Buscar(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = @"
                    SELECT
                        RTRIM(COD_ACREEDOR) AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM ARF_ACREEDORES
                    ORDER BY COD_ACREEDOR;
                ";

                response.Result = cn
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();
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

