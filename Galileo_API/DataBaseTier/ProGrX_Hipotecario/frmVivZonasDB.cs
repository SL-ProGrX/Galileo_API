using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivZonasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmVivZonasDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmVivZonasDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de zonas de vivienda 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<VivZonaData>> VivZonas_Lista_Obtener(int codEmpresa)
        {
            const string query = @"select IdZona,descripcion,Activa from ViviendaZonas order by IdZona";
            return DbHelper.ExecuteListQuery<VivZonaData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de provincias
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Provincias_Obtener(int codEmpresa)
        {
            const string query = @"select Provincia as item, rtrim(Descripcion) as descripcion from Provincias";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de cantones asignados o no a una zona de vivienda 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idZona"></param>
        /// <param name="provincia"></param>
        /// <param name="soloAsignadas"></param>
        /// <returns></returns>
        public ErrorDto<List<VivZonaCantonData>> Cantones_Obtener(
            int codEmpresa, int idZona, string provincia, bool soloAsignadas)
        {
            string sqlQuery = soloAsignadas
                ? @"
            SELECT 
                C.Canton AS canton,
                RTRIM(C.Descripcion) AS descripcion,
                CASE 
                    WHEN ISNULL(A.idZona, 0) = 0 THEN 0
                    ELSE 1
                END AS [check]
            FROM Cantones C
            INNER JOIN ViviendaZonaAsigna A 
                ON A.idZona = @IdZona
                AND C.provincia = A.provincia
                AND C.canton = A.canton
            WHERE C.provincia = @Provincia
            ORDER BY C.Canton;"
                    : @"
            SELECT 
                C.Canton AS canton,
                RTRIM(C.Descripcion) AS descripcion,
                CASE 
                    WHEN ISNULL(A.idZona, 0) = 0 THEN 0
                    ELSE 1
                END AS [check]
            FROM Cantones C
            LEFT JOIN ViviendaZonaAsigna A 
                ON A.idZona = @IdZona
                AND C.provincia = A.provincia
                AND C.canton = A.canton
            WHERE C.provincia = @Provincia
            ORDER BY C.Canton;";

            return DbHelper.ExecuteListQuery<VivZonaCantonData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new
                {
                    IdZona = idZona,
                    Provincia = provincia?.Trim()
                });
        }

        /// <summary>
        /// Asigna o desasigna un canton a una zona de vivienda
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idZona"></param>
        /// <param name="provincia"></param>
        /// <param name="canton"></param>
        /// <param name="usuario"></param>
        /// <param name="isChecked"></param>
        /// <returns></returns>
        public ErrorDto VivZonas_Asignar(
            int codEmpresa, int idZona, string provincia, string canton, string usuario, bool isChecked)
        {
            string sql = isChecked
                ? @"INSERT INTO ViviendaZonaAsigna (
            idZona, Provincia, Canton, Distrito, RegistroFecha, RegistroUsuario) 
            VALUES (@IdZona, @Provincia, @Canton, '', GETDATE(), @Usuario);"
                : @"DELETE FROM ViviendaZonaAsigna 
            WHERE idZona = @IdZona AND Provincia = @Provincia 
            AND Canton = @Canton AND Distrito = '';";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdZona = idZona,
                    Provincia = provincia?.Trim(),
                    Canton = canton?.Trim(),
                    Usuario = usuario
                }
            );
        }

        /// <summary>
        /// Asigna todos los cantones a una zona de vivienda
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idZona"></param>
        /// <param name="provincia"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto VivZonas_TodosCantones_Asignar(
            int codEmpresa, int idZona, string provincia, string usuario)
        {
            const string sql = @"
            INSERT INTO ViviendaZonaAsigna
            (
                idZona,
                Provincia,
                Canton,
                Distrito,
                RegistroFecha,
                RegistroUsuario
            )
            SELECT 
                @IdZona,
                C.Provincia,
                C.Canton,
                '',
                GETDATE(),
                @Usuario
            FROM Cantones C
            LEFT JOIN ViviendaZonaAsigna A 
                ON A.idZona = @IdZona
                AND C.provincia = A.provincia 
                AND C.canton = A.canton
            WHERE 
                C.provincia = @Provincia
                AND ISNULL(A.idZona, 0) = 0;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdZona = idZona,
                    Provincia = provincia?.Trim(),
                    Usuario = usuario
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Aplica - WEB",
                detalle: $"CrdHip Zonas / Coberturas Todos los Cantones, P.{provincia}, Z.{idZona}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Asignación de todos los cantones a esta zona realizada satisfactoriamente."
            };
        }

        /// <summary>
        /// Guarda la informacion de una zona de vivienda, ya sea creando una nueva o actualizando una existente
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivZonas_Guardar(int codEmpresa, string usuario, VivZonaData request)
        {
            var resp = request.idzona > 0
                ? ActualizarZona(codEmpresa, usuario, request)
                : InsertarZona(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina una zona de vivienda
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idZona"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto VivZonas_Eliminar(int codEmpresa, int idZona, string usuario)
        {
            const string sqlDelete = @"DELETE ViviendaZonas WHERE IdZona = @IdZona;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    IdZona = idZona
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Credito Hipotecario Zona Id: {idZona}"
            );

            return respDelete;
        }

        /// <summary>
        /// Actualiza informacion de zona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarZona(int codEmpresa, string usuario, VivZonaData request)
        {
            const string sqlUpdate = @"
            UPDATE ViviendaZonas
            SET
                descripcion = @Descripcion,
                Activa = @Activa
            WHERE IdZona = @IdZona;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    IdZona = request.idzona,
                    Descripcion = request.descripcion?.Trim(),
                    Activa = request.activa ? 1 : 0
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Credito Hipotecario Zona Id: {request.idzona}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Inserta un nueva zona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarZona(int codEmpresa, string usuario, VivZonaData request)
        {
            const string sqlNextId = @"SELECT ISNULL(MAX(IdZona), 0) + 1 FROM ViviendaZonas;";

            var respId = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlNextId,
                0);

            if (respId.Code < 0)
                return new ErrorDto
                {
                    Code = respId.Code,
                    Description = respId.Description
                };

            var nuevoId = respId.Result;

            const string sqlInsert = @"
            INSERT INTO ViviendaZonas
            (
                IdZona,
                descripcion,
                Activa,
                RegistroFecha,
                RegistroUsuario
            )
            VALUES
            (
                @IdZona,
                @Descripcion,
                @Activa,
                GETDATE(),
                @RegistroUsuario
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    IdZona = nuevoId,
                    Descripcion = request.descripcion?.Trim(),
                    Activa = request.activa ? 1 : 0,
                    RegistroUsuario = usuario
                });

            if (respInsert.Code < 0)
                return respInsert;

            request.idzona = nuevoId;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Credito Hipotecario Zona Id: {nuevoId}"
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
