using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficioPagoDB
    {
        private readonly IConfiguration _config;

        public frmAF_BeneficioPagoDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo para obtener la lista de beneficios de pago
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBenePagoData>> AfiBeneficioPagoLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBenePagoData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_Beneficio as item,  rtrim(cod_Beneficio) + ' - ' + descripcion as descripcion from afi_beneficios
                                       where estado = 'A' and cod_beneficio in (select cod_beneficio from afi_bene_pago
                                       where Estado = 'S')";
                    response.Result = connection.Query<AfiBenePagoData>(query).ToList();
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
        /// Metodo para obtener la tabla de beneficios de pago
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDto<List<Afi_Beneficio_Pago>> AfiBeneficioPagosTabla_Obtener(int CodCliente, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<Afi_Beneficio_Pago>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select B.*
                                    --,U.DESCRIPCION 
                                    from afi_bene_pago B inner join 
                                             SOCIOS S on  B.CEDULA = S.CEDULA 
                                            -- left join UPROGRAMATICA U on S.UP = U.CODIGO
                                             where B.cod_beneficio = '{cod_beneficio}' and B.ESTADO = 'S' 
                                            -- order by U.DESCRIPCION,B.CEDULA";
                    response.Result = connection.Query<Afi_Beneficio_Pago>(query).ToList();
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
        /// Metodo para obtener nombre del beneficiario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consec"></param>
        /// <param name="cedulabn"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDto Beneficiarios_Obtener(int CodCliente, int consec, string cedulabn, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select nombre from beneficiarios where cedula in(select cedula from afi_bene_pago where cod_beneficio = '{cod_beneficio}' 
                                        and consec = '{consec}' )and cedulabn = '{cedulabn}' ";
                    info.Description = connection.Query<string>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Metodo para generar el pago de los beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="tabla"></param>
        /// <returns></returns>
        public ErrorDto AfiBeneficioPago_Generar(int CodCliente, string usuario, List<Afi_Beneficio_Pago> tabla)
        {
            mTESFuncionesDB mTES = new mTESFuncionesDB(_config);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            List<Afi_Beneficio_Pago> afi_Beneficio_Pagos = tabla;
            //afi_Beneficio_Pagos = JsonConvert.DeserializeObject<List<Afi_Beneficio_Pago>>(tabla);

            ErrorDto info = new ErrorDto();
            info.Code = 0;
            long vTesoreria = 0;
            try
            {
                string query = "", vCtaBene = "", detalle = "", detalle1 = "", detalle2 = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select cod_cuenta   from afi_beneficios where cod_beneficio = '{afi_Beneficio_Pagos[0].cod_beneficio.Trim()}'";
                    vCtaBene = connection.Query<string>(query).FirstOrDefault();

                    query = $@"select descripcion  from afi_beneficios where cod_beneficio = '{afi_Beneficio_Pagos[0].cod_beneficio.Trim()}'";
                    detalle = connection.Query<string>(query).FirstOrDefault();

                    foreach (var item in afi_Beneficio_Pagos)
                    {
                        vCtaBene = (vCtaBene != null) ? vCtaBene : "0";
                        if (detalle.Length > 26)
                        {
                            detalle1 = detalle.Substring(0, 26);
                            detalle2 = detalle.Substring(26, detalle.Length - 26);
                        }
                        else
                        {
                            detalle1 = detalle;
                            detalle2 = "";
                        }

                        vTesoreria = mTES.fxgTesoreriaMaestro(CodCliente, usuario, new TesoreriaMaestroModel
                        {

                            vTipoDocumento = mTES.fxTipoDocumento(item.tipo_emision),
                            vBanco = item.cod_banco,
                            vMonto = item.monto,
                            vBeneficiario = item.nombre,
                            vCodigo = item.cedula,
                            vOP = 0,
                            vDetalle1 = detalle1,
                            vReferencia = 0,
                            vDetalle2 = detalle2,
                            vCuenta = item.cta_bancaria,
                            vFecha = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day
                        });

                        //Actualiza el estado en tabla afi_bene_otorga
                        query = $@"UPDATE afi_bene_otorga
                                    SET estado = 'E',
                                        autoriza_user = '{usuario}',
                                        autoriza_fecha = Getdate()
                                    WHERE cedula = '{item.cedula}'
                                      AND cod_beneficio = '{item.cod_beneficio}'
                                      AND consec = {item.consec}";

                        connection.Execute(query);

                        //Actualiza el estado en tabla afi_bene_pago
                        query = $@"UPDATE afi_bene_pago
                                    SET estado = 'E',
                                        tesoreria = '{vTesoreria}',
                                        envio_user  = '{usuario}',
                                        envio_fecha  = Getdate()
                                    WHERE cedula = '{item.cedula}'
                                      AND cod_beneficio = '{item.cod_beneficio}'
                                      AND consec = {item.consec}";

                        connection.Execute(query);

                        mTES.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                        {
                            vSolicitud = vTesoreria,
                            vCtaConta = item.cta_bancaria,
                            vMonto = item.monto,
                            vDH = "H",
                            vLinea = 1
                        });

                        mTES.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                        {
                            vSolicitud = vTesoreria,
                            vCtaConta = item.cta_bancaria,
                            vMonto = item.monto,
                            vDH = "D",
                            vLinea = 2
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }
    }
}