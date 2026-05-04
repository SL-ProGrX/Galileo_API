using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaAcredoresDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmPreaAcredoresDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmPreaAcredoresDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de acredores autorizados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdPreaAcredoresData>> CrPreaAcredores_ObtenerLista(int codEmpresa)
        {
            const string query = @"
                select
                    cod_acredor,
                    nombre,
                    nombre_giro,
                    isnull(modifica_nombre_giro, 0) as modifica_nombre_giro,
                    activo
                from Crd_Prea_Acredores
                order by cod_acredor;";

            return DbHelper.ExecuteListQuery<CrdPreaAcredoresData>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Guarda un acredor autorizado, insertando o actualizando según exista.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPreaAcredores_Guardar(int codEmpresa, string usuario, CrdPreaAcredoresData request)
        {
            var resp = ExisteAcredor(codEmpresa, request.cod_acredor)
                ? ActualizarAcredor(codEmpresa, usuario, request)
                : InsertarAcredor(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina un acredor autorizado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codAcredor"></param>
        /// <returns></returns>
        public ErrorDto CrPreaAcredores_Borrar(int codEmpresa, string usuario, string codAcredor)
        {
            const string sqlDelete = @"delete Crd_Prea_Acredores where cod_acredor = @CodAcredor;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodAcredor = codAcredor?.Trim()
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"PreAnalisis / Acredor : {codAcredor?.Trim()}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion eliminada satisfactoriamente..."
            };
        }

        #region Helpers
        /// <summary>
        /// Valida si ya existe el acredor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codAcredor"></param>
        /// <returns></returns>
        private bool ExisteAcredor(int codEmpresa, string codAcredor)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0)
                from Crd_Prea_Acredores
                where cod_acredor = @CodAcredor;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodAcredor = codAcredor?.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        /// <summary>
        /// Inserta un acredor autorizado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarAcredor(int codEmpresa, string usuario, CrdPreaAcredoresData request)
        {
            const string sqlInsert = @"
                insert into Crd_Prea_Acredores
                (
                    cod_acredor,
                    nombre,
                    nombre_giro,
                    modifica_nombre_giro,
                    activo
                )
                values
                (
                    @CodAcredor,
                    @Nombre,
                    @NombreGiro,
                    @ModificaNombreGiro,
                    @Activo
                );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodAcredor = request.cod_acredor?.Trim(),
                    Nombre = request.nombre?.Trim(),
                    NombreGiro = request.nombre_giro?.Trim(),
                    ModificaNombreGiro = request.modifica_nombre_giro ? 1 : 0,
                    Activo = request.activo ? 1 : 0
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"PreAnalisis / Acredor : {request.cod_acredor?.Trim()}"
            );

            return respInsert;
        }

        /// <summary>
        /// Actualiza un acredor autorizado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarAcredor(int codEmpresa, string usuario, CrdPreaAcredoresData request)
        {
            const string sqlUpdate = @"
                update Crd_Prea_Acredores
                set
                    nombre = @Nombre,
                    nombre_giro = @NombreGiro,
                    modifica_nombre_giro = @ModificaNombreGiro,
                    activo = @Activo
                where cod_acredor = @CodAcredor;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodAcredor = request.cod_acredor?.Trim(),
                    Nombre = request.nombre?.Trim(),
                    NombreGiro = request.nombre_giro?.Trim(),
                    ModificaNombreGiro = request.modifica_nombre_giro ? 1 : 0,
                    Activo = request.activo ? 1 : 0
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"PreAnalisis / Acredor : {request.cod_acredor?.Trim()}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Registra en bitácora.
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

        #endregion
    }
}
