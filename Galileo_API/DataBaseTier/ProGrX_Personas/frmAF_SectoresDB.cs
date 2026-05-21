using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAfSectoresDB
    {        
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 1;

        public FrmAfSectoresDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista y el total de sectores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <returns>ErrorDto con la lista y el total de sectores</returns>
        public ErrorDto<SectoresLista> AF_Sectores_Obtener(int codEmpresa)
        {
            string sqlTotal = "SELECT COUNT(*) FROM afi_sectores";
            string sqlLista = "SELECT cod_sector, descripcion FROM afi_sectores ORDER BY cod_sector";

            var total = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlTotal, 0, null);
            var lista = DbHelper.ExecuteListQuery<SectoresData>(_portalDb, codEmpresa, sqlLista, null);

            return new ErrorDto<SectoresLista>
            {
                Code = 0,
                Description = "OK",
                Result = new SectoresLista
                {
                    Total = total.Result,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un sector según si existe o no.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="sector">Datos del sector</param>
        /// <returns>ErrorDto con el resultado de la operación</returns>
        public ErrorDto AF_Sectores_Guardar(int codEmpresa, string usuario, SectoresData sector)
        {
            var queryExiste = "SELECT COUNT(*) FROM afi_sectores WHERE cod_sector = @cod_sector";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryExiste, 0, new { cod_sector = sector.Cod_Sector });
            return existe.Result == 0
                 ? AF_Sectores_Insertar(codEmpresa, usuario, sector)
                 : AF_Sectores_Actualizar(codEmpresa, usuario, sector);
        }

        /// <summary>
        /// Inserta un nuevo sector en la base de datos.
        /// </summary>
        private ErrorDto AF_Sectores_Insertar(int codEmpresa, string usuario, SectoresData sector)
        {
            string query = "INSERT INTO afi_sectores (descripcion) VALUES (@descripcion)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { descripcion = sector.Descripcion });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Sector: {sector.Cod_Sector} - {sector.Descripcion}");
            }
            return result;
        }

        /// <summary>
        /// Actualiza un sector existente en la base de datos.
        /// </summary>
        private ErrorDto AF_Sectores_Actualizar(int codEmpresa, string usuario, SectoresData sector)
        {
            string query = "UPDATE afi_sectores SET descripcion = @descripcion WHERE cod_sector = @cod_sector";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { descripcion = sector.Descripcion, cod_sector = sector.Cod_Sector });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Sector: {sector.Cod_Sector} - {sector.Descripcion}");
            }
            return result;
        }

        /// <summary>
        /// Elimina un sector por su código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="codSector">Código del sector</param>
        /// <returns>ErrorDto con el resultado de la eliminación</returns>
        public ErrorDto AF_Sectores_Eliminar(int codEmpresa, string usuario, int codSector)
        {
            string query = "DELETE FROM afi_sectores WHERE cod_sector = @cod_sector";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { cod_sector = codSector });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Sector: {codSector}");
            }
            return result;
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
