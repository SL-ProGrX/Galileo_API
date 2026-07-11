using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmAfComisionesDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1; // Módulo de clientes
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MTesFuncionesDb _TesFuncionesDB;

        public FrmAfComisionesDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
            _TesFuncionesDB = new MTesFuncionesDb(_config);
        }

        #region Remesa

        public ErrorDto<TablasListaGenericaModel> AF_ComisionesRemesa_Obtener(int CodEmpresa, bool exporta, FiltrosLazyLoadData filtros)
        {
            const string queryExporta = @"
                SELECT COD_COMISION, FECHA, USUARIO, ESTADO
                FROM afi_comisiones
                WHERE (
                    @Filtro IS NULL
                    OR CAST(COD_COMISION AS VARCHAR(50)) LIKE @Filtro
                    OR USUARIO LIKE @Filtro
                    OR ESTADO LIKE @Filtro
                    OR CONVERT(VARCHAR(25), FECHA, 120) LIKE @Filtro
                )
                ORDER BY
                    CASE WHEN @SortField = 'Cod_Comision' AND @SortDirection = 'ASC' THEN COD_COMISION END ASC,
                    CASE WHEN @SortField = 'Cod_Comision' AND @SortDirection = 'DESC' THEN COD_COMISION END DESC,
                    CASE WHEN @SortField = 'FECHA' AND @SortDirection = 'ASC' THEN FECHA END ASC,
                    CASE WHEN @SortField = 'FECHA' AND @SortDirection = 'DESC' THEN FECHA END DESC,
                    CASE WHEN @SortField = 'USUARIO' AND @SortDirection = 'ASC' THEN USUARIO END ASC,
                    CASE WHEN @SortField = 'USUARIO' AND @SortDirection = 'DESC' THEN USUARIO END DESC,
                    CASE WHEN @SortField = 'ESTADO' AND @SortDirection = 'ASC' THEN ESTADO END ASC,
                    CASE WHEN @SortField = 'ESTADO' AND @SortDirection = 'DESC' THEN ESTADO END DESC,
                    COD_COMISION ASC;";

            const string queryPaginado = @"
                SELECT COD_COMISION, FECHA, USUARIO, ESTADO
                FROM afi_comisiones
                WHERE (
                    @Filtro IS NULL
                    OR CAST(COD_COMISION AS VARCHAR(50)) LIKE @Filtro
                    OR USUARIO LIKE @Filtro
                    OR ESTADO LIKE @Filtro
                    OR CONVERT(VARCHAR(25), FECHA, 120) LIKE @Filtro
                )
                ORDER BY
                    CASE WHEN @SortField = 'Cod_Comision' AND @SortDirection = 'ASC' THEN COD_COMISION END ASC,
                    CASE WHEN @SortField = 'Cod_Comision' AND @SortDirection = 'DESC' THEN COD_COMISION END DESC,
                    CASE WHEN @SortField = 'FECHA' AND @SortDirection = 'ASC' THEN FECHA END ASC,
                    CASE WHEN @SortField = 'FECHA' AND @SortDirection = 'DESC' THEN FECHA END DESC,
                    CASE WHEN @SortField = 'USUARIO' AND @SortDirection = 'ASC' THEN USUARIO END ASC,
                    CASE WHEN @SortField = 'USUARIO' AND @SortDirection = 'DESC' THEN USUARIO END DESC,
                    CASE WHEN @SortField = 'ESTADO' AND @SortDirection = 'ASC' THEN ESTADO END ASC,
                    CASE WHEN @SortField = 'ESTADO' AND @SortDirection = 'DESC' THEN ESTADO END DESC,
                    COD_COMISION ASC
                OFFSET @OffsetRows ROWS
                FETCH NEXT @FetchRows ROWS ONLY;";

            var resultadoVacio = new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<AfComisionDto>()
            };

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var salida = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<AfComisionDto>()
                };

                var filtroTexto = filtros?.filtro?.Trim();
                var sortField = filtros?.sortField;
                var sortDirection = (filtros?.sortOrder ?? 0) == 0 ? "ASC" : "DESC";

                if (string.IsNullOrWhiteSpace(sortField) ||
                    (sortField != "Cod_Comision" &&
                     sortField != "FECHA" &&
                     sortField != "USUARIO" &&
                     sortField != "ESTADO"))
                {
                    sortField = "Cod_Comision";
                }

                var parametros = new DynamicParameters();
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);

                salida.total = connection.ExecuteScalar<int>("select COUNT(Cod_Comision) from afi_comisiones");

                if (exporta)
                {
                    salida.lista = connection.Query<AfComisionDto>(queryExporta, parametros).ToList();
                    return salida;
                }

                parametros.Add("OffsetRows", filtros?.pagina ?? 0);
                parametros.Add("FetchRows", filtros?.paginacion ?? 0);
                salida.lista = connection.Query<AfComisionDto>(queryPaginado, parametros).ToList();
                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener comisiones en remesa.", result.Code.GetValueOrDefault(-1), resultadoVacio);
        }


        /// <summary>
        /// Método que obtiene el total de una remesa de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_comision"></param>
        /// <returns></returns>
        public ErrorDto<decimal> AF_ComisionesRemesa_Total(int CodEmpresa, int cod_comision)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<decimal>
            {
                Code = 0,
                Description = "OK",
                Result = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                //busco consecutivo
                var query = @"select isnull(sum(monto),0) as Total from afi_comision_pago where cod_comision = @comision";
                response.Result = connection.Query<decimal>(query, new
                {
                    comision = cod_comision
                }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }


        /// <summary>
        /// Método que guarda una remesa de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="comision"></param>
        /// <returns></returns>
        public ErrorDto AF_ComisionesRemesa_Guardar(int CodEmpresa, string usuario, AfComisionDto comision)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                if (comision.cod_comision == 0)
                {
                    response = AF_ComisionesRemesa_Insertar(CodEmpresa, usuario, comision);
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
        /// Método que inserta una nueva remesa de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="comision"></param>
        /// <returns></returns>
        private ErrorDto AF_ComisionesRemesa_Insertar(int CodEmpresa, string usuario, AfComisionDto comision)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                //busco consecutivo
                var query = @"select isnull(max(cod_comision),0) + 1 as Ultimo from afi_comisiones";
                comision.cod_comision = connection.Query<int>(query).FirstOrDefault();

                string vFecha = MProGrXAuxiliarDB.validaFechaGlobal(comision.fecha, "yyyy-MM-dd HH:mm:ss") ?? string.Empty;

                if (comision.cod_comision > 0)
                {
                    query = @"insert into afi_comisiones (cod_comision, fecha, usuario, estado)
                                  values (@cod_comision, @fecha, @usuario, @estado)";

                    connection.Execute(query, new
                    {
                        cod_comision = comision.cod_comision,
                        fecha = vFecha,
                        usuario = usuario,
                        estado = comision.estado
                    });

                    response.Description = comision.cod_comision.ToString();

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        Modulo = vModulo,
                        Movimiento = "Registra - Web",
                        DetalleMovimiento = $"Remesa Pago de Comision de Afiliacion :  {comision.cod_comision}."
                    });

                }
                else
                {
                    response.Code = -1;
                    response.Description = "No se pudo obtener el consecutivo para la comision.";
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
        /// Método que elimina una remesa de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_comision"></param>
        /// <returns></returns>
        public ErrorDto AF_ComisionesRemesa_Borrar(int CodEmpresa, string usuario, int cod_comision)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                //verifico el estado de la comision
                var query = @"select estado from afi_comisiones where cod_comision = @cod_comision";
                var estado = connection.Query<string>(query, new
                {
                    cod_comision = cod_comision
                }).FirstOrDefault();

                if (estado != "A")
                {
                    response.Code = -1;
                    response.Description = "La Remesa actual; no se encuentra en estado Abierto, no se puede eliminar...";
                    return response;
                }

                query = @"delete from afi_comisiones where cod_comision = @cod_comision";
                connection.Execute(query, new
                {
                    cod_comision = cod_comision
                });
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = "Elimina - Web",
                    DetalleMovimiento = $"Remesa : Pago de Comisiones Afiliacion : {cod_comision}."
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        #endregion

        #region Generacion
        
        /// <summary>
        /// Solo busca las remesas que se encuentran abiertas o en generacion (A ó G)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfComisionDto>> AF_ComisionesGenera_Obtener(int CodEmpresa)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfComisionDto>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                using var connection = new SqlConnection(conn);
                const string query = @"select * from afi_comisiones where estado in('A','G') order by fecha desc";

                response.Result = connection.Query<AfComisionDto>(query).ToList();

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
        /// Método que busca los promotores por tipo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfComisionPromotorData>> AF_ComisionesGenera_Buscar(int CodEmpresa, string tipo)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfComisionPromotorData>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };

            string tipoEjecutivo = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            if (tipoEjecutivo.Length > 1)
            {
                tipoEjecutivo = tipoEjecutivo.Substring(0, 1);
            }

            if (tipoEjecutivo != "P" && tipoEjecutivo != "C" && tipoEjecutivo != "E")
            {
                response.Code = -2;
                response.Description = "Debe indicar un tipo de b&uacute;squeda v&aacute;lido.";
                return response;
            }

            try
            {
                using var connection = new SqlConnection(conn);
                const string query = @"exec spAFIComisionesConsulta @TipoEjecutivo";

                response.Result = connection.Query<AfComisionPromotorData>(
                    query,
                    new { TipoEjecutivo = tipoEjecutivo }).ToList();

                if (response.Result.Count == 0)
                {
                    response.Description = "No se encontraron registros.";
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
        /// Método que genera las comisiones por tipo promotor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="comision"></param>
        /// <param name="promotor"></param>
        /// <returns></returns>
        public ErrorDto AF_ComisionesGenera_Generar(int CodEmpresa, string usuario, int comision, List<AfComisionPromotorData> promotor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                //Valida el Estado de la Remesa
                var query = @"select count(*) as Existe from Afi_Comisiones
                                    where cod_comision = @comision
                                    and estado in('A','G') ";
                var existe = connection.Query<int>(query, new
                {
                    comision = comision
                }).FirstOrDefault();

                if (existe == 0)
                {
                    response.Code = -1;
                    response.Description = "La Remesa actual; ya se encuentra en proceso de Pago...";
                    return response;
                }

                promotor.ForEach(item =>
                {
                    query = @"exec spAFIComisionGenera @Comision, @Promotor";
                    connection.Execute(query, new
                    {
                        Comision = comision,
                        Promotor = item.id_promotor
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        Modulo = vModulo,
                        Movimiento = "Aplica - Web",
                        DetalleMovimiento = $"Generacion Comision de Afiliacion Id. {comision} . Prom. {item.id_promotor}"
                    });
                });

                //Actualiza el Estado de la Remesa
                query = @"update afi_comisiones set estado = 'G' where cod_comision = @comision";
                connection.Execute(query, new
                {
                    comision = comision
                });

                response.Description = "Proceso Realizado Satisfactoriamente...";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        #endregion

        #region Pago

        /// <summary>
        /// Solo busca las remesas que se encuentran en cola de Pago o Pagadas (C,G)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfComisionDto>> AF_ComisionesPago_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<AfComisionDto>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                const string query = @"select * from afi_comisiones where estado in('G','C') order by fecha desc";

                response.Result = connection.Query<AfComisionDto>(query).ToList();

                //datos prueba si es nulo
                if (response.Result.Count == 0)
                {
                    response.Result = new List<AfComisionDto>
                        {
                            new AfComisionDto
                            {
                                cod_comision = 1,
                                fecha = DateTime.Now.AddDays(-5),
                                usuario = "admin",
                                estado = "G"
                            },
                            new AfComisionDto
                            {
                                cod_comision = 2,
                                fecha = DateTime.Now.AddDays(-2),
                                usuario = "admin",
                                estado = "C"
                            }
                        };
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
        /// Método que obtiene los bancos por comision
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="comision"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ComisionesPagoBanco_Obtener(int CodEmpresa, int comision)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                const string query = @"Select 
                                        B.id_banco AS item,
                                        FORMAT(B.id_banco, '0000') + ' - ' + B.descripcion AS descripcion
		                                from Afi_Comision_Pago C inner join Promotores P on C.id_promotor = P.id_promotor
		                                inner join Tes_Bancos B on P.cod_Banco = B.id_banco
		                                Where C.cod_comision = @comision And traslado_fecha Is Null
		                                group by B.id_banco,B.descripcion";

                response.Result = connection.Query<DropDownListaGenericaModel>(query, new
                {
                    comision = comision
                }).ToList();

                //datos prueba si es nulo
                if (response.Result.Count == 0)
                {
                    response.Result = new List<DropDownListaGenericaModel>
                        {
                            new DropDownListaGenericaModel
                            {
                                item = 1,
                                descripcion = "0001 - Banco Prueba 1"
                            },
                            new DropDownListaGenericaModel
                            {
                                item = 2,
                                descripcion = "0002 - Banco Prueba 2"
                            }
                        };
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
        /// Método que busca los promotores por comision y banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="comision"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<AfComisionPagoData>> AF_ComisionesPago_Buscar(int CodEmpresa, int comision, int banco)
        {
            var response = new ErrorDto<List<AfComisionPagoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                string query = $@"
                                    SELECT 
                                        P.id_promotor,
                                        P.nombre,
                                        C.monto,
                                        P.tipo_documento,
                                        P.cuenta_ahorros,
                                        P.nombre_contacto
                                    FROM 
                                        Afi_Comision_Pago C
                                        INNER JOIN Promotores P ON C.id_promotor = P.id_promotor
                                        INNER JOIN Tes_Bancos B ON P.cod_banco = B.id_banco
                                    WHERE 
                                        C.cod_comision = @cod_comision
                                        AND C.traslado_fecha IS NULL
                                        AND B.id_banco = @id_banco;
                                ";
                response.Result = connection.Query<AfComisionPagoData>(query, new
                {
                    cod_comision = comision,
                    id_banco = banco
                }).ToList();

                //datos prueba si es nulo
                if (response.Result.Count == 0)
                {
                    response.Result = new List<AfComisionPagoData>
                        {
                            new AfComisionPagoData
                            {
                                id_promotor = 1,
                                nombre = "Juan Perez",
                                monto = 150000,
                                tipo_documento = "Cédula",
                                cuenta_ahorros = "1234567890",
                                nombre_contacto = "Juan Perez"
                            },
                            new AfComisionPagoData
                            {
                                id_promotor = 2,
                                nombre = "Maria Gomez",
                                monto = 120000,
                                tipo_documento = "Cédula",
                                cuenta_ahorros = "0987654321",
                                nombre_contacto = "Maria Gomez"
                            }
                        };
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
        /// Método que genera el pago de las comisiones por tipo promotor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="comision"></param>
        /// <param name="pagos"></param>
        /// <returns></returns>
        public ErrorDto AF_ComisionesPago_Generar(int CodEmpresa, string usuario, int comision, List<AfComisionPagoData> pagos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                //Genero Token
                var query = @"select top 1 id_token from tes_tokens where estado = 'A' order by registro_fecha";
                string? vToken = connection.Query<string>(query).FirstOrDefault();
                if (string.IsNullOrEmpty(vToken))
                {
                    vToken = _TesFuncionesDB.fxTesToken(CodEmpresa, usuario);
                }

                //Valida el Estado de la Remesa
                query = @"select count(*) as Existe from Afi_Comisiones
                                    where cod_comision = @comision
                                    and estado in('G','C') ";
                var existe = connection.Query<int>(query, new
                {
                    comision = comision
                }).FirstOrDefault();
                if (existe == 0)
                {
                    response.Code = -1;
                    response.Description = "La Remesa actual; ya se encuentra en procesada...";
                    return response;
                }

                //Actualiza el Estado de la Remesa como Cola de Pago / Al finalizar Revisa si ya fue Totalmente Pagada
                query = @"update afi_comisiones set estado = 'C' where cod_comision = @comision";
                connection.Execute(query, new
                {
                    comision = comision
                });


                pagos.ForEach(item =>
                {
                    query = @"exec spAFIComisionPago @Comision, @Promotor, @Usuario, dbo.MyGetdate()
			                       , @Token, @Remesa, 'AFI.COM' ";
                    connection.Execute(query, new
                    {
                        Comision = comision,
                        Promotor = item.id_promotor,
                        Usuario = usuario,
                        Token = vToken,
                        Remesa = comision
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        Modulo = vModulo,
                        Movimiento = "Aplica - Web",
                        DetalleMovimiento = $"Pago Comision de Afiliacion Id. {comision} . Prom. {item.id_promotor}"
                    });
                });


                response.Description = "Proceso Realizado Satisfactoriamente...";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        #endregion

        #region Reportes

        /// <summary>
        /// Método que obtiene los bancos para el reporte de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="chkRepRemesas"></param>
        /// <param name="cod_comision"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepBancos_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                var query = @"select distinct B.id_banco as item, B.descripcion 
                from Tes_Bancos B inner join afi_comision_pago C on B.id_banco = C.cod_banco";

                if (!chkRepRemesas)
                {
                    query += " where C.cod_comision = @cod_comision";
                }

                response.Result = connection.Query<DropDownListaGenericaModel>(query, new { cod_comision }).ToList();

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
        /// Método que obtiene los promotores para el reporte de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="chkRepRemesas"></param>
        /// <param name="cod_comision"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepPromotores_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                var query = @"select P.id_promotor as item, P.nombre as descripcion 
                           from promotores P inner join afi_comision_pago C on P.id_promotor = C.id_Promotor";

                if (!chkRepRemesas)
                {
                    query += " where C.cod_comision = @cod_comision";
                }

                response.Result = connection.Query<DropDownListaGenericaModel>(query, new { cod_comision }).ToList();

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
        /// Método que obtiene las remesas para el reporte de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepRemesa_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                var query = @"select cod_comision as item, 
                             FORMAT(cod_comision, '0000') + ' - ' + FORMAT(fecha, 'dd/MM/yyyy') as descripcion 
                             from afi_comisiones order by fecha desc";

                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();

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
        /// Método que obtiene los usuarios para el reporte de comisiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="chkRepRemesas"></param>
        /// <param name="cod_comision"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepUsuario_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                var query = @"select distinct Traslado_user as item, Traslado_user as descripcion 
                           from afi_comision_pago ";

                if (!chkRepRemesas)
                {
                    query += " where cod_comision = @cod_comision";
                }

                query += " order by Traslado_user";

                response.Result = connection.Query<DropDownListaGenericaModel>(query, new { cod_comision }).ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        #endregion
    }
}