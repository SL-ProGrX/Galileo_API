namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    using Galileo.DataBaseTier;
    using Galileo.Models.ERROR;
    using Galileo.Models.Security;
    using Galileo_API.Models.ProGrX_ControlTramites;

    public class FrmSifTagsOmisionesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 8;

        public FrmSifTagsOmisionesDB(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmSifTagsOmisionesDB(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el catálogo de omisiones/errores.
        /// </summary>
        public ErrorDto<List<SifTagsOmisionesModel>> SifTagsOmisiones_Obtener(int CodEmpresa)
        {
            const string sqlQuery = @"
                -- Catálogo de omisiones/errores (frmSIF_TagsOmisiones).
                SELECT
                    ID_ERROR as id_Error,
                    DESCRIPCION as descripcion,
                    MENSAJE as mensaje,
                    ACTIVO as activo
                FROM SIF_OMISIONES
                ORDER BY ID_ERROR";

            return DbHelper.ExecuteListQuery<SifTagsOmisionesModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Guarda omisión (Inserta o Actualiza dependiendo si existe o no).
        /// </summary>
        public ErrorDto SifTagsOmisiones_Guardar(int CodEmpresa, SifTagsOmisionesGuardarRequest request)
        {
            var existe = ExisteError(CodEmpresa, request.Id_Error);

            var resp = existe
                ? ActualizarError(CodEmpresa, request)
                : InsertarError(CodEmpresa, request);

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                CodEmpresa,
                request.Usuario,
                movimiento: existe ? "Modifica - WEB" : "Registra - WEB",
                detalle: $"Control de Errores: {request.Id_Error}");

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina omisión por identificador.
        /// </summary>
        public ErrorDto SifTagsOmisiones_Eliminar(int CodEmpresa, SifTagsOmisionesEliminarRequest request)
        {
            const string sqlDelete = @"
                -- Elimina omisión del catálogo (frmSIF_TagsOmisiones / VB5 fxGuardar DELETE).
                delete SIF_OMISIONES
                where ID_ERROR = @Id_Error";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlDelete,
                new { request.Id_Error });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                CodEmpresa,
                request.Usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Control de Errores: {request.Id_Error}");

            return respDelete;
        }

        /// <summary>
        /// Catálogo de módulos para asignación de omisiones.
        /// </summary>
        public ErrorDto<List<SifTagsOmisionesModuloOpcion>> SifTagsOmisiones_Modulos_Obtener(int CodEmpresa)
        {
            const string sqlQuery = @"
                -- Módulos disponibles (sif_modulos_tags / cboModulos).
                select
                    convert(varchar(50), cod_modulo) as item,
                    rtrim(Descripcion) as descripcion
                from sif_modulos_tags
                order by Descripcion";

            return DbHelper.ExecuteListQuery<SifTagsOmisionesModuloOpcion>(
                _portalDb,
                CodEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Omisiones activas y si están asociadas al módulo seleccionado.
        /// </summary>
        public ErrorDto<List<SifTagsOmisionesAsignacionModel>> SifTagsOmisiones_Asignacion_Obtener(
            int CodEmpresa,
            string Cod_Modulo)
        {
            const string sqlQuery = @"
                -- Omisiones activas vs relación SIF_OMISIONES_MODULOS (sbModulos_Load).
                select
                    O.ID_ERROR as id_Error,
                    O.descripcion as descripcion,
                    cast(case when M.ID_ERROR is null then 0 else 1 end as bit) as asignado
                from SIF_OMISIONES O
                left join SIF_OMISIONES_MODULOS M
                    on O.ID_ERROR = M.ID_ERROR
                    and M.cod_Modulo = @Cod_Modulo
                where O.ACTIVO = '1'
                order by M.ID_ERROR desc";

            return DbHelper.ExecuteListQuery<SifTagsOmisionesAsignacionModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new { Cod_Modulo });
        }

        /// <summary>
        /// Asigna o quita una omisión de un módulo. Sin bitácora (VB5 lsw_ItemCheck).
        /// </summary>
        public ErrorDto SifTagsOmisiones_Asignacion_Guardar(
            int CodEmpresa,
            SifTagsOmisionesAsignacionRequest request)
        {
            if (request.Asignado)
            {
                const string sqlInsert = @"
                    -- Relaciona omisión con módulo (lsw_ItemCheck Checked).
                    insert SIF_OMISIONES_MODULOS(ID_ERROR, cod_modulo)
                    values(@Id_Error, @Cod_Modulo)";

                return DbHelper.ExecuteNonQuery(
                    _portalDb,
                    CodEmpresa,
                    sqlInsert,
                    new
                    {
                        Id_Error = request.Id_Error,
                        Cod_Modulo = request.Cod_Modulo
                    });
            }

            const string sqlDelete = @"
                -- Quita relación omisión-módulo (lsw_ItemCheck Unchecked).
                delete SIF_OMISIONES_MODULOS
                where ID_ERROR = @Id_Error
                and cod_modulo = @Cod_Modulo";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlDelete,
                new
                {
                    Id_Error = request.Id_Error,
                    Cod_Modulo = request.Cod_Modulo
                });
        }

        /// <summary>
        /// Valida si existe una omisión por ID.
        /// </summary>
        private bool ExisteError(int CodEmpresa, int idError)
        {
            const string sqlExiste = @"
                -- Verifica existencia de ID_ERROR en SIF_OMISIONES.
                select isnull(count(*),0) as Existe
                from SIF_OMISIONES
                where ID_ERROR = @Id_Error";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                CodEmpresa,
                sqlExiste,
                0,
                new { Id_Error = idError });

            return resp.Result > 0;
        }

        /// <summary>
        /// Inserta omisión. La descripción se guarda tal como llega (VB5 INSERT sin UCase).
        /// </summary>
        private ErrorDto InsertarError(int CodEmpresa, SifTagsOmisionesGuardarRequest request)
        {
            const string sqlInsert = @"
                -- Inserta omisión nueva.
                insert into SIF_OMISIONES
                (
                    ID_ERROR,
                    DESCRIPCION,
                    MENSAJE,
                    ACTIVO
                )
                values
                (
                    @Id_Error,
                    @Descripcion,
                    @Mensaje,
                    @Activo
                )";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlInsert,
                new
                {
                    Id_Error = request.Id_Error,
                    Descripcion = (request.Descripcion ?? string.Empty).Trim(),
                    Mensaje = (request.Mensaje ?? string.Empty).Trim(),
                    Activo = (request.Activo ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Actualiza omisión. La descripción se guarda en mayúsculas (VB5 UCase).
        /// </summary>
        private ErrorDto ActualizarError(int CodEmpresa, SifTagsOmisionesGuardarRequest request)
        {
            const string sqlUpdate = @"
                -- Actualiza omisión existente.
                update SIF_OMISIONES
                set
                    DESCRIPCION = @Descripcion,
                    MENSAJE = @Mensaje,
                    ACTIVO = @Activo
                where ID_ERROR = @Id_Error";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlUpdate,
                new
                {
                    Id_Error = request.Id_Error,
                    Descripcion = (request.Descripcion ?? string.Empty).Trim().ToUpperInvariant(),
                    Mensaje = (request.Mensaje ?? string.Empty).Trim(),
                    Activo = (request.Activo ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Registrar en bitácora.
        /// </summary>
        private void RegistrarBitacora(int CodEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
