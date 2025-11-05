using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmFSL_ExpedienteApelacionesDB
    {
        private readonly IConfiguration _config;

        public frmFSL_ExpedienteApelacionesDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<fslTipoApelacion>> FslTipoApelacion_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<fslTipoApelacion>>();


            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_apelacion as item, rtrim(cod_apelacion) + ' - ' + DESCRIPCION as descripcion from FSL_TIPOS_APELACIONES WHERE ACTIVA = 1";

                    response.Result = connection.Query<fslTipoApelacion>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslTipoApelacion_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        public ErrorDto FslApelacion_Aplicar(int CodCliente, fslApleacionAplicar expediente)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            int mLinea = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Linea apelacion pendiente
                    var query = $@"select isnull( max(Linea),0) as 'Linea'
		                                from FSL_EXPEDIENTES_APELACIONES
		                                Where cod_Expediente = {expediente.cod_expediente} and resolucion = 'P'";

                    mLinea = connection.Query<int>(query).FirstOrDefault();

                    if (mLinea > 0)
                    {
                        info.Code = -1;
                        info.Description = "Ya se encuentra registrada una apelación (Pendiente de Resolución) a este expediente, verifique!";
                    }

                    var procedure = "[spFSL_ApelacionRegistra]";
                    var values = new
                    {
                        Expediente = expediente.cod_expediente,
                        tipo = expediente.cod_apelacion,
                        PresentaCedula = expediente.presentaCedula,
                        PresentaNombre = expediente.presentaNombre,
                        PresentaNotas = expediente.presentaNotas,
                        Usuario = expediente.usuario,
                    };

                    int res = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if (res == 0)
                    {
                        info.Code = -1;
                        info.Description = "No fue posible aplicar la operación";
                    }


                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        public ErrorDto FslResolucionApelacion_Aplicar(int CodCliente, string apelacion)
        {
            fslResolucionApleacion expediente = JsonConvert.DeserializeObject<fslResolucionApleacion>(apelacion) ?? new fslResolucionApleacion();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            int mLinea = 0;
            try
            {
                string query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select NUMERO_RESOLUTORES from FSL_Comites
		                            where cod_Comite = '{expediente.cod_comite}'";

                    int vNumResolutores = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select isnull( max(Linea),0) as 'Linea'  from FSL_EXPEDIENTES_APELACIONES
                                     Where cod_Expediente = {expediente.cod_expediente}  and resolucion = 'P' ";

                    mLinea = connection.Query<int>(query).FirstOrDefault();

                    if (expediente.miembros.Count < vNumResolutores)
                    {
                        info.Code = -1;
                        info.Description = $"Debe de indicar al menos ({vNumResolutores}) miembros del comité VALIDADOS! que den la resolución!";
                        return info;
                    }

                    query = $@"update FSL_EXPEDIENTES set RESOLUCION_ESTADO = '{expediente.estado}',
                                                         ESTADO = '{expediente.resolucion_estado}' where COD_EXPEDIENTE = {expediente.cod_expediente} ";

                    var result = connection.Execute(query);


                    query = $@"update FSL_EXPEDIENTES_APELACIONES set RESOLUCION_NOTAS = '{expediente.resolucion_notas}',
                                    RESOLUCION = '{expediente.cod_resolucion}',
                                    RESOLUCION_FECHA = getdate(), 
                                    RESOLUCION_USUARIO = '{expediente.resolucion_usuario}' 
                                    where COD_EXPEDIENTE = {expediente.cod_expediente} and Linea = {mLinea} ";
                    result = connection.Execute(query);


                    query = $@"delete FSL_EXPEDIENTE_COMITE WHERE COD_EXPEDIENTE = {expediente.cod_expediente} ";
                    result = connection.Execute(query);

                    foreach (var item in expediente.miembros)
                    {
                        query = $@"INSERT FSL_EXPEDIENTES_APELACIONES_COMITE(LINEA,COD_EXPEDIENTE,COD_COMITE,CEDULA,ASIGNA_FECHA,ASIGNA_USUARIO)
                                    values({mLinea},{expediente.cod_expediente},'{expediente.cod_comite}','{item.cedula}',getdate(),'{expediente.resolucion_usuario}')";

                        result = connection.Execute(query);

                    }

                    info.Description = "Expediente actualizado satisfactoriamente...";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

    }
}