using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosParametrosDB
    {
        private readonly PortalDB _portalDB;

        public FrmActivosParametrosDB(IConfiguration config)
        {
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
        /// Helpers genéricos para reducir duplicación
        /// </summary>
        /// <returns></returns>
        private static ErrorDto CreateOkResponse()
        {
            return new ErrorDto
            {
                Code        = 0,
                Description = "Ok"
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
        /// Método genérico para ejecutar consultas que retornan un solo registro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="defaultValue"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto<T> ExecuteSingleQuery<T>(
            int codEmpresa,
            string sql,
            T defaultValue,
            object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                result.Result = connection.Query<T>(sql, parameters).FirstOrDefault()!;
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = defaultValue;
            }

            return result;
        }

        private ErrorDto ExecuteNonQuery(
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                connection.Execute(sql, parameters);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        // -----------------------------------------------------------------
        // Métodos públicos
        // -----------------------------------------------------------------

        /// <summary>
        /// Método para consultar lista de parámetros generales (contabilidades).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Parametros_Contabilidad_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(cod_Contabilidad) AS item,
                       RTRIM(nombre)           AS descripcion
                FROM   CntX_Contabilidades";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql);
        }

        /// <summary>
        /// Método para establecer el mes inicial del módulo de activos fijos.
        /// </summary>
        public ErrorDto Activos_Parametros_EstablecerMes(int CodEmpresa, DateTime periodo)
        {
            var result = CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlExiste = @"SELECT COALESCE(COUNT(*),0) FROM Activos_parametros";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste);

                if (existe <= 0)
                {
                    result.Code        = -2;
                    result.Description = "No se han guardado los parámetros, debe guardarlos primero y luego establecer el inicio del módulo.";
                    return result;
                }

                const string sqlUpdateInicio = @"
                    UPDATE Activos_parametros
                    SET    inicio_anio = @anno,
                           inicio_mes  = @mes";

                connection.Execute(sqlUpdateInicio, new
                {
                    anno = periodo.Year,
                    mes  = periodo.Month
                });

                // Periodo anterior se marca como cerrado
                var vFecha = periodo.AddMonths(-1);

                const string sqlInsertPeriodo = @"
                    INSERT INTO Activos_periodos (anio, mes, estado, asientos, traslado)
                    VALUES (@anno, @mes, 'C', 'G', 'G')";

                connection.Execute(sqlInsertPeriodo, new
                {
                    anno = vFecha.Year,
                    mes  = vFecha.Month
                });
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        
        
        /// <summary>
        /// Método para consultar los parámetros generales de activos fijos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosParametrosData> Activos_Parametros_Consultar(int CodEmpresa)
        {
            const string sql = @"
                SELECT cod_empresa,
                       Enlace_Conta,
                       Enlace_SIFC,
                       REGISTRO_PERIODO_CERRADO,
                       nombre_empresa,
                       forzar_TipoActivo,
                       registroCompras,
                       tipo_anio,
                       inicio_anio
                FROM   Activos_parametros";

            return ExecuteSingleQuery(
                CodEmpresa,
                sql,
                new ActivosParametrosData());
        }


        /// <summary>
        /// Método para guardar los parámetros generales de activos fijos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto Activos_Parametros_Guardar(int CodEmpresa, string usuario, ActivosParametrosData datos)
        {
            var result = CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlExiste = @"SELECT COALESCE(COUNT(*),0) FROM Activos_parametros";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste);

                result = existe > 0
                    ? Activos_Parametros_Actualizar(CodEmpresa, usuario, datos)
                    : Activos_Parametros_Insertar(CodEmpresa, usuario, datos);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Método para actualizar los parámetros generales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto Activos_Parametros_Actualizar(int CodEmpresa, string usuario, ActivosParametrosData datos)
        {
            const string sql = @"
                UPDATE Activos_parametros
                SET    cod_empresa               = @cod_empresa,
                       nombre_empresa           = @nombre_empresa,
                       enlace_conta             = @enlace_conta,
                       enlace_sifc              = @enlace_sifc,
                       tipo_anio                = @tipo_anio,
                       forzar_TipoActivo        = @forzar_tipoactivo,
                       registroCompras          = @registrocompras,
                       REGISTRO_PERIODO_CERRADO = @registro_periodo_cerrado";

            return ExecuteNonQuery(CodEmpresa, sql, new
            {
                datos.cod_empresa,
                datos.nombre_empresa,
                datos.enlace_conta,
                datos.enlace_sifc,
                datos.tipo_anio,
                datos.forzar_tipoactivo,
                datos.registrocompras,
                datos.registro_periodo_cerrado
            });
        }



        /// <summary>
        /// Método para insertar los parámetros generales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto Activos_Parametros_Insertar(int CodEmpresa, string usuario, ActivosParametrosData datos)
        {
            const string sql = @"
                INSERT INTO Activos_parametros
                    (cod_empresa,
                     nombre_empresa,
                     enlace_conta,
                     enlace_sifc,
                     Tipo_Anio,
                     forzar_TipoActivo,
                     RegistroCompras,
                     REGISTRO_PERIODO_CERRADO)
                VALUES
                    (@cod_empresa,
                     @nombre_empresa,
                     @enlace_conta,
                     @enlace_sifc,
                     @tipo_anio,
                     @forzar_tipoactivo,
                     @registrocompras,
                     @registro_periodo_cerrado)";

            return ExecuteNonQuery(CodEmpresa, sql, new
            {
                datos.cod_empresa,
                datos.nombre_empresa,
                datos.enlace_conta,
                datos.enlace_sifc,
                datos.tipo_anio,
                datos.forzar_tipoactivo,
                datos.registrocompras,
                datos.registro_periodo_cerrado
            });
        }
    }
}