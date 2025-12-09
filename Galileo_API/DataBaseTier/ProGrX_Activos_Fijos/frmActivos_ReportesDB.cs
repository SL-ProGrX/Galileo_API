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
        /// Helpers genéricos para reducir duplicación
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initialResult"></param>
        /// <returns></returns>
        private static ErrorDto<T> CreateOkResponse<T>(T initialResult)
        {
            return new ErrorDto<T>
            {
                Code        = 0,
                Description = "Ok",
                Result      = initialResult
            };
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que retornan listas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto<List<T>> ExecuteListQuery<T>(
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = CreateOkResponse(new List<T>());

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                result.Result = connection.Query<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
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

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql);
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

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql, new { departamento });
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

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql);
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

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql);
        }


        /// <summary>
        /// Metodo para consultar el estado de un periodo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public ErrorDto<string> Activos_Reportes_PeriodoEstado(int CodEmpresa, DateTime fecha)
        {
            var result = CreateOkResponse(string.Empty);

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
            var result = CreateOkResponse(DateTime.Now);

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

            return ExecuteListQuery<ActivosReportesResponsableData>(CodEmpresa, sql);
        }
    }
}