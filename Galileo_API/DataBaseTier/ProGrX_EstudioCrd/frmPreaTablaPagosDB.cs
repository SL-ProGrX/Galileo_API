using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaTablaPagosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmPreaTablaPagosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmPreaTablaPagosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de instituciones para la tabla de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPreaTablaPagos_ObtenerInstituciones(int codEmpresa)
        {
            const string query = @"
                SELECT
                    cod_institucion AS item,
                    descripcion
                FROM instituciones
                ORDER BY cod_institucion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene la tabla de pagos por institución.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdPreaTablaPagosData>> CrPreaTablaPagos_Obtener(int codEmpresa, int codInstitucion)
        {
            const string query = @"
                SELECT
                    idx,
                    fecha,
                    usuario,
                    inicio,
                    corte,
                    npagos,
                    cod_institucion
                FROM crd_prea_tabla_pagos
                WHERE cod_institucion = @CodInstitucion
                ORDER BY inicio DESC;";

            return DbHelper.ExecuteListQuery<CrdPreaTablaPagosData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    CodInstitucion = codInstitucion
                });
        }

        /// <summary>
        /// Guarda un registro de la tabla de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPreaTablaPagos_Guardar(int codEmpresa, string usuario, CrdPreaTablaPagosData request)
        {
            var resp = request.idx > 0
                ? ActualizarTablaPago(codEmpresa, usuario, request)
                : InsertarTablaPago(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina un registro de la tabla de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idx"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrPreaTablaPagos_Eliminar(int codEmpresa, int idx, string usuario)
        {
            const string sqlDelete = @"
                DELETE crd_prea_tabla_pagos
                WHERE idx = @Idx;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    Idx = idx
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Estudio Credito Tabla de Pago [ID]: {idx}"
            );

            return respDelete;
        }

        /// <summary>
        /// Inserta un registro nuevo de tabla de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarTablaPago(int codEmpresa, string usuario, CrdPreaTablaPagosData request)
        {
            const string sqlInsert = @"
                INSERT INTO Crd_Prea_Tabla_pagos
                (
                    cod_institucion,
                    fecha,
                    usuario,
                    inicio,
                    corte,
                    npagos
                )
                VALUES
                (
                    @CodInstitucion,
                    GETDATE(),
                    @Usuario,
                    @Inicio,
                    @Corte,
                    @NPagos
                );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodInstitucion = request.cod_institucion,
                    Usuario = usuario,
                    Inicio = request.inicio,
                    Corte = request.corte,
                    NPagos = request.npagos
                });

            if (respInsert.Code < 0)
                return respInsert;

            var ultimoIdx = ObtenerUltimoIdx(codEmpresa);

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Estudio Credito Tabla de Pago [ID]: {ultimoIdx}"
            );

            return respInsert;
        }

        /// <summary>
        /// Actualiza un registro existente de tabla de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarTablaPago(int codEmpresa, string usuario, CrdPreaTablaPagosData request)
        {
            const string sqlUpdate = @"
                UPDATE Crd_Prea_Tabla_pagos
                SET
                    inicio = @Inicio,
                    corte = @Corte,
                    npagos = @NPagos,
                    usuario = @ModificaUsuario,
                    fecha = GETDATE()
                WHERE idx = @Idx;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    Idx = request.idx,
                    Inicio = request.inicio,
                    Corte = request.corte,
                    NPagos = request.npagos,
                    ModificaUsuario = usuario
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Estudio Credito Tabla de Pago [ID]: {request.idx}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Obtiene el último identificador generado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private int ObtenerUltimoIdx(int codEmpresa)
        {
            const string sql = @"
                SELECT ISNULL(MAX(idx), 0)
                FROM Crd_Prea_Tabla_pagos;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0);

            if (resp.Code < 0)
                return 0;

            return resp.Result;
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
    }
}
