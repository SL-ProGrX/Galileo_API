using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmAfBenSeguimientoRevisionesTagDb
    {
        private const string Modulo = "BEN";
        private readonly PortalDB _portalDb;

        public FrmAfBenSeguimientoRevisionesTagDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los beneficios pendientes de revision.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula">.</param>
        /// <returns></returns>
        public ErrorDto<List<AfBenSeguimientoBeneficioData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Beneficios_Obtener(
                int codEmpresa,
                string? cedula)
        {
            const string sql = """
                select top 3000
                    isnull(rtrim(B.CEDULA), '') as cedula,
                    isnull(rtrim(S.NOMBRE), '') as nombre,
                    isnull(rtrim(B.REGISTRA_USER), '')
                        as registra_user,
                    B.REGISTRA_FECHA as registra_fecha,
                    isnull(rtrim(B.COD_BENEFICIO), '')
                        as cod_beneficio,
                    B.CONSEC as consec
                from AFI_BENE_OTORGA B
                inner join SOCIOS S
                    on B.CEDULA = S.CEDULA
                where B.ANALISTA_REVISION is null
                  and B.ANALISTA_RECEPCION is not null
                  and (
                      @Cedula = ''
                      or B.CEDULA = @Cedula
                  )
                order by
                    B.REGISTRA_FECHA desc,
                    B.CONSEC desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfBenSeguimientoBeneficioData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Cedula = NormalizarTexto(cedula)
                    });
        }

        /// <summary>
        /// Obtiene el historial de etiquetas registrado para un beneficio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request">.</param>
        /// <returns></returns>
        public ErrorDto<List<AfBenSeguimientoRegistroData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Seguimiento_Obtener(
                int codEmpresa,
                AfBenSeguimientoClaveRequest? request)
        {
            string? validacion = ValidarClave(request);

            if (validacion is not null)
            {
                return DbHelper.CreateErrorResponse<
                    List<AfBenSeguimientoRegistroData>>(
                        validacion,
                        -2,
                        []);
            }

            const string sql = """
                select
                    isnull(rtrim(T.DESCRIPCION), '')
                        as descripcion,
                    isnull(rtrim(CT.NOTAS), '')
                        as notas,
                    CT.REGISTRO_FECHA
                        as registro_fecha,
                    isnull(rtrim(CT.REGISTRO_USUARIO), '')
                        as registro_usuario
                from SIF_CONTROL_TAGS CT
                inner join SIF_TAGS T
                    on CT.TAG_CODIGO = T.TAG_CODIGO
                where CT.DOCUMENTO = @Consecutivo
                  and CT.CODIGO = @CodBeneficio
                  and CT.COD_MODULO = @Modulo
                order by CT.REGISTRO_FECHA desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfBenSeguimientoRegistroData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Consecutivo =
                            request.consec.ToString(),
                        CodBeneficio =
                            request.cod_beneficio.Trim(),
                        Modulo
                    });
        }

        /// <summary>
        /// Obtiene las etiquetas activas del m&oacute;dulo BEN autorizadas
        /// para el usuario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            AF_frmAF_BenSeguimientoRevisionesTag_Etiquetas_Obtener(
                int codEmpresa,
                string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<
                    List<DropDownListaGenericaModel>>(
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

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
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
            AF_frmAF_BenSeguimientoRevisionesTag_Aviso_Obtener(
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

            var resultado = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                string.Empty,
                new
                {
                    TagCodigo = tagCodigo.Trim()
                });

            if (resultado.Code == -1)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description,
                    -1);
            }

            return DbHelper.OkResponse(
                resultado.Result ?? string.Empty);
        }

        /// <summary>
        /// Obtiene las omisiones disponibles y las asignadas al beneficio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBenSeguimientoOmisionData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Omisiones_Obtener(
                int codEmpresa,
                AfBenSeguimientoClaveRequest? request)
        {
            string? validacion = ValidarClave(request);

            if (validacion is not null)
            {
                return DbHelper.CreateErrorResponse<
                    List<AfBenSeguimientoOmisionData>>(
                        validacion,
                        -2,
                        []);
            }

            const string sql = """
                select
                    E.ID_ERROR as id_error,
                    isnull(rtrim(E.DESCRIPCION), '')
                        as descripcion,
                    ER.ID_ERROR as asignado,
                    isnull(rtrim(ER.APLICADO), 'N')
                        as aplicado,
                    isnull(rtrim(E.MENSAJE), '')
                        as mensaje,
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
                   and ER.CODIGO = @CodBeneficio
                   and ER.DOCUMENTO = @Consecutivo
                where E.ACTIVO = '1'
                  and exists (
                      select 1
                      from SIF_OMISIONES_MODULOS OM
                      where OM.ID_ERROR = E.ID_ERROR
                        and OM.COD_MODULO = @Modulo
                  )
                order by E.ID_ERROR;
                """;

            return DbHelper.ExecuteListQuery<
                AfBenSeguimientoOmisionData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Cedula = request.cedula.Trim(),
                        CodBeneficio =
                            request.cod_beneficio.Trim(),
                        Consecutivo =
                            request.consec.ToString(),
                        Modulo
                    });
        }

        /// <summary>
        /// Registra o elimina una omision asociada al beneficio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<AfBenSeguimientoOmisionCambiarData>
            AF_frmAF_BenSeguimientoRevisionesTag_Omision_Cambiar(
                int codEmpresa,
                AfBenSeguimientoOmisionCambiarRequest? request)
        {
            string? validacion = ValidarCambioOmision(request);

            if (validacion is not null)
            {
                return DbHelper.CreateErrorResponse<
                    AfBenSeguimientoOmisionCambiarData>(
                        validacion,
                        -2,
                        new AfBenSeguimientoOmisionCambiarData());
            }

            var ejecucion = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    const string sqlBeneficio = """
                        select count(1)
                        from AFI_BENE_OTORGA
                        where CEDULA = @Cedula
                          and COD_BENEFICIO = @CodBeneficio
                          and CONSEC = @Consecutivo
                          and ANALISTA_REVISION is null 
                          and ANALISTA_RECEPCION is not null;
                        """;

                    int existeBeneficio =
                        connection.ExecuteScalar<int>(
                            sqlBeneficio,
                            new
                            {
                                Cedula =
                                    request.cedula.Trim(),
                                CodBeneficio =
                                    request.cod_beneficio.Trim(),
                                Consecutivo =
                                    request.consec
                            },
                            transaction);

                    if (existeBeneficio == 0)
                    {
                        transaction.Rollback();

                        return DbHelper.CreateErrorResponse<
                            AfBenSeguimientoOmisionCambiarData>(
                                "El beneficio seleccionado no se encuentra pendiente de revisi&oacute;n.",
                                -2,
                                new AfBenSeguimientoOmisionCambiarData());
                    }

                    var parametrosClave = new
                    {
                        Cedula =
                            request.cedula.Trim(),
                        Modulo,
                        CodBeneficio =
                            request.cod_beneficio.Trim(),
                        Consecutivo =
                            request.consec.ToString(),
                        IdError =
                            request.id_error
                    };

                    if (request.seleccionado)
                    {
                        const string sqlExistente = """
                            select top 1
                                LINEA_ERR
                            from SIF_OMISIONESG
                            where CEDULA = @Cedula
                              and MODULO = @Modulo
                              and CODIGO = @CodBeneficio
                              and DOCUMENTO = @Consecutivo
                              and ID_ERROR = @IdError;
                            """;

                        int? lineaExistente =
                            connection.QueryFirstOrDefault<int?>(
                                sqlExistente,
                                parametrosClave,
                                transaction);

                        if (lineaExistente.HasValue)
                        {
                            transaction.Commit();

                            return DbHelper.CreateOkResponse(
                                new AfBenSeguimientoOmisionCambiarData
                                {
                                    linea_err =
                                        lineaExistente,
                                    seleccionado = true
                                });
                        }

                        const string sqlError = """
                            select count(1)
                            from SIF_OMISIONES E
                            where E.ID_ERROR = @IdError
                              and E.ACTIVO = '1'
                              and exists (
                                  select 1
                                  from SIF_OMISIONES_MODULOS M
                                  where M.ID_ERROR =
                                        E.ID_ERROR
                                    and M.COD_MODULO =
                                        @Modulo
                              );
                            """;

                        int existeError =
                            connection.ExecuteScalar<int>(
                                sqlError,
                                new
                                {
                                    IdError =
                                        request.id_error,
                                    Modulo
                                },
                                transaction);

                        if (existeError == 0)
                        {
                            transaction.Rollback();

                            return DbHelper.CreateErrorResponse<
                                AfBenSeguimientoOmisionCambiarData>(
                                    "La omisi&oacute;n seleccionada no es v&aacute;lida.",
                                    -2,
                                    new AfBenSeguimientoOmisionCambiarData());
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
                                @CodBeneficio,
                                @Consecutivo,
                                getdate(),
                                @Usuario
                            );
                            """;

                        int linea =
                            connection.ExecuteScalar<int>(
                                sqlInsertar,
                                new
                                {
                                    Cedula =
                                        request.cedula.Trim(),
                                    IdError =
                                        request.id_error,
                                    Modulo,
                                    CodBeneficio =
                                        request.cod_beneficio.Trim(),
                                    Consecutivo =
                                        request.consec.ToString(),
                                    Usuario =
                                        request.usuario.Trim()
                                },
                                transaction);

                        transaction.Commit();

                        return DbHelper.CreateOkResponse(
                            new AfBenSeguimientoOmisionCambiarData
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
                          and CODIGO = @CodBeneficio
                          and DOCUMENTO = @Consecutivo
                          and ID_ERROR = @IdError;
                        """;

                    string aplicado =
                        connection.QueryFirstOrDefault<string>(
                            sqlAplicado,
                            parametrosClave,
                            transaction) ?? "N";

                    if (aplicado.Equals(
                        "S",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        transaction.Rollback();

                        return DbHelper.CreateErrorResponse<
                            AfBenSeguimientoOmisionCambiarData>(
                                "La omisi&oacute;n ya fue aplicada y no puede eliminarse.",
                                -2,
                                new AfBenSeguimientoOmisionCambiarData
                                {
                                    seleccionado = true
                                });
                    }

                    const string sqlEliminar = """
                        delete from SIF_OMISIONESG
                        where CEDULA = @Cedula
                          and MODULO = @Modulo
                          and CODIGO = @CodBeneficio
                          and DOCUMENTO = @Consecutivo
                          and ID_ERROR = @IdError
                          and isnull(APLICADO, 'N') <> 'S';
                        """;

                    connection.Execute(
                        sqlEliminar,
                        parametrosClave,
                        transaction);

                    transaction.Commit();

                    return DbHelper.CreateOkResponse(
                        new AfBenSeguimientoOmisionCambiarData
                        {
                            linea_err = null,
                            seleccionado = false
                        });
                });

            if (ejecucion.Code == -1)
            {
                return DbHelper.CreateErrorResponse<
                    AfBenSeguimientoOmisionCambiarData>(
                        ejecucion.Description,
                        -1,
                        new AfBenSeguimientoOmisionCambiarData());
            }

            return ejecucion.Result
                ?? DbHelper.CreateErrorResponse<
                    AfBenSeguimientoOmisionCambiarData>(
                        "No fue posible procesar la omisi&oacute;n.",
                        -1,
                        new AfBenSeguimientoOmisionCambiarData());
        }

        /// <summary>
        /// Aplica la etiqueta al beneficio y marca las omisiones
        /// como aplicadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            AF_frmAF_BenSeguimientoRevisionesTag_Aplicar(
                int codEmpresa,
                AfBenSeguimientoAplicarRequest? request)
        {
            string? validacion = ValidarAplicar(request);

            if (validacion is not null)
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    -2);
            }

            var ejecucion = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    const string sqlBeneficio = """
                        select count(1)
                        FROM AFI_BENE_OTORGA B
                        WHERE B.CEDULA = @Cedula
                          AND B.COD_BENEFICIO = @CodBeneficio
                          AND B.CONSEC = @Consecutivo
                          AND ISNULL(B.ANALISTA_REVISION, 'N') = 'N'
                          AND B.ANALISTA_RECEPCION IS NOT NULL
                        """;

                    int existeBeneficio =
                        connection.ExecuteScalar<int>(
                            sqlBeneficio,
                            new
                            {
                                Cedula =
                                    request.cedula.Trim(),
                                CodBeneficio =
                                    request.cod_beneficio.Trim(),
                                Consecutivo =
                                    request.consec
                            },
                            transaction);

                    if (existeBeneficio == 0)
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            "El beneficio seleccionado no se encuentra pendiente de revisi&oacute;n.",
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
                              where TM.TAG_CODIGO =
                                    T.TAG_CODIGO
                                and TM.COD_MODULO =
                                    @Modulo
                          )
                          and exists (
                              select 1
                              from SIF_TAGS_GRUPOS TG
                              inner join SIF_GRPUSERS GU
                                  on TG.COD_GRUPO =
                                     GU.COD_GRUPO
                              where TG.TAG_CODIGO =
                                    T.TAG_CODIGO
                                and GU.USUARIO =
                                    @Usuario
                          );
                        """;

                    int existeEtiqueta =
                        connection.ExecuteScalar<int>(
                            sqlEtiqueta,
                            new
                            {
                                TagCodigo =
                                    request.tag_codigo.Trim(),
                                Modulo,
                                Usuario =
                                    request.usuario.Trim()
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
                            Codigo =
                                request.cod_beneficio.Trim(),
                            Tag =
                                request.tag_codigo.Trim(),
                            Usuario =
                                request.usuario.Trim(),
                            Notas =
                                request.observacion.Trim(),
                            Documento =
                                request.consec.ToString(),
                            Modulo,
                            Llave_01 =
                                request.cod_beneficio.Trim(),
                            Llave_02 =
                                request.consec.ToString(),
                            Llave_03 =
                                request.cedula.Trim()
                        },
                        transaction,
                        commandType:
                            CommandType.StoredProcedure);

                    const string sqlAplicarOmisiones = """
                        update SIF_OMISIONESG
                           set APLICADO = 'S'
                        where CEDULA = @Cedula
                          and MODULO = @Modulo
                          and CODIGO = @CodBeneficio
                          and DOCUMENTO = @Consecutivo;
                        """;

                    connection.Execute(
                        sqlAplicarOmisiones,
                        new
                        {
                            Cedula =
                                request.cedula.Trim(),
                            Modulo,
                            CodBeneficio =
                                request.cod_beneficio.Trim(),
                            Consecutivo =
                                request.consec.ToString()
                        },
                        transaction);

                    transaction.Commit();

                    return new ErrorDto
                    {
                        Code = 1,
                        Description =
                            "Proceso concluido con exito."
                    };
                });

            if (ejecucion.Code == -1)
            {
                return DbHelper.ErrorResponse(
                    ejecucion.Description,
                    -1);
            }

            return ejecucion.Result
                ?? DbHelper.ErrorResponse(
                    "No fue posible aplicar la revisi&oacute;n.",
                    -1);
        }

        private static string? ValidarClave(
            AfBenSeguimientoClaveRequest? request)
        {
            if (request is null)
            {
                return "La solicitud es requerida.";
            }

            if (string.IsNullOrWhiteSpace(
                request.cedula))
            {
                return "Debe indicar una c&eacute;dula v&aacute;lida.";
            }

            if (string.IsNullOrWhiteSpace(
                request.cod_beneficio))
            {
                return "Debe indicar el beneficio.";
            }

            if (request.consec <= 0)
            {
                return "Debe indicar un c&oacute;digo de beneficio v&aacute;lido.";
            }

            return null;
        }

        private static string? ValidarCambioOmision(
            AfBenSeguimientoOmisionCambiarRequest? request)
        {
            if (request is null)
            {
                return "La solicitud es requerida.";
            }

            string? validacionClave =
                ValidarClave(
                    new AfBenSeguimientoClaveRequest
                    {
                        cedula =
                            request.cedula,
                        cod_beneficio =
                            request.cod_beneficio,
                        consec =
                            request.consec
                    });

            if (validacionClave is not null)
            {
                return validacionClave;
            }

            if (request.id_error <= 0)
            {
                return "Debe indicar una omisi&oacute;n v&aacute;lida.";
            }

            if (string.IsNullOrWhiteSpace(
                request.usuario))
            {
                return "Debe indicar un usuario v&aacute;lido.";
            }

            return null;
        }

        private static string? ValidarAplicar(
            AfBenSeguimientoAplicarRequest? request)
        {
            if (request is null)
            {
                return "La solicitud es requerida.";
            }

            string? validacionClave =
                ValidarClave(
                    new AfBenSeguimientoClaveRequest
                    {
                        cedula =
                            request.cedula,
                        cod_beneficio =
                            request.cod_beneficio,
                        consec =
                            request.consec
                    });

            if (validacionClave is not null)
            {
                return validacionClave;
            }

            if (string.IsNullOrWhiteSpace(
                request.tag_codigo))
            {
                return "Debe seleccionar la etiqueta que desea aplicar.";
            }

            if (string.IsNullOrWhiteSpace(
                request.usuario))
            {
                return "Debe indicar un usuario v&aacute;lido.";
            }

            return null;
        }

        private static string NormalizarTexto(
            string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }
    }
}