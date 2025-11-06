using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;


namespace PgxAPI.DataBaseTier
{
    public class FrmCcCuentasBancariasDb
    {
        private readonly IConfiguration _config;

        public FrmCcCuentasBancariasDb(IConfiguration config)
        {
            _config = config;
        }

        #region CC


        /// <summary>
        /// Obtiene la lista de Bancos 
        /// </summary>
        /// <returns></returns>
        public List<BancosCC> BancosCC_Obtener(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<BancosCC> info = new List<BancosCC>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT rtrim(Tg.COD_GRUPO) AS COD_GRUPO , rtrim(Tg.DESCRIPCION)  AS DESCRIPCION
                                FROM Tes_Bancos B 
                                INNER JOIN tes_banco_docs D ON B.id_banco = D.id_banco AND D.tipo = 'TE' 
                                INNER JOIN TES_BANCOS_GRUPOS Tg ON B.cod_Grupo = Tg.cod_grupo 
                                WHERE Tg.Activo = 1 GROUP BY Tg.COD_GRUPO, Tg.DESCRIPCION 
                                ORDER BY Tg.COD_GRUPO";

                    info = connection.Query<BancosCC>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        /// <summary>
        /// Datos para validar la cuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Grupo"></param>
        /// <returns></returns>
        public ValidacionCC ValidacionCC_Obtener(int CodEmpresa, string Cod_Grupo)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ValidacionCC? info = null;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT  LCTA_Interna, LCTA_InterBancaria
                                FROM tes_Bancos_Grupos  
                                WHERE cod_grupo = '{Cod_Grupo}'";

                    info = connection.Query<ValidacionCC>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info ?? new ValidacionCC();
        }


        /// <summary>
        /// Valida si ya existe la cuenta para la cédula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public int ValidaCC_Existe(int CodEmpresa, string cuenta, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            int result = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT count(*) from SYS_CUENTAS_BANCARIAS where Identificacion = '{cedula}'
                                and CUENTA_INTERNA = '{cuenta}'";

                    result = connection.Query<int>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de cuentas bancarias de la cédula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="modulo"></param>
        /// <returns></returns>
        public List<SysCuentasBancariasDto> CuentasBancarias_Obtener(int CodEmpresa, string cedula, string? modulo)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<SysCuentasBancariasDto> info = new List<SysCuentasBancariasDto>();
            var query = "";

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (modulo == null || modulo == "null")
                    {
                        query = $@"SELECT rtrim(C.cod_Banco) as cod_banco, rtrim(B.Descripcion) as DESCRIPCION, CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS 'Tipo_Desc'
                                ,C.tipo,C.cod_Divisa,C.CUENTA_INTERNA,C.DESTINO, C.CUENTA_INTERBANCA, C.ACTIVA, C.REGISTRO_FECHA , C.REGISTRO_USUARIO, C.identificacion
                                , C.Modulo, isnull(C.CUENTA_DEFAULT,0) AS 'CUENTA_DEFAULT', (C.identificacion + '-' + C.CUENTA_INTERNA + '-' + C.cod_banco) AS DataKey
                                 FROM SYS_CUENTAS_BANCARIAS C 
                                 INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
                                 WHERE C.Identificacion = '{cedula}'";
                    }
                    else
                    {
                        query = $@"SELECT rtrim(C.cod_Banco) as cod_banco, rtrim(B.Descripcion) as DESCRIPCION, CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS 'Tipo_Desc'
                                , C.tipo, C.cod_Divisa,C.CUENTA_INTERNA,C.DESTINO, C.CUENTA_INTERBANCA, C.ACTIVA, C.REGISTRO_FECHA , C.REGISTRO_USUARIO, C.identificacion
                                , C.Modulo, isnull(C.CUENTA_DEFAULT,0) AS 'CUENTA_DEFAULT', (C.identificacion + '-' + C.CUENTA_INTERNA + '-' + C.cod_banco) AS DataKey
                                 FROM SYS_CUENTAS_BANCARIAS C 
                                 INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
                                 WHERE C.Identificacion = '{cedula}' AND C.Modulo = '{modulo}'";
                    }


                    info = connection.Query<SysCuentasBancariasDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        /// <summary>
        /// Actualiza la Cuenta Bancaria 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CuentaBancaria_Actualizar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE SYS_CUENTAS_BANCARIAS SET 
                                cod_banco = '{data.Cod_Banco}',
                                Tipo = '{data.Tipo}',
                                cod_Divisa = '{data.Cod_Divisa}',
                                DESTINO = '{data.Destino}',
                                CUENTA_INTERNA = '{data.Cuenta_Interna}',
                                CUENTA_INTERBANCA = {Convert.ToInt32(data.Cuenta_Interbanca)},
                                CUENTA_DEFAULT = {Convert.ToInt32(data.Cuenta_Default)},
                                ACTIVA = {Convert.ToInt32(data.Activa)},
                                Modulo = '{data.Modulo}',
                                registro_usuario = '{data.Registro_Usuario}',
                                registro_fecha = '{data.Registro_Fecha}'
                                WHERE CUENTA_INTERNA = '{data.Cuenta_Interna}'
                                AND Identificacion = '{data.Identificacion}'";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Crea la cuenta bancaria
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CuentaBancaria_Insertar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"insert SYS_CUENTAS_BANCARIAS(Identificacion,cod_banco,tipo,cod_divisa, modulo
                                ,DESTINO,CUENTA_INTERNA,CUENTA_INTERBANCA, CUENTA_DEFAULT, ACTIVA, REGISTRO_USUARIO,REGISTRO_FECHA) 
                                values('{data.Identificacion}','{data.Cod_Banco}','{data.Tipo}','{data.Cod_Divisa}','{data.Modulo}','{data.Destino}'
                                ,'{data.Cuenta_Interna}',{Convert.ToInt32(data.Cuenta_Interbanca)},{Convert.ToInt32(data.Cuenta_Default)},{Convert.ToInt32(data.Activa)}
                                ,'{data.Registro_Usuario}',Getdate())"
                            ;

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Borra la cuenta del cliente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CuentaBancaria_Borrar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //valido si la  cuanta ya esta en una transaccion?
                    var valida = $@"select count('X') from Tes_Transacciones where CTA_AHORROS = '{data.Cuenta_Interna.Replace("-", "").Trim()}'";
                   var existe = connection.Query<int>(valida).FirstOrDefault(); 

                    if(existe > 0)
                    {
                        resp.Code = -1;
                        resp.Description = "No se puede eliminar la cuenta, ya que está asociada a una transacción.";
                        return resp;
                    }


                    var query = $@"delete SYS_CUENTAS_BANCARIAS where Identificacion = '{data.Identificacion.Trim()}'
                                and cod_Banco = '{data.Cod_Banco.Trim()}'
                                and cuenta_Interna = '{data.Cuenta_Interna.Trim()}'";


                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        
        #endregion



    }
}
