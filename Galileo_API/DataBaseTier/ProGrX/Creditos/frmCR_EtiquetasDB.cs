using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrEtiquetasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 3;
        private const string guardadoExitoso = "Informacion guardada satisfactoriamente...";
        private const string eliminadoExitoso = "Informacion eliminada satisfactoriamente...";
        private const string guardadoNotificacionExitoso = "Informacion almacenada con exito!";
        private const string eliminadoNotificacionExitoso = "La notificacion ha sido eliminada!";

        public FrmCrEtiquetasDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrEtiquetasDb(PortalDB portalDb, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDb;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el catalogo principal de etiquetas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrEtiquetaData>> CrEtiquetas_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(T.TAG_CODIGO) as tag_codigo,
                    rtrim(isnull(T.DESCRIPCION, '')) as descripcion,
                    rtrim(isnull(T.COD_REQUISITO, '')) as cod_requisito,
                    isnull(rtrim(T.COD_REQUISITO) + ' - ' + rtrim(R.DESCRIPCION), '') as requisito_descripcion,
                    isnull(T.NOTA_LARGO, 0) as nota_largo,
                    cast(isnull(T.ESPERA_ACTIVA, 0) as bit) as espera_activa,
                    cast(isnull(T.ESPERA_DESACTIVA, 0) as bit) as espera_desactiva,
                    cast(isnull(T.ACTIVO, 0) as bit) as activo
                from CRD_TAGS T
                left join REQUISITOS_ADICIONALES R on T.COD_REQUISITO = R.COD_REQUISITO
                order by T.TAG_CODIGO;";

            return DbHelper.ExecuteListQuery<CrEtiquetaData>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Obtiene los requisitos para el combo de etiquetas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrEtiquetas_Requisitos_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(COD_REQUISITO) as item,
                    rtrim(COD_REQUISITO) + ' - ' + rtrim(DESCRIPCION) as descripcion
                from REQUISITOS_ADICIONALES
                order by COD_REQUISITO;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Obtiene las etiquetas para combos de notificacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrEtiquetas_TagsCombo_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(TAG_CODIGO) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CRD_TAGS
                order by TAG_CODIGO;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlQuery
            );
        }

        /// <summary>
        /// Obtiene la notificacion asociada a una etiqueta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tagCodigo"></param>
        /// <returns></returns>
        public ErrorDto<CrEtiquetaNotificacionData> CrEtiquetas_Notificacion_Obtener(int codEmpresa, string tagCodigo)
        {
            tagCodigo = LimpiarCodigo(tagCodigo);

            if (string.IsNullOrWhiteSpace(tagCodigo))
            {
                return new ErrorDto<CrEtiquetaNotificacionData>
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la etiqueta."
                };
            }

            const string sqlQuery = @"
                select
                    rtrim(CT.TAG_CODIGO) as tag_codigo,
                    rtrim(isnull(CT.PARA_TAG, '')) as para_tag,
                    rtrim(isnull(CT.PARA_EMAIL, '')) as para_email,
                    rtrim(isnull(CT.CC_TAG, '')) as cc_tag,
                    rtrim(isnull(CT.CC_EMAIL, '')) as cc_email,
                    isnull(CT.MENSAJE, '') as mensaje
                from CRD_TAGS_AVISOS CT
                where CT.TAG_CODIGO = @TagCodigo;";

            var resp = DbHelper.ExecuteListQuery<CrEtiquetaNotificacionData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new
                {
                    TagCodigo = tagCodigo
                });

            if (resp.Code < 0)
            {
                return new ErrorDto<CrEtiquetaNotificacionData>
                {
                    Code = resp.Code,
                    Description = resp.Description
                };
            }

            return new ErrorDto<CrEtiquetaNotificacionData>
            {
                Code = 0,
                Description = resp.Description,
                Result = resp.Result?.FirstOrDefault() ?? new CrEtiquetaNotificacionData
                {
                    tag_codigo = tagCodigo
                }
            };
        }

        /// <summary>
        /// Guarda una etiqueta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrEtiquetas_Guardar(int codEmpresa, CrEtiquetaGuardarRequest request)
        {
            request.usuario = LimpiarTexto(request.usuario);
            request.etiqueta.tag_codigo = LimpiarCodigo(request.etiqueta.tag_codigo);
            request.etiqueta.descripcion = LimpiarTexto(request.etiqueta.descripcion);
            request.etiqueta.cod_requisito = LimpiarCodigo(request.etiqueta.cod_requisito);

            if (string.IsNullOrWhiteSpace(request.etiqueta.tag_codigo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la etiqueta."
                };
            }

            if (request.etiqueta.tag_codigo.StartsWith("S", StringComparison.OrdinalIgnoreCase))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "No se puede agregar etiquetas que inicien con 'S', reservadas para el sistema."
                };
            }

            var existe = ExisteEtiqueta(codEmpresa, request.etiqueta.tag_codigo);

            return existe
                ? ActualizarEtiqueta(codEmpresa, request)
                : InsertarEtiqueta(codEmpresa, request);
        }

        /// <summary>
        /// Elimina una etiqueta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrEtiquetas_Eliminar(int codEmpresa, CrEtiquetaEliminarRequest request)
        {
            request.usuario = LimpiarTexto(request.usuario);
            request.tag_codigo = LimpiarCodigo(request.tag_codigo);

            if (string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la etiqueta."
                };
            }

            const string sqlDeleteAviso = @"
                delete from CRD_TAGS_AVISOS
                where TAG_CODIGO = @TagCodigo;";

            var respAviso = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDeleteAviso,
                new
                {
                    TagCodigo = request.tag_codigo
                });

            if (respAviso.Code < 0)
                return respAviso;

            const string sqlDeleteTag = @"
                delete from CRD_TAGS
                where TAG_CODIGO = @TagCodigo;";

            var respTag = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDeleteTag,
                new
                {
                    TagCodigo = request.tag_codigo
                });

            if (respTag.Code < 0)
                return respTag;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Elimina - WEB",
                $"Tipo de Etiqueta : {request.tag_codigo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = eliminadoExitoso
            };
        }

        /// <summary>
        /// Guarda la notificacion de una etiqueta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrEtiquetas_Notificacion_Guardar(
            int codEmpresa,
            CrEtiquetaNotificacionGuardarRequest request)
        {
            request.notificacion.tag_codigo = LimpiarCodigo(request.notificacion.tag_codigo);
            request.notificacion.para_tag = LimpiarCodigo(request.notificacion.para_tag);
            request.notificacion.cc_tag = LimpiarCodigo(request.notificacion.cc_tag);
            request.notificacion.para_email = LimpiarTexto(request.notificacion.para_email);
            request.notificacion.cc_email = LimpiarTexto(request.notificacion.cc_email);
            request.notificacion.mensaje = (request.notificacion.mensaje ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.notificacion.tag_codigo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la etiqueta."
                };
            }

            var existe = ExisteNotificacion(codEmpresa, request.notificacion.tag_codigo);

            return existe
                ? ActualizarNotificacion(codEmpresa, request.notificacion)
                : InsertarNotificacion(codEmpresa, request.notificacion);
        }

        /// <summary>
        /// Elimina la notificacion de una etiqueta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrEtiquetas_Notificacion_Eliminar(
            int codEmpresa,
            CrEtiquetaNotificacionEliminarRequest request)
        {
            request.tag_codigo = LimpiarCodigo(request.tag_codigo);

            if (string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el codigo de la etiqueta."
                };
            }

            const string sqlDelete = @"
                delete from CRD_TAGS_AVISOS
                where TAG_CODIGO = @TagCodigo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    TagCodigo = request.tag_codigo
                });

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = eliminadoNotificacionExitoso
            };
        }

        private ErrorDto InsertarEtiqueta(int codEmpresa, CrEtiquetaGuardarRequest request)
        {
            const string sqlInsert = @"
                insert into CRD_TAGS
                (
                    TAG_CODIGO,
                    DESCRIPCION,
                    COD_REQUISITO,
                    NOTA_LARGO,
                    ESPERA_ACTIVA,
                    ESPERA_DESACTIVA,
                    ACTIVO
                )
                values
                (
                    @TagCodigo,
                    @Descripcion,
                    @CodRequisito,
                    @NotaLargo,
                    @EsperaActiva,
                    @EsperaDesactiva,
                    @Activo
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                CrearParametrosEtiqueta(request.etiqueta)
            );

            return FinalizarGuardadoEtiqueta(
                codEmpresa,
                request.usuario,
                request.etiqueta.tag_codigo,
                "Registra - WEB",
                resp
            );
        }

        private ErrorDto ActualizarEtiqueta(int codEmpresa, CrEtiquetaGuardarRequest request)
        {
            const string sqlUpdate = @"
                update CRD_TAGS
                set DESCRIPCION = @Descripcion,
                    COD_REQUISITO = @CodRequisito,
                    NOTA_LARGO = @NotaLargo,
                    ESPERA_ACTIVA = @EsperaActiva,
                    ESPERA_DESACTIVA = @EsperaDesactiva,
                    ACTIVO = @Activo
                where TAG_CODIGO = @TagCodigo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                CrearParametrosEtiqueta(request.etiqueta)
            );

            return FinalizarGuardadoEtiqueta(
                codEmpresa,
                request.usuario,
                request.etiqueta.tag_codigo,
                "Modifica - WEB",
                resp
            );
        }

        private static object CrearParametrosEtiqueta(CrEtiquetaData etiqueta)
        {
            return new
            {
                TagCodigo = etiqueta.tag_codigo,
                Descripcion = etiqueta.descripcion,
                CodRequisito = ValorNulo(etiqueta.cod_requisito),
                NotaLargo = etiqueta.nota_largo,
                EsperaActiva = etiqueta.espera_activa ? 1 : 0,
                EsperaDesactiva = etiqueta.espera_desactiva ? 1 : 0,
                Activo = etiqueta.activo ? 1 : 0
            };
        }

        private ErrorDto FinalizarGuardadoEtiqueta(
            int codEmpresa,
            string usuario,
            string tagCodigo,
            string movimiento,
            ErrorDto resp)
        {
            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento,
                $"Tipo de Etiqueta : {tagCodigo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = guardadoExitoso
            };
        }

        private ErrorDto InsertarNotificacion(int codEmpresa, CrEtiquetaNotificacionData notificacion)
        {
            const string sqlInsert = @"
                insert into CRD_TAGS_AVISOS
                (
                    TAG_CODIGO,
                    PARA_TAG,
                    PARA_EMAIL,
                    CC_TAG,
                    CC_EMAIL,
                    MENSAJE
                )
                values
                (
                    @TagCodigo,
                    @ParaTag,
                    @ParaEmail,
                    @CcTag,
                    @CcEmail,
                    @Mensaje
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                CrearParametrosNotificacion(notificacion)
            );

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = guardadoNotificacionExitoso
            };
        }

        private ErrorDto ActualizarNotificacion(int codEmpresa, CrEtiquetaNotificacionData notificacion)
        {
            const string sqlUpdate = @"
                update CRD_TAGS_AVISOS
                set PARA_TAG = @ParaTag,
                    PARA_EMAIL = @ParaEmail,
                    CC_TAG = @CcTag,
                    CC_EMAIL = @CcEmail,
                    MENSAJE = @Mensaje
                where TAG_CODIGO = @TagCodigo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                CrearParametrosNotificacion(notificacion)
            );

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = guardadoNotificacionExitoso
            };
        }

        private static object CrearParametrosNotificacion(CrEtiquetaNotificacionData notificacion)
        {
            return new
            {
                TagCodigo = notificacion.tag_codigo,
                ParaTag = ValorNulo(notificacion.para_tag),
                ParaEmail = notificacion.para_email,
                CcTag = ValorNulo(notificacion.cc_tag),
                CcEmail = notificacion.cc_email,
                Mensaje = notificacion.mensaje
            };
        }

        private bool ExisteEtiqueta(int codEmpresa, string tagCodigo)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0)
                from CRD_TAGS
                where TAG_CODIGO = @TagCodigo;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    TagCodigo = tagCodigo
                });

            return resp.Result > 0;
        }

        private bool ExisteNotificacion(int codEmpresa, string tagCodigo)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0)
                from CRD_TAGS_AVISOS
                where TAG_CODIGO = @TagCodigo;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    TagCodigo = tagCodigo
                });

            return resp.Result > 0;
        }

        private static string LimpiarCodigo(string valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string LimpiarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private static object? ValorNulo(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Movimiento = movimiento,
                DetalleMovimiento = detalle,
                Modulo = VModulo
            });
        }
    }
}