using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfCartasNoCotizantesDB
    {
        private readonly IConfiguration _config;

        public FrmAfCartasNoCotizantesDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene el mes de corte para las cartas de no cotizantes, dependiendo del mes actual y el parámetro de aplicación en par_ahcr. (VB6: sbgAF_CartasNoCotizantes_ObtenerMesCorte)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Af_CartasNoCotizantes_Obtener(int CodEmpresa, int contabilidad)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<decimal>();
            decimal vMes = getGlngFechaCR(CodEmpresa, contabilidad);
            try
            {
                using var connection = new SqlConnection(conn);
                var query = $@"select cr_apl from par_ahcr";
                    var qryMes = connection.QueryFirstOrDefault<int>(query);

                    if (qryMes == 0)
                    {
                        if (vMes == 1)
                        {
                            vMes = 12;
                        }
                        else
                        {
                            vMes = vMes - 1;
                        }
                    }

                    response.Result = vMes;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }


        /// <summary>
        /// Obtiene los datos de las cartas de no cotizantes dependiendo de los filtros seleccionados. (VB6: sbgAF_CartasNoCotizantes_ObtenerDatos)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        private decimal getGlngFechaCR(int CodEmpresa, int contabilidad)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            decimal glngFechaCR;
            try
            {
                using var connection = new SqlConnection(conn);

                var queryPar_Ahcr = $@"select dbo.MyGetdate() as FechaProceso 
                          from CntX_Contabilidades where cod_contabilidad = @contabilidad";

                var vFechaProceso = connection.QueryFirstOrDefault<DateTime>(queryPar_Ahcr, new { contabilidad = contabilidad });

                int year = vFechaProceso.Year;
                int month = vFechaProceso.Month;
                string fechaStr = year.ToString() + month.ToString("00");
                glngFechaCR = decimal.Parse(fechaStr);
            }
            catch (Exception)
            {
                glngFechaCR = 0;
            }
            return glngFechaCR;
        }


        /// <summary>
        /// Obtiene los datos de las cartas de no cotizantes dependiendo de los filtros seleccionados. (VB6: sbgAF_CartasNoCotizantes_ObtenerDatos)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCartasNoCotizantesData>> Af_CartasNoCotizantesDatos_Obtener(int CodEmpresa, AfCartasNoCotizantesFiltros filtros)
        {
            var response = new ErrorDto<List<AfCartasNoCotizantesData>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                var query = $@"select S.cedula,S.nombre,datediff(m,A.fecahorro,dbo.MyGetdate()) as Meses
		                       ,isnull(sum(R.saldo),0) as Saldos,isnull(sum(V.Intc),0) as IntCor
		                       ,isnull(sum(V.IntM),0) as IntMor,isnull(sum(V.cuota),0) as Cuotas
		                        from Socios S inner join Ahorro_consolidado A on S.cedula = A.cedula
		                        and datediff(m,A.fecahorro,dbo.MyGetdate()) {filtros.meses} @MesesNoCotizar
		                        and S.estadoactual = 'S' and S.fechaingreso < @dtpIngreso
		                        left join Reg_Creditos R on S.cedula = R.cedula
		                        inner join Vista_Morosidad V on R.id_solicitud = V.id_solicitud
		                        inner join Catalogo C on R.codigo = C.codigo and C.retencion = 'N' and C.poliza = 'N'
		                        group by S.cedula,S.nombre,A.fecahorro
		                        Having isnull(Sum(V.cuota), 0) {filtros.mora} @CuotaMora";

                response.Result = connection.Query<AfCartasNoCotizantesData>(query,
                    new
                    {
                        MesesNoCotizar = filtros.mesesNoCotizar,
                        dtpIngreso = filtros.fechaIngreso,
                        CuotaMora = filtros.cuotaMora
                    }).ToList();

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