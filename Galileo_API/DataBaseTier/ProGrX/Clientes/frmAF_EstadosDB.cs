using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_EstadosDB
    {
        private readonly IConfiguration _config;
        private readonly mSecurityMainDb _mSecurity;

        public frmAF_EstadosDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener estados de persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AF_EstadosLista> AF_Estados_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_EstadosLista>
            {
                Code = 0,
                Result = new AF_EstadosLista()
                {
                    total = 0,
                    lista = new List<AF_EstadosDTO>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = "select COUNT(cod_estado) from afi_estados_persona";
                    response.Result.total = connection.Query<int>(queryT).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_estado LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_estado";
                    }

                    var query = $@"select cod_estado,descripcion,activo,deduce_creditos,deduce_patrimonio,deduce_ahorros 
                        from afi_estados_persona
                        {filtros.filtro} 
                        order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} ";

                    if (filtros.paginacion != 0 || filtros.paginacion == null)
                    {
                        query += $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    }
                    response.Result.lista = connection.Query<AF_EstadosDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = null;
            }
            return response;
        }

        /// <summary>
        /// Guardar estado de persona, 
        /// insertar o actualizar segun si existe o no
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_Estados_Guardar(int CodEmpresa, string Usuario, AF_EstadosDTO Info)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select isnull(count(*),0) as Existe from afi_estados_persona where cod_estado = @CodEstado";
                    int existe = connection.QueryFirstOrDefault<int>(query,
                        new
                        {
                            CodEstado = Info.cod_estado
                        }
                    );

                    if (existe == 0)
                    {
                        query = @"insert into afi_estados_persona(cod_estado,descripcion,activo,deduce_creditos,deduce_patrimonio,deduce_ahorros,registro_fecha,registro_usuario) 
                            values( @CodEstado, @Descripcion, @Activo, @DeduceCreditos, @DeducePatrimonio, @DeduceAhorros, GETDATE(), @Usuario)";

                        connection.Execute(query,
                            new
                            {
                                CodEstado = Info.cod_estado,
                                Descripcion = Info.descripcion,
                                Activo = Info.activo ? 1 : 0,
                                DeduceCreditos = Info.deduce_creditos ? 1 : 0,
                                DeducePatrimonio = Info.deduce_patrimonio ? 1 : 0,
                                DeduceAhorros = Info.deduce_ahorros ? 1 : 0,
                                Usuario
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Estado de Persona : " + Info.cod_estado,
                            Movimiento = "Registra - WEB",
                            Modulo = 9
                        });
                    }
                    else
                    {
                        query = @"update afi_estados_persona set descripcion = @Descripcion, activo = @Activo, 
                            deduce_creditos = @DeduceCreditos, deduce_patrimonio = @DeducePatrimonio, deduce_ahorros = @DeduceAhorros,
                            actualiza_fecha = GETDATE(), actualiza_usuario = @Usuario 
                            where cod_estado = @CodEstado";

                        connection.Execute(query,
                            new
                            {
                                CodEstado = Info.cod_estado,
                                Descripcion = Info.descripcion,
                                Activo = Info.activo ? 1 : 0,
                                DeduceCreditos = Info.deduce_creditos ? 1 : 0,
                                DeducePatrimonio = Info.deduce_patrimonio ? 1 : 0,
                                DeduceAhorros = Info.deduce_ahorros ? 1 : 0,
                                Usuario
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Estado de Persona : " + Info.cod_estado,
                            Movimiento = "Modifica - WEB",
                            Modulo = 9
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
        /// Borrar estado de persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="CodEstado"></param>
        /// <returns></returns>
        public ErrorDto AF_Estados_Eliminar(int CodEmpresa, string Usuario, string CodEstado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "delete afi_estados_persona where cod_estado = @CodEstado";
                    connection.Execute(query, new { CodEstado });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Estado de Persona : " + CodEstado,
                        Movimiento = "Elimina - WEB",
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

        /// <summary>
        /// Obtener movimientos de cambio de estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_Estados_MovimientosDTO>> AF_Estados_Movimientos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AF_Estados_MovimientosDTO>>
            {
                Code = 0,
                Result = new List<AF_Estados_MovimientosDTO>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.*,I.descripcion as EstadoInicial,F.descripcion as EstadoFinal 
                    from afi_estados_cambio C inner join afi_estados_persona I on C.cod_estado = I.cod_estado 
                    inner join afi_estados_persona F on C.cod_estado_cambio = F.cod_estado";

                    response.Result = connection.Query<AF_Estados_MovimientosDTO>(query).ToList();

                    if (response.Result != null)
                    {
                        foreach (var item in response.Result)
                        {
                            switch (item.cod_movimiento.Trim())
                            {
                                case "ING":
                                    item.cod_movimiento = "Ingreso";
                                    break;
                                case "REI":
                                    item.cod_movimiento = "Re-Ingreso";
                                    break;
                                case "REN":
                                    item.cod_movimiento = "Renuncia";
                                    break;
                                case "LIQ":
                                    item.cod_movimiento = "Liquidación";
                                    break;
                                case "ACT":
                                    item.cod_movimiento = "Activación";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
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
        /// Registrar movimiento de cambio de estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_Estados_Movimientos_Registrar(int CodEmpresa, AF_Estados_MovimientosDTO Info)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryV = "select COUNT(*) from afi_estados_cambio where cod_estado = @CodEstado and cod_estado_cambio = @EstadoCambio and cod_movimiento = @Movimiento";
                    int existe = connection.QueryFirstOrDefault<int>(queryV,
                        new
                        {
                            CodEstado = Info.estadoInicial,
                            Movimiento = Info.cod_movimiento,
                            EstadoCambio = Info.estadoFinal
                        }
                    );

                    if (existe > 0)
                    {
                        response.Code = -2;
                        response.Description = "El movimiento ya se encuentra registrado, por favor verifique";
                        return response;
                    }

                    var query = @"insert afi_estados_cambio(cod_estado,cod_movimiento,cod_estado_cambio,usuario,fecha) 
                            values( @EstadoInicial, @Movimiento, @EstadoFinal, @Usuario, GETDATE())";

                    connection.Execute(query,
                        new
                        {
                            EstadoInicial = Info.estadoInicial,
                            Movimiento = Info.cod_movimiento,
                            EstadoFinal = Info.estadoFinal,
                            Usuario = Info.usuario.ToUpper(),
                        }
                    );

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Info.usuario.ToUpper(),
                        DetalleMovimiento = "Cambio Estado M." + Info.cod_movimiento + " Ei." + Info.estadoInicial + " Ef." + Info.estadoFinal,
                        Movimiento = "Registra - WEB",
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

        /// <summary>
        /// Eliminar movimientos de cambio de estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Lista"></param>
        /// <returns></returns>
        public ErrorDto AF_Estados_Movimientos_Eliminar(int CodEmpresa, List<AF_Estados_MovimientosDTO> Lista)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in Lista)
                    {
                        switch (item.cod_movimiento.Trim())
                        {
                            case "Ingreso":
                                item.cod_movimiento = "ING";
                                break;
                            case "Re-Ingreso":
                                item.cod_movimiento = "REI";
                                break;
                            case "Renuncia":
                                item.cod_movimiento = "REN";
                                break;
                            case "Liquidación":
                                item.cod_movimiento = "LIQ";
                                break;
                            case "Activación":
                                item.cod_movimiento = "ACT";
                                break;
                            default:
                                break;
                        }

                        var query = @"delete afi_estados_cambio where cod_estado = @CodEstado and cod_estado_cambio = @EstadoCambio and cod_movimiento = @Movimiento";

                        connection.Execute(query,
                            new
                            {
                                CodEstado = item.cod_estado,
                                Movimiento = item.cod_movimiento,
                                EstadoCambio = item.cod_estado_cambio
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = item.usuario.ToUpper(),
                            DetalleMovimiento = "Cambio Estado M." + item.cod_movimiento + " Ei." + item.cod_estado + " Ef." + item.cod_estado_cambio,
                            Movimiento = "Elimina - WEB",
                            Modulo = 9
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
        /// Obtener entidades asociadas a un estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodEstado"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_Estados_EntidadesDTO>> AF_Estados_Entidades_Obtener(int CodEmpresa, string CodEstado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AF_Estados_EntidadesDTO>>
            {
                Code = 0,
                Result = new List<AF_Estados_EntidadesDTO>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Inst.COD_INSTITUCION,Inst.Descripcion, Inst.DESC_CORTA, 
                    case when isnull(Est.COD_INSTITUCION,0) = 0 then 0 else 1 end as 'Check'
                    from INSTITUCIONES Inst left join AFI_ESTADOS_INSTITUCIONES Est on Inst.COD_INSTITUCION = Est.COD_INSTITUCION
                    and Est.COD_ESTADO = @CodEstado
                    Where Inst.ACTIVA = 1";
                    response.Result = connection.Query<AF_Estados_EntidadesDTO>(query, new { CodEstado }).ToList();
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
        /// Guardar entidad asociada a un estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_Estados_Entidad_Guardar(int CodEmpresa, string Usuario, AF_Estados_EntidadesDTO Info)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (Info.check)
                    {
                        query = @"insert AFI_ESTADOS_INSTITUCIONES(cod_estado,cod_institucion,usuario,fecha)
                            values( @CodEstado, @CodInstitucion, @Usuario, GETDATE())";
                    } 
                    else
                    {
                        query = @"delete AFI_ESTADOS_INSTITUCIONES where cod_estado = @CodEstado and cod_institucion = @CodInstitucion";
                    }
                    connection.Execute(query, new { 
                        CodEstado = Info.cod_estado, 
                        CodInstitucion = Info.cod_institucion, 
                        Usuario 
                    } );
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
        /// Asociar o desasociar todas las entidades un estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="CodEstado"></param>
        /// <param name="Checked"></param>
        /// <returns></returns>
        public ErrorDto AF_Estados_EntidadesTodas_Guardar(int CodEmpresa, string Usuario, string CodEstado, bool Checked)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (Checked)
                    {
                        query = @"insert into AFI_ESTADOS_INSTITUCIONES(cod_estado,cod_institucion,usuario,fecha) 
                            (select @CodEstado, cod_institucion, @Usuario, GETDATE() 
                            from instituciones where activa = 1 and cod_institucion not in(select cod_institucion from AFI_ESTADOS_INSTITUCIONES
                            where cod_estado = @CodEstado))";
                    }
                    else
                    {
                        query = "delete AFI_ESTADOS_INSTITUCIONES where cod_estado = @CodEstado";
                    }
                    connection.Execute(query, new { CodEstado, Usuario });
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
