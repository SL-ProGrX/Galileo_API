using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXConsolidacionesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private readonly MCntXProfesionalDb _mCntXProfesional;
        private const int vModulo = 12;

        public FrmCntXConsolidacionesDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config),
                  new MCntXProfesionalDb(config))
        {
        }

        public FrmCntXConsolidacionesDb(
            PortalDB portalDb,
            MSecurityMainDb dbBitacora,
            MCntXProfesionalDb mCntXProfesional)
        {
            _portalDb = portalDb;
            _dbBitacora = dbBitacora;
            _mCntXProfesional = mCntXProfesional;
        }

        /// <summary>
        /// Obtiene una consolidación por código.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConsolida"></param>
        /// <returns></returns>
        public ErrorDto<CntXConsolidacionDefinicionData?> CntXConsolidaciones_Consulta_Obtener(
            int codEmpresa,
            int codConsolida)
        {
            const string query = @"
                select top 1
                    C.cod_consolida as cod_consolida,
                    C.descripcion as descripcion,
                    C.cod_contabilidad as cod_contabilidad,
                    E.nombre as nombre_contabilidad,
                    C.nivel as nivel,
                    isnull(C.registro_usuario, '') as registro_usuario,
                    isnull(convert(varchar(19), C.registro_fecha, 120), '') as registro_fecha,
                    isnull(C.actualiza_usuario, '') as actualiza_usuario,
                    isnull(convert(varchar(19), C.actualiza_fecha, 120), '') as actualiza_fecha
                from CNTX_CONSOLIDA_DEFINICION C
                inner join CNTX_CONTABILIDADES E
                    on C.COD_CONTABILIDAD = E.COD_CONTABILIDAD
                where C.cod_consolida = @codConsolida;";

            return DbHelper.ExecuteSingleQuery<CntXConsolidacionDefinicionData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { codConsolida });
        }

        /// <summary>
        /// Obtiene la lista de consolidaciones para búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXConsolidaciones_Lista_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    cod_consolida as item,
                    descripcion as descripcion
                from CNTX_CONSOLIDA_DEFINICION
                order by cod_consolida;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene contabilidades para selección de contabilidad base.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXConsolidaciones_Contabilidades_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    cast(COD_CONTABILIDAD as varchar(20)) as item,
                    rtrim(isnull(nombre, '')) as descripcion
                from CNTX_CONTABILIDADES
                order by COD_CONTABILIDAD;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene las contabilidades locales por máscara de la contabilidad base.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codConsolida"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXConsolidacionContabilidadData>> CntXConsolidaciones_ContabilidadesLocales_Obtener(
            int codEmpresa,
            int codContabilidad,
            int codConsolida)
        {
            const string query = @"
                select
                    E.COD_CONTABILIDAD as cod_contabilidad,
                    E.nombre as nombre,
                    cast(case when D.COD_CONTABILIDAD is null then 0 else 1 end as bit) as [checked]
                from CNTX_CONTABILIDADES B
                inner join CNTX_CONTABILIDADES E
                    on E.nivel1 = B.nivel1
                   and E.nivel2 = B.nivel2
                   and E.nivel3 = B.nivel3
                   and E.nivel4 = B.nivel4
                   and E.nivel5 = B.nivel5
                left join CNTX_CONSOLIDA_DEFINICION_DET D
                    on D.cod_consolida = @codConsolida
                   and D.COD_CONTABILIDAD = E.COD_CONTABILIDAD
                where B.COD_CONTABILIDAD = @codContabilidad
                order by E.COD_CONTABILIDAD;";

            return DbHelper.ExecuteListQuery<CntXConsolidacionContabilidadData>(
                _portalDb,
                codEmpresa,
                query,
                new { codContabilidad, codConsolida });
        }

        /// <summary>
        /// Obtiene el árbol raíz de portales.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXConsolidacionPortalNodeData>> CntXConsolidaciones_PortalesRaiz_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    cod_portal,
                    descripcion
                from CNTX_CONSOLIDA_PORTALES
                order by cod_portal;";

            var resp = DbHelper.ExecuteListQuery<CntXPortalData>(
                _portalDb,
                codEmpresa,
                query);

            if (resp.Code == -1)
            {
                return new ErrorDto<List<CntXConsolidacionPortalNodeData>>
                {
                    Code = -1,
                    Description = resp.Description,
                    Result = new List<CntXConsolidacionPortalNodeData>()
                };
            }

            var result = (resp.Result ?? new List<CntXPortalData>())
                .Select(x => new CntXConsolidacionPortalNodeData
                {
                    key = $"0x0{x.cod_portal}P",
                    label = x.descripcion,
                    tipo = "portal",
                    cod_portal = x.cod_portal,
                    loaded = false,
                    children = new List<CntXConsolidacionPortalNodeData>()
                })
                .ToList();

            return new ErrorDto<List<CntXConsolidacionPortalNodeData>>
            {
                Code = 0,
                Description = string.Empty,
                Result = result
            };
        }

        /// <summary>
        /// Obtiene las contabilidades externas disponibles en un portal para una máscara dada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPortal"></param>
        /// <param name="codContabilidadBase"></param>
        /// <param name="codConsolida"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXConsolidacionPortalNodeData>> CntXConsolidaciones_PortalesContabilidades_Obtener(
            int codEmpresa, int codPortal, int codContabilidadBase, int codConsolida)
        {
            var stringConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                var mascara = connection.QueryFirstOrDefault<CntXMascaraContaData>(
                    @"select nivel1, nivel2, nivel3, nivel4, nivel5 
                      from CNTX_CONTABILIDADES
                      where COD_CONTABILIDAD = @codContabilidadBase",
                    new { codContabilidadBase });

                if (mascara == null)
                {
                    return new ErrorDto<List<CntXConsolidacionPortalNodeData>>
                    {
                        Code = -2,
                        Description = "No se encontró la contabilidad base.",
                        Result = new List<CntXConsolidacionPortalNodeData>()
                    };
                }

                var portales = connection.Query<CntXPortalData>(
                    @"select P.*, C.COD_CONTABILIDAD
                      from CNTX_CONSOLIDA_PORTALES P
                      inner join CNTX_CONSOLIDA_PORTALES_CONTAS C
                          on P.cod_portal = C.cod_portal
                      where P.cod_portal = @codPortal",
                    new { codPortal }).ToList();

                var marcadas = connection.Query<CntXPortalContaRelacionData>(
                    @"select cod_portal, COD_CONTABILIDAD as cod_contabilidad
                      from CNTX_CONSOLIDA_PORTALES_CON
                      where cod_consolida = @codConsolida
                        and cod_portal = @codPortal",
                    new { codConsolida, codPortal }).ToList();

                var result = new List<CntXConsolidacionPortalNodeData>();

                foreach (var portalConta in portales)
                {
                    var connectionString = _mCntXProfesional.FxPortalPrueba(
                        portalConta.por_user,
                        _mCntXProfesional.FxPortalCifrado(portalConta.por_password, "D"),
                        portalConta.por_server,
                        portalConta.por_database);

                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        continue;
                    }

                    try
                    {
                        using var externalConnection = new SqlConnection(connectionString);
                        externalConnection.Open();

                        var conta = externalConnection.QueryFirstOrDefault<CntXContaNombreData>(
                            @"select COD_CONTABILIDAD as cod_contabilidad, nombre
                              from CNTX_CONTABILIDADES
                              where COD_CONTABILIDAD = @codContabilidad
                                and nivel1 = @nivel1
                                and nivel2 = @nivel2
                                and nivel3 = @nivel3
                                and nivel4 = @nivel4
                                and nivel5 = @nivel5",
                            new
                            {
                                codContabilidad = portalConta.cod_contabilidad,
                                nivel1 = mascara.nivel1,
                                nivel2 = mascara.nivel2,
                                nivel3 = mascara.nivel3,
                                nivel4 = mascara.nivel4,
                                nivel5 = mascara.nivel5
                            });

                        if (conta == null)
                        {
                            continue;
                        }

                        result.Add(new CntXConsolidacionPortalNodeData
                        {
                            key = $"0x0{codPortal}-{conta.cod_contabilidad}E",
                            label = conta.nombre,
                            tipo = "contabilidad",
                            cod_portal = codPortal,
                            cod_contabilidad = conta.cod_contabilidad,
                            @checked = marcadas.Any(x =>
                                x.cod_portal == codPortal &&
                                x.cod_contabilidad == conta.cod_contabilidad)
                        });
                    }
                    catch
                    {
                        // Si un portal no abre, se ignora igual que el VB6.
                    }
                }

                return new ErrorDto<List<CntXConsolidacionPortalNodeData>>
                {
                    Code = 0,
                    Description = string.Empty,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<CntXConsolidacionPortalNodeData>>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new List<CntXConsolidacionPortalNodeData>()
                };
            }
        }

        /// <summary>
        /// Guarda la consolidación y reemplaza sus detalles locales y de portales.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXConsolidaciones_Guardar(
            int codEmpresa,
            string usuario,
            CntXConsolidacionesGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "La descripción de la consolidación no es válida."
                };
            }

            if (request.cod_contabilidad <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "La contabilidad base no es válida."
                };
            }

            var stringConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                var codConsolida = request.cod_consolida;

                if (codConsolida > 0)
                {
                    connection.Execute(
                        @"update CNTX_CONSOLIDA_DEFINICION
                          set descripcion = @descripcion,
                              actualiza_usuario = @usuario,
                              actualiza_fecha = getdate()
                          where cod_consolida = @codConsolida",
                        new
                        {
                            descripcion = request.descripcion.Trim().ToUpper(),
                            usuario = usuario.Trim(),
                            codConsolida
                        },
                        transaction);

                    RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        "Modifica",
                        $"Consolidacion : {codConsolida}");
                }
                else
                {
                    codConsolida = connection.ExecuteScalar<int>(
                        "select isnull(max(cod_consolida),0) + 1 from CNTX_CONSOLIDA_DEFINICION",
                        transaction: transaction);

                    connection.Execute(
                        @"insert into CNTX_CONSOLIDA_DEFINICION
                          (descripcion, cod_consolida, COD_CONTABILIDAD, nivel, registro_usuario, registro_fecha)
                          values
                          (@descripcion, @codConsolida, @codContabilidad, @nivel, @usuario, getdate())",
                        new
                        {
                            descripcion = request.descripcion.Trim().ToUpper(),
                            codConsolida,
                            codContabilidad = request.cod_contabilidad,
                            nivel = request.nivel,
                            usuario = usuario.Trim()
                        },
                        transaction);

                    RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        "Registra",
                        $"Consolidación: {codConsolida}");
                }

                connection.Execute(
                    "delete CNTX_CONSOLIDA_DEFINICION_DET where cod_consolida = @codConsolida",
                    new { codConsolida },
                    transaction);

                foreach (var codContabilidad in request.contabilidades_locales.Distinct())
                {
                    connection.Execute(
                        @"insert into CNTX_CONSOLIDA_DEFINICION_DET
                          (cod_consolida, COD_CONTABILIDAD, registro_usuario, registro_fecha)
                          values
                          (@codConsolida, @codContabilidad, @usuario, getdate())",
                        new
                        {
                            codConsolida,
                            codContabilidad,
                            usuario = usuario.Trim()
                        },
                        transaction);
                }

                connection.Execute(
                    "delete CNTX_CONSOLIDA_PORTALES_CON where cod_consolida = @codConsolida",
                    new { codConsolida },
                    transaction);

                foreach (var item in request.contabilidades_portales
                             .Where(x => x.cod_portal > 0 && x.cod_contabilidad > 0)
                             .GroupBy(x => new { x.cod_portal, x.cod_contabilidad })
                             .Select(x => x.First()))
                {
                    connection.Execute(
                        @"insert into CNTX_CONSOLIDA_PORTALES_CON
                          (cod_consolida, cod_portal, COD_CONTABILIDAD, registro_usuario, registro_fecha)
                          values
                          (@codConsolida, @codPortal, @codContabilidad, @usuario, getdate())",
                        new
                        {
                            codConsolida,
                            codPortal = item.cod_portal,
                            codContabilidad = item.cod_contabilidad,
                            usuario = usuario.Trim()
                        },
                        transaction);
                }

                transaction.Commit();

                return new ErrorDto
                {
                    Code = codConsolida,
                    Description = "Información guardada satisfactoriamente..."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Borra una consolidación y todos sus detalles relacionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConsolida"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CntXConsolidaciones_Borrar(
            int codEmpresa,
            int codConsolida,
            string usuario)
        {
            if (codConsolida <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar una consolidación válida."
                };
            }

            var stringConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                connection.Execute(
                    "delete CNTX_CONSOLIDA_DEFINICION_DET where cod_consolida = @codConsolida",
                    new { codConsolida },
                    transaction);

                connection.Execute(
                    "delete CNTX_CONSOLIDA_PORTALES_CON where cod_consolida = @codConsolida",
                    new { codConsolida },
                    transaction);

                connection.Execute(
                    "delete CNTX_CONSOLIDA_HISTORIAL where cod_consolida = @codConsolida",
                    new { codConsolida },
                    transaction);

                connection.Execute(
                    "delete CNTX_CONSOLIDA_DEFINICION where cod_consolida = @codConsolida",
                    new { codConsolida },
                    transaction);

                transaction.Commit();

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    "Elimina",
                    $"Consolidacion: {codConsolida}");

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Registro eliminado satisfactoriamente."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).Trim().ToUpper(),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}

