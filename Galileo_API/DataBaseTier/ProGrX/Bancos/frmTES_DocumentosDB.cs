using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using Sinpe_CCD;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_DocumentosDB
    {
        private readonly IConfiguration? _config;
        private readonly mProGrX_AuxiliarDB _utils;
        private readonly int vModulo = 9;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmTES_DocumentosDB(IConfiguration? config)
        {
            _config = config;
            _utils = new mProGrX_AuxiliarDB(_config);
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtener lista de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_DocumentosLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "",
                Result = new List<DropDownListaGenericaModel>(),
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select Tipo as 'item',descripcion from tes_tipos_doc order by descripcion asc";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtener un tipo de documento por su código (tipo) mediante scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="scroll"></param>
        /// <returns></returns>
        public ErrorDTO<TesTiposDocDTO> Tes_Documentos_Scroll(int CodEmpresa, string tipo, int? scroll)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TesTiposDocDTO>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesTiposDocDTO()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    scroll = (scroll == null) ? 1 : scroll;
                    if (scroll == 1) //busca el registro anterior
                    {
                        where = $" WHERE Tipo < '{tipo}' ORDER BY Tipo desc";
                    }
                    else if (scroll == 2) //busca el registro siguiente
                    {
                        where = $" WHERE Tipo > '{tipo}' ORDER BY Tipo ASC";
                    }

                    var query = $@"select t.*, c.DESCRIPCION as tipo_asiento_desc from tes_tipos_doc t
                                   left join CNTX_TIPOS_ASIENTOS c ON t.TIPO_ASIENTO = c.TIPO_ASIENTO {where} ";
                    response.Result = connection.QueryFirstOrDefault<TesTiposDocDTO>(query);
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
        /// Obtener un tipo de documento por su código (tipo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO<TesTiposDocDTO> Tes_Documentos_Obtener(int CodEmpresa, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TesTiposDocDTO>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesTiposDocDTO()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select t.*, c.DESCRIPCION as tipo_asiento_desc from tes_tipos_doc t
                                   left join CNTX_TIPOS_ASIENTOS c ON t.TIPO_ASIENTO = c.TIPO_ASIENTO where tipo = @tipo";
                    response.Result = connection.QueryFirstOrDefault<TesTiposDocDTO>(query, new { tipo });
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
        /// Obtener lista de tipos de asientos para documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_DocumentosTiposAsientos_Obtener(int CodEmpresa, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "",
                Result = new List<DropDownListaGenericaModel>(),
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select Tipo_Asiento as 'item',descripcion from CNTX_TIPOS_ASIENTOS
                                    where cod_contabilidad = @contabilidad AND ACTIVO = 1
                                    order by descripcion asc";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { contabilidad = contabilidad }).ToList();
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
        /// Obtener un concepto de anulación de documentos por su código (concepto) mediante scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="concepto"></param>
        /// <param name="scroll"></param>
        /// <returns></returns>
        public ErrorDTO<DropDownListaGenericaModel> Tes_DocAnulaConceptos_Scroll(int CodEmpresa, string concepto, int? scroll)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<DropDownListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new DropDownListaGenericaModel()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    scroll = (scroll == null) ? 1 : scroll;
                    if (scroll == 1) //busca el registro anterior
                    {
                        where = $" WHERE Tipo_Asiento < '{concepto}' ORDER BY Tipo_Asiento desc";
                    }
                    else if (scroll == 2) //busca el registro siguiente
                    {
                        where = $" WHERE Tipo_Asiento > '{concepto}' ORDER BY Tipo_Asiento ASC";
                    }

                    var query = $@"select top 1 Tipo_Asiento as 'item',descripcion from CNTX_TIPOS_ASIENTOS {where} ";
                    response.Result = connection.QueryFirstOrDefault<DropDownListaGenericaModel>(query);
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
        /// Guardar o actualizar un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="documento"></param>
        /// <returns></returns>
        public ErrorDTO  TES_Documentos_Guardar(int CodEmpresa, string usuario, TesTiposDocDTO documento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Guardado correctamente"
            };

            try
            {

                if(documento.tipo == null && documento.tipo.Trim() == "")
                {
                    response.Code = -1;
                    response.Description = "El tipo de documento no puede ser nulo.";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {

                    // Verificar si el tipo ya existe
                    var existingTipo = "SELECT count(*) FROM tes_tipos_doc WHERE tipo = @Tipo";
                    int esEdicion = connection.ExecuteScalar<int>(existingTipo, new { documento.tipo });

                    if (esEdicion > 0)
                    {
                        var sql = @"
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
                        WHERE tipo = @Tipo";

                                connection.Execute(sql, new
                                {
                                    Tipo = documento.tipo,
                                    Descripcion = documento.descripcion.ToUpper().Trim(),
                                    Movimiento = documento.movimiento.Substring(0, 1),
                                    Generacion = documento.generacion ? 1: 0,
                                    TipoAsiento = documento.tipo_asiento,
                                    AsientoTransac = documento.asiento_transac ? 1 : 0,
                                    AsientoFormato = documento.asiento_formato ? 1 : 0,
                                    AsientoBanco = documento.asiento_banco ? 1 : 0,
                                    AsientoMascara = documento.asiento_mascara?.Trim(),
                                    Usuario = usuario,
                                    TipoIdentificacion = documento.int_reclasifica_id ? 1 : 0
                                });

                        _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Tipo de Documento : {documento.tipo}",
                            Movimiento = "Registra - Web",
                            Modulo = vModulo
                        });
                    }
                    else
                    {
                        var sql = @"
                        INSERT INTO tes_tipos_doc 
                            (tipo, descripcion, movimiento, generacion, tipo_asiento,
                             asiento_transac, asiento_banco, asiento_formato, asiento_mascara, 
                             REGISTRO_USUARIO,REGISTRO_FECHA, INT_RECLASIFICA_ID )
                        VALUES 
                            (@Tipo, @Descripcion, @Movimiento, @Generacion, @TipoAsiento,
                             @AsientoTransac, @AsientoBanco, @AsientoFormato, @AsientoMascara, 
                              @Usuario, GETDATE(), @TipoIdentificacion)";

                                connection.Execute(sql, new
                                {
                                    Tipo = documento.tipo,
                                    Descripcion = documento.descripcion.ToUpper().Trim(),
                                    Movimiento = documento.movimiento.Substring(0, 1),
                                    Generacion = documento.generacion ? 1 : 0,
                                    TipoAsiento = documento.tipo_asiento,
                                    AsientoTransac = documento.asiento_transac ? 1 : 0,
                                    AsientoFormato = documento.asiento_formato ? 1 : 0,
                                    AsientoBanco = documento.asiento_banco ? 1 : 0,
                                    AsientoMascara = documento.asiento_mascara?.Trim(),
                                    Usuario = usuario,
                                    TipoIdentificacion = documento.int_reclasifica_id ? 1 : 0
                                });

                                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = usuario,
                                    DetalleMovimiento = $"Tipo de Documento : {documento.tipo}",
                                    Movimiento = "Modifica - Web",
                                    Modulo = vModulo
                                });
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

        /// <summary>
        /// Eliminar un tipo de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO TES_Documentos_Eliminar(int CodEmpresa, string tipo,string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Eliminado correctamente"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var sql = @"DELETE FROM tes_tipos_doc WHERE tipo = @Tipo";
                    connection.Execute(sql, new { Tipo = tipo });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Documento : {tipo}",
                        Movimiento = "Elimina - Web",
                        Modulo = vModulo
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
        /// Obtener lista de conceptos de anulación de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO<List<TesDocAnulaConceptosData>> TES_DocAnulaConceptos_Obtener(int CodEmpresa, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TesDocAnulaConceptosData>>
            {
                Code = 0,
                Description = "",
                Result = new List<TesDocAnulaConceptosData>(),
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select ID_CONCEPTO_ANULA as 'id_conceptos', DESCRIPCION, ACTIVO FROM TES_ANULA_CONCEPTOS  WHERE TIPO = @tipo";
                    response.Result = connection.Query<TesDocAnulaConceptosData>(query, new { tipo }).ToList();
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
        /// Guardar o actualizar un concepto de anulación de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDTO TES_DocAnulaConcepto_Guardar(int CodEmpresa, string usuario ,string tipo, TesDocAnulaConceptosData concepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Guardado correctamente"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int pId = (concepto.id_conceptos == null) ? 0 : concepto.id_conceptos;
                    int activo = (concepto.activo) ? 1 : 0;
                    var proc = $@"exec spTes_Anula_Conceptos_Add {pId},'{tipo}', '{concepto.descripcion}', {activo}, '{usuario}' ";
                    var resp = connection.Query<TesDocAnulaConcepRespuesta>(proc).FirstOrDefault();

                    if (resp.pass == 1)
                    {
                        _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Concepto de Anulación de Documentos de Bancos Id: {resp.codigo} - {concepto.descripcion}",
                            Movimiento = resp.movimiento,
                            Modulo = vModulo
                        });
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description = resp.mensaje;
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

        /// <summary>
        /// Eliminar un concepto de anulación de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_conceptos"></param>
        /// <returns></returns>
        public ErrorDTO TES_DocAnulaConcepto_Eliminar(int CodEmpresa, int id_conceptos, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Eliminado correctamente"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var sql = @"exec spTes_Anula_Conceptos_Delete @id , @usuario";
                    var resp = connection.Query<TesDocAnulaConcepRespuesta>(sql, new { id = id_conceptos, usuario }).FirstOrDefault();

                    if (resp.pass == 1)
                    {
                        _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Concepto de Anulación de Documentos de Bancos Id: {id_conceptos}",
                            Movimiento = resp.movimiento,
                            Modulo = vModulo
                        });
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description = resp.mensaje;
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
