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
        /// Método para obtener las contabilidades disponibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Parametros_Contabilidad_Obtener(int codEmpresa)
        {
            const string sql = @"
            SELECT 
                RTRIM(cod_Contabilidad) AS item,
                RTRIM(nombre)           AS descripcion
            FROM CntX_Contabilidades";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql
            );
        }


        /// <summary>
        /// Método para establecer el mes/año de inicio del módulo de activos fijos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="periodo"></param>
        /// <returns></returns>
        public ErrorDto Activos_Parametros_EstablecerMes(int codEmpresa, DateTime periodo)
        {

            // 1. Verificar si existen parámetros
            const string sqlExiste = @"SELECT COUNT(1) FROM Activos_parametros";

            var existeResult = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                codEmpresa,
                sqlExiste,
                defaultValue: 0
            );

            // Si hubo error en la consulta de existencia, lo devolvemos
            if (existeResult.Code != 0)
            {
                return new ErrorDto
                {
                    Code = existeResult.Code,
                    Description = existeResult.Description
                };
            }

            if (existeResult.Result <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "No se han guardado los parámetros, debe guardarlos primero y luego establecer el inicio del módulo."
                };
            }

            // 2. Actualizar mes/año de inicio en Activos_parametros
            const string sqlUpdate = @"
        UPDATE Activos_parametros
        SET inicio_anio = @anno,
            inicio_mes  = @mes";

            var updateResult = DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                sqlUpdate,
                new { anno = periodo.Year, mes = periodo.Month }
            );

            // Si falla el update, devolvemos el error
            if (updateResult.Code != 0)
            {
                return updateResult;
            }

            // 3. Insertar el período anterior en Activos_periodos
            DateTime vFecha = periodo.AddMonths(-1);

            const string sqlInsert = @"
        INSERT INTO Activos_periodos (anio, mes, estado, asientos, traslado)
        VALUES (@anno, @mes, 'C', 'G', 'G');";

            var insertResult = DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                sqlInsert,
                new { anno = vFecha.Year, mes = vFecha.Month }
            );

            // Si el insert sale bien, insertResult vendrá con Code = 0 y Description = "Ok"
            return insertResult;
        }


        /// <summary>
        /// Método para consultar los parámetros generales de activos fijos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosParametrosData?> Activos_Parametros_Consultar(int CodEmpresa)
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

            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                sql,
                new ActivosParametrosData());
        }


        /// <summary>
        /// Método para guardar los parámetros generales de activos fijos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto Activos_Parametros_Guardar(int CodEmpresa, ActivosParametrosData datos)
        {
            var result = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlExiste = @"SELECT COALESCE(COUNT(*),0) FROM Activos_parametros";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste);

                result = existe > 0
                    ? Activos_Parametros_Actualizar(CodEmpresa, datos)
                    : Activos_Parametros_Insertar(CodEmpresa, datos);
            }
            catch (Exception ex)
            {
                result.Code = -1;
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
        private ErrorDto Activos_Parametros_Actualizar(int CodEmpresa, ActivosParametrosData datos)
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

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new
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
        private ErrorDto Activos_Parametros_Insertar(int CodEmpresa, ActivosParametrosData datos)
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

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new
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