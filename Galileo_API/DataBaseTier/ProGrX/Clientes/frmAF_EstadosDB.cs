using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFEstadosDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string SqlEstadoExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.afi_estados_persona
                    WHERE cod_estado = @CodEstado;";

        private const string SqlEstadoInsert = @"
                    INSERT INTO dbo.afi_estados_persona
                    (
                        cod_estado,
                        descripcion,
                        activo,
                        deduce_creditos,
                        deduce_patrimonio,
                        deduce_ahorros,
                        registro_fecha,
                        registro_usuario
                    )
                    VALUES
                    (
                        @CodEstado,
                        @Descripcion,
                        @Activo,
                        @DeduceCreditos,
                        @DeducePatrimonio,
                        @DeduceAhorros,
                        GETDATE(),
                        @Usuario
                    );";

        private const string SqlEstadoUpdate = @"
                    UPDATE dbo.afi_estados_persona
                    SET descripcion = @Descripcion,
                        activo = @Activo,
                        deduce_creditos = @DeduceCreditos,
                        deduce_patrimonio = @DeducePatrimonio,
                        deduce_ahorros = @DeduceAhorros,
                        actualiza_fecha = GETDATE(),
                        actualiza_usuario = @Usuario
                    WHERE cod_estado = @CodEstado;";

        public FrmAFEstadosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener estados de persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AfEstadosLista> AF_Estados_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AfEstadosLista>
            {
                Code = 0,
                Result = new AfEstadosLista()
                {
                    total = 0,
                    lista = new List<AfEstadosDto>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
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

                if (filtros.paginacion > 0)
                {
                    query += $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                }
                response.Result.lista = connection.Query<AfEstadosDto>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = new List<AfEstadosDto>();
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
        public ErrorDto AF_Estados_Guardar(int CodEmpresa, string Usuario, AfEstadosDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos del estado son requeridos.", -2);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                GuardarEstado(connection, CodEmpresa, Usuario, Info));

            return CrearRespuestaEstado(result);
        }
        
        
        /// <summary>
        /// Inserta o actualiza el estado de persona usando una conexión abierta.
        /// </summary>
        private bool GuardarEstado(SqlConnection connection, int codEmpresa, string usuario, AfEstadosDto info)
        {
            var parametros = CrearParametrosEstado(usuario, info);
            var existe = connection.QueryFirstOrDefault<int>(SqlEstadoExiste, parametros);
            var sql = existe == 0 ? SqlEstadoInsert : SqlEstadoUpdate;
            var movimiento = existe == 0 ? "Registra - WEB" : "Modifica - WEB";

            connection.Execute(sql, parametros);
            RegistrarBitacoraEstado(codEmpresa, usuario, info.cod_estado, movimiento);

            return true;
        }


        /// <summary>
        /// Crea la respuesta estándar para el guardado de estados.
        /// </summary>
        private static ErrorDto CrearRespuestaEstado(ErrorDto<bool> result)
        {
            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar estado de persona.", result.Code.GetValueOrDefault(-1));
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
                var query = "delete afi_estados_persona where cod_estado = @CodEstado";
                connection.Execute(query, new { CodEstado });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = "Estado de Persona : " + CodEstado,
                    Movimiento = "Elimina - WEB",
                    Modulo = 9
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
        /// Obtener movimientos de cambio de estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfEstadosMovimientosDto>> AF_Estados_Movimientos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfEstadosMovimientosDto>>
            {
                Code = 0,
                Result = new List<AfEstadosMovimientosDto>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"select C.*,I.descripcion as EstadoInicial,F.descripcion as EstadoFinal 
                    from afi_estados_cambio C inner join afi_estados_persona I on C.cod_estado = I.cod_estado 
                    inner join afi_estados_persona F on C.cod_estado_cambio = F.cod_estado";

                response.Result = connection.Query<AfEstadosMovimientosDto>(query).ToList();

                if (response.Result != null)
                {
                    foreach (var item in response.Result)
                    {
                        switch ((item.cod_movimiento ?? string.Empty).Trim())
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
        public ErrorDto AF_Estados_Movimientos_Registrar(int CodEmpresa, AfEstadosMovimientosDto Info)
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
                        Usuario = Info.usuario?.ToUpper() ?? string.Empty,
                    }
                );

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Info.usuario?.ToUpper() ?? string.Empty,
                    DetalleMovimiento = "Cambio Estado M." + Info.cod_movimiento + " Ei." + Info.estadoInicial + " Ef." + Info.estadoFinal,
                    Movimiento = "Registra - WEB",
                    Modulo = 9
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
        /// Eliminar movimientos de cambio de estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Lista"></param>
        /// <returns></returns>
        public ErrorDto AF_Estados_Movimientos_Eliminar(int CodEmpresa, List<AfEstadosMovimientosDto> Lista)
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
                foreach (var item in Lista)
                {
                    switch ((item.cod_movimiento ?? string.Empty).Trim())
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

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = item.usuario?.ToUpper() ?? string.Empty,
                        DetalleMovimiento = "Cambio Estado M." + item.cod_movimiento + " Ei." + item.cod_estado + " Ef." + item.cod_estado_cambio,
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
        /// Obtener entidades asociadas a un estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodEstado"></param>
        /// <returns></returns>
        public ErrorDto<List<AfEstadosEntidadesDto>> AF_Estados_Entidades_Obtener(int CodEmpresa, string CodEstado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfEstadosEntidadesDto>>
            {
                Code = 0,
                Result = new List<AfEstadosEntidadesDto>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"select Inst.COD_INSTITUCION,Inst.Descripcion, Inst.DESC_CORTA, 
                    case when isnull(Est.COD_INSTITUCION,0) = 0 then 0 else 1 end as 'Check'
                    from INSTITUCIONES Inst left join AFI_ESTADOS_INSTITUCIONES Est on Inst.COD_INSTITUCION = Est.COD_INSTITUCION
                    and Est.COD_ESTADO = @CodEstado
                    Where Inst.ACTIVA = 1";
                response.Result = connection.Query<AfEstadosEntidadesDto>(query, new { CodEstado }).ToList();
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
        public ErrorDto AF_Estados_Entidad_Guardar(int CodEmpresa, string Usuario, AfEstadosEntidadesDto Info)
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
                if (Info.check)
                {
                    query = @"insert AFI_ESTADOS_INSTITUCIONES(cod_estado,cod_institucion,usuario,fecha)
                            values( @CodEstado, @CodInstitucion, @Usuario, GETDATE())";
                }
                else
                {
                    query = @"delete AFI_ESTADOS_INSTITUCIONES where cod_estado = @CodEstado and cod_institucion = @CodInstitucion";
                }
                connection.Execute(query, new
                {
                    CodEstado = Info.cod_estado,
                    CodInstitucion = Info.cod_institucion,
                    Usuario
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
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Crea parámetros seguros para guardar estados.
        /// </summary>
        private static object CrearParametrosEstado(string usuario, AfEstadosDto info)
        {
            return new
            {
                CodEstado = NormalizarTexto(info.cod_estado),
                Descripcion = NormalizarTexto(info.descripcion),
                Activo = info.activo ? 1 : 0,
                DeduceCreditos = info.deduce_creditos ? 1 : 0,
                DeducePatrimonio = info.deduce_patrimonio ? 1 : 0,
                DeduceAhorros = info.deduce_ahorros ? 1 : 0,
                Usuario = NormalizarTexto(usuario)
            };
        }


        /// <summary>
        /// Registra la bitácora de un estado de persona.
        /// </summary>
        private void RegistrarBitacoraEstado(int codEmpresa, string usuario, string? codEstado, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Estado de Persona : {NormalizarTexto(codEstado)}",
                Movimiento = movimiento,
                Modulo = 9
            });
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}