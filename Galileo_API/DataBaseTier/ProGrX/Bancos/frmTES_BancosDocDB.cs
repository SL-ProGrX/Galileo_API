using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;


namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesBancosDocDB
    {
        private readonly PortalDB _portalDB;

        private readonly MSecurityMainDb BitacoraDb;
        private readonly int vModulo = 9;

        public FrmTesBancosDocDB(IConfiguration config)
        {
            BitacoraDb = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Carga el combo de grupos de documentos bancarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancoDocGrupos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select COD_GRUPO as 'item', DESCRIPCION as 'descripcion'
                                      From TES_BANCOS_GRUPOS Where ACTIVO = 1";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Carga el combo de bancos según el grupo especificado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancoDocBancos_Obtener(int CodEmpresa, string CodGrupo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select id_banco as 'item',descripcion as 'descripcion' from Tes_Bancos
                                        where estado = 'A' and cod_grupo = @codGrupo ";

                return conn.Query<DropDownListaGenericaModel>(query,new { codGrupo = CodGrupo }).ToList();
            });
        }

        /// <summary>
        /// Obtiene los tipos de documentos bancarios asociados a un banco específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<TesBancosDocData>> Tes_BancoDocTipos_Obtener(int CodEmpresa, string id_banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select D.*,A.tipo as TipoX
                                        from tes_tipos_doc D left join tes_banco_docs A on D.tipo = A.tipo
                                        and A.id_banco = @idBanco order by A.tipo desc ";

                return conn.Query<TesBancosDocData>(query, new { idBanco = id_banco }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la configuración de documentos bancarios para un banco específico y tipo de documento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<TesBancoDocDto> Tes_BancoDoc_Obtener(int CodEmpresa,int id_banco, string tipo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select * from tes_banco_docs where id_banco = @banco
                                      and tipo = @tipo ";

                return conn.QueryFirstOrDefault<TesBancoDocDto>(query, new { banco = id_banco, tipo = tipo }) ?? new TesBancoDocDto();
            });
        }

        /// <summary>
        /// Guarda o actualiza la configuración de documentos bancarios para un banco específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="bancoDoc"></param>
        /// <returns></returns>
        public ErrorDto Tes_BancoDoc_Guardar(int CodEmpresa, TesBancoDocTipoData bancoDoc)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var query = $@"select isnull(count(*),0) as Existe from tes_banco_docs where tipo = @tipo
                                        and id_banco = @banco ";

                var existe = conn.QueryFirstOrDefault<int>(query, new { tipo = bancoDoc.tipo, banco = bancoDoc.id_banco });
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

                conn.Execute(query, new
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

                return DbHelper.OkResponse("Configuración de documento bancario guardada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un tipo de documento bancario asociado a un banco específico, siempre y cuando no existan transacciones registradas para ese tipo de documento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="tipo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto TesBancoDoc_Eliminar(int CodEmpresa, int id_banco, string tipo, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                //'Verifica que no existan transacciones registradas.
                var query = $@"select count(*) as Existe from tes_Transacciones where tipo = @tipo
                                          and id_banco = @banco ";
                var existe = conn.QueryFirstOrDefault<int>(query, new { tipo = tipo, banco = id_banco });
                if (existe > 0)
                {
                    return DbHelper.ErrorResponse($"{existe} Transacciones registradas a este tipo de documento. NO SE PUEDE ELIMINAR.");
                }

                //Elimina la asignación de usuarios a este tipo de documento
                query = $@"delete tes_documentos_asg where tipo = @tipo and id_banco = @banco ";
                conn.Execute(query, new { tipo = tipo, banco = id_banco });

                //Elimina la asignación del documento al banco
                query = $@"delete tes_banco_docs where tipo = @tipo and id_banco = @banco ";
                conn.Execute(query, new { tipo = tipo, banco = id_banco });

                //bitacora
                BitacoraDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = "Cta. Id: " + id_banco + ", Tipo Doc: " + tipo,
                    Movimiento = "Elimina",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Tipo de documento bancario eliminado correctamente.");
            }
            catch (Exception ex)
            {
               return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
