using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmFndSeguimientoRevisionesTagsDb
    {
        private const string Modulo = "FND";
        private readonly PortalDB _portalDb;

        public FrmFndSeguimientoRevisionesTagsDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los contratos de fondos pendientes de revision.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<FndSeguimientoRevisionFondoData>>
            FND_frmFNDSeguimientoRevisionesTags_Fondos_Obtener(
                int codEmpresa,
                string? cedula)
        {
            const string sql = """
                select
                    isnull(rtrim(F.CEDULA), '') as cedula,
                    isnull(rtrim(S.NOMBRE), '') as nombre,
                    isnull(rtrim(F.USUARIO), '') as usuario,
                    F.COD_CONTRATO as cod_contrato,
                    isnull(rtrim(F.COD_PLAN), '') as cod_plan,
                    F.COD_OPERADORA as cod_operadora
                from FND_CONTRATOS F
                inner join SOCIOS S
                    on F.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on F.COD_OFICINA = O.COD_OFICINA
                where isnull(F.ANALISTA_REVISION, 'N') = 'N'
                  and F.ESTADO = 'A'
                  and (
                      @Cedula = ''
                      or F.CEDULA = @Cedula
                  )
                order by F.COD_CONTRATO desc;
                """;

            return DbHelper.ExecuteListQuery<FndSeguimientoRevisionFondoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cedula = NormalizarTexto(cedula) });
        }

        /// <summary>
        /// Obtiene el detalle del contrato seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FndSeguimientoRevisionDetalleData?>
            FND_frmFNDSeguimientoRevisionesTags_Detalle_Obtener(
                int codEmpresa,
                FndSeguimientoRevisionClaveRequest? request)
        {
            string? validacion = ValidarClave(request);

            if (validacion is not null)
            {
                return DbHelper.CreateErrorResponse<FndSeguimientoRevisionDetalleData?>(
                    validacion,
                    -2,
                    null);
            }

            const string sql = """
                select top 1
                    isnull(rtrim(C.CEDULA), '') as cedula,
                    isnull(rtrim(S.NOMBRE), '') as nombre,
                    C.COD_OPERADORA as cod_operadora,
                    isnull(rtrim(O.DESCRIPCION), '') as operadora,
                    isnull(rtrim(C.COD_PLAN), '') as cod_plan,
                    isnull(rtrim(P.DESCRIPCION), '') as [plan],
                    C.COD_CONTRATO as cod_contrato,
                    isnull(rtrim(C.ESTADO), '') as estado,
                    case
                        when C.ESTADO = 'A' then 'Activo'
                        else 'Liquidado'
                    end as estado_descripcion,
                    C.FECHA_INICIO as fecha_inicio,
                    isnull(C.MONTO, 0) as monto,
                    isnull(C.PLAZO, 0) as plazo,
                    isnull(rtrim(C.RENUEVA), '') as renueva,
                    case
                        when C.RENUEVA = 'S' then 'SI'
                        else 'NO'
                    end as renueva_descripcion,
                    isnull(C.INC_ANUAL, 0) as inc_anual,
                    isnull(rtrim(C.INC_TIPO), '') as inc_tipo,
                    case
                        when C.INC_TIPO = 'P' then 'Porcentaje'
                        else 'Monto'
                    end as inc_tipo_descripcion,
                    isnull(C.APORTES, 0) as aportes,
                    isnull(C.RENDIMIENTO, 0) as rendimiento,
                    isnull(C.APORTES, 0) +
                    isnull(C.RENDIMIENTO, 0) as total,
                    isnull(convert(varchar(30), C.OPERACION), '')
                        as operacion
                from FND_CONTRATOS C
                inner join SOCIOS S
                    on C.CEDULA = S.CEDULA
                inner join FND_PLANES P
                    on C.COD_PLAN = P.COD_PLAN
                   and C.COD_OPERADORA = P.COD_OPERADORA
                inner join FND_OPERADORAS O
                    on C.COD_OPERADORA = O.COD_OPERADORA
                where C.COD_OPERADORA = @CodOperadora
                  and C.COD_PLAN = @CodPlan
                  and C.COD_CONTRATO = @CodContrato
                  and C.CEDULA = @Cedula;
                """;

            return DbHelper.ExecuteSingleQuery<FndSeguimientoRevisionDetalleData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                CrearParametrosClave(request));
        }

        /// <summary>
        /// Obtiene el historial de etiquetas del contrato.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<FndSeguimientoRevisionRegistroData>>
            FND_frmFNDSeguimientoRevisionesTags_Seguimiento_Obtener(
                int codEmpresa,
                FndSeguimientoRevisionClaveRequest? request)
        {
            string? validacion = ValidarClave(request);

            if (validacion is not null)
            {
                return DbHelper.CreateErrorResponse<List<FndSeguimientoRevisionRegistroData>>(
                    validacion,
                    -2,
                    []);
            }

            const string sql = """
                select
                    isnull(rtrim(T.DESCRIPCION), '') as descripcion,
                    isnull(rtrim(CT.NOTAS), '') as notas,
                    CT.REGISTRO_FECHA as registro_fecha,
                    isnull(rtrim(CT.REGISTRO_USUARIO), '')
                        as registro_usuario
                from SIF_CONTROL_TAGS CT
                inner join SIF_TAGS T
                    on CT.TAG_CODIGO = T.TAG_CODIGO
                where CT.CODIGO = @CodPlan
                  and CT.DOCUMENTO = @CodContrato
                  and CT.COD_MODULO = @Modulo
                order by CT.REGISTRO_FECHA desc;
                """;

            return DbHelper.ExecuteListQuery<FndSeguimientoRevisionRegistroData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodPlan = request.cod_plan.Trim(),
                    CodContrato = request.cod_contrato.ToString(),
                    Modulo
                });
        }

        /// <summary>
        /// Obtiene las etiquetas activas del modulo FND autorizadas al usuario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FND_frmFNDSeguimientoRevisionesTags_Etiquetas_Obtener(
                int codEmpresa,
                string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    "Debe indicar un usuario v&aacute;lido.",
                    -2,
                    []);
            }

            const string sql = """
                select distinct
                    rtrim(T.TAG_CODIGO) as item,
                    rtrim(T.TAG_CODIGO) + ' - ' +
                    rtrim(T.DESCRIPCION) as descripcion
                from SIF_TAGS T
                inner join SIF_TAGS_GRUPOS TG
                    on T.TAG_CODIGO = TG.TAG_CODIGO
                inner join SIF_GRPUSERS GU
                    on TG.COD_GRUPO = GU.COD_GRUPO
                inner join SIF_TAGS_MODULOS TM
                    on T.TAG_CODIGO = TM.TAG_CODIGO
                where T.ACTIVO = 1
                  and GU.USUARIO = @Usuario
                  and TM.COD_MODULO = @Modulo
                order by item;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Usuario = usuario.Trim(),
                    Modulo
                });
        }

        /// <summary>
        /// Obtiene el aviso configurado para una etiqueta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tagCodigo"></param>
        /// <returns></returns>
        public ErrorDto
            FND_frmFNDSeguimientoRevisionesTags_Aviso_Obtener(
                int codEmpresa,
                string tagCodigo)
        {
            if (string.IsNullOrWhiteSpace(tagCodigo))
            {
                return DbHelper.ErrorResponse(
                    "Debe seleccionar una etiqueta.",
                    -2);
            }

            const string sql = """
                select top 1
                    isnull(MENSAJE, '')
                from SIF_TAGS_AVISOS
                where TAG_CODIGO = @TagCodigo;
                """;

            ErrorDto<string?> resultado = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                string.Empty,
                new { TagCodigo = tagCodigo.Trim() });

            if (resultado.Code == -1)
            {
                return DbHelper.ErrorResponse(resultado.Description, -1);
            }

            return DbHelper.OkResponse(resultado.Result ?? string.Empty);
        }

        /// <summary>
        /// Obtiene las omisiones del modulo FND y las asignadas al contrato.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<FndSeguimientoRevisionOmisionData>>
            FND_frmFNDSeguimientoRevisionesTags_Omisiones_Obtener(
                int codEmpresa,
                FndSeguimientoRevisionClaveRequest? request)
        {
            string? validacion = ValidarClave(request);

            if (validacion is not null)
            {
                return DbHelper.CreateErrorResponse<List<FndSeguimientoRevisionOmisionData>>(
                    validacion,
                    -2,
                    []);
            }

            const string sql = """
                select
                    E.ID_ERROR as id_error,
                    isnull(rtrim(E.DESCRIPCION), '') as descripcion,
                    ER.ID_ERROR as asignado,
                    isnull(rtrim(ER.APLICADO), 'N') as aplicado,
                    isnull(rtrim(E.MENSAJE), '') as mensaje,
                    ER.LINEA_ERR as linea_err,
                    cast(
                        case
                            when ER.ID_ERROR is null then 0
                            else 1
                        end
                        as bit
                    ) as seleccionado
                from SIF_OMISIONES E
                left join SIF_OMISIONESG ER
                    on E.ID_ERROR = ER.ID_ERROR
                   and ER.CEDULA = @Cedula
                   and ER.MODULO = @Modulo
                   and ER.CODIGO = @CodPlan
                   and ER.DOCUMENTO = @CodContrato
                where E.ACTIVO = '1'
                  and exists (
                      select 1
                      from SIF_OMISIONES_MODULOS OM
                      where OM.ID_ERROR = E.ID_ERROR
                        and OM.COD_MODULO = @Modulo
                  )
                order by E.ID_ERROR;
                """;

            return DbHelper.ExecuteListQuery<FndSeguimientoRevisionOmisionData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Cedula = request.cedula.Trim(),
                    CodPlan = request.cod_plan.Trim(),
                    CodContrato = request.cod_contrato.ToString(),
                    Modulo
                });
        }

        /// <summary>
        /// Registra o elimina una omision del contrato.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FndSeguimientoRevisionOmisionCambiarData>
            FND_frmFNDSeguimientoRevisionesTags_Omision_Cambiar(
                int codEmpresa,
                FndSeguimientoRevisionOmisionCambiarRequest? request)
        {
            string? validacion = ValidarCambioOmision(request);

            if (validacion is not null)
            {
                return CrearErrorCambioOmision(validacion, -2);
            }

            ErrorDto<ErrorDto<FndSeguimientoRevisionOmisionCambiarData>> ejecucion =
                DbHelper.WithConn(
                    _portalDb,
                    codEmpresa,
                    connection =>
                    {
                        connection.Open();
                        using var transaction = connection.BeginTransaction();

                        const string sqlContrato = """
                            select count(1)
                            from FND_CONTRATOS F
                            where F.CEDULA = @Cedula
                              and F.COD_PLAN = @CodPlan
                              and F.COD_CONTRATO = @CodContrato
                              and isnull(F.ANALISTA_REVISION, 'N') = 'N'
                              and F.ESTADO = 'A';
                            """;

                        int existeContrato = connection.ExecuteScalar<int>(
                            sqlContrato,
                            new
                            {
                                Cedula = request.cedula.Trim(),
                                CodPlan = request.cod_plan.Trim(),
                                CodContrato = request.cod_contrato
                            },
                            transaction);

                        if (existeContrato == 0)
                        {
                            transaction.Rollback();

                            return CrearErrorCambioOmision(
                                "El contrato seleccionado no se encuentra pendiente de revisi&oacute;n.",
                                -2);
                        }

                        var parametrosClave = new
                        {
                            Cedula = request.cedula.Trim(),
                            Modulo,
                            CodPlan = request.cod_plan.Trim(),
                            CodContrato = request.cod_contrato.ToString(),
                            IdError = request.id_error
                        };

                        if (request.seleccionado)
                        {
                            const string sqlExistente = """
                                select top 1 LINEA_ERR
                                from SIF_OMISIONESG
                                where CEDULA = @Cedula
                                  and MODULO = @Modulo
                                  and CODIGO = @CodPlan
                                  and DOCUMENTO = @CodContrato
                                  and ID_ERROR = @IdError;
                                """;

                            int? lineaExistente = connection.QueryFirstOrDefault<int?>(
                                sqlExistente,
                                parametrosClave,
                                transaction);

                            if (lineaExistente.HasValue)
                            {
                                transaction.Commit();

                                return DbHelper.CreateOkResponse(
                                    new FndSeguimientoRevisionOmisionCambiarData
                                    {
                                        linea_err = lineaExistente,
                                        seleccionado = true
                                    });
                            }

                            const string sqlOmision = """
                                select count(1)
                                from SIF_OMISIONES E
                                where E.ID_ERROR = @IdError
                                  and E.ACTIVO = '1'
                                  and exists (
                                      select 1
                                      from SIF_OMISIONES_MODULOS OM
                                      where OM.ID_ERROR = E.ID_ERROR
                                        and OM.COD_MODULO = @Modulo
                                  );
                                """;

                            int existeOmision = connection.ExecuteScalar<int>(
                                sqlOmision,
                                new
                                {
                                    IdError = request.id_error,
                                    Modulo
                                },
                                transaction);

                            if (existeOmision == 0)
                            {
                                transaction.Rollback();

                                return CrearErrorCambioOmision(
                                    "La omisi&oacute;n seleccionada no es v&aacute;lida.",
                                    -2);
                            }

                            const string sqlInsertar = """
                                insert into SIF_OMISIONESG
                                (
                                    CEDULA,
                                    ID_ERROR,
                                    MODULO,
                                    CODIGO,
                                    DOCUMENTO,
                                    REGISTRO_FECHA,
                                    REGISTRO_USUARIO
                                )
                                output inserted.LINEA_ERR
                                values
                                (
                                    @Cedula,
                                    @IdError,
                                    @Modulo,
                                    @CodPlan,
                                    @CodContrato,
                                    Getdate(),
                                    @Usuario
                                );
                                """;

                            int linea = connection.ExecuteScalar<int>(
                                sqlInsertar,
                                new
                                {
                                    Cedula = request.cedula.Trim(),
                                    IdError = request.id_error,
                                    Modulo,
                                    CodPlan = request.cod_plan.Trim(),
                                    CodContrato = request.cod_contrato.ToString(),
                                    Usuario = request.usuario.Trim()
                                },
                                transaction);

                            transaction.Commit();

                            return DbHelper.CreateOkResponse(
                                new FndSeguimientoRevisionOmisionCambiarData
                                {
                                    linea_err = linea,
                                    seleccionado = true
                                });
                        }

                        const string sqlAplicado = """
                            select top 1
                                isnull(APLICADO, 'N')
                            from SIF_OMISIONESG
                            where CEDULA = @Cedula
                              and MODULO = @Modulo
                              and CODIGO = @CodPlan
                              and DOCUMENTO = @CodContrato
                              and ID_ERROR = @IdError;
                            """;

                        string aplicado = connection.QueryFirstOrDefault<string>(
                            sqlAplicado,
                            parametrosClave,
                            transaction) ?? "N";

                        if (aplicado.Equals("S", StringComparison.OrdinalIgnoreCase))
                        {
                            transaction.Rollback();

                            return DbHelper.CreateErrorResponse(
                                "La omisi&oacute;n ya fue aplicada y no puede eliminarse.",
                                -2,
                                new FndSeguimientoRevisionOmisionCambiarData
                                {
                                    seleccionado = true
                                });
                        }

                        const string sqlEliminar = """
                            delete from SIF_OMISIONESG
                            where CEDULA = @Cedula
                              and MODULO = @Modulo
                              and CODIGO = @CodPlan
                              and DOCUMENTO = @CodContrato
                              and ID_ERROR = @IdError
                              and isnull(APLICADO, 'N') <> 'S';
                            """;

                        connection.Execute(
                            sqlEliminar,
                            parametrosClave,
                            transaction);

                        transaction.Commit();

                        return DbHelper.CreateOkResponse(
                            new FndSeguimientoRevisionOmisionCambiarData
                            {
                                linea_err = null,
                                seleccionado = false
                            });
                    });

            if (ejecucion.Code == -1)
            {
                return CrearErrorCambioOmision(ejecucion.Description, -1);
            }

            return ejecucion.Result
                ?? CrearErrorCambioOmision(
                    "No fue posible procesar la omisi&oacute;n.",
                    -1);
        }

        /// <summary>
        /// Aplica la etiqueta y marca las omisiones del contrato como aplicadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            FND_frmFNDSeguimientoRevisionesTags_Aplicar(
                int codEmpresa,
                FndSeguimientoRevisionAplicarRequest? request)
        {
            string? validacion = ValidarAplicar(request);

            if (validacion is not null)
            {
                return DbHelper.ErrorResponse(validacion, -2);
            }

            ErrorDto<ErrorDto> ejecucion = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    const string sqlContrato = """
                        select count(1)
                        from FND_CONTRATOS F
                        where F.CEDULA = @Cedula
                          and F.COD_OPERADORA = @CodOperadora
                          and F.COD_PLAN = @CodPlan
                          and F.COD_CONTRATO = @CodContrato
                          and isnull(F.ANALISTA_REVISION, 'N') = 'N'
                          and F.ESTADO = 'A';
                        """;

                    int existeContrato = connection.ExecuteScalar<int>(
                        sqlContrato,
                        new
                        {
                            Cedula = request.cedula.Trim(),
                            CodOperadora = request.cod_operadora,
                            CodPlan = request.cod_plan.Trim(),
                            CodContrato = request.cod_contrato
                        },
                        transaction);

                    if (existeContrato == 0)
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            "El contrato seleccionado no se encuentra pendiente de revisi&oacute;n.",
                            -2);
                    }

                    const string sqlEtiqueta = """
                        select count(1)
                        from SIF_TAGS T
                        where T.TAG_CODIGO = @TagCodigo
                          and T.ACTIVO = 1
                          and exists (
                              select 1
                              from SIF_TAGS_MODULOS TM
                              where TM.TAG_CODIGO = T.TAG_CODIGO
                                and TM.COD_MODULO = @Modulo
                          )
                          and exists (
                              select 1
                              from SIF_TAGS_GRUPOS TG
                              inner join SIF_GRPUSERS GU
                                  on TG.COD_GRUPO = GU.COD_GRUPO
                              where TG.TAG_CODIGO = T.TAG_CODIGO
                                and GU.USUARIO = @Usuario
                          );
                        """;

                    int existeEtiqueta = connection.ExecuteScalar<int>(
                        sqlEtiqueta,
                        new
                        {
                            TagCodigo = request.tag_codigo.Trim(),
                            Modulo,
                            Usuario = request.usuario.Trim()
                        },
                        transaction);

                    if (existeEtiqueta == 0)
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            "La etiqueta seleccionada no es v&aacute;lida para el usuario.",
                            -2);
                    }

                    connection.Execute(
                        "spSIFRegistraTags",
                        new
                        {
                            Codigo = request.cod_plan.Trim(),
                            Tag = request.tag_codigo.Trim(),
                            Usuario = request.usuario.Trim(),
                            Notas = request.observacion.Trim(),
                            Documento = request.cod_contrato.ToString(),
                            Modulo,
                            Llave_01 = request.cod_plan.Trim(),
                            Llave_02 = request.cod_contrato.ToString(),
                            Llave_03 = request.cedula.Trim()
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure);

                    const string sqlAplicarOmisiones = """
                        update SIF_OMISIONESG
                           set APLICADO = 'S'
                        where CEDULA = @Cedula
                          and MODULO = @Modulo
                          and CODIGO = @CodPlan
                          and DOCUMENTO = @CodContrato;
                        """;

                    connection.Execute(
                        sqlAplicarOmisiones,
                        new
                        {
                            Cedula = request.cedula.Trim(),
                            Modulo,
                            CodPlan = request.cod_plan.Trim(),
                            CodContrato = request.cod_contrato.ToString()
                        },
                        transaction);

                    transaction.Commit();

                    return DbHelper.OkResponse(
                        "Proceso concluido con &eacute;xito.");
                });

            if (ejecucion.Code == -1)
            {
                return DbHelper.ErrorResponse(ejecucion.Description, -1);
            }

            return ejecucion.Result
                ?? DbHelper.ErrorResponse(
                    "No fue posible aplicar la revisi&oacute;n.",
                    -1);
        }

        private static object CrearParametrosClave(
            FndSeguimientoRevisionClaveRequest request)
        {
            return new
            {
                Cedula = request.cedula.Trim(),
                CodOperadora = request.cod_operadora,
                CodPlan = request.cod_plan.Trim(),
                CodContrato = request.cod_contrato
            };
        }

        private static ErrorDto<FndSeguimientoRevisionOmisionCambiarData>
            CrearErrorCambioOmision(string mensaje, int codigo)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                codigo,
                new FndSeguimientoRevisionOmisionCambiarData());
        }

        private static string? ValidarClave(
            FndSeguimientoRevisionClaveRequest? request)
        {
            if (request is null)
            {
                return "La solicitud es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return "Debe indicar una c&eacute;dula v&aacute;lida.";
            }

            if (request.cod_contrato <= 0)
            {
                return "Debe indicar un contrato v&aacute;lido.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_plan))
            {
                return "Debe indicar un plan v&aacute;lido.";
            }

            if (request.cod_operadora <= 0)
            {
                return "Debe indicar una operadora v&aacute;lida.";
            }

            return null;
        }

        private static string? ValidarCambioOmision(
            FndSeguimientoRevisionOmisionCambiarRequest? request)
        {
            if (request is null)
            {
                return "La solicitud es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return "Debe indicar una c&eacute;dula v&aacute;lida.";
            }

            if (request.cod_contrato <= 0)
            {
                return "Debe indicar un contrato v&aacute;lido.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_plan))
            {
                return "Debe indicar un plan v&aacute;lido.";
            }

            if (request.id_error <= 0)
            {
                return "Debe indicar una omisi&oacute;n v&aacute;lida.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "Debe indicar un usuario v&aacute;lido.";
            }

            return null;
        }

        private static string? ValidarAplicar(
            FndSeguimientoRevisionAplicarRequest? request)
        {
            if (request is null)
            {
                return "La solicitud es requerida.";
            }

            string? validacionClave = ValidarClave(
                new FndSeguimientoRevisionClaveRequest
                {
                    cedula = request.cedula,
                    cod_contrato = request.cod_contrato,
                    cod_plan = request.cod_plan,
                    cod_operadora = request.cod_operadora
                });

            if (validacionClave is not null)
            {
                return validacionClave;
            }

            if (string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return "Debe seleccionar la etiqueta que desea aplicar.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "Debe indicar un usuario v&aacute;lido.";
            }

            return null;
        }

        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }
    }
}