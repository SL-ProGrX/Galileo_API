using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using System.Reflection;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_ImpresorasDB
    {
        private readonly IConfiguration? _config;
        private mSecurityMainDb DBBitacora;

        public frmTES_ImpresorasDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Actualiza impresoras 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="impresora"></param>
        /// <returns></returns>
        public ErrorDto Tes_Impresoras_Guardar(int CodEmpresa, string usuario, TES_ImpresorasDTO impresora)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Guardado correctamente"
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var sql = @"
                    UPDATE SYS_IMPRESORAS_TES SET 
                        COD_IMPRESORA_CHEQUES = @Cod_Impresora_Cheques,
                        COD_IMPRESORA_RECIBO = @Cod_Impresora_Recibo,
                        DESCRIPCION_CHEQUE = @Descripcion_Cheque,
                        DESCRIPCION_RECIBO = @Descripcion_Recibo,
                        REGISTRO_USUARIO = @Usuario,
                        REGISTRO_FECHA = GETDATE()";
                    connection.Execute(sql, new
                    {
                        Cod_Impresora_Cheques = impresora.cod_impresora_cheque,
                        Cod_Impresora_Recibo = impresora.cod_impresora_recibo,
                        Descripcion_Cheque = impresora.descripcion_cheque?.ToUpper().Trim(),
                        Descripcion_Recibo = impresora.descripcion_recibo?.ToUpper().Trim(),
                        Usuario = usuario,
                    });
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }



        /// <summary>
        /// Obtener impresoras
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<TES_ImpresorasDTO> Tes_Impresoras_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_ImpresorasDTO>
            {
                Code = 0,
                Description = "Ok",
                Result = new TES_ImpresorasDTO()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT COD_IMPRESORA_CHEQUES as cod_impresora_cheque, COD_IMPRESORA_RECIBO, DESCRIPCION_CHEQUE, DESCRIPCION_RECIBO FROM SYS_IMPRESORAS_TES";
                    response.Result = connection.QueryFirstOrDefault<TES_ImpresorasDTO>(query);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


    }
}