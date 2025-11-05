using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_EntregaDocumentosDB
    {
        private readonly IConfiguration? _config;
        private mSecurityMainDb DBBitacora;

        public frmTES_EntregaDocumentosDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaBancosDocumentos>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaBancosDocumentos>>
            {
                Code = 0,
                Result = new List<DropDownListaBancosDocumentos>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT id_banco, descripcion FROM Tes_Bancos WHERE estado = 'A'";
                    response.Result = connection.Query<DropDownListaBancosDocumentos>(query).ToList();
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
        /// Obtiene los tipos de documentos de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_Banco"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaTiposDocumentos>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaTiposDocumentos>>
            {
                Code = 0,
                Result = new List<DropDownListaTiposDocumentos>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string query = @"
            SELECT 
                RTRIM(T.Tipo) + ' - ' + RTRIM(T.Descripcion) AS ItmY,
                T.Tipo AS IdX,
                RTRIM(T.Descripcion) AS ItmX
            FROM 
                tes_banco_docs A
            INNER JOIN 
                Tes_Tipos_Doc T ON A.tipo = T.tipo
            WHERE 
                A.ID_BANCO = @CodBanco";

                response.Result = connection
                    .Query<DropDownListaTiposDocumentos>(query, new { CodBanco = cod_Banco })
                    .ToList();
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
        /// Obtiene la lista pendiente de entrega
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<EntregaDocumentoPendientesDTO>> listaPendientes_Obtener(int CodEmpresa, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            TES_EntregaDocumentosFiltros filtro = JsonConvert.DeserializeObject<TES_EntregaDocumentosFiltros>(filtros) ?? new TES_EntregaDocumentosFiltros();

            var response = new ErrorDTO<List<EntregaDocumentoPendientesDTO>>
            {
                Code = 0,
                Result = new List<EntregaDocumentoPendientesDTO>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string query = @"
                                SELECT 
                                    nsolicitud,
                                    ndocumento,
                                    beneficiario,
                                    monto,
                                    fecha_emision
                                FROM 
                                    Tes_Transacciones
                                WHERE 
                                    id_banco = @IdBanco
                                    AND tipo = @Tipo
                                    AND user_entrega IS NULL
                                    AND estado <> 'P'
                            ";

                if (!filtro.todas_fechas)
                {
                    query += " AND fecha_emision BETWEEN @FechaInicio AND @FechaFin";
                }
                query += " ORDER BY nsolicitud ASC";

                var parameters = new
                {
                    IdBanco = filtro.id_banco,
                    Tipo = filtro.tipo_doc,
                    FechaInicio = filtro.fecha_desde,
                    FechaFin = filtro.fecha_hasta
                };

                response.Result = connection.Query<EntregaDocumentoPendientesDTO>(query, parameters).ToList();

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
        /// Guarda la entrega del documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="trasladoLista"></param>
        /// <param name="estadoCheck"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO TES_documentosPendientes_Guardar(int CodEmpresa, string trasladoLista, string estadoCheck, string usuario)
        {
            List<int> lista;

            if (!string.IsNullOrWhiteSpace(trasladoLista) && trasladoLista.Trim().StartsWith("["))
            {
                lista = JsonConvert.DeserializeObject<List<int>>(trasladoLista) ?? new List<int>();
            }
            else
            {
                if (int.TryParse(trasladoLista, out int numero))
                    lista = new List<int> { numero };
                else
                    lista = new List<int>();
            }
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query;

                    foreach (var solicitud in lista)
                    {
                        if (estadoCheck == "1") 
                        {
                            query = @"UPDATE Tes_Transacciones  SET user_entrega = @usuario, 
                                  fecha_entrega = dbo.MyGetdate() WHERE nsolicitud = @nsolicitud";
                            connection.Execute(query, new
                            {
                                usuario = usuario,
                                nsolicitud = solicitud
                            });
                        }
                        else 
                        {
                            query = @"UPDATE Tes_Transacciones SET user_entrega = NULL, fecha_entrega = NULL 
                            WHERE nsolicitud = @nsolicitud";
                            connection.Execute(query, new
                            {
                                nsolicitud = solicitud
                            });
                        }
                    }

                    response.Description = "Registro guardado correctamente";
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