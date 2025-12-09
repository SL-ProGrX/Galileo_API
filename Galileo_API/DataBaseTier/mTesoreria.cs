using Dapper;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class MTesoreria
    {
        private readonly IConfiguration _config;
        private readonly string dirRDLC;
        public MTesoreria(IConfiguration config)
        {
            _config = config;
            dirRDLC = _config.GetSection("AppSettings").GetSection("RutaRDLC").Value ?? string.Empty;
        }

        /// <summary>
        /// Obtengo lista de tipos de documentos para modulo contable. 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> tes_TiposDocumentos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();
            resp.Code = 0;
            resp.Result = new List<DropDownListaGenericaModel>();
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select TIPO AS ITEM, DESCRIPCION from tes_tipos_doc";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtengo lista de Unidades disponibles para el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCbo(int CodEmpresa, string usuario, int banco, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();
            resp.Code = 0;
            resp.Result = new List<DropDownListaGenericaModel>();
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select rtrim(C.cod_unidad) as 'item', rtrim(C.descripcion) as 'descripcion' 
                                from tes_unidad_ASG A inner join CntX_Unidades C on A.cod_unidad = C.cod_unidad and C.cod_contabilidad = @contabilidad
                                Where A.id_Banco = @banco and A.nombre = @usuario and activa = 1
                                order by C.Descripcion ";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { contabilidad = contabilidad, banco = banco, usuario = usuario }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtengo lista de conceptos disponibles para el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="banco"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCbo(int CodEmpresa, string usuario, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();
            resp.Code = 0;
            resp.Result = new List<DropDownListaGenericaModel>();
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select rtrim(C.cod_Concepto) as 'item', rtrim(C.Descripcion) as 'descripcion'
                                from tes_conceptos_ASG A inner join Tes_Conceptos C on A.cod_concepto = C.cod_concepto
                                Where A.id_Banco = @banco and A.nombre = @usuario and estado = 'A'
                                order by C.Descripcion";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { banco = banco, usuario = usuario }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtengo validación de permisos para el usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vUsuario"></param>
        /// <param name="vTipo"></param>
        /// <param name="vGestion"></param>
        /// <returns></returns>
        public ErrorDto<bool> fxTesTipoAccesoValida(int CodEmpresa, string vBanco, string vUsuario, string vTipo, string vGestion = "S")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<bool>();
            resp.Code = 0;
            resp.Result = false;
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select isnull(Count(*),0) as Existe
                                from tes_documentos_ASG A inner join Tes_Tipos_Doc T on A.tipo = T.tipo
                                Where A.id_Banco = @banco and A.nombre = @usuario
                                and A.tipo = @tipo ";

                    switch (vGestion)
                    {
                        case "S":
                            query += " and A.SOLICITA = 1";
                            break;
                        case "A":
                            query += " and A.AUTORIZA = 1";
                            break;
                        case "G":
                            query += " and A.GENERA = 1";
                            break;
                        case "X":
                            query += " and A.ASIENTOS = 1";
                            break;
                        case "N":
                            query += " and A.ANULA = 1";
                            break;
                        default:
                            break;
                    }

                    var result = connection.QueryFirstOrDefault<int>(query, new { banco = vBanco, usuario = vUsuario, tipo = vTipo });

                    if (result > 0)
                    {
                        resp.Result = true;
                    }
                    else
                    {
                        resp.Result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = false;
            }
            return resp;
        }

        /// <summary>
        /// Obtener lista de bancos para carga de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="gestion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGestion(int CodEmpresa, string usuario, string gestion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();
            resp.Code = 0;
            resp.Result = new List<DropDownListaGenericaModel>();
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);

                // 1) Lista blanca: SOLO columnas reales permitidas para "gestión"
                var gestionesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Autoriza",
                    "Genera",
                    "Asientos"
                };

                query = $@"select id_banco as item,descripcion from Tes_Bancos where Estado = 'A' and id_Banco 
                                    in(select id_banco from tes_documentos_ASG Where nombre = @usuario and {gestion} = 1 
                                    group by id_banco)";
                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { usuario = usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtengo el valor del campo de comprobante para la carga de documentos
        /// </summary>
        /// <param name="vBanco"></param>
        /// <param name="vTipo"></param>
        /// <param name="vCampo"></param>
        /// <returns></returns>
        public ErrorDto<string> fxTesBancoDocsValor(int CodEmpresa, int vBanco, string vTipo, string vCampo = "Comprobante")
        {
            var resp = new ErrorDto<string>();
            resp.Code = 0;
            resp.Result = "";
            try
            {
                resp.Result = fxTesTipoDocExtraeDato(CodEmpresa, vBanco, vTipo, "Comprobante").Result ?? "";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }
            return resp;
        }


        /// <summary>
        /// Obtener los tipos de documentos para carga de emisión de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Banco"></param>
        /// <param name="Tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCboAcceso(int CodEmpresa, string Usuario, int Banco, string? Tipo = "S")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY, 
                        RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion 
                        FROM tes_documentos_ASG A INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo 
                        WHERE A.id_Banco = @banco AND A.nombre = @usuario";

                    switch (Tipo)
                    {
                        case "S":
                            query += " and A.SOLICITA = 1";
                            break;
                        case "A":
                            query += " and A.AUTORIZA = 1";
                            break;
                        case "G":
                            query += " and A.GENERA = 1";
                            break;
                        case "X":
                            query += " and A.ASIENTOS = 1";
                            break;
                        case "N":
                            query += " and A.ANULA = 1";
                            break;
                        default:
                            break;
                    }
                    query += " order by T.Descripcion";

                    resp.Result = connection
                        .Query<DropDownListaGenericaModel>(query, new { banco = Banco, usuario = Usuario })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Obtener los tipos de documentos para carga de firma electrónica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Banco"></param>
        /// <param name="Tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCboAccesoFirmas(int CodEmpresa, string Usuario, int Banco, string? Tipo = "S")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string query = $@" SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY, 
                    RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion 
                    FROM tes_documentos_ASG A INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo 
                    INNER JOIN TES_BANCO_DOCS D ON T.tipo = D.tipo AND A.id_banco = D.id_Banco 
                    WHERE A.id_Banco = @banco AND A.nombre = @usuario AND D.comprobante = '01'";

                switch (Tipo)
                {
                    case "S":
                        query += " and A.SOLICITA = 1";
                        break;
                    case "A":
                        query += " and A.AUTORIZA = 1";
                        break;
                    case "G":
                        query += " and A.GENERA = 1";
                        break;
                    case "X":
                        query += " and A.ASIENTOS = 1";
                        break;
                    case "N":
                        query += " and A.ANULA = 1";
                        break;
                    default:
                        break;
                }

                resp.Result = connection
                    .Query<DropDownListaGenericaModel>(query, new { banco = Banco, usuario = Usuario })
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene el consecutivo de un tipo de documento para un banco específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="tipo"></param>
        /// <param name="avance"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<long> fxTesTipoDocConsec(int CodEmpresa, int id_banco, string tipo, string avance = "+", string plan = "-sp-")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<long>();
            resp.Code = 0;
            resp.Result = 0;
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    long consecutivo = 0;
                    string strSQL;

                    if (tipo != "TE" && plan != "-sp-")
                    {
                        plan = "-sp-";
                    }

                    if (plan == "-sp-")
                    {
                        strSQL = $"SELECT Consecutivo FROM tes_banco_docs WHERE tipo = @Tipo AND id_banco = @Banco";
                    }
                    else
                    {
                        strSQL = $"SELECT ISNULL(NUMERO_TE, 0) AS Consecutivo FROM TES_BANCO_PLANES_TE WHERE id_banco = @Banco AND COD_PLAN = @Plan";
                    }

                    var result = connection.QueryFirstOrDefault<long>(strSQL,
                        new { Tipo = tipo, Banco = id_banco, Plan = plan });

                    switch (avance)
                    {
                        case "+":
                            consecutivo = result + 1;
                            break;
                        case "-":
                            consecutivo = result - 1;
                            break;
                        case "/":
                            consecutivo = result;
                            break;
                        default:
                            consecutivo = result;
                            break;
                    }

                    resp.Result = consecutivo;

                    if (avance != "/")
                    {
                        if (plan == "-sp-")
                        {
                            query = $"UPDATE tes_banco_docs SET consecutivo = consecutivo {avance} 1 WHERE Tipo = @Tipo AND id_banco = @Banco";
                        }
                        else
                        {
                            query = $"UPDATE TES_BANCO_PLANES_TE SET NUMERO_TE = ISNULL(NUMERO_TE,0) {avance} 1 WHERE COD_PLAN = @Plan AND id_banco = @Banco";
                        }
                        connection.Execute(query, new { Consecutivo = consecutivo, Tipo = tipo, Banco = id_banco, Plan = plan });

                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = 0;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene el consecutivo interno de un tipo de documento para un banco específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="tipo"></param>
        /// <param name="avance"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<long> fxTesTipoDocConsecInterno(int CodEmpresa, int id_banco, string tipo, string avance = "+", string plan = "-sp-")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<long>();
            resp.Code = 0;
            resp.Result = 0;

            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    long consecutivo = 0;
                    string strSQL;

                    if (tipo != "TE" && plan != "-sp-")
                    {
                        plan = "-sp-";
                    }

                    if (plan == "-sp-" || plan == "")
                    {
                        strSQL = $@"select isnull(CONSECUTIVO_DET,0) as 'Consecutivo' from tes_banco_docs where tipo = @Tipo and id_banco = @Banco ";
                    }
                    else
                    {
                        strSQL = $@"select isnull(NUMERO_INTERNO,0) as 'Consecutivo' 
                                   from TES_BANCO_PLANES_TE 
                                    where id_banco = @Banco and COD_PLAN = @Plan ";
                    }

                    var result = connection.QueryFirstOrDefault<long>(strSQL,
                        new { Tipo = tipo, Banco = id_banco, Plan = plan });

                    switch (avance)
                    {
                        case "+":
                            consecutivo = result + 1;
                            break;
                        case "-":
                            consecutivo = result - 1;
                            break;
                        case "/":
                            consecutivo = result;
                            break;
                        default:
                            consecutivo = result;
                            break;
                    }

                    resp.Result = consecutivo;

                    if (avance != "/")
                    {
                        if (plan == "-sp-")
                        {
                            query = $@"update tes_banco_docs set CONSECUTIVO_DET = isnull(CONSECUTIVO_DET,0) {avance} 1 where Tipo = @Tipo and id_banco = @Banco";
                        }
                        else
                        {
                            query = $"update TES_BANCO_PLANES_TE set NUMERO_INTERNO = isnull(NUMERO_INTERNO,0) {avance} 1 where cod_plan = @Tipo and id_banco = @Banco";
                        }
                        connection.Execute(query, new { Consecutivo = consecutivo, Tipo = tipo, Banco = id_banco, Plan = plan });

                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = 0;
            }
            return resp;
        }

        public ErrorDto<string> fxTesTipoDocExtraeDato(int CodEmpresa, int Banco, string TipoDoc, string Campo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<string>
            {
                Code = 0,
                Description = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string query = Campo?.Trim().ToLowerInvariant() switch
                {
                    "mod_consec" => @"
                select mod_consec as item
                from tes_banco_docs
                where tipo = @tipoDoc and id_banco = @banco",

                    "comprobante" => @"
                select Comprobante as item
                from tes_banco_docs
                where tipo = @tipoDoc and id_banco = @banco",

                    _ => null
                };

                if (query == null)
                {
                    resp.Code = -1;
                    resp.Description = "Campo no permitido para consulta.";
                    resp.Result = null;
                    return resp;
                }

                resp.Result = connection.QueryFirstOrDefault<string>(
                    query,
                    new { banco = Banco, tipoDoc = TipoDoc }
                );
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }

            return resp;
        }

        public ErrorDto<TesArchivosEspecialesData> sbCargaArchivosEspeciales(int CodEmpresa, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesArchivosEspecialesData>
            {
                Code = 0,
                Result = new TesArchivosEspecialesData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Select UTILIZA_FORMATO_ESPECIAL,ARCHIVO_CHEQUES_FIRMAS,ARCHIVO_CHEQUES_SIN_FIRMAS from TES_BANCOS where ID_BANCO = @banco";
                    var archivosData = connection.QueryFirstOrDefault<TesBancosArchivosData>(query, new { banco = banco });

                    if (archivosData != null)
                    {
                        string archivoFirmas = Path.Combine(dirRDLC, CodEmpresa.ToString(), archivosData.archivo_cheques_firmas) ?? "";
                        string archivoSinFirmas = Path.Combine(dirRDLC, CodEmpresa.ToString(), archivosData.archivo_cheques_sin_firmas) ?? "";

                        if (archivosData.utiliza_formato_especial == 1)
                        {
                            if (File.Exists(archivoFirmas))
                                response.Result.chequesFirmas = archivosData.archivo_cheques_firmas;
                            else
                                response.Result.chequesFirmas = "Banking_DocFormat01"; //Reporte con Firmas

                            if (File.Exists(archivoSinFirmas))
                                response.Result.chequesSinFirmas = archivosData.archivo_cheques_sin_firmas;
                            else
                                response.Result.chequesSinFirmas = "Banking_DocFormat02"; //Reporte sin Firmas
                        }
                        else
                        {
                            response.Result.chequesFirmas = "Banking_DocFormat01"; //Reporte con Firmas
                            response.Result.chequesSinFirmas = "Banking_DocFormat02"; //Reporte sin Firmas
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

        public ErrorDto sbTesBancosAfectacion(int CodEmpresa, int vSolicitud, string vTipo = "E")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spTESAfectaBancos @solicitud, @tipo";

                    connection.Execute(query, new { solicitud = vSolicitud, tipo = vTipo });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select id_banco as item,descripcion from Tes_Bancos where Estado = 'A' 
                                order by descripcion";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto sbTesBitacoraEspecial(int CodEmpresa, int pSolicitud, string pMovimiento, string pDetalle, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spTesBitacora @solicitud, @movimiento, @detalle, @usuario";

                    connection.Execute(query,
                        new
                        {
                            solicitud = pSolicitud,
                            movimiento = pMovimiento,
                            detalle = pDetalle.Length > 150 ? pDetalle.Substring(0, 150) : pDetalle,
                            usuario = Usuario
                        });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTESCombos(string tipo)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                switch (tipo)
                {
                    case "estado":
                        resp.Result = new List<DropDownListaGenericaModel>
                        {
                            new DropDownListaGenericaModel { item = "T", descripcion = "Todos" },
                            new DropDownListaGenericaModel { item = "S", descripcion = "Solicitados" },
                            new DropDownListaGenericaModel { item = "E", descripcion = "Emitidos" },
                            new DropDownListaGenericaModel { item = "A", descripcion = "Anulados" }
                        };
                        break;
                    case "busqueda":
                        resp.Result = new List<DropDownListaGenericaModel>
                        {
                            new DropDownListaGenericaModel { item = "T", descripcion = "Todos" },
                            new DropDownListaGenericaModel { item = "1", descripcion = "Por Número de Caso / Solicitud" },
                            new DropDownListaGenericaModel { item = "2", descripcion = "Por Nombre Beneficiario" },
                            new DropDownListaGenericaModel { item = "3", descripcion = "Por Número de Documento" },
                            new DropDownListaGenericaModel { item = "4", descripcion = "Por Número de Referencia (OP)" }
                        };
                        break;
                    case "documento":
                        resp.Result = new List<DropDownListaGenericaModel>
                        {
                            new DropDownListaGenericaModel { item = "C", descripcion = "Cheques" },
                            new DropDownListaGenericaModel { item = "T", descripcion = "Transferencias" },
                            new DropDownListaGenericaModel { item = "R", descripcion = "Reporte" }
                        };
                        break;
                    default:
                        resp.Result = new List<DropDownListaGenericaModel>
                        {
                            new DropDownListaGenericaModel { item = "T", descripcion = "Todos" }
                        };
                        resp.Code = 0;
                        resp.Description = "No se encontró el pTipo de Combo que se desea llenar.";
                        break;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCboGeneral(int CodEmpresa, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();
            resp.Code = 0;
            resp.Result = new List<DropDownListaGenericaModel>();
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select rtrim(cod_unidad) as 'item',rtrim(descripcion) as 'descripcion'
                                 from CntX_Unidades where cod_contabilidad = @contabilidad ";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { contabilidad = contabilidad }).ToList();
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
        /// Obtener los tipos de documentos para carga de emisión de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Banco"></param>
        /// <param name="Tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCbo(int CodEmpresa, int Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select T.Tipo as 'item', rtrim(T.Descripcion) as 'descripcion'
                                from tes_banco_docs A inner join Tes_Tipos_Doc T on A.tipo = T.tipo
                                Where A.id_Banco = @banco";


                    query += " order by T.Descripcion";

                    resp.Result = connection
                        .Query<DropDownListaGenericaModel>(query, new { banco = Banco })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCboGeneral(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();
            resp.Code = 0;
            resp.Result = new List<DropDownListaGenericaModel>();
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select rtrim(cod_Concepto) as 'item', rtrim(Descripcion) as 'descripcion'
                                from Tes_Conceptos ";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public class ActualizaCCParams
        {
            public string? Codigo { get; set; }
            public string? Tipo { get; set; }
            public string? Documento { get; set; }
            public int Banco { get; set; }
            public object? OP { get; set; }
            public string? Modulo { get; set; }
            public string? SubModulo { get; set; }
            public int Referencia { get; set; }
        }

        public ErrorDto sbTESActualizaCC(int CodEmpresa, ActualizaCCParams parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = ""
            };
            try
            {
                if (parametros.Modulo?.Trim() != "CC" || parametros.SubModulo?.Trim() != "C")
                {
                    resp.Code = -1;
                    resp.Description = "Módulo o Submódulo inválido";
                    return resp;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    string? query;
                    if (parametros.Referencia > 0)
                    {
                        // TIENE REFERENCIA
                        query = @"UPDATE DesemBolsos 
                           SET Cod_Banco = @lngBanco,
                               TDocumento = @strTipo,
                               NDocumento = @strDoc
                           WHERE ID_Desembolso = @strCodigo";
                        connection.Execute(query, new { Banco = parametros.Banco, Tipo = parametros.Tipo, Documento = parametros.Documento, Codigo = parametros.Codigo });
                    }
                    else
                    {
                        // NO TIENE REFERENCIA
                        query = @"UPDATE Reg_Creditos 
                           SET Cod_Banco = @lngBanco,
                               Documento_Referido = @documentoReferido
                           WHERE ID_Solicitud = @lngOP";
                        string documentoReferido = $"{parametros.Tipo}-{parametros.Documento}";
                        connection.Execute(query, new { Banco = parametros.Banco, documentoReferido, OP = parametros.OP });
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public string fxTesParametro(int CodEmpresa, string xCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string result = "";
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = "select valor from tes_parametros where cod_parametro = @codigo";
                    result = connection.QueryFirst<string>(query, new { codigo = xCodigo });
                }
            }
            catch (Exception)
            {
                result = "";
            }
            return result;
        }

        public ErrorDto<bool> fxValidaEmpresaSinpe(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = "select sinpe_activo from SIF_EMPRESA WHERE PORTAL_ID = @empresa";
                    result.Result = connection.QueryFirst<bool>(query, new { empresa = CodEmpresa });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }
            return result;
        }

        public ErrorDto<bool> fxTesBancoValida(int CodEmpresa, int vBanco, string vUsuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select isnull(Count(*),0) as Existe
                        from Tes_Bancos B inner join tes_Banco_ASG A on B.id_Banco = A.id_Banco
                        and A.nombre = @usuario Where B.estado = 'A' and B.id_Banco = @banco";

                    var resp = connection.QueryFirstOrDefault<int>(query, new
                    {
                        usuario = vUsuario,
                        banco = vBanco
                    });

                    if (resp > 0)
                    {
                        result.Result = true;
                    }
                    else
                    {
                        result.Code = -1;
                        result.Result = false;
                    }

                }
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Result = false;
            }
            return result;
        }

        public ErrorDto<bool> fxTesConceptoValida(int CodEmpresa, int vBanco, string vUsuario, string vConcepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select isnull(Count(*),0) as Existe
                        from tes_conceptos_ASG A inner join Tes_Conceptos C on A.cod_concepto = C.cod_concepto
                        Where A.id_Banco = @banco and A.nombre = @usuario and A.cod_concepto = @concepto";

                    var resp = connection.QueryFirstOrDefault<int>(query, new
                    {
                        banco = vBanco,
                        usuario = vUsuario,
                        concepto = vConcepto
                    });

                    if (resp > 0)
                    {
                        result.Result = true;
                    }
                    else
                    {
                        result.Code = -1;
                        result.Result = false;
                    }

                }
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Result = false;
            }
            return result;
        }

        public ErrorDto<bool> fxTesUnidadValida(int CodEmpresa, int vBanco, string vUsuario, string vUnidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@" select isnull(Count(*),0) as Existe
        from tes_unidad_ASG A inner join CntX_Unidades C on A.cod_unidad = C.cod_unidad
        Where A.id_Banco = @banco and A.nombre = @usuario and A.cod_unidad = @unidad";

                    var resp = connection.QueryFirstOrDefault<int>(query, new
                    {
                        banco = vBanco,
                        usuario = vUsuario,
                        unidad = vUnidad
                    });

                    if (resp > 0)
                    {
                        result.Result = true;
                    }
                    else
                    {
                        result.Code = -1;
                        result.Result = false;
                    }

                }
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Result = false;
            }
            return result;
        }

        public ErrorDto<bool> fxTesDocumentoVerifica(int CodEmpresa, int vBanco, string vtipo, string vDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select isnull(count(*),0) as Existe from Tes_Transacciones 
where id_banco = @banco and tipo = @tipo and Ndocumento = @documento and estado <> 'P'";

                    var resp = connection.QueryFirstOrDefault<int>(query, new
                    {
                        banco = vBanco,
                        tipo = vtipo,
                        documento = vDocumento
                    });

                    if (resp > 0)
                    {
                        result.Code = -1;
                        result.Result = false;

                    }
                    else
                    {
                        result.Result = true;
                    }

                }
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Result = false;
            }
            return result;
        }

        public static string fxTESCifrado(string vClave)
        {
            var strPass = new System.Text.StringBuilder();

            for (int i = 0; i < vClave.Length; i++)
            {
                char c = vClave[i];
                char cifrado = (char)(c + 7);
                strPass.Append(cifrado);
            }

            return strPass.ToString();
        }

        public static string fxStringCifrado(string pCadena)
        {
            if (string.IsNullOrEmpty(pCadena))
                return string.Empty;

            var vResBuilder = new StringBuilder(pCadena.Length * 3);
            for (int i = pCadena.Length - 1; i >= 0; i--)
            {
                int xChar = (int)pCadena[i];
                vResBuilder.Append(xChar.ToString("D3"));
            }
            string vRes = vResBuilder.ToString();

            var deltas = new int[] { +1, -5, +7, -13, -2, +3 }; // ciclo de 6 pasos
            int vSec = 0;

            var vResXBuilder = new StringBuilder(vRes.Length + vRes.Length / 3);

            for (int i = 0; i < vRes.Length; i += 3)
            {
                int len = Math.Min(3, vRes.Length - i);
                if (!int.TryParse(vRes.AsSpan(i, len), out int num))
                    continue; 

                int transformed = num + deltas[vSec];
                vResXBuilder.Append(transformed);

                vSec++;
                if (vSec == deltas.Length) vSec = 0;
            }

            return FxDepuraCadena(vResXBuilder.ToString());
        }

        public static string FxDepuraCadena(string xCadena)
        {
            var vResBuilder = new System.Text.StringBuilder();

            for (int i = 0; i < xCadena.Length; i += 2)
            {
                string chunk = xCadena.Substring(i, Math.Min(2, xCadena.Length - i));
                if (int.TryParse(chunk, out int num) && num > 31 && num != 39 && num != 34)
                {
                    vResBuilder.Insert(0, (char)num);
                }
            }

            return vResBuilder.ToString();
        }

        /// <summary>
        /// Metodo para validar si un usuario tiene permiso para un banco y tipo de documento específico.
        /// Campo Permiso:
        /// "Solicita", "Autoriza", "Genera", "Asientos", "Anula"
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vtipo"></param>
        /// <param name="vUsuario"></param>
        /// <param name="vPermiso"></param>
        /// <returns></returns>
        public bool fxValidaPermisoUserBancosTipo(int CodEmpresa, int vBanco, string vtipo, string vUsuario, string vPermiso)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            bool result = false;
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select count(T.Tipo) from tes_tipos_doc T left join tes_documentos_asg A on T.tipo = A.tipo
                                    and A.id_banco = @banco and A.nombre = @usuario
                                    Where T.tipo in(select Tipo from tes_banco_docs where id_banco = @banco)
                                    AND T.Tipo = @tipo AND isnull(A.{vPermiso},0) = 1";

                    var resp = connection.QueryFirstOrDefault<int>(query, new
                    {
                        tipo = vtipo,
                        banco = vBanco,
                        usuario = vUsuario
                    });

                    if (resp > 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public ErrorDto sbTesEmitirDocumento(
            int CodEmpresa, string vUsuario, int vModulo, int vSolicitud, string vDocumento = "", DateTime? vFecha = null)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = ""
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = @"SELECT C.monto, C.nsolicitud, T.doc_auto, T.comprobante,
                          ISNULL(B.firmas_desde,0) AS Firmas_Desde, B.Lugar_Emision,
                          X.descripcion AS TipoX, ISNULL(B.firmas_hasta,0) AS Firmas_Hasta,
                          dbo.MyGetdate() AS FechaX, C.id_Banco, C.tipo, C.modulo, 
                          C.op, C.referencia, C.codigo, C.subModulo, C.cod_divisa
                           FROM Tes_Transacciones C
                           INNER JOIN Tes_Bancos B ON C.id_Banco = B.id_banco
                           INNER JOIN tes_banco_docs T ON B.id_Banco = T.id_Banco AND C.tipo = T.tipo
                           INNER JOIN tes_tipos_doc X ON T.tipo = X.tipo
                           WHERE C.nsolicitud = @solicitud";

                    var data = connection.QueryFirstOrDefault<MTesTransaccionDto>(query, new { solicitud = vSolicitud });

                    if (data == null)
                    {
                        response.Code = -3;
                        response.Description = "No se encontró la solicitud especificada.";
                        return response;
                    }

                    string vTipo = data.tipo;
                    string vComprobante = data.comprobante;
                    bool vAutoConsec = data.doc_auto;
                    DateTime fechaEmision = vFecha ?? data.fechaX;
                    string vConsecutivo = string.Empty;

                    switch (vComprobante)
                    {
                        case "01":
                        case "02":
                        case "03":
                            if (vAutoConsec)
                                vConsecutivo = fxTesTipoDocConsec(CodEmpresa, data.id_banco, vTipo, "+").Result.ToString();

                            string nDocumentoClause = "";
                            if (vAutoConsec)
                                nDocumentoClause = ", NDocumento = @documento";
                            else if (!string.IsNullOrWhiteSpace(vDocumento))
                                nDocumentoClause = ", NDocumento = @documento";

                            var updateSql = @"
                                UPDATE Tes_Transacciones
                                SET Estado = 'I',
                                    Fecha_Emision = @fecha,
                                    Ubicacion_Actual = 'T',
                                    Fecha_Traslado = @fecha,
                                    User_Genera = @usuario"
                                    + nDocumentoClause +
                                    " WHERE nsolicitud = @solicitud";
                            connection.Execute(updateSql, new
                            {
                                fecha = fechaEmision.ToString("yyyy-MM-dd"),
                                usuario = vUsuario,
                                documento = vAutoConsec ? vConsecutivo : vDocumento,
                                solicitud = vSolicitud
                            });

                            sbTESActualizaCC(
                                    CodEmpresa,
                                    new ActualizaCCParams
                                    {
                                        Codigo = data.codigo,
                                        Tipo = vTipo,
                                        Documento = vConsecutivo,
                                        Banco = data.id_banco,
                                        OP = data.op != null ? (int)data.op : 0,
                                        Modulo = data.modulo,
                                        SubModulo = data.subModulo,
                                        Referencia = data.referencia != null ? (int)data.referencia : 0
                                    }
                                );
                            //Envió a impresión
                            break;
                        case "04":
                            response.Code = -1;
                            response.Description = "Las Transferencias Electrónicas no se pueden procesar directamente...";
                            break;
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


        public string fxTesTiposDocAsiento(int CodEmpresa, string vTipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = "";

            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select Movimiento from tes_tipos_doc Where tipo = @tipo ";

                    var resp = connection.QueryFirstOrDefault<string>(query, new
                    {
                        tipo = vTipo
                    });
                    response = resp ?? "";
                }
            }
            catch (Exception)
            {
                response = "";
            }

            return response;
        }

        public ErrorDto<bool> fxTesCuentaObligatoriaVerifica(int CodEmpresa, int vBanco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select isnull(count(*),0) as Existe from Tes_Bancos
                            where id_banco = @banco and INT_REQUIERE_CUENTA_DESTINO = 1 ";

                    var resp = connection.QueryFirstOrDefault<int>(query, new
                    {
                        banco = vBanco
                    });

                    if (resp > 0)
                    {
                        result.Code = -1;
                        result.Result = true;

                    }
                    else
                    {
                        result.Result = false;
                    }

                }
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Result = false;
            }
            return result;
        }

        public static string fxTesMesDescripcion(int vMes)
        {
            switch (vMes)
            {
                case 1:
                    return "ENERO";
                case 2:
                    return "FEBRERO";
                case 3:
                    return "MARZO";
                case 4:
                    return "ABRIL";
                case 5:
                    return "MAYO";
                case 6:
                    return "JUNIO";
                case 7:
                    return "JULIO";
                case 8:
                    return "AGOSTO";
                case 9:
                    return "SETIEMBRE";
                case 10:
                    return "OCTUBRE";
                case 11:
                    return "NOVIEMBRE";
                case 12:
                    return "DICIEMBRE";

            }

            return "";
        }

        public ErrorDto<TesReporteTransferenciaDto> sbTesReporteTransferencia(
        int CodEmpresa, int vBanco, long vTransac, string? vTipo = "C",
        string? vDocumento = "TE", string? vPlan = "-sp-")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<TesReporteTransferenciaDto>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesReporteTransferenciaDto()
            };

            decimal curMonto = 0;
            long lngCasos = 0;
            string strDivisa = "", vLetra = "";

            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = @"select cta as item, descripcion from Tes_Bancos where id_banco = @vBanco";
                    var banco = connection.QueryFirstOrDefault(query, new { vBanco });

                    if (banco != null)
                    {
                        vLetra = "Sirva la Presente para saludarlo y a la vez solicitarle debitar de nuestra cuenta corriente"
                               + " # " + banco.item + " la suma de ¢ ";
                    }

                    if (vTipo == "C")
                    {
                        string strSQL = @"select sum(Monto) as Monto, Count(*) as Casos, cod_divisa
                                  from Tes_Transacciones
                                  where tipo = @vDocumento and id_banco = @vBanco and documento_Base = @vTransac";
                        if (vPlan != "-sp-")
                        {
                            strSQL += " and Cod_Plan = @vPlan";
                        }
                        strSQL += " group by cod_divisa";

                        var rs = connection.QueryFirstOrDefault(strSQL, new { vDocumento, vBanco, vTransac, vPlan });
                        if (rs != null)
                        {
                            curMonto = rs.Monto;
                            lngCasos = rs.Casos;
                            strDivisa = rs.cod_divisa;
                        }

                        string vMontoLetras = MProGrXAuxiliarDB.NumeroALetras(curMonto).Result
                                              + fxDescDivisa(CodEmpresa, strDivisa).Result;

                        resp.Result.registros = lngCasos;
                        resp.Result.montoLetras = vMontoLetras;
                        resp.Result.totalMonto = curMonto;
                        resp.Result.fxNombre = fxTesParametro(CodEmpresa, "01");
                        resp.Result.fxPuesto = fxTesParametro(CodEmpresa, "02");
                        resp.Result.fxDepartamento = fxTesParametro(CodEmpresa, "03");
                        resp.Result.letras1 = vLetra;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = new TesReporteTransferenciaDto();
            }

            return resp;
        }


        public ErrorDto<string> fxDescDivisa(int CodEmpresa, string vDivisa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<string>()
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select top 1 descripcion from CNTX_DIVISAS where cod_divisa = @vDivisa";
                    string descripcion = connection.QueryFirstOrDefault<string>(query, new { vDivisa }) ?? "";

                    if (!string.IsNullOrEmpty(descripcion))
                    {
                        string strDescripcion = descripcion.Trim().ToLower();

                        // Tomar la primera palabra
                        string fxCodText = strDescripcion.Split(' ')[0].Trim();

                        if (string.IsNullOrEmpty(fxCodText))
                        {
                            fxCodText = strDescripcion;
                        }

                        // Normalizar capitalización
                        fxCodText = char.ToUpper(fxCodText[0]) + fxCodText.Substring(1);

                        // Normalizar capitalización
                        fxCodText = char.ToUpper(fxCodText[0]) + fxCodText.Substring(1);
                        fxCodText = fxCodText.Trim();

                        char ultima = fxCodText[fxCodText.Length - 1];
                        // Regla básica: vocal → "s", consonante → "es" (pluralización)
                        if ("aeiouáéíóú".Contains(char.ToLower(ultima)))
                        {
                            fxCodText += "s";
                        }
                        else
                        {
                            fxCodText += "es";
                        }

                        resp.Result = string.IsNullOrEmpty(fxCodText) ? strDescripcion : fxCodText;
                    }
                    else
                    {
                        resp.Result = " Colones";
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }
            return resp;
        }


        public ErrorDto<List<TokenConsultaModel>> spTes_Token_Consulta(int CodEmpresa, string usuario)
        {
            var response = new ErrorDto<List<TokenConsultaModel>> { Code = 0, Result = new() };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using (var connection = new SqlConnection(conn))
                {
                    var query = $@"exec spTes_Token_Consulta '', 'A' , @usuario";
                    response.Result = connection.Query<TokenConsultaModel>(query, new { usuario = usuario }).ToList();
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

        public ErrorDto spTes_Token_New(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var insert = connection.Execute(
                            "spTes_Token_New",
                            new { Usuario = usuario.ToUpper() },
                            commandType: CommandType.StoredProcedure
                        );

                    if (insert != -1)
                    {
                        //busco el ultimo token generado
                        var query = $@"select top 1 ID_TOKEN from Tes_Tokens where REGISTRO_USUARIO = @usuario order by REGISTRO_FECHA desc";
                        var token = connection.QueryFirstOrDefault<string>(query, new { usuario = usuario.ToUpper() });
                        response.Description = token;
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

        public void sbCrdOperacionTags(
            int CodEmpresa,
            long pOperacion,
            string pLinea,
            string pTag,
            string pUsuario,
            string? pAsignado = "",
            string? pNotas = "")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Verificar si el token está activo
                    var procedure = $@"[spCrdOperacionTagRegistra]";
                    var values = new
                    {
                        Operacion = pOperacion,
                        CrdLinea = pLinea,
                        Tag = pTag,
                        Usuario = pUsuario,
                        Asignado = pAsignado,
                        Notas = pNotas
                    };

                    connection.ExecuteAsync(procedure, values, commandType: System.Data.CommandType.StoredProcedure);

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

        }

    }
}
