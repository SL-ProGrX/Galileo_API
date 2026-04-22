using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaTiposExtrasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmPreaTiposExtrasDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmPreaTiposExtrasDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de tipos de extras 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdPreaTiposExtrasData>> CrPreaTiposExtras_Obtener(int codEmpresa)
        {
            const string query = @"select cod_extras,descripcion,prioridad 
                from Crd_Prea_Tipos_extras order by cod_extras";
            return DbHelper.ExecuteListQuery<CrdPreaTiposExtrasData>(
                _portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Guarda un tipo de extra, agregando uno nuevo o actualizando uno existente
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPreaTiposExtras_Guardar(int codEmpresa, string usuario, CrdPreaTiposExtrasData request)
        {
            var resp = ExisteTipoExtra(codEmpresa, request.cod_extras)
                ? ActualizarTipoExtra(codEmpresa, usuario, request)
                : InsertarTipoExtra(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina un tipo de extra
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codExtra"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrPreaTiposExtras_Eliminar(int codEmpresa, string codExtra, string usuario)
        {
            const string sqlDelete = @"delete Crd_Prea_Tipos_extras where cod_extras = @CodExtras;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodExtras = codExtra
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Estudio de Credito - Tipo Extra Id: {codExtra}"
            );

            return respDelete;
        }

        /// <summary>
        /// Valida si existe un tipo de extra 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codExtras"></param>
        /// <returns></returns>
        private bool ExisteTipoExtra(int codEmpresa, string codExtras)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0)
                FROM Crd_Prea_Tipos_extras WHERE cod_extras = @CodExtras;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodExtras = codExtras.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        /// <summary>
        /// Actualiza un tipo de extra
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarTipoExtra(int codEmpresa, string usuario, CrdPreaTiposExtrasData request)
        {
            const string sqlUpdate = @"
            UPDATE Crd_Prea_Tipos_extras
            SET
                descripcion = @Descripcion,
                prioridad = @Prioridad,
                modifica_usuario = @ModificaUsuario,
                modifica_Fecha = GETDATE()
            WHERE cod_extras = @CodExtras;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodExtras = request.cod_extras?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    Prioridad = request.prioridad,
                    ModificaUsuario = usuario
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Estudio de Credito - Tipo Extra Id: {request.cod_extras}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Agrega un tipo de extra
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarTipoExtra(int codEmpresa, string usuario, CrdPreaTiposExtrasData request)
        {
            const string sqlInsert = @"
            INSERT INTO Crd_Prea_Tipos_extras
            (
                cod_extras,
                descripcion,
                prioridad,
                registro_Usuario,
                registro_fecha
            )
            VALUES
            (
                @CodExtras,
                @Descripcion,
                @Prioridad,
                @RegistroUsuario,
                GETDATE()
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodExtras = request.cod_extras?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    Prioridad = request.prioridad,
                    RegistroUsuario = usuario
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Estudio de Credito - Tipo Extra Id: {request.cod_extras}"
            );

            return respInsert;
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
