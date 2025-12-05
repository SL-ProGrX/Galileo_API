using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.ProGrX_Activos_Fijos;


namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosObrasProcesoDB
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        public FrmActivosObrasProcesoDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Metodo para actualizar datos de finiquito de una obra en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estado"></param>
        /// <param name="fecha_finiquito"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto Activos_Obras_Actualizar(int CodEmpresa, string estado, DateTime fecha_finiquito, string contrato)
        {
            var query = "";
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                query = $@"UPDATE Activos_obras SET estado = @estado,fecha_finiquito =@fecha_finiquito 
                                    WHERE contrato = @contrato";
                connection.Execute(query, new { estado, fecha_finiquito, contrato });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Metodo de consulta de tipos de obras en Proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ObrasTipos_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select rtrim(cod_tipo) as 'item',rtrim(descripcion) as 'descripcion' FROM Activos_obras_tipos";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta de tipos de desembolso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ObrasTiposDesem_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select cod_desembolso as 'item',descripcion as 'descripcion' FROM Activos_obras_tDesem";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta el listado de obras en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Obras_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select rtrim(contrato) as 'item',rtrim(descripcion) as 'descripcion' FROM Activos_obras";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Metodo para consultar el listado de proveedores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Obra_Proveedores_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select cod_proveedor as 'item',descripcion as 'descripcion' from Activos_proveedores";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta lo datos de una obra en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<ActivosObrasData> Activos_Obras_Consultar(int CodEmpresa, string contrato)
        {
            var result = new ErrorDto<ActivosObrasData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosObrasData()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select o.contrato,o.Descripcion,o.Estado,Notas,o.COD_PROVEEDOR,o.fecha_finiquito,o.encargado,o.fecha_Inicio,
                                    o.fecha_estimada,o.ubicacion,o.presu_original,o.addendums,presu_actual,o.desembolsado,o.distribuido,
                                    o.Registro_Usuario,o.Registro_fecha,o.cod_tipo,
                                    T.descripcion as 'TipoObra',P.descripcion as Proveedor
                                    from Activos_obras O inner join Activos_obras_Tipos T on O.cod_tipo = T.cod_tipo
                                    inner join cxp_proveedores P on O.cod_proveedor = P.cod_proveedor
                                    where O.contrato =  @contrato";
                result.Result = connection.Query<ActivosObrasData>(query, new { contrato }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta de listado de adendums
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosObrasProcesoAdendumsData>> Activos_ObrasAdendums_Obtener(int CodEmpresa, string contrato, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosObrasProcesoAdendumsData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosObrasProcesoAdendumsData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                if (filtros.filtro != null)
                {
                    filtros.filtro = " WHERE   contrato = @contrato AND ( cod_Adendum LIKE '%" + filtros.filtro + "%' " +
                        " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                         " OR fecha LIKE '%" + filtros.filtro + "%' " +
                         " OR monto LIKE '%" + filtros.filtro + "%' ) ";
                }
                else
                {
                    filtros.filtro = " WHERE   contrato = @contrato ";
                }
                if (filtros.sortField == "" || filtros.sortField == null)
                {
                    filtros.sortField = "cod_Adendum";
                }
                var query = $@"SELECT cod_Adendum,descripcion,fecha,monto FROM Activos_obras_ade
                                        {filtros.filtro} 
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} 
                                        OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                result.Result = connection.Query<ActivosObrasProcesoAdendumsData>(query, new { contrato }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Consulta de lista de desembolsos de una obra en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosObrasProcesoDesembolsosData>> Activos_ObrasDesembolsos_Obtener(int CodEmpresa, string contrato, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosObrasProcesoDesembolsosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosObrasProcesoDesembolsosData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                if (filtros.filtro != null)
                {
                    filtros.filtro = " WHERE  D.contrato = @contrato AND ( D.cod_desembolso LIKE '%" + filtros.filtro + "%' " +
                        " OR D.COD_PROVEEDOR LIKE '%" + filtros.filtro + "%' " +
                         " OR D.Documento LIKE '%" + filtros.filtro + "%' " +
                         " OR D.fecha LIKE '%" + filtros.filtro + "%' " +
                         " OR D.monto LIKE '%" + filtros.filtro + "%' ) ";
                }
                else
                {
                    filtros.filtro = " WHERE   D.contrato = @contrato ";
                }
                if (filtros.sortField == "" || filtros.sortField == null)
                {
                    filtros.sortField = " D.secuencia";
                }
                var query = $@"select D.secuencia,D.cod_desembolso,D.COD_PROVEEDOR,D.Documento,D.fecha,D.monto,T.descripcion as Desembolso,P.descripcion as Proveedor
                                     from Activos_Obras_Desem D inner join Activos_obras_tDesem T on D.cod_desembolso = T.cod_desembolso
                                     inner join Activos_Proveedores P on D.cod_proveedor = P.cod_Proveedor 
                                      {filtros.filtro} 
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} 
                                        OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                result.Result = connection.Query<ActivosObrasProcesoDesembolsosData>(query, new { contrato }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Metodo para consulta de resultado de obras en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosObrasProcesoResultadosData>> Activos_ObrasResultados_Obtener(int CodEmpresa, string contrato)
        {
            var result = new ErrorDto<List<ActivosObrasProcesoResultadosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosObrasProcesoResultadosData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@" select O.ID_RESULTADOS,'ACTIVO' as Tipo,O.num_placa,A.valor_historico as Monto
                                     ,O.id_adicion,A.nombre,T.descripcion as TA
                                     from Activos_obras_resultados O 
                                     inner join Activos_Principal A on O.num_placa = A.num_placa
                                     inner join Activos_tipo_activo T on A.tipo_activo = T.tipo_activo
                                     where O.tipo = 'A' and O.contrato = @contrato
                                     UNION
                                     select O.ID_RESULTADOS,'MEJORAS' as Tipo,O.num_placa,A.Monto
                                     ,O.id_adicion,A.descripcion as nombre,T.descripcion as TA
                                     from Activos_obras_resultados O 
                                     inner join Activos_retiro_adicion A on O.num_placa = A.num_placa
                                     and O.id_adicion = A.ID_ADDRET
                                     inner join Activos_justificaciones T on A.cod_justificacion = T.cod_justificacion
                                     where O.tipo = 'M' and O.contrato = @contrato";
                result.Result = connection.Query<ActivosObrasProcesoResultadosData>(query, new { contrato }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Metodo para modificar el registro de la obre en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Activos_Obras_Modificar(int CodEmpresa, ActivosObrasData data, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"update Activos_obras set descripcion = @descripcion,encargado =@encargado, notas =@notas ,cod_proveedor =@cod_proveedor
                                   ,presu_original = @presu_original,presu_actual =@presu_actual ,ubicacion =@ubicacion ,fecha_inicio =@fecha_inicio
                                   ,fecha_estimada =@fecha_estimada,cod_tipo =@cod_tipo  where contrato = @contrato";

                connection.Execute(query, new
                {
                    data.descripcion,
                    data.encargado,
                    data.notas,
                    data.cod_proveedor,
                    data.presu_original,
                    data.presu_actual,
                    data.ubicacion,
                    data.fecha_inicio,
                    data.fecha_estimada,
                    data.cod_tipo,
                    data.contrato
                });
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo Activo :  {data.contrato}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Metodo para insertar un nuevo registro de una obra en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Activos_Obras_Insertar(int CodEmpresa, ActivosObrasData data, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"insert into Activos_obras(contrato,cod_tipo,descripcion,estado,encargado,cod_proveedor
                                ,fecha_inicio,fecha_estimada,notas,ubicacion,presu_original,addendums,presu_actual
                                ,desembolsado,distribuido, registro_usuario, registro_fecha)
                                values(
                                @contrato,@cod_tipo,@descripcion,'P',@encargado,@cod_proveedor
                                ,@fecha_inicio,@fecha_estimada,@notas,@ubicacion,@presu_original,@addendums,@presu_actual
                                ,@desembolsado,@distribuido, @usuario, getdate())";

                connection.Execute(query, new
                {
                    data.contrato,
                    data.cod_tipo,
                    data.descripcion,
                    data.encargado,
                    data.cod_proveedor,
                    data.fecha_inicio,
                    data.fecha_estimada,
                    data.notas,
                    data.ubicacion,
                    data.presu_original,
                    data.addendums,
                    data.presu_actual,
                    data.desembolsado,
                    data.distribuido,
                    usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo Activo :  {data.contrato}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Metodo para eliminar un registro de obras en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Activos_Obra_Eliminar(int CodEmpresa, string contrato, string usuario)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"delete Activos_Obras where contrato = @contrato";
                connection.Execute(query, new { contrato });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $" Tipo Activo : {contrato}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Metodo para guardar adendum  de una obra en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dato"></param>
        /// <param name="usuario"></param>
        /// <param name="contrato"></param>
        /// <param name="addendums"></param>
        /// <param name="presu_actual"></param>
        /// <returns></returns>
        public ErrorDto Activos_ObrasAdendum_Guardar(int CodEmpresa, ActivosObrasProcesoAdendumsData dato, string usuario, string contrato, decimal addendums, decimal presu_actual)
        {
            var query = "";
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                query = $@"select coalesce(count(*),0) as Existe from Activos_obras_ade 
                                where cod_adendum =@cod_adendum";
                var existe = connection.QueryFirstOrDefault<int>(query, new { dato.cod_Adendum });
                if (existe == 0)
                {
                    Activos_ObrasAdendum_Insertar(CodEmpresa, dato, contrato);
                    Activos_ObrasAdendum_Actualizar(CodEmpresa, contrato, dato.monto);
                }
                else
                {
                    result.Code = -2;
                    result.Description = "No se puede modificar la informacion procesada...";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Metodo de actualizar adendum de obra en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <param name="monto"></param>
        private void Activos_ObrasAdendum_Actualizar(int CodEmpresa, string contrato, decimal monto)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            var query = $@"update Activos_obras 
                            set addendums = addendums + @monto,
                                presu_actual = presu_actual + @monto  
                                WHERE contrato = @contrato";
            connection.Execute(query, new
            {
                monto,
                contrato
            });
        }


        /// <summary>
        /// Metodo para insertar nuevo adendum
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="contrato"></param>
        private void Activos_ObrasAdendum_Insertar(int CodEmpresa, ActivosObrasProcesoAdendumsData data, string contrato)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            var query = $@"insert into Activos_obras_ade(cod_adendum,contrato,descripcion,fecha,monto)
                                values(@cod_adendum,@contrato,@descripcion,@fecha,@monto )";

            connection.Execute(query, new
            {
                data.cod_Adendum,
                contrato,
                data.descripcion,
                data.fecha,
                data.monto
            });
        }


        /// <summary>
        /// Metodo para guardar nuevo desembolso de obra en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dato"></param>
        /// <param name="usuario"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto Activos_ObrasDesembolso_Guardar(int CodEmpresa, ActivosObrasProcesoDesembolsosData dato, string usuario, string contrato)
        {
            var query = "";
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                query = $@"select coalesce(count(*),0) + 1 as Secuencia from Activos_obras_desem 
                               where contrato = @contrato";
                var Secuencia = connection.QueryFirstOrDefault<int>(query, new { contrato });
                dato.secuencia = Secuencia;
                Activos_Desembolso_Insertar(CodEmpresa, dato, contrato);
                Activos_ObrasDesembolso_Actualizar(CodEmpresa, contrato, dato.monto);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Metodo para actualizar datos de desembolso de una obra en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <param name="monto"></param>
        private void Activos_ObrasDesembolso_Actualizar(int CodEmpresa, string contrato, decimal monto)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            var query = $@"update Activos_obras 
                                set desembolsado = desembolsado + @monto,
                                    presu_actual = presu_actual - @monto  
                                    WHERE contrato = @contrato";
            connection.Execute(query, new
            {
                monto,
                contrato
            });
        }


        /// <summary>
        /// Medoto para insertar nuevo desembolso 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="contrato"></param>
        private void Activos_Desembolso_Insertar(int CodEmpresa, ActivosObrasProcesoDesembolsosData data, string contrato)
        {
            using var connection = _portalDB.CreateConnection(CodEmpresa);
            var query = $@"insert into Activos_obras_desem(secuencia,contrato,cod_desembolso,cod_proveedor,documento,fecha,monto)
                                values(@secuencia,@contrato,@cod_desembolso,@cod_proveedor,@documento,@fecha,@monto )";

            connection.Execute(query, new
            {
                data.secuencia,
                contrato,
                data.cod_desembolso,
                data.cod_proveedor,
                data.documento,
                data.fecha,
                data.monto
            });
        }
    
    }
}