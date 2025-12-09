using Galileo.Models.ERROR;
using Galileo.Models;
using Dapper;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosReportesDB
    {
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;

        public FrmActivosReportesDB(IConfiguration config)
        {
            _mActivos = new MActivosFijos(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Metodo para consultar listado de departamentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Departamentos_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(cod_departamento) AS item,
                       RTRIM(descripcion)      AS descripcion
                FROM   Activos_departamentos
                ORDER BY cod_departamento";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, sql);
        }


        /// <summary>
        /// Metodo para consultar listado de secciones por departamento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="departamento"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Secciones_Obtener(int CodEmpresa, string departamento)
        {
            const string sql = @"
                SELECT RTRIM(cod_Seccion) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM   Activos_Secciones
                WHERE  cod_departamento = @departamento
                ORDER BY cod_Seccion";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, sql, new { departamento });
        }


        /// <summary>
        /// Metodo para consultar listado de tipos de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_TipoActivo_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(tipo_activo) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM   Activos_tipo_activo
                ORDER BY tipo_activo";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, sql);
        }


        /// <summary>
        /// Metodo para consultar listado de localizaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Localizacion_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(COD_LOCALIZA) AS item,
                       RTRIM(descripcion)  AS descripcion
                FROM   ACTIVOS_LOCALIZACIONES
                WHERE  Activa = 1
                ORDER BY descripcion";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, sql);
        }


        /// <summary>
        /// Metodo para consultar el estado de un periodo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Reportes_PeriodoEstado(int CodEmpresa, DateTime fecha)
        {
            var result = DbHelper.CreateOkResponse(string.Empty);

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT Estado
                    FROM   Activos_Periodos
                    WHERE  Anio = @anno
                    AND    Mes  = @mes";

                var estado = connection
                    .Query<string>(sql, new { anno = fecha.Year, mes = fecha.Month })
                    .FirstOrDefault();

                if (estado is null)
                {
                    result.Result = "Periodo No Registrado!";
                }
                else
                {
                    result.Result = estado == "C" ? "CERRADO" : "PENDIENTE";
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Metodo para consultar el periodo actual de una contabilidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            var result = DbHelper.CreateOkResponse(DateTime.Now);

            try
            {
                result.Result = _mActivos.fxCntX_PeriodoActual(CodEmpresa, contabilidad);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Metodo para consultar listado de responsables
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosReportesResponsableData>> Activos_Reportes_Responsables_Consultart(int CodEmpresa)
        {
            const string sql = @"
                SELECT IDENTIFICACION,
                       NOMBRE,
                       Departamento,
                       Seccion
                FROM vActivos_Personas";

            return DbHelper.ExecuteListQuery<ActivosReportesResponsableData>(_portalDB, CodEmpresa, sql);
        }
    }
}