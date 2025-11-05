using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvOrdenesAutorizacionDB
    {
        private readonly IConfiguration _config;

        public frmInvOrdenesAutorizacionDB(IConfiguration config)
        {
            _config = config;
        }



        public ErrorDto<List<ResolucionTransaccionDTO>> resolucionTransaccion_Obtener(int CodCliente, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ResolucionTransaccion_Filtros filtros = JsonConvert.DeserializeObject<ResolucionTransaccion_Filtros>(filtroString) ?? new ResolucionTransaccion_Filtros();
            var response = new ErrorDto<List<ResolucionTransaccionDTO>>();
            try
            {
                string where = "";
                if (filtros.fecha == "0")
                {
                    // Convertir la cadena ISO a DateTimeOffset
                    DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.fecha_inicio);
                    string fechainicio = fecha_inicio.ToString("yyyy-MM-dd");

                    DateTimeOffset fecha_corte = DateTimeOffset.Parse(filtros.fecha_corte);
                    string fechacorte = fecha_corte.ToString("yyyy-MM-dd");


                    where = $"WHERE R.Genera_Fecha BETWEEN '{fechainicio}' AND '{fechacorte}'";


                }

                if (filtros.fecha == "1")

                {
                    where = $"WHERE R.Genera_Fecha BETWEEN '1900-01-01 23:59:59' AND '2999-01-01 23:59:59'";
                }
                if (filtros.tipo == "R")
                {
                    using var connection = new SqlConnection(clienteConnString);
                    {

                        var query = $@"
	                                SELECT
                                    R.cod_requisicion AS Cod_Orden,
                                    'Requisiones' AS Tipo_Orden,
                                    0 AS Total,
                                    R.Genera_User AS User_Solicita,
                                    R.Genera_Fecha AS Fecha,
                                    C.descripcion AS Causa,
                                    R.notas AS Nota,
                                    'Proceso' AS proceso
                                FROM
                                    pv_requisiciones R
                                    INNER JOIN pv_entrada_salida C ON R.cod_entsal = C.cod_entsal 
                                    {where} AND R.ESTADO = 'P'";

                        response.Result = connection.Query<ResolucionTransaccionDTO>(query).ToList();
                    }

                }
                else
                {
                    using var connection = new SqlConnection(clienteConnString);

                    var query = $@"SELECT 
                                    R.boleta AS Cod_Orden,
                                    CASE 
                                        WHEN R.tipo = 'E' THEN 'Entrada'
                                        WHEN R.tipo = 'S' THEN 'Salida'
                                        ELSE R.tipo
                                    END AS Tipo_Orden,
                                    R.total,
                                    R.GENERA_USER,
	                                R.Genera_Fecha AS Fecha,
                                    C.descripcion AS Causa,
                                    R.notas AS Nota,
	                                'Proceso' AS proceso
                                FROM 
                                    pv_InvTranSac R
                                INNER JOIN 
                                    pv_entrada_salida C 
                                    ON R.cod_entsal = C.cod_entsal
                                        {where} AND R.TIPO = '{filtros.tipo}' AND R.ESTADO = 'P'";


                    response.Result = connection.Query<ResolucionTransaccionDTO>(query).ToList();

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

        public ErrorDto ResolucionTransaccion_Autorizar(int CodCliente, string tipo, string usuario ,List<ResolucionTransaccionDTO> lista)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();

            try
            {
                foreach (var item in lista)
                {
                    using var connection = new SqlConnection(clienteConnString);
                    {
                        if (item.seleccionado == true) {

                        string query = "";
                        if (tipo == "R")
                        {
                            query = @$"update pv_requisiciones set 
                                                autoriza_fecha = Getdate(),
                                                autoriza_user = '{usuario}'
                                             ,estado = 'A' 
                                            where cod_requisicion = '{item.Cod_Orden}' ";
                        }
                        else
                        {
                            query = @$"update pv_InvTranSac set 
                                            autoriza_fecha = Getdate(),
                                            autoriza_user =  '{usuario}', 
                                            estado = 'A' 
                                            where boleta = '{item.Cod_Orden}' and tipo = '{tipo}'";
                        }
                            response.Code = connection.Execute(query);
                        }

                  
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;

        }

        public ErrorDto ResolucionTransaccion_Rechazo(int CodCliente, string tipo, string usuario, List<ResolucionTransaccionDTO> lista)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();

            try
            {
                foreach (var item in lista)

                    if (item.seleccionado == true)
                    {
                        {
                    using var connection = new SqlConnection(clienteConnString);
                    {
                        string query = "";
                        if (tipo == "R")
                        {
                            query = @$"update pv_requisiciones set 
                                                autoriza_fecha = Getdate(),
                                                autoriza_user = '{usuario}'
                                             ,estado = 'R' where cod_requisicion = '{item.Cod_Orden}' ";
                        }
                        else
                        {
                            query = @$"update pv_InvTranSac set 
                                            autoriza_fecha = Getdate(),
                                            autoriza_user =  '{usuario}',
                                            estado = 'R' where boleta = '{item.Cod_Orden}' and tipo = '{tipo}'";
                        }

                        response.Code = connection.Execute(query);
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;

        }



    }
}