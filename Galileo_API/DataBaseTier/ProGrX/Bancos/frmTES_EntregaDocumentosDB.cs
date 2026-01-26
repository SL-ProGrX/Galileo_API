using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier
{
    public class FrmTesEntregaDocumentosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesEntregaDocumentosDB(IConfiguration config)
        {
            DBBitacora = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaBancosDocumentos>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = "SELECT id_banco, descripcion FROM Tes_Bancos WHERE estado = 'A'";

                return conn.Query<DropDownListaBancosDocumentos>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene los tipos de documentos de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_Banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaTiposDocumentos>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
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

                return conn.Query<DropDownListaTiposDocumentos>(query, new { CodBanco = cod_Banco }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista pendiente de entrega
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<EntregaDocumentoPendientesDto>> listaPendientes_Obtener(int CodEmpresa, string filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var filtro = JsonConvert.DeserializeObject<TesEntregaDocumentosFiltros>(filtros)
                         ?? new TesEntregaDocumentosFiltros();

            try
            {
                const string query = @"
                            SELECT 
                                nsolicitud,
                                ndocumento,
                                beneficiario,
                                monto,
                                fecha_emision
                            FROM Tes_Transacciones
                            WHERE 
                                id_banco = @IdBanco
                                AND tipo = @Tipo
                                AND user_entrega IS NULL
                                AND estado <> 'P'
                                AND (
                                    @TodasFechas = 1
                                    OR fecha_emision BETWEEN @FechaInicio AND @FechaFin
                                )
                            ORDER BY nsolicitud ASC;";

                var parameters = new
                {
                    IdBanco = filtro.id_banco,
                    Tipo = filtro.tipo_doc,
                    TodasFechas = filtro.todas_fechas ? 1 : 0,
                    FechaInicio = filtro.fecha_desde,
                    FechaFin = filtro.fecha_hasta
                };

                var response = conn.Query<EntregaDocumentoPendientesDto>(query, parameters).ToList();
                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<EntregaDocumentoPendientesDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda la entrega del documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="trasladoLista"></param>
        /// <param name="estadoCheck"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto TES_documentosPendientes_Guardar(int CodEmpresa, string trasladoLista, string estadoCheck, string usuario)
        {
            var lista = new List<int>();

            var texto = trasladoLista?.Trim();

            if (!string.IsNullOrWhiteSpace(texto) && texto.StartsWith('[')) // <-- S6610: char overload
            {
                lista = JsonConvert.DeserializeObject<List<int>>(texto) ?? new List<int>();
            }
            else if (int.TryParse(texto, out var numero))
            {
                lista.Add(numero);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (lista.Count == 0)
                    return DbHelper.OkResponse("Sin registros para actualizar");

                if (estadoCheck == "1")
                {
                    const string query = @"
                                UPDATE Tes_Transacciones
                                SET user_entrega = @usuario,
                                    fecha_entrega = dbo.MyGetdate()
                                WHERE nsolicitud IN @lista;";

                    conn.Execute(query, new { usuario, lista });
                }
                else
                {
                    const string query = @"
                                UPDATE Tes_Transacciones
                                SET user_entrega = NULL,
                                    fecha_entrega = NULL
                                WHERE nsolicitud IN @lista;";

                    conn.Execute(query, new { lista });
                }

                return DbHelper.OkResponse("Registro guardado correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }


    }
}