namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    using Galileo.DataBaseTier;
    using Galileo.Models.ERROR;
    using Galileo.Models.Security;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrCatalogoErroresAnalistasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmCrCatalogoErroresAnalistasDB(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoErroresAnalistasDB(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el catálogo de errores para analistas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoErroresAnalistasModel>> CrCatalogoErroresAnalistas_Obtener(int CodEmpresa)
        {
            const string sqlQuery = @"
                SELECT 
                    ID_ERROR as Id_Error,
                    DESCRIPCION as Descripcion,
                    MENSAJE as Mensaje,
                    ACTIVO as Activo
                FROM CRD_ANALISIS_ERRORES
                ORDER BY ID_ERROR";

            return DbHelper.ExecuteListQuery<CrCatalogoErroresAnalistasModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Guarda error de analista (Inserta o Actualiza dependiendo si existe o no).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoErroresAnalistas_Guardar(int CodEmpresa, CrCatalogoErroresAnalistasGuardarRequest request)
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
                detalle: $"Tipo de Garantía : {request.Id_Error}");

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina error de analista por identificador.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoErroresAnalistas_Eliminar(int CodEmpresa, CrCatalogoErroresAnalistasEliminarRequest request)
        {
            const string sqlDelete = @"
                delete CRD_ANALISIS_ERRORES
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
                detalle: $"Tipo de Garantía : {request.Id_Error}");

            return respDelete;
        }

        /// <summary>
        /// Valida si existe un error de analista por ID.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idError"></param>
        /// <returns></returns>
        private bool ExisteError(int CodEmpresa, int idError)
        {
            const string sqlExiste = @"
                select isnull(count(*),0) as Existe
                from CRD_ANALISIS_ERRORES
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
        /// Inserta error de analista.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarError(int CodEmpresa, CrCatalogoErroresAnalistasGuardarRequest request)
        {
            const string sqlInsert = @"
                insert into CRD_ANALISIS_ERRORES
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
                    request.Id_Error,
                    Descripcion = (request.Descripcion ?? string.Empty).Trim(),
                    Mensaje = (request.Mensaje ?? string.Empty).Trim(),
                    Activo = (request.Activo ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Actualiza error de analista.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarError(int CodEmpresa, CrCatalogoErroresAnalistasGuardarRequest request)
        {
            const string sqlUpdate = @"
                update CRD_ANALISIS_ERRORES
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
                    request.Id_Error,
                    Descripcion = (request.Descripcion ?? string.Empty).Trim(),
                    Mensaje = (request.Mensaje ?? string.Empty).Trim(),
                    Activo = (request.Activo ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Registrar en bitacora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
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
