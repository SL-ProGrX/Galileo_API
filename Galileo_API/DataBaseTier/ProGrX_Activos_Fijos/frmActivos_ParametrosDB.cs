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
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto Activos_Parametros_Guardar(int CodEmpresa, string usuario, ActivosParametrosData datos)
        {
            var result = DbHelper.CreateOkResponse();

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