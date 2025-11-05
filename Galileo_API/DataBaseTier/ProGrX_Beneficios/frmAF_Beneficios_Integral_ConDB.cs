using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_Integral_ConDB
    {
        private readonly IConfiguration _config;

        public frmAF_Beneficios_Integral_ConDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtengo la lista de beneficios registrados en una consulta general, con filtros.
        /// </summary>
        /// <param name="Jfiltro"></param>
        /// <returns></returns>
        public ErrorDTO<BeneConsultaDatosLista> BeneConsultasLista_Obtener(string Jfiltro)
        {
            BeneConsultaFiltros filtro = JsonConvert.DeserializeObject<BeneConsultaFiltros>(Jfiltro);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(filtro.codCliente);
            var response = new ErrorDTO<BeneConsultaDatosLista>();
            response.Result = new BeneConsultaDatosLista();
            try
            {
                string paginaActual = " ", paginacionActual = " ";
                string categoria = "";
                if (filtro.categoria != "T")
                {
                    categoria += $" Where COD_CATEGORIA like '%{filtro.categoria}%' ";
                }
                
                string where = $@" WHERE COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS {categoria}) ";
                string whereBeca = "";
                if(filtro.todasFechas == false)
                {
                    if (filtro.tipoFecha != null)
                    {


                        // Convertir la cadena ISO a DateTimeOffset
                        DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtro.fechaInicio);
                        string fechainicio = fecha_inicio.ToString("yyyy-MM-dd");

                        DateTimeOffset fecha_corte = DateTimeOffset.Parse(filtro.fechaCorte);
                        string fechacorte = fecha_corte.ToString("yyyy-MM-dd");



                        switch (filtro.tipoFecha)
                        {
                            case "R":
                                where += $" AND Registra_Fecha between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                                whereBeca += $" WHERE B.REGISTRA_FECHA between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                                break;
                            case "A":
                                where += $" AND Autoriza_Fecha between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                                whereBeca += $" WHERE B.APRUEBA_FECHA between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                                break;
                            case "P":
                                where += $" AND Pago_Fecha between '{fechainicio} 00:00:00' and '{fechacorte} 23:59:59' ";
                                break;
                            default:
                                break;
                        }
                    }
                }

              

                if (filtro.estado != "T")
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Estado like '%{filtro.estado}%' ";
                }

                switch (filtro.cedula)
                {
                    case null:

                        break;
                    case " ":

                        break;
                    case "0":

                        break;
                    case "":

                        break;
                    default:
                        where += where == "" ? " Where " : " And ";
                        where += $" cedula like '%{filtro.cedula.Trim()}%' ";
                        break;
                }

                if (filtro.noExpediente != null)
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" Expediente like '%{filtro.noExpediente}%' ";
                }

                if (filtro.usuario != null)
                {
                    where += where == "" ? " Where " : " And ";
                    where += $" UPPER(registra_user) like '%{filtro.usuario.Trim().ToUpper()}%' ";
                }

                string filtroTexto = "", filtroTextoBeca = "";
                if (filtro.filtro != null && filtro.filtro != "")
                {
                    filtroTexto = " AND ( Expediente LIKE '%" + filtro.filtro + "%' " +
                        "OR cedula LIKE '%" + filtro.filtro + "%' " +
                        "OR Beneficio_Desc LIKE '%" + filtro.filtro + "%' " +
                        "OR NOMBRE_BENEFICIARIO LIKE '%" + filtro.filtro + "%' " +
                        "OR registra_user LIKE '%" + filtro.filtro + "%' " +
                        "OR SEPELIO_IDENTIFICACION LIKE '%" + filtro.filtro + "%'" +
                        "OR PROVINCIA LIKE '%" + filtro.filtro + "%'" +
                        "OR Grupo LIKE '%" + filtro.filtro + "%' ) ";

                    if(whereBeca != "")
                    {
                        whereBeca += " AND ";
                    }
                    else
                    {
                        whereBeca = " WHERE ";
                    }

                    filtroTextoBeca = " ( B.COD_EXPEDIENTE LIKE '%" + filtro.filtro + "%' " +
                        "OR B.CEDULA_ASO LIKE '%" + filtro.filtro + "%' " +
                        "OR B.NOMBRE_ASO LIKE '%" + filtro.filtro + "%' " +
                        "OR B.ASO_EMAIL LIKE '%" + filtro.filtro + "%' " +
                        "OR B.PROM_SAL_GESTIONAR LIKE '%" + filtro.filtro + "%' ) ";
                }

                if (filtro.pagina != null)
                {
                    paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                }

                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    var query = $@"SELECT COUNT(*) FROM ( SELECT CONCAT(RIGHT(CONCAT('00000', H.ID_BENEFICIO), 5),
                                               TRIM(H.COD_BENEFICIO) ,
	                                           RIGHT(CONCAT('00000',H.CONSEC), 5)
	                                         )  AS Expediente, 
                                             COD_BENEFICIO,
                                             registra_user,
                                             cedula, Estado, Registra_Fecha, Autoriza_Fecha, Pago_Fecha 
                                      FROM vBeneficios_W_Integral H LEFT JOIN AFI_BENE_ESTADOS E ON E.COD_ESTADO = H.ESTADO AND E.COD_ESTADO IN (
                                      SELECT COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                                       SELECT COD_GRUPO FROM AFI_BENE_GRUPOS {categoria}
                                      ))) T {where} ";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();


                    query = $@"SELECT * FROM (
                                        SELECT 
                                        CONCAT(RIGHT(CONCAT('00000', H.ID_BENEFICIO), 5),
                                               TRIM(H.COD_BENEFICIO) ,
	                                           RIGHT(CONCAT('00000',H.CONSEC), 5)
	                                         )  AS Expediente,
                                        H.REGISTRA_FECHA, --Fecha solicitud / registro
                                        H.AUTORIZA_FECHA, --Fecha Aprovacion
                                        H.PAGO_FECHA, --Fecha Pago
                                        H.ID_BENEFICIO, -- Expediente
                                        H.CONSEC,   -- Expediente
                                        H.COD_BENEFICIO,   -- Expediente
                                        H.Beneficio_Desc, -- Beneficio
                                        H.MONTO, --Monto Aprobado
                                        H.MONTO_APLICADO, -- Monto Aplicado
                                        H.ESTADO, 
                                        CASE WHEN H.ESTADO = 'E' THEN 'ENVIADO'
										WHEN H.ESTADO is null OR H.ESTADO = '' THEN 'SIN ESTADO'
										ELSE (SELECT E.DESCRIPCION FROM AFI_BENE_ESTADOS E WHERE E.COD_ESTADO = H.ESTADO) 
										END AS ESTADO_DESC,
                                        H.cedula,  -- cedula
                                        H.NOMBRE_BENEFICIARIO, -- nombre completo 
                                        H.SEPELIO_IDENTIFICACION, -- Cedula Persona fallecida.
                                        Categoria_Desc, 
                                        Estado_Persona,
										(SELECT B.CRECE_GRUPO FROM AFI_BENE_OTORGA B WHERE ID_BENEFICIO = H.ID_BENEFICIO ) as Grupo,
										CASE
										WHEN (SELECT CAPACITACION_CMP FROM AFI_BENE_SOCIO_CRECE C 
										WHERE C.COD_BENEFICIO = H.COD_BENEFICIO AND C.CONSEC = H.CONSEC ) = 1 THEN 'SI'
										ELSE 'NO'
										END AS Capacitacion_Completa,
										CASE
										WHEN (SELECT APLICA_PRODUCTO FROM AFI_BENE_SOCIO_CRECE C 
										WHERE C.COD_BENEFICIO = H.COD_BENEFICIO AND C.CONSEC = H.CONSEC ) = 1 THEN 'SI'
										ELSE 'NO'
										END AS APLICA_PRODUCTO_FIN,
										H.TIPO,
										CASE 
											WHEN H.TIPO = 'M' THEN 'Monetario'
                                            WHEN H.TIPO = 'P' THEN 'Producto'
											ELSE 'Ambos'
										END AS TipoDesc,
										B.PAGOS_MULTIPLES ,
                                        H.MONTO_EJECUTADO ,
                                        H.REQUIERE_JUSTIFICACION,
                                        H.PROVINCIA,
										(select C.DESCRIPCION FROM CANTONES C 
										where COD_CANTON = S.CANTON AND COD_PROVINCIA = S.PROVINCIA )
										AS CANTON,
										(select D.DESCRIPCION FROM DISTRITOS D 
										where COD_CANTON = S.CANTON AND COD_PROVINCIA = S.PROVINCIA AND D.COD_DISTRITO = S.DISTRITO )
										AS Distrito,
										CASE 
										WHEN S.SEXO = 'F' THEN 'Femenino'
										WHEN S.SEXO = 'M' THEN 'Masculino'
										ELSE 'Otro'
										END AS 'Genero',
										S.AF_EMAIL,
									    H.registra_user --usuario
                                        ,CASE WHEN I.CASO_ID != '' THEN 'Interface'
										ELSE 'Manual'
										END AS int_desk
                                        FROM vBeneficios_W_Integral 
                                        H LEFT JOIN AFI_BENEFICIOS B ON B.COD_BENEFICIO = H.COD_BENEFICIO 
										LEFT JOIN SOCIOS S ON S.CEDULA = H.CEDULA
                                        LEFT JOIN AFI_BENE_OTORGA_INT I ON I.ID_BENEFICIO = H.ID_BENEFICIO
									  ) T {where} {filtroTexto} 
									  Order by Registra_fecha desc, Beneficio_Desc, Consec desc {paginaActual} {paginacionActual}";
                    response.Result.lista = connection.Query<BeneConsultaDatos>(query).ToList();

                    if(filtro.categoria == "B_BECA")
                    {
                        try
                        {
                            query = $@"
                                    SELECT 
                                      CONCAT(B.PERIODO_LECTIVO, 'BECA',B.COD_EXPEDIENTE ) as expediente,
                                      B.REGISTRA_FECHA as registra_fecha,
                                      B.APRUEBA_FECHA as autoriza_fecha,
                                      B.COD_EXPEDIENTE as id_beneficio,
                                      B.COD_EXPEDIENTE as consec,
                                      'BECA' as cod_beneficio ,
                                      'Besa Socioeconomica' as beneficio_desc,
                                      B.COD_ESTADO as estado, 
                                      E.ESTADO as  estado_desc ,
                                      B.CEDULA_ASO as cedula, 
                                      B.NOMBRE_ASO as nombre_beneficiario, 
                                      B.ADVERTENCIAS as estado_persona,
                                      B.ASO_EMAIL as af_email, 
                                      B.PROM_SAL_GESTIONAR as monto,
                                      'SIF' as int_desk
                                      FROM BECAS_V2_EXPEDIENTES B LEFT JOIN 
                                      BECAS_V2_ESTADOS_EXPEDIENTES E ON E.COD_ESTADO = B.COD_ESTADO 
                                      {whereBeca} {filtroTextoBeca} 
                                      ORDER BY B.COD_EXPEDIENTE desc {paginaActual} {paginacionActual}
                        ";

                            var becas = connection.Query<BeneConsultaDatos>(query).ToList();

                            response.Result.lista.AddRange(becas);
                        }
                        catch (Exception ex)
                        {
                            response.Description = ex.Message;
                        }
                        
                    }
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
            }

            return response;

        }

        /// <summary>
        /// Obtengo la lista de los estados que estan configurados en el beneficio por categoria, configurados en Configuracion Grupos.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="categoria"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfBeneficioIntegralDropsLista>> BeneConsultaEstados_Obtener(int CodCliente, string categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfBeneficioIntegralDropsLista>>();
            try
            {

                var query = "";

                using var connection = new SqlConnection(clienteConnString);
                {
                    if (categoria != "T")
                    {
                        query = $@"SELECT COD_ESTADO AS item, descripcion FROM AFI_BENE_ESTADOS WHERE COD_ESTADO IN (
                                        SELECT COD_ESTADO FROM  AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                                                                      SELECT COD_GRUPO FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = '{categoria}'
											                          )
                                        ) ORDER BY ORDEN ASC";
                    }
                    else
                    {
                        query = $@"SELECT COD_ESTADO AS item, descripcion FROM AFI_BENE_ESTADOS ORDER BY ORDEN ASC";
                    }

                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();

                    response.Result.Insert(0, new AfBeneficioIntegralDropsLista { item = "T", descripcion = "TODOS" });

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
        /// Obtengo la informacion del beneficio seleccionado en la consulta general.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDTO<BeneficioDTO> BeneficioIntegral_Obtener(int CodCliente, long beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<BeneficioDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select * FROM vBeneficios_Integral 
	                                 WHERE ID_BENEFICIO = {beneficio} ";
                    response.Result = connection.Query<BeneficioDTO>(query).FirstOrDefault();

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