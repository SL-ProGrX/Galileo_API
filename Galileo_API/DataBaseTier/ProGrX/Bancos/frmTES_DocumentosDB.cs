using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.Diagnostics.Internal;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesDocumentosDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 9;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmTesDocumentosDB(IConfiguration? config)
        {
            _portalDB = new PortalDB(config!);
            _Security_MainDB = new MSecurityMainDb(config!);
        }

        /// <summary>
        /// Obtener lista de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_DocumentosLista_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select Tipo as 'item',descripcion from tes_tipos_doc order by descripcion asc";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtener un tipo de documento por su código (tipo) mediante scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="scroll"></param>
        /// <returns></returns>
        public ErrorDto<TesTiposDocDto> Tes_Documentos_Scroll(int CodEmpresa, string tipo, int? scroll)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var tipoScroll = scroll ?? 1; // 1 = anterior, 2 = siguiente

                const string sql = @"
            SELECT TOP 1
                t.*,
                c.DESCRIPCION AS tipo_asiento_desc
            FROM tes_tipos_doc t
            LEFT JOIN CNTX_TIPOS_ASIENTOS c
                   ON t.TIPO_ASIENTO = c.TIPO_ASIENTO
            WHERE
                (
                    @scroll = 1 AND t.Tipo < @tipo
                )
                OR
                (
                    @scroll = 2 AND t.Tipo > @tipo
                )
            ORDER BY
                CASE WHEN @scroll = 1 THEN t.Tipo END DESC,
                CASE WHEN @scroll = 2 THEN t.Tipo END ASC;";

                var response = conn.QueryFirstOrDefault<TesTiposDocDto>(sql, new
                {
                    scroll = tipoScroll,
                    tipo
                }) ?? new TesTiposDocDto();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesTiposDocDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener un tipo de documento por su código (tipo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<TesTiposDocDto> Tes_Documentos_Obtener(int CodEmpresa, string tipo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select t.*, c.DESCRIPCION as tipo_asiento_desc from tes_tipos_doc t
                                   left join CNTX_TIPOS_ASIENTOS c ON t.TIPO_ASIENTO = c.TIPO_ASIENTO where tipo = @tipo";

                return conn.QueryFirstOrDefault<TesTiposDocDto>(query, new { tipo = tipo }) ?? new TesTiposDocDto();
            });
        }


        /// <summary>
        /// Obtener lista de tipos de asientos para documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_DocumentosTiposAsientos_Obtener(int CodEmpresa, int contabilidad)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select Tipo_Asiento as 'item',descripcion from CNTX_TIPOS_ASIENTOS
                                    where cod_contabilidad = @contabilidad AND ACTIVO = 1
                                    order by descripcion asc";

                return conn.Query<DropDownListaGenericaModel>(query, new { contabilidad = contabilidad }).ToList();
            });
        }

        /// <summary>
        /// Obtener un concepto de anulación de documentos por su código (concepto) mediante scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="concepto"></param>
        /// <param name="scroll"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel> Tes_DocAnulaConceptos_Scroll(int CodEmpresa, string concepto, int? scroll)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var tipoScroll = scroll ?? 1; // 1 = anterior, 2 = siguiente

                const string sql = @"
            SELECT TOP 1
                Tipo_Asiento AS item,
                descripcion
            FROM CNTX_TIPOS_ASIENTOS
            WHERE
                (
                    @scroll = 1 AND Tipo_Asiento < @concepto
                )
                OR
                (
                    @scroll = 2 AND Tipo_Asiento > @concepto
                )
            ORDER BY
                CASE WHEN @scroll = 1 THEN Tipo_Asiento END DESC,
                CASE WHEN @scroll = 2 THEN Tipo_Asiento END ASC;";

                var response = conn.QueryFirstOrDefault<DropDownListaGenericaModel>(sql, new
                {
                    scroll = tipoScroll,
                    concepto
                }) ?? new DropDownListaGenericaModel();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<DropDownListaGenericaModel>(ex.Message);
            }
        }

        /// <summary>
        /// Guardar o actualizar un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="documento"></param>
        /// <returns></returns>
        public ErrorDto TES_Documentos_Guardar(int CodEmpresa, string usuario, TesTiposDocDto documento)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var validation = ValidarDocumento(documento);
                if (validation.Code != 0) return validation;

                var existe = ExisteTipoDocumento(conn, documento.tipo!);
                var parameters = BuildDocumentoParams(documento, usuario);

                if (existe)
                {
                    ActualizarDocumento(conn, parameters);
                    RegistrarBitacora(CodEmpresa, usuario, documento.tipo!, "Registra - Web");
                }
                else
                {
                    InsertarDocumento(conn, parameters);
                    RegistrarBitacora(CodEmpresa, usuario, documento.tipo!, "Modifica - Web");
                }

                return new ErrorDto { Code = 0, Description = "Guardado correctamente" };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto ValidarDocumento(TesTiposDocDto documento)
        {
            if (documento == null)
                return DbHelper.ErrorResponse("El documento no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(documento.tipo))
                return DbHelper.ErrorResponse("El tipo de documento no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(documento.descripcion))
                return DbHelper.ErrorResponse("La descripción no puede ser nula.");

            if (string.IsNullOrWhiteSpace(documento.movimiento))
                return DbHelper.ErrorResponse("El movimiento no puede ser nulo.");

            return DbHelper.CreateOkResponse();
        }

        private static bool ExisteTipoDocumento(System.Data.IDbConnection conn, string tipo)
        {
            const string sql = "SELECT COUNT(1) FROM tes_tipos_doc WHERE tipo = @tipo";
            return conn.ExecuteScalar<int>(sql, new { tipo }) > 0;
        }

        private static object BuildDocumentoParams(TesTiposDocDto documento, string usuario)
        {
            return new
            {
                Tipo = documento.tipo!.Trim(),
                Descripcion = (documento.descripcion ?? string.Empty).ToUpper().Trim(),
                Movimiento = (documento.movimiento ?? string.Empty).Trim().Substring(0, 1),
                Generacion = documento.generacion ? 1 : 0,
                TipoAsiento = documento.tipo_asiento,
                AsientoTransac = documento.asiento_transac ? 1 : 0,
                AsientoFormato = documento.asiento_formato ? 1 : 0,
                AsientoBanco = documento.asiento_banco ? 1 : 0,
                AsientoMascara = documento.asiento_mascara?.Trim(),
                Usuario = usuario,
                TipoIdentificacion = documento.int_reclasifica_id ? 1 : 0
            };
        }

        private static void ActualizarDocumento(System.Data.IDbConnection conn, object parameters)
        {
            const string sql = @"
        UPDATE tes_tipos_doc 
        SET 
            descripcion = @Descripcion,
            movimiento = @Movimiento,
            generacion = @Generacion,
            tipo_asiento = @TipoAsiento,
            asiento_transac = @AsientoTransac,
            asiento_formato = @AsientoFormato,
            asiento_banco = @AsientoBanco,
            asiento_mascara = @AsientoMascara,
            MODIFICA_USUARIO = @Usuario,
            MODIFICA_FECHA = GETDATE(),
            INT_RECLASIFICA_ID = @TipoIdentificacion
        WHERE tipo = @Tipo;";

            conn.Execute(sql, parameters);
        }

        private static void InsertarDocumento(System.Data.IDbConnection conn, object parameters)
        {
            const string sql = @"
        INSERT INTO tes_tipos_doc 
            (tipo, descripcion, movimiento, generacion, tipo_asiento,
             asiento_transac, asiento_banco, asiento_formato, asiento_mascara, 
             REGISTRO_USUARIO, REGISTRO_FECHA, INT_RECLASIFICA_ID)
        VALUES 
            (@Tipo, @Descripcion, @Movimiento, @Generacion, @TipoAsiento,
             @AsientoTransac, @AsientoBanco, @AsientoFormato, @AsientoMascara, 
             @Usuario, GETDATE(), @TipoIdentificacion);";

            conn.Execute(sql, parameters);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string tipoDocumento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Tipo de Documento : {tipoDocumento}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Eliminar un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto TES_Documentos_Eliminar(int CodEmpresa, string tipo,string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Eliminado correctamente"
            };
            try
            {
                var sql = @"DELETE FROM tes_tipos_doc WHERE tipo = @Tipo";
                conn.Execute(sql, new { Tipo = tipo });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Documento : {tipo}",
                    Movimiento = "Elimina - Web",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Obtener lista de conceptos de anulación de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<TesDocAnulaConceptosData>> TES_DocAnulaConceptos_Obtener(int CodEmpresa, string tipo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select ID_CONCEPTO_ANULA as 'id_conceptos', DESCRIPCION, ACTIVO FROM TES_ANULA_CONCEPTOS  WHERE TIPO = @tipo";

                return conn.Query<TesDocAnulaConceptosData>(query, new { tipo }).ToList();
            });
        }

        /// <summary>
        /// Guardar o actualizar un concepto de anulación de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDto TES_DocAnulaConcepto_Guardar(int CodEmpresa, string usuario ,string tipo, TesDocAnulaConceptosData concepto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Guardado correctamente"
            };
            try
            {
                var proc = $@"exec spTes_Anula_Conceptos_Add @Id, @Tipo, @Descripcion, @Activo, @Usuario";
                var parametros = new
                {
                    Id = concepto?.id_conceptos ?? 0,
                    Tipo = tipo,
                    Descripcion = concepto?.descripcion,
                    Activo = (concepto!.activo) ? 1 : 0,
                    Usuario = usuario,
                };

                var resp = conn.Query<TesDocAnulaConcepRespuesta>(proc, parametros).FirstOrDefault();

                if (resp?.pass == 1)
                {
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto de Anulación de Documentos de Bancos Id: {resp.codigo} - {concepto.descripcion}",
                        Movimiento = resp.movimiento!,
                        Modulo = vModulo
                    });
                }
                else
                {
                    response.Code = -1;
                    response.Description = resp!.mensaje;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Eliminar un concepto de anulación de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_conceptos"></param>
        /// <returns></returns>
        public ErrorDto TES_DocAnulaConcepto_Eliminar(int CodEmpresa, int id_conceptos, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Eliminado correctamente"
            };
            try
            {
                var sql = @"exec spTes_Anula_Conceptos_Delete @id , @usuario";
                var resp = conn.Query<TesDocAnulaConcepRespuesta>(sql, new { id = id_conceptos, usuario }).FirstOrDefault();

                if (resp!.pass == 1)
                {
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto de Anulación de Documentos de Bancos Id: {id_conceptos}",
                        Movimiento = resp.movimiento!,
                        Modulo = vModulo
                    });
                }
                else
                {
                    response.Code = -1;
                    response.Description = resp!.mensaje;
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
