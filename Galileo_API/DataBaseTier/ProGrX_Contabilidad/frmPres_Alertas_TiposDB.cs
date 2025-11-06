using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_Alertas_TiposDB
    {
        private readonly IConfiguration _config;

        public frmPres_Alertas_TiposDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene la lista lazy de tipos de alerta 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<AlertasTiposLista> AlertasTipos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<AlertasTiposLista>();
            response.Result = new AlertasTiposLista();
            response.Result.total = 0;

            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);

                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM PRES_TIPOS_DESVIACIONES";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE COD_DESVIACION LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT cod_desviacion,descripcion, activa, requiere_justificacion,tipo,valor_desviacion,registro_usuario,registro_fecha,modifica_fecha,modifica_usuario
                                        FROM PRES_TIPOS_DESVIACIONES
                                          {filtro} 
                                         ORDER BY COD_DESVIACION
                                         {paginaActual}
                                         {paginacionActual} ";

                    response.Result.lista = connection.Query<AlertasTiposDto>(query).ToList();

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



        /// <summary>
        /// Guardo el registro del tipo de Alerta
        /// </summary>
        /// /// <param name="CodCliente"></param>
        /// <param name="alertatipo"></param>
        /// <returns></returns>
        public ErrorDto AlertasTipos_Insertar(int CodCliente, AlertasTiposDto alertatipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"INSERT INTO PRES_TIPOS_DESVIACIONES
                                ([COD_DESVIACION]
                                ,[DESCRIPCION]
                                ,[ACTIVA]
                                ,[REQUIERE_JUSTIFICACION]
                                ,[TIPO]
                                ,[VALOR_DESVIACION]
                                ,[REGISTRO_USUARIO]
                                ,[REGISTRO_FECHA]
                                ,[MODIFICA_USUARIO]
                                ,[MODIFICA_FECHA])
                          VALUES
                                (@CodDesviacion
                                ,@Descripcion
                                ,@Activa
                                ,@RequiereJustificacion
                                ,@Tipo
                                ,@ValorDesviacion
                                ,@RegistroUsuario
                                ,GETDATE()
                                ,@ModificaUsuario
                                ,@ModificaFecha)";

                    // Use parameters to prevent SQL injection and ensure proper data handling
                    var parameters = new
                    {
                        CodDesviacion = alertatipo.cod_desviacion,
                        Descripcion = alertatipo.descripcion,
                        Activa = alertatipo.activa,
                        RequiereJustificacion = alertatipo.requiere_justificacion,
                        Tipo = alertatipo.tipo,
                        ValorDesviacion = alertatipo.valor_desviacion,
                        RegistroUsuario = alertatipo.registro_usuario,
                        ModificaUsuario = alertatipo.modifica_usuario,
                        ModificaFecha = alertatipo.modifica_fecha
                    };

                    resp.Code = connection.Execute(query, parameters);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "AlertasTipos_Guardar: " + ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Actualiza el tipo de alerta
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="alertatipo"></param>
        /// <returns></returns>
        public ErrorDto AlertasTipos_Actualizar(int CodCliente, AlertasTiposDto alertatipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE PRES_TIPOS_DESVIACIONES
                           SET DESCRIPCION = @Descripcion,
                               ACTIVA = @Activa,
                               REQUIERE_JUSTIFICACION = @RequiereJustificacion,
                               TIPO = @Tipo,
                               VALOR_DESVIACION = @ValorDesviacion,
                               MODIFICA_USUARIO = @ModificaUsuario,
                               MODIFICA_FECHA = @ModificaFecha
                           WHERE COD_DESVIACION = @CodDesviacion";

                    // Use parameters to prevent SQL injection and ensure proper data handling
                    var parameters = new
                    {
                        CodDesviacion = alertatipo.cod_desviacion,
                        Descripcion = alertatipo.descripcion,
                        Activa = alertatipo.activa,
                        RequiereJustificacion = alertatipo.requiere_justificacion,
                        Tipo = alertatipo.tipo,
                        ValorDesviacion = alertatipo.valor_desviacion,
                        ModificaUsuario = alertatipo.modifica_usuario,
                        ModificaFecha = DateTime.Now
                    };

                    resp.Code = connection.Execute(query, parameters);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "AlertasTipos_Actualizar: " + ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Eliminar tipo de alerta
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="codDesviacion"></param>
        /// <returns></returns>
        public ErrorDto AlertasTipos_Eliminar(int CodCliente, string codDesviacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"DELETE FROM PRES_TIPOS_DESVIACIONES
                           WHERE COD_DESVIACION = @CodDesviacion";

                    // Use a parameter to safely pass the codDesviacion value
                    var parameters = new
                    {
                        CodDesviacion = codDesviacion
                    };

                    resp.Code = connection.Execute(query, parameters);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "AlertasTipos_Eliminar: " + ex.Message;
            }
            return resp;
        }


    }
}