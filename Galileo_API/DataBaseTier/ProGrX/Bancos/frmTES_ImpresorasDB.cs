using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier
{
    public class FrmTesImpresorasDb
    {
        private readonly PortalDB _portalDB;

        public FrmTesImpresorasDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Actualiza impresoras 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="impresora"></param>
        /// <returns></returns>
        public ErrorDto Tes_Impresoras_Guardar(int CodEmpresa, string usuario, TesImpresorasDto impresora)
        {
            try
            {
                var sql = @"
                    UPDATE SYS_IMPRESORAS_TES SET 
                        COD_IMPRESORA_CHEQUES = @Cod_Impresora_Cheques,
                        COD_IMPRESORA_RECIBO = @Cod_Impresora_Recibo,
                        DESCRIPCION_CHEQUE = @Descripcion_Cheque,
                        DESCRIPCION_RECIBO = @Descripcion_Recibo,
                        REGISTRO_USUARIO = @Usuario,
                        REGISTRO_FECHA = GETDATE()";

                var parametros = new
                {
                    Cod_Impresora_Cheques = impresora.cod_impresora_cheque,
                    Cod_Impresora_Recibo = impresora.cod_impresora_recibo,
                    Descripcion_Cheque = impresora.descripcion_cheque?.ToUpper().Trim(),
                    Descripcion_Recibo = impresora.descripcion_recibo?.ToUpper().Trim(),
                    Usuario = usuario,
                };

                DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, parametros);

                return DbHelper.OkResponse("Guardado correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        /// <summary>
        /// Obtener impresoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<TesImpresorasDto> Tes_Impresoras_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"SELECT COD_IMPRESORA_CHEQUES as cod_impresora_cheque, COD_IMPRESORA_RECIBO, DESCRIPCION_CHEQUE, DESCRIPCION_RECIBO FROM SYS_IMPRESORAS_TES";

                return conn.QueryFirstOrDefault<TesImpresorasDto>(query) ?? new TesImpresorasDto();
            });
        }


    }
}