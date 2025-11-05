using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_BancosDocDB
    {
        private readonly IConfiguration? _config;
        private readonly mSecurityMainDb BitacoraDb;
        private readonly int vModulo = 9;

        public frmTES_BancosDocDB(IConfiguration config)
        {
            _config = config;
            BitacoraDb = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Carga el combo de grupos de documentos bancarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_BancoDocGrupos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_GRUPO as 'item', DESCRIPCION as 'descripcion'
                                      From TES_BANCOS_GRUPOS Where ACTIVO = 1";

                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }
            return response;
        }

        /// <summary>
        /// Carga el combo de bancos según el grupo especificado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_BancoDocBancos_Obtener(int CodEmpresa, string CodGrupo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select id_banco as 'item',descripcion as 'descripcion' from Tes_Bancos
                                        where estado = 'A' and cod_grupo = @codGrupo ";

                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { codGrupo = CodGrupo }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }
            return response;

        }

        /// <summary>
        /// Obtiene los tipos de documentos bancarios asociados a un banco específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDTO<List<tesBancosDocData>> Tes_BancoDocTipos_Obtener(int CodEmpresa, string id_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<tesBancosDocData>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select D.*,A.tipo as TipoX
                                        from tes_tipos_doc D left join tes_banco_docs A on D.tipo = A.tipo
                                        and A.id_banco = '{id_banco}' order by A.tipo desc ";

                    response.Result = connection.Query<tesBancosDocData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<tesBancosDocData>();
            }
            return response;
        }

        /// <summary>
        /// Obtiene la configuración de documentos bancarios para un banco específico y tipo de documento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO<tesBancoDocDTO> Tes_BancoDoc_Obtener(int CodEmpresa,int id_banco, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<tesBancoDocDTO>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from tes_banco_docs where id_banco = @banco
                                      and tipo = @tipo ";

                    response.Result = connection.QueryFirstOrDefault<tesBancoDocDTO>(query, new { banco = id_banco, tipo = tipo });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new tesBancoDocDTO();
            }
            return response;
        }

        /// <summary>
        /// Guarda o actualiza la configuración de documentos bancarios para un banco específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="bancoDoc"></param>
        /// <returns></returns>
        public ErrorDTO Tes_BancoDoc_Guardar(int CodEmpresa, tesBancoDocTipoData bancoDoc)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(count(*),0) as Existe from tes_banco_docs where tipo = @tipo
                                        and id_banco = @banco ";

                    var existe = connection.QueryFirstOrDefault<int>(query, new { tipo = bancoDoc.tipo, banco = bancoDoc.id_banco });
                    if (existe > 0)
                    {
                        query = $@"
                                    UPDATE tes_banco_docs SET
                                        reg_autorizacion = @reg_autorizacion,
                                        reg_emision = @reg_emision,
                                        mod_consec = @mod_consec,
                                        doc_auto = @doc_auto,
                                        comprobante = @comprobante,
                                        consecutivo = @consecutivo,
                                        cuenta_min = @cuenta_min,
                                        cuenta_max = @cuenta_max,
                                        consecutivo_det = @consecutivo_det,
                                        actualiza_fecha = GETDATE(),
                                        actualiza_usuario = @actualiza_usuario
                                    WHERE
                                        tipo = @tipo AND
                                        id_banco = @banco";
                    }
                    else
                    {
                        query = $@"
                                    INSERT INTO tes_banco_docs (
                                        tipo, id_banco, reg_autorizacion, reg_emision, doc_auto,
                                        consecutivo, comprobante, mod_consec, cuenta_min, cuenta_max,
                                        consecutivo_det, registro_fecha, registro_usuario
                                    )
                                    VALUES (
                                        @tipo, @banco, @reg_autorizacion, @reg_emision, @doc_auto,
                                        @consecutivo, @comprobante, @mod_consec, @cuenta_min, @cuenta_max,
                                        @consecutivo_det, GETDATE(), @registro_usuario
                                    )";
                    }

                    int ckAutorizacion = bancoDoc.reg_autorizacion ? 1 : 0;
                    int ckEmision = bancoDoc.reg_emision ? 1 : 0;
                    int ckDocAuto = bancoDoc.doc_auto ? 1 : 0;

                    var result = connection.Execute(query, new
                    {
                        tipo = bancoDoc.tipo,
                        banco = bancoDoc.id_banco,
                        reg_autorizacion = ckAutorizacion,
                        reg_emision = ckEmision,
                        doc_auto = ckDocAuto,
                        consecutivo = bancoDoc.consecutivo,
                        comprobante = bancoDoc.comprobante,
                        mod_consec = bancoDoc.mod_consec,
                        cuenta_min = bancoDoc.cuenta_min,
                        cuenta_max = bancoDoc.cuenta_max,
                        consecutivo_det = bancoDoc.consecutivo_det,
                        registro_usuario = bancoDoc.registro_usuario,
                        actualiza_usuario = bancoDoc.registro_usuario
                    });

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
        /// Elimina un tipo de documento bancario asociado a un banco específico, siempre y cuando no existan transacciones registradas para ese tipo de documento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="tipo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO TesBancoDoc_Eliminar(int CodEmpresa, int id_banco, string tipo, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //'Verifica que no existan transacciones registradas.
                    var query = $@"select count(*) as Existe from tes_Transacciones where tipo = @tipo
                                          and id_banco = @banco ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { tipo = tipo, banco = id_banco });
                    if (existe > 0)
                    {
                        response.Code = -1;
                        response.Description = $"{existe} Transacciones registradas a este tipo de documento. NO SE PUEDE ELIMINAR.";
                        return response;
                    }

                    //Elimina la asignación de usuarios a este tipo de documento
                    query = $@"delete tes_documentos_asg where tipo = @tipo and id_banco = @banco ";
                    connection.Execute(query, new { tipo = tipo, banco = id_banco });

                    //Elimina la asignación del documento al banco
                    query = $@"delete tes_banco_docs where tipo = @tipo and id_banco = @banco ";
                    connection.Execute(query, new { tipo = tipo, banco = id_banco });

                    //bitacora
                    BitacoraDb.Bitacora(new Models.BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario.ToUpper(),
                        DetalleMovimiento = "Cta. Id: " + id_banco + ", Tipo Doc: " + tipo,
                        Movimiento = "Elimina",
                        Modulo = 9
                    });

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
