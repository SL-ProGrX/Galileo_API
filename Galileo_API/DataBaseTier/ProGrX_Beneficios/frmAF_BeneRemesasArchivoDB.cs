using PgxAPI.Models.AF;
using Microsoft.Data.SqlClient;
using Dapper;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneRemesasArchivoDB
    {
        private readonly IConfiguration? _config;
        private readonly mProGrX_AuxiliarDB mProGrX_Auxiliar ;

        public frmAF_BeneRemesasArchivoDB(IConfiguration config)
        {
            _config = config;
            mProGrX_Auxiliar = new mProGrX_AuxiliarDB(_config);
        }

        public ErrorDto<List<TipoDocumentosLista>> TipoDocumentos_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<TipoDocumentosLista>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "SELECT IdTipoDocumento as item, Nombre as descripcion FROM RMS_TiposDocumentos WHERE ACTIVO = 1";
                    response.Result = connection.Query<TipoDocumentosLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

        }
    
        public ErrorDto<RmsRemesasDataLista> RemesasArchivo_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<RmsRemesasDataLista>
            {
                Code = 0,
                Result = new RmsRemesasDataLista()
            };

            response.Result.total = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "SELECT * FROM RMS_Remesas WHERE CodDepartamentoOrigen = 38 AND IdEstado = 6";
                    response.Result.lista = connection.Query<RmsRemesasData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto<List<RmsRemesaDocuementos>> RemesaDocumentos_Obtener(int CodCliente, string filtros) 
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<RmsRemesaDocuementos>>();
;           response.Code = 0;

            RmsCargaFiltros rmsCarga = JsonConvert.DeserializeObject<RmsCargaFiltros>(filtros);

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string fechaIni = mProGrX_Auxiliar.validaFechaGlobal(rmsCarga.fecha_inicio);
                    string fechaCorte = mProGrX_Auxiliar.validaFechaGlobal(rmsCarga.fecha_corte);

                    var query = $@"SELECT abo.ID_BENEFICIO , 
                                    CONCAT(Format(abo.ID_BENEFICIO, '00000'), Trim(abo.COD_BENEFICIO), Format(abo.CONSEC, '00000')) as n_expediente,
                                    abo.REGISTRA_FECHA ,
                                    abo.REGISTRA_USER , 
                                    abo.ESTADO,
                                    (select descripcion FROM AFI_BENE_ESTADOS abe WHERE abe.COD_ESTADO =  abo.ESTADO) as estado_desc, 
                                    abo.CEDULA ,
                                    (select DISTINCT nombre from socios where cedula = abo.CEDULA ) as nombre,
                                    '' as notaOrigen 
                                    FROM AFI_BENE_OTORGA abo WHERE 
                                    CAST(abo.ID_BENEFICIO as VARCHAR(30)) NOT IN (
                                     SELECT Documento FROM RMS_RemesasDetalle WHERE IdRemesa IN (SELECT IdRemesa FROM RMS_Remesas WHERE CodDepartamentoOrigen = 38 AND IdEstado = 6)
                                    ) AND abo.REGISTRA_FECHA BETWEEN '{fechaIni}' AND '{fechaCorte}' 
                                    AND abo.COD_BENEFICIO IN (
	                                    SELECT COD_BENEFICIO FROM AFI_BENEFICIOS ab 
                                        WHERE ab.COD_CATEGORIA = '{rmsCarga.cod_categoria}'
                                    )";
                    response.Result = connection.Query<RmsRemesaDocuementos>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

        }

        public ErrorDto<RmsRemesasDetalleDataLista> RemesaDetalle_Obtener(int CodCliente, int IdRemesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<RmsRemesasDetalleDataLista>
            {
                Code = 0,
                Result = new RmsRemesasDetalleDataLista()
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM RMS_RemesasDetalle WHERE IdRemesa = {IdRemesa} ";
                    response.Result.lista = connection.Query<RmsRemesasDetalleData>(query).ToList();
                    response.Result.total = response.Result.lista.Count; 
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto RemesaArchivo_Guardar(int CodCliente, RmsRemesasData remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();
            string origen = _config.GetSection("AFI_Beneficios").GetSection("BeneDepOrigen").Value.ToString();
            string destino = _config.GetSection("AFI_Beneficios").GetSection("BeneDepDestino").Value.ToString();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";
                    if (remesa.IdRemesa == 0)
                    {
                        //Busco el codigo para la remesa
                        var queryCodRemesa = $@"SELECT ISNULL(MAX(CodRemesa), 0) + 1 
                                                FROM RMS_Remesas WHERE IdTipoDocumento = '{remesa.IdTipoDocumento}' ";
                        int codRemesa = connection.QueryFirstOrDefault<int>(queryCodRemesa);

                        query = $@"INSERT INTO RMS_Remesas 
                                    (CodRemesa, IdTipoDocumento, CodDepartamentoOrigen, CodDepartamentoDestino, RegistroUsuario, RegistroFecha , NotaOrigen, IdEstado, Activa) 
                                    VALUES ({codRemesa}, {remesa.IdTipoDocumento}, {origen} , {destino}, '{remesa.RegistroUsuario}', getdate(), '{remesa.NotaOrigen}', 6, 0 )";
                    }
                    else
                    {
                        query = $@"UPDATE RMS_Remesas SET NotaOrigen = '{remesa.NotaOrigen}' 
                                    WHERE IdRemesa = {remesa.IdRemesa}";
                    }

                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto RemesaDetalle_Guardar(int CodCliente, int idRemesa,string usuario , List<RmsRemesaDocuementos> documentos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    foreach (var item in documentos)
                    {

                        string fecha = mProGrX_Auxiliar.validaFechaGlobal(item.registra_fecha);

                        var query = $@"INSERT INTO ASECCSS.dbo.RMS_RemesasDetalle
                                        (
                                        IdRemesa,
                                        Documento,
                                        DocumentoRegistroFecha,
                                        DocumentoRegistroUsuario,
                                        DocumentoIdAsociado,
                                        DocumentoNombreAsociado,
                                        RegistroUsuario,
                                        RegistroFecha,
                                        IdEstado,
                                        Mascara
                                        )
                                        VALUES
                                        (
                                        {idRemesa},
                                        '{item.id_beneficio}',
                                        '{fecha}',
                                        '{item.registra_user}',
                                        '{item.cedula}',
                                        '{item.nombre}',
                                        '{usuario}',
                                        getdate(),
                                        6,
                                        '{item.n_expediente}')";

                        connection.Execute(query);
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