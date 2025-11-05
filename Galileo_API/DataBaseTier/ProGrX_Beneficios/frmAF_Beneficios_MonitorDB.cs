using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_MonitorDB
    {
        private readonly IConfiguration _config;

        public frmAF_Beneficios_MonitorDB(IConfiguration config)
        {
            _config = config;
        }
        /// <summary>
        /// Metodo para obtener los beneficios de la tabla vBeneficios_Integral
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="filtroString"></param>
        /// <returns></returns>
        public ErrorDTO<vBeneficios_IntegralDTOLista> BeneficiosMonitor_Obtener(int CodCliente, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            BeneficiosMonitor_Filtros filtros = JsonConvert.DeserializeObject<BeneficiosMonitor_Filtros>(filtroString) ?? new BeneficiosMonitor_Filtros();

            var response = new ErrorDTO<vBeneficios_IntegralDTOLista>();
            response.Result = new vBeneficios_IntegralDTOLista();

            string paginaActual = " ", paginacionActual = " ";
            try
            {
                string where = "";
                if (filtros.fecha != null)
                {
                    // Convertir la cadena ISO a DateTimeOffset
                    DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.fecha_inicio);
                    string fechainicio = fecha_inicio.ToString("yyyy-MM-dd");

                    DateTimeOffset fecha_corte = DateTimeOffset.Parse(filtros.fecha_corte);
                    string fechacorte = fecha_corte.ToString("yyyy-MM-dd");

                    switch (filtros.fecha)
                    {
                        case "R":
                            where = $" Where Registra_Fecha between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                            break;
                        case "A":
                            where = $" Where Autoriza_Fecha between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                            break;
                        case "P":
                            where = $" Where Pago_Fecha between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                            break;
                        default:
                            break;
                    }
                }

                if (filtros.estado != "T")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Estado = '{filtros.estado}' ";
                }

                if (filtros.institucion != "T")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Cod_Institucion = '{filtros.institucion}' ";
                }
                else
                {
                    filtros.unidad = "";
                }

                if (filtros.oficina != "T")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" cod_Oficina = '{filtros.oficina}' ";
                }

                if (filtros.estado_persona != "T")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" EstadoActual = '{filtros.estado_persona}' ";
                }

                //Lista de beneficios aqui...

                if (filtros.usuario_registra != null && filtros.usuario_registra != "")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Registra_Usuario like '%{filtros.usuario_registra}%' ";
                }

                if (filtros.usuario_autoriza != null && filtros.usuario_autoriza != "")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Autoriza_Usuario like '%{filtros.usuario_autoriza}%' ";
                }

                if (filtros.beneficio_id != null && filtros.beneficio_id != "")
                {

                    string beneficios = filtros.beneficio_id.Replace(",", "','");

                    where += where == "" ? " Where " : " And ";
                    where += $" Cod_Beneficio IN ('{beneficios}') ";
                }

                if (filtros.beneficiario_nombre != null && filtros.beneficiario_nombre != "")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" NOMBRE_BENEFICIARIO like '%{filtros.beneficiario_nombre}%' ";
                }

                if (filtros.solicita_id != null && filtros.solicita_id != "")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Solicita like '%{filtros.solicita_id}%' ";
                }

                if (filtros.solicita_nombre != null && filtros.solicita_nombre != "")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Solicita_Nombre like '%{filtros.solicita_nombre}%' ";
                }

                if (filtros.unidad != null && filtros.unidad != "")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Departamento_Desc = '{filtros.unidad.Trim()}' ";
                }

                if (filtros.vfiltro != null && filtros.vfiltro != "")
                {
                    where = where + " AND (Cod_Beneficio LIKE '%" + filtros.vfiltro + "%' OR cedula LIKE '%" + filtros.vfiltro + "%' OR NOMBRE_BENEFICIARIO LIKE '%" + filtros.vfiltro + "%') ";
                }

                if (filtros.pagina != null)
                {
                    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                }


                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    var query = $@"SELECT COUNT(*) FROM vBeneficios_Integral {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select 0 as 'Btn', Cod_Beneficio, Consec, Cedula, NOMBRE_BENEFICIARIO, Monto, Estado_Desc, Beneficio_Desc
                                   , Solicita, Solicita_Nombre, Registra_Fecha, Registra_User, Autoriza_Fecha, Autoriza_User 
                                   , Empresa_Desc, Departamento_Desc, Oficina_Desc
                                   from vBeneficios_Integral {where}  Order by Registra_fecha desc, Beneficio_Desc, Consec desc
                                    {paginaActual} {paginacionActual}";
                    response.Result.Beneficios = connection.Query<vBeneficios_IntegralDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new vBeneficios_IntegralDTOLista();
            }

            return response;

        }

        /// <summary>
        /// Metodo para obtener la lista de instituciones
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<OpcionesLista>> InstitucionesLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<OpcionesLista>>();
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(descripcion) as 'descripcion', cod_institucion as 'item'
			                  from instituciones order by descripcion";
                    response.Result = connection.Query<OpcionesLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<OpcionesLista>();
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de estados
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<OpcionesLista>> EstadosLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<OpcionesLista>>();
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_estado) as 'item', rtrim(descripcion) as 'descripcion'
				                          from  afi_Estados_Persona";
                    response.Result = connection.Query<OpcionesLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<OpcionesLista>();
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de oficinas
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<OpcionesLista>> OficinasLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
           var response = new ErrorDTO<List<OpcionesLista>>();
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_Oficina) as 'item', rtrim(descripcion) as 'descripcion'
				                         from  SIF_Oficinas order by Descripcion";
                    response.Result = connection.Query<OpcionesLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<OpcionesLista>();
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<OpcionesLista>> BeneficiosLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<OpcionesLista>>();
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" select COD_BENEFICIO as item, rtrim(DESCRIPCION) as descripcion
		                                    from AFI_BENEFICIOS 
		                                    where Estado = 'A' 
			                                --and descripcion like '%%' 
		                                    order by descripcion";
                    response.Result = connection.Query<OpcionesLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<OpcionesLista>();
            }
            return response;
        }
    }
}