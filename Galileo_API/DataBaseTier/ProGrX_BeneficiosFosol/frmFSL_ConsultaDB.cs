using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;

namespace PgxAPI.DataBaseTier
{
    public class frmFSL_ConsultaDB
    {
        private readonly IConfiguration _config;
        private mProGrX_AuxiliarDB mAuxiliarDB;

        public frmFSL_ConsultaDB(IConfiguration config)
        {
            _config = config;
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Consulta planes de FSL
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<FslConsultaListas>> FslConsultaPlanes_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<FslConsultaListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select COD_PLAN as item,  COD_PLAN + ' - ' + rtrim(Descripcion) as descripcion from FSL_PLANES  where activo = 1 order by descripcion";
                    response.Result = connection.Query<FslConsultaListas>(query).ToList();
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
        /// Consulta las causas segun plan seleccionado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_plan"></param>
        /// <returns></returns>
        public ErrorDTO<List<FslConsultaListas>> FslConsultaCausas_Obtener(int CodCliente, string cod_plan)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<FslConsultaListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_causa as item, rtrim(descripcion) as descripcion from FSL_PLANES_CAUSAS
		                    	   where cod_plan = '{cod_plan}' and Activa = 1 order by Descripcion";
                    response.Result = connection.Query<FslConsultaListas>(query).ToList();
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
        /// Consulta las enfermedades 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<FslConsultaListas>> FslConsultaEnfermedades_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<FslConsultaListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select cod_enfermedad as item ,DESCRIPCION as descripcion from FSL_TIPOS_ENFERMEDADES where Activa = 1 order by descripcion";
                    response.Result = connection.Query<FslConsultaListas>(query).ToList();
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
        /// Consulta los comites
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<FslConsultaListas>> FslConsutaComites_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<FslConsultaListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @"select cod_Comite as 'item',  RTRIM(cod_Comite) + ' - ' + DESCRIPCION AS 'descripcion' from FSL_COMITES
		                                Where ACTIVO = 1 ORDER BY COD_COMITE";
                    response.Result = connection.Query<FslConsultaListas>(query).ToList();
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
        /// Consulta los estados de las personas
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<FslConsultaListas>> FslConsutaEstadoPersonas_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<FslConsultaListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @"select COD_ESTADO as 'item', RTRIM(COD_ESTADO ) + ' - ' + DESCRIPCION AS 'descripcion' from AFI_ESTADOS_PERSONA   where Activo = 1 ORDER BY COD_ESTADO";
                    response.Result = connection.Query<FslConsultaListas>(query).ToList();
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
        /// Consulta los tipos de gestion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<FslConsultaListas>> FslConsutaTiposGestion_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<FslConsultaListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @"select COD_GESTION as 'item', RTRIM(COD_GESTION) + ' - ' + DESCRIPCION AS 'descripcion' from FSL_TIPOS_GESTIONES where Activa = 1 order by COD_GESTION";
                    response.Result = connection.Query<FslConsultaListas>(query).ToList();
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
        /// Consulta los tipos de apelaciones
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<FslConsultaListas>> FslConsutaTiposApelaciones_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<FslConsultaListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @"select COD_APELACION as 'item', RTRIM(COD_APELACION ) + ' - ' + DESCRIPCION AS 'descripcion' from FSL_TIPOS_APELACIONES  where Activa = 1 ORDER BY COD_APELACION";
                    response.Result = connection.Query<FslConsultaListas>(query).ToList();
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
        /// Consulta los miembros de un comite
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_comite"></param>
        /// <returns></returns>
        public ErrorDTO<List<FslConsultaListas>> FslConsultaComiteMiembros_Obtener(int CodCliente, string cod_comite)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<List<FslConsultaListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cedula as item,  rtrim(cedula) + ' - ' + rtrim(Nombre) as descripcion from FSL_COMITES_MIEMBROS
			                           where cod_comite = '{cod_comite}' and Activo = 1 order by Nombre";
                    response.Result = connection.Query<FslConsultaListas>(query).ToList();
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

        public ErrorDTO<List<FslConsultaExpedienteDatos>> FslConsultaExpedientes_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            fslConsultaFiltros _filtro = JsonConvert.DeserializeObject<fslConsultaFiltros>(filtros);

            var response = new ErrorDTO<List<FslConsultaExpedienteDatos>>();
            try
            {
                string filtro = "", vEstado = "";
                if (_filtro.estado == "TODOS")
                {
                    vEstado = "'A','R','P','X','Y'";
                }
                else
                {
                    vEstado = $"'{_filtro.estado}'";
                }

                if (_filtro.estado == "AP")
                {
                    filtro += " and RESOLUCION_ESTADO = 'Y' ";
                }

                if (_filtro.cod_plan != "TODOS")
                {
                    filtro += $" and COD_PLAN = '{_filtro.cod_plan}'";
                    string vCausas = "";
                    foreach (var item in _filtro.cod_causa)
                    {
                        vCausas += $"'{item.item}',";
                    }
                    vCausas = vCausas.TrimEnd(',');
                    filtro += $" and COD_CAUSA in({vCausas})";
                }

                if (_filtro.cod_enfermedad.Count > 0)
                {
                    string enfermedades = "";
                    foreach (var item in _filtro.cod_enfermedad)
                    {
                        if (item.item != "TODOS")
                        {
                            enfermedades += $"'{item.item}',";
                        }
                    }
                    enfermedades = enfermedades.TrimEnd(',');
                    filtro += $" and COD_ENFERMEDAD in({enfermedades})";
                }

                if (_filtro.fechas == false)
                {
                    // Convertir la cadena ISO a DateTimeOffset
                    string fechainicio = mAuxiliarDB.validaFechaGlobal(_filtro.fecha_inicio);
                    string fechacorte = mAuxiliarDB.validaFechaGlobal(_filtro.fecha_corte);

                    switch (_filtro.estado)
                    {
                        case "A":
                        case "R":
                            filtro += $" and Resolucion_Fecha between '{_filtro.fecha_inicio}' AND '{_filtro.fecha_corte}' ";
                            break;
                        default:
                            filtro += $" and Registro_Fecha between '{_filtro.fecha_inicio}' AND '{_filtro.fecha_corte}' ";
                            break;
                    }
                }

                if (_filtro.texto_buscar != null && _filtro.texto_buscar != "")
                {
                    switch (_filtro.cod_buscarPor)
                    {
                        case "01":
                            filtro += $"  and cedula like '%{_filtro.texto_buscar}%'";
                            break;
                        case "02":
                            filtro += $"  and Presenta_Cedula like '%{_filtro.texto_buscar}%'";
                            break;
                        case "03":
                            filtro += $"  and Presenta_Nombre like '%{_filtro.texto_buscar}%'";
                            break;
                        default:
                            break;
                    }
                }

                if (_filtro.nombre != null && _filtro.nombre != "")
                {
                    filtro += $" and Nombre like '%{_filtro.nombre}%'";
                }

                if (_filtro.cod_tipo != "TODOS")
                {
                    filtro += $" and Tipo_Desembolso = '{_filtro.cod_tipo}'";
                }

                if (_filtro.estadoPersona != "")
                {
                    filtro += $" and EstadoActual = '{_filtro.estadoPersona}'";
                }

                if (_filtro.cod_comite != "TODOS")
                {
                    filtro += $" and COD_COMITE = '{_filtro.cod_comite}'";

                    if (_filtro.resueltoMiembro != "TODOS")
                    {
                        filtro += $" and dbo.fxFSL_Expediente_ComiteMiembro(Cod_Expediente, '{_filtro.resueltoMiembro}' ) >= 1 ";
                    }
                }

                if (_filtro.expediente != "")
                {
                    filtro += $" and cod_expediente = '{_filtro.expediente}'";
                }

                if (_filtro.usuario != "")
                {
                    filtro += $" and Registro_Usuario = '{_filtro.usuario}'";
                }

                if (_filtro.gestionRegistrada != "TODOS")
                {
                    filtro += $"  and dbo.fxFSL_Expediente_GestionRegistrada(cod_Expediente,'{_filtro.gestionRegistrada}') >= 1";
                }

                if (_filtro.apelacionRegistrada != "TODOS")
                {
                    filtro += $"  and dbo.fxFSL_Expediente_ApelacionRegistrada(cod_Expediente,'{_filtro.apelacionRegistrada}') >= 1";
                }


                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_EXPEDIENTE,CEDULA,NOMBRE,EDAD,REGISTRO_USUARIO,REGISTRO_FECHA,ESTADO_DESC
                                   , PLAN_DESC,CAUSA_DESC,ENFERMEDAD_DESC,COMITE_DESC,RESOLUCION_FECHA
                                   ,TOTAL_DISPONIBLE, TOTAL_APLICADO,TOTAL_SOBRANTE, PRESENTA_CEDULA, PRESENTA_NOMBRE
                                    from vFSL_CasosLista Where Estado in({vEstado}) {filtro}";
                    response.Result = connection.Query<FslConsultaExpedienteDatos>(query).ToList();
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