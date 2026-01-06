using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndReportesGeneralesDb
    {
        private readonly PortalDB _portalDB;

        public FrmFndReportesGeneralesDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener el catálogo de reportes generales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_ReportesGenerales_Catalogo_Obtener(int CodEmpresa, int Index)
        {
            string query = Index switch
            {
                // 0 - Oficinas
                0 => "select rtrim(cod_Oficina) as item, rtrim(descripcion) as descripcion from SIF_Oficinas order by descripcion",

                // 1 - Estados Persona
                1 => "select rtrim(cod_estado) as item, rtrim(descripcion) as descripcion from afi_estados_persona order by descripcion",

                // 2 - Divisas
                2 => "select COD_DIVISA AS item, DESCRIPCION as descripcion From vSys_Divisas",

                // 3 - Documentos
                3 => @"select rtrim(Tipo_Documento) as item, rtrim(Descripcion) as descripcion 
                        from sif_documentos Where Tipo_Documento in('FLIQ','FND','FNC','FRND','PLA','RE','NC','ND','SINPE','TD','PGSP')",

                // 4 - Conceptos
                4 => "select rtrim(cod_Concepto) as item, rtrim(Descripcion) as descripcion from sif_conceptos Where cod_Concepto like 'FND%'",

                // 5 - Operadoras
                5 => "select descripcion, cod_operadora as item from FND_Operadoras",

                // 6 - Intituciones
                6 => "select cod_institucion as item, rtrim(descripcion) as descripcion from instituciones where Activa = 1 order by descripcion",

                // 7 - U.Programatica
                7 => "select Codigo as item, Descripcion from UPROGRAMATICA",

                // Default → retorna vacío
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                var response = new ErrorDto<List<DropDownListaGenericaModel>>();
                response.Code = -1;
                response.Description = "Opción inválida.";
                response.Result = null;
                return response;
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtener lista de planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="CodPlan"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_ReportesGenerales_Planes_Obtener(int CodEmpresa, int CodOperadora, string? CodPlan, string? Usuario)
        {
            string sql = @"select cod_plan as item, descripcion from fnd_planes where cod_operadora = @CodOperadora";

            if (!string.IsNullOrEmpty(CodPlan))
            {
                sql += " and Cod_Plan = @CodPlan ";
            }
            if (!string.IsNullOrEmpty(Usuario))
            {
                sql += " and dbo.fxFnd_Seguridad_Acceso_Planes(@Usuario, cod_operadora, cod_plan) = 1";
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Navegación por scroll de planes
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="CodPlan"></param>
        /// <param name="scrollCode"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel> Fnd_ReportesGenerales_Plan_Scroll_Obtener(int CodEmpresa, int CodOperadora, string? CodPlan, int scrollCode)
        {
            var response = new ErrorDto<DropDownListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new DropDownListaGenericaModel()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                string query = @"SELECT TOP 1 cod_plan as item, descripcion FROM fnd_planes WHERE cod_operadora = @CodOperadora ";

                if (!string.IsNullOrEmpty(CodPlan) && CodPlan != "-")
                {
                    if (scrollCode == 1)
                    {
                        query += " AND cod_plan > @CodPlanActual ORDER BY cod_plan ASC";
                    }
                    else
                    {
                        query += " AND cod_plan < @CodPlanActual ORDER BY cod_plan DESC";
                    }
                }
                else
                {
                    query += " ORDER BY cod_plan ASC";
                }

                response.Result = connection.QueryFirstOrDefault<DropDownListaGenericaModel>(query, new
                {
                    CodOperadora,
                    CodPlanActual = CodPlan
                });
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
        /// Aplicar proceso de cubo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto Fnd_ReportesGenerales_CuboAplicar(int CodEmpresa, FndReportesGeneralesCuboFiltros filtros)
        {
            DateTime fechaInicioDt = (filtros.fecha_inicio ?? DateTime.Today).Date;
            DateTime fechaCorteDt = (filtros.fecha_corte ?? DateTime.Today).Date.AddDays(1).AddTicks(-1); 

            if (filtros.chk_todos)
            {
                fechaInicioDt = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
                fechaCorteDt = DateTime.Today;
            }

            var fechaInicioStr = fechaInicioDt.ToString("yyyy/MM/dd");
            var fechaCorteStr = fechaCorteDt.ToString("yyyy/MM/dd");

            const string query = "exec spFndMovAnalisisCubo @fechaInicioStr, @fechaCorteStr";
            return DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                new { fechaInicioStr, fechaCorteStr }
            );
        }

    }
}
